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
using Emgu.CV.VideoSurveillance;

/*
 * EmguCV: 2.4.10 
 */
namespace ChromaKeyGame {
    class CameraCapture {

        private Capture capture = null;
        private bool captureInProgress;
        private Image imageElement;
        private MainWindow mWindow;
        private int lowHThreshold = 85;
        private int HighHThresdhold = 155;
        delegate void UpdateImageElementDelegate(BitmapSource temp);

        // Bug: 调试启动的话，内存占用稳定;若运行启动的话，偶尔出现内存飙升并卡死的问题！
        //      
        //      
        public CameraCapture(MainWindow w, Image imgE) {
            //CvInvoke.UseOpenCL = false;
            this.mWindow = w;
            this.imageElement = imgE;
            
            try {
                this.capture = new Capture(0);
                // this.capture = new KinectCapture(KinectCapture.DeviceType.Kinect, KinectCapture.ImageGeneratorOutputMode.Qvga60Hz);
                this.capture.ImageGrabbed += UpdateFrame;
                this.capture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 1280);
                this.capture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 720);
                //CvInvoke.cveVideoCaptureSet(this.capture.Ptr, Emgu.CV.CvEnum.CapProp.FrameWidth, 1280);
                //CvInvoke.cveVideoCaptureSet(this.capture.Ptr, Emgu.CV.CvEnum.CapProp.FrameHeight, 720);
                this.capture.Start();
                
                this.capture.FlipHorizontal = true;
                Console.WriteLine("Start");
                Console.WriteLine(this.capture.Height + " : " + this.capture.Width);

            } catch (NullReferenceException e) {
                Console.WriteLine("Error: CameraCapture, " + e.Message);
                MessageBox.Show(e.Message);
            }
        }

        private void UpdateFrame(object sender, EventArgs arg) {
            
            //Mat frame = new Mat();
            //this.capture.Retrieve(frame, 0);
            
            // convert certain color pixel to transparent
            //Image<Bgr, Byte> originalImage = frame.ToImage<Bgr, byte>();
            Image<Bgr, Byte> originalImage = this.capture.RetrieveBgrFrame();
            //byte[,,] test = originalImage.Data;
            //int i = 1;
            Image<Bgra, Byte> processedImage = this.processTransparent(originalImage,
                                                                        this.capture.Width,
                                                                        this.capture.Height,
                                                                        this.HighHThresdhold,
                                                                        this.lowHThreshold);
            try {
                this.mWindow.Dispatcher.Invoke(
                    new Action(
                        delegate {

                            // convert to bitmapsources for Image
                            BitmapSource originalTemp = BitmapSourceConvert.ToBitmapSource(originalImage);
                            BitmapSource processedTemp = BitmapSourceConvert.ToBitmapSource(processedImage);

                            if (originalImage == null) {
                                Console.WriteLine("Error, ToBitmapSource Exception");
                                this.Stop(); 
                                this.ReleaseData();
                            } else {

                                // display
                                this.mWindow.CameraImage.Source = originalTemp;
                                this.mWindow.ProcessedImage.Source = processedTemp;
                            }
                        }),
                        DispatcherPriority.Normal);
            } catch (Exception e) {
                MessageBox.Show("Error, Show Image" + e.Message);
            }
        }

        private Image<Bgra, Byte> processTransparent(Image<Bgr, Byte> originalImg, int w, int h, int high, int low) {
            // TODO: 增强对比度，增加色彩区别度 ？
            //       目前使用 blur
            Image<Bgra, byte> processedImage = null;
            Image<Bgr, Byte> blurImage = null;
            Image<Hsv, float> blurImageHSV = null;
            try {
                processedImage = originalImg.Convert<Bgra, byte>();
                blurImage = originalImg.SmoothBlur(11, 11, true);
                blurImageHSV = blurImage.Convert<Hsv, float>();
                //CvInvoke.cvSmooth(originalImg, blurImage, SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
                //blurImage._MorphologyEx(, CV_MORPH_OP.)
                // TEST
                //CvInvoke.cvSmooth(blurImage, blurImage2, SMOOTH_TYPE.CV_BLUR, 1, 17, 0, 0);
                //originalImg.Dilate(10);
                //CvInvoke.cvSmooth(originalImg, blurImage, SMOOTH_TYPE.CV_GAUSSIAN, 23, 23, 0, 0);
                //blurImage = blurImage.Dilate(1);
                //CvInvoke.MedianBlur(originalImg, blurImage, 9);
                //CvInvoke.GaussianBlur(originalImg, blurImage,  new System.Drawing.Size(7, 7), 13);
                //CvInvoke.cvCvtColor(blurImage, processedImage, COLOR_CONVERSION.BGR2BGRA);
                //Image<Bgra, float> ori_float1 = new Image<Bgra, float>(w, h);
                //Image<Bgr, float> ori_float2 = new Image<Bgr, float>(w, h);
                //Image<Hsv, float> originalImgHSV = new Image<Hsv, float>(w, h);
                //CvInvoke.cvConvertScale(blurImage, ori_float1, 1.0, 0);
                //CvInvoke.CvtColor(ori_float1, ori_float2, Emgu.CV.CvEnum.ColorConversion.Bgra2Bgr);
                //CvInvoke.CvtColor(ori_float2, originalImgHSV, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);

                for (int i = 0; i < h; i++) {
                    for (int j = 0; j < w; j++) {

                        // 1: get hvs data, and based on hvs'h
                        float hValue = blurImageHSV.Data[i, j, 0];
                        if (hValue > low && hValue < high) {
                            //image.Data[i, j, 0] = 0;
                            //image.Data[i, j, 1] = 0;
                            //image.Data[i, j, 2] = 0;
                            processedImage.Data[i, j, 3] = 0;
                            //Console.WriteLine("Change to transparent");
                        }

                        // 2: use algorithm to find out certain color pixel
                        //if(true) {
                        //    processedImage.Data[i, j, 3] = this.isGreen(blurImage.Data[i, j, 0], blurImage.Data[i, j, 1], blurImage.Data[i, j, 2]);
                        //    //processedImage.Data[i, j, 3] = this.isGreen(processedImage.Data[i, j, 0], processedImage.Data[i, j, 1], processedImage.Data[i, j, 2]);
                        //} 
                     
                    }
                }
                
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            } finally {
                blurImage.Dispose();
                blurImageHSV.Dispose();
                //originalImg.Dispose();
            }

            return processedImage;
        }

        private byte isGreen(byte bb, byte bg, byte br) {
            float b = bb / 255f;
            float g = bg / 255f;
            float r = br / 255f;
            float a = 1;
            // Calculate the average intensity of the texel's red and blue components
            float rbAverage = (r + b) * 0.75f;

            // Calculate the difference between the green element intensity and the
            // average of red and blue intensities
            float gDelta = g - rbAverage;

            // If the green intensity is greater than the average of red and blue
            // intensities, calculate a transparency value in the range 0.0 to 1.0
            // based on how much more intense the green element is
            //a = 1.0f - gDelta > 0.4f ? (gDelta < 1f ? gDelta : 1f) : 0;
            a = gDelta > 0 ? 0 : 1;
            return (byte)(a * 255);

            // Use the cube of the of the transparency value. That way, a fragment that
            // is partially translucent becomes even more translucent. This sharpens
            // the final result by avoiding almost but not quite opaque fragments that
            // tend to form halos at color boundaries.
            // a = a * a * a;
            // return (byte)(a * 255);
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
