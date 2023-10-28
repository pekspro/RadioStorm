namespace Pekspro.RadioStorm.MAUI.Converters;

internal sealed class StringToColorConverter : BindableObject, IValueConverter
{
    public static readonly BindableProperty FallbackColorProperty =
        BindableProperty.Create(nameof(FallbackColor), typeof(Color), typeof(StringToColorConverter), null, BindingMode.OneWay, null, null);

    public Color FallbackColor
    {
        get { return (Color) GetValue(FallbackColorProperty); }
        set { SetValue(FallbackColorProperty, value); }
    }

    public float Alpha { get; set; } = 1;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
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

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
