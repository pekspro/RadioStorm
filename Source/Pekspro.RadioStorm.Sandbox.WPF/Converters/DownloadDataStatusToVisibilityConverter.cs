namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

public sealed class DownloadDataStatusToVisibilityConverter : IValueConverter
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
        {
            return Visibility.Collapsed;
        }

        var value = (DownloadDataStatus)v;

        if (value == VisibilityValue)
        {
            return Visibility.Visible;
        }

        if (UseVisibilityValue2 && value == VisibilityValue2)
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
