using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.DTO
{
    public class MediaState
    {
        public static string Playing => "playing";
        public static string Paused => "paused";
        public static string Stopped => "stopped";
    }
}