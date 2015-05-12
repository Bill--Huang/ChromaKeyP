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
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;

namespace ChromaKeyGame {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        private CameraCapture capture = null;
        public MainWindow() {
            InitializeComponent();
        }

        private void image1_Initialized(object sender, EventArgs e) {
            //Image<Bgr, Byte> image = new Image<Bgr, byte>(400, 100, new Bgr(255, 255, 255));
            //image.Draw("Hello, world", new System.Drawing.Point(10, 50), Emgu.CV.CvEnum.FontFace.HersheyPlain, 3.0, new Bgr(255.0, 0.0, 0.0));

            //image1.Source = BitmapSourceConvert.ToBitmapSource(image);
        }


        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.capture = new CameraCapture(this, this.CameraImage);
        } 

        private void Window_Closed(object sender, EventArgs e) {
            if (this.capture != null) {
                this.capture.Stop();
                this.capture.ReleaseData();
            }
        }
    }
}
