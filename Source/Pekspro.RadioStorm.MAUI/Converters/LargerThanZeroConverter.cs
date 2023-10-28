namespace Pekspro.RadioStorm.MAUI.Converters;

public sealed class LargerThanZeroConverter : IValueConverter
{
    public LargerThanZeroConverter()
    {
    }

    public bool TrueValue { get; set; } = true;
    
    public bool FalseValue { get; set; } = false!;
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i && i > 0)
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
