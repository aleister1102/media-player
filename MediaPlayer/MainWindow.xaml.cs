using MediaPlayer.DTO;
using Microsoft.Win32;
using System;
using System.ComponentModel;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _mediaController;

            TimeElapsed.DataContext = _mediaTimer;
            TimeRemaining.DataContext = _mediaTimer;
            ProgressSlider.DataContext = _mediaTimer;
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
                if (_mediaController.MediaList.Any((media) => media.FilePath == filePath) is false)
                {
                    _mediaController.MediaList.Add(new Media() { FilePath = filePath });
                }
            }
        }

        private void MediaListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedMedia = (Media)MediaListView.SelectedItem;

            _mediaController.CurrentMedia = selectedMedia;

            Player.Source = new Uri(selectedMedia.FilePath, UriKind.Absolute);

            InitNewTimer();
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

            _mediaTimer.TimeElapsed = new TimeSpan(hours, minutes, seconds);
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
                    PauseMedia();
                else
                    PlayMedia();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source is not null)
                StopMedia();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source is not null)
            {
                StopMedia();
                PlayMedia();
            }
        }

        private void PlayMedia()
        {
            Player.Play();
            _mediaTimer.Start();
            _mediaController.UpdateState(MediaState.Playing);
        }

        private void PauseMedia()
        {
            Player.Pause();
            _mediaTimer.Stop();
            _mediaController.UpdateState(MediaState.Paused);
        }

        private void StopMedia()
        {
            Player.Stop();
            _mediaTimer.Stop();
            _mediaController.UpdateState(MediaState.Stopped);
        }

        private void ProgressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            double value = ProgressSlider.Value;

            TimeSpan newPosition = TimeSpan.FromSeconds(value);

            Player.Position = newPosition;
        }
    }
}