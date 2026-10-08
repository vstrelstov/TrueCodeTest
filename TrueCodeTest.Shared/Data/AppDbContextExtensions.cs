using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TrueCodeTest.Shared.Data;

public static class AppDbContextExtensions
{
    public static IServiceCollection AddAppDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string? migrationsAssembly = null)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringNames.Default);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Строка подключения '{ConnectionStringNames.Default}' не задана.");
        }

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly(
                migrationsAssembly ?? typeof(AppDbContext).Assembly.GetName().Name!)));

        return services;
    }
}
