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
        public StorageFolder folder { get; set; }

        public StorageFile file { get; set; }



        public System.Threading.Timer timer;

        public PowerOuttageHandler(Configuration configuration)
        {
            timer = new System.Threading.Timer(RecordMachineStatus, configuration, 10000, 7000);
            InitializeClass();
           
        }

        private void InitializeClass()
        {
            this.folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            this.file = GetFile(folder).Result;
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
           StorageFile file = await folder.GetFileAsync("PowerOuttageHandler.txt");

            Stream stream = await file.OpenStreamForReadAsync();
            string text;

            using (StreamReader reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            if(text.ToLower().Contains("running")==true)
            {
                text = text.Replace("RUNNING", "DOWN");
            }
            else
            {
                text = "";
            }
            return text;
        }
    }
}
