namespace Pekspro.RadioStorm.Sandbox.WPF.TemplateSelector
{
    public class FavoriteTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element is not null && item is not null)
            {
                if (item is ChannelModel)
                {
                    return element.FindResource("ChannelTemplate") as DataTemplate;
                }
                else
                {
                    return element.FindResource("ProgramTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }
}
