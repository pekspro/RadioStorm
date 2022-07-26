namespace Pekspro.RadioStorm.MAUI.Converters;

#nullable enable

public class StringToVisibilityConverter : IValueConverter
{
    public StringToVisibilityConverter()
    {
        NullOrEmptyValue = Visibility.Collapsed;
    }

    #region NullValue property

    private Visibility _NullValue;

    public Visibility NullOrEmptyValue
    {
        get
        {
            return _NullValue;
        }
        set
        {
            _NullValue = value;
        }
    }

    #endregion

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string? s = value as string;

        if (string.IsNullOrWhiteSpace(s))
        {
            return NullOrEmptyValue;
        }

        if (NullOrEmptyValue == Visibility.Collapsed)
        {
            return Visibility.Visible;
        }
        else
        {
            return Visibility.Collapsed;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
