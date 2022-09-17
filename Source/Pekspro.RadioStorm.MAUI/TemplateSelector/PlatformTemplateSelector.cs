namespace Pekspro.RadioStorm.MAUI.TemplateSelector;

public sealed class PlatformTemplateSelector : DataTemplateSelector
{
    public DataTemplate DefaultTemplate { get; set; } = null!;

    public DataTemplate WindowsTemplate { get; set; } = null!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
#if WINDOWS
        return WindowsTemplate;
#else
        return DefaultTemplate;
#endif
    }
}
