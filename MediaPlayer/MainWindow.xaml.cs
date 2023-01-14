using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

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
    }
}