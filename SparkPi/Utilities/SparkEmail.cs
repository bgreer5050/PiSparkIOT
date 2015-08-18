using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SparkPi.Utilities
{
    public class SparkEmail
    {

        public static async void Send(string strBody)
        {

            try
            {
                string strEmailMessage = "to=bgreer@metal-matic.com&from=apollo@metal-matic.com&subject=Spark Issue&body=" + strBody;

                //create a request
                var request = (HttpWebRequest)WebRequest.Create("http://10.0.100.16/Messaging/SendEmail");

                byte[] buffer = Encoding.UTF8.GetBytes(strEmailMessage);


                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";


                Task<Stream> postTask = request.GetRequestStreamAsync();

                //We could do other stuff here since we have not called await on our task object yet.

                Stream post = await postTask;
                post.Write(buffer, 0, buffer.Length);
                post.Flush();

                Task<WebResponse> responseTask = request.GetResponseAsync();
                WebResponse webResponse = await responseTask;

                HttpWebResponse response = (HttpWebResponse)webResponse;
                Stream responseStream = response.GetResponseStream();
                Debug.Write(response.StatusCode.ToString());

                StreamReader reader = new StreamReader(responseStream);
                var responseValue = reader.ReadToEnd();


            }
            catch (Exception Ex)
            {

                Debug.Write(Ex.Message.ToString());

            }
        }
    }
}
