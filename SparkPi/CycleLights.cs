using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkPi
{
    public class CycleLights
    {
        public CycleLights()
        {
            this._greenON = false;
            this._yellowON = false;
            this._redON = true;
        }

        private bool _greenON;

        public bool GreenOn
        {
            get { return _greenON; }
            set { _greenON = value; }
        }

        private bool _yellowON;

        public bool YellowON
        {
            get { return _yellowON; }
            set { _yellowON = value; }
        }


        private bool _redON;

        public bool RedON
        {
            get { return _redON; }
            set { _redON = value; }
        }



    }
}
