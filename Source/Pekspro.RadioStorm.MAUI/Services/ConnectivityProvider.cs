namespace Pekspro.RadioStorm.MAUI.Services;

internal class ConnectivityProvider : IConnectivityProvider
{
    public bool HasInternetAccess => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
}
