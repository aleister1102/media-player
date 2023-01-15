using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaPlayer
{
    internal class ShuffleButtonBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isShuffled = (bool)value;

            string color = isShuffled ? "#bee6fd" : "#ddd";

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}