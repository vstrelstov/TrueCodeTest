namespace TrueCodeTest.CurrencyUpdater.Options;

public sealed class CurrencyUpdaterOptions
{
    public const string SectionName = "CurrencyUpdater";

    /// <summary>Адрес ленты ежедневных курсов ЦБ РФ.</summary>
    public string CbrDailyUrl { get; set; } = "https://www.cbr.ru/scripts/XML_daily.asp";

    /// <summary>Пауза между циклами обновления.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>Запускать ли обновление сразу при старте сервиса.</summary>
    public bool RunOnStartup { get; set; } = true;

    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Количество попыток на один цикл, 1 — без повторов.</summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>Базовая задержка между попытками.</summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(2);
}
