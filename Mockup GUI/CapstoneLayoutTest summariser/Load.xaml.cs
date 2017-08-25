using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;

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
            LoadFile("testfile.bin");
            //listView.Items.Add(new VideoData { Name = "Patient 1", URL = "..\\..\\test2.zip" });
            //listView.Items.Add(new VideoData { Name = "Patient 2", URL = "..\\..\\test.zip" });
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void load_button_Click(object sender, RoutedEventArgs e)
        {
            //disable if the current item is not complete
            VideoData data = (VideoData)listView.SelectedItem;
            if (data == null)
            {
                MessageBox.Show("Must select an item.", "Error");
            }
            else
            {

                okResult = data.URL;
                DialogResult = true;

                Close();
            }

        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
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
                        SaveFile("testfile.bin");
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
        private void SaveFile(string filePath)
        {
            VideoData[] items = new VideoData[listView.Items.Count];
            listView.Items.CopyTo(items, 0);

            // Serialize the items and save it to a file.
            using (FileStream fs = File.Create(filePath))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(fs, items);
            }

        }
        private void RemoveItem_OnClick(object sender, RoutedEventArgs e)
        {
            listView.Items.Remove(listView.SelectedItem);  // remove the selected Item 
            SaveFile("testfile.bin"); //save the data
        }

        private void LoadFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                VideoData[] items = null;

                using (FileStream fs = File.OpenRead(filePath))
                {
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    items = (VideoData[])bf.Deserialize(fs);
                }

                listView.Items.Clear();
                foreach (VideoData item in items)
                {
                    if (File.Exists(item.URL))
                        listView.Items.Add(item);
                }
            }
        }
    }
}
