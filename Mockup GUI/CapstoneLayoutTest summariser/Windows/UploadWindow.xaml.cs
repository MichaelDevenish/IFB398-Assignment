using System;
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
using WMPLib;

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
        private string vidFileName, vidPathName, newPath, newPathName;
        private List<string> vidList = new List<string>();
        private int segNum;
        private int videosToProcess = 0;
        private BackgroundWorker processingWorker;
        private BackgroundWorker splittingWorker;
        private string user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public UploadWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            processingWorker = new BackgroundWorker();
            processingWorker.DoWork += processingWorker_DoWork;
            processingWorker.WorkerSupportsCancellation = true;
            splittingWorker = new BackgroundWorker();
            splittingWorker.DoWork += splittingWorker_DoWork;
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
                splittingWorker.RunWorkerAsync();
                textBox.Text = open.FileName;
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
                case 3://complete
                    LoadCurrent();
                    break;
            }
        }
        private void QuitMenu(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                processingWorker.CancelAsync();
                finalResult = false;
                Close();
            }
        }

        private void LoadCurrent()
        {

            //load into the main window
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
                rightButton.Content = "Close";
                label.Content = "Uploading 0%";

                processingWorker.RunWorkerAsync();
            }

            else
            {
                MessageBox.Show("Invalid File Type", "Error");
            }
        }

        private void ProcessingComplete()
        {
            progressBar.Visibility = Visibility.Hidden;
            windowMode = 3;
            leftButton.Visibility = Visibility.Visible;
            leftButton.Content = "Close";
            rightButton.Content = "Load";
            label.Content = "Processing Complete";
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {

            if (((TextBox)sender).Text == "Video Link" || ((TextBox)sender).Text == "Name")
            {
                ((TextBox)sender).Text = "";
            }
        }

        private void splittingWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Dispatcher.Invoke(() => { rightButton.IsEnabled = false; });
            //start in backgroundworker and do callbacks to enable the buttons and move the progressbar
            user = user.Replace("\\", "/");
            vidPathName = vidPathName.Replace("\\", "/");
            vidFileName = System.IO.Path.GetFileName(vidPathName);
            string originPath = vidPathName;
            newPath = user.Insert(user.Length, "/Model/Youtube/");
            newPathName = newPath.Insert(newPath.Length, vidFileName);
            vidList.Add(newPathName);
            if (!System.IO.File.Exists(newPathName))
            {
                File.Copy(@originPath, @newPathName);
            }
            int split = 10;//TEMP change to gui prop

            string[] splitPath = vidFileName.Split('.');
            string process = "ffmpeg -i " + newPathName + " -c copy -f segment -segment_time "
            + split + " " + newPath + splitPath[0] + "-%d." + splitPath[1];

            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine(process);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();

            DirectoryInfo d = new DirectoryInfo(newPath);
            FileInfo[] Files = d.GetFiles("*");
            List<string> countList = new List<string>();
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains(splitPath[0] + "-"))
                    countList.Add(file.Name);
            }
            videosToProcess += countList.Count();
            Dispatcher.Invoke(() => { rightButton.IsEnabled = true; });
            //change this to write to screen in callback
            //Console.WriteLine(cmd.StandardOutput.ReadToEnd());

            e.Cancel = true;
            return;
        }

        private void processingWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            for (int i = 0; i < vidList.Count; i++)
            {
                bool processing = true;
                segNum = 0;
                while (processing && !processingWorker.CancellationPending)
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
                    string newSegName = vidList[i].Insert(vidList[i].Length - 4, "-" + segNum.ToString());
                    if (System.IO.File.Exists(newSegName))
                    {
                        int split = 10;//TEMP change to gui prop


                        segNum++;
                        Dispatcher.Invoke(() =>
                           {

                               progressBar.Value = 100 * (((double)segNum - 1) / ((double)videosToProcess));
                               label.Content = "Processing " + progressBar.Value + "%";
                           });
                        strCmdText = strCmdText.Insert(strCmdText.Length - 9, segNum.ToString());
                        strCmdText = strCmdText.Insert(strCmdText.Length - 4, split.ToString());
                        strCmdText = strCmdText.Insert(strCmdText.Length, newSegName);
                        strCmdText = strCmdText.Insert(7, user);
                        Process cmd = new Process();
                        cmd.StartInfo.FileName = "cmd.exe";
                        cmd.StartInfo.RedirectStandardInput = true;
                        cmd.StartInfo.RedirectStandardOutput = true;
                        cmd.StartInfo.CreateNoWindow = true;
                        cmd.StartInfo.UseShellExecute = false;
                        cmd.StartInfo.WorkingDirectory = user + "/Model/";
                        cmd.Start();
                        cmd.StandardInput.WriteLine("activate capstone");
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.WriteLine(strCmdText);
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.Close();
                        Console.WriteLine(cmd.StandardOutput.ReadToEnd());
                        cmd.WaitForExit();
                    }
                    else
                    {
                        processing = false;
                    }


                }

            }
            //TODO export the data into a zip file and clean the working directory
            Dispatcher.Invoke(() => { ProcessingComplete(); });

            e.Cancel = true;
            return;
        }

    }
}