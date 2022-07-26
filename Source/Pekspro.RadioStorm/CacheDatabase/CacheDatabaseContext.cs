namespace Pekspro.RadioStorm.CacheDatabase;

public class CacheDatabaseContext : DbContext
{
    public string FileName;

    private ILoggerFactory LoggerFactory;

    //private static readonly LoggerFactory _logger
    //    = new LoggerFactory(new[] { new ConsoleLoggerProvider((_, __) => true, true) });

    public CacheDatabaseContext()
    {
        // Only used by ef migrations.
        FileName = "@:";
        LoggerFactory = null!;
    }

    public CacheDatabaseContext(IOptions<StorageLocations> storageLocationOptions, ILoggerFactory loggerFactory)
    {
        FileName = Path.Combine(storageLocationOptions.Value.CacheSettingsPath, "cachedb.sqlite");
        LoggerFactory = loggerFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLoggerFactory(LoggerFactory)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseModel(CompiledModel.CacheDatabaseContextModel.Instance)
            .UseSqlite($"Data Source={FileName};");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EpisodeData>().ToTable("EpisodeData");
        modelBuilder.Entity<EpisodeListSyncStatusData>().ToTable("EpisodeListSyncStatusData");
        modelBuilder.Entity<ListSyncStatusData>().ToTable("ListSyncStatusData");
        modelBuilder.Entity<ProgramData>().ToTable("ProgramData");
        modelBuilder.Entity<ChannelData>().ToTable("ChannelData");
        modelBuilder.Entity<EpisodeSongListItemData>().ToTable("EpisodeSongListItemData");
        modelBuilder.Entity<EpisodeSongListSyncStatusData>().ToTable("EpisodeSongListSyncStatusData");
        modelBuilder.Entity<ChannelSongListItemData>().ToTable("ChannelSongListItemData");
        modelBuilder.Entity<ChannelSongListSyncStatusData>().ToTable("ChannelSongListSyncStatusData");
        modelBuilder.Entity<ScheduledEpisodeListItemData>().ToTable("ScheduledEpisodeListItemData");
        modelBuilder.Entity<ScheduledEpisodeListSyncStatusData>().ToTable("ScheduledEpisodeListSyncStatusData");
        modelBuilder.Entity<ChannelStatusData>().ToTable("ChannelStatusData");
        modelBuilder.Entity<DataBaseSchema>().ToTable("DataBaseSchema");

        // var dateTimeOffsetConverter = new DateTimeOffsetToTicksConverter();

        modelBuilder.Entity<EpisodeData>().HasKey(e => e.EpisodeId);
        modelBuilder.Entity<EpisodeData>().Property(e => e.EpisodeId).ValueGeneratedNever();
        modelBuilder.Entity<EpisodeData>().HasIndex(e => new { e.ProgramId, e.PublishDate });
        modelBuilder.Entity<EpisodeData>().Property(e => e.PublishDate).HasColumnType("bigint");
        modelBuilder.Entity<EpisodeData>().Property(e => e.PublishDate).HasConversion<DateTimeOffsetToTicksConverter>();
        modelBuilder.Entity<EpisodeData>().Property(e => e.AudioStreamWithMusicExpireDate).HasColumnType("bigint");
        modelBuilder.Entity<EpisodeData>().Property(e => e.AudioStreamWithMusicExpireDate).HasConversion<DateTimeOffsetToTicksConverter>(); ;
        modelBuilder.Entity<EpisodeData>().Property(e => e.LatestUpdateTime).HasColumnType("bigint");
        modelBuilder.Entity<EpisodeData>().Property(e => e.LatestUpdateTime).HasConversion<DateTimeOffsetToTicksConverter>();


        modelBuilder.Entity<EpisodeListSyncStatusData>().HasKey(e => e.ProgramId);
        modelBuilder.Entity<EpisodeListSyncStatusData>().Property(e => e.ProgramId).ValueGeneratedNever();
        modelBuilder.Entity<EpisodeListSyncStatusData>().Property(e => e.LatestUpdateTime).HasColumnType("bigint");
        modelBuilder.Entity<EpisodeListSyncStatusData>().Property(e => e.LatestUpdateTime).HasConversion<DateTimeOffsetToTicksConverter>();;
        modelBuilder.Entity<EpisodeListSyncStatusData>().Property(e => e.LatestFullSynchronizingTime).HasColumnType("bigint");
        modelBuilder.Entity<EpisodeListSyncStatusData>().Property(e => e.LatestFullSynchronizingTime).HasConversion<DateTimeOffsetToTicksConverter>();;

        modelBuilder.Entity<ListSyncStatusData>().HasKey(e => e.TypeId);
        modelBuilder.Entity<ListSyncStatusData>().Property(e => e.TypeId).ValueGeneratedNever();
        modelBuilder.Entity<ListSyncStatusData>().Property(e => e.LatestUpdateTime).HasColumnType("bigint");
        modelBuilder.Entity<ListSyncStatusData>().Property(e => e.LatestUpdateTime).HasConversion<DateTimeOffsetToTicksConverter>();;

        modelBuilder.Entity<ProgramData>().HasKey(e => e.ProgramId);
        modelBuilder.Entity<ProgramData>().Property(e => e.ProgramId).ValueGeneratedNever();
        modelBuilder.Entity<ProgramData>().Property(e => e.LatestUpdateTime).HasColumnType("bigint");
        modelBuilder.Entity<ProgramData>().Property(e => e.LatestUpdateTime).HasConversion<DateTimeOffsetToTicksConverter>();;

        modelBuilder.Entity<ChannelData>().HasKey(e => e.ChannelId);
        modelBuilder.Entity<ChannelData>().Property(e => e.ChannelId).ValueGeneratedNever();
        modelBuilder.Entity<ChannelData>().Property(e => e.LatestUpdateTime).HasColumnType("bigint");
        modelBuilder.Entity<ChannelData>().Property(e => e.LatestUpdateTime).HasConversion<DateTimeOffsetToTicksConverter>();;

        modelBuilder.Entity<EpisodeSongListItemData>().HasKey(e => e.EpisodeSongListItemDataId);
        modelBuilder.Entity<EpisodeSongListItemData>().Property(e => e.EpisodeSongListItemDataId).ValueGeneratedOnAdd();
        modelBuilder.Entity<EpisodeSongListItemData>().HasIndex(e => new { e.EpisodeId, e.PublishDate });
        modelBuilder.Entity<EpisodeSongListItemData>().Property(e => e.LatestUpdateTime).HasColumnType("bigint");
        modelBuilder.Entity<EpisodeSongListItemData>().Property(e => e.LatestUpdateTime).HasConversion<DateTimeOffsetToTicksConverter>();;
        modelBuilder.Entity<EpisodeSongListItemData>().Property(e => e.PublishDate).HasColumnType("bigint");
        modelBuilder.Entity<EpisodeSongListItemData>().Property(e => e.PublishDate).HasConversion<DateTimeOffsetToTicksConverter>();;

        modelBuilder.Entity<EpisodeSongListSyncStatusData>().HasKey(e => e.EpisodeId);
        modelBuilder.Entity<EpisodeSongListSyncStatusData>().Property(e => e.EpisodeId).ValueGeneratedNever();
        modelBuilder.Entity<EpisodeSongListSyncStatusData>().Property(e => e.LatestUpdateTime).HasColumnType("bigint");
        modelBuilder.Entity<EpisodeSongListSyncStatusData>().Property(e => e.LatestUpdateTime).HasConversion<DateTimeOffsetToTicksConverter>();;

        modelBuilder.Entity<ChannelSongListItemData>().HasKey(e => e.ChannelSongListItemDataId);
        modelBuilder.Entity<ChannelSongListItemData>().Property(e => e.ChannelSongListItemDataId).ValueGeneratedOnAdd();
        modelBuilder.Entity<ChannelSongListItemData>().HasIndex(e => new { e.ChannelId, e.PublishDate });
        modelBuilder.Entity<ChannelSongListItemData>().Property(e => e.LatestUpdateTime).HasColumnType("bigint");
        modelBuilder.Entity<ChannelSongListItemData>().Property(e => e.LatestUpdateTime).HasConversion<DateTimeOffsetToTicksConverter>();;
        modelBuilder.Entity<ChannelSongListItemData>().Property(e => e.PublishDate).HasColumnType("bigint");
        modelBuilder.Entity<ChannelSongListItemData>().Property(e => e.PublishDate).HasConversion<DateTimeOffsetToTicksConverter>();;

        modelBuilder.Entity<ChannelSongListSyncStatusData>().HasKey(e => e.ChannelId);
        modelBuilder.Entity<ChannelSongListSyncStatusData>().Property(e => e.ChannelId).ValueGeneratedNever();
        modelBuilder.Entity<ChannelSongListSyncStatusData>().Property(e => e.LatestUpdateTime).HasColumnType("bigint");
        modelBuilder.Entity<ChannelSongListSyncStatusData>().Property(e => e.LatestUpdateTime).HasConversion<DateTimeOffsetToTicksConverter>();;

        modelBuilder.Entity<ScheduledEpisodeListItemData>().HasKey(e => e.ScheduledEpisodeDataId);
        modelBuilder.Entity<ScheduledEpisodeListItemData>().Property(e => e.ScheduledEpisodeDataId).ValueGeneratedOnAdd();
        modelBuilder.Entity<ScheduledEpisodeListItemData>().HasIndex(e => new { e.ChannelId, e.Date });
        modelBuilder.Entity<ScheduledEpisodeListItemData>().Property(e => e.Date).HasColumnType("bigint");
        modelBuilder.Entity<ScheduledEpisodeListItemData>().Property(e => e.Date).HasConversion<DateTimeOffsetToTicksConverter>();;
        modelBuilder.Entity<ScheduledEpisodeListItemData>().Property(e => e.StartTimeUtc).HasColumnType("bigint");
        modelBuilder.Entity<ScheduledEpisodeListItemData>().Property(e => e.StartTimeUtc).HasConversion<DateTimeOffsetToTicksConverter>();;
        modelBuilder.Entity<ScheduledEpisodeListItemData>().Property(e => e.EndTimeUtc).HasColumnType("bigint");
        modelBuilder.Entity<ScheduledEpisodeListItemData>().Property(e => e.EndTimeUtc).HasConversion<DateTimeOffsetToTicksConverter>();;

        modelBuilder.Entity<ScheduledEpisodeListSyncStatusData>().HasKey(e => e.ChannelId);
        modelBuilder.Entity<ScheduledEpisodeListSyncStatusData>().Property(e => e.ChannelId).ValueGeneratedNever();
        modelBuilder.Entity<ScheduledEpisodeListSyncStatusData>().HasIndex(e => e.ChannelId);
        modelBuilder.Entity<ScheduledEpisodeListSyncStatusData>().HasIndex(e => e.Date);
        modelBuilder.Entity<ScheduledEpisodeListSyncStatusData>().Property(e => e.Date).HasColumnType("bigint");
        modelBuilder.Entity<ScheduledEpisodeListSyncStatusData>().Property(e => e.Date).HasConversion<DateTimeOffsetToTicksConverter>();;
        modelBuilder.Entity<ScheduledEpisodeListSyncStatusData>().Property(e => e.LatestUpdateTime).HasColumnType("bigint");
        modelBuilder.Entity<ScheduledEpisodeListSyncStatusData>().Property(e => e.LatestUpdateTime).HasConversion<DateTimeOffsetToTicksConverter>();;

        modelBuilder.Entity<ChannelStatusData>().HasKey(e => e.ChannelId);
        modelBuilder.Entity<ChannelStatusData>().Property(e => e.ChannelId).ValueGeneratedNever();
        modelBuilder.Entity<ChannelStatusData>().Property(e => e.CurrentStartTime).HasColumnType("bigint");
        modelBuilder.Entity<ChannelStatusData>().Property(e => e.CurrentStartTime).HasConversion<DateTimeOffsetToTicksConverter>();;
        modelBuilder.Entity<ChannelStatusData>().Property(e => e.CurrentEndTime).HasColumnType("bigint");
        modelBuilder.Entity<ChannelStatusData>().Property(e => e.CurrentEndTime).HasConversion<DateTimeOffsetToTicksConverter>();;
        modelBuilder.Entity<ChannelStatusData>().Property(e => e.LatestUpdateTime).HasColumnType("bigint");
        modelBuilder.Entity<ChannelStatusData>().Property(e => e.LatestUpdateTime).HasConversion<DateTimeOffsetToTicksConverter>();;
        modelBuilder.Entity<ChannelStatusData>().Property(e => e.NextStartTime).HasColumnType("bigint");
        modelBuilder.Entity<ChannelStatusData>().Property(e => e.NextStartTime).HasConversion<DateTimeOffsetToTicksConverter>();;
        modelBuilder.Entity<ChannelStatusData>().Property(e => e.NextEndTime).HasColumnType("bigint");
        modelBuilder.Entity<ChannelStatusData>().Property(e => e.NextEndTime).HasConversion<DateTimeOffsetToTicksConverter>();;

        modelBuilder.Entity<DataBaseSchema>().HasKey(e => e.DataBaseSchemaId);
        modelBuilder.Entity<DataBaseSchema>().Property(e => e.DataBaseSchemaId).ValueGeneratedNever();
    }

    public DbSet<EpisodeData> EpisodeData { get; set; } = null!;
    public DbSet<EpisodeListSyncStatusData> EpisodeListSyncStatusData { get; set; } = null!;
    public DbSet<ListSyncStatusData> ListSyncStatusData { get; set; } = null!;
    public DbSet<ProgramData> ProgramData { get; set; } = null!;
    public DbSet<ChannelData> ChannelData { get; set; } = null!;
    public DbSet<EpisodeSongListItemData> EpisodeSongListItemData { get; set; } = null!;
    public DbSet<EpisodeSongListSyncStatusData> EpisodeSongListSyncStatusData { get; set; } = null!;
    public DbSet<ChannelSongListItemData> ChannelSongListItemData { get; set; } = null!;
    public DbSet<ChannelSongListSyncStatusData> ChannelSongListSyncStatusData { get; set; } = null!;
    public DbSet<ScheduledEpisodeListItemData> ScheduledEpisodeListItemData { get; set; } = null!;
    public DbSet<ScheduledEpisodeListSyncStatusData> ScheduledEpisodeListSyncStatusData { get; set; } = null!;
    public DbSet<ChannelStatusData> ChannelStatusData { get; set; } = null!;
    public DbSet<DataBaseSchema> DataBaseSchema { get; set; } = null!;
}
