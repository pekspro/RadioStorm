using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pekspro.RadioStorm;
using Pekspro.RadioStorm.Audio;
using Pekspro.RadioStorm.Sandbox.Common;
using Pekspro.RadioStorm.Sandbox.Console;
using Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider;
using Pekspro.RadioStorm.UI;
using Pekspro.RadioStorm.Utilities;

SQLitePCL.Batteries.Init();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<Worker>();

        services.TryAddSingleton<IAudioManager, DummyAudioManager>();
        services.TryAddSingleton<IMainThreadRunner, MainThreadRunner>();
        services.TryAddSingleton<IVersionProvider, VersionProvider>();
        services.AddRadioStorm(ctx.Configuration);
        services.AddRadioStormSandboxTools(ctx.Configuration);
        services.AddRadioStormFileProviders(ctx.Configuration, true);
        services.AddRadioStormUI(ctx.Configuration);
    })
.Build();

await host.RunAsync();
