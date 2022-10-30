#region Licence and Terms
// Accord.NET Extensions Framework
// https://github.com/dajuric/accord-net-extensions
//
// Copyright © Darko Jurić, 2014 
// darko.juric2@gmail.com
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU Lesser General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU Lesser General Public License for more details.
// 
//   You should have received a copy of the GNU Lesser General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/lgpl.txt>.
//
#endregion

using Accord.Extensions;
using Accord.Extensions.Imaging;
using Accord.Extensions.Math.Geometry;
using Accord.Extensions.Vision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Database = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Accord.Extensions.Imaging.Annotation>>;
using Point = AForge.IntPoint;
using ObjectAnnotater.Components;

namespace ObjectAnnotater
{
    public partial class ObjectAnnotater : Form
    {
        ImageStreamReader capture = null;
        string databaseFileName = null;
        public Database Database { get; private set; }

        DrawingManager drawingManager = null;

        public ObjectAnnotater()
        {
            InitializeComponent();
        }

        public ObjectAnnotater(ImageStreamReader capture, string databaseFileName)
        {
            InitializeComponent();

            this.capture = capture;

            drawingManager = new DrawingManager(pictureBox);
            drawingManager.OnElementSelected += drawingManager_OnElementSelected;

            this.databaseFileName = databaseFileName;
            Database = new Database();
            Database.Load(databaseFileName);

            loadCurrentImageAnnotations();
        }

        void drawingManager_OnElementSelected(DrawingManager sender, DrawingAnnotation element)
        {
            this.txtAnnotationLabel.Text = element.Annotation.Label;
        }

        Image<Bgr, byte> frame = null;

        #region Commands

        private void loadCurrentImageAnnotations()
        {
            if (capture.Position == capture.Length)
                return;

            drawingManager.Clear();

            frame = capture.ReadAs<Bgr, byte>(); //the order is relevant (position is automatically increased)
            this.pictureBox.Image = frame.ToBitmap();

            var imageKey = getCurrentImageKey();

            if (Database.ContainsKey(imageKey))
                drawingManager.AddRange(Database[imageKey]);

            this.Text = getCurrentImageKey() + " -> " + new FileInfo(databaseFileName).Name;
            this.slider.Value = (int)Math.Max(0, this.capture.Position - 1);
            this.slider.Maximum = (int)(capture.Length - 1);
            this.lblCurrentFrame.Text = this.slider.Value.ToString();
            this.lblTotalFrames.Text = this.slider.Maximum.ToString();
        }

        private void drawingManager_OnAnnotationSelect(object sender, Annotation selectedAnnotation)
        {
            this.txtAnnotationLabel.Text = selectedAnnotation.Label;
            this.txtAnnotationLabel.SelectionStart = this.txtAnnotationLabel.SelectionLength = 0;
        }

        private void getFrame(long offset)
        {
            saveCurrentAnnotations();

            capture.Seek(offset, SeekOrigin.Begin);
            loadCurrentImageAnnotations();
            GC.Collect();
        }

        private void saveCurrentAnnotations()
        {
            var imageKey = getCurrentImageKey();

            var dbAnnotations = Database.ContainsKey(imageKey) ? Database[imageKey]: new List<Annotation>();
            var annotations = drawingManager.DrawingAnnotations.Select(x=> x.Annotation).ToList();

            var areEqual = annotations.All(dbAnnotations.Contains) && annotations.Count == dbAnnotations.Count;
            btnSave.Enabled = btnSave.Enabled || !areEqual;

            if (annotations.Any()) //do not save empty list
            {
                Database[imageKey] = annotations;
            }
        }

        private string getCurrentImageKey()
        {
            if (capture is ImageDirectoryReader == false)
                throw new NotSupportedException("Unsupported image stream reader!");

            capture.Seek(-1);
            string imageKey = (capture as ImageDirectoryReader).CurrentImageName;
            var dbDirInfo = new DirectoryInfo(Path.GetDirectoryName(databaseFileName));
            imageKey = imageKey.GetRelativeFilePath(dbDirInfo);
            capture.Seek(+1);

            return imageKey;
        }

        #endregion

        private void ObjectAnnotater_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnSave.Enabled) //is modified
            {
                var result = MessageBox.Show("Do you want to save current changes ?", "Save annotations", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == System.Windows.Forms.DialogResult.Yes)
                    saveToFile();
            }

            drawingManager.Dispose();
        }

        private void txtLabel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control || e.Alt || e.KeyData == Keys.Delete || drawingManager.Selected == null)
                e.Handled = true;
        }

        private void txtLabel_KeyUp(object sender, KeyEventArgs e)
        {
            if (drawingManager.Selected == null)
                return;

            drawingManager.DefaultLabel = txtAnnotationLabel.Text; 
            drawingManager.Selected.Annotation.Label = txtAnnotationLabel.Text;
            pictureBox.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Delete:
                    drawingManager.RemoveSelected();
                    break;

                case Keys.Control | Keys.A:
                    this.btnToggleLabels.Checked = !btnToggleLabels.Checked;                    
                    break;

                case Keys.Control | Keys.S:
                    saveToFile();
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnToggleLabels_CheckedChanged(object sender, EventArgs e)
        {
            drawingManager.ShowLabels = btnToggleLabels.Checked;
        }

        private void saveToFile()
        {
            saveCurrentAnnotations();
            Database.Save(databaseFileName);

            this.btnSave.Enabled = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            saveToFile();          
        }

        private void slider_ValueChanged(object sender, EventArgs e)
        {
            getFrame(slider.Value);
        }

        private void btnPrepareSamples_Click(object sender, EventArgs e)
        {
            var prepareSamplesForm = new SampleGeneration.SamplePreparation(this.Database);
            prepareSamplesForm.ShowDialog(this);
        }
    }
}
