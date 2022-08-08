namespace Pekspro.RadioStorm.Utilities;

public interface IConnectivityProvider
{
    bool HasInternetAccess { get; }
}
