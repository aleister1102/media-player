using System.Collections.ObjectModel;

namespace MediaPlayer.DTO
{
    internal class MediaPlaylist
    {
        public string Path { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public ObservableCollection<Media> MediaList { get; set; } = new();
    }
}