using Microsoft.Extensions.Options;
using TrueCodeTest.CurrencyUpdater;
using TrueCodeTest.CurrencyUpdater.Cbr;
using TrueCodeTest.CurrencyUpdater.Options;
using TrueCodeTest.CurrencyUpdater.Services;
using TrueCodeTest.Shared.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<CurrencyUpdaterOptions>(
    builder.Configuration.GetSection(CurrencyUpdaterOptions.SectionName));

builder.Services.AddAppDbContext(builder.Configuration);

builder.Services.AddScoped<ICurrencyImporter, CurrencyImporter>();

var updaterOptions = builder.Configuration
    .GetSection(CurrencyUpdaterOptions.SectionName)
    .Get<CurrencyUpdaterOptions>() ?? new CurrencyUpdaterOptions();

builder.Services
    .AddHttpClient<ICbrClient, CbrClient>(client =>
    {
        client.BaseAddress = new Uri(updaterOptions.CbrDailyUrl);
        client.Timeout = updaterOptions.HttpTimeout;
    });

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
