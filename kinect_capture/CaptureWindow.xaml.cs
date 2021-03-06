﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DepthBasics
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;

    /// Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        /// Active Kinect sensor
        private KinectSensor sensor;

        /// Bitmap that will hold color information
        public WriteableBitmap depthBitmap;
        public WriteableBitmap colorBitmap;
        public WriteableBitmap colorBitmap_temp;

        //private WriteableBitmap depthBitmap = null;

        /// Intermediate storage for the depth data received from the camera
        private short[] depthPixels;
        private ushort[] raw = null;

        /// Intermediate storage for the depth data converted to color
        private byte[] colorDepthPixels;

        private byte[] colorPixels;

        private bool recording =false;
        private string folder_path;
        RecordRgbHandler rgb_handler;
        RecordRgbThread rgb_thread;
        RecordDepthHandler depth_handler;
        RecordDepthThread depth_thread;
        public bool readDepth=false;
        public bool readRgb=false;

        private void UpdateFolder() {
            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string loaded_at = System.DateTime.Now.ToString("HHmmss", CultureInfo.CurrentUICulture.DateTimeFormat);
            folder_path = Path.Combine(myPhotos, "KinectData", DateTime.Now.ToString("yyyy_MM_dd_")+loaded_at);
            //folder_path = Path.Combine(;
            if (Directory.Exists(folder_path))
            {
                Console.WriteLine("That path exists already.");
                return;
            }
            Console.WriteLine("Creating path " + folder_path);
            // Try to create the directory.
            Directory.CreateDirectory(folder_path+"\\rgb");
            Directory.CreateDirectory(folder_path + "\\depth");
            rgb_handler.UpdatePath(folder_path+"\\");
            depth_handler.UpdatePath(folder_path+"\\");
        }
        public MainWindow()
        {
            InitializeComponent();

            rgb_thread = new RecordRgbThread(this, folder_path);
            rgb_handler = new RecordRgbHandler(rgb_thread);
            rgb_handler.Start();
            depth_thread = new RecordDepthThread(this, folder_path);
            depth_handler = new RecordDepthHandler(depth_thread);
            depth_handler.Start();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the depth stream to receive depth frames

                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                this.sensor.ColorStream.Enable(ColorImageFormat.YuvResolution640x480Fps15);
                //this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);

                // Allocate space to put the depth pixels we'll receive
                this.depthPixels = new short[this.sensor.DepthStream.FramePixelDataLength];
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // Allocate space to put the color pixels we'll create
                this.colorDepthPixels = new byte[this.sensor.DepthStream.FramePixelDataLength * sizeof(int)];

                // This is the bitmap we'll display on-screen
                this.depthBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray16, null);
                
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                this.colorBitmap_temp = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                this.raw = new ushort[this.sensor.ColorStream.FrameWidth* this.sensor.ColorStream.FrameHeight];

                // Set the image we display to point to the bitmap where we'll put the image data
                //this.ImageDepth.Source = this.depthBitmap;
                this.ImageColor.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new depth frame data
                this.sensor.DepthFrameReady += this.SensorDepthFrameReady;
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                    this.sensor.DepthStream.Range = DepthRange.Near;
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// Execute shutdown tasks
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
            Console.WriteLine("Close press");
            rgb_handler.Close();
            depth_handler.Close();
        }

        /// Event handler for Kinect sensor's DepthFrameReady event
        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                
                if (depthFrame != null && recording && !readDepth)
                {
                    short max_value = 0;
                    short min_value = 32767;
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyPixelDataTo(this.depthPixels);

                    // Convert the depth to RGB
                    for (int i = 0; i < this.depthPixels.Length; ++i){
                        // discard the portion of the depth that contains only the player index
                        short depth = (short)(this.depthPixels[i] >> DepthImageFrame.PlayerIndexBitmaskWidth);
                        //ushort depth = (depthPixels[i] >> 3);
                        if (depth > max_value){
                            max_value = depth;
                        }
                        if (depth < min_value){
                            min_value = depth;
                        }
                        //raw[i] = depth;
                        //raw[i] = (ushort)(i/10);
                        //raw[i]++;
                        raw[i] = (ushort)((depth+1)*10);
                        
                    }

                    // Write the pixel data into our bitmap

                    this.depthBitmap = null;
                    GC.Collect();
                    this.depthBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray16, null);

                    this.depthBitmap.WritePixels(
                        new Int32Rect(0, 0, depthBitmap.PixelWidth, depthBitmap.PixelHeight),
                        raw,
                        depthBitmap.PixelWidth *2,
                        0);
                    this.depthBitmap.Freeze();
                    readDepth = true;
                    Console.WriteLine(min_value + " - " + max_value);
                }
            }
        }

        /// Event handler for Kinect sensor's ColorFrameReady event
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    if (!(recording && readRgb))
                    {
                        // Copy the pixel data from the image to a temporary array
                        colorFrame.CopyPixelDataTo(this.colorPixels);

                        if (this.colorBitmap.IsFrozen) {
                            this.colorBitmap_temp = null;
                            GC.Collect();
                            this.colorBitmap_temp = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                        }
                        // Write the pixel data into our bitmap
                        this.colorBitmap.WritePixels(
                            new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                            this.colorPixels,
                            this.colorBitmap.PixelWidth * sizeof(int),
                            0);
                        if (recording)
                        {
                            colorBitmap_temp = colorBitmap.Clone();
                            colorBitmap_temp.Freeze();
                            readRgb = true;
                        }
                    }
                    
                }
            }
        }
        
        /// <summary>
        /// Handles the checking or unchecking of the near mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxRecordChanged(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null)
            {
                // will not function on non-Kinect for Windows devices
                try
                {
                    if (this.checkBoxRecord.IsChecked.GetValueOrDefault())
                    {
                        recording = true;
                        UpdateFolder();
                        rgb_handler.EnableRecord();
                        depth_handler.EnableRecord();
                        Console.WriteLine("Start record");
                        this.statusBarText.Text = "GRABANDO....";
                    }
                    else
                    {
                        recording = false;
                        rgb_handler.DisableRecord();
                        depth_handler.DisableRecord();
                        Console.WriteLine("Pause");
                        this.statusBarText.Text = "EN PAUSA....";
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }
        /// <summary>
        /// Handles the checking or unchecking of the near mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxNearModeChanged(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null)
            {
                // will not function on non-Kinect for Windows devices
                try
                {
                    if (this.checkBoxNearMode.IsChecked.GetValueOrDefault())
                    {
                        this.sensor.DepthStream.Range = DepthRange.Near;
                        //recording = true;
                    }
                    else
                    {
                        this.sensor.DepthStream.Range = DepthRange.Default;
                        //recording = false;
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }
    }
}