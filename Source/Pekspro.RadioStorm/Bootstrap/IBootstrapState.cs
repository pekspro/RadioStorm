namespace Pekspro.RadioStorm.Bootstrap;

public interface IBootstrapState
{
    public bool BootstrapCompleted { get; set; }

    public bool FileProvidersInitialized { get; set; }
}
