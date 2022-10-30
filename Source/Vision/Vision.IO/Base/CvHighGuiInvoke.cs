﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Accord.Extensions.Vision
{
    public class VideoCodec
    {
        public static readonly VideoCodec MPEG1 = VideoCodec.FromName('P', 'I', 'M', '1');
        public static readonly VideoCodec MotionJpeg = VideoCodec.FromName('M', 'J', 'P', 'G');
        public static readonly VideoCodec IntelYUV = VideoCodec.FromName('I', 'Y', 'U', 'V');
        //public static readonly VideoCodec WMV = VideoCodec.FromName('W', 'M', 'V', '3'); //TODO: does it exist ?
        public static readonly VideoCodec UserSelection = new VideoCodec(-1);

        int codec;

        /// <summary>
        /// Creates new video codec from an id.
        /// </summary>
        /// <param name="codec">Codec id.</param>
        private VideoCodec(int codec)
        {
            this.codec = codec;
        }

        /// <summary>
        /// Creates new video codec id from 4-character code. For example, FromName('P','I','M','1') is MPEG-1 codec, FromName('M','J','P','G') is motion-jpeg codec etc.
        /// </summary>
        /// <param name="c1">First char.</param>
        /// <param name="c2">Second char.</param>
        /// <param name="c3">Third char.</param>
        /// <param name="c4">Fourth char.</param>
        /// <returns>Video codec.</returns>
        public static VideoCodec FromName(char c1, char c2, char c3, char c4)
        {
            int codec = (((c1) & 255) + (((c2) & 255) << 8) + (((c3) & 255) << 16) + (((c4) & 255) << 24));
            return new VideoCodec(codec);
        }

        /// <summary>
        /// Creates new video codec id from 4-character string.
        /// </summary>
        /// <param name="codecName">4-character string codec name.</param>
        /// <returns>Video codec.</returns>
        public static VideoCodec FromName(string codecName)
        {
            if (codecName.Length != 4)
                throw new Exception("Codec name is 4-characters long!");

            return VideoCodec.FromName(codecName[0], codecName[1], codecName[2], codecName[3]);
        }

        public static implicit operator int(VideoCodec videoCodec)
        {
            return videoCodec.codec;
        }

        public static explicit operator VideoCodec(int code)
        {
            return new VideoCodec(code);
        }

        public static explicit operator VideoCodec(string code)
        {
            return VideoCodec.FromName(code);
        }

    }

    /// <summary>
    /// Internal class for OpenCV highgui library invocation.
    /// </summary>
    internal static class CvHighGuiInvoke
    {
        public const CallingConvention CvCallingConvention = CallingConvention.Cdecl;
        public const string OPENCV_CORE_LIBRARY = "opencv_core248";
        public const string OPENCV_HIGHGUI_LIBRARY = "opencv_highgui248";

        [SuppressUnmanagedCodeSecurity]
        [DllImport(OPENCV_HIGHGUI_LIBRARY, CallingConvention = CvCallingConvention)]
        public static extern IntPtr cvCreateCameraCapture(int index);

        [DllImport(OPENCV_HIGHGUI_LIBRARY, CallingConvention = CvCallingConvention)]
        public static extern IntPtr cvCreateFileCapture([MarshalAs(UnmanagedType.LPStr)] string filename);


        [SuppressUnmanagedCodeSecurity]
        [DllImport(OPENCV_HIGHGUI_LIBRARY, CallingConvention = CvCallingConvention)]
        public static extern void cvReleaseCapture(ref IntPtr capture);


        [SuppressUnmanagedCodeSecurity]
        [DllImport(OPENCV_HIGHGUI_LIBRARY, CallingConvention = CvCallingConvention)]
        public static extern int cvGrabFrame(IntPtr capture);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(OPENCV_HIGHGUI_LIBRARY, CallingConvention = CvCallingConvention)]
        public static extern IntPtr cvQueryFrame(IntPtr capture);


        [SuppressUnmanagedCodeSecurity]
        [DllImport(OPENCV_HIGHGUI_LIBRARY, CallingConvention = CvCallingConvention)]
        public static extern double cvGetCaptureProperty(IntPtr capture, CaptureProperty propertyId);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(OPENCV_HIGHGUI_LIBRARY, CallingConvention = CvCallingConvention)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool cvSetCaptureProperty(IntPtr capture, CaptureProperty propertyId, double value);

        public static Size GetImageSize(IntPtr capturePtr)
        {
            return new Size
            {
                Width = (int)CvHighGuiInvoke.cvGetCaptureProperty(capturePtr, CaptureProperty.FrameWidth),
                Height = (int)CvHighGuiInvoke.cvGetCaptureProperty(capturePtr, CaptureProperty.FrameHeight)
            };
        }

        public static bool SetImageSize(IntPtr capturePtr, Size newSize)
        {
            bool success;
            success = CvHighGuiInvoke.cvSetCaptureProperty(capturePtr, CaptureProperty.FrameWidth, newSize.Width);
            success &= CvHighGuiInvoke.cvSetCaptureProperty(capturePtr, CaptureProperty.FrameHeight, newSize.Height);

            return success;
        }


        /************************************************ image IO ****************************************************/

        [SuppressUnmanagedCodeSecurity]
        [DllImport(OPENCV_HIGHGUI_LIBRARY, CallingConvention = CvCallingConvention)]
        public static extern IntPtr cvLoadImage([MarshalAs(UnmanagedType.LPStr)] String filename, ImageLoadType loadType);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(OPENCV_CORE_LIBRARY, CallingConvention = CvCallingConvention)]
        public static extern void cvReleaseImage(ref IntPtr image);
        /************************************************ image IO ****************************************************/

        /************************************************ videWriter IO ****************************************************/

        /// <summary>
        /// Creates video writer structure.
        /// </summary>
        /// <param name="filename">Name of the output video file.</param>
        /// <param name="fourcc">4-character code of codec used to compress the frames. See <see cref="VideoCodec"/> class.</param>
        /// <param name="fps">Frame rate of the created video stream. </param>
        /// <param name="frameSize">Size of video frames.</param>
        /// <param name="isColor">If true, the encoder will expect and encode color frames, otherwise it will work with grayscale frames </param>
        /// <returns>The video writer</returns>
        [DllImport(OPENCV_HIGHGUI_LIBRARY, CallingConvention = CvCallingConvention)]
        public static extern IntPtr cvCreateVideoWriter([MarshalAs(UnmanagedType.LPStr)] String filename, int fourcc, double fps, Size frameSize, [MarshalAs(UnmanagedType.Bool)] bool isColor);

        /// <summary>
        /// Writes/appends one frame to video file.
        /// </summary>
        /// <param name="writer">video writer structure.</param>
        /// <param name="image">the written frame</param>
        /// <returns>True on success, false otherwise</returns>
        [DllImport(OPENCV_HIGHGUI_LIBRARY, CallingConvention = CvCallingConvention)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool cvWriteFrame(IntPtr writer, IntPtr image);

        /// <summary>
        /// Finishes writing to video file and releases the structure.
        /// </summary>
        /// <param name="writer">pointer to video file writer structure</param>
        [DllImport(OPENCV_HIGHGUI_LIBRARY, CallingConvention = CvCallingConvention)]
        public static extern void cvReleaseVideoWriter(ref IntPtr writer);

        /************************************************ videWriter IO ****************************************************/

        static CvHighGuiInvoke()
        {
            Platform.AddDllSearchPath();
        }
    }

    /// <summary>
    /// OpenCV capture properties for camera and video.
    /// </summary>
    internal enum CaptureProperty: int
    {
        PosFrames = 1,
        FrameWidth = 3,
        FrameHeight = 4,
        FPS = 5,
        FrameCount = 7,

        /************** camera properties ******************/
        Brightness = 10,
        Contrast = 11,
        Saturation = 12,
        Hue = 13,
        Gain = 14,
        Exposure = 15,
        /************** camera properties ******************/

        ConvertRGB = 16
    }

    /// <summary>
    /// OpenCV image load mode.
    /// </summary>
    internal enum ImageLoadType: int
    {
        /// <summary>
        /// Loads the image as is (including the alpha channel if present)
        /// </summary>
        Unchanged = -1,

        /// <summary>
        /// Loads the image as an intensity one
        /// </summary>
        Grayscale = 0,

        /// <summary>
        ///  Loads the image in the RGB format
        /// </summary>
        Color = 1,
    }
}
