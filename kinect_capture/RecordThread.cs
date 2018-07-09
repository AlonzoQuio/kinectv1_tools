using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Microsoft.Samples.Kinect.DepthBasics
{
    class RecordThread
    {
        MainWindow window;
        bool _shouldStop;
        bool _recording;
        int frame;
        string loaded_at;
        public RecordThread(MainWindow window,string loaded_at) {
            _shouldStop = false;
            _recording = false;
            this.window = window;
            frame = 0;
            this.loaded_at=loaded_at;
        }
        public void DoWork()
        {
            _shouldStop = false;
            while (!_shouldStop)
            {
                if (_recording && window.readDepth && window.readRgb) {

                    // create a png bitmap encoder which knows how to save a .png file
                    BitmapEncoder encoder = new PngBitmapEncoder();

                    // create frame from the writable bitmap and add to encoder
                    encoder.Frames.Add(BitmapFrame.Create(window.colorBitmap_temp));
                    
                    string path = loaded_at + "_" + frame + "_rgb.png";
                    // write the new file to disk
                    try
                    {
                        using (FileStream fs = new FileStream(path, FileMode.Create))
                        {
                            encoder.Save(fs);
                        }
                    }
                    catch (IOException)
                    {
                    }
                    // create a png bitmap encoder which knows how to save a .png file
                    BitmapEncoder encoder_d = new PngBitmapEncoder();

                    encoder_d.Frames.Add(BitmapFrame.Create(window.depthBitmap));

                    path = loaded_at + "_" + frame + "_depth.png";
                    // write the new file to disk
                    try
                    {
                        using (FileStream fs = new FileStream(path, FileMode.Create))
                        {
                            encoder_d.Save(fs);
                        }
                    }
                    catch (IOException)
                    {
                    }

                    frame++;
                    window.readDepth = false;
                    window.readRgb = false;
                }
            }
            Console.WriteLine("Thread finished");
        }
        public void RequestStop()
        {
            _shouldStop = true;
        }
        public void EnableRecord() {
            _recording = true;
        }
        public void DisableRecord()
        {
            _recording = false;
        }
    }
}
