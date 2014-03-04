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
        SeamCarvingContext context;

        public MainWindow()
        {
            InitializeComponent();
            context = new SeamCarvingContext();
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

                context.imageDataArray = ImageToByte(bitmap, context);

                display(context.imageDataArray, (int)bitmap.Width, (int)bitmap.Height, context);
            }
        }

        /// <summary>
        /// on gradient button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            calculateGradient(context);
            display(context.gradientArray, (int)ImageControl.Width, (int)ImageControl.Height, context);
        }

        /// <summary>
        /// on heat map button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            calculateHeat(context);
            display(context.energyArray, (int)ImageControl.Width, (int)ImageControl.Height, context);
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
            calculateGradient(context);
            calculateHeat(context);
            calculateSeam(context);
            display(context.imageDataArray, (int)ImageControl.Width, (int)ImageControl.Height, context);
            resize(context);
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


        public void calculateGradient(SeamCarvingContext injectedContext)
        {
            injectedContext.gradientArray = new byte[injectedContext.imageDataArray.Length];
            for (int i = 1; i < ImageControl.Height - 1; i++)
            {
                for (int j = 1; j < ImageControl.Width - 1; j++)
                {
                    pixel last = getPixelInfo(i, j - 1, injectedContext.imageDataArray);
                    pixel current = getPixelInfo(i, j, injectedContext.imageDataArray);
                    pixel next = getPixelInfo(i, j + 1, injectedContext.imageDataArray);

                    byte gradient = (byte)((((current.red - next.red + 1) * (current.blue - next.blue + 1) * (current.green - next.green + 1)) + last.red) / 128);

                    byte[] bytes = BitConverter.GetBytes(gradient);

                    setPixel(injectedContext.gradientArray, i, j, 0, gradient);
                    setPixel(injectedContext.gradientArray, i, j, 1, gradient);
                    setPixel(injectedContext.gradientArray, i, j, 2, gradient);
                    setPixel(injectedContext.gradientArray, i, j, 3, 0xff);
                }
            }
        }

        public byte[] ImageToByte(BitmapImage img, SeamCarvingContext injectedContext)
        {
            injectedContext.stride = img.PixelWidth * 4;
            int size = img.PixelHeight * injectedContext.stride;
            byte[] pixels = new byte[size];
            img.CopyPixels(pixels, injectedContext.stride, 0);
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

        public void display(byte[] toDisplay, int width, int height, SeamCarvingContext injectedContext)
        {
            Bitmap newBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapData bmData = newBitmap.LockBits(new System.Drawing.Rectangle(0, 0, newBitmap.Width, newBitmap.Height), ImageLockMode.ReadWrite, newBitmap.PixelFormat);
            IntPtr pNative = bmData.Scan0;
            Marshal.Copy(toDisplay, 0, pNative, width * height * 4);
            newBitmap.UnlockBits(bmData);

            injectedContext.newBitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                newBitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                injectedContext.newBitmapImage.BeginInit();
                injectedContext.newBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                injectedContext.newBitmapImage.StreamSource = memory;
                injectedContext.newBitmapImage.EndInit();
            }
            ImageControl.Width = width;
            Application.Current.MainWindow.Height = height + 400;
            Application.Current.MainWindow.Width = width + 600;
            
            ImageControl.Height = height;
            ImageControl.Source = injectedContext.newBitmapImage;

        }

        public pixel getPixelInfo(int i, int j,  byte[] imageDataArray)
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

        public void calculateHeat(SeamCarvingContext injectedContext)
        {
            injectedContext.energyArray = context.gradientArray;
            injectedContext.energy = new int[injectedContext.gradientArray.Length / 4];
            for (int i = 0; i < ImageControl.Height; i++)
            {
                byte current = getPixel(injectedContext.gradientArray, i, 0, 0);
                setIndex(injectedContext.energy, i, 0, (int)current);
            }

            for (int i = 1; i < ImageControl.Width - 1; i++)
            {
                for (int j = 0; j < ImageControl.Height; j++)
                {
                    if (j == 0 || j == (ImageControl.Height - 1))
                    {
                        setIndex(injectedContext.energy, j, i, Int32.MaxValue);
                    }
                    else
                    {
                        int current = (int)getPixel(injectedContext.gradientArray, j, i, 0);
                        int neg = getIndex(injectedContext.energy, j - 1, i - 1);
                        int lat = getIndex(injectedContext.energy, j, i - 1);
                        int pos = getIndex(injectedContext.energy, j + 1, i - 1);
                        int least = (Math.Min(Math.Min(neg, lat), pos));
                        int toSet = current + least;
                        int maxVal = 255 * (int)ImageControl.Width;
                        setIndex(injectedContext.energy, j, i, (current + least));

                        double ratio = (((double)toSet) / ((double)maxVal)) * 128;

                        byte pixelValue = (byte)(ratio * 256);

                        setPixel(injectedContext.energyArray, j, i, 0, pixelValue);
                        setPixel(injectedContext.energyArray, j, i, 1, pixelValue);
                        setPixel(injectedContext.energyArray, j, i, 2, pixelValue);
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

        

        public void resize(SeamCarvingContext injectedContext)
        {
            int newSize = ((int)ImageControl.Width - 1) * ((int)ImageControl.Height) * 4;
            byte[] newImage = new byte[newSize];

            int encountered = 0;
            for (int i = 0; i < ImageControl.Width; i++)
            {
                encountered = 0;
                for (int f = 0; f < ImageControl.Height - 1; f++)
                {
                    if (getIndex(injectedContext.dirtyArray, f, i) != 1)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            setPixel(newImage, f - encountered, i, k, getPixel(injectedContext.imageDataArray, f, i, k));
                        }
                    }
                    else
                    {
                        encountered = 1;
                    }
                }
            }
            ImageControl.Height--;
            injectedContext.imageDataArray = newImage;
        }

        public void calculateSeam(SeamCarvingContext injectedContext)
        {
            injectedContext.dirtyArray = new int[injectedContext.energy.Length];

            int j = findMinIndex(injectedContext.energy);
            for (int i = (int)ImageControl.Width - 2; i >= 0; i--)
            {
                if (j == 0)
                {
                    j += 1;
                }
                int up = getIndex(injectedContext.energy, j + 1, i);
                int lat = getIndex(injectedContext.energy, j, i);
                int down = getIndex(injectedContext.energy, j - 1, i);

                if (up < lat && up < down)
                {
                    setPixel(injectedContext.imageDataArray, j, i, 2, 0xff);
                    setPixel(injectedContext.imageDataArray, j, i, 1, 0x00);
                    setPixel(injectedContext.imageDataArray, j, i, 0, 0x00);
                    setPixel(injectedContext.imageDataArray, j, i, 3, 0xff);
                    setIndex(injectedContext.dirtyArray, j, i, 1);
                    j = j + 1;
                }
                else if (lat < down && lat < up)
                {
                    setPixel(injectedContext.imageDataArray, j, i, 2, 0xff);
                    setPixel(injectedContext.imageDataArray, j, i, 1, 0x00);
                    setPixel(injectedContext.imageDataArray, j, i, 3, 0xff);
                    setPixel(injectedContext.imageDataArray, j, i, 0, 0x00);
                    setIndex(injectedContext.dirtyArray, j, i, 1);
                    j = j;
                }
                else
                {
                    setPixel(injectedContext.imageDataArray, j, i, 2, 0xff);
                    setPixel(injectedContext.imageDataArray, j, i, 3, 0xff);
                    setPixel(injectedContext.imageDataArray, j, i, 1, 0x00);
                    setPixel(injectedContext.imageDataArray, j, i, 0, 0x00);
                    setIndex(injectedContext.dirtyArray, j, i, 1);
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
