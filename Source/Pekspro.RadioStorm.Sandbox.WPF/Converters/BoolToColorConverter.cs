namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

public sealed class BoolToColorConverter : IValueConverter
{
    public BoolToColorConverter()
    {
    }

    #region TrueValue property

    public Color TrueValue { get; set; }

    #endregion


    #region FalseValue property

    public Color FalseValue { get; set; }

    #endregion


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool v = false;
        if (value is bool)
        {
            v = (bool)value;
        }

        if (v)
        {
            return TrueValue;
        }

        return FalseValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
