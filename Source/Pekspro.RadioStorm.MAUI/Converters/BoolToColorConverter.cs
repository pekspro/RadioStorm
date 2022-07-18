namespace Pekspro.RadioStorm.MAUI.Converters;

#nullable enable

public class BoolToColorConverter : IValueConverter
{
    public BoolToColorConverter()
    {
    }

    public Color TrueColor { get; set; } = null!;
    
    public Color FalseColor { get; set; } = null!;

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
