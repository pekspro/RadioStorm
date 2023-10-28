namespace Pekspro.RadioStorm.MAUI.Services;

#nullable enable

public static class ServiceProviderHelper
{
    public static TService? GetService<TService>() => Current.GetService<TService>();
    public static TService GetRequiredService<TService>() where TService : notnull => Current.GetRequiredService<TService>();

    public static IServiceProvider Current => IPlatformApplication.Current!.Services;
}
