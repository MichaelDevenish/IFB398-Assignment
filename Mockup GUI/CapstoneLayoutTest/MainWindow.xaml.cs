using DataGraph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CapstoneLayoutTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private const int CONTROLS_HIDE_SPEED = 10;
        private const int CONTROLS_SHOW_SPEED = 2;
        private const int CONTROLS_MIN_HEIGHT = 0;
        private const int CONTROLS_MAX_HEIGHT = 25;
        private const int CONTROLS_HIDE_DELAY = 1000;
        private const int PROGRESS_BAR_UPDATE_SPEED = 15;
        private const int SCREENSHOT_TIME = 150;
        private bool videoState = true;
        private bool currentlyRenderingPopup = false;
        private bool running = false;
        BackgroundWorker HideControlsThread;
        BackgroundWorker VideoProgressThread;
        BackgroundWorker ShowControllsThread;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //dummy
            double[,] leftArray = { { 0, 0 }, { 1, 20 }, { 2, 20 }, { 3, 10 }, { 4, 10 }, { 5, 30 }, { 6, 40 }, { 7, 50 }, { 8, 50 }, { 9, 60 }, { 10, 50 }, { 11, 50 }, { 12, 20 }, { 13, 20 }, { 14, 50 }, { 15, 50 }, { 16, 50 }, { 17, 40 }, { 18, 50 }, { 19, 60 }, { 20, 60 } };
            double[,] rightArray = { { 0, 0 }, { 1, 20 }, { 2, 20 }, { 3, 10 }, { 4, 20 }, { 5, 30 }, { 6, 40 }, { 7, 50 }, { 8, 50 }, { 9, 50 }, { 10, 50 }, { 11, 50 }, { 12, 20 }, { 13, 20 }, { 14, 50 }, { 15, 60 }, { 16, 50 }, { 17, 40 }, { 18, 50 }, { 19, 60 }, { 20, 60 } };
            double[,] testArray = { { 0, 1 }, { 1, 21 }, { 2, 21 }, { 3, 11 }, { 4, 21 }, { 5, 31 }, { 6, 41 }, { 7, 51 }, { 8, 51 }, { 9, 51 }, { 10, 51 }, { 11, 51 }, { 12, 21 }, { 13, 21 }, { 14, 51 }, { 15, 61 }, { 16, 51 }, { 17, 41 }, { 18, 51 }, { 19, 61 }, { 20, 61 } };


            GraphDataset left = BuildDataset("left", leftArray, Brushes.SteelBlue, 0);
            GraphDataset right = BuildDataset("right", rightArray, Brushes.Orange, 1);
            GraphDataset test = BuildDataset("test", testArray, Brushes.Tan, 0);

            //dummy

            running = true;
            mediaElement.Play();

            canGraph.AddDataset(left);
            canGraph.AddDataset(right);
            //canGraph.AddDataset(test);
            canGraph.XAxisName = "Minutes";
            canGraph.YAxisName = "Moves";
            canGraph.XDivisor = 1;
            canGraph.YDivisor = 5;
            canGraph.DrawGraph();
        }

        //dummy
        private GraphDataset BuildDataset(string name, double[,] data, Brush brush, int inc)
        {
            string[] datatypes = { "walking", "running", "sprinting", "jogging", "skipping", "test" };
            GraphDataset temp = new GraphDataset(name, brush);
            for (int i = 0; i <= data.GetUpperBound(0); i++)
            {
                GraphNode node = new GraphNode(data[i, 0], data[i, 1], datatypes[i % 5 + inc]);
                node.AddButtonHover(HoverButtonHandeler(node));
                node.AddButtonClick(ClickButtonHandeler(node));
                temp.AddNode(node);
            }
            return temp;
        }
        //dummy

        private RoutedEventHandler ClickButtonHandeler(GraphNode node)
        {
            return new RoutedEventHandler((object subSender, RoutedEventArgs subE) =>
            {
                mediaElement.Pause();
                mediaElement.Position = TimeSpan.FromSeconds((int)node.GetCoords()[0]);
                mediaElement.Play();
            });
        }

        private MouseEventHandler HoverButtonHandeler(GraphNode node)
        {
            return new MouseEventHandler((object subSender, MouseEventArgs subE) =>
            {
                if (node.NodeButton.ToolTip == null && !currentlyRenderingPopup)
                {
                    currentlyRenderingPopup = true;
                    ToolTipService.SetToolTip(node.NodeButton, GetScreenshotAtTime((int)node.GetCoords()[0]));
                    currentlyRenderingPopup = false;
                }
            });
        }

        private Image GetScreenshotAtTime(int currentMinute)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)mediaElement.Width, (int)mediaElement.Height, 96, 96, PixelFormats.Pbgra32);
            Image img = new Image();

            TimeSpan prePos = mediaElement.Position;
            if (videoState) mediaElement.Pause();
            mediaElement.Position = TimeSpan.FromSeconds(currentMinute);
            Thread.Sleep(SCREENSHOT_TIME);
            rtb.Render(mediaElement);
            img.Source = BitmapFrame.Create(rtb);

            mediaElement.Position = prePos;
            if (videoState) mediaElement.Play();
            return img;
        }


        private void mediaElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PausePlay();
        }

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

        private void SetPausePlayImage(bool pausePlay)
        {
            Uri uriSource = null;
            if (pausePlay) uriSource = new Uri(@"/CapstoneLayoutTest;component/Images/ic_pause_white_24dp.png", UriKind.Relative);
            else uriSource = new Uri(@"/CapstoneLayoutTest;component/Images/ic_play_arrow_white_24dp.png", UriKind.Relative);
            pausePlayImage.Source = new BitmapImage(uriSource);
        }

        private BackgroundWorker SetupBackgroundWorker(DoWorkEventHandler doWork, bool cancelable)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = cancelable;
            worker.DoWork += doWork;
            worker.RunWorkerAsync();
            return worker;
        }

        //Event Handlers
        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            scrollBar.Maximum = (int)mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            VideoProgressThread = SetupBackgroundWorker(VideoProgressThread_DoWork, false);
        }

        private void scrollBar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (videoState) mediaElement.Play();
        }

        private void scrollBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            mediaElement.Pause();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            running = false;
        }
        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            Upload upload = new Upload();
            upload.ShowDialog();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            Load load = new Load();
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

        //Background workers
        private void VideoProgressThread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (running)
            {
                Dispatcher.Invoke(() => scrollBar.Value = mediaElement.Position.TotalSeconds);
                Thread.Sleep(PROGRESS_BAR_UPDATE_SPEED);
            }
            e.Cancel = true;
            return;
        }

        private void ShowControllsThread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            bool run = false;
            Dispatcher.Invoke(() => run = ControlPanel.Height < CONTROLS_MAX_HEIGHT);
            while (run)
            {
                if (ShowControllsThread.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                Dispatcher.Invoke(() =>
                {
                    ControlPanel.Height++;
                    ControlGrid.Height++;
                });
                Thread.Sleep(CONTROLS_SHOW_SPEED);
                Dispatcher.Invoke(() => run = ControlPanel.Height < CONTROLS_MAX_HEIGHT);
            } while (run) ;
            e.Cancel = true;
            return;
        }

        private void HideControlsThread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Thread.Sleep(CONTROLS_HIDE_DELAY);
            bool run = false;
            Dispatcher.Invoke(() => run = ControlPanel.Height > CONTROLS_MIN_HEIGHT);
            while (run)
            {
                if (HideControlsThread.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                Dispatcher.Invoke(() =>
                {
                    ControlPanel.Height--;
                    ControlGrid.Height--;
                });
                Thread.Sleep(CONTROLS_HIDE_SPEED);
                Dispatcher.Invoke(() => run = ControlPanel.Height > CONTROLS_MIN_HEIGHT);
            }
            e.Cancel = true;
            return;
        }
    }
}
