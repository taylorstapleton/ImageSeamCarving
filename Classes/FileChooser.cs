using Microsoft.Win32;
using SeamCarving.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeamCarving.Classes
{
    public class FileChooser : IFileChooser
    {
        /// <summary>
        /// system FileDialog hook
        /// </summary>
        private OpenFileDialog fileDialog;

        /// <summary>
        /// constructor
        /// </summary>
        public FileChooser()
        {
            fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
        }

        /// <summary>
        /// used to choose a file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool getFile(out string path)
        {
            if (fileDialog.ShowDialog() == true)
            {
                path = fileDialog.FileName;
                return true;
            }
            path = string.Empty;
            return false;
        }
    }
}
