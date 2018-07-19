using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Microsoft.Samples.Kinect.DepthBasics
{
    class RecordRgbThread
    {
        MainWindow window;
        bool _shouldStop;
        bool _recording;
        int frame;
        string loaded_at;
        public RecordRgbThread(MainWindow window, string loaded_at)
        {
            _shouldStop = false;
            _recording = false;
            this.window = window;
            frame = 0;
            this.loaded_at = loaded_at;
        }
        public void DoWork()
        {
            _shouldStop = false;
            while (!_shouldStop)
            {
                if (_recording && window.readRgb)
                {
                    // create a png bitmap encoder which knows how to save a .png file
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    //BitmapEncoder encoder = new BmpBitmapEncoder();

                    // create frame from the writable bitmap and add to encoder
                    encoder.Frames.Add(BitmapFrame.Create(window.colorBitmap_temp));

                    string path = loaded_at + "rgb\\"+ frame + ".png";
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

                    //frame+=5;
                    frame++;
                    window.readRgb = false;
                }
            }
            Console.WriteLine("Thread finished");
        }
        public void RequestStop()
        {
            _shouldStop = true;
        }
        public void EnableRecord()
        {
            _recording = true;
        }
        public void DisableRecord()
        {
            _recording = false;
        }
        public void UpdatePath(string path)
        {
            loaded_at = path;
            frame = 0;
        }
    }
}
