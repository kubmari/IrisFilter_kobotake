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
            //bitmap (grayscaleImage) to array of values
            //Prewitt
            //Back to bitmapsource
            mainimage.Source = displayImage;
        }

        private void resultPreview_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            parentWindow.enableCalculationButton();
        }
    }
}
