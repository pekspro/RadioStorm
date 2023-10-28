namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

public sealed class BoolToDoubleConverter : IValueConverter
{
    public BoolToDoubleConverter()
    {
        TrueValue = 1;
        FalseValue = 0;
    }

    public double TrueValue { get; set; }
    public double FalseValue { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool v = false;
        if (value is bool)
        {
            v = (bool)value;
        }

        if (v)
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
