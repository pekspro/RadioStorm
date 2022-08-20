namespace Pekspro.RadioStorm.MAUI.TemplateSelector;

public class FavoriteTemplateSelector : DataTemplateSelector
{
    public DataTemplate ChannelTemplate { get; set; } = null!;

    public DataTemplate ProgramTemplate { get; set; } = null!;

    public DataTemplate ChannelTemplateWindows { get; set; } = null!;

    public DataTemplate ProgramTemplateWindows { get; set; } = null!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
#if WINDOWS
        if (item is ChannelModel)
        {
            return ChannelTemplateWindows;
        }
        else
        {
            return ProgramTemplateWindows;
        }
#else
        if (item is ChannelModel)
        {
            return ChannelTemplate;
        }
        else
        {
            return ProgramTemplate;
        }
#endif
    }
}
