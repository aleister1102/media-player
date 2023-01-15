using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace MediaPlayer.converters
{
    internal class StateToToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string state = (string)value;

            string toolTip = "Play/Pause";

            if (state == DTO.MediaState.Playing)
                toolTip = "Pause";
            else if (state == DTO.MediaState.Paused || state == DTO.MediaState.Stopped)
                toolTip = "Play";

            return toolTip;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}