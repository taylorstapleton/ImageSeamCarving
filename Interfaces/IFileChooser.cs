using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SeamCarving.Interfaces
{
    public interface IFileChooser
    {
        /// <summary>
        /// returns true if open file is successful and has path filled with result.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool getFile(out string path);
    }
}
