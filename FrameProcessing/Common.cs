using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AForge.Imaging.Filters;
using System.Drawing;

namespace FrameProcessing
{
    static class Common
    {

        /*
            Changing the color of the input image to black-and-white
            @parameters: Bitmap(input)
            @output: Bitmap
            */
        public static Bitmap GrauwertBild(Bitmap input, bool central = true)
        {
            int[] mHeight = new int[3];
                mHeight[0] = input.Height / 2;
                mHeight[1] = input.Height / 10;
                mHeight[2] = input.Height - input.Height / 10;

            Bitmap greyscale = new Bitmap(input.Width, input.Height);
            for (int h = 0; h < mHeight.Length; ++h)
            {
                for (int x = 0; x < input.Width; x++)
                {
                    if (central)
                    {
                        Common.greyPixel(ref greyscale, input, x, mHeight[h]);
                        continue;
                    }

                    for (int y = 0; y < input.Height; y++)
                        Common.greyPixel(ref greyscale, input, x, y);
                }
            }

            return greyscale;
        }


        /*
            Set a grey pixel to a Bitmap bm
            @parameters: Bitmap(bm), Bitmap(input), int(x), int(y)
            @output: Bitmap(bm)
            */
        private static void greyPixel(ref Bitmap bm, Bitmap input, int x, int y)
        {
            bm.SetPixel(x, y, Common.pixelToGreenScale(input.GetPixel(x, y)));
        }

        /*
            Get a while-black pixel from colorful one
            @parameters: Color(input)
            @output: Color
            */
        public static Color pixelToGreenScale(Color input)
        {
            int grey = (int)(input.R * 0.3 + input.G * 0.59 + input.B * 0.11);
            return Color.FromArgb(input.A, grey, grey, grey);
        }

        /*
            Generating a file name
            @parameters: string(startPart)
            @output: string
            */
        public static string generateNameForTheFile(string startPart = "generatedlog_")
        {
            for (int i = 0; i < 100; ++i)
            {
                string n = startPart + i + ".txt";
                if (!System.IO.File.Exists(n))
                    return n;
            }

            return startPart + ".txt";
        }

        /*  
            Image cutter
            @parameters: Bitmap(input), char(cutType)
            @output: Bitmap
            */
        public static Bitmap imageCutter(Bitmap input, int cutType)
        {
            switch(cutType)
            {
                default:
                    /*
                        In the default method the cropped image is the middle part of the input image,
                        where there's only 5% of the height with full width.
                        */

                    int new_height = input.Size.Height / 10; // 10% of the image height
                    int new_y = input.Size.Height / 2 - new_height / 2;

                    // Get filter to apply it on the image
                    Crop filter = new Crop(new Rectangle(0, new_y, input.Size.Width, new_height));
                    Bitmap output = filter.Apply(input); input.Dispose();

                    return output; 
            }
        }
    }
}
