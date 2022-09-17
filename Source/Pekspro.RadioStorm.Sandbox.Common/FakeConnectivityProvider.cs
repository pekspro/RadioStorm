namespace Pekspro.RadioStorm.Sandbox.Common;

public sealed class FakeConnectivityProvider : IConnectivityProvider
{
    public bool HasInternetAccess { get; set; } = true;
}
