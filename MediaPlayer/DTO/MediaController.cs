﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;

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

        public ObservableCollection<MediaPlaylist> Playlists { get; set; } = new();
        public MediaPlaylist CurrentPlaylist { get; set; } = new();
        public MediaPlaylist RecentlyPlayedList { get; set; } = new();
        public Media CurrentMedia { get; set; } = new();

        public string State { get; set; } = "stop";
        public BitmapImage PlayButtonImage { get; set; } = new();
        public string WindowTitle { get; set; } = string.Empty;
        public bool IsShuffled { get; set; } = false;

        public MediaTimer Timer { get; set; } = new();

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