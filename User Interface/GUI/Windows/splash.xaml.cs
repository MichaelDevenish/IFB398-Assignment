﻿using CapstoneLayoutTest.Helper_Functions;
using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;

namespace CapstoneLayoutTest
{
    /// <summary>
    /// Interaction logic for splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
        private UploadWindow upload;
        public Splash()
        {
            InitializeComponent();
            DataManager.LoadFile("processedData.bin", listView);
        }

        /// <summary>
        /// Loads the main window with the selected data
        /// </summary>
        /// <param name="path">the path to the data to load</param>
        public void loadWindow(string path)
        {
            MainWindow signIn = new MainWindow(path)
            {
                Width = 0,
                Height = 0,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false
            };
            signIn.Show();
            if (upload != null) upload.Owner = signIn;
            this.Close();
        }

        /// <summary>
        /// Loads the currently selected item in the listview
        /// </summary>
        private void LoadExisting()
        {
            VideoData data = (VideoData)listView.SelectedItem;
            if (data == null)
            {
                MessageBox.Show("Must select an item.", "Error");
            }
            else
            {
                loadWindow(data.URL);
            }
        }

        /// <summary>
        /// imports a preprocessed data file (unless it is already in the view) and shows the result 
        /// </summary>
        private void LoadPreprocessed()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "*.zip|*.zip";
            if (dlg.ShowDialog() == true)
            {
                using (ZipArchive archive = ZipFile.OpenRead(dlg.FileName))
                {
                    if (archive.Entries.Select(s => s.FullName == "output.csv" || s.FullName == "video.mp4").Count() == 2)
                    {
                        bool exists = false;
                        foreach (VideoData datas in listView.Items)
                        {
                            if (datas.URL == dlg.FileName)
                            {
                                exists = true;
                            }
                        }
                        if (!exists)
                        {
                            listView.Items.Add(new VideoData { Name = System.IO.Path.GetFileNameWithoutExtension(dlg.SafeFileName), URL = dlg.FileName });
                            DataManager.SaveFile("processedData.bin", listView);
                        }
                        loadWindow(dlg.FileName);
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

        private void ImportPreprocessed_Click(object sender, RoutedEventArgs e)
        {
            LoadPreprocessed();
        }

        private void SelectExisting_Click(object sender, RoutedEventArgs e)
        {
            LoadExisting();
        }

        private void ImportToProcess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                upload = new UploadWindow();
                upload.Owner = Window.GetWindow(this);
                upload.Show();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Model must be installed to upload data");
            }
        }
    }
}
