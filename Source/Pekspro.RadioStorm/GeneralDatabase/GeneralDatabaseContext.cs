namespace Pekspro.RadioStorm.GeneralDatabase;

public sealed class GeneralDatabaseContext : DbContext
{
    private ILoggerFactory LoggerFactory { get; }

    public string FileName;

    public GeneralDatabaseContext()
    {
        // Only used by ef migrations.
        FileName = "@:";
        LoggerFactory = null!;
    }

    public GeneralDatabaseContext(IOptions<StorageLocations> storageLocationOptions, ILoggerFactory loggerFactory)
    {
        FileName = Path.Combine(storageLocationOptions.Value.LocalSettingsPath, "generaldb.sqlite");
        LoggerFactory = loggerFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLoggerFactory(LoggerFactory)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseModel(CompiledModel.GeneralDatabaseContextModel.Instance)
            .UseSqlite($"Data Source={FileName};")
#if DEBUG
            .EnableSensitiveDataLogging()
#endif            
            ;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DownloadState>().ToTable("DownloadState");

        modelBuilder.Entity<DownloadState>().HasKey(e => e.EpisodeId);
    }

    public DbSet<DownloadState> DownloadState { get; set; } = null!;
}
