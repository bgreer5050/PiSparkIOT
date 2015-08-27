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
        public ViewModel viewModel;
        public SparkQueue sparkQueue;
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
        private static System.Threading.Timer systemStateMonitor;
        //************************************************************************************

        /// <summary>
        /// UI VARIABLES **********************************************************************
        /// </summary>
        private string Test = "";
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush yellowBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);
        private SolidColorBrush greenBrush = new SolidColorBrush(Windows.UI.Colors.Green);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
        

        //**************************************************************************************


        /// <summary>
        /// SHARED VARIABLES FOR UI UPDATE **********************************************************************
        /// </summary>
        public CycleLights cycleLights;


        //**************************************************************************************


        /// <summary>
        /// Flag Variables *********************************************************************
        /// </summary>
        public static bool blnDateReceivedFromServer = false;

        //**************************************************************************************

        /// <summary>
        /// Testing Variables *******************************************************************
        /// </summary>
        int inputCounter = 0;
        int outPutCounter = 0;
        //***************************************************************************************

            // TODO How to use this ?
        System.Threading.SynchronizationContext _uiSyncContext;
        

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
            TimerUpdateUI.Interval = TimeSpan.FromMilliseconds(300);
            TimerUpdateUI.Tick += TimerUpdateUI_Tick;
            TimerUpdateUI.Start();


            setUpSystem();
            setUpBoardIO();


            controller = new Controller();
            configuration = new Configuration();
            network = new Network();
            sparkQueue = new SparkQueue();
            sparkQueue.DataReadyForPickUp += SparkQueue_DataReadyForPickUp;


            //Utilities.SparkEmail.Send("TEST FROM PI");
        }

        private void SparkQueue_DataReadyForPickUp(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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

                txtblockTime.Text = "Time Of Day: " + DateTime.Now.TimeOfDay.ToString();
               
          
            Debug.WriteLine(inputCounter.ToString());
            Debug.WriteLine(outPutCounter.ToString());
        }

        private void setUpSystem()
        {
            viewModel = new ViewModel();
            
            cycleLights = new CycleLights();
            //timeOfSystemStartup = DateTime.UtcNow;
            //txtblockTime.Text = timeOfSystemStartup.time ToShortDateString();
            txtSystemStartTime.Text = DateTime.Now.TimeOfDay.ToString();
            currentSystemState = SystemState.DOWN;

            systemStateMonitor = new System.Threading.Timer(stateMonitorCheck, configuration, 1000, 1000);
        }

        private void stateMonitorCheck(object state)
        {
            Debug.WriteLine("QUEUE SIZE:" + sparkQueue.Count.ToString());
            
            var currTime = DateTime.UtcNow;
            Debug.WriteLine(DateTime.UtcNow.ToString());

            //Debug.Print("Checking if Machine State = Running");
            //Debug.Print("System State: " + currentSystemState.ToString()); 

            if (currentSystemState == SystemState.RUNNING)
            {
               
                var timeSinceLastHeartbeat = getMillisecondsSinceLastHeartBeat(currTime);
                if ((configuration.CycleLengthMs * configuration.GracePeriodMultiple) < timeSinceLastHeartbeat)
                {
                    //Debug.Print("Change State to Down");
                    setSystemStateToDown(currTime,configuration,sparkQueue);
                }
            }
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
                cycleLights.GreenOn = true;
                cycleLights.YellowON = false;
                cycleLights.RedON = false;
                handleHeartBeat(DateTime.Now, controller, configuration,sparkQueue);

            }
            else
            {
                cycleLights.GreenOn = false;
                cycleLights.YellowON = true;
                cycleLights.RedON = false;
            }

            Debug.WriteLine(sender.Read().ToString());
        }

        private void TimerUpdateUI_Tick(object sender, object e)
        {
            // txtblockTime.Text = Test;

            UpdateUI();

            //if (blnDateReceivedFromServer)
            //{
            //    PiTimeRed.Fill = grayBrush;
            //    PiTimeGreen.Fill = greenBrush;
            //}
            //else
            //{
            //    PiTimeGreen.Fill = grayBrush;
            //    PiTimeRed.Fill = redBrush;
            //}

        }

        private void UpdateUI()
        {
            if (cycleLights.GreenOn) { greenLED.Fill = greenBrush; } else { greenLED.Fill = grayBrush; }

            if (cycleLights.YellowON) { yellowLED.Fill = yellowBrush; } else { yellowLED.Fill = grayBrush; }

            if (cycleLights.RedON) { redLED.Fill = redBrush; } else { redLED.Fill = grayBrush; }

            txtCycleCount.Text = totalNumberOfCycles.ToString();
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



        private static void handleHeartBeat(DateTime time,Controller controller,Configuration config,SparkQueue sparkQueue)     
        {

            totalRuntimeMilliseconds += getMillisecondsSinceLastHeartBeat(time);
            totalNumberOfCycles++;
            numberOfHeartBeatsSinceLastStateChange++;
            timeOfLastHeartbeat = time;

            TimeSpan ts = time - timeOfLastHeartbeat;

            double totalMillisecondsSinceLastCycle = ts.Ticks / 10000.0;

            if (currentSystemState == SystemState.DOWN &&
            numberOfHeartBeatsSinceLastStateChange >  config.HeartbeatsRequiredToChangeState &&
            totalMillisecondsSinceLastCycle < (config.CycleLengthMs * 1.5))
            {
                setSystemSateToRun(time,config,sparkQueue);
            }

        }

        private static void setSystemSateToRun(DateTime time, Configuration config, SparkQueue sparkQueue)
        {
            // TODO Conside doing something with an output here

            Debug.WriteLine("RUN RUN RUN RUN");

            currentSystemState = SystemState.RUNNING;
            timeOfLastSystemStateChange = time;
            numberOfHeartBeatsSinceLastStateChange = 0;  
            MachineEvent evt = new MachineEvent { AssetID =  config.AssetNumber, state = "RUNNING", ticks = DateTime.Now.Ticks.ToString() };
            sparkQueue.Enqueue(@"assetID=" + config.AssetNumber + "&state=" + evt.state + "&ticks=" + evt.ticks.ToString());

            //+ "&blnNetworkUp=" + network.NetworkUp.ToString() + "&blntimeupdated=" + Program.time.TimeUpdated.ToString());
        }


        private static void setSystemStateToDown(DateTime time, Configuration config,SparkQueue sparkQueue)
        {

            Debug.WriteLine("DOWN DOWN DOWN DOWN");
            
            // TODO Conside doing something with an output here

            currentSystemState = SystemState.DOWN;
            timeOfLastSystemStateChange = time;
            numberOfHeartBeatsSinceLastStateChange = 0;
           
            MachineEvent evt = new MachineEvent { AssetID = config.AssetNumber, state = "DOWN", ticks = DateTime.Now.Ticks.ToString() };
            //sparkQueue.Enqueue(@"assetID=" + config.AssetNumber + "&state=" + evt.state + "&ticks=" + evt.ticks.ToString() + "&blnNetworkUp=" + network.NetworkUp.ToString() + "&blntimeupdated=" + Program.time.TimeUpdated.ToString());

            sparkQueue.Enqueue(@"assetID=" + config.AssetNumber + "&state=" + evt.state + "&ticks=" + evt.ticks.ToString());

            //Thread threadRecordStateInMachineStateLog = new Thread(new ThreadStart(UpdateMachineStateOnSDCard));
            //threadRecordStateInMachineStateLog.Start();
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
