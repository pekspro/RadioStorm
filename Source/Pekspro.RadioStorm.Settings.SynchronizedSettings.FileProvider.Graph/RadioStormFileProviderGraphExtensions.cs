namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider.Graph;

public static class RadioStormFileProviderGraphExtensions
{
    public static IServiceCollection AddRadioStormFileProviderGraph(this IServiceCollection services, IConfiguration Configuration, bool useLocalRedirect)
    {
        //IConfigurationSection configurationSection = Configuration.GetSection("Graph");
        //if (configurationSection.Exists())
        //{
        //    services.Configure<PublicClientApplicationOptions>(configurationSection);
        //}

        
        if (!string.IsNullOrWhiteSpace(Secrets.Secrets.GraphClientId))
        {
            services.Configure<PublicClientApplicationOptions>(options =>
            {
                options.Instance = "https://login.microsoftonline.com/";
                options.ClientId = Secrets.Secrets.GraphClientId ?? throw new Exception("Graph client id is not configured.");
                options.TenantId = "common";

                if (useLocalRedirect)
                {
                    options.RedirectUri = "http://localhost"; // In WPF, Console. Maui Windows also works.
                }
                else
                {
                    options.RedirectUri = "msauth://com.pekspro.radiostorm"; // In MAUI
                }
            });
        }
    
        services.TryAddSingleton<IGraphHelper, GraphHelper>();
        services.TryAddSingleton<GraphFileProvider, GraphFileProvider>();
        services.TryAddTransient<GraphViewModel, GraphViewModel>();

        return services;
    }
}
