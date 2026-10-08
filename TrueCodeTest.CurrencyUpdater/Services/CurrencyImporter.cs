using Microsoft.EntityFrameworkCore;
using TrueCodeTest.CurrencyUpdater.Cbr;
using TrueCodeTest.Shared.Data;

namespace TrueCodeTest.CurrencyUpdater.Services;

public interface ICurrencyImporter
{
    /// <summary>
    /// Добавляет новые валюты и обновляет курс у уже сохранённых.
    /// </summary>
    Task<ImportResult> ImportAsync(IReadOnlyList<CbrRate> rates, CancellationToken cancellationToken);
}

public sealed class CurrencyImporter(AppDbContext dbContext, ILogger<CurrencyImporter> logger)
    : ICurrencyImporter
{
    public async Task<ImportResult> ImportAsync(
        IReadOnlyList<CbrRate> rates,
        CancellationToken cancellationToken)
    {
        if (rates.Count == 0)
        {
            logger.LogWarning("Лента ЦБ не вернула ни одного курса, импортировать нечего.");
            return new ImportResult(0, 0, 0);
        }

        // Один запрос загружает текущее состояние: получается одно чтение и
        // одно сохранение вместо запроса на каждую валюту.
        var existing = await dbContext.Currencies
            .ToDictionaryAsync(x => x.Name, cancellationToken);

        var inserted = 0;
        var updated = 0;
        var unchanged = 0;

        foreach (var rate in rates)
        {
            if (existing.TryGetValue(rate.Name, out var currency))
            {
                if (currency.Rate == rate.Rate)
                {
                    unchanged++;
                    continue;
                }

                currency.Rate = rate.Rate;
                updated++;
            }
            else
            {
                var created = new Shared.Entities.Currency { Name = rate.Name, Rate = rate.Rate };
                dbContext.Currencies.Add(created);

                // Добавляем в словарь, чтобы повтор названия внутри одной ленты
                // считался обновлением, а не второй вставкой.
                existing[rate.Name] = created;
                inserted++;
            }
        }

        if (inserted > 0 || updated > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation(
            "Импорт курсов завершён: добавлено {Inserted}, обновлено {Updated}, без изменений {Unchanged}.",
            inserted,
            updated,
            unchanged);

        return new ImportResult(inserted, updated, unchanged);
    }
}

/// <summary>Итог одного прогона импорта.</summary>
public sealed record ImportResult(int Inserted, int Updated, int Unchanged);
