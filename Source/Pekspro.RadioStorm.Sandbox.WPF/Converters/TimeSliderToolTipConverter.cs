﻿namespace Pekspro.RadioStorm.Sandbox.WPF.Converters;

#nullable enable

sealed class TimeSliderToolTipConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var seconds = System.Convert.ToInt32(value);

        if (seconds >= 3600)
        {
            return string.Format("{0:00}:{1:00}:{2:00}", seconds / 60 / 60 % 60, seconds / 60 % 60, seconds % 60);
        }

        return string.Format("{0:00}:{1:00}", seconds / 60 % 60, seconds % 60);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
