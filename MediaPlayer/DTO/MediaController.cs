using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System;
using System.Windows;
using System.ComponentModel;

namespace MediaPlayer.DTO
{
    internal class MediaController : INotifyPropertyChanged
    {
        private readonly Dictionary<string, string> _playButtonImages = new()
            {
                {MediaState.Playing, "images/pause.png" },
                {MediaState.Paused, "images/play.png" },
                {MediaState.Stopped, "images/play.png" },
            };

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Media> MediaList { get; set; } = new();

        public Media CurrentMedia { get; set; } = new();

        public string State { get; set; } = "stop";

        public BitmapImage PlayButtonImage { get; set; } = new();

        public string WindowTitle { get; set; } = string.Empty;

        public MediaController()
        {
            PlayButtonImage = new BitmapImage(new Uri(_playButtonImages[MediaState.Stopped], UriKind.Relative));
        }

        public bool IsPlaying() => "playing" == State;

        public bool IsPaused() => "paused" == State;

        public bool IsStopped() => "stopped" == State;

        public void UpdateState(string newState)
        {
            State = newState;
            PlayButtonImage = new BitmapImage(new Uri(_playButtonImages[State], UriKind.Relative));
            WindowTitle = $"MediaPlayer is {State} - {CurrentMedia.Name}";
        }
    }
}