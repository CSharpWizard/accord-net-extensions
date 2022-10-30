﻿using System.Collections.Generic;
using Accord.Imaging;
using Point = AForge.IntPoint;

namespace Accord.Extensions.Imaging
{
    /// <summary>
    /// Border following extensions.
    /// </summary>
    public static class BorderFollowingExtensions
    {
        /// <summary>
        /// Extracts the contour from a single object in a grayscale image. (uses Accord built-in function)
        /// </summary>
        /// <param name="im">Image.</param>
        /// <param name="minGradientStrength">The pixel value threshold above which a pixel
        /// is considered black (belonging to the object). Default is zero.</param>
        public static List<Point> FindContour(this Image<Gray, byte> im, byte minGradientStrength = 0)
        {
            BorderFollowing bf = new BorderFollowing(minGradientStrength); 
            return bf.FindContour(im.ToAForgeImage(copyAlways: false, failIfCannotCast: true));
        }
    }
}
