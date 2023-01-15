using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaPlayer.converters
{
    internal class TimeSpanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = (TimeSpan)value;

            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;
            int seconds = timeSpan.Seconds;

            string result = $"{(hours < 10 ? $"0{hours}" : hours)}:" +
                            $"{(minutes < 10 ? $"0{minutes}" : minutes)}:" +
                            $"{(seconds < 10 ? $"0{seconds}" : seconds)}";

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}