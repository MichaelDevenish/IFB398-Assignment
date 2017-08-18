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
using System.IO;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        public MainWindow()
        {
            InitializeComponent();
            SizeToContent = SizeToContent.Height;
            SetupWindow();
        }

        /// <summary>
        /// Main function that controls setting everything up
        /// </summary>
        private void SetupWindow()
        {


            double[,] leftArray = { { 0, 0 }, { 1, 20 }, { 2, 20 }, { 3, 10 }, { 4, 10 }, { 5, 30 }, { 6, 40 }, { 7, 50 }, { 8, 50 }, { 9, 60 }, { 10, 50 }, { 11, 50 }, { 12, 20 }, { 13, 20 }, { 14, 50 }, { 15, 50 }, { 16, 50 }, { 17, 40 }, { 18, 50 }, { 19, 60 }, { 20, 60 } };
            double[,] leftArray2 = { { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 4 }, { 4, 5 }, { 5, 6 }, { 6, 7 }, { 7, 8 }, { 8, 9 }, { 9, 10 }, { 10, 11 }, { 11, 12 }, { 12.5, 13 }, { 13, 14 }, { 14, 15 }, { 15, 16 }, { 16, 17 }, { 17, 18 }, { 18, 19 }, { 19, 20 } };
            double[,] rightArray = { { 0, 0 }, { 1, 20 }, { 2, 20 }, { 3, 10 }, { 4, 20 }, { 5, 30 }, { 6, 40 }, { 7, 50 }, { 8, 50 }, { 9, 50 }, { 10, 50 }, { 11, 50 }, { 12, 20 }, { 13, 20 }, { 14, 50 }, { 15, 60 }, { 16, 50 }, { 17, 40 }, { 18, 50 }, { 19, 60 }, { 20, 60 } };
            double[,] testArray = { { 0, 1 }, { 1, 21 }, { 2, 21 }, { 3, 11 }, { 4, 21 }, { 5, 31 }, { 6, 41 }, { 7, 51 }, { 8, 51 }, { 9, 51 }, { 10, 51 }, { 11, 51 }, { 12, 21 }, { 13, 21 }, { 14, 51 }, { 15, 61 }, { 16, 51 }, { 17, 41 }, { 18, 51 }, { 19, 61 }, { 20, 61 } };

            startingTimes = new List<double>();
            endingTimes = new List<double>();
            GraphDataset left = BuildDataset2("left", leftArray2, Brushes.SteelBlue, 0);
            GraphDataset right = BuildDataset("right", rightArray, Brushes.Orange, 1);
            GraphDataset test = BuildDataset("test", testArray, Brushes.Tan, 0);
            string path = "..\\..\\test.zip";

            // GraphDataset csvDataset = CSVToDataset("..\\..\\output.csv", "left", Brushes.SteelBlue);
            //dummy

            canGraph.AddDataset(ImportData(path));
            mediaElement.Play();
            //canGraph.AddDataset(right);

            //canGraph.AddDataset(test);
            //canGraph.XAxisName = "Minutes";
            //canGraph.YAxisName = "Moves";
            //canGraph.XDivisor = 1;
            //canGraph.YDivisor = 5;
            // graphSlider.Width = canGraph.SummariserWidth;
        }

        private GraphDataset ImportData(string path)
        {
            GraphDataset csvDataset = null;
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                File.Delete("tempvideo.mp4");
                File.Delete("tempfile.csv");
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string a = entry.FullName;
                    if (a == "output.csv")
                    {
                        entry.ExtractToFile("tempfile.csv");
                        csvDataset = CSVToDataset("tempfile.csv", "left", Brushes.SteelBlue);
                        File.Delete("tempfile.csv");
                    }
                    else if (entry.Name == "video.mp4")
                    {
                        entry.ExtractToFile("tempvideo.mp4");
                        mediaElement.Source = new Uri("tempvideo.mp4", UriKind.Relative);
                    }
                }
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
            GraphDataset temp = new GraphDataset(name, brush);
            foreach (string[] line in lines)
            {
                try
                {
                    SummariserNode node = new SummariserNode(double.Parse(line[1]), double.Parse(line[2]), line[3]);
                    temp.AddNode(node);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return temp;
        }

        //dummy
        private GraphDataset BuildDataset(string name, double[,] data, Brush brush, int inc)
        {
            string[] datatypes = { "walking", "running", "sprinting", "jogging", "skipping", "test", "sgfesa", "dsafaef", "feafasdf", "a", "s", "d", "f", "g", "h", "j" };
            GraphDataset temp = new GraphDataset(name, brush);
            for (int i = 0; i <= data.GetUpperBound(0); i++)
            {
                GraphNode node = new GraphNode(data[i, 0], data[i, 1], datatypes[i % 3]);
                //node.AddButtonHover(HoverButtonHandeler(node));
                //node.AddButtonClick(ClickButtonHandeler(node));
                temp.AddNode(node);
            }
            return temp;
        }
        private GraphDataset BuildDataset2(string name, double[,] data, Brush brush, int inc)
        {
            string[] datatypes = { "walking", "running", "sprinting", "jogging", "skipping", "test", "sgfesa", "dsafaef", "feafasdf", "a", "s", "d", "f", "g", "h", "j" };
            GraphDataset temp = new GraphDataset(name, brush);
            for (int i = 0; i <= data.GetUpperBound(0); i++)
            {
                SummariserNode node = new SummariserNode(data[i, 0], data[i, 1], datatypes[i % 11]);
                //node.AddButtonHover(HoverButtonHandeler(node));
                //node.AddButtonClick(ClickButtonHandeler(node));
                temp.AddNode(node);
            }
            return temp;
        }
        //dummy

        /// <summary>
        /// Returns an event handler for the supplied node that handles clicking on the button 
        /// </summary>
        /// <param name="node">the node the handler relates to</param>
        /// <returns>a click event relating to the supplied node</returns>
        //private RoutedEventHandler ClickButtonHandeler(GraphNode node)
        //{
        //    return new RoutedEventHandler((object subSender, RoutedEventArgs subE) =>
        //    {
        //        mediaElement.Pause();
        //        mediaElement.Position = TimeSpan.FromSeconds((int)node.GetCoords()[0]);
        //        mediaElement.Play();
        //    });
        //}

        /// <summary>
        /// Returns an event handler for the supplied node that handles hovering over the button 
        /// </summary>
        /// <param name="node">the node the handler relates to</param>
        /// <returns>a mouse event relating to the supplied node</returns>
        //private MouseEventHandler HoverButtonHandeler(GraphNode node)
        //{
        //    return new MouseEventHandler((object subSender, MouseEventArgs subE) =>
        //    {
        //        if (node.NodeButton.ToolTip == null && !currentlyRenderingPopup)
        //        {
        //            currentlyRenderingPopup = true;
        //            ToolTipService.SetToolTip(node.NodeButton, GetScreenshotAtTime((int)node.GetCoords()[0], mediaElement));
        //            currentlyRenderingPopup = false;
        //        }
        //    });
        //}

        /// <summary>
        /// Returns a screen-shot of the currently playing video in the supplied MediaElement at the supplied seconds
        /// </summary>
        /// <param name="currentSecond">the second to take the screen-shot at</param>
        /// <param name="player">the MediaElement to screen-shot</param>
        /// <returns>an image of the screen-shot</returns>
        private Image GetScreenshotAtTime(int currentSecond, MediaElement player)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)player.Width, (int)player.Height, 96, 96, PixelFormats.Pbgra32);
            Image img = new Image();

            TimeSpan prePos = player.Position;
            if (videoState) player.Pause();
            player.Position = TimeSpan.FromSeconds(currentSecond);
            Thread.Sleep(SCREENSHOT_TIME);
            rtb.Render(player);
            img.Source = BitmapFrame.Create(rtb);

            player.Position = prePos;
            if (videoState) player.Play();
            return img;
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

        private void LoadNewData(Load load)
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
            string output = load.OkResult;
            canGraph.AddDataset(ImportData(output));
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
            //if ((Slider)sender == graphSlider)
            playerSlider.Value = ((Slider)sender).Maximum * (1.0d / ((Slider)sender).ActualWidth * e.GetPosition((Slider)sender).X);
            //if ((Slider)sender == playerSlider)
            graphSlider.Value = ((Slider)sender).Maximum * (1.0d / ((Slider)sender).ActualWidth * e.GetPosition((Slider)sender).X);
            if (videoState) { mediaElement.Play(); }
        }

        private void setPositionInSeconds(double time)
        {
            mediaElement.Position = TimeSpan.FromSeconds(time);
            playerSlider.Value = time;
            graphSlider.Value = time;
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
                        ControlPanel.Height--;
                        ControlGrid.Height--;
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
            playerSlider.Maximum = (int)mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            graphSlider.Maximum = (int)mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            VideoProgressThread = SetupBackgroundWorker(VideoProgressThread_DoWork, false);
            canGraph.DrawGraph(mediaElement.NaturalDuration.TimeSpan.TotalSeconds);
            graphSlider.Height = canGraph.Height;

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
            Load load = new Load();
            load.ShowDialog();
            if ((bool)load.DialogResult)
            {
                LoadNewData(load);

            }
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


            if (e.Key == Key.Space)
            {
                PausePlay();
            }
            if (e.Key == Key.Left)
            {
                int index = startingTimes.BinarySearch(mediaElement.Position.TotalSeconds);
                if (index < 0) index = (~index) - 1;
                double time = startingTimes[index];
                double current = mediaElement.Position.TotalSeconds;
                if (endingTimes[index] >= current && index - 1 >= 0) time = startingTimes[index - 1];
                setPositionInSeconds(time);
            }
            if (e.Key == Key.Right)
            {
                int index = startingTimes.BinarySearch(mediaElement.Position.TotalSeconds);
                if (index < 0) index = (~index) - 1;
                double time = startingTimes[index];
                double current = mediaElement.Position.TotalSeconds;
                if (time <= current && index + 1 < startingTimes.Count) time = startingTimes[index + 1];
                setPositionInSeconds(time);
            }
        }

    }
}
