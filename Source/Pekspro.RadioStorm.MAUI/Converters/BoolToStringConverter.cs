namespace Pekspro.RadioStorm.MAUI.Converters;

#nullable enable

public class BoolToStringConverter : IValueConverter
{
    public BoolToStringConverter()
    {
    }

    public String TrueValue { get; set; } = null!;
    
    public String FalseValue { get; set; } = null!;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b && b)
        {
            return TrueValue!;
        }

        return FalseValue!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
