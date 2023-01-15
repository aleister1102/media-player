using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MediaPlayer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private class Media
        {
            public string FilePath { get; set; } = string.Empty;

            public string Name => Path.GetFileName(FilePath);
        }

        private readonly ObservableCollection<Media> _files = new();

        private string _currentMedia = string.Empty;

        private bool _isPlaying = false;

        private DispatcherTimer _timer;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MediaListView.ItemsSource = _files;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog { Multiselect = true };

            if (browsingScreen.ShowDialog() == true)
            {
                LoadFilesFrom(browsingScreen.FileNames.ToArray());
            }
        }

        private void LoadFilesFrom(string[] filesPaths)
        {
            foreach (var filePath in filesPaths)
            {
                if (_files.Any(file => file.FilePath == filePath) is false)
                {
                    _files.Add(new Media() { FilePath = filePath });
                }
            }
        }

        private void MediaListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedMedia = (Media)MediaListView.SelectedItem;

            _currentMedia = selectedMedia.FilePath;

            Player.Source = new Uri(_currentMedia, UriKind.Absolute);

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1, 0) };
            _timer.Tick += TimerTick;
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            int hours = Player.Position.Hours;
            int minutes = Player.Position.Minutes;
            int seconds = Player.Position.Seconds;
            TimeElapsed.Text = $"{hours}:{minutes}:{seconds}";
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            int hours = Player.NaturalDuration.TimeSpan.Hours;
            int minutes = Player.NaturalDuration.TimeSpan.Minutes;
            int seconds = Player.NaturalDuration.TimeSpan.Seconds;
            TimeRemaining.Text = $"{hours}:{minutes}:{seconds}";

            ProgressSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source is not null)
            {
                if (_isPlaying == false)
                    PlayMedia();
                else
                    PauseMedia();
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source is not null)
            {
                StopMedia();
                PlayMedia();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source is not null)
                StopMedia();
        }

        private void PlayMedia()
        {
            Player.Play();

            _isPlaying = true;

            _timer.Start();

            PlayButtonImage.Source = new BitmapImage(new Uri("images/pause.png", UriKind.Relative));

            Title = $"MediaPlayer is playing - {Path.GetFileName(Player.Source.LocalPath)}";
        }

        private void PauseMedia()
        {
            Player.Pause();

            _isPlaying = false;

            _timer.Stop();

            PlayButtonImage.Source = new BitmapImage(new Uri("images/play.png", UriKind.Relative));

            Title = $"MediaPlayer is paused - {Path.GetFileName(Player.Source.LocalPath)}";
        }

        private void StopMedia()
        {
            Player.Stop();

            _isPlaying = false;

            _timer.Stop();

            PlayButtonImage.Source = new BitmapImage(new Uri("images/play.png", UriKind.Relative));

            Title = $"MediaPlayer is stopped - {Path.GetFileName(Player.Source.LocalPath)}";
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = ProgressSlider.Value;
            TimeSpan newPosition = TimeSpan.FromSeconds(value);
            Player.Position = newPosition;
        }
    }
}