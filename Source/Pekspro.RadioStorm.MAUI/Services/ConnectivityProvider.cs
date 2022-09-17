namespace Pekspro.RadioStorm.MAUI.Services;

internal sealed class ConnectivityProvider : IConnectivityProvider
{
    public bool HasInternetAccess => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
}
