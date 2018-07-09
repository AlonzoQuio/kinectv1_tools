using System.Threading;

namespace Microsoft.Samples.Kinect.DepthBasics
{
    class RecordHandler
    {
        RecordThread record=null;
        public RecordHandler(RecordThread record)
        {
            this.record = record;
        }
        public void Start()
        {
            if (record != null)
            {
                Thread thread = new Thread(record.DoWork);
                thread.Start();
            }

        }
        public bool Close()
        {
            bool status = false;
            if (record != null)
            {
                record.RequestStop();
                status = true;
            }
            return status;
        }
        public void EnableRecord()
        {
            record.EnableRecord();
        }
        public void DisableRecord()
        {
            record.DisableRecord();
        }
    }
}
