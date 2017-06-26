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

        const double XMIN = 60;
        const double YMIN = 110;

        private bool videoState = true;
        private bool currentlyRenderingPopup = false;
        private bool running = false;
        BackgroundWorker backgroundWorker1;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double xmax = canGraph.Width;
            double ymax = canGraph.Height - YMIN;
            double xdatamax = 20; double ydatamax = 60;

            double[,] leftArray = { { 0, 0 }, { 1, 20 }, { 2, 20 }, { 3, 10 }, { 4, 10 }, { 5, 30 }, { 6, 40 }, { 7, 50 }, { 8, 50 }, { 9, 60 }, { 10, 50 }, { 11, 50 }, { 12, 20 }, { 13, 20 }, { 14, 50 }, { 15, 50 }, { 16, 50 }, { 17, 40 }, { 18, 50 }, { 19, 60 }, { 20, 61 } };
            double[,] rightArray = { { 0, 0 }, { 1, 20 }, { 2, 20 }, { 3, 10 }, { 4, 20 }, { 5, 30 }, { 6, 40 }, { 7, 50 }, { 8, 50 }, { 9, 50 }, { 10, 50 }, { 11, 50 }, { 12, 20 }, { 13, 20 }, { 14, 50 }, { 15, 60 }, { 16, 50 }, { 17, 40 }, { 18, 50 }, { 19, 60 }, { 20, 60 } };
            GraphDataset left = BuildDataset("left", leftArray, Brushes.SteelBlue);//new GraphDataset("left", Brushes.SteelBlue);
            GraphDataset right = BuildDataset("right", rightArray, Brushes.Orange);

            Brush[] brushes = { Brushes.SteelBlue, Brushes.Orange };

            running = true;
            mediaElement.Play();
            canGraph.AddDataset(left);
            canGraph.AddDataset(right);
            canGraph.XAxisName = "Minutes";
            canGraph.YAxisName = "Moves";
            canGraph.XDivisor = 1;
            canGraph.YDivisor = 10;
            canGraph.DrawGraph();
            DrawDataPoints(xdatamax, ydatamax, leftArray, brushes[0]);
            DrawDataPoints(xdatamax, ydatamax, rightArray, brushes[1]);
        }


        private GraphDataset BuildDataset(string name, double[,] data, Brush brush)
        {
            GraphDataset temp = new GraphDataset(name, brush);
            for (int i = 0; i <= data.GetUpperBound(0); i++)
            {
                GraphNode node = new GraphNode(data[i, 0], data[i, 1], "test");
                temp.AddNode(node);
            }
            return temp;
        }

        private void DrawDataPoints(double xdatamax, double ydatamax, double[,] pointsArray, Brush brush)
        {
            List<Button> buttons = new List<Button>();
            PointCollection points = new PointCollection();
            for (int a = 0; a <= pointsArray.GetUpperBound(0); a++)
            {
                double xpoint = (((canGraph.Width - XMIN) / xdatamax) * (pointsArray[a, 0])) + XMIN;
                double ypoint = (canGraph.Height - YMIN) - (((canGraph.Height - YMIN) / ydatamax) * (pointsArray[a, 1]));
                points.Add(new Point(xpoint, ypoint));
                CreateGraphPoint(buttons, xpoint, ypoint, brush, a);
            }

            Polyline polyline = GeneratePolyline(brush, points);
            canGraph.Children.Add(polyline);
            foreach (Button b in buttons) canGraph.Children.Add(b);

        }


        private void CreateGraphPoint(List<Button> buttons, double xpoint, double ypoint, Brush br, int currentMinute)
        {
            Button b = new Button();
            b.Width = 10; b.Height = 10;
            b.Style = (Style)(Resources["CircleButton"]);
            b.Background = br;

            b.MouseEnter += new MouseEventHandler((object subSender, MouseEventArgs subE) =>
           {
               if (b.ToolTip == null && !currentlyRenderingPopup)
               {
                   currentlyRenderingPopup = true;
                   ToolTipService.SetToolTip(b, GetScreenshotAtTime(currentMinute));
                   currentlyRenderingPopup = false;
               }
           });

            b.Click += new RoutedEventHandler((object subSender, RoutedEventArgs subE) =>
            {
                mediaElement.Pause();
                mediaElement.Position = TimeSpan.FromSeconds(currentMinute);
                Thread.Sleep(50);
                mediaElement.Play();
            });

            Canvas.SetLeft(b, xpoint - 5);
            Canvas.SetTop(b, ypoint - 5);
            buttons.Add(b);
        }

        private Image GetScreenshotAtTime(int currentMinute)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)mediaElement.Width, (int)mediaElement.Height, 96, 96, PixelFormats.Pbgra32);
            Image img = new Image();

            TimeSpan prePos = mediaElement.Position;
            if (videoState) mediaElement.Pause();
            mediaElement.Position = TimeSpan.FromSeconds(currentMinute);
            Thread.Sleep(200);
            rtb.Render(mediaElement);
            img.Source = BitmapFrame.Create(rtb);

            mediaElement.Position = prePos;
            if (videoState) mediaElement.Play();
            return img;
        }

        private static Polyline GeneratePolyline(Brush brush, PointCollection points)
        {
            Polyline polyline = new Polyline();
            polyline.StrokeThickness = 3;
            polyline.Stroke = brush;
            polyline.Points = points;
            return polyline;

        }

        private void mediaElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (videoState)
            {
                mediaElement.Pause();
                videoState = false;
            }
            else
            {
                mediaElement.Play();
                videoState = true;
            }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (running)
            {
                Dispatcher.Invoke(() => scrollBar.Value = mediaElement.Position.TotalSeconds);
                Thread.Sleep(15);
            }
            e.Cancel = true;
            return;
        }
        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            scrollBar.Maximum = (int)mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerAsync();
        }

        private void scrollBar_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            mediaElement.Position = TimeSpan.FromSeconds(((ScrollBar)sender).Value);
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
    }
}


//Movements object
//Left List (side obj)
//right list (side obj)
//functions to get x and y max

//Side object
//array of points objects