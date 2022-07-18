namespace Pekspro.RadioStorm.MAUI.Converters;

#nullable enable

public class BoolToIntConverter : IValueConverter
{
    public BoolToIntConverter()
    {
    }

    public int TrueValue { get; set; }
    
    public int FalseValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b && b)
        {
            return TrueValue;
        }

        return FalseValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
