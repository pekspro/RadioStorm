namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

public sealed class BoolToNullableBoolConverter : IValueConverter
{
    public BoolToNullableBoolConverter()
    {
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool v = false;
        if (value is bool)
        {
            v = (bool)value;
        }

        return v;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool?)
        {
            var v = (bool?)value;

            if (v.HasValue)
            {
                return v.Value;
            }
        }

        return false;
    }
}
