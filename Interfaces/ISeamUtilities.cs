using SeamCarving.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SeamCarving.Interfaces
{
    interface ISeamUtilities
    {
        void setPixel(byte[] arr, int x, int y, int color, byte toSet, SeamCarvingContext context);

        byte getPixel(byte[] arr, int x, int y, int color, SeamCarvingContext context);

        pixel getPixelInfo(int i, int j, byte[] imageDataArray, SeamCarvingContext injectedContext);

        BitmapImage createBitmapFromFilePath(string path);

        byte[] ImageToByte(BitmapImage toCopy, SeamCarvingContext injectedContext);

        int findMinIndex(int[] arr, SeamCarvingContext injectedContext);

        int getIndex(int[] arr, int x, int y, SeamCarvingContext injectedContext);

        void setIndex(int[] arr, int x, int y, int toSet, SeamCarvingContext injectedContext);
    }
}
