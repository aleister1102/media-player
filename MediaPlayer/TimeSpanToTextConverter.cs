using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaPlayer
{
    internal class TimeSpanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = (TimeSpan)value;

            string result = $"{timeSpan.Hours}:{timeSpan.Minutes}:{timeSpan.Seconds}";

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}