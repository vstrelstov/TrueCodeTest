using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace TrueCodeTest.CurrencyUpdater.Cbr;

/// <summary>
/// Разбирает XML-документ ЦБ РФ (windows-1251) в набор <see cref="CbrRate"/>.
/// </summary>
/// <remarks>
/// Для каждой валюты в ленте указаны <c>Nominal</c> и <c>Value</c>, например
/// 100 JPY = 54,2753 RUB. В БД нужен курс за одну единицу, поэтому приоритет
/// отдаётся элементу <c>VunitRate</c>, а при его отсутствии <c>Value</c>
/// делится на <c>Nominal</c>.
/// </remarks>
public static class CbrRateParser
{
    // Лента объявлена в windows-1251 — это legacy-кодировка, её нужно
    // зарегистрировать заранее, на части платформ она недоступна по умолчанию.
    public static readonly Encoding FeedEncoding = RegisterWindows1251();

    private static Encoding RegisterWindows1251()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(1251);
    }

    /// <summary>
    /// Разбирает тело ответа ленты ЦБ РФ в исходной кодировке.
    /// </summary>
    /// <param name="xmlBytes">Байты ответа, ещё в windows-1251.</param>
    /// <exception cref="InvalidDataException">В документе нет ни одного элемента <c>Valute</c>.</exception>
    public static IReadOnlyList<CbrRate> Parse(ReadOnlySpan<byte> xmlBytes)
    {
        var xml = FeedEncoding.GetString(xmlBytes);
        return ParseXml(xml);
    }

    /// <summary>
    /// Разбирает ленту ЦБ РФ из уже декодированной строки.
    /// </summary>
    public static IReadOnlyList<CbrRate> ParseXml(string xml)
    {
        XDocument document;
        try
        {
            document = XDocument.Parse(xml);
        }
        catch (System.Xml.XmlException exception)
        {
            throw new InvalidDataException("The CBR feed is not valid XML.", exception);
        }

        var rates = new List<CbrRate>();

        foreach (var valute in document.Descendants("Valute"))
        {
            var name = ((string?)valute.Element("Name"))?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            var rate = ReadUnitRate(valute);
            if (rate is null)
            {
                continue;
            }

            rates.Add(new CbrRate(name, rate.Value));
        }

        if (rates.Count == 0)
        {
            throw new InvalidDataException("The CBR feed did not contain any currency rates.");
        }

        return rates;
    }

    private static decimal? ReadUnitRate(XElement valute)
    {
        var unitRate = ParseDecimal((string?)valute.Element("VunitRate"));
        if (unitRate is > 0)
        {
            return unitRate;
        }

        // Запасной вариант: Value относится не к одной единице, а к Nominal.
        var value = ParseDecimal((string?)valute.Element("Value"));
        var nominal = ParseDecimal((string?)valute.Element("Nominal"));

        if (value is > 0 && nominal is > 0)
        {
            return value.Value / nominal.Value;
        }

        return null;
    }

    /// <summary>
    /// Разбирает число из ленты ЦБ РФ.
    /// </summary>
    /// <remarks>
    /// Все числовые поля ленты используют запятую как десятичный разделитель,
    /// а VunitRate для мелких валют дополнительно приходит в экспоненциальной
    /// форме, например «4,84195E-05». Поэтому запятая заменяется на точку и
    /// значение разбирается инвариантной культурой.
    /// </remarks>
    private static decimal? ParseDecimal(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var normalized = raw.Trim().Replace(',', '.');

        return decimal.TryParse(
            normalized,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : null;
    }
}
