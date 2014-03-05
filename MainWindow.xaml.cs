using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.Cache;
using SeamCarving.Interfaces;
using SeamCarving.Classes;

namespace SeamCarving
{
    /// new comment for commit
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SeamCarvingContext context;
        private ISeamUtilities seamUtilities;
        private IGradientCalculator gradientCalculator;
        private IHeatCalculator heatCalculator;

        public MainWindow()
        {
            InitializeComponent();
            this.context = new SeamCarvingContext();

            this.context.Height = ImageControl.Height;
            this.context.Width = ImageControl.Width;

            this.seamUtilities = new SeamUtilities();
            this.gradientCalculator = new GradientCalculator(seamUtilities);
            this.heatCalculator = new HeatCalculator(seamUtilities);
        }

        /// <summary>
        /// on choose file button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IFileChooser fileChooser = new FileChooser();
            string path = string.Empty;

            if (fileChooser.getFile(out path) == true)
            {
                BitmapImage bitmap = this.seamUtilities.createBitmapFromFilePath(path);

                this.context.Height = bitmap.PixelHeight;
                this.context.Width = bitmap.PixelWidth;

                this.context.imageDataArray = this.seamUtilities.ImageToByte(bitmap, context);

                display(context.imageDataArray, (int)bitmap.Width, (int)bitmap.Height, context);
            }
        }

        /// <summary>
        /// on gradient button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.gradientCalculator.calculateGradient(context);
            display(context.gradientArray, (int)ImageControl.Width, (int)ImageControl.Height, context);
            var test = context.gradientArray.Max();
        }

        /// <summary>
        /// on heat map button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.heatCalculator.calculateHeat(context);
            display(context.energyArray, (int)ImageControl.Width, (int)ImageControl.Height, context);
        }

        /// <summary>
        /// on seam carve button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.gradientCalculator.calculateGradient(context);
            this.heatCalculator.calculateHeat(context);
            calculateSeam(context);
            display(context.imageDataArray, (int)ImageControl.Width, (int)ImageControl.Height, context);
            resize(context);
            context.Height--;
        }



        public void resize(SeamCarvingContext injectedContext)
        {
            int newSize = ((int)ImageControl.Width - 1) * ((int)ImageControl.Height) * 4;
            byte[] newImage = new byte[newSize];

            int encountered = 0;
            for (int i = 0; i < ImageControl.Width; i++)
            {
                encountered = 0;
                for (int f = 0; f < ImageControl.Height - 1; f++)
                {
                    if (this.seamUtilities.getIndex(injectedContext.dirtyArray, f, i, injectedContext) != 1)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            seamUtilities.setPixel(newImage, f - encountered, i, k, seamUtilities.getPixel(injectedContext.imageDataArray, f, i, k, injectedContext), injectedContext);
                        }
                    }
                    else
                    {
                        encountered = 1;
                    }
                }
            }
            ImageControl.Height--;
            injectedContext.imageDataArray = newImage;
        }

        public void calculateSeam(SeamCarvingContext injectedContext)
        {
            injectedContext.dirtyArray = new int[injectedContext.energy.Length];

            int j = this.seamUtilities.findMinIndex(injectedContext.energy, injectedContext);
            for (int i = (int)ImageControl.Width - 2; i >= 0; i--)
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
                    j = j;
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

        



        public void display(byte[] toDisplay, int width, int height, SeamCarvingContext injectedContext)
        {
            Bitmap newBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapData bmData = newBitmap.LockBits(new System.Drawing.Rectangle(0, 0, newBitmap.Width, newBitmap.Height), ImageLockMode.ReadWrite, newBitmap.PixelFormat);
            IntPtr pNative = bmData.Scan0;
            Marshal.Copy(toDisplay, 0, pNative, width * height * 4);
            newBitmap.UnlockBits(bmData);

            injectedContext.newBitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                newBitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                injectedContext.newBitmapImage.BeginInit();
                injectedContext.newBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                injectedContext.newBitmapImage.StreamSource = memory;
                injectedContext.newBitmapImage.EndInit();
            }
            ImageControl.Width = width;
            Application.Current.MainWindow.Height = height + 400;
            Application.Current.MainWindow.Width = width + 600;

            ImageControl.Height = height;
            ImageControl.Source = injectedContext.newBitmapImage;
        }
    }
}
