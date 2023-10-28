namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

sealed class SafeStringToColorConverter : IValueConverter
{

    public SafeStringToColorConverter()
    {
    }

    public Color FallbackColor { get; set; }
    public double Opacity { get; set; } = 1;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Color? converted = GetColorFromHexString(value as string, Opacity);
        if (converted.HasValue)
        {
            return converted.Value;
        }

        return FallbackColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public static Color? GetColorFromHexString(string? hexColor, double opacity)
    {
        if (hexColor is not null && hexColor.Length == 7 && hexColor.StartsWith("#"))
        {
            byte r, g, b;
            // from #AARRGGBB string
            if (
                    byte.TryParse(hexColor.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r)
                        &&
                    byte.TryParse(hexColor.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g)
                        &&
                    byte.TryParse(hexColor.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b)
               )
            {
                Color color = Color.FromArgb((byte)(255 * opacity), r, g, b);

                return color;
            }
        }

        return null;
    }
}
