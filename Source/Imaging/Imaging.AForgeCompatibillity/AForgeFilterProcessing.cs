﻿using Accord.Extensions;
using AForge.Imaging.Filters;
using System;
using System.Drawing;

namespace Accord.Extensions.Imaging
{
    public static class AForgeFilterProcessing
    {
        static AForgeFilterProcessing()
        { }

        /// <summary>
        /// Executes specified filter on an image (without using parallel processor).
        /// </summary>
        /// <param name="filter">AForge filter.</param>
        /// <param name="inPlace">Execute in place or not. Please use this switch correctly as some filters may not be processed correctly.</param>
        /// <returns>Processed image. In case <see cref="inPlace"/> is set to true, result is processed source image (can be discarded).</returns>
        public static Image<TColor, TDepth> ApplyFilter<TColor, TDepth, TFilter>(this Image<TColor, TDepth> img, TFilter filter, bool inPlace = false)
            where TColor: IColor
            where TDepth: struct
            where TFilter: IFilter
        {
            bool castOnly = img.CanCastToAForgeImage(); //we do not want to convert just cast
            if (castOnly == false)
                throw new Exception("AForge filters can not be applied to this image format!");

            Image<TColor, TDepth> dest = null;
            if (inPlace)
                dest = img;
            else
                dest = img.CopyBlank();

            //ToAforgeImage() will only cast image (cause the conversion path has been checked by CanCastToAForgeImage()
            filter.Apply(
                img.ToAForgeImage(copyAlways: false, failIfCannotCast: true), 
                dest.ToAForgeImage(copyAlways: false, failIfCannotCast: true)
                        );

            return dest;
        }

        /// <summary>
        /// Executes specified filter on an image (without using parallel processor). As destination image size may be different from source in-place filtering is not allowed.
        /// </summary>
        /// <param name="filter">AForge <see cref="BaseTransformationFilter"/>.</param>
        public static Image<TColor, TDepth> ApplyFilter<TColor, TDepth, TFilter>(this Image<TColor, TDepth> img, TFilter filter)
            where TColor : IColor
            where TDepth : struct
            where TFilter: BaseTransformationFilter
        {
            return ApplyFilter<TColor, TDepth, TFilter, TColor>(img, filter);
        }

        /// <summary>
        /// Executes specified filter on an image (without using parallel processor). As destination image size may be different from source in-place filtering is not allowed.
        /// </summary>
        /// <param name="filter">AForge <see cref="BaseTransformationFilter"/>.</param>
        public static Image<TDstColor, TDepth> ApplyFilter<TSrcColor, TDepth, TFilter, TDstColor>(this Image<TSrcColor, TDepth> img, TFilter filter)
            where TSrcColor : IColor
            where TDstColor: IColor
            where TDepth : struct
            where TFilter : BaseTransformationFilter
        {
            bool castOnly = img.CanCastToAForgeImage(); //we do not want to convert just cast
            if (castOnly == false)
                throw new Exception("AForge filters can not be applied to this image format!");

            //ToAforgeImage() will only cast image (cause the conversion path has been checked by CanCastOnlyToAForgeImage()
            var dest = filter.Apply(img.ToAForgeImage()).ToImage<TDstColor, TDepth>();
            return dest;
        }
    }
}
