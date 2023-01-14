using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MediaPlayer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private class File
        {
            public string Path { get; set; } = "";

            public string Name => System.IO.Path.GetFileName(Path);
        }

        private ObservableCollection<File> _files = new();
        private bool _playing = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FilesListView.ItemsSource = _files;
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
                if (_files.Any(file => file.Path == filePath) is false)
                {
                    Debug.WriteLine(filePath);
                    _files.Add(new File() { Path = filePath });
                }
            }
        }

        private void FilesListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedMedia = (File)FilesListView.SelectedItem;

            Player.Source = new Uri(selectedMedia.Path, UriKind.Absolute);
            Stop();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source is not null && _playing == false)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        private void Stop()
        {
            Player.Stop();

            _playing = false;
            PlayButtonImage.Source = new BitmapImage(new Uri("images/play.png", UriKind.Relative));
        }

        private void Play()
        {
            Player.Play();

            _playing = true;
            PlayButtonImage.Source = new BitmapImage(new Uri("images/pause.png", UriKind.Relative));
        }

        private void Pause()
        {
            Player.Pause();

            _playing = false;
            PlayButtonImage.Source = new BitmapImage(new Uri("images/play.png", UriKind.Relative));
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source is not null)
            {
                Stop();
                Play();
            }
        }
    }
}