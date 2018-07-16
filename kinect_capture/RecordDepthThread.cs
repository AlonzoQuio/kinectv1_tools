using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Microsoft.Samples.Kinect.DepthBasics
{
    class RecordDepthThread
    {
        MainWindow window;
        bool _shouldStop;
        bool _recording;
        int frame_d;
        string loaded_at;
        public RecordDepthThread(MainWindow window, string loaded_at)
        {
            _shouldStop = false;
            _recording = false;
            this.window = window;
            frame_d = 0;
            this.loaded_at = loaded_at;
        }
        public void DoWork()
        {
            _shouldStop = false;
            while (!_shouldStop)
            {
                if (_recording && window.readDepth)
                {
                    BitmapEncoder encoder_d = new PngBitmapEncoder();

                    // create frame from the writable bitmap and add to encoder
                    //window.depthBitmap.Freeze();
                    //window.Dispatcher.Invoke(new Action(() => window.depthBitmap.Lock()));
                    //WriteableBitmap temp = window.depthBitmap.Clone();
                    encoder_d.Frames.Add(BitmapFrame.Create(window.depthBitmap));

                    //string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

                    //string myPhotos = "";// Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                    //string path_d = Path.Combine("", loaded_at + "_" + frame_d + "_depth.png");
                    string path_d = loaded_at + frame_d + "_depth.png";
                    // write the new file to disk
                    try
                    {
                        using (FileStream fs = new FileStream(path_d, FileMode.Create))
                        {
                            encoder_d.Save(fs);
                        }

                        //this.statusBarText.Text = string.Format("{0} {1}", Properties.Resources.ScreenshotWriteSuccess, path);
                    }
                    catch (IOException)
                    {
                        //this.statusBarText.Text = string.Format("{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
                    }

                    //frame_d+=2;
                    frame_d++;
                    window.readDepth = false;
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
            frame_d = 0;
        }
    }
}
