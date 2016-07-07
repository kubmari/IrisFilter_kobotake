using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace IrisFilter_kobotake
{
    /// <summary>
    /// Interaction logic for resultPreview.xaml
    /// </summary>
    public partial class resultPreview : Window
    {
         static MainWindow parentWindow;

        private Bitmap grayscaleImage;
        
        public resultPreview()
        {
            InitializeComponent();

        }
        

        private void resultPreview_Loaded(object sender, RoutedEventArgs e)
        {
            resultPreview.parentWindow = this.Owner as MainWindow;
            grayscaleImage = parentWindow.grayscaleImage;
            BitmapSource displayImage = Processing.Bitmap2BitmapSource(grayscaleImage);
            int[,] arrayImage = Processing.bitmapToArray(grayscaleImage);
            parentWindow.appendOutputConsole("Image converted to grayscale array!\n");
            //Prewitt and further processing
            int[,] outputPrewitt = Processing.prewitt3(arrayImage, grayscaleImage.Width, grayscaleImage.Height);
            parentWindow.appendOutputConsole("Prewitt Calculated\n");
            //Back to bitmapsource
            int test = Processing.findMax(outputPrewitt, grayscaleImage.Width, grayscaleImage.Height);
            Console.WriteLine("MAX VAL: " + test);
            outputPrewitt = Processing.scaleValuesTo255(outputPrewitt, grayscaleImage.Width, grayscaleImage.Height);
            Bitmap outputBitmap = Processing.arrayToBitmap(outputPrewitt, grayscaleImage.Width, grayscaleImage.Height);
            
            BitmapSource outputBitmapSource = Processing.Bitmap2BitmapSource(outputBitmap);
            parentWindow.appendOutputConsole("BitmapSource ready\n");

            mainimage.Source = outputBitmapSource;
        }

        private void resultPreview_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            parentWindow.enableCalculationButton();
        }
    }
}
