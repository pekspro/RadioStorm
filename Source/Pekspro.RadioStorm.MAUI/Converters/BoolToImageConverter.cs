namespace Pekspro.RadioStorm.MAUI.Converters;

#nullable enable

public class BoolToImageConverter : IValueConverter
{
    public BoolToImageConverter()
    {
    }

    #region TrueImage property

    private IImageSource? _TrueImage;

    public IImageSource? TrueImage
    {
        get
        {
            return _TrueImage;
        }
        set
        {
            _TrueImage = value;
        }
    }

    #endregion

    #region FalseImage property

    private IImageSource? _FalseImage;

    public IImageSource? FalseImage
    {
        get
        {
            return _FalseImage;
        }
        set
        {
            _FalseImage = value;
        }
    }

    #endregion

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b && b)
        {
            return TrueImage!;
        }

        return FalseImage!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
