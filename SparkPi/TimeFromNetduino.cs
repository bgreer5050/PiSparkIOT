using System;
using Microsoft.SPOT;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace ApolloSpark
{
    public class Time
    {
        private long tickOffset; // Keeps track of how many ticks the Spark is off from the actual time.
        private bool timeIsUpdated;
        private Network network;
        private bool blnFailNotificationSent;


        public Time()
        {
            blnFailNotificationSent = false;
            this.network = Program.network;
            this.timeIsUpdated = false;
            this.thread = new Thread(MonitorTime);
            thread.Start();
        }

        private void MonitorTime()
        {

            while (this.TimeUpdated == false)
            {
                try
                {
                    //if (network.NetworkUp == true)
                    //{
                    UpdateNetduinoDateTime();
                    //}
                }
                catch (Exception ex)
                {
                    Debug.Print("FAILED Time Update - L48");
                    Debug.Print(ex.Message);
                    Thread.Sleep(30000);

                }

            }
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


        public void UpdateNetduinoDateTime()
        {
            try
            {


                DateTime time1 = DateTime.Now;

                string server = "time.nist.gov";
                //string server = "10.0.200.5";

                var timeZoneOffset = -6;

                var currentTime = GetNtpTime(server, timeZoneOffset);
                Debug.Print(currentTime.ToLocalTime().ToString());

                Microsoft.SPOT.Hardware.Utility.SetLocalTime(currentTime.ToLocalTime());

                TimeSpan ts = DateTime.Now - time1;
                tickOffset = ts.Ticks;
                TimeUpdated = true;
                Debug.Print(DateTime.Now.ToLocalTime().ToString());
                Debug.Print(DateTime.Now.ToUniversalTime().ToString());

            }
            catch
            {

            }
            //Debug.GC(true);
        }

        //-----------------------------------------------------------------------------------------------------
        private DateTime GetNtpTime(String TimeServer, int UTC_Offset)
        {
            EndPoint ep = new IPEndPoint(Dns.GetHostEntry(TimeServer).AddressList[0], 13);
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            byte[] msg = new Byte[50];
            s.Connect(ep);
            s.Send(new byte[1], 0, 0);
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

        private Thread thread;

        public Thread Thread
        {
            get
            {
                return thread;
            }
            set
            {
                thread = value;
            }
        }

    }
}


