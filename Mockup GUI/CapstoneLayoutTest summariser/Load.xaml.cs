using System;
using System.Collections.Generic;
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
            Owner = Application.Current.MainWindow;
            listView.Items.Add(new VideoData { Name = "Patient 1", Status = "Complete", URL = "..\\..\\test2.zip" });
            listView.Items.Add(new VideoData { Name = "Patient 2", Status = "Complete", URL = "..\\..\\test.zip" });
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
            else if (data.Complete)
            {
                //Upload upload = new Upload(data.URL);
                //upload.ShowDialog();
                //if (upload.Result == true)
                //{
                okResult = data.URL;
                DialogResult = true;

                Close();
                //}
                //else
                //{
                //    MessageBox.Show("An error was encountered when downloading files.", "Error");
                //}
            }
            else
            {
                MessageBox.Show("Selected item is not complete.", "Error");
            }
        }
    }
}
