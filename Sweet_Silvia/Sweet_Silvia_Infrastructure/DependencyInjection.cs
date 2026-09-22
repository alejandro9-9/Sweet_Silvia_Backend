using Amazon;
using Amazon.S3;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Sweet_Silvia_Application.Abstractions.Audit;
using Sweet_Silvia_Application.Abstractions.Email;
using Sweet_Silvia_Application.Abstractions.Payments;
using Sweet_Silvia_Application.Abstractions.Security;
using Sweet_Silvia_Application.Abstractions.Shipping;
using Sweet_Silvia_Application.Abstractions.Storage;
using Sweet_Silvia_Application.Abstractions.Time;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.RefreshToken;
using Sweet_Silvia_Domain.User;
using Sweet_Silvia_Infrastructure.Abstractions.Audit;
using Sweet_Silvia_Infrastructure.Abstractions.Email;
using Sweet_Silvia_Infrastructure.Abstractions.Orders;
using Sweet_Silvia_Infrastructure.Abstractions.Payments;
using Sweet_Silvia_Infrastructure.Abstractions.Security;
using Sweet_Silvia_Infrastructure.Abstractions.Shipping;
using Sweet_Silvia_Infrastructure.Abstractions.Storage;
using Sweet_Silvia_Infrastructure.Abstractions.Time;
using Sweet_Silvia_Infrastructure.Repositories;

namespace Sweet_Silvia_Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("SweetSilviaDatabase")
            ?? throw new InvalidOperationException("Connection string 'SweetSilviaDatabase' not found.");
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration section is missing or invalid.");
        var refreshTokenOptions = configuration.GetSection("RefreshToken").Get<RefreshTokenOptions>() ?? new RefreshTokenOptions();
        var emailVerificationOptions = configuration.GetSection("EmailVerification").Get<EmailVerificationOptions>() ?? new EmailVerificationOptions();
        var emailDeliveryOptions = configuration.GetSection("Email").Get<EmailDeliveryOptions>() ?? new EmailDeliveryOptions();
        var googleAuthOptions = configuration.GetSection("Google").Get<GoogleAuthOptions>() ?? new GoogleAuthOptions();
        var olvaOptions = configuration.GetSection("Olva").Get<OlvaOptions>() ?? new OlvaOptions();
        var izipayOptions = configuration.GetSection("Izipay").Get<IzipayOptions>() ?? new IzipayOptions();
        var fileStorageOptions = configuration.GetSection("FileStorage").Get<FileStorageOptions>() ?? new FileStorageOptions();
        var orderExpirationOptions = configuration.GetSection("Orders").Get<OrderExpirationOptions>() ?? new OrderExpirationOptions();

        ValidateJwt(jwtOptions);
        ValidateRefreshToken(refreshTokenOptions);
        ValidateEmail(emailVerificationOptions, emailDeliveryOptions, environment);
        ValidateGoogleAuth(googleAuthOptions);
        ValidateIzipay(izipayOptions);
        ValidateFileStorage(fileStorageOptions);
        ValidateOrderExpiration(orderExpirationOptions);

        static void ConfigureDbContext(DbContextOptionsBuilder options, string databaseConnectionString)
        {
            options.UseNpgsql(databaseConnectionString, npgsqlOptions =>
                npgsqlOptions.EnableRetryOnFailure());
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            ConfigureDbContext(options, connectionString));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddHostedService<RefreshTokenCleanupService>();
        services.AddSingleton(orderExpirationOptions);
        services.AddHostedService<OrderExpirationService>();

        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton(jwtOptions);
        services.AddSingleton(refreshTokenOptions);
        services.AddSingleton(emailVerificationOptions);
        services.AddSingleton(emailDeliveryOptions);
        services.AddSingleton(googleAuthOptions);
        services.AddSingleton(olvaOptions);
        services.AddSingleton(izipayOptions);
        services.AddSingleton(fileStorageOptions);
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IAuditLogWriter, AuditLogWriter>();
        services.AddSingleton<IVerificationCodeGenerator, VerificationCodeGenerator>();
        services.AddHttpClient<IGoogleIdTokenValidator, GoogleIdTokenValidator>(client =>
        {
            client.BaseAddress = new Uri("https://oauth2.googleapis.com/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        RegisterEmailSender(services, emailDeliveryOptions);
        RegisterOlvaCourierClient(services, olvaOptions);
        RegisterIzipayPaymentGateway(services, izipayOptions);
        RegisterFileStorage(services, fileStorageOptions);
        RegisterAuthentication(services, jwtOptions);

        return services;
    }

    private static void RegisterFileStorage(IServiceCollection services, FileStorageOptions options)
    {
        if (string.Equals(options.Provider, "S3", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(RegionEndpoint.GetBySystemName(options.S3Region)));
            services.AddScoped<IFileStorageService, S3FileStorageService>();
            return;
        }

        services.AddScoped<IFileStorageService, LocalFileStorageService>();
    }

    private static void RegisterOlvaCourierClient(IServiceCollection services, OlvaOptions options)
    {
        services.AddHttpClient<IOlvaCourierClient, OlvaCourierClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
        });
    }

    private static void RegisterIzipayPaymentGateway(IServiceCollection services, IzipayOptions options)
    {
        services.AddSingleton<IIzipaySignatureValidator, IzipaySignatureValidator>();
        services.AddHttpClient<IIzipayPaymentGateway, IzipayPaymentGateway>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(20);
        });
    }

    private static void RegisterEmailSender(IServiceCollection services, EmailDeliveryOptions options)
    {
        if (string.Equals(options.Provider, "Brevo", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<IEmailSender, BrevoEmailSender>(client =>
            {
                client.BaseAddress = new Uri("https://api.brevo.com/v3/");
                client.Timeout = TimeSpan.FromSeconds(15);
            });
            return;
        }

        services.AddScoped<IEmailSender, LoggingEmailSender>();
    }

    private static void RegisterAuthentication(IServiceCollection services, JwtOptions options)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = options.Issuer,
                    ValidAudience = options.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey))
                };
                jwtBearerOptions.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var roleIdValue = context.Principal?.FindFirstValue("roleId");
                        if (!Guid.TryParse(userIdValue, out var userId) || !Guid.TryParse(roleIdValue, out var roleId))
                        {
                            context.Fail("The authenticated user claims are invalid.");
                            return;
                        }

                        var database = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                        var user = await database.Users
                            .AsNoTracking()
                            .SingleOrDefaultAsync(item => item.Id == userId, context.HttpContext.RequestAborted);
                        var role = user is null
                            ? null
                            : await database.Roles
                                .AsNoTracking()
                                .SingleOrDefaultAsync(item => item.Id == roleId, context.HttpContext.RequestAborted);

                        if (user is null || user.Status != UserStatus.Active || user.RoleId != roleId || role is null || !role.IsActive)
                        {
                            context.Fail("The user is no longer active or the token role is stale.");
                        }
                    }
                };
            });
    }

    private static void ValidateGoogleAuth(GoogleAuthOptions options)
    {
        if (options.Enabled && string.IsNullOrWhiteSpace(options.ClientId))
        {
            throw new InvalidOperationException("Google:ClientId must be configured when Google authentication is enabled.");
        }

        if (string.IsNullOrWhiteSpace(options.ClientSecret) != string.IsNullOrWhiteSpace(options.RedirectUri))
        {
            throw new InvalidOperationException("Google:ClientSecret and Google:RedirectUri must be configured together for authorization-code flow.");
        }

        if (!string.IsNullOrWhiteSpace(options.RedirectUri) &&
            (!Uri.TryCreate(options.RedirectUri, UriKind.Absolute, out var redirectUri) ||
             (!string.Equals(redirectUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
              !string.Equals(redirectUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))))
        {
            throw new InvalidOperationException("Google:RedirectUri must be an absolute HTTP or HTTPS URL.");
        }
    }

    private static void ValidateJwt(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer) ||
            string.IsNullOrWhiteSpace(options.Audience) ||
            string.IsNullOrWhiteSpace(options.SecretKey) ||
            options.SecretKey.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) ||
            Encoding.UTF8.GetByteCount(options.SecretKey) < 32 ||
            options.ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("JWT configuration is invalid. The secret key must contain at least 32 bytes.");
        }
    }

    private static void ValidateRefreshToken(RefreshTokenOptions options)
    {
        if (options.ExpirationDays is < 1 or > 365)
        {
            throw new InvalidOperationException("RefreshToken:ExpirationDays must be between 1 and 365.");
        }
    }

    private static void ValidateEmail(EmailVerificationOptions verificationOptions, EmailDeliveryOptions deliveryOptions, IHostEnvironment environment)
    {
        if (verificationOptions.CodeExpirationMinutes <= 0 || verificationOptions.ResendCooldownSeconds < 0)
        {
            throw new InvalidOperationException("Email verification configuration is invalid.");
        }

        var useBrevo = string.Equals(deliveryOptions.Provider, "Brevo", StringComparison.OrdinalIgnoreCase);
        var useLog = string.Equals(deliveryOptions.Provider, "Log", StringComparison.OrdinalIgnoreCase);

        if (!useBrevo && !useLog)
        {
            throw new InvalidOperationException("Email provider must be 'Brevo' or 'Log'.");
        }

        if (useLog && !environment.IsDevelopment())
        {
            throw new InvalidOperationException("Email:Provider=Log is only allowed in Development. Configure Brevo before deploying.");
        }

        if (useBrevo &&
            (!deliveryOptions.ApiKey.StartsWith("xkeysib-", StringComparison.Ordinal) ||
             !MailAddress.TryCreate(deliveryOptions.From, out _)))
        {
            throw new InvalidOperationException("Brevo email configuration is invalid. The API key must start with 'xkeysib-' and From must contain a valid verified sender.");
        }
    }

    private static void ValidateIzipay(IzipayOptions options)
    {
        if (!options.Enabled)
        {
            return;
        }

        var apiKey = string.IsNullOrWhiteSpace(options.ApiKey) ? options.PublicKey : options.ApiKey;
        if (!HasProductionValue(options.MerchantCode) ||
            !HasProductionValue(apiKey) ||
            !HasProductionValue(options.HashKey) ||
            !IsHttpsUrl(options.BaseUrl) ||
            !IsPublicHttpsUrl(options.NotificationUrl) ||
            !IsPublicHttpsUrl(options.TermsAndConditionsUrl))
        {
            throw new InvalidOperationException(
                "Izipay configuration is invalid. Configure the merchant code, payment API key, hash key and public HTTPS URLs before enabling it.");
        }

        if (!string.Equals(options.Environment, "Sandbox", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(options.Environment, "Production", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Izipay:Environment must be 'Sandbox' or 'Production'.");
        }
    }

    private static void ValidateFileStorage(FileStorageOptions options)
    {
        if (!string.Equals(options.Provider, "S3", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var publicBucket = string.IsNullOrWhiteSpace(options.S3PublicBucketName)
            ? options.S3BucketName
            : options.S3PublicBucketName;
        var privateBucket = string.IsNullOrWhiteSpace(options.S3PrivateBucketName)
            ? options.S3BucketName
            : options.S3PrivateBucketName;

        if (!HasProductionValue(publicBucket) ||
            !HasProductionValue(privateBucket) ||
            string.Equals(publicBucket, privateBucket, StringComparison.OrdinalIgnoreCase) ||
            !HasProductionValue(options.S3Region) ||
            !HasProductionValue(options.S3PublicBaseUrl) ||
            !IsHttpsUrl(options.S3PublicBaseUrl))
        {
            throw new InvalidOperationException(
                "S3 storage requires distinct public and private buckets, a region and a public HTTPS base URL.");
        }
    }

    private static void ValidateOrderExpiration(OrderExpirationOptions options)
    {
        if (options.PendingPaymentExpirationHours is < 1 or > 168)
        {
            throw new InvalidOperationException("Orders:PendingPaymentExpirationHours must be between 1 and 168.");
        }
    }

    private static bool HasProductionValue(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           !value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) &&
           !value.Contains("your-", StringComparison.OrdinalIgnoreCase);

    private static bool IsHttpsUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;

    private static bool IsPublicHttpsUrl(string? value)
        => IsHttpsUrl(value) &&
           Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
           !uri.IsLoopback &&
           !uri.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase) &&
           !uri.Host.Contains("your-", StringComparison.OrdinalIgnoreCase);
}
