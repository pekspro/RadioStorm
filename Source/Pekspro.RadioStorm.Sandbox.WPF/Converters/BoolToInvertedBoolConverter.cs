namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

public sealed class BoolToInvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }
        else
        {
            return false;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool)
        {
            var v = (bool)value;
            return !v;
        }

        return false;
    }
}
