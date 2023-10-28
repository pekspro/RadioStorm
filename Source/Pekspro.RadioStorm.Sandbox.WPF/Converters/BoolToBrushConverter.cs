namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

public sealed class BoolToBrushConverter : IValueConverter
{
    public BoolToBrushConverter()
    {
    }

    #region TrueValue property

    public Brush? TrueValue { get; set; }

    #endregion


    #region FalseValue property

    public Brush? FalseValue { get; set; }

    #endregion


    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool v = false;
        if (value is bool)
        {
            v = (bool)value;
        }

        if (v)
        {
            return TrueValue!;
        }

        return FalseValue!;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
