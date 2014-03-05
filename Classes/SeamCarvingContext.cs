using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SeamCarving.Classes
{
    public class SeamCarvingContext
    {
        #region class variables
        /// <summary>
        /// current image data
        /// meant to be 4 bytes per pixel
        /// </summary>
        public byte[] imageDataArray;

        /// <summary>
        /// an array of the gradient values
        /// meant to be 4 bytes per pixel
        /// </summary>
        public byte[] gradientArray;

        /// <summary>
        /// an array of the dynamic energy values to be displayed
        /// </summary>
        public byte[] energyArray;

        /// <summary>
        /// an array of integer values {0|1} denoting if the designated array spot is dirty
        /// and needs to be removed
        /// </summary>
        public int[] dirtyArray;

        /// <summary>
        /// integer values of the energy. not for displaying.
        /// </summary>
        public int[] energy;

        /// <summary>
        /// storing the stride.
        /// </summary>
        public int stride;

        /// <summary>
        /// storage for the new bitmap image to be resized.
        /// </summary>
        public BitmapImage newBitmapImage;

        /// <summary>
        /// Height of the current scenario
        /// </summary>
        public double Height;

        /// <summary>
        /// Width of the current scenario
        /// </summary>
        public double Width;
        #endregion
    }
}
