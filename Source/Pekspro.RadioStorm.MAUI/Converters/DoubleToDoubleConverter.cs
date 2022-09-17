namespace Pekspro.RadioStorm.MAUI.Converters;

public sealed class DoubleToDoubleConverter : IValueConverter
{
    public DoubleToDoubleConverter()
    {
    }

    public double Multiplier { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double v)
        {
            return v * Multiplier;
        }

        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
