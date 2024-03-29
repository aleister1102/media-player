﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaPlayer.converters
{
    internal class TimeSpanToSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = (TimeSpan)value;

            double totalSeconds = timeSpan.TotalSeconds;

            return totalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}