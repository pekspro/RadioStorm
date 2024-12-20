﻿namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public BoolToVisibilityConverter()
    {
        VisibleValue = true;
    }

    #region VisibleValue property

    private bool _VisibleValue;

    public bool VisibleValue
    {
        get
        {
            return _VisibleValue;
        }
        set
        {
            _VisibleValue = value;
        }
    }

    #endregion



    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool v = false;
        if (value is bool)
        {
            v = (bool)value;
        }

        if (v == VisibleValue)
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
