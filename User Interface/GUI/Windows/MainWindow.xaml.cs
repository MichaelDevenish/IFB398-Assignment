﻿using DataGraph;
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
using CapstoneLayoutTest.Helper_Functions;

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
                        csvDataset = CSVToDataset("tempfile.csv", "left");
                        File.Delete("tempfile.csv");
                        if (csvDataset == null) return null;
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
        private GraphDataset CSVToDataset(string url, string name)
        {
            try
            {
                CSVDatasetLoader loader = new CSVDatasetLoader(url);
                startingTimes = loader.AppendDataToSortedList(1, startingTimes);
                endingTimes = loader.AppendDataToSortedList(2, endingTimes);
                return loader.GenerateDataset(name);
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException)
                {
                    MessageBox.Show("File does not exist");
                }
                else if (e is IOException)
                {
                    MessageBox.Show("File is currently being used by another process");
                }
            }
            return null;
        }

        /// <summary>
        /// Plays or pauses the video depending on the current video state
        /// </summary>
        private void PausePlay()
        {
            switch (videoState)
            {
                case true:
                    mediaElement.Pause();
                    ControlBarHelper.SetPausePlayImage(false, pausePlayImage);
                    videoState = false;
                    break;
                case false:
                    mediaElement.Play();
                    ControlBarHelper.SetPausePlayImage(true, pausePlayImage);
                    videoState = true;
                    break;
            }
        }

        /// <summary>
        ///loads the selected file into the GUI
        /// </summary>
        /// <param name="result">the file to load</param>
        public void LoadNewData(string result)
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
            GraphDataset data = ImportData(result);
            if (data == null)
            {
                MessageBox.Show("invalid data");
                return;
            }
            Thread.Sleep(25);
            canGraph.AddDataset(data);
            mediaElement.Play();

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
        }

        /// <summary>
        /// Sets all of the sliders positions in seconds
        /// </summary>
        /// <param name="time">the time to set</param>
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

        #region Background workers
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
                string timeString = ControlBarHelper.IntToTimeString(currentTime) + "/" + ControlBarHelper.IntToTimeString(totalTime);
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
        #endregion

        #region Event Handlers
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
            try
            {
                UploadWindow upload = new UploadWindow();
                upload.Owner = Window.GetWindow(this);
                upload.Show();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Model must be installed to upload data");
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            Load load = new Load();
            load.Owner = Window.GetWindow(this);
            load.ShowDialog();
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
            if (e.LeftButton == MouseButtonState.Pressed) VideoSliderbarMove(sender, e);
        }

        private void scrollBar2_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            VideoSliderbarMove(sender, e);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    PausePlay();
                    break;
                case Key.Left:
                    ProcessLeftKey();
                    break;
                case Key.Right:
                    ProcessRightKey();
                    break;
                case Key.Home:
                    mediaElement.Position = TimeSpan.FromSeconds(0);
                    break;
                case Key.End:
                    mediaElement.Position = TimeSpan.FromSeconds(mediaElement.NaturalDuration.TimeSpan.TotalSeconds);
                    break;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int num = canGraph.NumOfDatasets;
            if (Width > 29 && num > 0 && mediaElement.ActualWidth > 12)
            {
                canGraph.Width = mediaElement.ActualWidth - 11;
                canGraph.DrawGraph(mediaElement.NaturalDuration.TimeSpan.TotalSeconds);
            }
        }
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("./User Guide.pdf"))
                System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "/User Guide.pdf");
            else MessageBox.Show("User Guide could not be found");
        }
        #endregion
    }

    #region converters
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
    #endregion
}