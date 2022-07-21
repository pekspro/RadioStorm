using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Pekspro.RadioStorm.Audio;
using Pekspro.RadioStorm.Sandbox.Common;
using Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider;
using Pekspro.RadioStorm.UI;
using Pekspro.RadioStorm.Utilities;

namespace Pekspro.RadioStorm.Sandbox.Console;

public class Program
{
    public static void Main(string[] args)
    {
        SQLitePCL.Batteries.Init();

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();

                services.TryAddSingleton<IAudioManager, DummyAudioManager>();
                services.TryAddSingleton<IMainThreadRunner, MainThreadRunner>();
                services.TryAddSingleton<IVersionProvider, VersionProvider>();
                services.AddRadioStorm(hostContext.Configuration);
                services.AddRadioStormSandboxTools(hostContext.Configuration);
                services.AddRadioStormFileProviders(hostContext.Configuration, true);
                services.AddRadioStormUI(hostContext.Configuration);
            });
}
