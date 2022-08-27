﻿using System;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pekspro.RadioStorm.Audio;
using Pekspro.RadioStorm.Audio.Models;
using Pekspro.RadioStorm.Downloads;
using Pekspro.RadioStorm.Settings;
using Pekspro.RadioStorm.Settings.SynchronizedSettings.ListenState;
using Pekspro.RadioStorm.Settings.SynchronizedSettings.RecentHistory;
using Pekspro.RadioStorm.Utilities;

namespace Pekspro.RadioStorm.Sandbox.Console;

class DummyAudioManager : AudioManagerBase
{
    public DummyAudioManager(
        IMainThreadTimerFactory mainThreadTimerFactory,
        IMainThreadRunner mainThreadRunner,
        IListenStateManager listenStateManager,
        IRecentPlayedManager recentPlayedManager,
        IDownloadManager downloadManager,
        ILocalSettings localSettings,
        IMessenger messenger,
        ILogger<DummyAudioManager> logger)
        : base(mainThreadTimerFactory, mainThreadRunner, listenStateManager, recentPlayedManager, downloadManager, localSettings, messenger, logger, false)
    {

    }

    protected override void MediaPlay(PlayList playlistItem)
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

    protected override void MediaRefreshButtonStates()
    {
        throw new NotImplementedException();
    }

    protected override void MediaRefreshLengthAndPosition()
    {
        throw new NotImplementedException();
    }
    
    protected override void MediaSetVolume(int volume)
    {
        throw new NotImplementedException();
    }

    public override bool HasVolumeSupport => false;
}
