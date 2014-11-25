using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using System.Collections.ObjectModel;
using WhatsappViewer.DataSources;
using Microsoft.Win32;


namespace WhatsappViewer
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

        IDataSource data;
        OpenFileDialog openFileDialog1 = new OpenFileDialog() { Filter = "All Supported files|*.crypt7;*.crypt;*.db;*.sqlite;|Android Crypted (.crypt7)|*.crypt7|Android Crypted (.crypt)|*.crypt|Android (.db)|*.db|IOS (.sqlite)|*.sqlite|All Files (*.*)|*.*" };

        private void ButtonSelectFile_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                if (openFileDialog1.ShowDialog() != true)
                    return;

                var filename = openFileDialog1.FileName.ToLower();

                if (filename.EndsWith(".crypt7") || filename.EndsWith(".crypt") || filename.EndsWith(".db"))
                {
                    data = new DataSourceAndroid(openFileDialog1.FileName);
                }
                else if (filename.EndsWith(".sqlite"))
                {
                    data = new DataSourceIOS(filename);
                }
                else
                {
                    MessageBox.Show("File not supported!");
                    return;
                }

                setFileInfo(openFileDialog1.FileName);
                TreeView1.ItemsSource = data.getChats();
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while opening file!\nMessage:\n" + ex.Message);
            }

        }


        private void TreeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var sel = TreeView1.SelectedValue as ChatItem;
            if (sel == null)
                return;

            if (data is DataSourceAndroid)
                ListView1.ItemsSource = data.getMessages(sel.name);
            else if (data is DataSourceIOS)
                ListView1.ItemsSource = data.getMessages(sel.id + "");
        }

        private void MediaHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var data = (ListView1.ItemsSource as List<IMessageItem>)[ListView1.SelectedIndex];

            if (data == null)
                return;
            try
            {
                var x = Utils.downloadMedia(data.media);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void setFileInfo(string fileName)
        {
            groupBoxInfo.DataContext = Utils.getFileInfo(fileName);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var search = (sender as TextBox).Text;
            if (string.IsNullOrWhiteSpace(search))
                TreeView1.ItemsSource = data.getChats();
            else
                TreeView1.ItemsSource = data.getChats().Where(o => o.name.Contains(search) || o.descr.Contains(search));
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {


            // Create an instance of the open file dialog box.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog1.CheckFileExists = false;


            var sel = TreeView1.SelectedValue as ChatItem;

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = openFileDialog1.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                foreach (IMessageItem item in ListView1.SelectedItems)
                {
                    System.IO.File.AppendAllText(openFileDialog1.FileName, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\n", item.datetime, sel.name, sel.descr, item.sender, item.message));
                }
            }

        }

    }
}
