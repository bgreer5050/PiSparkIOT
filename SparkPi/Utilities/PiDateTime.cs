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
        string s = "";
        public PiDateTime()
        {
            GetDateTimeAsync();
            Debug.WriteLine(s);
        }

        public async Task GetPiDateTime()
        {
           s = await GetDateTimeAsync();
        }

        static async Task<string> GetDateTimeAsync()
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

           Task<string> getStringTask = client.GetStringAsync(@"http://10.0.200.152:3515/Home/GetDateTime");
           // Task<string> getStringTask = client.GetStringAsync(@"http://yahoo.com/");

            string urlContents = await getStringTask;

            return urlContents;
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
