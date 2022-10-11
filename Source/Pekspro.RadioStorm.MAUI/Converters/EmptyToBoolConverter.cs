namespace Pekspro.RadioStorm.MAUI.Converters;

public sealed class EmptyToBoolConverter : IValueConverter
{
    public EmptyToBoolConverter()
    {
        
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool emptyValue = true;

        if (parameter is string b)
        {
            emptyValue = System.Convert.ToBoolean(b);
        }
        
        if (value is not string s || string.IsNullOrWhiteSpace(s))
        {
            return emptyValue;
        }

        return !emptyValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
