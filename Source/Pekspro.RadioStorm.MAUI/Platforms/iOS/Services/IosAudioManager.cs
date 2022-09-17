using Microsoft.Toolkit.Mvvm.Messaging;
using Pekspro.RadioStorm.Audio;
using Pekspro.RadioStorm.Audio.Models;
using Pekspro.RadioStorm.Settings;
using Pekspro.RadioStorm.Settings.SynchronizedSettings.ListenState;
using Pekspro.RadioStorm.Settings.SynchronizedSettings.RecentHistory;
using Pekspro.RadioStorm.Utilities;
using System;

namespace Pekspro.RadioStorm.MAUI.Services;

sealed class IosAudioManager : AudioManagerBase
{
    public IosAudioManager(
        IMainThreadTimerFactory mainThreadTimerFactory,
        IMainThreadRunner mainThreadRunner,
        IListenStateManager listenStateManager,
        IRecentPlayedManager recentPlayedManager,
        IDownloadManager downloadManager,
        ILocalSettings localSettings,
        IMessenger messenger,
        ILogger<IosAudioManager> logger)
        : base(mainThreadTimerFactory, mainThreadRunner, listenStateManager, recentPlayedManager, downloadManager, localSettings, messenger, logger, false)
    {

    }

    protected override void MediaPlay(PlayListItem playlistItem)
    {
        throw new NotImplementedException();
    }

    protected override void MediaPlay()
    {
        throw new NotImplementedException();
    }

    protected override void MediaPause()
    {
        throw new NotImplementedException();
    }

    protected override void MediaSetPlaybackPosition(TimeSpan position)
    {
        throw new NotImplementedException();
    }

    protected override void MediaRefreshLengthAndPosition()
    {
        throw new NotImplementedException();
    }

    protected override void MediaRefreshButtonStates()
    {
        throw new NotImplementedException();
    }

    protected override void MediaSetVolume(double volume)
    {
        throw new NotImplementedException();
    }

    protected override void SetPlaybackRate(double speedRatio)
    {
        throw new NotImplementedException();
    }
}
