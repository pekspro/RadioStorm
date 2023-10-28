namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

public sealed class DateTimeOffsetToLocalStringConverter : IValueConverter
{
    public DateTimeOffsetToLocalStringConverter()
    {

    }

    public string Format { get; set; } = "HH:mm:ss";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset offset)
        {
            return offset.LocalDateTime.ToString(Format);
        }

        return "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
