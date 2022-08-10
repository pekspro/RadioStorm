using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pekspro.RadioStorm.Options;
using Pekspro.RadioStorm.Sandbox.Common.Options;
using Pekspro.RadioStorm.Settings;
using Pekspro.RadioStorm.Settings.SynchronizedSettings;
using Pekspro.RadioStorm.UI.Utilities;

namespace Pekspro.RadioStorm.Sandbox.Common;

public static class RadioStormSandboxToolExtensions
{
    public static IServiceCollection AddRadioStormSandboxTools(this IServiceCollection services, IConfiguration Configuration)
    {
        IConfigurationSection configurationSection = Configuration.GetSection("StorageLocation");
        services.Configure<FakeStorageLocations>(configurationSection);

        string baseStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PEK's Productions", "RadioStorm");
        services.Configure<StorageLocations>(a => a.ConfigureFromBasePath(baseStoragePath));

        services.TryAddSingleton<ISettingsService, SettingsService>();
        services.TryAddSingleton<FakeRoamingFileProvider1, FakeRoamingFileProvider1>();
        services.TryAddSingleton<FakeRoamingFileProvider2, FakeRoamingFileProvider2>();
        services.TryAddSingleton<IUriLauncher, UriLauncher>();
        services.TryAddSingleton<IConnectivityProvider, FakeConnectivityProvider>();

        return services;
    }

    public static void SetupFakeRoamingFileProviders(IServiceProvider services)
    {
        var fakeRoamingFileProvider1 = services.GetRequiredService<FakeRoamingFileProvider1>();
        var fakeRoamingFileProvider2 = services.GetRequiredService<FakeRoamingFileProvider2>();
        var sharedSettingsManager = services.GetRequiredService<ISharedSettingsManager>();
        sharedSettingsManager.RegisterFilerProvider(fakeRoamingFileProvider1);
        sharedSettingsManager.RegisterFilerProvider(fakeRoamingFileProvider2);
    }
}
