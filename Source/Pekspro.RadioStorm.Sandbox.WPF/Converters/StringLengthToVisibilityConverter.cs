namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

public sealed class StringLengthToVisibilityConverter : IValueConverter
{
    public StringLengthToVisibilityConverter()
    {
        EmptyValue = Visibility.Collapsed;
    }

    #region NullValue property

    private Visibility _NullValue;

    public Visibility EmptyValue
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
        if (value is null || string.IsNullOrEmpty(value.ToString()))
        {
            return EmptyValue;
        }

        if (EmptyValue == Visibility.Collapsed)
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
