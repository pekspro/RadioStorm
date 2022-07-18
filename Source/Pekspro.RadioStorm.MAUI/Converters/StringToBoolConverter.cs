namespace Pekspro.RadioStorm.MAUI.Converters;

public class StringToBoolConverter : IValueConverter
{
    public StringToBoolConverter()
    {
        NullOrEmptyValue = false;
    }

    #region NullValue property

    private bool _NullValue;

    public bool NullOrEmptyValue
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
        string s = value as string;

        if (string.IsNullOrWhiteSpace(s))
        {
            return NullOrEmptyValue;
        }

        return !NullOrEmptyValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
