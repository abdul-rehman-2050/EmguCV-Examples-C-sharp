using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;




namespace EMGU_test
{
    public partial class BlobDetection : Form
    {

        

        private VideoCapture _capture = null;
        private bool _captureInProgress;
        private MotionHistory _motionHistory;
        private IBackgroundSubtractor _forgroundDetector;


        delegate void SetTextCallback(string text);
      
        
        public BlobDetection()
        {
            InitializeComponent();
            _motionHistory = new MotionHistory(
               1.0, //in second, the duration of motion history you wants to keep
               0.05, //in second, maxDelta for cvCalcMotionGradient
               0.5); //in second, minDelta for cvCalcMotionGradient
         
        }            

        private void loadVideo()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "WMV Files (*.wmv)|*.wmv|mp4 Files (*.mp4)|*.mp4|All files (*.*)|*.*";

            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _capture = null;
                    _capture = new VideoCapture(openFileDialog1.FileName.ToString());
                    TotalFrame = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
                    Fps = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
                    label1.Text = "Frame count = " + TotalFrame + ", Fps: " + Fps;
                    
                    _capture.ImageGrabbed += ProcessFrame;
                    curFrameCount = 0;

                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }                    

        }

     

        private Mat frame = new Mat();
        private Mat smoothedFrame = new Mat();
        private Mat _segMask = new Mat();
        private Mat _forgroundMask = new Mat();
      
        private double TotalFrame;
        private double Fps;
        private double curFrameCount = 0;
        Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
        Mat hier = new Mat();
           
        
        private void ProcessFrame(object sender, EventArgs arg)
        {
          


            _capture.Retrieve(frame);
            if (frame != null)
            {

                //CvInvoke.Resize(frame, frame, new Size(imageBox1.Width, imageBox1.Height), 0, 0, Inter.Linear);    //This resizes the image to the size of Imagebox1 
                CvInvoke.Resize(frame, frame, new Size(640, 480), 0, 0, Inter.Linear); 
                //CvInvoke.GaussianBlur(frame, smoothedFrame, new Size(3, 3), 1); //filter out noises
                if (_forgroundDetector == null)
                {
                    _forgroundDetector = new BackgroundSubtractorMOG2();
                }

                _forgroundDetector.Apply(frame, _forgroundMask);
                //CvInvoke.Canny(_forgroundMask, smoothedFrame, 100, 60);

                //CvInvoke.Threshold(_forgroundMask, smoothedFrame, 0, 255, Emgu.CV.CvEnum.ThresholdType.Otsu | Emgu.CV.CvEnum.ThresholdType.Binary);
                contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
                CvInvoke.FindContours(_forgroundMask, contours, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                if (contours.Size > 0)
                {
                    for (int i = 0; i < contours.Size; i++)
                    {
                        double area = CvInvoke.ContourArea(contours[i]);
                        Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);

                        if (area > 300 && rect.Y>200 && area<3500 && rect.X>100 && rect.X<400)
                        {
                            CvInvoke.Rectangle(frame, rect, new MCvScalar(0, 255, 0), 6);
                            
                        }
                    }
                }


                //CvInvoke.DrawContours(frame, contours, -1, new MCvScalar(255, 0, 0));



                imageBox1.Image = frame;
                curFrameCount++;
                if (curFrameCount >= TotalFrame - 10)
                {
                    updateButton2Text("Load Video");
                    _capture.Stop();
                    _capture.Dispose();
                    _captureInProgress = false;
                    _capture = null;
                }
                updateCurFrameCount("Current Frame # " + curFrameCount);
               

            }
            else
            {
                _capture.Pause();
                _capture.ImageGrabbed -= ProcessFrame;
                MessageBox.Show("null frame");
                
                

            }
            
            
            
        }

        


        #region "update text and labels controls etc"

        private void UpdateText(String text)
        {
            if (!IsDisposed && !Disposing && InvokeRequired)
            {
                Invoke((Action<String>)UpdateText, text);
            }
            else
            {
                label3.Text = text;
            }
        }


        private void updateButton2Text(string text)
        {

            if (this.button2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(updateButton2Text);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.button2.Text = text;              

            }
            
        
        }

        private void updateCurFrameCount(string text)
        {

            if (this.label2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(updateCurFrameCount);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.label2.Text = text;
            }
            
            
            

        }

        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            if (_capture != null)
            {
                if (_captureInProgress)
                {  //stop the capture
                    button2.Text = "Start";
                    _capture.Pause();
                }
                else 
                {
                                       
                    button2.Text = "Pause";
                    _capture.Start();
                }

                _captureInProgress = !_captureInProgress;
            }
            else
            {

                loadVideo();
                button2.Text = "Pause";
                _capture.Start();
                _captureInProgress = true;
            
            }
        }

    }
}
