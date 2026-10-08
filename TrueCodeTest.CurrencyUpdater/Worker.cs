using Microsoft.Extensions.Options;
using TrueCodeTest.CurrencyUpdater.Cbr;
using TrueCodeTest.CurrencyUpdater.Options;
using TrueCodeTest.CurrencyUpdater.Services;

namespace TrueCodeTest.CurrencyUpdater;

/// <summary>
/// Фоновый сервис: периодически загружает курсы валют с сайта ЦБ РФ и сохраняет их в БД.
/// </summary>
public sealed class Worker(
    ICbrClient cbrClient,
    IServiceScopeFactory scopeFactory,
    IOptions<CurrencyUpdaterOptions> options,
    ILogger<Worker> logger) : BackgroundService
{
    private readonly CurrencyUpdaterOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Currency updater started. Interval: {Interval}, run on startup: {RunOnStartup}.",
            _options.Interval,
            _options.RunOnStartup);

        if (_options.RunOnStartup)
        {
            await RunCycleAsync(stoppingToken);
        }

        // PeriodicTimer, в отличие от цикла с Task.Delay, не накапливает
        // смещение на длительность предыдущей итерации.
        using var timer = new PeriodicTimer(_options.Interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunCycleAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }

        logger.LogInformation("Currency updater stopped.");
    }

    private async Task RunCycleAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= _options.MaxAttempts; attempt++)
        {
            try
            {
                var rates = await cbrClient.GetRatesAsync(cancellationToken);

                // Импортёр и его DbContext — scoped, а воркер — singleton,
                // поэтому на каждый цикл создаётся отдельный scope.
                using (var scope = scopeFactory.CreateScope())
                {
                    var importer = scope.ServiceProvider.GetRequiredService<ICurrencyImporter>();
                    await importer.ImportAsync(rates, cancellationToken);
                }

                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                if (attempt == _options.MaxAttempts)
                {
                    // Ошибка намеренно не пробрасывается: неудачный цикл не
                    // должен останавливать сервис, иначе следующий тик не наступит.
                    logger.LogError(
                        exception,
                        "Currency update failed after {Attempts} attempts.",
                        _options.MaxAttempts);
                    return;
                }

                var delay = TimeSpan.FromMilliseconds(
                    _options.RetryBaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));

                logger.LogWarning(
                    exception,
                    "Currency update attempt {Attempt} of {Attempts} failed. Retrying in {Delay}.",
                    attempt,
                    _options.MaxAttempts,
                    delay);

                try
                {
                    await Task.Delay(delay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }
}
