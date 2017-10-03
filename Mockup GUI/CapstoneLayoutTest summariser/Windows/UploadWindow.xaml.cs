﻿using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CapstoneLayoutTest
{
    /// <summary>
    /// Interaction logic for UploadWindow.xaml
    /// </summary>
    public partial class UploadWindow : Window
    {
        private bool finalResult = false;
        public bool Result { get { return finalResult; } }
        private int windowMode = 0;
        private int loadBarPercentage = 0;
        private string path, root, vidFileName, vidPathName, newPath, newPathName, splitTime;
        private int vidNum, segNum;
        private BackgroundWorker demoCodeWorker1;
        private string user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public UploadWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            demoCodeWorker1 = new BackgroundWorker();
            demoCodeWorker1.DoWork += DemoCodeWorker1_DoWork;
            progressBar.Maximum = 100;
        }

        /// <summary>
        /// Call this function to load the data into the main window
        /// </summary>
        /// <param name="resultURI">the zip folder containing the data that is to be shown on the main window</param>
        public void LoadResult(string resultURI)
        {
            if (Owner.IsLoaded)
            {
                if (Owner is Splash)
                {
                    ((Splash)Owner).loadWindow(resultURI);
                    Close();
                }
                else if (Owner is MainWindow)
                {
                    ((MainWindow)Owner).LoadNewData(resultURI);
                    Close();
                }
            }
            else MessageBox.Show("Parent window has been closed, aborting load.");


        }


        private void SelectFile()
        {
            Microsoft.Win32.OpenFileDialog open = new Microsoft.Win32.OpenFileDialog();
            open.DefaultExt = ".mp4";
            open.Filter = "mpeg Files (*.mpeg)|*.mp4";

            if ((bool)open.ShowDialog())
            {
                vidPathName = open.FileName;
                SegmentVideo();
                textBox.Text = open.FileName;
            }
        }

        private void SegmentVideo()
        {
            user = user.Replace("\\", "/");
            vidPathName = vidPathName.Replace("\\", "/");
            splitTime = "10";
            vidFileName = System.IO.Path.GetFileName(vidPathName);
            string originPath = vidPathName;
            newPath = user.Insert(user.Length, "/Model/Youtube/");
            newPathName = newPath.Insert(newPath.Length, vidFileName);
            if (!System.IO.File.Exists(newPathName))
            {
                File.Copy(@originPath, @newPathName);
            }

            string strCmdText = "python python_video_processing.py -f  -s ";
            strCmdText = strCmdText.Insert(strCmdText.Length, splitTime);
            strCmdText = strCmdText.Insert(37, newPathName);
            strCmdText = strCmdText.Insert(7, newPath);

            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine(strCmdText);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }

        private void ProcessModel()
        {

            bool processing = true;
            segNum = 0;
            while (processing)
            {
                string strCmdText = "python /Model/scripts/run_all_pipeline.py -c  -sn  -sl  -i ";
                if (segNum == 0)
                {
                    strCmdText = strCmdText.Insert(strCmdText.Length - 14, "y");
                }
                else
                {
                    strCmdText = strCmdText.Insert(strCmdText.Length - 14, "n");
                }
                string newSegName = newPathName.Insert(newPathName.Length - 4, "-" + segNum.ToString());
                if (System.IO.File.Exists(newSegName))
                {
                    segNum++;
                    Dispatcher.Invoke(() =>
                    {
                        progressBar.Value = 100 * ((segNum - 1) / segNum);
                        label.Content = setProgressText(loadBarPercentage, windowMode);

                    });
                    strCmdText = strCmdText.Insert(strCmdText.Length - 9, segNum.ToString());
                    strCmdText = strCmdText.Insert(strCmdText.Length - 4, splitTime);
                    strCmdText = strCmdText.Insert(strCmdText.Length, newSegName);
                    strCmdText = strCmdText.Insert(7, user);
                    Process cmd = new Process();
                    cmd.StartInfo.FileName = "cmd.exe";
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.RedirectStandardOutput = true;
                    cmd.StartInfo.CreateNoWindow = true;
                    cmd.StartInfo.UseShellExecute = false;
                    cmd.Start();
                    cmd.StandardInput.WriteLine("activate capstone");
                    cmd.StandardInput.Flush();
                    cmd.StartInfo.WorkingDirectory = user + "/Model/";
                    cmd.StandardInput.WriteLine(strCmdText);
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                    Console.WriteLine(cmd.StandardOutput.ReadToEnd());
                    cmd.WaitForExit();
                }
                else
                {
                    windowMode++;
                    processing = false;
                }

            }
        }

        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            switch (windowMode)
            {
                case 0://link
                    SelectFile();
                    break;
                case 3://complete
                    finalResult = false;
                    Close();
                    break;
            }
        }

        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            switch (windowMode)
            {
                case 0://link
                    UploadFile();
                    break;
                case 1://upload
                    QuitMenu("Are you sure you wish to cancel?");
                    break;
                case 2://processing
                    QuitMenu("Do you wish to close the window?\nNote processing will still continue and\nprogress can be found in the load menu.");
                    break;
                case 3://complete
                    LoadCurrent();
                    break;
                case 4://loading
                    QuitMenu("Are you sure you wish to cancel?");
                    break;
            }
        }
        private void QuitMenu(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                finalResult = false;
                Close();
            }
        }

        private void LoadCurrent()
        {
            windowMode = 4;
            nameBox.Visibility = Visibility.Hidden;
            textBox.Visibility = Visibility.Hidden;
            leftButton.Visibility = Visibility.Hidden;
            rightButton.Visibility = Visibility.Visible;
            progressBar.Visibility = Visibility.Visible;
            label.Visibility = Visibility.Visible;
            rightButton.Content = "Cancel";
            label.Content = "Loading 0%";

            demoCodeWorker1.RunWorkerAsync();
        }

        private void UploadFile()
        {
            if (System.IO.Path.GetExtension(textBox.Text) == ".mp4")
            {
                windowMode = 1;
                nameBox.Visibility = Visibility.Hidden;
                textBox.Visibility = Visibility.Hidden;
                leftButton.Visibility = Visibility.Hidden;
                label.Visibility = Visibility.Visible;
                progressBar.Visibility = Visibility.Visible;
                ProcessModel();
                rightButton.Content = "Close";
                label.Content = "Uploading 0%";

                demoCodeWorker1.RunWorkerAsync();
            }

            else
            {
                MessageBox.Show("Invalid File Type", "Error");
            }
        }


        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {

            if (((TextBox)sender).Text == "Video Link" || ((TextBox)sender).Text == "Name")
            {
                ((TextBox)sender).Text = "";
            }
        }

        private string setProgressText(int progress, int mode)
        {
            string result = "";
            switch (mode)
            {
                case 1://upload
                    result = "Uploading " + progress + "%";
                    break;
                case 2://processing
                    result = "Processing " + progress + "%";
                    break;
                case 4://loading
                    result = "Loading " + progress + "%";
                    break;
            }
            return result;
        }

        private void DemoCodeWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            /*while (loadBarPercentage < 100)
            {
                loadBarPercentage++;
                Dispatcher.Invoke(() =>
                {
                    progressBar.Value = loadBarPercentage;
                    label.Content = setProgressText(loadBarPercentage, windowMode);

                });
                Thread.Sleep(10);
            }
            loadBarPercentage = 0;*/

            switch (windowMode)
            {
                case 1:
                    Dispatcher.Invoke(() => rightButton.Content = "Close");
                    DemoCodeWorker1_DoWork(sender, e);
                    break;
                case 2:
                    windowMode++;
                    Dispatcher.Invoke(() => ProcessingComplete());
                    break;
                case 4:
                    Dispatcher.Invoke(() =>
                    {
                        finalResult = true;
                        Close();
                    });
                    break;
            }
            e.Cancel = true;
            return;
        }

        private void ProcessingComplete()
        {
            progressBar.Visibility = Visibility.Hidden;
            leftButton.Visibility = Visibility.Visible;
            leftButton.Content = "Close";
            rightButton.Content = "Load";
            label.Content = "Processing Complete";
        }
    }
}