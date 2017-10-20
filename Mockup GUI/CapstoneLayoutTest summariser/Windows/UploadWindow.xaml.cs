using System;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using CapstoneLayoutTest.Helper_Functions;

namespace CapstoneLayoutTest
{
    /// <summary>
    /// Interaction logic for UploadWindow.xaml
    /// </summary>
    public partial class UploadWindow : Window
    {
        private const string SAVELOCATION = "/Model/processed/";
        private const string PROCESSINGLOCATION = "/Model/processing/";
        private const int SMALLHEIGHT = 80;
        private const int TALLHEIGHT = 390;
        private const int STANDARDHEIGHT = 125;

        private bool finalResult = false;
        private string vidPathName, processingLocation, saveLocation, user;
        private int split, videosToProcess, windowMode = 0;
        private ObservableCollection<VideoItem> vidList = new ObservableCollection<VideoItem>();
        private BackgroundWorker processingWorker;
        private BackgroundWorker splittingWorker;

        public bool Result { get { return finalResult; } }
        public ObservableCollection<VideoItem> VidList { get { return vidList; } }

        public UploadWindow()
        {
            user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string file = user.Insert(user.Length, "\\Model\\");
            if (Directory.Exists(file))
            {
                InitializeComponent();
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                listBox.ItemsSource = VidList;
                CreateWorkers();
                SetDataLocations();
            }
            else
            {
                throw new FileNotFoundException();
            }
        }
        #region setup functions
        /// <summary>
        /// sets up the loading and saving locations
        /// </summary>
        private void SetDataLocations()
        {
            saveLocation = user.Insert(user.Length, SAVELOCATION);
            processingLocation = user.Insert(user.Length, PROCESSINGLOCATION);
        }

        /// <summary>
        /// Creates all of the background workers
        /// </summary>
        private void CreateWorkers()
        {
            processingWorker = new BackgroundWorker();
            processingWorker.DoWork += processingWorker_DoWork;
            processingWorker.WorkerSupportsCancellation = true;
            splittingWorker = new BackgroundWorker();
            splittingWorker.DoWork += splittingWorker_DoWork;
        }
        #endregion

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

        /// <summary>
        /// Opens a file dialog to select a video and starts a background worker to 
        /// </summary>
        private void SelectFile()
        {
            Microsoft.Win32.OpenFileDialog open = new Microsoft.Win32.OpenFileDialog();
            open.DefaultExt = ".mp4";
            open.Filter = "video files (*.mov,*.mp4)|*.mov;*.mp4";

            if ((bool)open.ShowDialog())
            {
                vidPathName = open.FileName;
                string p = System.IO.Path.GetFileName(vidPathName);

                if (vidList.Any(e => e.OldDir == vidPathName))
                    MessageBox.Show("Video is already selected");
                else if (vidList.Any(e => e.Title == System.IO.Path.GetFileName(vidPathName)))
                    MessageBox.Show("Cant have two files with the same name");
                else if (File.Exists(saveLocation + System.IO.Path.GetFileNameWithoutExtension(p) + ".zip"))
                    MessageBox.Show("A video by that name has already been processed");
                else
                {
                    bool result = int.TryParse(splitCountBox.Text, out split);
                    if (result)
                    {
                        splitCountBox.IsEnabled = false;
                        splittingWorker.RunWorkerAsync();
                    }
                    else MessageBox.Show("invalid split format");
                }
            }
        }

        /// <summary>
        /// calls LoadResult on the selected video in the listBox
        /// </summary>
        private void LoadCurrent()
        {
            VideoItem video = ((VideoItem)listBox.SelectedItem);
            LoadResult(saveLocation + System.IO.Path.GetFileNameWithoutExtension(video.Title) + ".zip");
        }

        /// <summary>
        /// Starts processing the imported videos 
        /// </summary>
        private void UploadFile()
        {
            windowMode = 1;
            ProcessingLayoutSmall();
            processingWorker.RunWorkerAsync();
        }
        private void ProcessAllVideos()
        {
            ListView list = null;
            Dispatcher.Invoke(() => list = DataManager.FirstLoad("processedData.bin"));
            for (int i = 0; i < vidList.Count; i++) ProcessVideo(list, i);
            Array.ForEach(Directory.GetFiles(processingLocation), File.Delete);
            Dispatcher.Invoke(() =>
            {
                DataManager.SaveFile("processedData.bin", list);
                ProcessingCompleteLayout();
            });
        }

        private void ProcessVideo(ListView list, int i)
        {
            bool processing = true;
            int segNum = 0;
            while (processing && !processingWorker.CancellationPending)
            {
                string strCmdText = "python /Model/scripts/run_all_pipeline.py -c  -sn  -sl  -i ";
                if (segNum == 0) strCmdText = strCmdText.Insert(strCmdText.Length - 14, "y");
                else strCmdText = strCmdText.Insert(strCmdText.Length - 14, "n");

                string newSegName = vidList[i].NewDir.Insert(vidList[i].NewDir.Length - 4, "-" + segNum.ToString());

                if (System.IO.File.Exists(newSegName))
                {
                    segNum++;
                    Dispatcher.Invoke(() => progressBar.Value = 100 * (((double)segNum - 1) / (videosToProcess)));
                    ComputeSingleSegment(segNum, strCmdText, newSegName);
                }
                else processing = false;
            }
            Dispatcher.Invoke(() => FileManager.CompressAndSaveResults(list,
                System.IO.Path.GetFileNameWithoutExtension(vidList[i].Title),
                saveLocation, processingLocation));
        }

        private void ComputeSingleSegment(int segNum, string strCmdText, string newSegName)
        {
            strCmdText = strCmdText.Insert(strCmdText.Length - 9, segNum.ToString());
            strCmdText = strCmdText.Insert(strCmdText.Length - 4, split.ToString());
            strCmdText = strCmdText.Insert(strCmdText.Length, "\"" + newSegName + "\"");
            strCmdText = strCmdText.Insert(7, user);

            Shell cmd = new Shell();
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

        private void Splitter()
        {
            Dispatcher.Invoke(() => rightButton.IsEnabled = false);
            user = user.Replace("\\", "/");
            string path = vidPathName.Replace("\\", "/");

            string vidFileName = System.IO.Path.GetFileName(path);
            string[] splitPath = { System.IO.Path.GetFileNameWithoutExtension(vidFileName), System.IO.Path.GetExtension(vidFileName) };
            string newPathName = processingLocation.Insert(processingLocation.Length, splitPath[0] + ".mp4");

            if (!Directory.Exists(processingLocation)) Directory.CreateDirectory(processingLocation);
            Dispatcher.Invoke(() => vidList.Add(new VideoItem() { Title = vidFileName, OldDir = vidPathName, NewDir = newPathName }));

            if (!System.IO.File.Exists(newPathName))
                FileManager.CopyVideo(path, newPathName, splitPath[1]);

            SplitVideo(newPathName, splitPath[0]);
            Dispatcher.Invoke(() => rightButton.IsEnabled = true);

            videosToProcess += FileManager.GetNumberofSubfiles(vidFileName, processingLocation);
        }

        private void SplitVideo(string newPathName, string name)
        {
            string process = "ffmpeg -i \"" + newPathName + "\" -c copy -f segment -segment_time "
                        + split + " \"" + processingLocation + name + "-%d.mp4\"";
            Shell cmd = new Shell();
            cmd.Start();
            cmd.WriteLastItem(process);
            string output = cmd.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
            Dispatcher.Invoke(() =>
            {
                textBox.Text += output;
                textBox.ScrollToEnd();
            });
        }

        #region layouts

        /// <summary>
        /// Small processing Layout
        /// </summary>
        private void ProcessingLayoutSmall()
        {
            MinHeight = SMALLHEIGHT;
            Height = SMALLHEIGHT;
            MaxHeight = SMALLHEIGHT;
            leftButton.Content = "More";
            textBox.Visibility = Visibility.Hidden;
            removeButton.Visibility = Visibility.Hidden;
            listBox.Visibility = Visibility.Hidden;
            rightButton.Visibility = Visibility.Hidden;
            label1.Visibility = Visibility.Hidden;
            label2.Visibility = Visibility.Hidden;
            splitCountBox.Visibility = Visibility.Hidden;
            rightButton.Content = "Close";
        }

        /// <summary>
        /// expanded processing Layout
        /// </summary>
        private void ProcessingLayoutLarge()
        {
            MinHeight = TALLHEIGHT;
            Height = TALLHEIGHT;
            MaxHeight = TALLHEIGHT;
            textBox.Visibility = Visibility.Visible;
            leftButton.Content = "Less";
        }

        /// <summary>
        /// Layout used when processing is complete
        /// </summary>
        private void ProcessingCompleteLayout()
        {
            windowMode = 3;
            MinHeight = STANDARDHEIGHT;
            Height = STANDARDHEIGHT;
            MaxHeight = STANDARDHEIGHT;

            textBox.Visibility = Visibility.Hidden;
            listBox.Visibility = Visibility.Visible;
            removeButton.Visibility = Visibility.Visible;
            leftButton.Content = "Close";
            removeButton.Content = "Show";
        }
        #endregion

        #region events
        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            switch (windowMode)
            {
                case 0://link
                    SelectFile();
                    break;
                case 1://processing
                    if ((string)leftButton.Content == "More") ProcessingLayoutLarge();
                    else ProcessingLayoutSmall();
                    break;
                case 3://complete
                    finalResult = false;
                    Close();
                    break;
            }
        }

        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            UploadFile();
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedItem != null) removeButton.IsEnabled = true;
            else removeButton.IsEnabled = false;
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (windowMode != 3)
            {
                videosToProcess -= FileManager.GetNumberofSubfiles(((VideoItem)listBox.SelectedItem).Title, processingLocation);
                vidList.Remove((VideoItem)listBox.SelectedItem);
                if (vidList.Count == 0)
                {
                    splitCountBox.IsEnabled = true;
                    rightButton.IsEnabled = false;
                }
            }
            else LoadCurrent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            processingWorker.CancelAsync();
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text == "Video Link" || ((TextBox)sender).Text == "Name")
                ((TextBox)sender).Text = "";
        }
        #endregion

        #region workers
        private void splittingWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Splitter();
            e.Cancel = true;
            return;
        }

        private void processingWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            ProcessAllVideos();
            e.Cancel = true;
            return;
        }
        #endregion
    }
}
public class VideoItem
{
    public string Title { get; set; }
    public string OldDir { get; set; }
    public string NewDir { get; set; }

}