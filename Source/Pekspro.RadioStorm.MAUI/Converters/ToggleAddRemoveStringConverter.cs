namespace Pekspro.RadioStorm.MAUI.Converters;

public sealed class ToggleAddRemoveStringConverter : IMultiValueConverter
{
    public ToggleAddRemoveStringConverter()
    {
    }

    public string AddValue { get; set; } = string.Empty;
    
    public string RemoveValue { get; set; } = string.Empty;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
        {
            return string.Empty;
        }

        if (values[1] is bool b && b)
        {
            return RemoveValue;
        }

        return AddValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
