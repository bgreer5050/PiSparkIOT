using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkPi.Utilities
{
    public class PiDateTime
    {
        private DateTime _dateTime;
        private DateTime _universalDateTime;
        private DateTime _localTime;

        public DateTime DateTime
        {
            get { return DateTime.Now; }
            set { _dateTime = value; }
        }


        public DateTime UniversalDateTime
        {
            get {
                return DateTime.Now.ToUniversalTime();
            }
            set { _universalDateTime = value; }
        }


        public DateTime MyProperty
        {
            get {
                return DateTime.Now.ToLocalTime();
            }
            set { _localTime = value; }
        }




    }
}
