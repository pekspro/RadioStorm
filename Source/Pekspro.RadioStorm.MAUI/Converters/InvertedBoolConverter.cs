namespace Pekspro.RadioStorm.MAUI.Converters;

internal class InvertedBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is bool b)
        {
            return !b;
        }
        else
        {
            return false;
        }        
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
