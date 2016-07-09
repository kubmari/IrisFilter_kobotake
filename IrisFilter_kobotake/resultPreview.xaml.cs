﻿using System;
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
            double[,] arrayImage =Processing.bitmapToArray(grayscaleImage);
             
           
            parentWindow.appendOutputConsole("Image converted to grayscale array!\n");

            //Prewitt and further processing
            Processing.ImageData imageData = new Processing.ImageData();
            imageData.calculateImageData(arrayImage, grayscaleImage.Width, grayscaleImage.Height);
            double[,] gradientOrientation = imageData.gradientOrientation;
           
            parentWindow.appendOutputConsole("Prewitt Calculated\n");
            //Back to bitmapsource
        /*    for(int i=0; i<grayscaleImage.Width; i++)
            {
                for(int j=0; j<grayscaleImage.Height; j++)
                {
                    Console.Write(gradientOrientation[i,j]+" ");
                }
                Console.WriteLine();
            }*/
            int[,] outputPrewitt = Processing.scaleAtanValuesTo255(imageData.gradientOrientation, grayscaleImage.Width, grayscaleImage.Height);
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
