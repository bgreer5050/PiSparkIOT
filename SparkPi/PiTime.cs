using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SparkPi
{
    public static class PiTime
    {

        public void UpdateNetduinoDateTime()
        {
            try
            {
                Windows.System.SystemManagementContract db = new Windows.System.SystemManagementContract();
                DateTime.

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


      

    }
}
