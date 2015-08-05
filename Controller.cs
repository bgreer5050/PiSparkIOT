using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SparkPi
{
    public class Controller
    {

        public Controller()
        {

        }

        private IPAddress  ipAddress;

        public IPAddress  IPAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; }
        }

      
    }
}
