using System.Diagnostics;
using CoffeeTunes.WebApi.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CoffeeTunes.WebApi.Services;

public class DatabaseMigrationBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<DatabaseMigrationBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CoffeeTunesDbContext>();

        logger.LogInformation("Starting database migration");
        var sw = Stopwatch.StartNew();

        await dbContext.Database.MigrateAsync(stoppingToken);

        sw.Stop();
        logger.LogInformation("Finished database migration in {Elapsed}", sw.Elapsed);
    }
}