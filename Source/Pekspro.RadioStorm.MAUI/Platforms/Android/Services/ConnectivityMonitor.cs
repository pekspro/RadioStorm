﻿using Android.Net;

namespace Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

sealed class ConnectivityMonitor : ConnectivityManager.NetworkCallback
{
    private ILogger? _Logger;

    private ILogger Logger
    {
        get
        {
            return _Logger ??= ServiceProviderHelper.GetRequiredService<ILogger<ConnectivityMonitor>>();
        }
    }


    public override void OnAvailable(Network network)
    {
        base.OnAvailable(network);

        Logger.LogInformation("Network available: {0}", network);
    }

    public override void OnLost(Network network)
    {
        base.OnLost(network);

        Logger.LogInformation("Network lost: {0}", network);
    }
}
