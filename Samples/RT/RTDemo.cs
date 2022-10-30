﻿using Accord.Extensions;
using Accord.Extensions.Imaging;
using Accord.Extensions.Math.Geometry;
using Accord.Extensions.Vision;
using System;
using System.Threading;
using System.Windows.Forms;

namespace RT
{
    public partial class RTDemo : Form
    {
        StreamableSource videoStreamSource;

        PicoClassifier picoClassifier;
        Detector<PicoClassifier> detector;
        GroupMatching<Rectangle> groupMatching;

        public RTDemo()
        {
            InitializeComponent();

            PicoClassifierHexDeserializer.FromHexFile("myClassifier.ea", out picoClassifier);
            //picoClassifier.ToHexFile("faceSerialized.ea");

            detector = new Detector<PicoClassifier>(picoClassifier);
            detector.StartSize = picoClassifier.GetSize(regionScale: 50);

            groupMatching = new RectangleGroupMatching();

            //videoStreamSource = new CameraCapture();
            videoStreamSource = new FileCapture("S:/Detekcija_Ruke/WIN_20140317_120459.mp4");
            //videoStreamSource = new ImageDirectoryReader(@"C:\", "*.jpg");
           
            if(videoStreamSource is CameraCapture)
                (videoStreamSource as CameraCapture).FrameSize = new Size(640, 480);

            videoStreamSource.Open();

            Application.Idle += Application_Idle;
        }

        void Application_Idle(object sender, EventArgs e)
        {
            var image = videoStreamSource.ReadAs<Bgr, byte>();//.Resize(new Size(320, 240), InterpolationMode.Bilinear);
           if (image == null)
               return;

           var grayIm = image.Convert<Gray, byte>();

           var detections = detector.Detect(grayIm, (im, window, classifier) =>
           {
               float conf;
               classifier.ClassifyRegion(im, window, out conf, 0);
               if (conf > 3)
                   return true;
               else
                   return false;
           });

           /*var groupMatches = groupMatching.Group(detections);

           foreach (var groupMatch in groupMatches)
           {
              image.Draw(groupMatch.Representative, Bgr8.Red, 3);
           }*/

           foreach (var d in detections)
           {
               image.Draw(d, Bgr8.Red, 3);
           }

           this.pictureBox.Image = image.ToBitmap();
           //Thread.Sleep(1000);
        }

        private void RTDemo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoStreamSource != null)
                videoStreamSource.Close();
        }
    }
}
