//
//  CameraCapture
//  TestEmgucv
//
//  Created by BillHuang on 2015/5/11 10:30:36.
//  Copyright (c) Bill. All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.Util;
using Emgu.CV.VideoSurveillance;

namespace ChromaKeyGame {
    class CameraCapture {

        private Capture capture = null;
        private bool captureInProgress;
        private Image imageElement;
        private MainWindow mWindow;
        private int lowHThreshold = 80;
        private int HighHThresdhold = 120;
        delegate void UpdateImageElementDelegate(BitmapSource temp);
        public CameraCapture(MainWindow w, Image imgE) {
            CvInvoke.UseOpenCL = false;
            this.mWindow = w;
            this.imageElement = imgE;
            
            try {
                this.capture = new Capture(0);
                this.capture.ImageGrabbed += UpdateFrame;
                this.capture.Start();
                this.capture.FlipHorizontal = true;
                Console.WriteLine("Start");

            } catch (NullReferenceException e) {
                Console.WriteLine("Error: CameraCapture, " + e.Message);
                //MessageBox.Show(excpt.Message);
            }
        }

        private void UpdateFrame(object sender, EventArgs arg) {
           
            Mat frame = new Mat();
            this.capture.Retrieve(frame, 0);
            
            // get hvs data, and based on hvs'h, convert blue bg to transparent
            Image<Bgra, Byte> originalImage = frame.ToImage<Bgra, byte>();

            Image<Bgra, Byte> processedImage = this.processTransparent(originalImage,
                                                                        frame.Width,
                                                                        frame.Height,
                                                                        this.HighHThresdhold,
                                                                        this.lowHThreshold);
            try {
                this.mWindow.Dispatcher.Invoke( 
                    new Action(
                        delegate {

                            // convert to bitmapsources for Image
                            BitmapSource originalTemp = BitmapSourceConvert.ToBitmapSource(originalImage);
                            BitmapSource processedTemp = BitmapSourceConvert.ToBitmapSource(processedImage);

                            if (originalImage == null || processedTemp == null) {
                                Console.WriteLine("Error, ToBitmapSource Exception");
                                this.Stop(); 
                                this.ReleaseData();
                            } else {

                                // display
                                this.mWindow.BGImage.Source = originalTemp;
                                this.mWindow.CameraImage.Source = processedTemp;
                            }
                        }),
                        DispatcherPriority.Normal);
            } catch (Exception e) {
                Console.WriteLine("Error, Show Image" + e.Message);
            }
            //Mat grayFrame = new Mat();
            //CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);
            //Mat smallGrayFrame = new Mat();
            //CvInvoke.PyrDown(grayFrame, smallGrayFrame);
            //Mat smoothedGrayFrame = new Mat();
            //CvInvoke.PyrUp(smallGrayFrame, smoothedGrayFrame);

            ////Image<Gray, Byte> smallGrayFrame = grayFrame.PyrDown();
            ////Image<Gray, Byte> smoothedGrayFrame = smallGrayFrame.PyrUp();
            //Mat cannyFrame = new Mat();
            //CvInvoke.Canny(smoothedGrayFrame, cannyFrame, 100, 60);

            //Image<Gray, Byte> cannyFrame = smoothedGrayFrame.Canny(100, 60);
            
            //captureImageBox.Image = frame;
            //grayscaleImageBox.Image = grayFrame;
            //smoothedGrayscaleImageBox.Image = smoothedGrayFrame;
            //cannyImageBox.Image = cannyFrame;
        }

        private Image<Bgra, Byte> processTransparent(Image<Bgra, Byte> originalImg, int w, int h, int high, int low) {
            Image<Bgra, Byte> blurImage = new Image<Bgra, byte>(w, h);
            //blurImage = originalImg;
            
            CvInvoke.MedianBlur(originalImg, blurImage, 17);
            //CvInvoke.GaussianBlur(originalImg, blurImage,  new System.Drawing.Size(7, 7), 13);

            Image<Bgra, Byte> processedImage = originalImg.Copy();
            Image<Bgra, float> ori_float1 = new Image<Bgra, float>(w, h);
            Image<Bgr, float> ori_float2 = new Image<Bgr, float>(w, h);
            Image<Hsv, float> originalImgHSV = new Image<Hsv, float>(w, h);
            CvInvoke.cvConvertScale(blurImage, ori_float1, 1.0, 0);
            CvInvoke.CvtColor(ori_float1, ori_float2, Emgu.CV.CvEnum.ColorConversion.Bgra2Bgr);
            CvInvoke.CvtColor(ori_float2, originalImgHSV, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);

            for (int i = 0; i < 480; i++) {
                for (int j = 0; j < 640; j++) {
                    float hValue = originalImgHSV.Data[i, j, 0];
                    if (hValue > low && hValue < high) {
                        //image.Data[i, j, 0] = 0;
                        //image.Data[i, j, 1] = 0;
                        //image.Data[i, j, 2] = 0;
                        processedImage.Data[i, j, 3] = 0;
                        //Console.WriteLine("Change to transparent");
                    }
                    //Console.WriteLine(h);
                }
            }
            return processedImage;
            //return blurImage;
        }

        private void UpdateImageElement(BitmapSource temp) {
            
        }

        private void captureButtonClick(object sender, EventArgs e) {
            if (this.capture != null) {
                if (this.captureInProgress) {  //stop the capture
                    //captureButton.Text = "Start Capture";
                    this.capture.Pause();
                } else {
                    //start the capture
                    //captureButton.Text = "Stop";
                    this.capture.Start();
                }

                this.captureInProgress = !this.captureInProgress;
            }
        }

        public void Stop() {
            this.capture.Stop();
        }

        public void ReleaseData() {
            if (this.capture != null)
                this.capture.Dispose();
        }

        private void FlipHorizontalButtonClick(object sender, EventArgs e) {
            if (this.capture != null) this.capture.FlipHorizontal = !this.capture.FlipHorizontal;
        }

        private void FlipVerticalButtonClick(object sender, EventArgs e) {
            if (this.capture != null) this.capture.FlipVertical = !this.capture.FlipVertical;
        }
    }
}
