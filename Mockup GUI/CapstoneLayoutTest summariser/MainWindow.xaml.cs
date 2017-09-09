using DataGraph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.IO.Compression;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Globalization;

namespace CapstoneLayoutTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //CONSTANTS
        private const int CONTROLS_HIDE_SPEED = 10;
        private const int CONTROLS_SHOW_SPEED = 2;
        private const int CONTROLS_MIN_HEIGHT = 0;
        private const int CONTROLS_MAX_HEIGHT = 25;
        private const int CONTROLS_HIDE_DELAY = 500;
        private const int PROGRESS_BAR_UPDATE_SPEED = 15;
        private const int SCREENSHOT_TIME = 150;

        //GLOBALS
        private bool videoState = true;
        private BackgroundWorker HideControlsThread;
        private BackgroundWorker VideoProgressThread;
        private BackgroundWorker ShowControllsThread;
        private List<double> startingTimes;
        private List<double> endingTimes;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MainWindow(string loadPath)
        {
            InitializeComponent();
            SetupWindow(loadPath);
        }

        /// <summary>
        /// Main function that controls setting everything up
        /// </summary>
        private void SetupWindow(string loadPath)
        {
            this.Hide();
            startingTimes = new List<double>();
            endingTimes = new List<double>();
            canGraph.AddDataset(ImportData(loadPath));
            mediaElement.Play();

        }

        /// <summary>
        /// Loads the selected path into the mediaElement and returns the accompanying data
        /// </summary>
        /// <param name="path">the path to load</param>
        /// <returns>the meta-data for when activities happen</returns>
        private GraphDataset ImportData(string path)
        {
            GraphDataset csvDataset = null;
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                File.Delete("tempvideo.mp4");
                File.Delete("tempfile.csv");
                bool good = true;
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string a = entry.FullName;
                    if (a == "output.csv")
                    {
                        entry.ExtractToFile("tempfile.csv");
                        csvDataset = CSVToDataset("tempfile.csv", "left", Brushes.SteelBlue);
                        File.Delete("tempfile.csv");
                    }
                    else if (entry.Name == "video.mp4" && good)
                    {
                        entry.ExtractToFile("tempvideo.mp4");
                        mediaElement.Source = new Uri("tempvideo.mp4", UriKind.Relative);
                    }
                }
                if (!good) return null;
            }

            return csvDataset;
        }

        /// <summary>
        /// converts a csv of probability,start,end,activity to a GraphDataset
        /// </summary>
        private GraphDataset CSVToDataset(string url, string name, Brush brush)
        {
            List<string[]> lines = File.ReadAllLines(url).Select(a => a.Split(',')).ToList();
            startingTimes = startingTimes.Concat(lines.Skip(1).ToList().Select(a => double.Parse(a.ElementAt(1)))).ToList();
            endingTimes = endingTimes.Concat(lines.Skip(1).ToList().Select(a => double.Parse(a.ElementAt(2)))).ToList();
            startingTimes.Sort();
            endingTimes.Sort();
            GraphDataset temp = new GraphDataset(name);
            foreach (string[] line in lines)
            {
                try
                {
                    double per = double.Parse(line[0]) * 100;
                    Brush col = PercentToProbabilityColour(per);
                    SummariserNode node = new SummariserNode(double.Parse(line[1]), double.Parse(line[2]), line[3], col);
                    temp.AddNode(node);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return temp;
        }

        ///// <summary>
        ///// Returns a screen-shot of the currently playing video in the supplied MediaElement at the supplied seconds
        ///// </summary>
        ///// <param name="currentSecond">the second to take the screen-shot at</param>
        ///// <param name="player">the MediaElement to screen-shot</param>
        ///// <returns>an image of the screen-shot</returns>
        //private Image GetScreenshotAtTime(int currentSecond, MediaElement player)
        //{
        //    RenderTargetBitmap rtb = new RenderTargetBitmap((int)player.Width, (int)player.Height, 96, 96, PixelFormats.Pbgra32);
        //    Image img = new Image();

        //    TimeSpan prePos = player.Position;
        //    if (videoState) player.Pause();
        //    player.Position = TimeSpan.FromSeconds(currentSecond);
        //    Thread.Sleep(SCREENSHOT_TIME);
        //    rtb.Render(player);
        //    img.Source = BitmapFrame.Create(rtb);

        //    player.Position = prePos;
        //    if (videoState) player.Play();
        //    return img;
        //}

        /// <summary>
        /// Plays or pauses the video depending on the current video state
        /// </summary>
        private void PausePlay()
        {
            switch (videoState)
            {
                case true:
                    mediaElement.Pause();
                    SetPausePlayImage(false);
                    videoState = false;
                    break;
                case false:
                    mediaElement.Play();
                    SetPausePlayImage(true);
                    videoState = true;
                    break;
            }
        }

        /// <summary>
        /// Sets the pausePlayImage to either pause(true) or play(false)
        /// </summary>
        /// <param name="pausePlay">the state of the button</param>
        private void SetPausePlayImage(bool pausePlay)
        {
            Uri uriSource = null;
            if (pausePlay) uriSource = new Uri(@"/CapstoneLayoutTest;component/Images/ic_pause_white_24dp.png", UriKind.Relative);
            else uriSource = new Uri(@"/CapstoneLayoutTest;component/Images/ic_play_arrow_white_24dp.png", UriKind.Relative);
            pausePlayImage.Source = new BitmapImage(uriSource);
        }

        /// <summary>
        /// Converts an int to a string that represents [HH:]MM:SS
        /// </summary>
        /// <param name="seconds">the seconds to convert</param>
        /// <returns>a string representing [HH:]MM:SS</returns>
        private string IntToTimeString(int seconds)
        {
            string builder = "";
            if (seconds >= 3600) builder += seconds / 3600 + ":";
            if (seconds % 3600 > 60)
            {
                int min = seconds % 3600 / 60;
                if (min < 10) builder += "0";
                builder += min + ":";
            }
            else builder += "00:";

            int sec = seconds % 60;
            if (sec < 10) builder += "0";
            builder += sec;
            return builder;
        }

        /// <summary>
        /// opens up a load window and loads the result into the GUI
        /// </summary>
        private void LoadNewData()
        {
            Load load = new Load();
            load.ShowDialog();
            if ((bool)load.DialogResult)
            {
                startingTimes = new List<double>();
                endingTimes = new List<double>();

                canGraph.ClearDatasets();
                if (HideControlsThread != null) HideControlsThread.CancelAsync();
                if (VideoProgressThread != null) VideoProgressThread.CancelAsync();
                if (ShowControllsThread != null) ShowControllsThread.CancelAsync();

                Thread.Sleep(25);
                mediaElement.Stop();
                mediaElement.Source = null;

                Thread.Sleep(25);
                GraphDataset data = ImportData(load.OkResult);
                if (data == null)
                {
                    MessageBox.Show("invalid data");
                    return;
                }
                Thread.Sleep(25);
                canGraph.AddDataset(data);
                mediaElement.Play();
            }
        }


        /// <summary>
        /// Moves the video to the position of the selected slider and updates the other slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoSliderbarMove(object sender, MouseEventArgs e)
        {
            if (videoState) { mediaElement.Pause(); }
            mediaElement.Position = TimeSpan.FromSeconds(((Slider)sender).Maximum * (1.0d / ((Slider)sender).ActualWidth * e.GetPosition((Slider)sender).X));
            playerSlider.Value = ((Slider)sender).Maximum * (1.0d / ((Slider)sender).ActualWidth * e.GetPosition((Slider)sender).X);
            graphSlider.Value = ((Slider)sender).Maximum * (1.0d / ((Slider)sender).ActualWidth * e.GetPosition((Slider)sender).X);
            if (videoState) { mediaElement.Play(); }
            //colorRectangle.Fill = PercentToProbabilityColour(100 * (playerSlider.Value / playerSlider.Maximum));
        }

        private static Brush PercentToProbabilityColour(double percent)
        {
            byte red = (byte)((percent > 51) ? 255 * (1 - 2 * (percent - 50)) / 100 : 255);
            byte green = (byte)((percent > 50) ? 255 : 255 * (2 * percent / 100));
            byte blue = 0;
            Brush color = new SolidColorBrush(Color.FromRgb(red, green, blue));
            return color;
        }

        private void setPositionInSeconds(double time)
        {
            mediaElement.Position = TimeSpan.FromSeconds(time);

            playerSlider.Value = time;

            graphSlider.Value = time;
        }

        /// <summary>
        /// Holds the code that is shared between the left and right button presses
        /// </summary>
        /// <param name="result">whether to round up or down to the next data point</param>
        private void ProcessLeftRightKeys(Func<int, double, double, double> result)
        {
            int index = startingTimes.BinarySearch(mediaElement.Position.TotalSeconds);
            if (index < 0) index = (~index) - 1;
            double time = startingTimes[index];
            time = result(index, time, mediaElement.Position.TotalSeconds);
            setPositionInSeconds(time);
        }

        /// <summary>
        /// Called when the right key is pressed, moves the video to the next data point
        /// </summary>
        private void ProcessRightKey()
        {
            var result = new Func<int, double, double, double>((index, time, current) =>
            {
                if (time <= current && index + 1 < startingTimes.Count) time = startingTimes[index + 1];
                return time;
            });
            ProcessLeftRightKeys(result);
        }

        /// <summary>
        /// Called when the left key is pressed, moves the video to the previous data point
        /// </summary>
        private void ProcessLeftKey()
        {
            var result = new Func<int, double, double, double>((index, time, current) =>
            {
                if (endingTimes[index] >= current && index - 1 >= 0) time = startingTimes[index - 1];
                return time;
            });
            ProcessLeftRightKeys(result);
        }

        /// <summary>
        /// The second half of the window loading, called when the video loads
        /// </summary>
        private void SetupAfterVideoLoaded()
        {
            Width = 720;
            mediaElement.Position = new TimeSpan(0);
            WindowStyle = WindowStyle.SingleBorderWindow;
            ShowInTaskbar = true;
            ShowActivated = true;
            SizeToContent = SizeToContent.Height;
            playerSlider.Maximum = (int)mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            graphSlider.Maximum = (int)mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            VideoProgressThread = SetupBackgroundWorker(VideoProgressThread_DoWork, false);
            canGraph.DrawGraph(mediaElement.NaturalDuration.TimeSpan.TotalSeconds);
            graphSlider.Height = canGraph.Height;
            this.Show();
            this.Activate();
        }

        /// <summary>
        /// Sets up and runs a BackgroundWorker
        /// </summary>
        /// <param name="doWork">the function for it to run</param>
        /// <param name="cancelable">if it can be closed early</param>
        /// <returns>a BackgroundWorker based of the supplied constraints that is running</returns>
        private BackgroundWorker SetupBackgroundWorker(DoWorkEventHandler doWork, bool cancelable)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += doWork;
            worker.RunWorkerAsync();
            return worker;
        }

        //Background workers
        /// <summary>
        /// USed to update the scrollbar progress as the video is playing
        /// </summary>
        /// <param name="sender">The parent thread</param>
        /// <param name="e">arguments</param>
        private void VideoProgressThread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!VideoProgressThread.CancellationPending)
            {
                int currentTime = 0; int totalTime = 0;
                if (videoState && !VideoProgressThread.CancellationPending) Dispatcher.Invoke(() =>
                {
                    if (VideoProgressThread.CancellationPending) return;
                    currentTime = (int)mediaElement.Position.TotalSeconds;
                    totalTime = (int)mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                });
                string timeString = IntToTimeString(currentTime) + "/" + IntToTimeString(totalTime);
                if (videoState && !VideoProgressThread.CancellationPending) Dispatcher.Invoke(() =>
                {
                    VideoTime.Content = timeString;
                    playerSlider.Value = mediaElement.Position.TotalSeconds;
                    graphSlider.Value = mediaElement.Position.TotalSeconds;
                });

                Thread.Sleep(PROGRESS_BAR_UPDATE_SPEED);
            }
            e.Cancel = true;
            return;
        }

        /// <summary>
        /// Shows the video controls
        /// </summary>
        /// <param name="sender">The parent thread</param>
        /// <param name="e">arguments</param>
        private void ShowControllsThread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            bool run = false;
            if (!ShowControllsThread.CancellationPending) Dispatcher.Invoke(() => run = ControlPanel.Height < CONTROLS_MAX_HEIGHT);
            while (run)
            {
                if (ShowControllsThread.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (!ShowControllsThread.CancellationPending) Dispatcher.Invoke(() =>
                {
                    ControlPanel.Height++;
                    ControlGrid.Height++;
                });
                Thread.Sleep(CONTROLS_SHOW_SPEED);
                if (!ShowControllsThread.CancellationPending) Dispatcher.Invoke(() => run = ControlPanel.Height < CONTROLS_MAX_HEIGHT);
            }
        }

        /// <summary>
        /// hides the video controls
        /// </summary>
        /// <param name="sender">The parent thread</param>
        /// <param name="e">arguments</param>
        private void HideControlsThread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Thread.Sleep(CONTROLS_HIDE_DELAY);
            bool run = false;
            if (!HideControlsThread.CancellationPending) Dispatcher.Invoke(() => run = ControlPanel.Height > CONTROLS_MIN_HEIGHT);
            while (run)
            {
                if (HideControlsThread.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (ControlPanel.Height - 1 > 0)
                        {
                            ControlPanel.Height--;
                            ControlGrid.Height--;
                        }
                        else { run = false; }
                    });
                }
                Thread.Sleep(CONTROLS_HIDE_SPEED);
                if (!HideControlsThread.CancellationPending) Dispatcher.Invoke(() => run = ControlPanel.Height > CONTROLS_MIN_HEIGHT);
            }
        }

        //Event Handlers
        private void mediaElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PausePlay();
        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            SetupAfterVideoLoaded();

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (HideControlsThread != null) HideControlsThread.CancelAsync();
            if (VideoProgressThread != null) VideoProgressThread.CancelAsync();
            if (ShowControllsThread != null) ShowControllsThread.CancelAsync();
        }
        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            UploadWindow upload = new UploadWindow();
            upload.ShowDialog();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            LoadNewData();

        }

        private void scrollBar_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Position = TimeSpan.FromSeconds(((Slider)sender).Value);

        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (HideControlsThread != null) HideControlsThread.CancelAsync();
            ShowControllsThread = SetupBackgroundWorker(ShowControllsThread_DoWork, true);
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ShowControllsThread != null) ShowControllsThread.CancelAsync();
            HideControlsThread = SetupBackgroundWorker(HideControlsThread_DoWork, true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PausePlay();
        }

        private void scrollBar2_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                VideoSliderbarMove(sender, e);

        }

        private void scrollBar2_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            VideoSliderbarMove(sender, e);

        }

        private void scrollBar2_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) PausePlay();
            if (e.Key == Key.Left) ProcessLeftKey();
            if (e.Key == Key.Right) ProcessRightKey();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            int num = canGraph.NumOfDatasets;
            if (Width > 29 && num > 0 && mediaElement.ActualWidth > 12)
            {
                //change to be equal to video width and centered\\

                canGraph.Width = mediaElement.ActualWidth - 11;
                //SizeToContent = SizeToContent.Height;
                canGraph.DrawGraph(mediaElement.NaturalDuration.TimeSpan.TotalSeconds);
            }

        }

    }
    public class InlineCanGraph : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value) - 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }

    }
    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness(0, 20, 0, (double)value + 50);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }

    }
    public class MarginConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness(0, 0, (double)value - 40, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }

    }
    public class MarginConverter3 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness(0, 0, (double)value + 60, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }

    }
}
//123/9