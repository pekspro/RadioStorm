namespace Pekspro.RadioStorm.MAUI.Converters;

internal class StringToColorConverter : IValueConverter
{
    public Color FallbackColor { get; set; }
    public float Alpha { get; set; } = 1;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            var color = Color.Parse(value as string);

            if (Alpha != 1)
            {
                color = new Color(color.Red, color.Green, color.Blue, Alpha);
            }

            return color;
        }
        catch
        {
            return FallbackColor;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
