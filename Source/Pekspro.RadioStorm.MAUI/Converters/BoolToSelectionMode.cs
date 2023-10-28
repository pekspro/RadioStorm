namespace Pekspro.RadioStorm.MAUI.Converters;

public sealed class BoolToSelectionModeConverter : IValueConverter
{
    public BoolToSelectionModeConverter()
    {
    }

    public SelectionMode TrueSelectionMode { get; set; } = SelectionMode.Multiple;

#if WINDOWS

    // This enables an hovering effect (at least in dark mode).
    public SelectionMode FalseSelectionMode { get; set; } = SelectionMode.Single;
    
#else
    public SelectionMode FalseSelectionMode { get; set; } = SelectionMode.None;
    
#endif

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
        {
            return TrueSelectionMode!;
        }

        return FalseSelectionMode!;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
