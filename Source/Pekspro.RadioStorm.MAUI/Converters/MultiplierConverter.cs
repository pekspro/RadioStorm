namespace Pekspro.RadioStorm.MAUI.Converters;

public sealed class MultiplierConverter : IMultiValueConverter
{
    public double MinValue { get; set; } = 0.01;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || !targetType.IsAssignableFrom(typeof(double)))
        {
            return MinValue;
        }

        double result = 1;
        
        foreach (var value in values)
        {
            if (value is float f)
            {
                if (float.IsNormal(f))
                {
                    result *= f;
                }
                else
                {
                    return MinValue;
                }
            }
            else if (value is double d)
            {
                if (double.IsNormal(d))
                {
                    result *= d;
                }
                else
                {
                    return MinValue;
                }
            }
            else
            {
                return MinValue;
            }
        }
        
        return Math.Max(MinValue, result);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
