using Nito.AsyncEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SparkPi
{
    public class SparkQueue
    {
        public delegate void SparkQueueEventHandler(object sender, EventArgs e);
        public event SparkQueueEventHandler DataReadyForPickUp;
        public int QueueCycleMilliSeconds { get; set; }
        public string SubDirectoryPath { get; set; }
        public string DataFileName { get; set; }
      
        private Queue inboundQueue;
        private Queue outboundQueue;
        private Timer InboundDataTimer;
        private Timer OutboundDataTimer;
        private Object FILELOCK = new Object();
        public StorageFolder folder;
        public StorageFile file;
   
        public SparkQueue()
        {
            //SubDirectoryPath = subDirectory;
            //DataFileName = fileName;

            this.folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            this.file = folder.CreateFileAsync("SparkQueueDB.txt", CreationCollisionOption.OpenIfExists).AsTask().Result;
            //GetResults();
            //file = folder.GetFileAsync("SparkQueueDB.txt").GetResults();

            QueueCycleMilliSeconds = 500;

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

            InboundDataTimer = new Timer(new TimerCallback(ProcessInboundEvent), new Object(), 1250, 1250);
            OutboundDataTimer = new Timer(new TimerCallback(ProcessOutboundEvent), new Object(), 1250, 1250);
        }
      
        private void ProcessInboundEvent(object o)
        {
            Debug.WriteLine("Check For Inbound");
            while (inboundQueue.Count > 0)
            {
                Debug.WriteLine("YES - Inbound Exists");
                var line = inboundQueue.Peek().ToString();
                bool success = writeDataToFileAsync(line);
               
                if(success==true)
                {
                    inboundQueue.Dequeue();
                }
            }
        }
        private void ProcessOutboundEvent(object o)
        {

            Debug.WriteLine("Outbound Queue: " + outboundQueue.Count.ToString());

            if (outboundQueue.Count == 0)
            {
                //Debug.Print("There is nothing in Outbound Queue.  Check if there is anything on the SD Card");
                readDataFromFile();
            }
            else
            {
                if (DataReadyForPickUp != null)
                {
                    Debug.WriteLine("Firing DataReadyForPickUp");
                    DataReadyForPickUp(this, new EventArgs());
                }
            }
            _syncLock.Release();

        }

        private SemaphoreSlim _syncLock = new SemaphoreSlim(1);
        private bool writeDataToFileAsync(string line)
        {
             bool result = false;
           // var result = new TaskCompletionSource<bool>();
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StreamWriter writer;
            try
            {
                var createFileTask = folder.CreateFileAsync("SparkQueueDB.txt", CreationCollisionOption.OpenIfExists);
                StorageFile dbFile = createFileTask.GetResults();

                var taskGetStreamWriter = dbFile.OpenStreamForWriteAsync();
                
                writer = new StreamWriter(taskGetStreamWriter.Result);
                taskGetStreamWriter.Wait();
                writer.WriteLine(line);
                writer.Flush();
                result = true;
            }
            catch(Exception ex)
            {
                
                result = false;
            }

            return result;
        }
        private void readDataFromFile()
        {
            var line = "";
            try
            {
                StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;


                var task = folder.GetFileAsync("SparkQueueDB.txt");
                StorageFile file = task.GetResults();

                //Create Stream and Dispose

                var GetReader = file.OpenStreamForReadAsync(); // Create a task called GetReader
                StreamReader reader = new StreamReader(GetReader.Result); //The StreamReader ctor takes a stream in its constructor

                line = reader.ReadLine();
                reader.Dispose();
            }
            catch (System.IO.IOException ex)
            {

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
        private async Task<bool> removeDataFromFileAsync(string lineToRemove)
        {
            await _syncLock.WaitAsync();
            bool result = false;
           
                StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.GetFileAsync("SparkQueueDB.txt");
                var lines = new ArrayList();
                    using (var reader = new StreamReader(await file.OpenStreamForReadAsync()))
                    {
                        string line = "";
                        while ((line = reader.ReadLine()) != null)
                        {
                            lines.Add(line);
                        }
                    }
                    using (StreamWriter writer = new StreamWriter(await file.OpenStreamForWriteAsync()))
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
                                Debug.WriteLine("LINE BEING REMOVED");
                            }
                        }
                        result = true;
                    }
            _syncLock.Release();
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
                if (removeDataFromFileAsync(line).Result)
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
                    lock (FILELOCK)
                    {
                    return GetCountAsync().Result; 
                    }
            }
        }

        private async Task<int> GetCountAsync()
        {
            await Task.Delay(100);
            int records = 0;
            
                StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.GetFileAsync("SparkQueueDB.txt");
                StreamReader reader = new StreamReader(await file.OpenStreamForReadAsync());

                string line = "";
                while((line = reader.ReadLine()) != null  )
                {
                    records++;
                }

            return records;
        }
    }
}