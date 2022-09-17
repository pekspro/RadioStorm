namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider.Graph;

public sealed partial class GraphViewModel : ObservableObject
{
    #region Private properties

    private IGraphHelper GraphHelper { get; }
    private IMainThreadRunner MainThreadRunner { get; }
    private IBootstrapState BootstrapState { get; }

    #endregion

    #region Constructor

    public GraphViewModel()
    {
        GraphHelper = null!;
        MainThreadRunner = null!;
        BootstrapState = null!;
        IsSignedIn = true;
        IsBusy = false;
        UserName = "user@example.com";
    }

    public GraphViewModel(
        IGraphHelper graphHelper,
        IMainThreadRunner mainThreadRunner,
        IBootstrapState bootstrapState,
        IMessenger messenger
        )
    {
        GraphHelper = graphHelper;
        MainThreadRunner = mainThreadRunner;
        BootstrapState = bootstrapState;
        IsTestEnvironment = Secrets.Secrets.IsTestEnvironment;

        messenger.Register<ProviderStateChangedEventArgs>(this, (a, b) => { UpdateState(); });
        messenger.Register<FileProvidersInitialized>(this, (a, b) => { UpdateState(); });
        
        UpdateState();
    }

    #endregion

    #region Properties

    [ObservableProperty]
    private bool _IsInitialized;

    [ObservableProperty]
    private bool _IsSignedIn;

    [ObservableProperty]
    private bool _IsBusy;

    [ObservableProperty]
    private string? _UserName;

    [ObservableProperty]
    private ProviderState _ProviderState;

    public bool IsTestEnvironment { get; }
    
    #endregion

    #region Commands

    private bool CanSignIn => !IsBusy && !IsSignedIn;

    [RelayCommand(CanExecute = nameof(CanSignIn))]
    public void SignIn()
    {
        _ = GraphHelper.SignIn();
    }

    private bool CanSignOut => !IsBusy && IsSignedIn;

    [RelayCommand(CanExecute = nameof(CanSignOut))]
    public void SignOut()
    {
        _ = GraphHelper.SignOut();
    }

    #endregion

    #region Methods

    private void UpdateState()
    {
        MainThreadRunner.RunInMainThread(() =>
        {
            IsSignedIn = GraphHelper.State == ProviderState.SignedIn;
            IsBusy = GraphHelper.State == ProviderState.Loading;
            ProviderState = GraphHelper.State;
            UserName = GraphHelper.UserName;
            IsInitialized = BootstrapState.FileProvidersInitialized && GraphHelper.IsConfigured;

            SignInCommand.NotifyCanExecuteChanged();
            SignOutCommand.NotifyCanExecuteChanged();
        });
    }

    #endregion
}
