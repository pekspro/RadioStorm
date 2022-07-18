namespace Pekspro.RadioStorm.GeneralDatabase;

public class GeneralDatabaseContext : DbContext
{
    public string FileName;

    public GeneralDatabaseContext()
    {
        // Only used by ef migrations.
        FileName = "@:";
    }

    public GeneralDatabaseContext(IOptions<StorageLocations> storageLocationOptions)
    {
        FileName = Path.Combine(storageLocationOptions.Value.LocalSettingsPath, "generaldb.sqlite");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder /*.AddFilter("Microsoft", LogLevel.Warning)
					   .AddFilter("System", LogLevel.Warning)
					   .AddFilter("SampleApp.Program", LogLevel.Debug)*/
                   .AddConsole();
        });

        optionsBuilder
            .UseLoggerFactory(loggerFactory)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseModel(CompiledModel.GeneralDatabaseContextModel.Instance)
            .UseSqlite($"Data Source={FileName};");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DownloadState>().ToTable("DownloadState");

        modelBuilder.Entity<DownloadState>().HasKey(e => e.EpisodeId);
    }

    public DbSet<DownloadState> DownloadState { get; set; } = null!;
}
