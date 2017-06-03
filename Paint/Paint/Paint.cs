﻿using MetroFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Paint
{
    public partial class Paint : MetroFramework.Forms.MetroForm
    {
        #region Declare
        enum DRAW_STATUS { COMPLETE, INCOMPLETE };
        //Color color = Color.Black;
        int penWidth = 1;
        string objectChoose;
        ObjectDrawing Shape;
        Bitmap doubleBuffer, fillImage;
        GraphicsList grapList;
        DRAW_STATUS status;
        bool isSaved = true;
        Graphics pen; // pen width
        bool isCrop = false;
        #endregion

        // danh sách màu mặc định
        List<Color> listColor = new List<Color>() { Color.Black , Color.Black , Color.White , Color.Silver , Color.Blue , Color.Green , Color.Lime , Color.Teal ,Color.Orange
                                                    , Color.Brown , Color.Pink , Color.Magenta , Color.Purple , Color.Red , Color.Yellow };
        public Paint()
        {
            InitializeComponent();
            for (int i = 1; i < 14; i++)
            {
                MetroFramework.Controls.MetroTile _Tile = new MetroFramework.Controls.MetroTile();
                _Tile.Size = new Size(30, 30);
                _Tile.Tag = i;
                _Tile.UseCustomBackColor = true;
                _Tile.BackColor = listColor[i];
                //_Tile.Style = (MetroColorStyle)i;
                _Tile.Click += (sender, e) =>
                {
                    
                    mtitleCurrentColor.UseCustomBackColor = true;
                    mtitleCurrentColor.BackColor = _Tile.BackColor;

                    mtitleCurrentColor.Refresh();
                };
                flColors.Controls.Add(_Tile);
            }

            doubleBuffer = new Bitmap(Screen.PrimaryScreen.Bounds.Width - 300, Screen.PrimaryScreen.Bounds.Height, picPaint.CreateGraphics());
            Graphics g = Graphics.FromImage(doubleBuffer);
            g.Clear(Color.White);
            fillImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width - 300, Screen.PrimaryScreen.Bounds.Height, picPaint.CreateGraphics());
            g = Graphics.FromImage(fillImage);
            g.Clear(Color.White);
            grapList = new GraphicsList();

            pen = pn_penWidth.CreateGraphics();
        }
        private Color ChooseColor(int index)
        {
            return listColor[index];
        }
        private void MLEditColor_Click(object sender, System.EventArgs e)
        {
            ColorDialog EditColor = new ColorDialog();
            EditColor.ShowDialog();
            mtitleCurrentColor.BackColor = EditColor.Color;
        }

        private void picPaint_Paint(object sender, PaintEventArgs e)
        {
            doubleBuffer = fillImage.Clone(new Rectangle(0, 0, fillImage.Width, fillImage.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(doubleBuffer);
            if (grapList._list.Count > 0)
            {
                btnUndo.Enabled = true;
                grapList.Draw(g);
            }
            else
            {
                btnUndo.Enabled = false;
            }
            if (status == DRAW_STATUS.INCOMPLETE && objectChoose != "bucket" && objectChoose != "none")
                Shape.DrawHandlePoint(g);
            e.Graphics.DrawImageUnscaled(doubleBuffer, 0, 0);
        }

        private void picPaint_MouseDown(object sender, MouseEventArgs e)
        {
            isSaved = false;
            if (e.Button == MouseButtons.Left)
            {

                //Neu da chon doi tuong
                if (objectChoose == "bucket")
                {
                    //Shape = null;    
                    BucketDrawing bucket = new BucketDrawing(mtitleCurrentColor.BackColor);

                    fillImage = bucket.Fill(doubleBuffer, fillImage, e.X, e.Y);

                    picPaint.Refresh();
                }
                else
                 if (objectChoose == "rectangle" || objectChoose == "circle" || objectChoose == "star" || objectChoose == "line" || objectChoose == "rhombus" || objectChoose == "triangle" || objectChoose == "pentagon" || objectChoose == "hexagon" || objectChoose == "crop")
                {


                    if (Shape != null && Shape.CheckLocation(e.Location) >= 0)
                    {
                        Shape.Mouse_Down(e);
                        status = DRAW_STATUS.INCOMPLETE;

                        if (Shape.CheckLocation(e.Location) == 0)
                            Cursor = Cursors.SizeAll;
                        if (Shape.CheckLocation(e.Location) > 0)
                            Cursor = Cursors.Cross;
                    }

                    else
                    {
                        if (objectChoose != "crop")
                        {
                            status = DRAW_STATUS.COMPLETE;
                            ChooseObject();
                            Shape.Mouse_Down(e);
                            grapList._list.Insert(grapList._list.Count, Shape);

                        }
                        else
                        {
                            if (isCrop == true)
                            {
                                if (grapList._list.Count != 0)
                                    grapList._list.RemoveAt(grapList._list.Count - 1);
                                picPaint.Refresh();
                            }

                            status = DRAW_STATUS.COMPLETE;
                            ChooseObject();
                            Shape.Mouse_Down(e);
                            grapList._list.Insert(grapList._list.Count, Shape);

                        }
                    }

                }
                else
                {
                    status = DRAW_STATUS.COMPLETE;
                    ChooseObject();
                    Shape.Mouse_Down(e);
                    grapList._list.Insert(grapList._list.Count, Shape);
                }
            }

            else
            {
                status = DRAW_STATUS.COMPLETE;
                Shape = null;
            }

        }
        private void picPaint_MouseMove(object sender, MouseEventArgs e)
        {

            //toolStripStatusLabel1.Text = "Cursor: " + e.Location.X + " x " + e.Location.Y;

            if (Shape != null)
            {
                Shape.Mouse_Move(e);
                status = DRAW_STATUS.INCOMPLETE;
                if (Shape.CheckLocation(e.Location) == 0)
                    Cursor = Cursors.SizeAll;
                else if (Shape.CheckLocation(e.Location) > 0)
                    Cursor = Cursors.Cross;
                else
                    Cursor = Cursors.Default;
                picPaint.Refresh();
            }

        }

        private void picPaint_MouseUp(object sender, MouseEventArgs e)
        {
            if (objectChoose == "pencil" || objectChoose == "eraser")
            {
                Shape = null;
                status = DRAW_STATUS.COMPLETE;
            }
            if (Shape != null)

                Shape.Mouse_Up(e);
        }

        private void TB_penWidth_Scroll(object sender, ScrollEventArgs e)
        {
            penWidth = TB_penWidth.Value;
            //lb_penWidth.Text = TB_penWidth.Value.ToString();
            DrawpenWidth();
        }
        public void btnObject_Click(object sender, EventArgs e)
        {

            Button btnObject = (Button)sender;
            objectChoose = btnObject.Name.Remove(0, 3).ToLower();
            if (objectChoose != "crop")
                isCrop = false;

            if (objectChoose == "crop" && isCrop == true)
            {
                status = DRAW_STATUS.COMPLETE;

                int width = Math.Abs(Shape._endPoint.X - Shape._startPoint.X);
                int height = Math.Abs(Shape._endPoint.Y - Shape._startPoint.Y);
                Rectangle ROI = new Rectangle(Shape._startPoint.X + 1, Shape._startPoint.Y + 1, width - 2, height - 2);

                fillImage = CropImage(doubleBuffer, ROI);

                //panelPaint.Size = fillImage.Size;

                picPaint.Refresh();
                isCrop = false;
            }
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            status = DRAW_STATUS.COMPLETE;
            grapList.RemoveLast();
            //picPaint.Refresh();
        }

        private Bitmap CropImage(Bitmap src, Rectangle Roi)
        {
            Bitmap croppedImg;
            croppedImg = src.Clone(Roi, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            return croppedImg;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            //Shape = null;
            grapList._list.Clear();
            if (isSaved == false)
            {
                DialogResult dlr = MessageBox.Show("Do you want to save first?", "Absoluke Paint", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dlr == DialogResult.Yes)
                {
                    btnSaveAs_Click(sender, e);
                    isSaved = true;
                    //panelPaint.Size = new Size(1145, 737);
                    fillImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width - 300, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    Graphics g = Graphics.FromImage(fillImage);
                    g.Clear(Color.White);
                    picPaint.Size = fillImage.Size;
                    picPaint.Refresh();
                    isSaved = true;
                }
                else
                {
                    //panelPaint.Size = new Size(1145, 737);
                    fillImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width - 300, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    Graphics g = Graphics.FromImage(fillImage);
                    g.Clear(Color.White);
                    picPaint.Size = fillImage.Size;
                    picPaint.Refresh();
                    isSaved = true;
                }
            }
            else
            {
                //panelPaint.Size = new Size(1145, 737);
                fillImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width - 300, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(fillImage);
                g.Clear(Color.White);
                picPaint.Size = fillImage.Size;
                picPaint.Refresh();
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "All Picture Files|*.bmp;*.ico;*.gif;*.jpeg;*.jpg;*.jfif;*.png;*.tif;*.tiff;*.wmf;*.emf|" +
                            "Windows Bitmap (*.bmp)|*.bmp|" +
                            "All Files (*.*)|*.*";
            openFileDialog1.Title = "Open an Image File";

            btnNew_Click(sender, e);
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    fillImage = new Bitmap(openFileDialog1.FileName);
                    picPaint.Image = fillImage;
                    picPaint.Refresh();
                }
                catch
                {
                    MessageBox.Show("Can't read this file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.Filter = "BMP (*.bmp)|*.bmp|All File (*.*)|*.*";
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.OverwritePrompt = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                doubleBuffer.Save(saveFileDialog1.FileName);
            isSaved = true;
        }

        private void ChooseObject()
        {
            switch (objectChoose)
            {
                case "pencil":
                    Shape = new PenDrawing(mtitleCurrentColor.BackColor, penWidth);
                    break;

                case "eraser":
                    Shape = new EraserDrawing(penWidth);
                    break;
                case "rectangle":
                    Shape = new RectangleDrawing(mtitleCurrentColor.BackColor, penWidth);
                    break;
                case "circle":
                    Shape = new CircleDrawing(mtitleCurrentColor.BackColor, penWidth);
                    break;
                case "star":
                    Shape = new StarDrawing(mtitleCurrentColor.BackColor, penWidth);
                    break;
                case "line":
                    Shape = new LineDrawing(mtitleCurrentColor.BackColor, penWidth);
                    break;
                case "triangle":
                    Shape = new TriangleDrawing(mtitleCurrentColor.BackColor, penWidth);
                    break;
                case "rhombus":
                    Shape = new RhombusDrawing(mtitleCurrentColor.BackColor, penWidth);
                    break;
                case "pentagon":
                    Shape = new PentagonDrawing(mtitleCurrentColor.BackColor, penWidth);
                    break;
                case "hexagon":
                    Shape = new HexagonDrawing(mtitleCurrentColor.BackColor, penWidth);
                    break;
                case "crop":
                    {
                        Shape = new CropRectangle();
                        isCrop = true;
                    }
                    break;

                default:
                    objectChoose = "none";
                    Shape = new NoneShapeDrawing();
                    break;
            }
        }

        private void pn_penWidth_Paint(object sender, PaintEventArgs e)
        {
            DrawpenWidth();
        }

        private void llbAbout_Click(object sender, EventArgs e)
        {

        }

        public void DrawpenWidth()
        {
            pen.Clear(Color.FromArgb(128, 128, 255));
            SolidBrush brush = new SolidBrush(mtitleCurrentColor.BackColor);
            RectangleF rec = new RectangleF(2, 2, pn_penWidth.Width, pn_penWidth.Height);
            pen.DrawLine(new Pen(mtitleCurrentColor.BackColor, penWidth), new Point(5, pn_penWidth.Height / 2), new Point(pn_penWidth.Width - 5, pn_penWidth.Height / 2));
        }
    }
}