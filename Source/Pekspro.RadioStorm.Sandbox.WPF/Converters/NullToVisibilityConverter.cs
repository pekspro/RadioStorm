namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

public sealed class NullToVisibilityConverter : IValueConverter
{
    public NullToVisibilityConverter()
    {
        NullValue = Visibility.Collapsed;
    }

    #region NullValue property

    private Visibility _NullValue;

    public Visibility NullValue
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

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return NullValue;
        }

        if (NullValue == Visibility.Collapsed)
        {
            return Visibility.Visible;
        }
        else
        {
            return Visibility.Collapsed;
        }

    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
