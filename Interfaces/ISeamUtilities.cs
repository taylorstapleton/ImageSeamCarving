using SeamCarving.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeamCarving.Interfaces
{
    interface ISeamUtilities
    {
        void setPixel(byte[] arr, int x, int y, int color, byte toSet, SeamCarvingContext context);

        byte getPixel(byte[] arr, int x, int y, int color, SeamCarvingContext context);

        pixel getPixelInfo(int i, int j, byte[] imageDataArray, SeamCarvingContext injectedContext);
    }
}
