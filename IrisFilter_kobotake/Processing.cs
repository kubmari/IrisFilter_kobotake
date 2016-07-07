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
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    System.Drawing.Color tempPixel = input.GetPixel(x, y);
                    imageArray[x, y] = tempPixel.R;

                }
            }

            return imageArray;
        }

        public static int[,] lbitmapToArray(Bitmap input)
        {
            int[,] imageArray = new int[input.Width, input.Height];
            Rectangle rect = new Rectangle(0, 0, input.Width, input.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                input.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, input.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * input.Height;
            byte[] rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            for(int i=0; i<input.Width; i++)
            {
                for(int j=0; j<input.Height; j++)
                {
                    int index = j + i * input.Width;
                    Console.WriteLine("Image Array[" + i + "," + j + "] gets value from rgbValue[" +index + "]= " + rgbValues[index] + "\n");
                    imageArray[i, j] = rgbValues[index];
                }
            }

            Console.WriteLine("RGB Vals:\n");

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Console.Write(rgbValues[j + i * input.Width] + " ");
                }
                Console.WriteLine("");
            }


            Console.WriteLine("Image Array:\n");

                for(int i=0; i<8; i++)
            {
                for(int j=0; j<8; j++)
                {
                    Console.Write(imageArray[i, j]+" ");
                }
                Console.WriteLine("");
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


        public static int[,] parConv3(int[,] input, int[,] kernel, int sizeX, int sizeY)
        {
            int[,] outputImage = new int[sizeX, sizeY];

            Parallel.For(1, sizeX - 1, x =>
              {
                  for(int y=1; y<sizeY-1; y++)
                  {
                      int resultPixel = 0;
                      for (int n = -1; n <= 1; n++)
                      {
                          for (int m = -1; m <= 1; m++)
                          {
                              resultPixel = resultPixel + (kernel[1 + n, 1 + m] * input[x + n, y + m]);

                          }
                      }
                      outputImage[x, y] = resultPixel;
                  }
              }
              );

            return outputImage;

        }


        public static int[,] parGradientMagnitude(int[,] horizontalGradient, int[,] verticalGradient, int sizeX, int sizeY)
        {
            int[,] outputGradientMagnitude = new int[sizeX, sizeY];

            Parallel.For(0, sizeX, x =>
            {
                for (int y=0; y< sizeY; y++)
                {
                    outputGradientMagnitude[x, y] = (int)(Math.Sqrt(Math.Pow((double)horizontalGradient[x, y], 2) + Math.Pow((double)verticalGradient[x, y], 2)));
                }
            });

            return outputGradientMagnitude;
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

            int[,] horizontalGradient = new int[sizeX, sizeY];
            int[,] verticalGradient = new int[sizeX, sizeY];
            int[,] outputGradientMagnitude = new int[sizeX, sizeY];
            Parallel.Invoke(() =>
            {
                horizontalGradient = parConv3(input, horizontalFilter, sizeX, sizeY);
            },
            ()=>
            {
                verticalGradient = parConv3(input, verticalFilter, sizeX, sizeY);
            });
   
            outputGradientMagnitude = parGradientMagnitude(horizontalGradient, verticalGradient, sizeX, sizeY);
            return outputGradientMagnitude;
        }

        //scales matrices with values between -255..255 to 0..255
        public static int[,] scaleValuesTo255(int[,] inputMatrix, int sizeX, int sizeY)
        {
            int[,] outputMatrix = new int[sizeX, sizeY];

            int minVal = inputMatrix.Cast<int>().Min();
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    outputMatrix[i, j] = inputMatrix[i, j] - minVal; //0..n  
                }
            }

            int maxVal = inputMatrix.Cast<int>().Max();
            for (int i=0; i< sizeX; i++)
            {
                for(int j=0; j< sizeY; j++)
                {
                    outputMatrix[i, j] = (outputMatrix[i, j]*255) / maxVal; //0-1 * 255 => 0-255 
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
