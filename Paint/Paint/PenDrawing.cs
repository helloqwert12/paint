﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;


namespace Paint
{
    class PenDrawing : ObjectDrawing
    {
        #region Declare
        protected List<Point> _listPoint;
        #endregion

        #region Method
        public PenDrawing() : base()
        {
           
            _listPoint = new List<Point>(2) { new Point(0, 0), new Point(0, 1) };
            _grapPath.AddCurve(_listPoint.ToArray());
            _grapPath.Widen(new Pen(_color, _penWidth));
           
            _listPoint.Clear();
        }

        public PenDrawing(Color color, int penWidth) : base(color, penWidth)
        {
          
            _listPoint = new List<Point>(2) { new Point(0, 0), new Point(0, 1) };
            _grapPath.AddCurve(_listPoint.ToArray());
            _grapPath.Widen(new Pen(_color, _penWidth));
          
            _listPoint.Clear();
        }
        public override void Draw(Graphics g)
        {
            Pen p = new Pen(_color, _penWidth);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (_listPoint.Count == 1)
                g.DrawLine(p, _listPoint[0], _listPoint[0]);    
            else
                g.DrawLines(p, _listPoint.ToArray());
            p.Dispose();
        }
        #endregion

        #region Event
        public override void Mouse_Down(MouseEventArgs e)
        {
            _listPoint.Insert(_listPoint.Count, e.Location);
        }

        public override void Mouse_Move(MouseEventArgs e)
        {
            _listPoint.Insert(_listPoint.Count, e.Location);
        }

        public override void Mouse_Up(MouseEventArgs e)
        {
            base.Mouse_Up(e);
        }

        #endregion
    }
}
