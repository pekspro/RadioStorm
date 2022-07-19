namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

public class BoolToStringConverter : IValueConverter
{
    public BoolToStringConverter()
    {

    }


    #region TrueValue property

    private string _TrueValue;

    public string TrueValue
    {
        get
        {
            return _TrueValue;
        }
        set
        {
            _TrueValue = value;
        }
    }

    #endregion


    #region FalseValue property

    private string _FalseValue;

    public string FalseValue
    {
        get
        {
            return _FalseValue;
        }
        set
        {
            _FalseValue = value;
        }
    }

    #endregion


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool v = false;
        if (value is bool)
        {
            v = (bool)value;
        }

        if (v)
        {
            return TrueValue;
        }

        return FalseValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
