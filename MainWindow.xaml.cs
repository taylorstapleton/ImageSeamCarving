﻿using Microsoft.Win32;
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

        public MainWindow()
        {
            InitializeComponent();
            this.context = new SeamCarvingContext();

            this.context.Height = ImageControl.Height;
            this.context.Width = ImageControl.Width;

            this.seamUtilities = new SeamUtilities();
            this.gradientCalculator = new GradientCalculator(seamUtilities);
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
        }

        /// <summary>
        /// on heat map button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            calculateHeat(context);
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
            calculateHeat(context);
            calculateSeam(context);
            display(context.imageDataArray, (int)ImageControl.Width, (int)ImageControl.Height, context);
            resize(context);
            context.Height--;
        }

        

        public void calculateHeat(SeamCarvingContext injectedContext)
        {
            injectedContext.energyArray = injectedContext.gradientArray;
            injectedContext.energy = new int[injectedContext.gradientArray.Length / 4];
            for (int i = 0; i < ImageControl.Height; i++)
            {
                byte current = seamUtilities.getPixel(injectedContext.gradientArray, i, 0, 0, injectedContext);
                this.seamUtilities.setIndex(injectedContext.energy, i, 0, (int)current, injectedContext);
            }

            for (int i = 1; i < ImageControl.Width - 1; i++)
            {
                for (int j = 0; j < ImageControl.Height; j++)
                {
                    if (j == 0 || j == (ImageControl.Height - 1))
                    {
                        this.seamUtilities.setIndex(injectedContext.energy, j, i, Int32.MaxValue, injectedContext);
                    }
                    else
                    {
                        int current = (int)seamUtilities.getPixel(injectedContext.gradientArray, j, i, 0, injectedContext);
                        int neg = this.seamUtilities.getIndex(injectedContext.energy, j - 1, i - 1, injectedContext);
                        int lat = this.seamUtilities.getIndex(injectedContext.energy, j, i - 1, injectedContext);
                        int pos = this.seamUtilities.getIndex(injectedContext.energy, j + 1, i - 1, injectedContext);
                        int least = (Math.Min(Math.Min(neg, lat), pos));
                        int toSet = current + least;
                        int maxVal = 255 * (int)ImageControl.Width;
                        this.seamUtilities.setIndex(injectedContext.energy, j, i, (current + least), injectedContext);

                        double ratio = (((double)toSet) / ((double)maxVal)) * 128;

                        byte pixelValue = (byte)(ratio * 256);

                        seamUtilities.setPixel(injectedContext.energyArray, j, i, 0, pixelValue, injectedContext);
                        seamUtilities.setPixel(injectedContext.energyArray, j, i, 1, pixelValue, injectedContext);
                        seamUtilities.setPixel(injectedContext.energyArray, j, i, 2, pixelValue, injectedContext);
                    }
                }
            }
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
