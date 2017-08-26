using Microsoft.Win32;
using System.IO.Compression;
using System.Linq;
using System.Windows;

namespace CapstoneLayoutTest
{
    /// <summary>
    /// Interaction logic for Load.xaml
    /// </summary>
    public partial class Load : Window
    {
        private string okResult = "";
        public string OkResult { get { return okResult; } }
        public Load()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            DataManager.LoadFile("testfile.bin", listView);
        }

        /// <summary>
        /// imports a preprocessed data file (unless it is already in the view then it shows a warning and aborts) and shows the result 
        /// </summary>
        private void ImportPreprocessed()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "*.zip|*.zip";
            if (dlg.ShowDialog() == true)
            {
                using (ZipArchive archive = ZipFile.OpenRead(dlg.FileName))
                {
                    if (archive.Entries.Select(s => s.FullName == "output.csv" || s.FullName == "video.mp4").Count() == 2)
                    {
                        foreach (VideoData datas in listView.Items)
                        {
                            if (datas.URL == dlg.FileName)
                            {
                                MessageBox.Show("Data has already been imported");
                                return;
                            }
                        }
                        listView.Items.Add(new VideoData { Name = dlg.SafeFileName.Split('.')[0], URL = dlg.FileName });
                        DataManager.SaveFile("testfile.bin", listView);
                        okResult = dlg.FileName;
                        DialogResult = true;

                        Close();
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Invalid file, Aborted", "Warning");
                        return;
                    }

                }
            }
        }

        /// <summary>
        /// Loads the currently selected item in the listview
        /// </summary>
        private void LoadExisting()
        {
            VideoData data = (VideoData)listView.SelectedItem;
            if (data == null) MessageBox.Show("Must select an item.", "Error");
            else
            {
                okResult = data.URL;
                DialogResult = true;
                Close();
            }
        }



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadExisting();

        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            ImportPreprocessed();

        }

        private void RemoveItem_OnClick(object sender, RoutedEventArgs e)
        {
            listView.Items.Remove(listView.SelectedItem);  // remove the selected Item 
            DataManager.SaveFile("testfile.bin", listView); //save the data
        }


    }
}
