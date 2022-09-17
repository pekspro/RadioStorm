namespace Pekspro.RadioStorm.Bootstrap;

internal sealed class BootstrapState : IBootstrapState
{
    #region Private properties
    
    private IMessenger Messenger { get; }
    private ILogger Logger { get; }

    #endregion

    #region Constructor
    
    public BootstrapState(IMessenger messenger, ILogger<BootstrapState> logger)
    {
        Messenger = messenger;
        Logger = logger;
    }

    #endregion

    #region Properties

    #region BootstrapCompleted property

    private bool _BootstrapCompleted;

    public bool BootstrapCompleted
    {
        get
        {
            return _BootstrapCompleted;
        }
        set
        {
            if (!value || _BootstrapCompleted)
            {
                Logger.LogError($"Cannot set {nameof(BootstrapCompleted)} to {value}. Current value: {_BootstrapCompleted}.");
                return;
            }
            else
            {
                _BootstrapCompleted = value;
                Messenger.Send(new BootstrapCompleted());
            }
        }
    }

    #endregion
    
    #region FileProvidersInitialized property

    private bool _FileProvidersInitialized;

    public bool FileProvidersInitialized
    {
        get
        {
            return _FileProvidersInitialized;
        }
        set
        {
            if (!value || _FileProvidersInitialized)
            {
                Logger.LogError($"Cannot set {nameof(FileProvidersInitialized)} to {value}.");
                return;
            }
            else
            {
                _FileProvidersInitialized = value;
                Messenger.Send(new FileProvidersInitialized());
            }
        }
    }

    #endregion

    #endregion
}
