using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Vlc.DotNet.Wpf.Samples
{
    public class VolumeEventArgs : EventArgs
    {
        public double NewVolume { get; private set; }

        public VolumeEventArgs(double newVolume)
        {
            NewVolume = newVolume;
        }
    }

    public class MediaViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private double _volume;
        private double _progress;

        public event EventHandler<VolumeEventArgs> VolumeChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public double Volume { get { return _volume; } set { SetVolume(value); } }
        public double Progress { get { return _progress; } set { SetProgress(value); } }

        private void SetVolume(double volume)
        {
            if (_volume == volume)
                return;
            _volume = volume;
            var volumeChanged = VolumeChanged;
            if (volumeChanged != null)
                volumeChanged(this, new VolumeEventArgs(_volume));
            OnPropertyChanged("Volume");
        }

        private void SetProgress(double progress)
        {
            _progress = progress;
            OnPropertyChanged("Progress");
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            myControl.MediaPlayer.VlcLibDirectoryNeeded += OnVlcControlNeedsLibDirectory;
            myControl.MediaPlayer.PositionChanged += MediaPlayer_PositionChanged;
            myControl.MediaPlayer.Stopped += MediaPlayer_Stopped;
            myControl.MediaPlayer.EndReached += MediaPlayer_EndReached;
            myControl.MediaPlayer.EndInit();

            _viewModel = new MediaViewModel();
            _viewModel.VolumeChanged += _viewModel_VolumeChanged;
            DataContext = _viewModel;
        }

        void _viewModel_VolumeChanged(object sender, VolumeEventArgs e)
        {
            myControl.MediaPlayer.Audio.Volume = (int)(e.NewVolume * 200);
        }

        void MediaPlayer_PositionChanged(object sender, Core.VlcMediaPlayerPositionChangedEventArgs e)
        {
            _viewModel.Progress = e.NewPosition;
        }

        void MediaPlayer_Stopped(object sender, Core.VlcMediaPlayerStoppedEventArgs e)
        {
        }

        void MediaPlayer_EndReached(object sender, Core.VlcMediaPlayerEndReachedEventArgs e)
        {
        }

        private void OnVlcControlNeedsLibDirectory(object sender, Forms.VlcLibDirectoryNeededEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            if (currentDirectory == null)
                return;
            if (AssemblyName.GetAssemblyName(currentAssembly.Location).ProcessorArchitecture == ProcessorArchitecture.X86)
                e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"..\..\..\lib\x86\"));
            else
                e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"..\..\..\lib\x64\"));
        }

        private static bool IsLocalPath(string path)
        {
            if (path.StartsWith("http:\\", StringComparison.OrdinalIgnoreCase))
                return false;
            return new Uri(path).IsFile;
        }

        private static string GetFilePath(Window owner = null)
        {
            var fd = new Microsoft.Win32.OpenFileDialog { Filter = "Video Files|*.mpg;*.mp4;*.m4v;*.avi" };
            var result = fd.ShowDialog(owner) ?? false;
            if (!result)
                return null;
            return fd.FileName;
        }

        private void OnPlayButtonClick(object sender, RoutedEventArgs e)
        {
            var filePath = GetFilePath(this);
            if (filePath == null)
                return;
            if (myControl.MediaPlayer.State == Core.Interops.Signatures.MediaStates.Playing)
            {
                //myControl.MediaPlayer.Stop(); // causes deadlock if not placed in a separate thread!
                var thread = new System.Threading.Thread(delegate() { myControl.MediaPlayer.Stop(); });
                thread.Start();
            }
            if (IsLocalPath(filePath))
                myControl.MediaPlayer.Play(new FileInfo(filePath));
            else
                myControl.MediaPlayer.Play(new Uri(filePath));
            _viewModel.Volume = .5;
            //myControl.MediaPlayer.Play(new Uri("http://download.blender.org/peach/bigbuckbunny_movies/big_buck_bunny_480p_surround-fix.avi"));
            //myControl.MediaPlayer.Play(new FileInfo(@"..\..\..\Vlc.DotNet\Samples\Videos\BBB trailer.mov"));
        }

        private void OnStopButtonClick(object sender, RoutedEventArgs e)
        {
            myControl.MediaPlayer.Stop();
        }

        private void OnForwardButtonClick(object sender, RoutedEventArgs e)
        {
            var rate = myControl.MediaPlayer.Rate < 2 ? 2 : 1;
            myControl.MediaPlayer.Rate = rate;
        }

        private void GetLength_Click(object sender, RoutedEventArgs e)
        {
            GetLength.Content = myControl.MediaPlayer.Length + " ms";
        }

        private void GetCurrentTime_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentTime.Content = myControl.MediaPlayer.Time + " ms";
        }

        private void SetCurrentTime_Click(object sender, RoutedEventArgs e)
        {
            myControl.MediaPlayer.Time = 5000;
            SetCurrentTime.Content = myControl.MediaPlayer.Time + " ms";
        }
    }
}
