using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SparkPi
{
    public class TimeManager
    {

        private long tickOffset; // Keeps track of how many ticks the Spark is off from the actual time.
        private bool timeIsUpdated;




        public TimeManager()
        {

            DateTime time1 = DateTime.Now;

            string server = "time.nist.gov";
            //string server = "10.0.200.5";

            var timeZoneOffset = -6;
            var currentTime = GetNtpTime(server, timeZoneOffset);
            Debug.WriteLine(currentTime.ToLocalTime().ToString());

            TimeSpan ts = DateTime.Now - time1;
            tickOffset = ts.Ticks;
            TimeUpdated = true;
            Debug.WriteLine(DateTime.Now.ToLocalTime().ToString());
            Debug.WriteLine(DateTime.Now.ToUniversalTime().ToString());
        }



        public bool TimeUpdated
        {
            get { return timeIsUpdated; }
            set { timeIsUpdated = value; }
        }

        public long TickOffset
        {
            get { return tickOffset; }
            set { tickOffset = value; }
        }





        static TimeSpan _offset = new TimeSpan(0, 0, 0);
        public static TimeSpan CurrentOffset //Doesn't have to be public, it is for me because I'm presenting it on the UI for my information
        {
            get { return _offset; }
            private set { _offset = value; }
        }

        public static DateTime Now
        {
            get
            {
                return DateTime.Now - CurrentOffset;
            }
        }

        static void UpdateOffset(DateTime currentCorrectTime) //May need to be public if you're getting the correct time outside of this class
        {
            CurrentOffset = DateTime.UtcNow - currentCorrectTime;
            //Note that I'm getting network time which is in UTC, if you're getting local time use DateTime.Now instead of DateTime.UtcNow. 
        }

        private DateTime GetNtpTime(String TimeServer, int UTC_Offset)
        {
            EndPoint ep = new IPEndPoint(Dns.GetHostEntry(TimeServer).AddressList[0], 13);
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            byte[] msg = new Byte[50];
            s.Bind(ep);  //Connect(ep);
            s. Send(new byte[1], 0, 0);
            s.Receive(msg);
            String S = new String(System.Text.Encoding.UTF8.GetChars(msg));
            DateTime UTC_Time = new DateTime(Convert.ToInt32("20" + S.Substring(7, 2)), Convert.ToInt32(S.Substring(10, 2)), Convert.ToInt32(S.Substring(13, 2)), Convert.ToInt32(S.Substring(16, 2)), Convert.ToInt32(S.Substring(19, 2)), Convert.ToInt32(S.Substring(22, 2)));
            int ST_DST = Convert.ToInt32(S.Substring(25, 2));
            TimeSpan offsetAmount;
            if (ST_DST == 0 || ST_DST > 50) //Winter Standard Time 
            {
                offsetAmount = new TimeSpan(0, UTC_Offset, 0, 0, 0);
            }
            else //Summer Daylight Saving Time 
            {
                offsetAmount = new TimeSpan(0, UTC_Offset + 1, 0, 0, 0);
            }
            DateTime LocalDateTime = (UTC_Time + offsetAmount);

            return LocalDateTime;
        }
    }
}
