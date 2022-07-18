namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

public partial class DownloadSettingsViewModel : ObservableObject
{
    #region Private properties

    private ILocalSettings Settings { get; }
    private IDownloadManager DownloadManager { get; }
    private IListenStateManager ListenStateManager { get; }

    #endregion

    #region Constructor

    public DownloadSettingsViewModel()
        : this(null!, null!, null!, null!)
    {
        DownloadedCount = 12;
        DownloadedListenedCount = 7;
        DownloadedSize.SizeInBytes = 1024 * 1024 * 39 / 4;
    }

    public DownloadSettingsViewModel
        (
            ILocalSettings localSettings, 
            IDownloadManager downloadManager,
            IListenStateManager listenStateManager,
            IMessenger messenger
        )
    {
        Settings = localSettings;
        DownloadManager = downloadManager;
        ListenStateManager = listenStateManager;

        messenger?.Register<DownloadAdded>(this, (a, b) => RefreshProperties());
        messenger?.Register<DownloadDeleted>(this, (a, b) => RefreshProperties());
        messenger?.Register<DownloadUpdated>(this, (a, b) => RefreshProperties());

        messenger?.Register<ListenStateChanged>(this, (a, b) => RefreshProperties());

        var autoRemovedDownloadFileOptions = new List<AutoRemovedDownloadFileDescription>();
        autoRemovedDownloadFileOptions.Add(new AutoRemovedDownloadFileDescription(-1));
        autoRemovedDownloadFileOptions.Add(new AutoRemovedDownloadFileDescription(1));
        autoRemovedDownloadFileOptions.Add(new AutoRemovedDownloadFileDescription(2));
        autoRemovedDownloadFileOptions.Add(new AutoRemovedDownloadFileDescription(3));
        autoRemovedDownloadFileOptions.Add(new AutoRemovedDownloadFileDescription(5));
        autoRemovedDownloadFileOptions.Add(new AutoRemovedDownloadFileDescription(7));
        autoRemovedDownloadFileOptions.Add(new AutoRemovedDownloadFileDescription(14));
        autoRemovedDownloadFileOptions.Add(new AutoRemovedDownloadFileDescription(30));

        AutoRemovedDownloadFileOptions = autoRemovedDownloadFileOptions;

        if (listenStateManager is not null)
        {
            RefreshProperties();
        }
    }

    #endregion

    #region Properties

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteAllCommand))]
    private int _DownloadedCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ListenedString))]
    [NotifyCanExecuteChangedFor(nameof(DeleteListenedCommand))]
    private int _DownloadedListenedCount;

    public FileSize DownloadedSize { get; } = new FileSize();

    public string ListenedString
    {
        get
        {
            if (DownloadedListenedCount == 1)
            {
                return Strings.Settings_Downloads_Listened_Single;
            }

            return string.Format(Strings.Settings_Downloads_Listened_Multiple, DownloadedListenedCount);
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteAllCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteListenedCommand))]
    private bool _IsDeleting;

    public IReadOnlyList<AutoRemovedDownloadFileDescription> AutoRemovedDownloadFileOptions { get; }

    public AutoRemovedDownloadFileDescription AutoRemovedDownloadFilesValue
    {
        get
        {
            int s = Settings.AutoRemoveListenedDownloadedFilesDayDelay;

            var v = AutoRemovedDownloadFileOptions.FirstOrDefault(a => a.DayCount == s);
            if (v is not null)
            {
                return v;
            }

            return AutoRemovedDownloadFileOptions.First();
        }
        set
        {
            Settings.AutoRemoveListenedDownloadedFilesDayDelay = value.DayCount;
        }
    }

    #endregion

    #region Commands

    private bool CanDeleteAll => !IsDeleting && DownloadedCount > 0;

    [RelayCommand(CanExecute = nameof(CanDeleteAll))]
    private void DeleteAll()
    {
        DeleteDownloads(false);
    }

    private bool CanDeleteListened => !IsDeleting && DownloadedListenedCount > 0;

    [RelayCommand(CanExecute = nameof(CanDeleteListened))]
    private void DeleteListened()
    {
        DeleteDownloads(true);
    }

    #endregion

    #region Methods

    private void RefreshProperties()
    {
        var downloads = DownloadManager.GetDownloads();

        DownloadedCount = downloads.Count();

        DownloadedListenedCount =
            downloads.Where(a => ListenStateManager.IsFullyListen(a.EpisodeId)).Count();

        DownloadedSize.SizeInBytes = (ulong)downloads.Sum(a => (long)a.BytesDownloaded);
    }

    private void DeleteDownloads(bool onlyListened)
    {
        IsDeleting = true;

        var downloads = DownloadManager.GetDownloads();

        foreach (var download in downloads)
        {
            if (onlyListened && !ListenStateManager.IsFullyListen(download.EpisodeId))
            {
                continue;
            }

            DownloadManager.DeleteDownload(download.ProgramId, download.EpisodeId);
        }

        IsDeleting = false;
    }

    #endregion
}
