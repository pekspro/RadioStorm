namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

public sealed record AutoRemovedDownloadFileDescription(int DayCount)
{
    public override string ToString()
    {
        return DayCount switch
        {
            < 0 => Strings.Settings_Downloads_AutoRemove_Option_Never,
            1 => Strings.Settings_Downloads_AutoRemove_Option_OneDay,
            _ => string.Format(Strings.Settings_Downloads_AutoRemove_Option_XDays, DayCount)
        };
    }
}
