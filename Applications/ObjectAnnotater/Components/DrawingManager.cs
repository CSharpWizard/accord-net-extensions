﻿using Accord.Extensions.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MoreLinq;
using Accord.Extensions.Math.Geometry;

namespace ObjectAnnotater.Components
{
    public class DrawingManager: IDisposable
    {
        static DrawingAnnotation[] prototypes = new DrawingAnnotation[] 
        {
            new PointAnnotation(),
            new RectangleAnnotation()
        };

        public delegate void ElementSelected(DrawingManager sender, DrawingAnnotation element);
        public event ElementSelected OnElementSelected;

        PictureBox pictureBox;
        List<DrawingAnnotation> drawingAnnotations;
        ContextMenu menu;

        public DrawingManager(PictureBox pictureBox)
        {
            this.pictureBox = pictureBox;
            drawingAnnotations = new List<DrawingAnnotation>();

            createMenu();


            this.pictureBox.Paint += pictureBox_Paint;
            this.pictureBox.MouseDown += pictureBox_MouseDown;
            this.pictureBox.MouseMove += pictureBox_MouseMove;
            this.pictureBox.MouseUp += pictureBox_MouseUp;
        }

        private void createMenu()
        {
            Action<object, EventArgs, Type> onMenuItemClick = (sender, args, annType) =>
            {
                this.DefaultAnnotationType = annType;

                var thisItem = (MenuItem)sender;
                thisItem.Checked = true;

                foreach (var item in menu.MenuItems)
                {
                    if (((MenuItem)item).Text != thisItem.Text)
                    {
                        ((MenuItem)item).Checked = false;
                    }
                }
            };

            menu = new ContextMenu(new MenuItem[] 
            {
               new MenuItem("Point", (sender, args) => onMenuItemClick(sender, args, typeof(PointAnnotation))),
               new MenuItem("Rectangle", (sender, args) => onMenuItemClick(sender, args, typeof(RectangleAnnotation)))
            });

            foreach (var item in menu.MenuItems)
            {
                ((MenuItem)item).RadioCheck = true;
            }

            menu.MenuItems[0].PerformClick();
        }

        void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            foreach (var drawingAnn in drawingAnnotations)
            {
                drawingAnn.Draw(e.Graphics);
            }
        }

        void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.Selected != null)
            {
                this.Selected.OnMouseUp(this, e);
                pictureBox.Invalidate();
            }
        }

        void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.Selected != null)
            {
                this.Selected.OnMouseMove(this, e);
                pictureBox.Invalidate();

                if (this.OnElementSelected != null) OnElementSelected(this, this.Selected);
            }
        }

        public Type DefaultAnnotationType { get; set; }
        public String DefaultLabel { get; set; }

        void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            var selectedAnn = drawingAnnotations
                              .Where(x => x.BoundingRectangle.Contains(e.Location.ToPt()))
                              .OrderBy(x => x.BoundingRectangle.Area())
                              .FirstOrDefault();

            if (e.Button != MouseButtons.Left)
            {
                if (selectedAnn == null) //right click on empty space
                {
                    menu.Show(pictureBox, e.Location);
                    return;
                }

                if (selectedAnn != null) //if selected
                {
                    drawingAnnotations.Where(x => x.Equals(selectedAnn) == false).ForEach(x => x.IsSelected = false);
                    selectedAnn.IsSelected = true;
                    return;
                }
            }

            drawingAnnotations.ForEach(x => x.IsSelected = false);

            var newAnn = (DrawingAnnotation)Activator.CreateInstance(DefaultAnnotationType);
            newAnn.Initialize(pictureBox);
            newAnn.Annotation.Label = DefaultLabel;
            newAnn.IsSelected = true;
            newAnn.OnMouseDown(this, e);

            drawingAnnotations.Add(newAnn);

            pictureBox.Invalidate();
        }

        public void AddRange(IEnumerable<Annotation> annotations)
        {
            foreach (var ann in annotations)
            {
                Add(ann);
            }
        }

        public void Add(Annotation annotation)
        {
            DrawingAnnotation drawingAnn = null;
            foreach (var prototype in prototypes)
            {
                if (prototype.BelongsTo(annotation.Polygon))
                {
                    drawingAnn = (DrawingAnnotation)Activator.CreateInstance(prototype.GetType());
                    drawingAnn.Initialize(pictureBox);
                    drawingAnn.Annotation = annotation;
                    drawingAnn.IsSelected = false;
                    break;
                }
            }

            if (drawingAnn == null)
                throw new Exception("Could not determine the drawing annotation type!");

            drawingAnnotations.Add(drawingAnn);
        }

        public void RemoveSelected()
        {
            if (this.Selected == null)
                return;

            drawingAnnotations.Remove(this.Selected);
            pictureBox.Invalidate();
        }

        public DrawingAnnotation Selected 
        {
            get 
            {
                return drawingAnnotations.Where(x => x.IsSelected).FirstOrDefault();
            }
        }

        public IReadOnlyCollection<DrawingAnnotation> DrawingAnnotations { get { return drawingAnnotations; } }

        bool showLabels = true;
        public bool ShowLabels
        {
            get { return showLabels; }
            set 
            {
                drawingAnnotations.ForEach(x => x.ShowLabel = value);
                showLabels = value;
                pictureBox.Invalidate();
            }
        }

        public void Clear()
        {
            this.drawingAnnotations.Clear();
        }

        public void Dispose()
        {
            this.pictureBox.Paint -= pictureBox_Paint;
            this.pictureBox.MouseDown -= pictureBox_MouseDown;
            this.pictureBox.MouseMove -= pictureBox_MouseMove;
            this.pictureBox.MouseUp -= pictureBox_MouseUp;

            menu.Dispose();
            this.drawingAnnotations.Clear();
            pictureBox.Invalidate();
        }
    }
}
