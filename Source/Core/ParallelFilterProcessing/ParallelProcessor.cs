﻿using System;
using System.Collections.Generic;
using Accord.Extensions;
using System.Linq;
using System.Threading.Tasks;
using Point = AForge.IntPoint;

namespace Accord.Extensions
{
    /// <summary>
    /// Represents options for parallel patch computing.
    /// </summary>
    public class ParallelOptions
    {
        /// <summary>
        /// Creates default options.
        /// </summary>
        public ParallelOptions()
        {
#if DEBUG
            ForceSequential = true;
#else
            ForceSequential = false;
#endif
            ParallelTrigger = (size) => { return size.Width * size.Height > 100 * 100 * sizeof(byte); };
        }

        /// <summary>
        /// Force sequential execution even if parallel should be used.
        /// </summary>
        public bool ForceSequential { get; set; }

        /// <summary>
        /// Function that returns true if parallel processing should be used. Default one uses image size 100x100 as trigger.
        /// </summary>
        public Func<Int32Size, bool> ParallelTrigger { get; set; }

        /// <summary>
        /// Returns whether parallel processor executes function in parallel or not.
        /// </summary>
        /// <param name="srcSize">Source image size.</param>
        /// <returns></returns>
        public bool ShouldProcessParallel(Int32Size srcSize)
        {
            if (!ForceSequential && ParallelTrigger(srcSize) == true)
                return true;
            else
                return false;
        }
    }

    /// <summary>
    /// Represents a core class for parallel patch processing.
    /// </summary>
    /// <typeparam name="TSrc">Source data type.</typeparam>
    /// <typeparam name="TDest">Output data type.</typeparam>
    public class ParallelProcessor<TSrc, TDest>
    {
        /// <summary>
        /// Function that creates destination structure.
        /// </summary>
        /// <returns></returns>
        public delegate TDest ImageCreator();
        /// <summary>
        /// Function that performs patch processing.
        /// </summary>
        /// <param name="src">Source structure</param>
        /// <param name="dest">Destination structure</param>
        /// <param name="area">ROI for destination structure</param>
        public delegate void ProcessPatch(TSrc src, TDest dest, Int32Rect area);

        private List<Int32Rect> patches;

        protected ImageCreator destImageCreator;
        private ProcessPatch processPatch;
        private Int32Size imageSize;
        private bool runParallel;

        /// <summary>
        /// Creates parallel patch processor.
        /// </summary>
        /// <param name="imageSize">2D structure size.</param>
        /// <param name="destImageCreator">Function that creates destination structure.</param>
        /// <param name="processPatch">Function that performs patch processing.</param>
        public ParallelProcessor(Int32Size imageSize, ImageCreator destImageCreator, ProcessPatch processPatch)
            : this(imageSize, destImageCreator, processPatch, new ParallelOptions(), 0)
        { }

        /// <summary>
        /// Creates parallel patch processor.
        /// </summary>
        /// <param name="imageSize">2D structure size.</param>
        /// <param name="destImageCreator">Function that creates destination structure.</param>
        /// <param name="processPatch">Function that performs patch processing.</param>
        /// <param name="parallelOptions">Parallel options.</param>
        /// <param name="minPatchHeight">Minimal patch height. Patches that has lower size will not be created.</param>
        public ParallelProcessor(Int32Size imageSize, ImageCreator destImageCreator, ProcessPatch processPatch, ParallelOptions parallelOptions, int minPatchHeight = 0)
        {
            Initialize(imageSize, destImageCreator, processPatch, parallelOptions, minPatchHeight);
        }

        protected ParallelProcessor()
        { }

        protected void Initialize(Int32Size imageSize, ImageCreator destImageCreator, ProcessPatch processPatch, ParallelOptions parallelOptions, int minPatchHeight)
        {
            this.imageSize = imageSize;
            this.destImageCreator = destImageCreator;
            this.processPatch = processPatch;
            this.runParallel = parallelOptions.ShouldProcessParallel(imageSize); //assume depth = sizeof(byte)

            if (runParallel) //do not build structures if they are not needed
            {
                makePatches(imageSize, minPatchHeight, out patches);
            }
        }

        /// <summary>
        /// Gets or sets image creator. Thus function is called only once.
        /// </summary>
        public ImageCreator DestImageCreator { get { return destImageCreator; } set { destImageCreator = value; } }
        /// <summary>
        /// Gets or sets patch process function. This function is called for every patch.
        /// </summary>
        public virtual ProcessPatch ProcessPatchFunc { get { return processPatch; } set { processPatch = value; } }

        /// <summary>
        /// Runs parallel processor.
        /// </summary>
        /// <param name="image">Source image</param>
        /// <returns>Processed destination image. The image which is created with <see cref="ImageCreator"/>.</returns>
        public virtual TDest Process(TSrc image)
        {
            TDest destImg = destImageCreator();

            if (runParallel) //process parallel
            {
                //do patches
                Parallel.For(0, patches.Count, (int i) =>
                {
                    processPatch(image, destImg, patches[i]);
                });
            }
            else //process sequential
            {
                processPatch(image, destImg, new Int32Rect(new Point(), imageSize));
            }

            return destImg;
        }

        private void makePatches(Int32Size imgSize, int minPatchHeight, out List<Int32Rect> patches)
        {
            int patchHeight, verticalPatches;
            getPatchInfo(imgSize, out patchHeight, out verticalPatches);
            minPatchHeight = System.Math.Max(minPatchHeight, patchHeight);

            patches = new List<Int32Rect>();

            for (int y = 0; y < imgSize.Height; )
            {
                int h = System.Math.Min(patchHeight, imgSize.Height - y);

                Int32Rect patch = new Int32Rect(0, y, imgSize.Width, h);
                patches.Add(patch);

                y += h;
            }

            //ensure minPatchSize (merge last two patches if necessary)
            if (patches.Last().Height < minPatchHeight) 
            {
                var penultimate = patches[patches.Count - 1 - 1];
                var last = patches[patches.Count - 1];

                var mergedPatch = new Int32Rect 
                {
                    X = penultimate.X,
                    Y = penultimate.Y,
                    Width = penultimate.Width,
                    Height = penultimate.Height + last.Height
                };

                patches.RemoveRange(patches.Count - 1 - 1, 2);
                patches.Add(mergedPatch);
            }
        }

        private void getPatchInfo(Int32Size imgSize, out int patchHeight, out int verticalPatches)
        {
            int numOfCores = System.Environment.ProcessorCount;
            int minNumOfPatches = numOfCores * 2;

            float avgNumPatchElements = (float)(imgSize.Width * imgSize.Height) / minNumOfPatches;

            //make patch look like a long stripe (it is probably more efficient to process than a square patch)
            patchHeight = (int)System.Math.Floor(avgNumPatchElements / imgSize.Width);

            //get number of patches
            verticalPatches = (int)System.Math.Ceiling((float)imgSize.Height / patchHeight);
        }
    }
}
