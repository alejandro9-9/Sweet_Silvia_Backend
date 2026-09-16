using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using System.Net;
using System.Data.Common;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.RateLimiting;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Filters;
using Sweet_Silvia_Application;
using Sweet_Silvia_Infrastructure;
using Sweet_Silvia_Infrastructure.Abstractions.Storage;

var builder = WebApplication.CreateBuilder(args);

if (int.TryParse(builder.Configuration["PORT"], out var port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var allowedOrigins = (builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .Select(origin => origin.Trim().TrimEnd('/'))
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (!builder.Environment.IsDevelopment())
{
    ValidateProductionOrigins(allowedOrigins);
    ValidateProductionDatabase(builder.Configuration.GetConnectionString("SweetSilviaDatabase"));
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    var configuredProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];
    foreach (var proxy in configuredProxies)
    {
        if (IPAddress.TryParse(proxy, out var address))
        {
            options.KnownProxies.Add(address);
        }
    }
});

builder.Services.AddCors(options => options.AddPolicy("defaultCorsPolicy", policy =>
{
    if (builder.Environment.IsDevelopment())
    {
        policy.SetIsOriginAllowed(origin =>
            allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase) ||
            Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.IsLoopback)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        return;
    }

    if (allowedOrigins.Length > 0)
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        return;
    }

    throw new InvalidOperationException("Cors:AllowedOrigins must be configured outside Development.");
}));

builder.Services.AddScoped<GlobalActionLoggingFilter>();
builder.Services.AddScoped<GlobalExceptionsFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionsFilter>();
    options.Filters.Add<GlobalActionLoggingFilter>();
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.AddPolicy("api", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    }

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var applyMigrationsOnStartup = builder.Configuration.GetValue<bool?>("Database:ApplyMigrationsOnStartup")
    ?? builder.Environment.IsDevelopment();
if (applyMigrationsOnStartup)
{
    await app.ApplyMigrationsAsync();
}
app.UseForwardedHeaders();
app.UseRateLimiter();
app.UseCors("defaultCorsPolicy");
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
var fileStorageOptions = app.Services.GetRequiredService<FileStorageOptions>();
if (string.Equals(fileStorageOptions.Provider, "Local", StringComparison.OrdinalIgnoreCase))
{
    var publicUploadRoot = Path.GetFullPath(fileStorageOptions.LocalRootPath);
    Directory.CreateDirectory(publicUploadRoot);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(publicUploadRoot),
        RequestPath = "/uploads"
    });
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("api");
app.MapGet("/health", async (ApplicationDbContext database, CancellationToken cancellationToken) =>
{
    try
    {
        return await database.Database.CanConnectAsync(cancellationToken)
            ? Results.Ok(new { status = "healthy" })
            : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});
app.Run();

static void ValidateProductionOrigins(IEnumerable<string> origins)
{
    var configuredOrigins = origins.ToArray();
    if (configuredOrigins.Length == 0)
    {
        throw new InvalidOperationException("Cors:AllowedOrigins must be configured outside Development.");
    }

    foreach (var origin in configuredOrigins)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            uri.IsLoopback ||
            uri.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            origin.Contains("your-", StringComparison.OrdinalIgnoreCase) ||
            origin.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Cors origin '{origin}' is invalid. Production origins must be public HTTPS origins without paths.");
        }
    }
}

static void ValidateProductionDatabase(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("The production database connection string is missing.");
    }

    var connection = new DbConnectionStringBuilder { ConnectionString = connectionString };
    var host = GetConnectionValue(connection, "Host");
    var sslMode = GetConnectionValue(connection, "SSL Mode");
    var rootCertificate = GetConnectionValue(connection, "Root Certificate");

    if (string.IsNullOrWhiteSpace(host) ||
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
        host.Contains("YOUR_", StringComparison.OrdinalIgnoreCase) ||
        !string.Equals(sslMode, "VerifyFull", StringComparison.OrdinalIgnoreCase) ||
        string.IsNullOrWhiteSpace(rootCertificate) ||
        rootCertificate.Contains("YOUR_", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "The production database must use a non-local host, SSL Mode=VerifyFull and a root certificate.");
    }
}

static string? GetConnectionValue(DbConnectionStringBuilder connection, string key)
    => connection.TryGetValue(key, out var value) ? Convert.ToString(value) : null;

public partial class Program;
