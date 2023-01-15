using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.DTO
{
    internal class MediaPlaylist
    {
        public string Name { get; set; } = string.Empty;

        public ObservableCollection<Media> MediaList { get; set; } = new();
    }
}