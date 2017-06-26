﻿using DataGraph;
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

        private bool videoState = true;
        private bool currentlyRenderingPopup = false;
        private bool running = false;
        BackgroundWorker backgroundWorker1;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double[,] leftArray = { { 0, 0 }, { 1, 20 }, { 2, 20 }, { 3, 10 }, { 4, 10 }, { 5, 30 }, { 6, 40 }, { 7, 50 }, { 8, 50 }, { 9, 60 }, { 10, 50 }, { 11, 50 }, { 12, 20 }, { 13, 20 }, { 14, 50 }, { 15, 50 }, { 16, 50 }, { 17, 40 }, { 18, 50 }, { 19, 60 }, { 20, 60 } };
            double[,] rightArray = { { 0, 0 }, { 1, 20 }, { 2, 20 }, { 3, 10 }, { 4, 20 }, { 5, 30 }, { 6, 40 }, { 7, 50 }, { 8, 50 }, { 9, 50 }, { 10, 50 }, { 11, 50 }, { 12, 20 }, { 13, 20 }, { 14, 50 }, { 15, 60 }, { 16, 50 }, { 17, 40 }, { 18, 50 }, { 19, 60 }, { 20, 60 } };

            GraphDataset left = BuildDataset("left", leftArray, Brushes.SteelBlue);
            GraphDataset right = BuildDataset("right", rightArray, Brushes.Orange);

            running = true;
            mediaElement.Play();

            canGraph.AddDataset(left);
            canGraph.AddDataset(right);
            canGraph.XAxisName = "Minutes";
            canGraph.YAxisName = "Moves";
            canGraph.XDivisor = 1;
            canGraph.YDivisor = 10;
            canGraph.DrawGraph();
        }


        private GraphDataset BuildDataset(string name, double[,] data, Brush brush)
        {
            GraphDataset temp = new GraphDataset(name, brush);
            for (int i = 0; i <= data.GetUpperBound(0); i++)
            {
                GraphNode node = new GraphNode(data[i, 0], data[i, 1], "test");
                node = SetupButtons(node);
                temp.AddNode(node);
            }
            return temp;
        }

        private GraphNode SetupButtons(GraphNode node)
        {

            node.AddButtonHover(new MouseEventHandler((object subSender, MouseEventArgs subE) =>
            {
                if (node.NodeButton.ToolTip == null && !currentlyRenderingPopup)
                {
                    currentlyRenderingPopup = true;
                    ToolTipService.SetToolTip(node.NodeButton, GetScreenshotAtTime((int)node.GetCoords()[0]));
                    currentlyRenderingPopup = false;
                }
            }));

            node.AddButtonClick(new RoutedEventHandler((object subSender, RoutedEventArgs subE) =>
            {
                mediaElement.Pause();
                mediaElement.Position = TimeSpan.FromSeconds((int)node.GetCoords()[0]);
                Thread.Sleep(50);
                mediaElement.Play();
            }));
            return node;
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
