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

            InboundDataTimer = new Timer(new TimerCallback(ProcessInboundEventAsync), new Object(), 1250, 1250);
            OutboundDataTimer = new Timer(new TimerCallback(ProcessOutboundEventAsync), new Object(), 1250, 1250);
        }
      
        private async void ProcessInboundEventAsync(object o)
        {
            //Debug.Print("Check For Inbound");
            while (inboundQueue.Count > 0)
            {
                await _syncLock.WaitAsync();

                Debug.WriteLine("YES - Inbound Exists");
                var line = inboundQueue.Peek().ToString();
                if (await writeDataToFileAsync(line))
                {
                    inboundQueue.Dequeue();
                }
                _syncLock.Release();
            }
        }
        private async void ProcessOutboundEventAsync(object o)
        {
            Debug.WriteLine("Outbound Queue: " + outboundQueue.Count.ToString());

            if (outboundQueue.Count == 0)
            {
                //Debug.Print("There is nothing in Outbound Queue.  Check if there is anything on the SD Card");
                await _syncLock.WaitAsync();
                readDataFromFileAsync();
                _syncLock.Release();
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

        private SemaphoreSlim _syncLock = new SemaphoreSlim(1);
        private async Task<bool> writeDataToFileAsync(string line)
        {
             bool result = false;
           // var result = new TaskCompletionSource<bool>();
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StreamWriter writer;
            try
            {
                await _syncLock.WaitAsync();
                StorageFile dbFile = await folder.CreateFileAsync("SparkQueueDB.txt", CreationCollisionOption.OpenIfExists);


                writer = new StreamWriter(await dbFile.OpenStreamForWriteAsync());

                writer.WriteLine(line);
                writer.Flush();
                result = true;
            }
            catch(Exception ex)
            {
                
                result = false;
            }
            _syncLock.Release();
            return result;
        }
        private async void readDataFromFileAsync()
        {
            var line = "";
                    try
                    {
                    StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    StorageFile file = await folder.GetFileAsync("SparkQueueDB.txt");
                        using (StreamReader reader = new StreamReader(await file.OpenStreamForReadAsync()))
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
        private async Task<bool> removeDataFromFileAsync(string lineToRemove)
        {
            bool result = false;
            if (File.Exists(FullFilePath))
            {
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
            if (File.Exists(FullFilePath))
            {
                StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.GetFileAsync("SparkQueueDB.txt");
                StreamReader reader = new StreamReader(await file.OpenStreamForReadAsync());

                string line = "";
                while((line = reader.ReadLine()) != null  )
                {
                    records++;
                }

            }
            return records;
        }
    }
}