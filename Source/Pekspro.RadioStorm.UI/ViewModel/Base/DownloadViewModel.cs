namespace Pekspro.RadioStorm.UI.ViewModel.Base;

public partial class DownloadViewModel : ObservableObject
{
    #region Private properties

    protected ILogger Logger { get; }

    protected IMainThreadRunner MainThreadRunner { get; }

    private RefreshSettings? NextRefreshSettings;

    private Task? ActiveRefreshTask;

    private CancellationTokenSource? ActiveCancellationTokenSource;

    #endregion

    #region Constructor

    public DownloadViewModel(ILogger? logger, IMainThreadRunner mainThreadRunner)
    {
        _DownloadState = DownloadStates.NoStarted;
        Logger = logger ?? NullLoggerFactory.Instance.CreateLogger("x");
        MainThreadRunner = mainThreadRunner;
    }

    #endregion

    #region Properties

    public enum DownloadStates
    {
        NoStarted,
        Downloading,
        Done,
        NoData,
        Cancelled,
        Error
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(HasData))]
    [NotifyPropertyChangedFor(nameof(HasNoData))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyCanExecuteChangedFor(nameof(FullRefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    protected DownloadStates _DownloadState;

    partial void OnDownloadStateChanged(DownloadStates value)
    {
        if (value != DownloadStates.Downloading)
        {
            IsRefreshing = false;
        }
    }

    private bool IsNotBusy => !IsBusy;

    public bool IsBusy => DownloadState == DownloadStates.Downloading;

    public bool HasError => DownloadState == DownloadStates.Error;

    public bool IsCancelled => DownloadState == DownloadStates.Cancelled;

    public bool HasNoData => DownloadState == DownloadStates.NoData;

    public bool HasData => DownloadState == DownloadStates.Done;

    [ObservableProperty]
    protected bool _IsRefreshing;

    #endregion

    #region Commands

    [ObservableProperty]
    private bool _IsActive;

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private void FullRefresh()
    {
        QueueRefresh(new RefreshSettings(AllowCache: false, FullRefresh: true));
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private void Refresh()
    {
        QueueRefresh(new RefreshSettings(AllowCache: false));
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private void Update()
    {
        QueueRefresh(new RefreshSettings(AllowCache: false, FullRefresh: true));
    }

    [RelayCommand(CanExecute = nameof(IsBusy))]
    private void Cancel()
    {
        CancelRefresh();
    }

    #endregion

    #region Methods

    public virtual void OnNavigatedTo(bool refresh = true)
    {
        IsActive = true;

        if (refresh)
        {
            if (DownloadState == DownloadStates.Done)
            {
                QueueRefresh(new RefreshSettings());
            }
            else
            {
                QueueRefresh(new RefreshSettings(AllowCache: true, FullRefresh: true));
            }
        }
    }

    public virtual void OnNavigatedFrom()
    {
        IsActive = false;

        CancelRefresh();
    }

    internal virtual Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected void QueueRefresh(RefreshSettings refreshSettings)
    {
        MainThreadRunner.RunInMainThread(() => MainThreadQueueRefreshAsync(refreshSettings));
    }

    protected async void MainThreadQueueRefreshAsync(RefreshSettings refreshSettings)
    {
        if (!IsActive)
        {
            Logger.LogTrace($"Page is not active. Ignoring refresh request.");
            return;
        }

        if (ActiveRefreshTask is null)
        {
            NextRefreshSettings = refreshSettings;

            while (NextRefreshSettings is not null)
            {
                ActiveCancellationTokenSource = new CancellationTokenSource();

                refreshSettings = NextRefreshSettings;
                NextRefreshSettings = null;

                Logger.LogTrace($"Refreshing with settings: {refreshSettings}");

                ActiveRefreshTask = RefreshAsync(refreshSettings, ActiveCancellationTokenSource.Token);

                try
                {
                    await ActiveRefreshTask;
                    IsRefreshing = false;
                    
                    Logger.LogTrace($"Completed refreshing with settings: {refreshSettings}");
                }
                catch (TaskCanceledException)
                {
                    DownloadState = DownloadStates.Cancelled;
                }
                catch (Exception ex)
                {
                    DownloadState = DownloadStates.Error;
                    IsRefreshing = false;

                    Logger.LogError(ex, $"Failed refreshing with settings: {refreshSettings}");
                }

                ActiveRefreshTask = null;
                ActiveCancellationTokenSource = null;
            }
        }
        else
        {
            NextRefreshSettings = refreshSettings;
            ActiveCancellationTokenSource!.Cancel();
        }
    }

    protected void CancelRefresh()
    {
        if (ActiveCancellationTokenSource is not null)
        {
            ActiveCancellationTokenSource.Cancel();
        }
    }

    #endregion
}
