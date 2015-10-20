using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;
using System.Diagnostics;

namespace SparkPi
{
    public class PowerOuttageHandler
    {

        private StorageFolder storageFolder;
        private StorageFile storageFile;

        public StorageFolder folder {

            get {
                if(this.storageFolder != null)
                {
                   
                }
                else
                {
                    this.storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                }
                return this.storageFolder;
               
            }
            set {
                this.storageFolder = value;

            }

        }

        public StorageFile file
        {
            get
            {
                if(this.storageFile!=null)
                {
                  
                }
                else
                {
                    this.storageFile = folder.CreateFileAsync("PowerOuttageHandler.txt",CreationCollisionOption.ReplaceExisting).AsTask().Result;
                }
                return this.storageFile;
            }
            set
            {
                this.file = value;
            }
        }


        public System.Threading.Timer timer;

        public PowerOuttageHandler(Configuration configuration)
        {
            timer = new System.Threading.Timer(RecordMachineStatus, configuration, 10000, 7000);
            //InitializeClass();
           
        }

       

        private async void RecordMachineStatus(object state)
        {
            try
            {
                Configuration config = (Configuration)state;
                string strMachineState = @"assetID=" + config.AssetNumber + "&state=" + StartPage.currentSystemState.ToString() + "&ticks=" + DateTime.Now.Ticks;
                byte[] writeBytes = System.Text.Encoding.UTF8.GetBytes(strMachineState);

                using (var stream = await this.file.OpenStreamForWriteAsync())
                {
                    stream.Write(writeBytes, 0, writeBytes.Length);
                    Debug.WriteLine("POWER OUTTAGE STATE WRITTEN");
                }
                
            }
            catch(Exception ex)
            {
                Debug.WriteLine("POWER OUTTAGE ERROR:");
                Debug.WriteLine(ex.Message);
            }

                
        }

        private async static Task<StorageFile> GetFile(StorageFolder folder)
        {
            return await folder.CreateFileAsync("PowerOuttageHandler.txt", CreationCollisionOption.ReplaceExisting);
        }

        internal async static Task<string> CheckLog()
        {
            
           StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
           string text;

            var _file  = await folder.TryGetItemAsync("PowerOuttageHandler.txt");
            if (_file != null) // Check if the file exists.  If this is the first time the controller is booting, the file will not exist.
            {
                StorageFile file = (StorageFile)_file;
                Stream stream = await file.OpenStreamForReadAsync();

                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }

                if (text.ToLower().Contains("running") == true)
                {
                    text = text.Replace("RUNNING", "DOWN");
                }
                else
                {
                    text = "";
                }
            }
            else
            {
                text = "";
            }
            return text;
        }
    }
}
