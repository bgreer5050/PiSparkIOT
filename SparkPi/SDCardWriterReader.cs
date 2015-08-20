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
        StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;

        public async void WriteToSDCardAsync()
        {

            StorageFile sampleFile = await folder.CreateFileAsync("sample.txt", CreationCollisionOption.OpenIfExists);

            Debug.WriteLine(sampleFile.Path);

            Debug.WriteLine("PAUSE");

        }


        public async void WriteToCard(string strFileName,IEnumerable<string> listLinesToWrite)
        {
            IStorageItem item = await folder.GetItemAsync(strFileName);
            StorageFile file = (StorageFile)item;



            await Windows.Storage.FileIO.WriteLinesAsync(file, listLinesToWrite);
            
        }
    }
}
