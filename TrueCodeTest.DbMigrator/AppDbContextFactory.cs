using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TrueCodeTest.Shared;
using TrueCodeTest.Shared.Data;

namespace TrueCodeTest.DbMigrator;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<AppDbContextFactory>(optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString(ConnectionStringNames.Default)
            ?? "Host=localhost;Port=5432;Database=devdb;Username=postgres;Password=password";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(
                typeof(AppDbContextFactory).Assembly.GetName().Name!))
            .Options;

        return new AppDbContext(options);
    }
}
