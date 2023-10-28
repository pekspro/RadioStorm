namespace Pekspro.RadioStorm.MAUI.Converters;

#nullable enable

public sealed class BoolToDoubleConverter : IValueConverter
{
    public BoolToDoubleConverter()
    {
    }

    public double TrueValue { get; set; }
    
    public double FalseValue { get; set; }
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
        {
            return TrueValue;
        }

        return FalseValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
