namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

public class BoolToSelectionModeConverter : IValueConverter
{
    public BoolToSelectionModeConverter()
    {
    }

    public SelectionMode TrueSelectionMode { get; set; } = SelectionMode.Multiple;

    public SelectionMode FalseSelectionMode { get; set; } = SelectionMode.Single;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b && b)
        {
            return TrueSelectionMode!;
        }

        return FalseSelectionMode!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
