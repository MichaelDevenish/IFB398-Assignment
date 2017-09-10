using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for Upload.xaml
    /// </summary>
    public partial class Upload : Window
    {
        private bool finalResult = false;
        public bool Result { get { return finalResult; } }
        private int windowMode = 0;
        private int loadBarPercentage = 0;
        private string path, root, vidFilename, newPath;
        private int vidNum;
        private BackgroundWorker demoCodeWorker1;
        private string user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public Upload()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;
            demoCodeWorker1 = new BackgroundWorker();
            demoCodeWorker1.DoWork += DemoCodeWorker1_DoWork;
            progressBar.Maximum = 100;
        }
        public Upload(string fileToRead)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;
            windowMode = 4;
            progressBar.Maximum = 100;
            demoCodeWorker1 = new BackgroundWorker();
            demoCodeWorker1.DoWork += DemoCodeWorker1_DoWork;
            LoadCurrent();
        }
        private void SelectFile()
        {
            Microsoft.Win32.OpenFileDialog open = new Microsoft.Win32.OpenFileDialog();
            open.DefaultExt = ".mp4";
            open.Filter = "mpeg Files (*.mpeg)|*.mp4";

            if ((bool)open.ShowDialog())
            {
                
                path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                vidFilename = open.FileName;
                SegmentVideo();
                //ProcessModel();
                textBox.Text = open.FileName;
            }
        }

        private void SegmentVideo()
        {
            string splitTime = "10";
            string originPath = path.Insert(path.Length - 1, vidFilename);
            newPath = user.Insert(user.Length-1, "/Model/Youtube/");
            string newPathName = newPath.Insert(newPath.Length - 1, vidFilename);
            File.Copy(@originPath, @newPathName);

            string cmdWithSplit, cmdWithPath, cmdWithVidDir;
            string strCmdText = "python python_video_processing.py -f  -s ";
            cmdWithSplit = strCmdText.Insert(strCmdText.Length - 1, splitTime);
            cmdWithVidDir = cmdWithSplit.Insert(37, newPathName);
            cmdWithPath = cmdWithVidDir.Insert(7, newPath);

            System.Diagnostics.Process.Start("CMD.exe", cmdWithPath);
        }

        private void ProcessModel()
        {
            
            string strCmdText = "python /Model/scripts/run_all_pipeline.py -i ";
            string cmdWithVid, cmdWithModel, cmdWithVidDir;

            bool processing = true;
            while (processing) {

                cmdWithVidDir = strCmdText.Insert(strCmdText.Length - 1, newPath);
                cmdWithVid = cmdWithVidDir.Insert(strCmdText.Length - 1, vidFilename);
                cmdWithModel = strCmdText.Insert(7, user);
                System.Diagnostics.Process.Start("CMD.exe", cmdWithModel);

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
            while (loadBarPercentage < 100)
            {
                loadBarPercentage++;
                Dispatcher.Invoke(() =>
                {
                    progressBar.Value = loadBarPercentage;
                    label.Content = setProgressText(loadBarPercentage, windowMode);

                });
                Thread.Sleep(10);
            }
            loadBarPercentage = 0;

            switch (windowMode)
            {
                case 1:
                    windowMode++;
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
