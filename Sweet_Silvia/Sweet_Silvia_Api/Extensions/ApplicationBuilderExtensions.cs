using Microsoft.EntityFrameworkCore;
using Sweet_Silvia_Infrastructure;

namespace Sweet_Silvia_Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task ApplyMigrationsAsync(this IApplicationBuilder application)
    {
        const int maximumAttempts = 10;

        for (var attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            using var scope = application.ApplicationServices.CreateScope();

            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();
                return;
            }
            catch (Exception exception) when (attempt < maximumAttempts)
            {
                var logger = scope.ServiceProvider
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Database migration");
                logger.LogWarning(
                    exception,
                    "Database unavailable. Migration attempt {Attempt} of {MaximumAttempts} failed.",
                    attempt,
                    maximumAttempts);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        throw new InvalidOperationException("Database migrations failed after the maximum number of attempts.");
    }
}
