using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IrisFilter_kobotake
{
    class Processing
    {

        public static int[,] bitmapToArray(Bitmap input)
        {
         int[,] imageArray = new int[input.Width, input.Height];
         for(int x=0; x< input.Width; x++)
         {
             for(int y=0; y< input.Height; y++)
             {
                 System.Drawing.Color tempPixel = input.GetPixel(x, y);
                 imageArray[x, y] = tempPixel.R;

             }
         }

            return imageArray;
        }


        public static Bitmap arrayToBitmap(int[,] input, int sizeX, int sizeY)
        {
            Bitmap imageBitmap = new Bitmap(sizeX, sizeY);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    System.Drawing.Color newCol = System.Drawing.Color.FromArgb(255, input[x, y], input[x, y], input[x, y]);
                   
                    imageBitmap.SetPixel(x,y,newCol);
                    

                }
            }

            return imageBitmap;
        }

        public static Bitmap im2GrayBitmap(BitmapImage input)
        {
            Bitmap bitmapGrayImage = MakeGrayscale3(BitmapImage2Bitmap(input));

            return bitmapGrayImage;
        }


        //source http://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale
        public static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
               });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }


        //Prewitt gradient on 3x3 neighbourhood
        public static int[,] prewitt3(int[,] input, int sizeX, int sizeY)
        {
            int[,] horizontalFilter =new int[3, 3] {    {  1,  1,  1 }, 
                                                        {  0,  0,  0 }, 
                                                        { -1, -1, -1 } };


            int[,] verticalFilter = new int[3, 3] {     { -1, 0, 1 },
                                                        { -1, 0, 1 },
                                                        { -1, 0, 1 } };

            int[,] outputImage = new int[sizeX, sizeY];

            for(int x=0; x< sizeX; x++)
            {
                for(int y=0; y< sizeY; y++)
                {
                    outputImage[x, y] = 0;
                }
            }

            for (int x=1; x< sizeX-1; x++)
            {
                for(int y=1; y< sizeY-1; y++)
                {
                    int resultHorizontalPixel = 0;
                    for (int n=-1; n<=1; n++)
                    {
                        for(int m=-1; m<=1;m++)
                        {
                            resultHorizontalPixel = resultHorizontalPixel + (horizontalFilter[1+n, 1+m] * input[x+n, y+m]);
                           
                        }
                    }
                    outputImage[x, y] = resultHorizontalPixel;


                }
            }


            //UWAGA ZAIMPLEMENTOWAC VERTICAL FILTER I DODAC DO OBLICZEN MAGNITUDY (teraz jest G=sqrt(Gx^2 + Gx^2) powinno byc G=sqrt(Gx^2 + Gy^2))
            int[,] gradientMagnitude = new int[sizeX, sizeY];

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    gradientMagnitude[x, y] = (int)(Math.Sqrt(Math.Pow((double)outputImage[x, y], 2) + Math.Pow((double)outputImage[x, y], 2)));
                }
            }
            return gradientMagnitude;
        }
        //scales matrices with values between -255..255 to 0..255
        public static int[,] scaleValuesTo255(int[,] inputMatrix, int sizeX, int sizeY)
        {
            int[,] outputMatrix = new int[sizeX, sizeY];
            
            int minVal = findMin(inputMatrix, sizeX, sizeY);
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    outputMatrix[i, j] = inputMatrix[i, j] - minVal; //0..n  
                }
            }

            int maxVal = findMax(outputMatrix, sizeX, sizeY);
            for (int i=0; i< sizeX; i++)
            {
                for(int j=0; j< sizeY; j++)
                {
                    outputMatrix[i, j] = (outputMatrix[i, j]*255) / maxVal; //0-1 * 255 => 0-255 
                }
            }
            return outputMatrix;
        }

        public static int findMax(int[,] inputMatrix, int sizeX, int sizeY)
        {
            int maxVal = int.MinValue;
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    if (inputMatrix[i,j] > maxVal)
                        maxVal = inputMatrix[i,j];

                }
            }
            return maxVal;
        }

        public static int findMin(int[,] inputMatrix, int sizeX, int sizeY)
        {
            int minVal = int.MaxValue;
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    if (inputMatrix[i, j] < minVal)
                        minVal = inputMatrix[i, j];

                }
            }
            return minVal;
        }

        //source http://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        //source http://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource Bitmap2BitmapSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSource retval;

            try
            {
                retval = Imaging.CreateBitmapSourceFromHBitmap(
                             hBitmap,
                             IntPtr.Zero,
                             System.Windows.Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
        }

    }
}
