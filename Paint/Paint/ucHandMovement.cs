﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;


using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using EmguCV;
using Emgu.Util;
namespace Paint
{
    public partial class ucHandMovement : MetroFramework.Controls.MetroUserControl
    {
        Capture capture;
        Image<Bgr, byte> currentFrame;
        bool captureInProgress;
        Matrix<int> mDefect;
        Point[] contourPoints;  //Mang diem contours
        int minCr, maxCr, minCb, maxCb;
        bool isDrag;
        bool useVirtualMouse;

        public ucHandMovement()
        {
            if (!this.DesignMode)
            {
                this.InitializeComponent();

                isDrag = false;
                useVirtualMouse = false;          

                minCr = 131;
                maxCr = 185;
                minCb = 80;
                maxCb = 135;

                tbMinCr.Value = minCr;
                tbMaxCr.Value = maxCr;
                tbMinCb.Value = minCb;
                tbMaxCb.Value = maxCb;

                lblMinCr.Text = tbMinCr.Value.ToString();
                lblMinCb.Text = tbMinCb.Value.ToString();
                lblMaxCr.Text = tbMaxCr.Value.ToString();
                lblMaxCb.Text = tbMaxCb.Value.ToString();
            }
        }
        public void ReleaseData()
        {
            tgVirtualMouse.Checked = false;
        }
        private void Ycc_Scroll(object sender, ScrollEventArgs e)
        {
            MetroFramework.Controls.MetroTrackBar temp = sender as MetroFramework.Controls.MetroTrackBar;
            switch(temp.Tag.ToString())
            {
                case "1":   //minCr
                    minCr = tbMinCr.Value;
                    lblMinCr.Text = tbMinCr.Value.ToString();
                    break;
                case "2":   //maxCr
                    maxCr = tbMaxCr.Value;
                    lblMaxCr.Text = tbMaxCr.Value.ToString();
                    break;
                case "3":   //minCb
                    minCb = tbMinCb.Value;
                    lblMinCb.Text = tbMinCb.Value.ToString();
                    break;
                case "4":   //maxCb
                    maxCb = tbMaxCb.Value;
                    lblMaxCb.Text = tbMaxCb.Value.ToString();
                    break;
            }                                    
        }

        void ProcessFramAndUpdateGUI(object Sender, EventArgs agr)
        {
            int Finger_num = 0;
            Double Result1 = 0;
            Double Result2 = 0;
            //querying image
            
            currentFrame = capture.QueryFrame().ToImage<Bgr, byte>();
            int widthROI = currentFrame.Size.Width / 4;
            int heightROI = currentFrame.Size.Height / 4;
           // currentFrame = currentFrame.Copy(new Rectangle(widthROI, heightROI, widthROI * 2, heightROI * 2));
            if (currentFrame == null) return;


            //Cach 1***************************
            //Applying YCrCb filter
            //Image<Ycc, Byte> currentYCrCbFrame = currentFrame.Convert<Ycc, byte>();
            //Image<Gray, byte> skin = new Image<Gray, byte>(currentFrame.Width, currentFrame.Height);

            //skin = currentYCrCbFrame.InRange(new Ycc(0, minCr, minCb), new Ycc(255, maxCr, maxCb));

            ////Erode
            //Mat rect_12 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(10, 10), new Point(5, 5));
            //CvInvoke.Erode(skin, skin, rect_12, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            ////Dilate
            //Mat rect_6 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(6, 6), new Point(3, 3));
            //CvInvoke.Dilate(skin, skin, rect_6, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            //**********************************

            //Cach 2**************************** Phong
            IColorSkinDetector skinDetector = new YCrCbSkinDetector();
            Image<Gray, byte> skin = skinDetector.DetectSkin(currentFrame, new Ycc(0, minCr, minCb), new Ycc(255, maxCr, maxCb));
            //**********************************

            skin = skin.Flip(FlipType.Horizontal);

            //smoothing the filterd , eroded and dilated image.
            skin = skin.SmoothGaussian(9);
            picSkinCam.Image = skin.ToBitmap();
            currentFrame = currentFrame.Flip(FlipType.Horizontal);

            #region Extract contours and hull
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(skin, contours, new Mat(), Emgu.CV.CvEnum.RetrType.External,
                                          Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            VectorOfPoint biggestContour = new VectorOfPoint();// mang point[] chua vien` lon nhat

            //Tim area contours lon nhat
            for (int i = 0; i < contours.Size; i++)
            {
                VectorOfPoint contour = contours[i]; // chuyen sang Point[][]
                Result1 = CvInvoke.ContourArea(contour, false);// tinh area

                if (Result1 > Result2)
                {
                    Result2 = Result1;
                    biggestContour = contour;
                }
            }

            //Gi do
            try
            {
                lblNote.Text = "";
                if (biggestContour != null)
                {

                    CvInvoke.ApproxPolyDP(biggestContour, biggestContour, 0.00025, false);

                    contourPoints = biggestContour.ToArray();

                    currentFrame.Draw(contourPoints, new Bgr(255, 0, 255), 4);


                    VectorOfPoint hull = new VectorOfPoint();
                    VectorOfInt convexHull = new VectorOfInt();
                    CvInvoke.ConvexHull(biggestContour, hull, true); //~ Hull   //Chinh clockwise thanh true          
                    RotatedRect minAreaBox = CvInvoke.MinAreaRect(hull);


                    currentFrame.Draw(new CircleF(minAreaBox.Center, 5), new Bgr(Color.Black), 4);

                    CvInvoke.ConvexHull(biggestContour, convexHull, true);    //Chinh clockwise thanh true


                    
                    currentFrame.Draw(minAreaBox, new Bgr(200, 0, 0), 1);


                    // ve khung ban tay khung bao quanh tay
                    currentFrame.DrawPolyline(hull.ToArray(), true, new Bgr(200, 125, 75), 4);
                    currentFrame.Draw(new CircleF(new PointF(minAreaBox.Center.X, minAreaBox.Center.Y), 3), new Bgr(200, 125, 75));

                    // tim  convex defect
                    Mat defect = new Mat();
                    CvInvoke.ConvexityDefects(biggestContour, convexHull, defect);

                    // chuyen sang Matrix 

                    if (!defect.IsEmpty)
                    {
                        mDefect = new Matrix<int>(defect.Rows, defect.Cols, defect.NumberOfChannels);
                        defect.CopyTo(mDefect);
                    }

                    #region Counting finger
                    if (mDefect.Rows == 0) return;
                    PointF[] start = new PointF[mDefect.Rows];
                    int num = 0;
                    start[0] = new PointF(0, 0);

                    try
                    {
                        for (int i = 0; i < mDefect.Rows; i++)
                        {
                            int startIdx = mDefect.Data[i, 0];
                            int depthIdx = mDefect.Data[i, 1];
                            int endIdx = mDefect.Data[i, 2];


                            Point startPoint = contourPoints[startIdx];
                            Point endPoint = contourPoints[endIdx];
                            Point depthPoint = contourPoints[depthIdx];

                            CircleF startCircle = new CircleF(startPoint, 5f);
                            CircleF endCircle = new CircleF(endPoint, 5f);
                            CircleF depthCircle = new CircleF(depthPoint, 5f);

                            LineSegment2D Line = new LineSegment2D(startPoint, new Point((int)minAreaBox.Center.X, (int)minAreaBox.Center.Y));


                            //Cach 1
                            //if ((startCircle.Center.Y < minAreaBox.Center.Y || depthCircle.Center.Y < minAreaBox.Center.Y) &&
                            //          (startCircle.Center.Y < depthCircle.Center.Y) &&
                            //          (Math.Sqrt(Math.Pow(startCircle.Center.X - depthCircle.Center.X, 2) +
                            //                     Math.Pow(startCircle.Center.Y - depthCircle.Center.Y, 2)) >
                            //                     minAreaBox.Size.Height / 6.5))
                            //{
                            //    Finger_num++;
                            //}

                            //Cach 2
                            if ((startPoint.Y < minAreaBox.Center.Y && endPoint.Y < minAreaBox.Center.Y) && (startPoint.Y < endPoint.Y) &&
                    (Math.Sqrt(Math.Pow(startPoint.X - endPoint.X, 2) + Math.Pow(startPoint.Y - endPoint.Y, 2)) > minAreaBox.Size.Height / 7))
                            {
                                if (getAngle(startPoint, minAreaBox.Center, start[num]) > 10 )
                                {
                                    Finger_num++;
                                    start[num] = startPoint;
                                    num++;
                                    currentFrame.Draw(Line, new Bgr(Color.Violet), 2);
                                    currentFrame.Draw(startCircle, new Bgr(Color.OrangeRed), 5);
                                    //currentFrame.Draw(endCircle, new Bgr(Color.Black), 5);
                                    //currentFrame.Draw(Finger_num.ToString(), new Point(startPoint.X - 10, startPoint.Y), FontFace.HersheyPlain, 2, new Bgr(Color.Orange), 3);
                                    //currentFrame.Draw(Finger_num.ToString(), new Point(endPoint.X - 10, endPoint.Y), FontFace.HersheyPlain, 2, new Bgr(Color.Orange), 3);


                                }
                            }
                        }
                    }
                    catch
                    {
                        return;
                    }

                    #endregion

                }
                #region Tracking
                MCvMoments moment = new MCvMoments();               // a new MCvMoments object

                try
                {
                    moment = CvInvoke.Moments(biggestContour, false);        // Moments of biggestContour
                }
                catch (NullReferenceException except)
                {
                    //label3.Text = except.Message;
                    return;
                }

                //CvInvoke.cvMoments(biggestContour, ref moment, 0);
                double m_00 = CvInvoke.cvGetSpatialMoment(ref moment, 0, 0);
                double m_10 = CvInvoke.cvGetSpatialMoment(ref moment, 1, 0);
                double m_01 = CvInvoke.cvGetSpatialMoment(ref moment, 0, 1);

                int current_X = Convert.ToInt32(m_10 / m_00) / 10;      // X location of centre of contour              
                int current_Y = Convert.ToInt32(m_01 / m_00) / 10;      // Y location of center of contour

                #endregion

                if (useVirtualMouse)
                {
                    // move cursor to center of contour only if Finger count is 1 or 0
                    // i.e. palm is closed

                    if (Finger_num == 0 || Finger_num == 1)
                    {
                        Cursor.Position = new Point(current_X * 20, current_Y * 20);
                    }

                    // Leave the cursor where it was and Do mouse click, if finger count >= 4

                    if (Finger_num >= 3)
                    {
                        if (!isDrag)
                        {
                            DoMouseDown();
                            isDrag = true;
                        }
                        else
                        {
                            DoMouseUp();
                            isDrag = false;
                        }
                        //Cursor.Position = new Point(current_X * 20, current_Y * 20);
                        //Cursor.Position = new Point(300, 300);             
                    }
                }
            }
            catch
            {
                Finger_num = 0;
                lblNote.Text = "Opps! Make sure to have a 'white space'";
                return;
            }
            #endregion


            //Display image from camera
            picInputCam.Image = currentFrame.ToBitmap();

            lblNumFinger.Text = Finger_num.ToString();
        }

        #region Mouse event for hand gesture
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;          // mouse left button pressed 
        private const int MOUSEEVENTF_LEFTUP = 0x04;            // mouse left button unpressed
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;         // mouse right button pressed
        private const int MOUSEEVENTF_RIGHTUP = 0x10;           // mouse right button unpressed

        //this function will click the mouse using the parameters assigned to it
        public void DoMouseClick()
        {
            //Call the imported function with the cursor's current position
            
            uint X = Convert.ToUInt32(Cursor.Position.X);
            uint Y = Convert.ToUInt32(Cursor.Position.Y);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }

        //void sendMouseRightclick(Point p)
        //{
        //    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, X, Y, 0, 0);
        //}

        //void sendMouseDoubleClick(Point p)
        //{
        //    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, p.X, p.Y, 0, 0);

        //    Thread.Sleep(150);

        //    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, p.X, p.Y, 0, 0);
        //}

        //void sendMouseRightDoubleClick(Point p)
        //{
        //    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, p.X, p.Y, 0, 0);

        //    Thread.Sleep(150);

        //    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, p.X, p.Y, 0, 0);
        //}

        void DoMouseDown()
        {
            uint X = Convert.ToUInt32(Cursor.Position.X);
            uint Y = Convert.ToUInt32(Cursor.Position.Y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, X, Y, 0, 0);
        }

        void DoMouseUp()
        {
            uint X = Convert.ToUInt32(Cursor.Position.X);
            uint Y = Convert.ToUInt32(Cursor.Position.Y);
            mouse_event(MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }

        private void tgVirtualMouse_CheckedChanged(object sender, EventArgs e)
        {
            if (tgVirtualMouse.Checked)
            {
                useVirtualMouse = true;
            }
            else
            {
                useVirtualMouse = false;
            }
        }
        #endregion

        //Ham tim goc
        double getAngle(PointF s, PointF f, PointF e)
        {
            double l1 = Math.Sqrt(Math.Pow(f.X - s.X, 2) + Math.Pow(f.Y - s.Y, 2));
            double l2 = Math.Sqrt(Math.Pow(f.X - e.X, 2) + Math.Pow(f.Y - e.Y, 2));
            float dot = (s.X - f.X) * (e.X - f.X) + (s.Y - f.Y) * (e.Y - f.Y);
            double angle = Math.Acos(dot / (l1 * l2));
            angle = angle * 180 / Math.PI;
            return angle;
        }
        private void btnStart_Click(object sender, EventArgs e)
        {          
            #region
            if (capture == null)
            {
                try
                {
                    capture = new Capture();

                    capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps, 5);              

                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }
            #endregion

            if (capture != null)
            {
                if (captureInProgress)
                {
                    btnStart.Text = "Pause";
                    Application.Idle += ProcessFramAndUpdateGUI;
                }
                else
                {
                    btnStart.Text = "Start";
                    Application.Idle -= ProcessFramAndUpdateGUI;
                }
                captureInProgress = !captureInProgress;
            }
        }
        public void Stop()
        {
            Application.Idle -= ProcessFramAndUpdateGUI;
            capture.Dispose();
        }
        private void pnlSetting_Click(object sender, EventArgs e)
        {
            if (pnlSettingInfo.Visible == false)
            {
                pnlSettingInfo.Visible = true;
            }
            else
                pnlSettingInfo.Visible = false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.M))
            {
                if (tgVirtualMouse.Checked)
                    tgVirtualMouse.Checked = false;
                else
                    tgVirtualMouse.Checked = true;
            }          
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
