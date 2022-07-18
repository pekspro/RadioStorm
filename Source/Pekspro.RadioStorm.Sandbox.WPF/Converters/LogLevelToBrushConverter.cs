namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

public class LogLevelToBrushConverter : IValueConverter
{
    public LogLevelToBrushConverter()
    {

    }

    public Brush DefaultValue { get; set; } = new SolidColorBrush(Color.FromRgb(0, 0, 0));

    public Brush TraceValue { get; set; } = new SolidColorBrush(Color.FromRgb(128, 128, 128));

    public Brush WarningValue { get; set; } = new SolidColorBrush(Color.FromRgb(173, 159, 67));

    public Brush ErrorValue { get; set; } = new SolidColorBrush(Color.FromRgb(173, 78, 67));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Error => ErrorValue,
                LogLevel.Critical => ErrorValue,
                LogLevel.Warning => WarningValue,
                LogLevel.Trace => TraceValue,
                LogLevel.Debug => TraceValue,
                _ => DefaultValue
            };
        }

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
