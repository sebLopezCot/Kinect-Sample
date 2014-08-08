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
using Microsoft.Research.Kinect.Audio;
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;

namespace Kinect_Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Runtime nui;

        int sliderValue, tiltValue;

        Boolean rgbImageExpanded, depthImageExpanded, leftImageExpanded;
        Boolean dualCameraModeEnabled, rgbImageWasExpanded, depthImageWasExpanded;


        private void SetupKinect() 
        {
            if (Runtime.Kinects.Count == 0)
            {
                this.Title = "No Kinect connected";
                updateTiltButton.IsEnabled = false;
            }
            else 
            {
                // use the first kinect
                nui = Runtime.Kinects[0];
                nui.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseDepth);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // sets up the Kinect RGB and Depth Cameras along with Initialization of the Sensor
            SetupKinect();
            nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_VideoFrameReady);
            nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
            nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
            nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.Depth);

            tiltValue = 0; // sets the tilt value to neutral
            
            // Expanding initialization
            rgbImageExpanded = false;
            depthImageExpanded = false;
            leftImageExpanded = false;
            dualCameraModeEnabled = false;
            rgbImageWasExpanded = false;
            depthImageWasExpanded = false;

            // Sets the Kinect tilt to 0
            nui.NuiCamera.ElevationAngle = 0;
        }

        #region FrameReady Event Listeners

        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            if (!rgbImageExpanded && !depthImageExpanded)
            {
                rightImage.Source = e.ImageFrame.ToBitmapSource();
            }
            else if (!rgbImageExpanded && depthImageExpanded) 
            {
                leftImage.Source = e.ImageFrame.ToBitmapSource();
            }
            else if (rgbImageExpanded && !depthImageExpanded) 
            {
                rightImage.Source = e.ImageFrame.ToBitmapSource();
            }
            else if (rgbImageExpanded && depthImageExpanded) // Dual Camera Mode
            {
                leftImage.Source = e.ImageFrame.ToBitmapSource();
            }
        }

        void nui_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            if (!rgbImageExpanded && !depthImageExpanded)
            {
                leftImage.Source = e.ImageFrame.ToBitmapSource();
            }
            else if (rgbImageExpanded && !depthImageExpanded)
            {
                leftImage.Source = e.ImageFrame.ToBitmapSource();
            }
            else if (!rgbImageExpanded && depthImageExpanded)
            {
                rightImage.Source = e.ImageFrame.ToBitmapSource();
            }
            else if (rgbImageExpanded && depthImageExpanded) // Dual Camera Mode
            {
                leftImage.Source = e.ImageFrame.ToBitmapSource();
            }
        }

        #endregion

        # region Tilt Logic

        private void tiltSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sliderValue = (int)tiltSlider.Value;
            sliderValueLabel.Content = "Slider Value:      " + sliderValue.ToString();
        }

        private void updateTiltButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // updates the tilt angle and values
                nui.NuiCamera.ElevationAngle = sliderValue;
                tiltValue = sliderValue;
                tiltValueLabel.Content = "Current Value:   " + sliderValue.ToString();
            }
            catch (Exception ex) { }
        }

        private void resetTiltButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // sets everything to the initial value
                nui.NuiCamera.ElevationAngle = 0;
                tiltValue = 0;
                sliderValue = 0;
                tiltSlider.Value = 0;
                sliderValueLabel.Content = "Slider Value:      0";
                tiltValueLabel.Content = "Current Value:   0";
            }
            catch (Exception ex) { }
        }

        #endregion

        # region Expanding Images/Camera Modes

        private void expandLeftImage() 
        {
            leftImage.Width = 640;
            leftImage.Height = 480;
            dualCameraModeButton.Visibility = Visibility.Visible;
        }

        private void contractLeftImage() 
        {
            leftImage.Width = 320;
            leftImage.Height = 240;
            dualCameraModeButton.Visibility = Visibility.Hidden;
        }

        private void dualCameraModeButton_Click(object sender, RoutedEventArgs e)
        {
            // Dual Camera Mode Logic
            if (dualCameraModeEnabled)
            {
                leftImageExpanded = true;
                rgbImageExpanded = false;
                depthImageExpanded = false;

                if (rgbImageWasExpanded) 
                {
                    rgbImageExpanded = true;
                    rgbImageWasExpanded = false;
                }

                if (depthImageWasExpanded) 
                {
                    depthImageExpanded = true;
                    depthImageWasExpanded = false;
                }

                dualCameraModeEnabled = false;
                dualCameraModeButton.Content = "Enable Dual Camera Mode";
                rightImage.Visibility = Visibility.Visible;
            }
            else 
            {
                if (rgbImageExpanded) 
                {
                    rgbImageWasExpanded = true;
                }

                if (depthImageExpanded) 
                {
                    depthImageWasExpanded = true;
                }

                rgbImageExpanded = true;
                depthImageExpanded = true;
                dualCameraModeEnabled = true;
                dualCameraModeButton.Content = "Disable Dual Camera Mode";
                rightImage.Visibility = Visibility.Hidden;
            }
        }

        private void leftImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Expanding Logic(Left Image Clicked)
            if (leftImageExpanded == false)
            {
                expandLeftImage();
                leftImageExpanded = true;
                rgbImageExpanded = true;
            }
            else 
            {
                contractLeftImage();
                leftImageExpanded = false;
                rgbImageExpanded = false;
                depthImageExpanded = false;
                dualCameraModeEnabled = false;
                dualCameraModeButton.Content = "Enable Dual Camera Mode";
                rightImage.Visibility = Visibility.Visible;
                rgbImageWasExpanded = false;
                depthImageWasExpanded = false;
            }
        }

        private void rightImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Expanding logic(Right Image Clicked)
            if (depthImageExpanded == true)
            {
                depthImageExpanded = false;
                rgbImageExpanded = true;
            }
            else if (!depthImageExpanded && rgbImageExpanded)
            {
                rgbImageExpanded = false;
                depthImageExpanded = true;
            }

            if (leftImageExpanded == false)
            {
                expandLeftImage();
                leftImageExpanded = true;
                depthImageExpanded = true;
            }
        }

        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {
            nui.Uninitialize();
        }

    }
}
