﻿using System;
using Accord.Extensions;
using Accord.Extensions.Imaging;
using Accord.Extensions.Imaging.Filters;

namespace Accord.Extensions.Imaging.Algorithms.LINE2D
{
    /// <summary>
    /// Image template pyramid. See <see cref="ImageTemplate"/> for details.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ImageTemplatePyramid<T>: ITemplatePyramid, ITemplatePyramid<T>
        where T: ImageTemplate, new()
    {
        /*
        /// <summary>
        /// Minimum number of features per template.
        /// </summary>
        static int DEFAULT_MIN_FEATURES = 30; //not used - suppress the compiler warning
         */

        /// <summary>
        /// Maximum number of features per pyramid level.
        /// </summary>
        static int[] DEFAULT_MAX_FEATURES_PER_LEVEL = new int[] { 100/*, 100 / 2*/ }; //bigger image towards smaller one   

        /// <summary>
        /// Gets the image templates. One for each pyramid level.
        /// </summary>
        public T[] Templates { get; private set; }

        /// <summary>
        /// Creates an empty image template pyramid. Requires initialization.
        /// </summary>
        public ImageTemplatePyramid() { }

        /// <summary>
        /// Initialized image template pyramid with the provided templates.
        /// </summary>
        /// <param name="templates"></param>
        public void Initialize(T[] templates)
        { 
            this.Templates = templates;
        }

        /// <summary>
        /// Creates image templates pyramid from the input image by using the provided parameters.
        /// </summary>
        /// <param name="sourceImage">Input image.</param>
        /// <param name="classLabel">Templates class label.</param>
        /// <param name="minFeatureStrength">Minimum feature's gradient strength.</param>
        /// <param name="minNumberOfFeatures">Minimum number of features. If the number of features is less that minimum null will be returned.</param>
        /// <param name="maxNumberOfFeaturesPerLevel">Maximum number of features per template. The features will be scattered semi-uniformly.</param>
        /// <returns>Image template pyramid.</returns>
        public static ImageTemplatePyramid<T> CreatePyramid(Image<Bgr, Byte> sourceImage, string classLabel, int minFeatureStrength = 40, int minNumberOfFeatures = 30, int[] maxNumberOfFeaturesPerLevel = null)
        { 
            return CreatePyramid<Bgr, byte>(sourceImage, classLabel,
                                            (image, minFeatures, maxFeatures, label) => 
                                            {
                                                var t = new T();
                                                t.Initialize(sourceImage, minFeatureStrength, maxFeatures, label);
                                                return t;
                                            },
                                            minNumberOfFeatures, maxNumberOfFeaturesPerLevel);
        }

        /// <summary>
        /// Creates image templates pyramid from the black-white image by using the provided parameters.
        /// </summary>
        /// <param name="sourceImage">Input image.</param>
        /// <param name="classLabel">Templates class label.</param>
        /// <param name="minFeatureStrength">Minimum feature's gradient strength.</param>
        /// <param name="minNumberOfFeatures">Minimum number of features. If the number of features is less that minimum null will be returned.</param>
        /// <param name="maxNumberOfFeaturesPerLevel">Maximum number of features per template. The features will be scattered semi-uniformly.</param>
        /// <returns>Image template pyramid.</returns>
        public static ImageTemplatePyramid<T> CreatePyramidFromPreparedBWImage(Image<Gray, Byte> sourceImage, string classLabel, int minFeatureStrength = 40, int minNumberOfFeatures = 30, int[] maxNumberOfFeaturesPerLevel = null)
        {
            return CreatePyramid<Gray, byte>(sourceImage, classLabel,
                                            (image, minFeatures, maxFeatures, label) =>
                                            {
                                                var t = new T();
                                                t.Initialize(sourceImage, minFeatureStrength, maxFeatures, label);
                                                return t;
                                            },
                                            minNumberOfFeatures, maxNumberOfFeaturesPerLevel);
        }

        private static ImageTemplatePyramid<T> CreatePyramid<TColor, TDepth>(Image<TColor, TDepth> sourceImage, string classLabel, 
                                                                             Func<Image<TColor, TDepth>, int, int, string, T> templateCreationFunc,
                                                                             int minNumberOfFeatures, int[] maxNumberOfFeaturesPerLevel = null)
            where TColor: IColor
            where TDepth: struct
        {
            maxNumberOfFeaturesPerLevel = maxNumberOfFeaturesPerLevel ?? DEFAULT_MAX_FEATURES_PER_LEVEL;

            int nPyramids = maxNumberOfFeaturesPerLevel.Length;
            T[] templates = new T[nPyramids];
            var image = sourceImage;

            bool isValid = true;
            for (int pyrLevel = 0; pyrLevel < nPyramids; pyrLevel++)
            {
                if (pyrLevel > 0)
                    image = image.PyrDown();

                var newTemplate = templateCreationFunc(image, minNumberOfFeatures, maxNumberOfFeaturesPerLevel[pyrLevel], classLabel);
                templates[pyrLevel] = newTemplate;

                if (templates[pyrLevel].Features.Length < minNumberOfFeatures) //if there is no enough features mark Pyramid as invalid
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                var pyr = new ImageTemplatePyramid<T>(); pyr.Initialize(templates);
                return pyr;
            }
            else
                return null;
        }

        #region ITemplatePyramid Interface

        ITemplate[] ITemplatePyramid.Templates
        {
            get
            {
                return this.Templates;
            }
        }

        #endregion
    }
}
