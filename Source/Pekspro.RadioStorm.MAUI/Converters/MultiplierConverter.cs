namespace Pekspro.RadioStorm.MAUI.Converters;

#nullable enable

public class MultiplierConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || !targetType.IsAssignableFrom(typeof(double)))
        {
            return 0;
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
                    return 0;
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
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        
        return Math.Max(0, result);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
