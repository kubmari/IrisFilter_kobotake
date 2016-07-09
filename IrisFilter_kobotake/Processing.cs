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

        public static double[,] bitmapToArray(Bitmap input)
        {
            double[,] imageArray = new double[input.Width, input.Height];
            Rectangle rect = new Rectangle(0, 0, input.Width, input.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                input.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, input.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * input.Height;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            for (int i = 0; i < input.Width; i++)
            {
                for (int j = 0; j < input.Height; j++)
                {
                    int index = i + j * input.Width;
                    imageArray[i, j] = rgbValues[index * 4];
                }
            }
            return imageArray;
        }

        // source http://stackoverflow.com/questions/13511661/create-bitmap-from-double-two-dimentional-array
        public static unsafe Bitmap arrayToBitmap(int[,] input, int sizeX, int sizeY)
        {
            Bitmap imageBitmap = new Bitmap(sizeX, sizeY);
            BitmapData bmpData = imageBitmap.LockBits(
                new Rectangle(0, 0, sizeX, sizeY),
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );



            ColorARGB* ptr = (ColorARGB*)bmpData.Scan0;

            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    byte color = (byte)input[i, j];

                    ColorARGB* position = ptr + i + j * sizeX;
                    position->A = 255;
                    position->R = color;
                    position->G = color;
                    position->B = color;
                }
            }
            imageBitmap.UnlockBits(bmpData);
            return imageBitmap;

        }
        public struct ColorARGB
        {
            public byte B;
            public byte G;
            public byte R;
            public byte A;

            public ColorARGB(System.Drawing.Color color)
            {
                A = color.A;
                R = color.R;
                G = color.G;
                B = color.B;
            }

            public ColorARGB(byte a, byte r, byte g, byte b)
            {
                A = a;
                R = r;
                G = g;
                B = b;
            }

            public System.Drawing.Color ToColor()
            {
                return System.Drawing.Color.FromArgb(A, R, G, B);
            }
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





        public static class FilterKernels
        {
            public static double[,] filterHorizontal = new double[3, 3] {    {  1,  1,  1 },
                                                                {  0,  0,  0 },
                                                                { -1, -1, -1 }};


            public static double[,] filterVertical = new double[3, 3] {     { -1, 0, 1 },
                                                               { -1, 0, 1 },
                                                               { -1, 0, 1 }};
        }

        public class ImageData
        {

            public double[,] imageData;
            public int sizeX;
            public int sizeY;
            public double[,] gradientHorizontal;
            public double[,] gradientVertical;
            public double[,] gradientMagnitude;
            public double[,] gradientOrientation;

            //Main method used to populate fields with data
            public void calculateImageData(double[,] _imageData, int _sizeX, int _sizeY)
            {
                imageData = _imageData;
                setSize(_sizeX, _sizeY);
                generateGradients();
            }

            private void parGradientMagnitude()
            {
                this.gradientMagnitude = new double[sizeX, sizeY];
                Parallel.For(0, sizeX, x =>
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        this.gradientMagnitude[x, y] = (Math.Sqrt(Math.Pow((double)this.gradientHorizontal[x, y], 2) + Math.Pow((double)this.gradientVertical[x, y], 2)));
                    }
                });

            }

            private void setSize(int _sizeX, int _sizeY)
            {
                sizeX = _sizeX;
                sizeY = _sizeY;
            }

            private void generateGradients()
            {
                calculateGradients();
                parGradientMagnitude();
                calculateOrientation();
            }

            private void calculateOrientation()
            {
                gradientOrientation = new double[sizeX, sizeY];
                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        /*   if (gradientVertical[i, j] == 0)
                           {
                               gradientVertical[i, j] = 0.001;
                           }
                              gradientOrientation[i, j] = Math.Atan(gradientHorizontal[i, j] / gradientVertical[i, j]);
                              */
                        /*     if (gradientHorizontal[i, j] == 0)
                             {
                                 gradientHorizontal[i, j] = 0.001;
                             }
                             gradientOrientation[i, j] = Math.Atan(gradientVertical[i, j] / gradientHorizontal[i, j]);
                             */
                        gradientOrientation[i, j] = Math.Atan2(gradientVertical[i,j], gradientHorizontal[i,j]);


                    }
                }
            }

            private void calculateGradients()
            {
                Parallel.Invoke(() =>
                {
                    gradientHorizontal = new double[sizeX, sizeY];
                    gradientHorizontal = parConv3(FilterKernels.filterHorizontal);
                },
                () =>
                {
                    gradientVertical = new double[sizeX, sizeY];
                    gradientVertical = parConv3(FilterKernels.filterVertical);
                });
            }

            public double[,] parConv3(double[,] kernel)
            {
                double[,] outputImage = new double[sizeX, sizeY];

                Parallel.For(1, sizeX - 1, x =>
                {
                    for (int y = 1; y < sizeY - 1; y++)
                    {
                        double resultPixel = 0;
                        for (int n = -1; n <= 1; n++)
                        {
                            for (int m = -1; m <= 1; m++)
                            {
                                resultPixel = resultPixel + (kernel[1 + n, 1 + m] * imageData[x + n, y + m]);

                            }
                        }
                        outputImage[x, y] = resultPixel;
                    }
                }
                  );

                return outputImage;


            }
        }



    

        public static int[,] scaleValuesTo255(double[,] inputMatrix, int sizeX, int sizeY)
        {
            int[,] outputMatrix = new int[sizeX, sizeY];

            double minVal = inputMatrix.Cast<double>().Min();
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    inputMatrix[i,j] = inputMatrix[i, j] - minVal; //0..n  
                }
            }

            double maxVal = inputMatrix.Cast<double>().Max();
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    inputMatrix[i,j] = ((inputMatrix[i, j] * 255.0) / maxVal); //0-1 * 255 => 0-255 
                    outputMatrix[i, j] = (int)inputMatrix[i, j];
                }
            }
            return outputMatrix;
        }
        //Doesnt always scale to 0-255. Actual range can be smaller (eg. 10-200) but always within 0-255
        public static int[,] softScaleValuesTo255(double[,] inputMatrix, int sizeX, int sizeY)
        {
            int[,] outputMatrix = new int[sizeX, sizeY];
            
            double minVal = inputMatrix.Cast<double>().Min();
                for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    inputMatrix[i, j] = inputMatrix[i, j] - minVal; //0..n  
                }
            }

            double maxVal = inputMatrix.Cast<double>().Max();
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    inputMatrix[i, j] = (inputMatrix[i, j]*255/2.0); //0-1 * 255 => 0-255 
                    outputMatrix[i, j] = (int)inputMatrix[i, j];
                }
            }
            return outputMatrix;
        }

        public static int[,] scaleAtanValuesTo255(double[,] inputMatrix, int sizeX, int sizeY)
        {
            int[,] outputMatrix = new int[sizeX, sizeY];

            double minVal = inputMatrix.Cast<double>().Min();
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    inputMatrix[i, j] = inputMatrix[i, j] + Math.PI ; //0..2n  
                }
            }

            double maxVal = inputMatrix.Cast<double>().Max();
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    inputMatrix[i, j] = (inputMatrix[i, j] / Math.PI); //0-2pi => 0-1 
                    inputMatrix[i, j] = (inputMatrix[i, j] / 2);
                    inputMatrix[i, j] = (inputMatrix[i, j] *255);
          
                    outputMatrix[i, j] = (int)inputMatrix[i, j];
                }
            }
            
            return outputMatrix;
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
