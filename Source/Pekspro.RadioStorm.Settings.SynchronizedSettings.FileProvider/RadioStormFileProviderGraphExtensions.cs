namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider;

public static class RadioStormFileProviderExtensions
{
    public static IServiceCollection AddRadioStormFileProviders(this IServiceCollection services, IConfiguration configuration, bool useLocalRedirect)
    {
        services.AddRadioStormFileProviderGraph(configuration, useLocalRedirect);

        services.TryAddSingleton<IFileProvidersManager, FileProvidersManager>();

        return services;
    }
}
