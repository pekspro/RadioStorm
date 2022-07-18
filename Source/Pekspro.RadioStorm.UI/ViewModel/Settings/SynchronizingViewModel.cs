namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

public partial class SynchronizingViewModel : ObservableObject
{
    #region Private properties

    private ISharedSettingsManager SharedSettingsManager { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public SynchronizingViewModel()
    {
        SharedSettingsManager = null!;
        IsSynchronizing = false;
        LatestSynchronizingTime = SampleData.SampleTime;
    }

    public SynchronizingViewModel(
        ISharedSettingsManager sharedSettingsManager,
        IMessenger messenger,
        IMainThreadRunner mainThreadRunner,
        IBootstrapState bootstrapState
        )
    {
        SharedSettingsManager = sharedSettingsManager;

        messenger.Register<SynchronizingStateChanged>(this, (a, e) =>
        {
            mainThreadRunner.RunInMainThread(() =>
            {
                LatestSynchronizingTime = SharedSettingsManager.LatestSynchronizingTime;
                IsSynchronizing = SharedSettingsManager.IsSynchronizing;
            });
        });
        
        messenger.Register<FileProvidersInitialized>(this, (a, e) =>
        {
            mainThreadRunner.RunInMainThread(() =>
            {
                IsInitialized = true;
            });
        });

        messenger.Register<ProviderStateChangedEventArgs>(this, (a, b) => 
        {
            mainThreadRunner.RunInMainThread(() =>
            {
                OnPropertyChanged(nameof(HasAnyRemoteSignedInProvider));

                StartSynchronizingCommand.NotifyCanExecuteChanged();
                StartFullSynchronizingCommand.NotifyCanExecuteChanged();
            });
        });

        IsSynchronizing = SharedSettingsManager.IsSynchronizing;
        LatestSynchronizingTime = SharedSettingsManager.LatestSynchronizingTime;
        IsInitialized = bootstrapState.FileProvidersInitialized;
    }

    #endregion

    #region Properties

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartSynchronizingCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartFullSynchronizingCommand))]
    private bool _IsInitialized;

    public bool HasAnyRemoteSignedInProvider => SharedSettingsManager.HasAnyRemoteSignedInProvider;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartSynchronizingCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartFullSynchronizingCommand))]
    [NotifyPropertyChangedFor(nameof(CurrentSynchronizingText))]
    private bool _IsSynchronizing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentSynchronizingText))]
    private DateTimeOffset? _LatestSynchronizingTime;

    private bool CanStartSynchronizing => !IsSynchronizing && IsInitialized && HasAnyRemoteSignedInProvider;

    public string CurrentSynchronizingText => GetLatestSynchronizingText(LatestSynchronizingTime, IsSynchronizing);

    #endregion

    #region Commands

    [RelayCommand(CanExecute = nameof(CanStartSynchronizing))]
    private void StartSynchronizing()
    {
        SharedSettingsManager.SynchronizeSettingsAsync(new SynchronizeSettings(true, false));
    }

    [RelayCommand(CanExecute = nameof(CanStartSynchronizing))]
    private void StartFullSynchronizing()
    {
        SharedSettingsManager.SynchronizeSettingsAsync(new SynchronizeSettings(true, true));
    }

    #endregion


    #region Methods
    
    private string GetLatestSynchronizingText(DateTimeOffset? dateTime, bool isSynchronizing)
    {
        if (isSynchronizing)
        {
            return Strings.Settings_Synchronize_SyncState_Running;
        }

        if (!dateTime.HasValue)
        {
            return Strings.Settings_Synchronize_SyncState_None;
        }

        return string.Format(Strings.Settings_Synchronize_SyncState_Done, dateTime.Value.ToString("HH:mm:ss"));
    }

    #endregion
}
