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
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;




namespace EMGU_test
{
    public partial class VideoCaptureFromFile : Form
    {

        

        private VideoCapture _capture = null;
        private bool _captureInProgress;
        delegate void SetTextCallback(string text);
      
        
        public VideoCaptureFromFile()
        {
            InitializeComponent();           
         
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
                    // _capture.ImageGrabbed += new EventHandler(_capture_ImageGrabbed);
                    _capture.ImageGrabbed += ProcessFrame;
                    curFrameCount = 0;

                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }                      


        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        Mat frame = new Mat();
        Mat smoothedFrame = new Mat();
        private double TotalFrame;
        private double Fps;
        private double curFrameCount = 0;
           
        
        private void ProcessFrame(object sender, EventArgs arg)
        {
          //   _capture.Read(frame);


            _capture.Retrieve(frame);
            if (frame != null)
            {



                CvInvoke.GaussianBlur(frame, smoothedFrame, new Size(3, 3), 1); //filter out noises
                //pictureBox1.Image = frame.Bitmap;
                imageBox1.Image = smoothedFrame;
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
                    //start the capture
                    //loadVideo();
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
