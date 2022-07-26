namespace Pekspro.RadioStorm.MAUI.TemplateSelector;

public class FavoriteTemplateSelector : DataTemplateSelector
{
    public DataTemplate ChannelTemplate { get; set; } = null!;

    public DataTemplate ProgramTemplate { get; set; } = null!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is ChannelModel)
        {
            return ChannelTemplate;
        }
        else
        {
            return ProgramTemplate;
        }
    }
}
