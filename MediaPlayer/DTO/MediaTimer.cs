using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace MediaPlayer.DTO
{
    internal class MediaTimer : INotifyPropertyChanged
    {
        public DispatcherTimer Timer { get; set; } = new();
        public TimeSpan TimeElapsed { get; set; } = TimeSpan.Zero;
        public TimeSpan TimeRemaining { get; set; } = TimeSpan.Zero;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Start() => Timer.Start();

        public void Pause() => Timer.Stop();

        public void Stop()
        {
            Timer.Stop();
            TimeElapsed = TimeSpan.Zero;
        }

        public void InitNewTimer(EventHandler timerTick)
        {
            Timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1, 0) };
            Timer.Tick += timerTick;
        }
    }
}