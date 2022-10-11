namespace Pekspro.RadioStorm.MAUI.Converters;

public sealed class OrConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null)
        {
            return false;
        }

        foreach (var value in values)
        {
            if (value is bool b)
            {
                if (b)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        
        return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
