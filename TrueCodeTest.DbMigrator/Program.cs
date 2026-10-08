using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrueCodeTest.DbMigrator;
using TrueCodeTest.Shared.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<MigratorOptions>(
    builder.Configuration.GetSection(MigratorOptions.SectionName));

var migrationsAssembly = typeof(Program).Assembly.GetName().Name!;
builder.Services.AddAppDbContext(builder.Configuration, migrationsAssembly);

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DbMigrator");
var options = host.Services.GetRequiredService<IOptions<MigratorOptions>>().Value;

logger.LogInformation("Применение миграций из сборки {Assembly}.", migrationsAssembly);

var applied = false;

for (var attempt = 1; attempt <= options.MaxAttempts && !applied; attempt++)
{
    try
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pending = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

        if (pending.Count == 0)
        {
            logger.LogInformation("Неприменённых миграций нет, схема актуальна.");
        }
        else
        {
            logger.LogInformation(
                "К применению: {Count} миграций ({Migrations}).",
                pending.Count,
                string.Join(", ", pending));

            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Миграции применены.");
        }

        applied = true;
    }
    catch (Exception exception) when (attempt < options.MaxAttempts)
    {
        logger.LogWarning(
            exception,
            "Попытка {Attempt} из {Attempts} не удалась. Повтор через {Delay}.",
            attempt,
            options.MaxAttempts,
            options.RetryDelay);

        await Task.Delay(options.RetryDelay);
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Не удалось применить миграции, попыток больше не осталось.");
        return 1;
    }
}

return 0;
