using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SparkPi.Utilities
{
    public  class PiDateTime
    {
        private  DateTime _dateTime;
        private  DateTime _universalDateTime;
        private  DateTime _localTime;
        private long _tickOffset; //Difference between startup time and real time
        

        public PiDateTime()
        {

            
            //GetDateTimeAsync().Wait();
        }

        static async Task GetDateTimeAsync()
        {
            try
            {
                WebRequest request = WebRequest.Create("http://10.0.200.152:3515/Home/GetDateTime");
                
                WebResponse response = await request.GetResponseAsync();

                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.
                Debug.WriteLine(responseFromServer);
                // Clean up the streams and the response.
                reader.Dispose();

            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
           


            //using (var client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri("http://10.0.200.152:3515/");
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //    client.Timeout = System.TimeSpan.FromMilliseconds(10000);

            //    // New code:
            //    HttpResponseMessage response = await client.GetAsync("Home/GetDateTime");
            //    if (response.IsSuccessStatusCode)
            //    {
            //        string strTime = await response.Content.ReadAsStringAsync();
            //        Debug.WriteLine("***************************");
            //        Debug.WriteLine(strTime);
            //    }
            //    else
            //    {
            //        Debug.WriteLine(response.StatusCode.ToString());
            //    }
            //}
        }

        public  DateTime DateTime
        {
            get {

                return DateTime.Now;

                }
            set { _dateTime = value; }
        }


        public  DateTime UniversalDateTime
        {
            get {
                return DateTime.Now.ToUniversalTime();
            }
            set { _universalDateTime = value; }
        }


        public  DateTime MyProperty
        {
            get {
                return DateTime.Now.ToLocalTime();
            }
            set { _localTime = value; }
        }




    }
}
