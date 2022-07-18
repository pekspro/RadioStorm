namespace Pekspro.RadioStorm.Bootstrap;

public interface IVersionMigrator
{
    Task MigrateAsync(string previousVersion, string newVersion);
}
