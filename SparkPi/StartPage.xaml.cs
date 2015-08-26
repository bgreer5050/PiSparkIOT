// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace SparkPi
{
    public sealed partial class StartPage : Page
    {
        /// <summary>
        /// Main Classes ***********************************************************************************
        /// </summary>
        public Network network;
        public Configuration configuration;
        public Controller controller;
        //***************************************************************************************************


        /// <summary>
        /// Data Collection Variables **********************************************************************
        /// </summary>
        public static DateTime timeOfSystemStartup;
        private static DateTime timeOfLastSystemStateChange;
        private static DateTime timeOfLastHeartbeat;
        private static long numberOfHeartBeatsSinceLastStateChange;
        private static long totalNumberOfCycles;
        private static long totalRuntimeMilliseconds;
        public static SystemState currentSystemState;
        //****************************************************************************************************


        /// <summary>
        /// INPUT AND OUTPUT PIN DECLARATIONS **********************************************
        /// </summary>
        //private const int LED_PIN = 6;
        //private const int HEARTBEAT_PIN = 5;

//        private GpioPin ledPin;
        private GpioPin heartBeatPin;
        private GpioPin OutPutHeartBeatPinTesting;

        //***********************************************************************************



        /// <summary>
        /// Timers **********************************************************************
        /// </summary>
        private DispatcherTimer timer;
        private DispatcherTimer timerDateTime;
        private DispatcherTimer TimerUpdateUI;
        //************************************************************************************

        /// <summary>
        /// UI VARIABLES **********************************************************************
        /// </summary>
        private string Test = "";
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush yellowBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush greenBrush = new SolidColorBrush(Windows.UI.Colors.Green);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
         
        //**************************************************************************************

        /// <summary>
        /// Flag Variables *********************************************************************
        /// </summary>
        int intShowDateTimeFlag = 1;
        public static bool blnDateReceivedFromServer = false;

        //**************************************************************************************

        /// <summary>
        /// Testing Variables *******************************************************************
        /// </summary>
        int inputCounter = 0;
        int outPutCounter = 0;
        //***************************************************************************************





        Utilities.PiDateTime piDateTime; 

        System.Threading.SynchronizationContext _uiSyncContext;

        SparkQueue queue = new SparkQueue();

        public StartPage()
        {
            InitializeComponent();

            SDCardWriterReader SDReader = new SDCardWriterReader();
            SDReader.WriteToSDCardAsync();

            _uiSyncContext = System.Threading.SynchronizationContext.Current;
            DateTime dt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
            txtblockTime.Text = dt.TimeOfDay.ToString();

            //timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromMilliseconds(500);
            //timer.Tick += Timer_Tick;

            timerDateTime = new DispatcherTimer();
            timerDateTime.Interval = TimeSpan.FromMilliseconds(300);
            //timerDateTime.Tick += TimerDateTime_Tick;
            timerDateTime.Tick += TimerDateTime_Tick1;
            timerDateTime.Start();


            TimerUpdateUI = new DispatcherTimer();
            TimerUpdateUI.Interval = TimeSpan.FromMilliseconds(500);
            TimerUpdateUI.Tick += TimerUpdateUI_Tick;
            TimerUpdateUI.Start();


            setUpSystem();
            setUpBoardIO();



   //         if (pin != null)
   //         {
   //             timer.Start();
			//setUpSystem();
   //         }
            
            //txtblockTime.Text = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();
            txtblockTime.Text = DateTime.UtcNow.ToString() + " ---- " + DateTime.Now.ToUniversalTime() + "----" + DateTime.Now.ToLocalTime();


            controller = new Controller();
            configuration = new Configuration();
            network = new Network();


            //Utilities.SparkEmail.Send("TEST FROM PI");
        }

        private void TimerDateTime_Tick1(object sender, object e)
        {
            //Test = "BLANK";

            if(OutPutHeartBeatPinTesting.Read()==GpioPinValue.High)
            {
                OutPutHeartBeatPinTesting.Write(GpioPinValue.Low);
            }
            else
            {
                OutPutHeartBeatPinTesting.Write(GpioPinValue.High);
                outPutCounter += 1;
            }


            //if (intShowDateTimeFlag == 1)
            //{
            //    txtblockTime.Text = "Universal Time: " + piDateTime.DateTime.ToUniversalTime();
            //    intShowDateTimeFlag = 2;
            //}
           
            //else if (intShowDateTimeFlag == 2)
            //{
            //    txtblockTime.Text = "Time Of Day: " + piDateTime.DateTime.TimeOfDay.ToString();
            //    intShowDateTimeFlag = 3;
            //}

            //else if (intShowDateTimeFlag == 3)
            //{
            //    txtblockTime.Text = "Local Time: " + piDateTime.DateTime.ToLocalTime();
            //    intShowDateTimeFlag = 1;
            //}

            Debug.WriteLine(inputCounter.ToString());
            Debug.WriteLine(outPutCounter.ToString());

        }

        private void setUpSystem()
        {
            //timeOfSystemStartup = DateTime.UtcNow;
            //txtblockTime.Text = timeOfSystemStartup.time ToShortDateString();
            txtSystemStartTime.Text = DateTime.Now.TimeOfDay.ToString();
            currentSystemState = SystemState.DOWN;

        }

        private void setUpBoardIO()
        {

            var gpioController = GpioController.GetDefault();
            heartBeatPin = gpioController.OpenPin(5);
            heartBeatPin.SetDriveMode(GpioPinDriveMode.InputPullDown);

            heartBeatPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            heartBeatPin.ValueChanged += HeartBeatPin_ValueChanged;

            OutPutHeartBeatPinTesting = gpioController.OpenPin(6);
            OutPutHeartBeatPinTesting.SetDriveMode(GpioPinDriveMode.Output);


        }

        private void HeartBeatPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            Test = "HEART BEAT RECEIVED";
            if (sender.Read() == GpioPinValue.High)
            {
                inputCounter += 1;
                totalNumberOfCycles += 1;
            }

            Debug.WriteLine(sender.Read().ToString());
        }

        private void TimerUpdateUI_Tick(object sender, object e)
        {
            // txtblockTime.Text = Test;

            UpdateUI();

            if (blnDateReceivedFromServer)
            {
                PiTimeRed.Fill = grayBrush;
                PiTimeGreen.Fill = greenBrush;
            }
            else
            {
                PiTimeGreen.Fill = grayBrush;
                PiTimeRed.Fill = redBrush;
            }

        }

        private void UpdateUI()
        {
            CheckPiDateTime();
        }

        private void CheckPiDateTime()
        {
            if (blnDateReceivedFromServer == false)
            {

                try
                {
                    piDateTime = new Utilities.PiDateTime();
                }
                catch (Exception)
                {

                    // TODO Log and Notify
                }

                blnDateReceivedFromServer = false;
            }
            else
            {
                blnDateReceivedFromServer = true;
            }
        }

        private void TimerDateTime_Tick(object sender, EventArgs e)
        {
            if (intShowDateTimeFlag == 1)
            {
                txtblockTime.Text = "Universal Time: " + piDateTime.DateTime.ToUniversalTime();
                intShowDateTimeFlag = 2;
            }
           
            else if (intShowDateTimeFlag == 2)
            {
                txtblockTime.Text = "Time Of Day: " + piDateTime.DateTime.TimeOfDay.ToString();
                intShowDateTimeFlag = 3;
            }

            else if (intShowDateTimeFlag == 3)
            {
                txtblockTime.Text = "Local Time: " + piDateTime.DateTime.ToLocalTime();
                intShowDateTimeFlag = 1;
            }
           
        }

        //private void InitGPIO()
        //{
        //    var gpio = GpioController.GetDefault();

        //    // Show an error if there is no GPIO controller
        //    if (gpio == null)
        //    {
        //        pin = null;
        //        GpioStatus.Text = "There is no GPIO controller on this device.";
        //        return;
        //    }

        //    pin = gpio.OpenPin(LED_PIN);
        //    pinValue = GpioPinValue.High;
        //    pin.Write(pinValue);
        //    pin.SetDriveMode(GpioPinDriveMode.Output);


        //    GpioStatus.Text = "GPIO pin initialized correctly.";

        //}

        //private void Timer_Tick(object sender, object e)
        //{
        //    if (pinValue == GpioPinValue.High)
        //    {
        //        pinValue = GpioPinValue.Low;
        //        pin.Write(pinValue);
        //        redLED.Fill = redBrush;
        //        greenLED.Fill = grayBrush;
        //    }
        //    else
        //    {
        //        pinValue = GpioPinValue.High;
        //        pin.Write(pinValue);
        //        redLED.Fill = grayBrush;
        //        greenLED.Fill = greenBrush;
        //    }
        //}


        async Task<int> AccessTheWebAsync()
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

            Task<string> getStringTask = client.GetStringAsync(@"http://yahoo.com");
            string urlContents = await getStringTask;

            return urlContents.Length;

        }

        private async void  btnTest_Click(object sender, RoutedEventArgs e)
        {
           int result = await AccessTheWebAsync();
           
        }

        private void btnSetMachineStatusRUN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSetMachineStatusDOWN_Click(object sender, RoutedEventArgs e)
        {

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
