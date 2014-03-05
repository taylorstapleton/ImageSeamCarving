using SeamCarving.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeamCarving.Classes
{
    class GradientCalculator : IGradientCalculator
    {
        ISeamUtilities seamUtilities;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="injectedSeamUtilities"></param>
        public GradientCalculator(ISeamUtilities injectedSeamUtilities)
        {
            this.seamUtilities = injectedSeamUtilities;
        }

        /// <summary>
        /// given a seam carving context, calculates the gradient of every pixel.
        /// </summary>
        /// <param name="injectedContext"></param>
        public void calculateGradient(SeamCarvingContext injectedContext)
        {
            injectedContext.gradientArray = new byte[injectedContext.imageDataArray.Length];
            for (int i = 1; i < injectedContext.Height - 1; i++)
            {
                for (int j = 1; j < injectedContext.Width - 1; j++)
                {
                    pixel last = seamUtilities.getPixelInfo(i, j - 1, injectedContext.imageDataArray, injectedContext);
                    pixel current = seamUtilities.getPixelInfo(i, j, injectedContext.imageDataArray, injectedContext);
                    pixel next = seamUtilities.getPixelInfo(i, j + 1, injectedContext.imageDataArray, injectedContext);

                    byte gradient = calculateGradientOfPixel(last, current, next);

                    seamUtilities.setPixel(injectedContext.gradientArray, i, j, 0, gradient, injectedContext);
                    seamUtilities.setPixel(injectedContext.gradientArray, i, j, 1, gradient, injectedContext);
                    seamUtilities.setPixel(injectedContext.gradientArray, i, j, 2, gradient, injectedContext);
                    seamUtilities.setPixel(injectedContext.gradientArray, i, j, 3, 0xff, injectedContext);
                }
            }
        }

        /// <summary>
        /// takes three pixels and calculates the current pixels difference from the pixels next to it.
        /// </summary>
        /// <param name="last"></param>
        /// <param name="current"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public byte calculateGradientOfPixel(pixel last, pixel current, pixel next)
        {
            return (byte)((((current.red - next.red + 1) * (current.blue - next.blue + 1) * (current.green - next.green + 1)) + last.red) / 128);
        }
    }
}
