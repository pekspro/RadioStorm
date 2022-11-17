namespace Pekspro.RadioStorm.Sandbox.WPF.ScheduledEpisode;

public sealed partial class ScheduledEpisodesControl : UserControl
{
    public ScheduledEpisodesControl()
    {
        InitializeComponent();

        if (App.ServiceProvider is not null)
        {
            var messenger = App.ServiceProvider.GetRequiredService<IMessenger>();
            var mainThreadRunner = App.ServiceProvider.GetRequiredService<IMainThreadRunner>();

            messenger.Register<SemiCompletedScheduleEpisodesListLoaded>(this, (a, b) =>
            {
                mainThreadRunner.RunInMainThread(() =>
                {
                    if (SchedulesEpisodesViewModel.ChannelId == b.ChannelId)
                    {
                        ListViewScheduledEpisodes.ScrollIntoView(SchedulesEpisodesViewModel.Items[b.IndexOfFirstIncompleted]);
                    }
                });
            });
        }
    }

    private SchedulesEpisodesViewModel SchedulesEpisodesViewModel => (SchedulesEpisodesViewModel) DataContext;
}
