using MediaState = Pekspro.RadioStorm.UI.Model.MediaState;

namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

public class MediaStateConverter : IValueConverter
{
    public MediaStateConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is MediaState b)
        {
            if (b == MediaState.CanPlay)
            {
                return "\u25B6";
            }
            else if (b == MediaState.CanPause)
            {
                return "\u23F8";
            }
        }

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
