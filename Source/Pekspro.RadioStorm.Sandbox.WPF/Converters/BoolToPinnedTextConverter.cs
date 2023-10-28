namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

public sealed class BoolToPinnedTextConverter : IValueConverter
{
    public BoolToPinnedTextConverter()
    {

    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool v = false;
        if (value is bool)
        {
            v = (bool)value;
        }

        if (v)
        {
            return "Unpin";
        }

        return "Pin";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
