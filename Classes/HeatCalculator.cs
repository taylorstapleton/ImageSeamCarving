using SeamCarving.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeamCarving.Classes
{
    class HeatCalculator : IHeatCalculator
    {
        #region class variables
        /// <summary>
        /// seam utils
        /// </summary>
        ISeamUtilities seamUtilities;
        #endregion

        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="injectedUtilities"></param>
        public HeatCalculator(ISeamUtilities injectedUtilities)
        {
            this.seamUtilities = injectedUtilities;
        }
        #endregion

        #region heat calculation methods
        /// <summary>
        /// calculates the heatmap of the given seam carving context
        /// </summary>
        /// <param name="injectedContext"></param>
        public void calculateHeat(SeamCarvingContext injectedContext)
        {
            injectedContext.energyArray = injectedContext.gradientArray;
            injectedContext.energy = new int[injectedContext.gradientArray.Length / 4];
            for (int i = 0; i < injectedContext.Height; i++)
            {
                byte current = this.seamUtilities.getPixel(injectedContext.gradientArray, i, 0, 0, injectedContext);
                this.seamUtilities.setIndex(injectedContext.energy, i, 0, (int)current, injectedContext);
            }

            for (int i = 1; i < injectedContext.Width - 1; i++)
            {
                for (int j = 0; j < injectedContext.Height; j++)
                {
                    if (j == 0 || j == (injectedContext.Height - 1))
                    {
                        this.seamUtilities.setIndex(injectedContext.energy, j, i, Int32.MaxValue, injectedContext);
                    }
                    else
                    {
                        int current = (int)this.seamUtilities.getPixel(injectedContext.gradientArray, j, i, 0, injectedContext);
                        int neg = this.seamUtilities.getIndex(injectedContext.energy, j - 1, i - 1, injectedContext);
                        int lat = this.seamUtilities.getIndex(injectedContext.energy, j, i - 1, injectedContext);
                        int pos = this.seamUtilities.getIndex(injectedContext.energy, j + 1, i - 1, injectedContext);
                        int least = (Math.Min(Math.Min(neg, lat), pos));
                        int toSet = current + least;
                        int maxVal = 255 * (int)injectedContext.Width;
                        this.seamUtilities.setIndex(injectedContext.energy, j, i, (current + least), injectedContext);

                        double ratio = (((double)toSet) / ((double)maxVal)) * 128;

                        byte pixelValue = (byte)(ratio * 256);

                        this.seamUtilities.setPixel(injectedContext.energyArray, j, i, 0, pixelValue, injectedContext);
                        this.seamUtilities.setPixel(injectedContext.energyArray, j, i, 1, pixelValue, injectedContext);
                        this.seamUtilities.setPixel(injectedContext.energyArray, j, i, 2, pixelValue, injectedContext);
                    }
                }
            }
        }
        #endregion
    }
}
