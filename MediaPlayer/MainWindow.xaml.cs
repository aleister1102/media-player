using MediaPlayer.DTO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace MediaPlayer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private readonly MediaController _mediaController = new();
        private readonly string _recentlyFile = "resources/recently.txt";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _mediaController;

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
                var mediaPaths = browsingScreen.FileNames;

                LoadMedia(currentPlaylist, mediaPaths);
            }
        }

        private static void LoadMedia(MediaPlaylist playlist, string[] mediaPaths)
        {
            foreach (var filePath in mediaPaths)
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
            var currentMedia = (Media)MediaListView.SelectedItem;

            if (currentMedia is not null)
            {
                MediaListView.ScrollIntoView(currentMedia);

                Player.Stop();
                Player.Source = new Uri(currentMedia.FilePath, UriKind.Absolute);

                _mediaController.Timer.Stop();
                _mediaController.Timer.InitNewTimer(TimerTick);
                _mediaController.UpdateState(MediaState.Stopped);
                _mediaController.CurrentMedia = currentMedia;
            }
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            int hours = Player.Position.Hours;
            int minutes = Player.Position.Minutes;
            int seconds = Player.Position.Seconds;

            var currentPosition = new TimeSpan(hours, minutes, seconds);

            if (currentPosition <= _mediaController.Timer.TimeRemaining)
                _mediaController.Timer.TimeElapsed = currentPosition;
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            int hours = Player.NaturalDuration.TimeSpan.Hours;
            int minutes = Player.NaturalDuration.TimeSpan.Minutes;
            int seconds = Player.NaturalDuration.TimeSpan.Seconds;

            _mediaController.Timer.TimeRemaining = new TimeSpan(hours, minutes, seconds);

            ProgressSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source is not null)
            {
                if (_mediaController.IsPlaying())
                {
                    Player.Pause();
                    _mediaController.Timer.Pause();
                    _mediaController.UpdateState(MediaState.Paused);
                }
                else
                {
                    Player.Play();
                    _mediaController.Timer.Start();
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
                _mediaController.Timer.Stop();
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
            var currentPlaylist = (MediaPlaylist)PlaylistComboBox.SelectedItem;

            if (currentPlaylist is not null)
            {
                _mediaController.CurrentPlaylist = currentPlaylist;

                ReloadPlaylist(currentPlaylist);
            }
        }

        private static void ReloadPlaylist(MediaPlaylist currentPlaylist)
        {
            if (currentPlaylist.Path != string.Empty)
            {
                currentPlaylist.MediaList.Clear();

                var mediaPaths = File.ReadAllLines(currentPlaylist.Path).Skip(1).ToArray();

                LoadMedia(currentPlaylist, mediaPaths);
            }
        }

        private void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            string playlistName = PlaylistNameTextBox.Text;

            if (playlistName == "")
                MessageBox.Show("Please type a playlist name!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else if (_mediaController.Playlists.Any(playlist => playlist.Name == playlistName))
                MessageBox.Show("This playlist already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                var newPlaylist = new MediaPlaylist { Name = playlistName };

                _mediaController.Playlists.Add(newPlaylist);
                _mediaController.CurrentPlaylist = newPlaylist;

                MessageBox.Show("Create playlist successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SavePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var currentPlaylist = _mediaController.CurrentPlaylist;

            if (currentPlaylist is not null)
            {
                string playlistName = currentPlaylist.Name;

                if (playlistName is "")
                    MessageBox.Show("Please create a playlist first!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                {
                    currentPlaylist.Path = BrowseSavingPath(playlistName);

                    SavePlaylist(currentPlaylist);

                    MessageBox.Show($"Save '{playlistName}' playlist successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private static string BrowseSavingPath(string playlistName)
        {
            string path = "";

            var savingScreen = new SaveFileDialog()
            {
                FileName = playlistName,
                Filter = "Text Files|*.txt"
            };

            if (savingScreen.ShowDialog() is true)
                path = savingScreen.FileName;

            return path;
        }

        private static void SavePlaylist(MediaPlaylist playlist)
        {
            string path = playlist.Path;

            if (path is not "")
            {
                string playlistName = Path.GetFileNameWithoutExtension(path);
                List<string> mediaPaths = playlist.MediaList.Select(media => media.FilePath).ToList();

                File.WriteAllText(path, $"Playlist name:  {playlistName}\n");
                File.AppendAllLines(path, mediaPaths);
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
                var tempPlaylist = new MediaPlaylist() { Name = "Untitled" };
                _mediaController.Playlists.Add(tempPlaylist);
                _mediaController.CurrentPlaylist = tempPlaylist;
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
                var playlistPaths = browsingScreen.FileNames;

                LoadPlaylists(playlistPaths);
            }
        }

        private void LoadPlaylists(string[] playlistsPath)
        {
            foreach (var playlistPath in playlistsPath)
            {
                AddNewPlaylist(playlistPath);
            }
        }

        private void AddNewPlaylist(string playlistPath)
        {
            string playlistName = Path.GetFileNameWithoutExtension(playlistPath);

            if (IsPlaylistExists(playlistName) is false)
            {
                var newPlaylist = new MediaPlaylist { Path = playlistPath, Name = playlistName };
                string[] mediaPaths = File.ReadAllLines(playlistPath).Skip(1).ToArray();

                LoadMedia(newPlaylist, mediaPaths);

                _mediaController.Playlists.Add(newPlaylist);
                _mediaController.CurrentPlaylist = newPlaylist;
            }
        }

        private bool IsPlaylistExists(string playlistName)
        {
            bool result = _mediaController.Playlists.Any(playlist => playlist.Name == playlistName) is true;
            return result;
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaController.IsShuffled = !_mediaController.IsShuffled;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = MediaListView.SelectedIndex;
            if (currentIndex is -1) return;

            int mediaCount = _mediaController.CurrentPlaylist.MediaList.Count;
            int previousIndex = PreviousMedia(currentIndex, mediaCount);

            MediaListView.SelectedIndex = previousIndex;
        }

        private int PreviousMedia(int currentIndex, int mediaCount)
        {
            int previousIndex = _mediaController.IsShuffled
                ? new Random().Next(mediaCount)
                : (currentIndex - 1 < 0 ? 0 : currentIndex - 1);

            return previousIndex;
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            int currentIndex = MediaListView.SelectedIndex;

            int mediaCount = _mediaController.CurrentPlaylist.MediaList.Count;
            int nextIndex = NextMedia(currentIndex, mediaCount);

            MediaListView.SelectedIndex = nextIndex;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = MediaListView.SelectedIndex;
            if (currentIndex is -1) return;

            int mediaCount = _mediaController.CurrentPlaylist.MediaList.Count;
            int nextIndex = NextMedia(currentIndex, mediaCount);

            MediaListView.SelectedIndex = nextIndex;
        }

        private int NextMedia(int currentIndex, int mediaCount)
        {
            int nextIndex = _mediaController.IsShuffled
               ? new Random().Next(mediaCount)
               : (currentIndex + 1 == mediaCount ? mediaCount : currentIndex + 1);

            return nextIndex;
        }

        private void SavePlayedMedia(Media media)
        {
            if (Directory.Exists("resources") is false)
                Directory.CreateDirectory("resources");

            File.AppendAllText(_recentlyFile, $"{media.FilePath}\n");
        }

        private void LoadRecentlyPlayedMedia()
        {
            var recentlyPlayedList = _mediaController.RecentlyPlayedList;
            var recentlyMediaPaths = File.ReadAllLines(_recentlyFile);

            LoadMedia(recentlyPlayedList, recentlyMediaPaths);
        }

        private void RecentlyPlayedListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var currentMedia = (Media)RecentlyPlayedListView.SelectedItem;

            if (currentMedia is not null)
            {
                RecentlyPlayedListView.ScrollIntoView(RecentlyPlayedListView.SelectedItem);

                Player.Source = new Uri(currentMedia.FilePath, UriKind.Absolute);
                Player.Stop();

                _mediaController.Timer.Stop();
                _mediaController.Timer.InitNewTimer(TimerTick);
                _mediaController.CurrentMedia = currentMedia;
                _mediaController.UpdateState(MediaState.Stopped);
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
                ReloadPlaylist(currentPlaylist);
            }
        }
    }
}