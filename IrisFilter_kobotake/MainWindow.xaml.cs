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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
namespace IrisFilter_kobotake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] fileNames;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void canvas_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            fileNames = files;
            textConsole.Text = "Loaded images: \n";
            foreach (string file in fileNames)
            {

                textConsole.AppendText(file+'\n');
            }

        }

        private void button_LoadFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Images: (*.png)|*.png";
            if(openFileDialog.ShowDialog()==true)
            {
                fileNames = openFileDialog.FileNames;
                textConsole.Text = "Loaded images: \n";
                foreach (string filename in fileNames)
                {
                    textConsole.AppendText(filename + '\n');
                }
            }

        }

        private void button_Calculate_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
