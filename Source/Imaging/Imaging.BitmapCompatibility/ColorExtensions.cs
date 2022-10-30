﻿using Accord.Extensions.Imaging.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accord.Extensions.Imaging
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Gets System.Drawing.Color from TColor
        /// </summary>
        /// <typeparam name="TColor">Member of IColor</typeparam>
        /// <param name="color">Color.</param>
        /// <param name="opacity">Opacity. If color has 4 channels opacity is discarded.</param>
        /// <returns>System.Drawing.Color</returns>
        public static System.Drawing.Color ToColor<TColor>(this TColor color, byte opacity = Byte.MaxValue)
            where TColor : IColor
        {
            int[] colorArr = HelperMethods.ColorToArray<TColor, int>(color);
            correctValueMapping<TColor>(ref colorArr);

            switch (colorArr.Length)
            {
                case 1:
                    return Color.FromArgb(opacity, 0, colorArr[0]);
                case 2:
                    return Color.FromArgb(opacity, colorArr[0], colorArr[1]);
                case 3:
                    return Color.FromArgb(opacity, colorArr[0], colorArr[1], colorArr[2]);
                case 4:
                    return Color.FromArgb(colorArr[0], colorArr[1], colorArr[2], colorArr[3]);
            }

            throw new Exception("Unknown color model!");
        }

        private static void correctValueMapping<TColor>(ref int[] colorArr)
             where TColor : IColor
        {
            if (ColorInfo.GetInfo<TColor, double>().ConversionCodename == "BGR") //TODO (priority: lowest): other way to do that (without harcoding) - converters ?
            {
                var temp = colorArr[0];
                colorArr[0] = colorArr[2];
                colorArr[2] = temp;
            }
        }

        public static Bgr32 ToBgr(this System.Drawing.Color color)
        {
            return new Bgr32 { B = color.B, G = color.G, R = color.R };
        }
    }
}
