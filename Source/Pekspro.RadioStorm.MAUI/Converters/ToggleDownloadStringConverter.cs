namespace Pekspro.RadioStorm.MAUI.Converters;

public sealed class ToggleDownloadStringConverter : IMultiValueConverter
{
    public ToggleDownloadStringConverter()
    {
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
        {
            return string.Empty;
        }

        if (values[1] is bool b && b)
        {
            return Strings.General_RemoveDownload;
        }

        return Strings.General_Download;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
