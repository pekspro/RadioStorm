namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

public sealed partial class SynchronizingViewModel : ObservableObject
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
        LatestSynchronizingResult = new SynchronizingResult(SampleData.SampleTime, true, true, new HashSet<string>());
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
                LatestSynchronizingResult = SharedSettingsManager.LatestSynchronizingResult;
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
        LatestSynchronizingResult = SharedSettingsManager.LatestSynchronizingResult;
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
    [NotifyPropertyChangedFor(nameof(CurrentSynchronizingTextShort))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(IsSynchronizingOrHasError))]
    private bool _IsSynchronizing;

    public bool IsSynchronizingOrHasError => IsSynchronizing || HasError;
    
    public bool HasError => !IsSynchronizing && LatestSynchronizingResult?.Successful == false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentSynchronizingText))]
    [NotifyPropertyChangedFor(nameof(CurrentSynchronizingTextShort))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(IsSynchronizingOrHasError))]
    private SynchronizingResult? _LatestSynchronizingResult;


    private bool CanStartSynchronizing => !IsSynchronizing && IsInitialized && HasAnyRemoteSignedInProvider;

    public string CurrentSynchronizingText => GetLatestSynchronizingText(LatestSynchronizingResult, IsSynchronizing);

    public string CurrentSynchronizingTextShort =>
        IsSynchronizing ? Strings.Settings_Synchronize_SyncState_Running :
        HasError ? Strings.Settings_Synchronize_SyncState_Failed_Short :
        Strings.Settings_Synchronize_SyncState_None;

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
    
    private string GetLatestSynchronizingText(SynchronizingResult? synchronizingResult, bool isSynchronizing)
    {
        if (isSynchronizing)
        {
            return Strings.Settings_Synchronize_SyncState_Running;
        }

        if (synchronizingResult is null)
        {
            return Strings.Settings_Synchronize_SyncState_None;
        }

        if (synchronizingResult.Successful)
        {
            return string.Format(Strings.Settings_Synchronize_SyncState_Done, synchronizingResult.SynchronizingTime.ToString("HH:mm:ss"));
        }
        else
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format(Strings.Settings_Synchronize_SyncState_Failed, synchronizingResult.SynchronizingTime.ToString("HH:mm:ss")));

            if (!synchronizingResult.HadInternetAccess)
            {
                sb.Append(Strings.Settings_Synchronize_SyncState_Failed_NoInternet);
            }
            else
            {
                sb.Append(string.Format(Strings.Settings_Synchronize_SyncState_Failed_ProviderError, String.Join(", ", synchronizingResult.FailedProviders)));
            }

            return sb.ToString();
        }
    }

    #endregion
}
