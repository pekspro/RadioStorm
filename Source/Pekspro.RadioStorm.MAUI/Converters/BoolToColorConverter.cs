namespace Pekspro.RadioStorm.MAUI.Converters;

public sealed class BoolToColorConverter : BindableObject, IValueConverter
{
    public BoolToColorConverter()
    {
    }

    public static readonly BindableProperty TrueColorProperty =
        BindableProperty.Create(nameof(TrueColor), typeof(Color), typeof(BoolToColorConverter), null, BindingMode.OneWay, null, null);

    public Color TrueColor
    {
        get { return (Color) GetValue(TrueColorProperty); }
        set { SetValue(TrueColorProperty, value); }
    }

    public static readonly BindableProperty FalseColorProperty =
        BindableProperty.Create(nameof(FalseColor), typeof(Color), typeof(BoolToColorConverter), null, BindingMode.OneWay, null, null);

    public Color FalseColor
    {
        get { return (Color) GetValue(FalseColorProperty); }
        set { SetValue(FalseColorProperty, value); }
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b && b)
        {
            return TrueColor!;
        }

        return FalseColor!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
