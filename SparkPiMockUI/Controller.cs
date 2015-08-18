using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkPiMockUI
{
    public class Controller
    {

        private string ipAddress;
        private string modelNumber;
        private string revision;
        
        public Controller()
        {
            this.modelNumber = "Pi 2";

        }


        public string IPAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; }
        }

        public string ModelNumber
        {
            get { return modelNumber; }
            set { modelNumber = value; }
        }

        public string Revision
        {
            get { return revision; }
            set { revision = value; }
        }




    }

    public class Configuration
    {
        private string _assetNumber;
        private float _gracePeriodMultiple;
        private int _cycleLengthMs;
        private int _heartbeatsRequiredToChangeState;
        private string _enabled;


        public Configuration()
        {
            this.AssetNumber = "9999";
            this._gracePeriodMultiple = 2.0f;
            this.CycleLengthMs = 10000;
            this.HeartbeatsRequiredToChangeState = 2;
            
        }

        public string AssetNumber
        {
            get { return _assetNumber; }
            set { _assetNumber = value; }
        }

        public string Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public int HeartbeatsRequiredToChangeState
        {
            get { return _heartbeatsRequiredToChangeState; }
            set { _heartbeatsRequiredToChangeState = value; }
        }

        public int CycleLengthMs
        {
            get { return _cycleLengthMs; }
            set { _cycleLengthMs = value; }
        }

        public float GracePeriodMultiple
        {
            get
            {
                return 2.0f;
            }
            set { _gracePeriodMultiple = value; }
        }

    }
}
