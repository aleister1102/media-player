using MediaPlayer.DTO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Threading;

namespace MediaPlayer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private readonly MediaController _mediaController = new();
        private readonly MediaTimer _mediaTimer = new();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _mediaController;

            TimeElapsed.DataContext = _mediaTimer;
            TimeRemaining.DataContext = _mediaTimer;
            ProgressSlider.DataContext = _mediaTimer;
        }

        private void AddMediaButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog { Multiselect = true };

            if (browsingScreen.ShowDialog() is true)
            {
                LoadMediaFrom(browsingScreen.FileNames.ToArray());
            }
        }

        private void LoadMediaFrom(string[] filesPaths)
        {
            foreach (var filePath in filesPaths)
            {
                var currentMediaList = _mediaController.CurrentPlaylist.MediaList;

                if (currentMediaList.Any((media) => media.FilePath == filePath) is false)
                {
                    currentMediaList.Add(new Media() { FilePath = filePath });
                }
            }
        }

        private void MediaListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedMedia = (Media)MediaListView.SelectedItem;

            if (selectedMedia is not null)
            {
                _mediaController.CurrentMedia = selectedMedia;

                MediaListView.ScrollIntoView(MediaListView.SelectedItem);

                Player.Source = new Uri(selectedMedia.FilePath, UriKind.Absolute);

                InitNewTimer();
            }
        }

        private void InitNewTimer()
        {
            _mediaTimer.Timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1, 0) };
            _mediaTimer.Timer.Tick += TimerTick;
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            int hours = Player.Position.Hours;
            int minutes = Player.Position.Minutes;
            int seconds = Player.Position.Seconds;

            var currentPosition = new TimeSpan(hours, minutes, seconds);

            if (currentPosition <= _mediaTimer.TimeRemaining)
                _mediaTimer.TimeElapsed = currentPosition;
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            int hours = Player.NaturalDuration.TimeSpan.Hours;
            int minutes = Player.NaturalDuration.TimeSpan.Minutes;
            int seconds = Player.NaturalDuration.TimeSpan.Seconds;

            _mediaTimer.TimeRemaining = new TimeSpan(hours, minutes, seconds);

            ProgressSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source is not null)
            {
                if (_mediaController.IsPlaying())
                {
                    Player.Pause();
                    _mediaTimer.Stop();
                    _mediaController.UpdateState(MediaState.Paused);
                }
                else
                {
                    Player.Play();
                    _mediaTimer.Start();
                    _mediaController.UpdateState(MediaState.Playing);
                }
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source is not null)
            {
                Player.Stop();
                _mediaTimer.Stop();
                _mediaController.UpdateState(MediaState.Stopped);
            }
        }

        private void ProgressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            double value = ProgressSlider.Value;

            TimeSpan newPosition = TimeSpan.FromSeconds(value);

            Player.Position = newPosition;
        }

        private void PlaylistComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedPlayList = (MediaPlaylist)PlaylistComboBox.SelectedItem;

            if (selectedPlayList is not null)
            {
                _mediaController.CurrentPlaylist = selectedPlayList;
            }
        }

        private void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            string playListName = PlaylistNameTextBox.Text;

            if (playListName == "")
            {
                Message.Text = "Fail: please type a playlist name!";
            }
            else
            {
                if (_mediaController.Playlists.Any(playlist => playlist.Name == playListName))
                    Message.Text = "Fail: this playlist already exists!";
                else
                {
                    var newPlaylist = new MediaPlaylist { Name = playListName };

                    _mediaController.Playlists.Add(newPlaylist);

                    PlaylistComboBox.SelectedItem = newPlaylist;

                    Message.Text = "Success: create playlist successfully!";
                }
            }
        }

        private void SavePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            MediaPlaylist playList = _mediaController.CurrentPlaylist;

            if (Directory.Exists("playlists") is false)
            {
                Directory.CreateDirectory("playlists");
            }

            if (playList is not null)
            {
                string playListName = playList.Name;

                if (playListName is "")
                {
                    Message.Text = "Fail: please create a playlist first!";
                    return;
                }

                string path = Path.Combine("playlists", $"{playListName}.txt");

                List<string> mediaList = playList.MediaList.Select(media => media.FilePath).ToList();

                File.WriteAllText(path, $"Playlist name:  {playListName}\n");
                File.AppendAllLines(path, mediaList);

                Message.Text = $"Success: save {playListName} playlist successfully!";
            }
        }

        private void ClearMediaListButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaController.CurrentPlaylist.MediaList.Clear();
        }

        private void DeletePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var currentPlaylist = (MediaPlaylist)PlaylistComboBox.SelectedItem;

            if (currentPlaylist is not null)
                _mediaController.Playlists.Remove(currentPlaylist);
        }

        private void DeleteMedia_Click(object sender, RoutedEventArgs e)
        {
            var media = (Media)MediaListView.SelectedItem;

            if (media is not null)
                _mediaController.CurrentPlaylist.MediaList.Remove(media);
        }

        private void LoadPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog() { Multiselect = true };

            if (browsingScreen.ShowDialog() is true)
            {
                LoadPlaylistsFrom(browsingScreen.FileNames.ToArray());
            }
        }

        private void LoadPlaylistsFrom(string[] playlistsPath)
        {
            foreach (var playlistPath in playlistsPath)
            {
                string playlistName = Path.GetFileNameWithoutExtension(playlistPath);

                if (_mediaController.Playlists.Any((playlist) => playlist.Name == playlistName) is false)
                {
                    _mediaController.CurrentPlaylist = new MediaPlaylist { Name = playlistName };

                    string[] mediaPaths = File.ReadAllLines(playlistPath).Skip(1).ToArray();

                    LoadMediaFrom(mediaPaths);

                    _mediaController.Playlists.Add(_mediaController.CurrentPlaylist);
                }
            }

            PlaylistComboBox.SelectedIndex = 0;
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaController.IsShuffled = !_mediaController.IsShuffled;
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            int currentIndex = MediaListView.SelectedIndex;
            int totalMedia = _mediaController.CurrentPlaylist.MediaList.Count;
            int nextIndex = GetNextMediaIndex(currentIndex, totalMedia);

            MediaListView.SelectedIndex = nextIndex;
        }

        private int GetNextMediaIndex(int currentIndex, int totalMedia)
        {
            int nextIndex;

            if (_mediaController.IsShuffled)
            {
                nextIndex = new Random().Next(totalMedia);
            }
            else
                nextIndex = (currentIndex + 1) % totalMedia;

            return nextIndex;
        }
    }
}