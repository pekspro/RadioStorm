namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

public class NullImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return DependencyProperty.UnsetValue;
        }
        
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
