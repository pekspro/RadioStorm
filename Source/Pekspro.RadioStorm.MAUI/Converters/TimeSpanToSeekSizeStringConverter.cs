namespace Pekspro.RadioStorm.MAUI.Converters;

public sealed class TimeSpanToSeekSizeStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan sp)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (sp.TotalSeconds < 0)
            {
                stringBuilder.Append("-");
                sp = -sp;
            }
            else
            {
                stringBuilder.Append("+");
            }

            if (sp.Seconds != 0)
            {
                stringBuilder.Append(sp.Seconds);
                stringBuilder.Append("s");
            }
            else
            {
                stringBuilder.Append(sp.Minutes);
                stringBuilder.Append("m");
            }

            return stringBuilder.ToString();
        }

        return "?";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
