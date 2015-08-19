using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SparkPi
{
    public class SDCardWriterReader
    {
        public async void WriteToSDCardAsync()
        {
            

            StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;
            // StorageFolder folder = await externalDevices.CreateFolderAsync("TestX");
            try
            {
                StorageFolder sdCard = (await externalDevices.GetFoldersAsync()).FirstOrDefault();
            }
            catch(System.UnauthorizedAccessException ex)
            {
                Debug.Write(ex.Message);
                Debug.Write("Pause");
                throw;
            }
            //if (sdCard != null)
            //{
            //    Debug.WriteLine("SD CARD AVAILABLE");
            //}
            //else
            //{
            //    Debug.WriteLine("NO SD CARD AVAILABLE");

            //}
        }
    }
}
