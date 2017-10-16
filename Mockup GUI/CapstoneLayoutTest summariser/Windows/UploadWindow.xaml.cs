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
using System.Collections.ObjectModel;
using System.IO.Compression;
using CapstoneLayoutTest.Helper_Functions;

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
        private string vidFileName, vidPathName, processingLocation, newPathName, saveLocation;
        public ObservableCollection<VideoItem> VidList { get { return vidList; } }
        private ObservableCollection<VideoItem> vidList = new ObservableCollection<VideoItem>();
        private int segNum;
        int split = 0;
        private int videosToProcess = 0;
        private BackgroundWorker processingWorker;
        private BackgroundWorker splittingWorker;
        private string user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public UploadWindow()
        {
            string file = user.Insert(user.Length, "\\Model\\");
            if (Directory.Exists(file))
            {
                InitializeComponent();
                listBox.ItemsSource = VidList;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                processingWorker = new BackgroundWorker();
                processingWorker.DoWork += processingWorker_DoWork;
                processingWorker.WorkerSupportsCancellation = true;
                splittingWorker = new BackgroundWorker();
                splittingWorker.DoWork += splittingWorker_DoWork;
                progressBar.Maximum = 100;
                saveLocation = user.Insert(user.Length, "/Model/processed/");
                processingLocation = user.Insert(user.Length, "/Model/Youtube/");
            }
            else
            {
                throw new FileNotFoundException();
            }
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
                string p = System.IO.Path.GetFileName(vidPathName);
                if (vidList.Any(e => e.OldDir == vidPathName))
                {
                    MessageBox.Show("Video is already selected");
                }
                else if (vidList.Any(e => e.Title == System.IO.Path.GetFileName(vidPathName)))
                {
                    MessageBox.Show("Cant have two files with the same name");
                }
                else if (File.Exists(saveLocation + p.Split('.')[0] + ".zip"))
                {
                    MessageBox.Show("A video by that name has already been processed");
                }
                else
                {
                    bool result = Int32.TryParse(splitCountBox.Text, out split);
                    if (result)
                    {
                        splitCountBox.IsEnabled = false;
                        splittingWorker.RunWorkerAsync();
                    }
                    else
                    {
                        MessageBox.Show("invalid split format");
                    }


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
                case 1:
                    if (leftButton.Content == "More")
                    {
                        MinHeight = 390;
                        Height = 390;
                        MaxHeight = 390;
                        textBox.Visibility = Visibility.Visible;
                        leftButton.Content = "Less";
                    }
                    else
                    {
                        MinHeight = 80;
                        Height = 80;
                        MaxHeight = 80;
                        textBox.Visibility = Visibility.Hidden;
                        leftButton.Content = "More";
                    }
                    //show more
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
            //call loadResult to do so
        }

        private void UploadFile()
        {

            windowMode = 1;
            leftButton.Content = "More";
            MinHeight = 80;
            Height = 80;
            MaxHeight = 80;
            removeButton.Visibility = Visibility.Hidden;
            listBox.Visibility = Visibility.Hidden;
            rightButton.Visibility = Visibility.Hidden;
            label1.Visibility = Visibility.Hidden;
            label2.Visibility = Visibility.Hidden;
            splitCountBox.Visibility = Visibility.Hidden;

            rightButton.Content = "Close";

            processingWorker.RunWorkerAsync();

        }

        private void ProcessingComplete()
        {
            windowMode = 3;
            MinHeight = 125;
            Height = 125;
            MaxHeight = 125;

            textBox.Visibility = Visibility.Hidden;
            listBox.Visibility = Visibility.Visible;
            removeButton.Visibility = Visibility.Visible;
            leftButton.Content = "Close";
            removeButton.Content = "Show";
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedItem != null)
            {

                removeButton.IsEnabled = true;
            }
            else
            {
                removeButton.IsEnabled = false;
            }
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (windowMode != 3)
            {
                DirectoryInfo d = new DirectoryInfo(processingLocation);
                FileInfo[] Files = d.GetFiles("*");
                List<string> countList = new List<string>();
                string[] splitPath = ((VideoItem)listBox.SelectedItem).Title.Split('.');
                foreach (FileInfo file in Files)
                {
                    if (file.Name.Contains(splitPath[0] + "-"))
                        countList.Add(file.Name);
                }
                videosToProcess -= countList.Count();

                vidList.Remove((VideoItem)listBox.SelectedItem);
                if (vidList.Count == 0)
                {
                    splitCountBox.IsEnabled = true;
                    rightButton.IsEnabled = false;
                }
            }
            else
            {
                LoadCurrent();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            processingWorker.CancelAsync();
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

            user = user.Replace("\\", "/");

            string path = vidPathName.Replace("\\", "/");
            vidFileName = System.IO.Path.GetFileName(path);
            string originPath = path;
            newPathName = processingLocation.Insert(processingLocation.Length, vidFileName);
            Dispatcher.Invoke(() => { vidList.Add(new VideoItem() { Title = vidFileName, OldDir = vidPathName, NewDir = newPathName }); });
            if (!System.IO.File.Exists(newPathName))
            {
                File.Copy(@originPath, @newPathName);
            }


            string[] splitPath = vidFileName.Split('.');
            string process = "ffmpeg -i " + newPathName + " -c copy -f segment -segment_time "
            + split + " " + processingLocation + splitPath[0] + "-%d." + splitPath[1];

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

            DirectoryInfo d = new DirectoryInfo(processingLocation);
            FileInfo[] Files = d.GetFiles("*");
            List<string> countList = new List<string>();
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains(splitPath[0] + "-"))
                    countList.Add(file.Name);
            }
            videosToProcess += countList.Count();
            Dispatcher.Invoke(() =>
            {
                rightButton.IsEnabled = true;
                textBox.Text += cmd.StandardOutput.ReadToEnd();
                textBox.ScrollToEnd();
            });


            e.Cancel = true;
            return;
        }


        private void processingWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            ListView list = null;
            Dispatcher.Invoke(() =>
            {
                list = DataManager.CreateListView();
                DataManager.LoadFile("processedData.bin", list);
            });

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
                    string newSegName = vidList[i].NewDir.Insert(vidList[i].NewDir.Length - 4, "-" + segNum.ToString());
                    if (System.IO.File.Exists(newSegName))
                    {
                        segNum++;
                        Dispatcher.Invoke(() => { progressBar.Value = 100 * (((double)segNum - 1) / ((double)videosToProcess)); });
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
                        string output = cmd.StandardOutput.ReadToEnd();
                        Console.WriteLine(output);
                        cmd.WaitForExit();
                        Dispatcher.Invoke(() =>
                        {
                            textBox.Text += output;
                            textBox.ScrollToEnd();
                        });
                    }
                    else
                    {
                        processing = false;
                    }
                }
                string name = vidList[i].Title.Split('.')[0];
                using (ZipArchive zip = ZipFile.Open(saveLocation + name + ".zip", ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(processingLocation + name + ".mp4", "video.mp4");
                    zip.CreateEntryFromFile(processingLocation + name + ".csv", "output.csv");
                }
                Dispatcher.Invoke(() =>
                {
                    list.Items.Add(new VideoData { Name = name, URL = saveLocation + name + ".zip" });
                });
            }
            Array.ForEach(Directory.GetFiles(processingLocation), File.Delete);
            Dispatcher.Invoke(() =>
            {
                DataManager.SaveFile("processedData.bin", list);
                ProcessingComplete();
            });

            e.Cancel = true;
            return;
        }

    }
}
public class VideoItem
{
    public string Title { get; set; }
    public string OldDir { get; set; }
    public string NewDir { get; set; }

}