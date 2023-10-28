namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

public sealed class SafeStringToBrushConverter : IValueConverter
{

    public SafeStringToBrushConverter()
    {
    }

    public Brush? FallbackBrush { get; set; }

    public double Opacity { get; set; } = 1;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Color? converted = SafeStringToColorConverter.GetColorFromHexString(value as string, Opacity);
        if (converted.HasValue)
        {
            SolidColorBrush br = new SolidColorBrush(converted.Value);

            return br;
        }

        return FallbackBrush!;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
