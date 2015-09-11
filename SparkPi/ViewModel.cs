using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkPi.VM
{
    public class ViewModel
    {
        /// <summary>
        /// Create all of the objects and values we will need for our View on ctor
        /// </summary>
        public ViewModel()
        {
            this.CycleLights = new CycleLights();
            this.TotalNumberOfCycles = 0;
            this.TicksSinceLastCycle = 0;
            this.NumberOfHeartBeatsSinceLastStateChange = 0;
            this.TotalRunTimeMilliSeconds = 0;
            List<string> errorsList = new List<string>();
            this.Errors = errorsList;
            this.Cluster = new TroubleshootingCluster();
            
        }

        public CycleLights CycleLights { get; set; }
        public TroubleshootingCluster Cluster { get; set; }
        public long TotalNumberOfCycles { get; set; }
        public long TicksSinceLastCycle { get; set; }
        public long NumberOfHeartBeatsSinceLastStateChange { get; set; }
        public List<string> Errors { get; set; }
        public long TotalRunTimeMilliSeconds { get; set; }

        public DateTime SystemTime
        {
            get { return DateTime.Now.ToLocalTime(); }
            set { }
        }
        public DateTime SystemStartupTime { get; set; }
        public DateTime TimeOfLastHeartBeat { get; set; }
        public double TotalMilliSecondsSinceLastCycle
        {
            get
            {
                TimeSpan ts = DateTime.Now.Date - this.TimeOfLastHeartBeat;
                return ts.Ticks / 1000.0;
            }
            set
            {
                TotalMilliSecondsSinceLastCycle = value;
            }
        }

        public double SecondsSinceLastCycle
        {
            get
            {
               return this.TotalMilliSecondsSinceLastCycle / 1000.0;
            }
            set
            {

            }

        }
        public int MyProperty { get; set; }

        public void BindBusinessLayerToViewModel()
        {
            
        }
    }
}
