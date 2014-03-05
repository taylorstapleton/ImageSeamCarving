using SeamCarving.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SeamCarving.Classes
{
    public class SeamUtilities : ISeamUtilities
    {
        /// <summary>
        /// sets the pixel value in the given byte array at location (x,y) with the given color to the given value.
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        /// <param name="toSet"></param>
        public void setPixel(byte[] arr, int x, int y, int color, byte toSet, SeamCarvingContext context)
        {
            int index = (int)context.Width * 4 * x + y * 4;
            arr[index + color] = toSet;
        }

        /// <summary>
        /// gets the pixel value from a given byte array at location (x,y) with the given color.
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public byte getPixel(byte[] arr, int x, int y, int color, SeamCarvingContext context)
        {
            int index = (int)context.Width * 4 * x + y * 4;
            return arr[index + color];
        }

        /// <summary>
        /// given a location of a pixel (x,y), returns the info about that pixel.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="imageDataArray"></param>
        /// <param name="injectedContext"></param>
        /// <returns></returns>
        public pixel getPixelInfo(int i, int j, byte[] imageDataArray, SeamCarvingContext injectedContext)
        {
            byte[] red = { 0, 0, 0, this.getPixel(imageDataArray, i, j, 0, injectedContext) };
            byte[] green = { 0, 0, 0, this.getPixel(imageDataArray, i, j, 1, injectedContext) };
            byte[] blue = { 0, 0, 0, this.getPixel(imageDataArray, i, j, 2, injectedContext) };
            byte[] alpha = { 0, 0, 0, this.getPixel(imageDataArray, i, j, 3, injectedContext) };

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

            pixel toReturn = new pixel();
            toReturn.red = redInt;
            toReturn.green = greenInt;
            toReturn.blue = blueInt;
            toReturn.alpha = alphaInt;

            return toReturn;
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

        /// <summary>
        /// takes a given bitmapImage and copies it into a new byte array and returns it.
        /// </summary>
        /// <param name="img"></param>
        /// <param name="injectedContext"></param>
        /// <returns></returns>
        public byte[] ImageToByte(BitmapImage toCopy, SeamCarvingContext injectedContext)
        {
            injectedContext.stride = toCopy.PixelWidth * 4;
            int size = toCopy.PixelHeight * injectedContext.stride;
            byte[] pixels = new byte[size];
            toCopy.CopyPixels(pixels, injectedContext.stride, 0);
            return pixels;
        }
    }
}
