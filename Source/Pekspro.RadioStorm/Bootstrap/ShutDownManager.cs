namespace Pekspro.RadioStorm.Bootstrap;

internal sealed class ShutDownManager : IShutDownManager
{
    #region Private properties
    
    private IDownloadManager DownloadManager;

    private ISharedSettingsManager SharedSettingsManager;

    private IAudioManager AudioManager;

    #endregion

    #region Constructor

    public ShutDownManager(IDownloadManager downloadManager, ISharedSettingsManager sharedSettingsManager, IAudioManager audioManager)
    {
        DownloadManager = downloadManager;
        SharedSettingsManager = sharedSettingsManager;
        AudioManager = audioManager;
    }

    #endregion

    #region Methods

    public Task ShutDownAsync()
    {
        AudioManager.Pause();

        List<Task> tasks = new List<Task>
        {
            SharedSettingsManager.ForceSaveNowAsync(),
            DownloadManager.ShutDownAsync()
        };

        return Task.WhenAll(tasks);
    }

    #endregion
}
