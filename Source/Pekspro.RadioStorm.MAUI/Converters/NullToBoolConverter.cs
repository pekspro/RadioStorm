namespace Pekspro.RadioStorm.MAUI.Converters;

public sealed class NullToBoolConverter : IValueConverter
{
    public NullToBoolConverter()
    {
        NullValue = false;
    }

    #region NullValue property

    private bool _NullValue;

    public bool NullValue
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
        if (value is null)
        {
            return NullValue;
        }

        return !NullValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
