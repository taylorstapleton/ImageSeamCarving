using SeamCarving.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
