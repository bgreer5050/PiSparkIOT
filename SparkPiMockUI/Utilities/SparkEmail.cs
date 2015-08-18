using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SparkPiMockUI.Utilities
{
    public class SparkEmail
    {

        public static async void Send(string strBody)
        {


            var pairs = new List<KeyValuePair<string, string>>
                 {
                    new KeyValuePair<string, string>("", "abc")
                 };

            var content = new FormUrlEncodedContent(pairs);

            var client = new HttpClient { BaseAddress = new Uri("http://10.0.0.1") };
            //  var response = client.PostAsync("/Messaging/SendEmail", content).Result;
            Task<HttpResponseMessage> getStringTask = client.PostAsync("/Messaging/SendEmail", content);
            HttpResponseMessage message = await client.PostAsync("/Messaging/SendEmail", content);

        }
    }
}
