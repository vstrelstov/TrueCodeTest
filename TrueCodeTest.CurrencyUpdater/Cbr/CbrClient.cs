namespace TrueCodeTest.CurrencyUpdater.Cbr;

/// <summary>
/// Клиент для получения курсов валют с сайта ЦБ РФ.
/// </summary>
public interface ICbrClient
{
    Task<IReadOnlyList<CbrRate>> GetRatesAsync(CancellationToken cancellationToken);
}

/// <inheritdoc />
public sealed class CbrClient(HttpClient httpClient, ILogger<CbrClient> logger) : ICbrClient
{
    public async Task<IReadOnlyList<CbrRate>> GetRatesAsync(CancellationToken cancellationToken)
    {
        // Читаем байты, а не строку: лента отдаётся в CP-1251, и декодирование
        // силами HttpClient как UTF-8 испортило бы все названия валют.
        var bytes = await httpClient.GetByteArrayAsync(string.Empty, cancellationToken);

        var rates = CbrRateParser.Parse(bytes);

        logger.LogDebug("Parsed {Count} currency rates from the CBR feed.", rates.Count);

        return rates;
    }
}
