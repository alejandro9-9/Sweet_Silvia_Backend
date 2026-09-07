using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.RateLimiting;
using Sweet_Silvia_Api.Extensions;
using Sweet_Silvia_Api.Filters;
using Sweet_Silvia_Application;
using Sweet_Silvia_Infrastructure;

var builder = WebApplication.CreateBuilder(args);

if (int.TryParse(builder.Configuration["PORT"], out var port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options => options.AddPolicy("defaultCorsPolicy", policy =>
{
    if (allowedOrigins.Length > 0)
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        return;
    }

    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException("Cors:AllowedOrigins must be configured outside Development.");
    }

    policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
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
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.ApplyMigrationsAsync();
app.UseRateLimiter();
app.UseCors("defaultCorsPolicy");
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.Run();

public partial class Program;
