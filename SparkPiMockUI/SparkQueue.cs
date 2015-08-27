using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SparkPiMockUI
{
    public class SparkQueue
    {
        public delegate void SparkQueueEventHandler(object sender, EventArgs e);
        public event SparkQueueEventHandler DataReadyForPickUp;
        public int QueueCycleMilliSeconds { get; set; }
        public string SubDirectoryPath { get; set; }
        public string DataFileName { get; set; }
        public string FullFilePath
        {
            get { return System.IO.Directory.GetCurrentDirectory() + "\\" + DataFileName; }
        }
        private Queue inboundQueue;
        private Queue outboundQueue;
        private Timer InboundDataTimer;
        private Timer OutboundDataTimer;
        private Object FILELOCK = new Object();
      

        public SparkQueue()
        {
            //SubDirectoryPath = subDirectory;
            //DataFileName = fileName;

         
            if(File.Exists(FullFilePath)==false)
            {
                FileStream fs = File.Create(FullFilePath);
                fs.Flush();
                fs.Dispose();
            }

            //this.file = folder.CreateFileAsync("SparkQueueDB.txt", CreationCollisionOption.OpenIfExists).GetResults();
            //file = folder.GetFileAsync("SparkQueueDB.txt").GetResults();

            QueueCycleMilliSeconds = 250;

            initializeClass();
        }
        private void initializeClass()
        {
            inboundQueue = new Queue();
            outboundQueue = new Queue();

            //if (Program.strPowerOuttageMissedDownEvent.Length > 1)
            //{
            //    this.Enqueue(Program.strPowerOuttageMissedDownEvent);
            //    Program.strPowerOuttageMissedDownEvent = "";
            //}

            InboundDataTimer = new Timer(new TimerCallback(ProcessInboundEventAsync), new Object(), 250, 250);
            OutboundDataTimer = new Timer(new TimerCallback(ProcessOutboundEvent), new Object(), 250, 250);
            
            
        }

        private  void ProcessInboundEventAsync(object o)
        {
            
            Debug.Print("Check For Inbound");
            while (inboundQueue.Count > 0)
            {
                Debug.Print("YES - Inbound Exists");

                var line = inboundQueue.Peek().ToString();
                if (writeDataToFile(line))
                {
                    inboundQueue.Dequeue();
                }

            }
        }
        private void ProcessOutboundEvent(object o)
        {
            //Debug.Print("Check For Outbound");

            if (outboundQueue.Count == 0)
            {
                //Debug.Print("There is nothing in Outbound Queue.  Check if there is anything on the SD Card");

                readDataFromFileAsync();
            }
            else
            {
                if (DataReadyForPickUp != null)
                {
                    Debug.WriteLine("Firing DataReadyForPickUp");
                    DataReadyForPickUp(this, new EventArgs());
                }
            }
        }
        private bool writeDataToFile(string line)
        {
            bool result = false;
            if (File.Exists(FullFilePath))
            {
                lock (FILELOCK)
                {
                    using (StreamWriter writer = new StreamWriter(FullFilePath, true))
                    {
                        writer.WriteLine(line);
                        writer.Flush();
                        result = true;
                    }
                }
            }
            return result;
        }
        private void readDataFromFileAsync()
        {
            if (File.Exists(FullFilePath))
            {
                var line = "";


                try
                {

                    using (StreamReader reader = new StreamReader(FullFilePath))
                    {
                        line = reader.ReadLine();
                        reader.Dispose();
                    }
                }
                catch (System.IO.IOException ex)
                {
                    throw;
                }
                if (line != null)
                {
                    Debug.WriteLine("There is something on the SD Card.  Add it to the outbound queue and fire DataReadyForPickup");
                    outboundQueue.Enqueue(line);
                    if (DataReadyForPickUp != null)
                    {
                        DataReadyForPickUp(this, new EventArgs());
                    }
                }
            }
        }
        private bool removeDataFromFile(string lineToRemove)
        {
            var result = false;
            if (File.Exists(FullFilePath))
            {
                lock (FILELOCK)
                {
                    var lines = new ArrayList();
                    using (var reader = new StreamReader(FullFilePath))
                    {
                        string line = "";
                        while ((line = reader.ReadLine()) != null)
                        {
                            lines.Add(line);
                        }
                    }
                    using (StreamWriter writer = new StreamWriter(FullFilePath))
                    {
                        foreach (var l in lines)
                        {
                            if (l.ToString() != lineToRemove)
                            {
                                writer.WriteLine(l);
                                writer.Flush();
                            }
                            else
                            {
                                Debug.Print("LINE BEING REMOVED");
                            }
                        }
                        result = true;
                    }
                }
            }
            return result;
        }
        public void Enqueue(string textToAdd)
        {
            if (!inboundQueue.Contains(textToAdd) && textToAdd != null)
            {
                inboundQueue.Enqueue(textToAdd);
            }
        }
        public bool Dequeue()
        {
            bool blnSuccess = false;
            var line = "";
            if (outboundQueue.Count > 0)
            {
                line = outboundQueue.Peek().ToString();
                if (removeDataFromFile(line))
                {
                    outboundQueue.Dequeue();
                    blnSuccess = true;
                }
            }
            return blnSuccess;
        }
        public string Peek()
        {
            return ((outboundQueue.Count > 0) ? outboundQueue.Peek().ToString() : "");
        }
        public int Count
        {
            get
            {
                int records = 0;
                if (File.Exists(FullFilePath))
                {
                    lock (FILELOCK)
                    {
                        using (StreamReader reader = new StreamReader(FullFilePath))
                        {
                            string line = "";
                            while ((line = reader.ReadLine()) != null)
                            {
                                records++;
                            }
                        }
                    }
                }
                return records;
            }

        }
    }
}