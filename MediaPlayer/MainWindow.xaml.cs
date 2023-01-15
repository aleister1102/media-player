using MediaPlayer.DTO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        private readonly string _recentlyFile = "resources/recently.txt";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _mediaController;

            TimeElapsed.DataContext = _mediaTimer;
            TimeRemaining.DataContext = _mediaTimer;
            ProgressSlider.DataContext = _mediaTimer;

            LoadRecentlyPlayedMedia();
        }

        private void AddMediaButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Media|*.mp4;*.mkv;*.flv;*.mpg;*.mp3"
            };

            if (browsingScreen.ShowDialog() is true)
            {
                var currentPlaylist = _mediaController.CurrentPlaylist;

                LoadMedia(currentPlaylist, browsingScreen.FileNames.ToArray());
            }
        }

        private static void LoadMedia(MediaPlaylist playlist, string[] filesPaths)
        {
            foreach (var filePath in filesPaths)
            {
                var mediaList = playlist.MediaList;

                if (mediaList.Any((media) => media.FilePath == filePath) is false)
                {
                    mediaList.Add(new Media() { FilePath = filePath });
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
                Player.Stop();

                _mediaTimer.Stop();
                _mediaController.UpdateState(MediaState.Stopped);

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
                    _mediaTimer.Pause();
                    _mediaController.UpdateState(MediaState.Paused);
                }
                else
                {
                    Player.Play();
                    _mediaTimer.Start();
                    _mediaController.UpdateState(MediaState.Playing);

                    SavePlayedMedia(_mediaController.CurrentMedia);

                    LoadRecentlyPlayedMedia();
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
            var selectedPlaylist = (MediaPlaylist)PlaylistComboBox.SelectedItem;

            if (selectedPlaylist is not null)
            {
                _mediaController.CurrentPlaylist = selectedPlaylist;

                if (selectedPlaylist.Path != string.Empty)
                {
                    string[] mediaPaths = File.ReadAllLines(selectedPlaylist.Path).Skip(1).ToArray();

                    LoadMedia(_mediaController.CurrentPlaylist, mediaPaths);
                }
            }
        }

        private void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            string playListName = PlaylistNameTextBox.Text;

            if (playListName == "")
            {
                MessageBox.Show("Please type a playlist name!",
                                "Warning",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
            else
            {
                if (_mediaController.Playlists.Any(playlist => playlist.Name == playListName))
                    MessageBox.Show("This playlist already exists!",
                                    "Errot",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                else
                {
                    var newPlaylist = new MediaPlaylist { Name = playListName };

                    _mediaController.Playlists.Add(newPlaylist);
                    _mediaController.CurrentPlaylist = newPlaylist;

                    MessageBox.Show("Create playlist successfully!",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
            }
        }

        private void SavePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            MediaPlaylist playList = _mediaController.CurrentPlaylist;

            if (playList is not null)
            {
                string playListName = playList.Name;

                if (playListName is "")
                {
                    MessageBox.Show("Please create a playlist first!",
                                    "Warning",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                var savingScreen = new SaveFileDialog()
                {
                    FileName = playListName,
                    Filter = "Text Files|*.txt"
                };

                if (savingScreen.ShowDialog() == false) return;

                string path = savingScreen.FileName;

                List<string> mediaList = playList.MediaList.Select(media => media.FilePath).ToList();

                File.WriteAllText(path, $"Playlist name:  {playListName}\n");
                File.AppendAllLines(path, mediaList);

                MessageBox.Show($"Save '{playListName}' playlist successfully!",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        private void ClearPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaController.CurrentPlaylist.MediaList.Clear();
        }

        private void DeletePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var currentPlaylist = (MediaPlaylist)PlaylistComboBox.SelectedItem;

            if (currentPlaylist is not null)
            {
                _mediaController.Playlists.Remove(currentPlaylist);

                if (currentPlaylist.Path != string.Empty)
                    File.Delete(currentPlaylist.Path);
            }
        }

        private void DeleteMedia_Click(object sender, RoutedEventArgs e)
        {
            var media = (Media)MediaListView.SelectedItem;

            if (media is not null)
                _mediaController.CurrentPlaylist.MediaList.Remove(media);
        }

        private void LoadPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog()
            {
                Filter = "Text Files|*.txt",
                Multiselect = true
            };

            if (browsingScreen.ShowDialog() is true)
            {
                LoadPlaylists(browsingScreen.FileNames.ToArray());
            }
        }

        private void LoadPlaylists(string[] playlistsPath)
        {
            foreach (var playlistPath in playlistsPath)
            {
                string playlistName = Path.GetFileNameWithoutExtension(playlistPath);

                if (_mediaController.Playlists.Any((playlist) => playlist.Name == playlistName) is false)
                {
                    var currentPlaylist = new MediaPlaylist { Path = playlistPath, Name = playlistName };
                    string[] mediaPaths = File.ReadAllLines(playlistPath).Skip(1).ToArray();

                    LoadMedia(currentPlaylist, mediaPaths);

                    _mediaController.CurrentPlaylist = currentPlaylist;
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

            int nextIndex = _mediaController.IsShuffled
                ? new Random().Next(totalMedia)
                : (currentIndex + 1) % totalMedia;

            MediaListView.SelectedIndex = nextIndex;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = MediaListView.SelectedIndex;
            if (currentIndex is -1) return;

            int totalMedia = _mediaController.CurrentPlaylist.MediaList.Count;

            int previousIndex = _mediaController.IsShuffled
                ? new Random().Next(totalMedia)
                : (currentIndex - 1 < 0 ? 0 : currentIndex - 1);

            MediaListView.SelectedIndex = previousIndex;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = MediaListView.SelectedIndex;
            if (currentIndex is -1) return;

            int totalMedia = _mediaController.CurrentPlaylist.MediaList.Count;

            int nextIndex = _mediaController.IsShuffled
               ? new Random().Next(totalMedia)
               : (currentIndex + 1 == totalMedia ? totalMedia : currentIndex + 1);

            MediaListView.SelectedIndex = nextIndex;
        }

        private void SavePlayedMedia(Media media)
        {
            if (Directory.Exists("resources") is false)
                Directory.CreateDirectory("resources");

            File.AppendAllText(_recentlyFile, $"{media.FilePath}\n");
        }

        private void LoadRecentlyPlayedMedia()
        {
            var recentlyMedia = File.ReadAllLines(_recentlyFile);

            var recentlyPlayedList = _mediaController.RecentlyPlayedList;

            LoadMedia(recentlyPlayedList, recentlyMedia);
        }

        private void RecentlyPlayedListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedMedia = (Media)RecentlyPlayedListView.SelectedItem;

            if (selectedMedia is not null)
            {
                _mediaController.CurrentMedia = selectedMedia;

                RecentlyPlayedListView.ScrollIntoView(RecentlyPlayedListView.SelectedItem);

                Player.Source = new Uri(selectedMedia.FilePath, UriKind.Absolute);
                Player.Stop();

                _mediaTimer.Stop();
                _mediaController.UpdateState(MediaState.Stopped);

                InitNewTimer();
            }
        }

        private void ClearRecentlyListButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaController.RecentlyPlayedList.MediaList.Clear();

            File.WriteAllText(_recentlyFile, "");
        }

        private void ReloadPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var currentPlaylist = _mediaController.CurrentPlaylist;

            if (currentPlaylist.Path != string.Empty)
            {
                string[] mediaPaths = File.ReadAllLines(currentPlaylist.Path).Skip(1).ToArray();

                LoadMedia(_mediaController.CurrentPlaylist, mediaPaths);
            }
        }
    }
}