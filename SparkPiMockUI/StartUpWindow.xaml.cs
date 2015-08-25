using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SparkPiMockUI
{
    /// <summary>
    /// Interaction logic for StartUpWindow.xaml
    /// </summary>
    public partial class StartUpWindow : Window
    {
        public static DateTime timeOfSystemStartup;
        private static DateTime timeOfLastSystemStateChange;
        private static DateTime timeOfLastHeartbeat;
        private static long numberOfHeartBeatsSinceLastStateChange;
        private static long totalNumberOfCycles;
        private static long totalRuntimeMilliseconds;
        public static SystemState currentSystemState;

        private DispatcherTimer timer;
        private DispatcherTimer timerDateTime;
        private SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        private SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);
        private SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
        private SolidColorBrush grayBrush = new SolidColorBrush(Colors.LightGray);
        Utilities.PiDateTime piDateTime = new Utilities.PiDateTime();

        System.Threading.SynchronizationContext _uiSyncContext;

        int intShowDateTimeFlag;

        public Network network;
        public Configuration configuration;
        public Controller controller;

        public StartUpWindow()
        {
             InitializeComponent();
            _uiSyncContext = System.Threading.SynchronizationContext.Current;


            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;

            timerDateTime = new DispatcherTimer();
            timerDateTime.Interval = TimeSpan.FromMilliseconds(3000);
            timerDateTime.Tick += TimerDateTime_Tick;
            timerDateTime.Start();

            intShowDateTimeFlag = 1;
            //InitGPIO();
            //if (pin != null)
            //{
                timer.Start();
            //}

            //txtblockTime.Text = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();

             controller = new Controller();
             configuration = new Configuration();
             network = new Network();

             setUpSystem();

        }

        private void setUpSystem()
        {
            //timeOfSystemStartup = DateTime.UtcNow;
            //txtblockTime.Text = timeOfSystemStartup.time ToShortDateString();
             txtSystemStartTime.Text = DateTime.Now.TimeOfDay.ToString();
        }

        private void TimerDateTime_Tick(object sender, EventArgs e)
        {
            if (intShowDateTimeFlag == 1)
            {
                txtblockTime.Text = "Universal Time: " + piDateTime.DateTime.ToUniversalTime();
                intShowDateTimeFlag = 2;
            }
            else if(intShowDateTimeFlag == 2)
            {
                txtblockTime.Text = "Long Date String: " + piDateTime.DateTime.ToLongDateString();
                intShowDateTimeFlag = 3;
            }
            else if (intShowDateTimeFlag == 3)
            {
                txtblockTime.Text = "Time Of Day: " + piDateTime.DateTime.TimeOfDay.ToString();
                intShowDateTimeFlag = 4;
            }
           
            else if (intShowDateTimeFlag == 4)
            {
                txtblockTime.Text = "Local Time: " + piDateTime.DateTime.ToLocalTime();
                intShowDateTimeFlag = 5;
            }
            else if (intShowDateTimeFlag == 5)
            {
                txtblockTime.Text = "Long Date String: " + piDateTime.DateTime.ToLongDateString();
                intShowDateTimeFlag = 1;
            }
        }

        private void Timer_Tick(object sender, object e)
        {
                redLED.Fill = redBrush;
                greenLED.Fill = grayBrush;
        }
        private void btnSetMachineStatusRUN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSetMachineStatusDOWN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCycleSimulate_Click(object sender, RoutedEventArgs e)
        {

        }

        async Task<int> AccessTheWebAsync()
        {
            //System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

            //Task<string> getStringTask = client.GetStringAsync(@"http://yahoo.com");
            //string urlContents = await getStringTask;

            //return urlContents.Length;
            return 1;

        }

        private async void btnTest_Click(object sender, RoutedEventArgs e)
        {
            int result = await AccessTheWebAsync();
           // txtBlockURLLength.Text = result.ToString();
        }

        private static long getMillisecondsSinceLastStateChange(DateTime time)
        {
            TimeSpan ts = time - timeOfLastSystemStateChange;
            return ts.Ticks / TimeSpan.TicksPerMillisecond;
        }
        private static long getMillisecondsSinceLastHeartBeat(DateTime time)
        {
           
            TimeSpan ts = time - timeOfLastHeartbeat;
            return ts.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public enum SystemState
        {
            DOWN = 0,
            RUNNING
        };
        public struct MachineEvent
        {
            public string AssetID { get; set; }
            public string state { get; set; }
            public string ticks { get; set; }
        }
    }

}
