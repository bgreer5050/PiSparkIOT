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

        public DateTime DateTime
        {
            get { return _dateTime; }
            set { _dateTime = value; }
        }

        private DateTime _universalDateTime;

        public DateTime UniversalDateTime
        {
            get {
                return DateTime.Now.ToUniversalTime();
            }
            set { _universalDateTime = value; }
        }

        private DateTime _localTime;

        public DateTime MyProperty
        {
            get {
                return DateTime.Now.ToLocalTime();
            }
            set { _localTime = value; }
        }




    }
}
