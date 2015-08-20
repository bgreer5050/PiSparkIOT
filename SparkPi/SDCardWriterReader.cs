using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

        private ConcurrentDictionary<string, SemaphoreSlim> fileLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        public async Task WriteToCardAsync(string strFileName, IEnumerable<string> listLinesToWrite)
        {
            var semaphoreSlim = fileLocks.GetOrAdd(strFileName, new SemaphoreSlim(1, 1));

            await semaphoreSlim.WaitAsync();
            try
            {
                IStorageItem item = await folder.GetItemAsync(strFileName);
                StorageFile file = (StorageFile)item;

                await Windows.Storage.FileIO.WriteLinesAsync(file, listLinesToWrite);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
