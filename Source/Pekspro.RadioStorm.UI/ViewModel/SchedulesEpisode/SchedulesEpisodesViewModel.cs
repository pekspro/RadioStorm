using static Pekspro.RadioStorm.UI.ViewModel.Channel.ChannelDetailsViewModel;

namespace Pekspro.RadioStorm.UI.ViewModel.SchedulesEpisode;

public sealed partial class SchedulesEpisodesViewModel : DownloadViewModel
{
    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private ISchedulesEpisodeFactory SchedulesEpisodeFactory { get; }
    private IDateTimeProvider DateTimeProvider { get; }
    private DateOnly FirstDate { get; }    

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public SchedulesEpisodesViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        SchedulesEpisodeFactory = null!;
        DateTimeProvider = null!;
        DownloadState = DownloadStates.Done;

        Items.Add(SchedulesEpisodeModel.CreateWithSampleData(0));
        Items.Add(SchedulesEpisodeModel.CreateWithSampleData(1));
        Items.Add(SchedulesEpisodeModel.CreateWithSampleData(2));
    }

    public SchedulesEpisodesViewModel(
        IDataFetcher dataFetcher,
        ISchedulesEpisodeFactory songListItemModelFactory,
        IMainThreadTimerFactory mainThreadTimerFactory,
        IDateTimeProvider dateTimeProvider,
        IWeekdaynameHelper weekdaynameHelper,
        IMainThreadRunner mainThreadRunner,
        ILogger<SongsViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        SchedulesEpisodeFactory = songListItemModelFactory;
        DateTimeProvider = dateTimeProvider;

        FirstDate = DateOnly.FromDateTime(dateTimeProvider.SwedishNow);

        for(int i = 0; i < 10; i++)
        {
            DateOnly date = FirstDate.AddDays(i);

            (string name, _) = weekdaynameHelper.GetRelativeWeekdayName(date);

            DateOptions.Add(name);
        }
    }

    #endregion

    #region Properties

    public int ChannelId { get; set; }

    [ObservableProperty]
    public ObservableCollection<SchedulesEpisodeModel> _Items = new ObservableCollection<SchedulesEpisodeModel>();

    [ObservableProperty]
    private string? _Title;

    [ObservableProperty]
    private string? _ChannelColor;

    [ObservableProperty]
    public ObservableCollection<string> _DateOptions = new ObservableCollection<string>();

    [ObservableProperty]
    public int _DateOptionIndex;
    
    partial void OnDateOptionIndexChanged(int value)
    {
        QueueRefresh(new RefreshSettings(FullRefresh: true));
    }

    #endregion

    #region Methods

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        if (refreshSettings.FullRefresh)
        {
            DownloadState = DownloadStates.Downloading;
        }

        Items = new ObservableCollection<SchedulesEpisodeModel>();

        DateOnly date = FirstDate.AddDays(DateOptionIndex);
        
        Logger.LogInformation($"Getting scheduled episodes for channel {ChannelId} for date {date:yyyy-MM-dd} {refreshSettings}");
        var data = await DataFetcher.GetScheduledEpisodeListAsync(ChannelId, date, refreshSettings.AllowCache, cancellationToken);

        Logger.LogInformation($"Got scheduled episodes for channel {ChannelId} for date {date:yyyy-MM-dd} {refreshSettings}. {data!.Count} items");

        foreach (var item in data.OrderBy(a => a.Date))
        {
            var model = SchedulesEpisodeFactory.Create(item);

            Items.Add(model);
        }

        //DateOnly swedishCurrentDate = DateOnly.FromDateTime(DateTimeProvider.SwedishNow);

        //for (int i = 0; i < 10; i++)
        //{
        //    var date = swedishCurrentDate.AddDays(i);
        //    Logger.LogInformation($"Getting scheduled episodes for channel {ChannelId} for date {date:yyyy-MM-dd} {refreshSettings}");
        //    var data = await DataFetcher.GetScheduledEpisodeListAsync(ChannelId, date, refreshSettings.AllowCache, cancellationToken);

        //    Logger.LogInformation($"Got scheduled episodes for channel {ChannelId} for date {date:yyyy-MM-dd} {refreshSettings}. {data!.Count} items");

        //    foreach (var item in data.OrderBy(a => a.Date))
        //    {
        //        var model = SchedulesEpisodeFactory.Create(item);

        //        Items.Add(model);
        //    }
        //}

        if (Items.Count > 0)
        {
            DownloadState = DownloadStates.Done;
        }
        else
        {
            DownloadState = DownloadStates.NoData;
        }
    }

    public void OnNavigatedTo(object parameter)
    {
        StartParameter startParameter = 
            StartParameterHelper.Deserialize(parameter, ChannelDetailsStartParameterJsonContext.Default.StartParameter);
        
        ChannelId = startParameter.ChannelId;
        Title = startParameter.ChannelName;
        ChannelColor = startParameter.Color;

        if (!Items.Any())
        {
            QueueRefresh(new RefreshSettings(FullRefresh: true));
        }

        base.OnNavigatedTo();
    }

    #endregion
}
