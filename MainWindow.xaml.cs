using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.Cache;
using SeamCarving.Interfaces;
using SeamCarving.Classes;

namespace SeamCarving
{
    /// new comment for commit
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[] imageDataArray;
        byte[] gradientArray;
        byte[] energyArray;
        BitmapImage newBitmapImage;
        int[] dirtyArray;
        int[] energy;
        int stride;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// on choose file button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IFileChooser fileChooser = new FileChooser();
            string path = string.Empty;

            if (fileChooser.getFile(out path) == true)
            {
                BitmapImage bitmap = createBitmapFromFilePath(path);

                imageDataArray = ImageToByte(bitmap);

                display(imageDataArray, (int)bitmap.Width, (int)bitmap.Height);
            }
        }

        /// <summary>
        /// on gradient button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            calculateGradient();
            display(gradientArray, (int)ImageControl.Width, (int)ImageControl.Height);
        }

        /// <summary>
        /// on heat map button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            calculateHeat();
            display(energyArray, (int)ImageControl.Width, (int)ImageControl.Height);
        }

        /// <summary>
        /// on seam carve button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //for (int i = 0; i < 3; i++)
            //{
            calculateGradient();
            calculateHeat();
            calculateSeam();
            display(imageDataArray, (int)ImageControl.Width, (int)ImageControl.Height);
            resize();
            //}
        }

        /// <summary>
        /// creates a bitmap image with the proper options set from the provided path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private BitmapImage createBitmapFromFilePath(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.CacheOption = BitmapCacheOption.None;
            bitmap.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();
            return bitmap;
        }


        public void calculateGradient()
        {
            gradientArray = new byte[imageDataArray.Length];
            for (int i = 1; i < ImageControl.Height - 1; i++)
            {
                for (int j = 1; j < ImageControl.Width - 1; j++)
                {
                    pixel last = getPixelInfo(i, j - 1);
                    pixel current = getPixelInfo(i, j);
                    pixel next = getPixelInfo(i, j + 1);

                    byte gradient = (byte)((((current.red - next.red + 1) * (current.blue - next.blue + 1) * (current.green - next.green + 1)) + last.red) / 128);


                    byte[] bytes = BitConverter.GetBytes(gradient);

                    //gradient = (byte)(255 - gradient);

                    setPixel(gradientArray, i, j, 0, gradient);
                    setPixel(gradientArray, i, j, 1, gradient);
                    setPixel(gradientArray, i, j, 2, gradient);
                    setPixel(gradientArray, i, j, 3, 0xff);
                }
            }
        }

        public byte[] ImageToByte(BitmapImage img)
        {
            stride = img.PixelWidth * 4;
            int size = img.PixelHeight * stride;
            byte[] pixels = new byte[size];
            img.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        public byte getPixel(byte[] arr, int x, int y, int color)
        {
            int index = (int)ImageControl.Width * 4 * x + y * 4;
            return arr[index + color];
        }

        public void setPixel(byte[] arr, int x, int y, int color , byte toSet)
        {
            int index = (int)ImageControl.Width * 4 * x + y * 4;
            arr[index + color] = toSet;
        }

        public void display(byte[] toDisplay, int width, int height)
        {
            Bitmap newBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapData bmData = newBitmap.LockBits(new System.Drawing.Rectangle(0, 0, newBitmap.Width, newBitmap.Height), ImageLockMode.ReadWrite, newBitmap.PixelFormat);
            IntPtr pNative = bmData.Scan0;
            Marshal.Copy(toDisplay, 0, pNative, width * height * 4);
            newBitmap.UnlockBits(bmData);

            newBitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                newBitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                newBitmapImage.BeginInit();
                newBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                newBitmapImage.StreamSource = memory;
                newBitmapImage.EndInit();
            }
            ImageControl.Width = width;
            Application.Current.MainWindow.Height = height + 400;
            Application.Current.MainWindow.Width = width + 600;
            
            ImageControl.Height = height;
            ImageControl.Source = newBitmapImage;

        }

        public pixel getPixelInfo(int i, int j)
        {
                    byte[] red = {0,0,0,getPixel(imageDataArray, i, j, 0)};
                    byte[] green = {0,0,0,getPixel(imageDataArray, i, j, 1)};
                    byte[] blue = {0,0,0,getPixel(imageDataArray, i, j, 2)};
                    byte[] alpha = {0,0,0,getPixel(imageDataArray, i, j, 3)};

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(red);
                        Array.Reverse(green);
                        Array.Reverse(blue);
                        Array.Reverse(alpha);
                    }

                    int redInt = BitConverter.ToInt32(red, 0);
                    int greenInt = BitConverter.ToInt32(green, 0);
                    int blueInt = BitConverter.ToInt32(blue, 0);
                    int alphaInt = BitConverter.ToInt32(alpha, 0);

                    pixel toReturn =  new pixel();
                    toReturn.red = redInt;
                    toReturn.green = greenInt;
                    toReturn.blue = blueInt;
                    toReturn.alpha = alphaInt;

                    return toReturn;
        }


        public class pixel
        {
            public int red{get; set;}
            public int green{get; set;}
            public int blue{get; set;}
            public int alpha{get; set;}
            
        }

        

        public void calculateHeat()
        {
            energyArray = gradientArray;
            energy = new int[gradientArray.Length / 4];
            for (int i = 0; i < ImageControl.Height; i++)
            {
                byte current = getPixel(gradientArray, i, 0, 0);
                setIndex(energy, i, 0, (int)current);
            }

            for (int i = 1; i < ImageControl.Width - 1; i++)
            {
                for (int j = 0; j < ImageControl.Height; j++)
                {
                    if (j == 0 || j == (ImageControl.Height - 1))
                    {
                        setIndex(energy, j, i, Int32.MaxValue);
                    }
                    else
                    {
                        int current = (int)getPixel(gradientArray, j, i, 0);
                        int neg = getIndex(energy, j - 1, i - 1);
                        int lat = getIndex(energy, j, i - 1);
                        int pos = getIndex(energy, j + 1, i - 1);
                        int least = (Math.Min(Math.Min(neg, lat), pos));
                        int toSet = current + least;
                        int maxVal = 255 * (int)ImageControl.Width;
                        setIndex(energy, j, i, (current + least));

                        double ratio = (((double)toSet) / ((double)maxVal)) * 128;

                        byte pixelValue = (byte)(ratio * 256);

                        setPixel(energyArray, j, i, 0, pixelValue);
                        setPixel(energyArray, j, i, 1, pixelValue);
                        setPixel(energyArray, j, i, 2, pixelValue);
                    }
                }
            }
        }

        void setIndex(int[] arr, int x , int y , int toSet)
        {
            int index = x * (int)ImageControl.Width + y;
            arr[index] = toSet;
        }

        int getIndex(int[] arr, int x , int y)
        {
            int index = x * (int)ImageControl.Width + y;
            return arr[index];
        }

        

        public void resize()
        {
            int newSize = ((int)ImageControl.Width - 1) * ((int)ImageControl.Height) * 4;
            byte[] newImage = new byte[newSize];

            int encountered = 0;
            for (int i = 0; i < ImageControl.Width; i++)
            {
                encountered = 0;
                for (int f = 0; f < ImageControl.Height - 1; f++)
                {
                    if (getIndex(dirtyArray, f, i) != 1)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            setPixel(newImage, f - encountered, i, k, getPixel(imageDataArray, f, i, k));
                        }
                    }
                    else
                    {
                        encountered = 1;
                    }
                }
            }
            ImageControl.Height--;
            imageDataArray = newImage;
        }

        public void calculateSeam()
        {
            dirtyArray = new int[energy.Length];

            int j = findMinIndex(energy);
            for (int i = (int)ImageControl.Width - 2; i >= 0; i--)
            {
                if (j == 0)
                {
                    j += 1;
                }
                int up = getIndex(energy, j + 1, i);
                int lat = getIndex(energy, j, i);
                int down = getIndex(energy, j - 1, i);

                if (up < lat && up < down)
                {
                    setPixel(imageDataArray, j, i, 2, 0xff);
                    setPixel(imageDataArray, j, i, 1, 0x00);
                    setPixel(imageDataArray, j, i, 0, 0x00);
                    setPixel(imageDataArray, j, i, 3, 0xff);
                    setIndex(dirtyArray, j, i, 1);
                    j = j + 1;
                }
                else if (lat < down && lat < up)
                {
                    setPixel(imageDataArray, j, i, 2, 0xff);
                    setPixel(imageDataArray, j, i, 1, 0x00);
                    setPixel(imageDataArray, j, i, 3, 0xff);
                    setPixel(imageDataArray, j, i, 0, 0x00);
                    setIndex(dirtyArray, j, i, 1);
                    j = j;
                }
                else
                {
                    setPixel(imageDataArray, j, i, 2, 0xff);
                    setPixel(imageDataArray, j, i, 3, 0xff);
                    setPixel(imageDataArray, j, i, 1, 0x00);
                    setPixel(imageDataArray, j, i, 0, 0x00);
                    setIndex(dirtyArray, j, i, 1);
                    j = j - 1;
                }
            }
        }

        public int findMinIndex(int[] arr)
        {
            int lowest = Int32.MaxValue;
            int toReturn = 0;
            for(int i = 0; i < ImageControl.Height; i++)
            {
                int current = getIndex(arr, i, (int)ImageControl.Width -2);
                if(current < lowest)
                {
                    lowest = current;
                    toReturn = i;
                }
            }
            return toReturn;
        }
    }
}
