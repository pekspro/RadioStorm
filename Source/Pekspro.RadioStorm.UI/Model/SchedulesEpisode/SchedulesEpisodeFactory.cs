namespace Pekspro.RadioStorm.UI.Model.SchedulesEpisode;

public class SchedulesEpisodeFactory : ISchedulesEpisodeFactory
{
    #region Private properites
    
    private IServiceProvider ServiceProvider { get; }

    #endregion

    #region Constructor
    
    public SchedulesEpisodeFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    #endregion

    #region Methods

    public SchedulesEpisodeModel Create(ScheduledEpisodeListItemData scheduledEpisodeListItemData)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<SchedulesEpisodeModel>>();
        var weekdaynameHelper = ServiceProvider.GetRequiredService<IWeekdaynameHelper>();
        var dateTimeProvider = ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var model = new SchedulesEpisodeModel(scheduledEpisodeListItemData, weekdaynameHelper, dateTimeProvider, logger);

        return model;
    }

    #endregion
}
