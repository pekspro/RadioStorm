namespace Pekspro.RadioStorm.MAUI.Views;

public partial class AboutControl : ContentView
{
	public AboutControl()
	{
		InitializeComponent();

        if (Services.ServiceProvider.Current is not null)
        {
            var aboutViewModel = Services.ServiceProvider.GetRequiredService<AboutViewModel>();

            BindingContext = aboutViewModel;
        }
    }

    private AboutViewModel ViewModel => (AboutViewModel) BindingContext;

    private readonly List<DateTime> TapTimestamps = new();

    private void VersionNumberLabel_Tapped(object sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        TapTimestamps.Add(now);

        // If more than 5 taps within latest 10 seconds
        if (TapTimestamps.Count(a => a >= now.AddSeconds(-10)) > 5)
        {
            TapTimestamps.Clear();
            ViewModel.LocalSettings.ShowDebugSettings = !ViewModel.LocalSettings.ShowDebugSettings;
        }
    }
}
