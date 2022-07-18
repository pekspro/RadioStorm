namespace Pekspro.RadioStorm.MAUI.Converters;

public class DownloadDataStatusToVisibilityConverter : IValueConverter
{
    public DownloadDataStatusToVisibilityConverter()
    {
    }

    public DownloadDataStatus VisibilityValue { get; set; }
    public DownloadDataStatus VisibilityValue2 { get; set; }
    public bool UseVisibilityValue2 { get; set; }

    public object Convert(object v, Type targetType, object parameter, CultureInfo culture)
    {
        if (!(v is DownloadDataStatus))
            return false;

        var value = (DownloadDataStatus)v;

        if (value == VisibilityValue)
            return true;
        
        if (UseVisibilityValue2 && value == VisibilityValue2)
            return true;

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
