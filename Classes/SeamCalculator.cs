using SeamCarving.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeamCarving.Classes
{
    public class SeamCalculator : ISeamCalculator
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
        /// <param name="injectedSeamUtilities"></param>
        public SeamCalculator(ISeamUtilities injectedSeamUtilities)
        {
            this.seamUtilities = injectedSeamUtilities;
        }
        #endregion

        #region seam calculation methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="injectedContext"></param>
        public void calculateSeam(SeamCarvingContext injectedContext)
        {
            injectedContext.dirtyArray = new int[injectedContext.energy.Length];

            int j = this.seamUtilities.findMinIndex(injectedContext.energy, injectedContext);
            for (int i = (int)injectedContext.Width - 2; i >= 0; i--)
            {
                if (j == 0)
                {
                    j += 1;
                }
                int up = this.seamUtilities.getIndex(injectedContext.energy, j + 1, i, injectedContext);
                int lat = this.seamUtilities.getIndex(injectedContext.energy, j, i, injectedContext);
                int down = this.seamUtilities.getIndex(injectedContext.energy, j - 1, i, injectedContext);

                if (up < lat && up < down)
                {
                    seamUtilities.setPixel(injectedContext.imageDataArray, j, i, 2, 0xff, injectedContext);
                    seamUtilities.setPixel(injectedContext.imageDataArray, j, i, 1, 0x00, injectedContext);
                    seamUtilities.setPixel(injectedContext.imageDataArray, j, i, 0, 0x00, injectedContext);
                    seamUtilities.setPixel(injectedContext.imageDataArray, j, i, 3, 0xff, injectedContext);
                    this.seamUtilities.setIndex(injectedContext.dirtyArray, j, i, 1, injectedContext);
                    j = j + 1;
                }
                else if (lat < down && lat < up)
                {
                    seamUtilities.setPixel(injectedContext.imageDataArray, j, i, 2, 0xff, injectedContext);
                    seamUtilities.setPixel(injectedContext.imageDataArray, j, i, 1, 0x00, injectedContext);
                    seamUtilities.setPixel(injectedContext.imageDataArray, j, i, 3, 0xff, injectedContext);
                    seamUtilities.setPixel(injectedContext.imageDataArray, j, i, 0, 0x00, injectedContext);
                    this.seamUtilities.setIndex(injectedContext.dirtyArray, j, i, 1, injectedContext);
                }
                else
                {
                    seamUtilities.setPixel(injectedContext.imageDataArray, j, i, 2, 0xff, injectedContext);
                    seamUtilities.setPixel(injectedContext.imageDataArray, j, i, 3, 0xff, injectedContext);
                    seamUtilities.setPixel(injectedContext.imageDataArray, j, i, 1, 0x00, injectedContext);
                    seamUtilities.setPixel(injectedContext.imageDataArray, j, i, 0, 0x00, injectedContext);
                    this.seamUtilities.setIndex(injectedContext.dirtyArray, j, i, 1, injectedContext);
                    j = j - 1;
                }
            }
        }
        #endregion
    }
}
