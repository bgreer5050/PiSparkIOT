using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SparkPi
{
    public class PowerOuttageHandler
    {
        public StorageFolder folder { get; set; }
        public StorageFile file { get; set; }

        public System.Threading.Timer timer;

        public PowerOuttageHandler()
        {
            timer = new System.Threading.Timer(RecordMachineStatus, null, 10000, 5000);

            this.folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            this.file = GetFile(folder).Result;
        }

        private void RecordMachineStatus(object state)
        {
            throw new NotImplementedException();
        }

        private async static Task<StorageFile> GetFile(StorageFolder folder)
        {
            return await folder.CreateFileAsync("PowerOuttageHandler.txt", CreationCollisionOption.FailIfExists);
        }


    }
}
