﻿using Accord.Extensions;
using Accord.Extensions.Imaging;
using Accord.Extensions.Math.Geometry;
using System;
using System.Windows.Forms;
using Database = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Accord.Extensions.Imaging.Annotation>>;
using RangeF = AForge.Range;

namespace ObjectAnnotater.SampleGeneration
{
    public partial class SamplePreparation : Form
    {
        Database database;

        public SamplePreparation(Database database = null)
        { 
            InitializeComponent();
            this.database = database;
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            if (database == null)
                return;

            /*if (!chkUnifiyScales.Checked && !chkRandomize.Checked && !chkInflate.Checked)
                MessageBox.Show("Please select at least one option!", "Selection error", MessageBoxButtons.OK, MessageBoxIcon.Error);*/

            Database newDb = database.Clone();

            //inflate
            if (chkInflate.Checked)
            {
                var inflateFactor = (float)this.nInflateFactor.Value;

                newDb = newDb.ModifyAnnotations(imgAnn => 
                {
                    var rect = imgAnn.Polygon.BoundingRect();
                    var inflatedRect = (RectangleF)rect.Inflate(inflateFactor, inflateFactor);

                    var modifiedImgAnn = (Annotation)imgAnn.Clone();
                    modifiedImgAnn.Polygon = Rectangle.Round(inflatedRect).Vertices();
                    return modifiedImgAnn;
                });
            }

            //randomize
            if (chkRandomize.Checked)
            {
               var locationRangeX = new RangeF((float)nLocMinX.Value, (float)nLocMaxX.Value);
               var locationRangeY = new RangeF((float)nLocMinY.Value, (float)nLocMaxY.Value);

               var scaleRangeWidth = new RangeF((float)nScaleMinWidth.Value, (float)nScaleMaxWidth.Value);
               var scaleRangeHeight = new RangeF((float)nScaleMinHeight.Value, (float)nScaleMaxHeight.Value);

               var numSamples = (int)nSamples.Value;

               newDb = newDb.Randomize
                        (
                            locationRand: new Pair<RangeF>(locationRangeX, locationRangeY), 
                            scaleRand: new Pair<RangeF>(scaleRangeWidth, scaleRangeHeight), 
                            nRandsPerSample: numSamples
                        );
            }

            //unify scales
            if (chkUnifiyScales.Checked)
            {
                var widthHeightRatio = (float)nWidthHeightRatio.Value;

                newDb = newDb.ModifyAnnotations(imgAnn => 
                {
                    var rect = imgAnn.Polygon.BoundingRect();
                    var scaledRect = ((RectangleF)rect).ScaleTo(widthHeightRatio, correctLocation: true);

                    var modifiedImgAnn = (Annotation)imgAnn.Clone();
                    modifiedImgAnn.Polygon = Rectangle.Round(scaledRect).Vertices();
                    return modifiedImgAnn;
                });
            }

            //clamp
            var imWidth = (int)nImageWidth.Value;
            var imHeight = (int)nImageHeight.Value;

            newDb = newDb.ModifyAnnotations(imgAnn =>
            {
                var rect = imgAnn.Polygon.BoundingRect();
                var clampedRect = rect.Intersect(new Size(imWidth, imHeight), preserveScale: true);

                var modifiedImgAnn = (Annotation)imgAnn.Clone();
                modifiedImgAnn.Polygon = Rectangle.Round(clampedRect).Vertices();
                return modifiedImgAnn;
            });

            using (var diag = new SaveFileDialog())
            {
                diag.Filter = "(*.xml)|*.xml";
                diag.OverwritePrompt = true;

                var result = diag.ShowDialog();
                if (result == DialogResult.OK)
                {
                    newDb.Save(diag.FileName);
                    MessageBox.Show("Saved!", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void nScaleWidth_ValueChanged(object sender, EventArgs e)
        {
            this.nScaleMinWidth.Value = Math.Min(this.nScaleMinWidth.Value, this.nScaleMaxWidth.Value);
            this.nScaleMaxWidth.Value = Math.Max(this.nScaleMinWidth.Value, this.nScaleMaxWidth.Value);
        }

        private void nScaleHeight_ValueChanged(object sender, EventArgs e)
        {
            this.nScaleMinHeight.Value = Math.Min(this.nScaleMinHeight.Value, this.nScaleMaxHeight.Value);
            this.nScaleMaxHeight.Value = Math.Max(this.nScaleMinHeight.Value, this.nScaleMaxHeight.Value);
        }

        private void nLocX_ValueChanged(object sender, EventArgs e)
        {
            this.nLocMinX.Value = Math.Min(this.nLocMinX.Value, this.nLocMaxX.Value);
            this.nLocMaxX.Value = Math.Max(this.nLocMinX.Value, this.nLocMaxX.Value);
        }

        private void nLocY_ValueChanged(object sender, EventArgs e)
        {
            this.nLocMinY.Value = Math.Min(this.nLocMinY.Value, this.nLocMaxY.Value);
            this.nLocMaxY.Value = Math.Max(this.nLocMinY.Value, this.nLocMaxY.Value);
        }

        private void chkInflate_CheckedChanged(object sender, EventArgs e)
        {
            gpInflate.Enabled = ((CheckBox)sender).Checked;
        }

        private void chkRandomize_CheckedChanged(object sender, EventArgs e)
        {
            gpRandomization.Enabled = ((CheckBox)sender).Checked;
        }

        private void chkUnifiyScales_CheckedChanged(object sender, EventArgs e)
        {
            gpUnifyScales.Enabled = ((CheckBox)sender).Checked;
        }
    }
}
