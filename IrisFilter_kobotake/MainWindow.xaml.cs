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
using System.Drawing;

namespace IrisFilter_kobotake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] fileNames;
        private BitmapImage[] loadedImages;
        string acceptedExtension = ".png";
        bool isBatch = false;

        //checks if program should work for 1 image online or in batch mode
        public bool IsBatch
        {
            get { return isBatch; }
            set
            {
                isBatch = value;

                if(fileNames.Length==1)
                {
                    Console.WriteLine("Single Image Mode ON");
                    loadedImages = new BitmapImage[1];
                    loadedImages[0] = new BitmapImage(new Uri(fileNames[0]));
                    mainimage.Source = loadedImages[0];
                }
                else if( fileNames.Length > 1)
                {
                    mainimage.Source = new BitmapImage(new Uri("pack://application:,,,/img/batchModeImage.png", UriKind.Absolute));
                    Console.WriteLine("Batch Mode ON");
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void canvas_Drop(object sender, DragEventArgs e)
        {
            loadDataNames((string[])e.Data.GetData(DataFormats.FileDrop));
        }

        private void button_LoadFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Images: (*"+acceptedExtension+")|*"+acceptedExtension;
            if(openFileDialog.ShowDialog()==true)
            {
                loadDataNames(openFileDialog.FileNames);
            }

        }

        public Bitmap grayscaleImage;
        private void button_Calculate_Click(object sender, RoutedEventArgs e)
        {
            resultPreview newWindow = new resultPreview();
            newWindow.Owner = this;
            disableCalculationButton();

            //Assuming single image case!
            BitmapImage loadedImage = loadedImages[0];
            grayscaleImage = Processing.im2GrayBitmap(loadedImage);
            newWindow.Show();
            
        }
        
       

        //Loading names of files and triggering propertysetter
        private void loadDataNames(string[] _fileNames)
        {
            string[] validatedfileNames = validateNames(_fileNames);
            fileNames = validatedfileNames;
            if (validatedfileNames.Length>0)
            {
                textConsole.Text = "Loaded images: \n";
                foreach (string filename in validatedfileNames)
                {
                    textConsole.AppendText(filename + "\n");
                }
                IsBatch = validatedfileNames.Length > 1 ? true : false;
            }
        }

        public void appendOutputConsole(string text)
        {
            textConsole.AppendText(text);
        }
        //Making sure you only put acceptedExtension on load array
        private string[] validateNames(string[] _fileNames)
        {
            List<string> acceptedStrings = new List<string>();

            foreach (string fileName in _fileNames)
            {
                if(fileName.EndsWith(acceptedExtension))
                {
                    acceptedStrings.Add(fileName);
                    Console.WriteLine(fileName);
                }
  
            }

            string[] outputNames = acceptedStrings.ToArray();
            return outputNames;
        }

        public void enableCalculationButton()
        {
            button_Calculate.IsEnabled = true;

        }
        public void disableCalculationButton()
        {
            button_Calculate.IsEnabled = false;

        }
    }
}
