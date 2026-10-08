namespace TrueCodeTest.DbMigrator;

public sealed class MigratorOptions
{
    public const string SectionName = "Migrator";
    
    public int MaxAttempts { get; set; } = 5;

    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(3);
}
