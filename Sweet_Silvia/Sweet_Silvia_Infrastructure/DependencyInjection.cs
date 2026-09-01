using Amazon;
using Amazon.S3;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sweet_Silvia_Application.Abstractions.Audit;
using Sweet_Silvia_Application.Abstractions.Email;
using Sweet_Silvia_Application.Abstractions.Security;
using Sweet_Silvia_Application.Abstractions.Shipping;
using Sweet_Silvia_Application.Abstractions.Storage;
using Sweet_Silvia_Application.Abstractions.Time;
using Sweet_Silvia_Domain.Abstractions;
using Sweet_Silvia_Domain.RefreshToken;
using Sweet_Silvia_Infrastructure.Abstractions.Audit;
using Sweet_Silvia_Infrastructure.Abstractions.Email;
using Sweet_Silvia_Infrastructure.Abstractions.Security;
using Sweet_Silvia_Infrastructure.Abstractions.Shipping;
using Sweet_Silvia_Infrastructure.Abstractions.Storage;
using Sweet_Silvia_Infrastructure.Abstractions.Time;
using Sweet_Silvia_Infrastructure.Repositories;

namespace Sweet_Silvia_Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SweetSilviaDatabase")
            ?? throw new InvalidOperationException("Connection string 'SweetSilviaDatabase' not found.");
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration section is missing or invalid.");
        var refreshTokenOptions = configuration.GetSection("RefreshToken").Get<RefreshTokenOptions>() ?? new RefreshTokenOptions();
        var emailVerificationOptions = configuration.GetSection("EmailVerification").Get<EmailVerificationOptions>() ?? new EmailVerificationOptions();
        var emailDeliveryOptions = configuration.GetSection("Email").Get<EmailDeliveryOptions>() ?? new EmailDeliveryOptions();
        var olvaOptions = configuration.GetSection("Olva").Get<OlvaOptions>() ?? new OlvaOptions();
        var fileStorageOptions = configuration.GetSection("FileStorage").Get<FileStorageOptions>() ?? new FileStorageOptions();

        ValidateJwt(jwtOptions);
        ValidateRefreshToken(refreshTokenOptions);
        ValidateEmail(emailVerificationOptions, emailDeliveryOptions);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.EnableRetryOnFailure()));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton(jwtOptions);
        services.AddSingleton(refreshTokenOptions);
        services.AddSingleton(emailVerificationOptions);
        services.AddSingleton(emailDeliveryOptions);
        services.AddSingleton(olvaOptions);
        services.AddSingleton(fileStorageOptions);
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IAuditLogWriter, AuditLogWriter>();
        services.AddSingleton<IVerificationCodeGenerator, VerificationCodeGenerator>();

        RegisterEmailSender(services, emailDeliveryOptions);
        RegisterOlvaCourierClient(services, olvaOptions);
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
            });
    }

    private static void ValidateJwt(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer) ||
            string.IsNullOrWhiteSpace(options.Audience) ||
            string.IsNullOrWhiteSpace(options.SecretKey) ||
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

    private static void ValidateEmail(EmailVerificationOptions verificationOptions, EmailDeliveryOptions deliveryOptions)
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

        if (useBrevo &&
            (!deliveryOptions.ApiKey.StartsWith("xkeysib-", StringComparison.Ordinal) ||
             !MailAddress.TryCreate(deliveryOptions.From, out _)))
        {
            throw new InvalidOperationException("Brevo email configuration is invalid. The API key must start with 'xkeysib-' and From must contain a valid verified sender.");
        }
    }
}
