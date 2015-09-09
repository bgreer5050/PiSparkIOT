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


    public  class TroubleshootingCluster
    {
         Random rand = new Random();

        private  TroubleshootingCluster.Color ledInbound;
        public  TroubleshootingCluster.Color LedInbound
        {
            get {
                int x = rand.Next(0, 10);
                switch(x)
                {
                    case 0:
                        return Color.Red;
                        break;

                    case 1:
                        return Color.Yellow;
                        break;

                    case 2:
                        return Color.Green;
                        break;

                    case 3:
                        return Color.Green;
                        break;


                    case 4:
                        return Color.Green;
                        break;

                    case 5:
                        return Color.Green;
                        break;

                    case 6:
                        return Color.Green;
                        break;

                    case 7:
                        return Color.Green;
                        break;

                    case 8:
                        return Color.Green;
                        break;

                    case 9:
                        return Color.Green;
                        break;

                    case 10:
                        return Color.Green;
                        break;

                    default:
                        return Color.Gray;
                }

                }
            set { ledInbound = value; }
        }

        private  TroubleshootingCluster.Color ledOutbound;
        public  TroubleshootingCluster.Color LedOutbound
        {
            get
            {
                int x = rand.Next(0, 3);
                switch (x)
                {
                    case 0:
                        return Color.Red;
                        break;

                    case 1:
                        return Color.Yellow;
                        break;

                    case 2:
                        return Color.Green;
                        break;

                    case 3:
                        return Color.Green;
                        break;

                    default:
                        return Color.Gray;
                }

            }
            set { ledOutbound = value; }
        }

        private  TroubleshootingCluster.Color ledPosting;
        public  TroubleshootingCluster.Color LedPosting
        {
            get
            {
                int x = rand.Next(0, 3);
                switch (x)
                {
                    case 0:
                        return Color.Red;
                        break;

                    case 1:
                        return Color.Yellow;
                        break;

                    case 2:
                        return Color.Green;
                        break;

                    case 3:
                        return Color.Green;
                        break;

                    default:
                        return Color.Gray;
                }

            }
            set { ledPosting = value; }
        }

        private  TroubleshootingCluster.Color ledAux1;
        public  TroubleshootingCluster.Color LedAux1
        {
            get
            {
                int x = rand.Next(0, 3);
                switch (x)
                {
                    case 0:
                        return Color.Red;
                        break;

                    case 1:
                        return Color.Yellow;
                        break;

                    case 2:
                        return Color.Green;
                        break;

                    case 3:
                        return Color.Green;
                        break;

                    default:
                        return Color.Gray;
                }

            }
            set { ledAux1 = value; }
        }

        private  TroubleshootingCluster.Color ledAux2;
        public  TroubleshootingCluster.Color LedAux2
        {
            get
            {
                int x = rand.Next(0, 3);
                switch (x)
                {
                    case 0:
                        return Color.Red;
                        break;

                    case 1:
                        return Color.Yellow;
                        break;

                    case 2:
                        return Color.Green;
                        break;

                    case 3:
                        return Color.Green;
                        break;

                    default:
                        return Color.Gray;
                }

            }
            set { ledAux2 = value; }
        }

        private  TroubleshootingCluster.Color ledAux3;
        public  TroubleshootingCluster.Color LedAux3
        {
            get
            {
                int x = rand.Next(0, 3);
                switch (x)
                {
                    case 0:
                        return Color.Red;
                        break;

                    case 1:
                        return Color.Yellow;
                        break;

                    case 2:
                        return Color.Green;
                        break;

                    case 3:
                        return Color.Green;
                        break;

                    default:
                        return Color.Gray;
                }

            }
            set { ledAux3 = value; }
        }

        private  TroubleshootingCluster.Color ledAux4;
        public  TroubleshootingCluster.Color LedAux4
        {
            get
            {
                int x = rand.Next(0, 3);
                switch (x)
                {
                    case 0:
                        return Color.Red;
                        break;

                    case 1:
                        return Color.Yellow;
                        break;

                    case 2:
                        return Color.Green;
                        break;

                    case 3:
                        return Color.Green;
                        break;

                    default:
                        return Color.Gray;
                }

            }
            set { ledAux4 = value; }
        }

        public enum Color
        {
            Red = 0,
            Yellow = 1,
            Green = 2,
            Gray = 3
        };
    }
}
