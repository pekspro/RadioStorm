namespace Pekspro.RadioStorm.Sandbox.Common;

public class FakeConnectivityProvider : IConnectivityProvider
{
    public bool HasInternetAccess { get; set; } = true;
}
