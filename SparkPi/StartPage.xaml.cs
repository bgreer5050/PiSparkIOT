// Copyright (c) Microsoft. All rights reserved.

using Nito.AsyncEx;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI;
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
        public VM.ViewModel viewModel;

        public SparkQueue sparkQueue;
        public PowerOuttageHandler powerHandler;
        //***************************************************************************************************


        /// <summary>
        /// Data Collection Variables **********************************************************************
        /// </summary>
        public static DateTime timeOfSystemStartup;
        public static DateTime timeOfLastSystemStateChange;
        public static DateTime timeOfLastHeartbeat;
        public static long numberOfHeartBeatsSinceLastStateChange;
        public static long totalNumberOfCycles;
        public static long totalRuntimeMilliseconds;
        public static SystemState currentSystemState;
        //****************************************************************************************************


        /// <summary>
        /// INPUT AND OUTPUT PIN DECLARATIONS **********************************************
        /// </summary>
        //private const int LED_PIN = 6;
        //private const int HEARTBEAT_PIN = 5;

        private GpioPin heartBeatPin;


        //***********************************************************************************



        /// <summary>
        /// Timers **********************************************************************
        /// </summary>
        private DispatcherTimer timer;
        private DispatcherTimer timerDateTime;
       
        private static System.Threading.Timer systemStateMonitor;
        private static System.Threading.Timer cycleCountUpdate;
        private static System.Threading.Timer timerTestController;




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
            SetUpMisc();
            //while(true)
            //{

            //}


            _uiSyncContext = System.Threading.SynchronizationContext.Current;
            DateTime dt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
            txtblockTime.Text = dt.Hour.ToString() + ":" + dt.Minute.ToString(); // ToString("HH:mm:ss");
            //timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromMilliseconds(500);
            //timer.Tick += Timer_Tick;

            timerDateTime = new DispatcherTimer();
            timerDateTime.Interval = TimeSpan.FromMilliseconds(300);
            //timerDateTime.Tick += TimerDateTime_Tick;
            timerDateTime.Tick += TimerDateTime_Tick1;
            timerDateTime.Start();

          

            setUpSystem();
            setUpBoardIO();
            //powerHandler = new PowerOuttageHandler();

            controller = new Controller();
            configuration = new Configuration();
            network = new Network();

            viewModel = new VM.ViewModel(controller, configuration, cycleLights, network, sparkQueue, this);

            //sparkQueue = new SparkQueue();
            sparkQueue.DataReadyForPickUp += SparkQueue_DataReadyForPickUp;

            Utilities.SparkEmail.Send(this.configuration.AssetNumber + " Spark Controller Starting");
        }

        private async void SetUpMisc()
        {
            sparkQueue = new SparkQueue();
            sparkQueue.itializeClass().Wait();
        }



        private System.Threading.SemaphoreSlim _semaphore = new System.Threading.SemaphoreSlim(1);
        private async void SparkQueue_DataReadyForPickUp(object sender, EventArgs e)
        {
            _semaphore.Wait();
            bool postSucceed = await ProcessOutboundQueue();
            Debug.WriteLine("DATA READY FOR PICKUP NOT IMPLEMENTED");
            if(postSucceed)
            {
               await sparkQueue.Dequeue();
            }
            _semaphore.Release();
        }

        private async Task<bool> ProcessOutboundQueue()
        {
            string postData = "";
            bool blnPostSuccessful = false;
            // TODO Decide how many times I should try to post until we cancel ???
            StringBuilder sb = new StringBuilder();
            postData = sparkQueue.Peek();
            Debug.WriteLine("Post Data: " + postData);

            if (postData != null && postData.Length > 10)
            {
                HttpClient client = new HttpClient();
                //var body = String.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=notify.windows.com", MyCredentials, MyCredentials2);
                var body = String.Format(postData);
                StringContent theContent = new StringContent(body, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage aResponse;
                try
                {
                    aResponse = await client.PostAsync(new Uri(@"http://sparkhub.metal-matic.com/api/MachineRunState/Record"), theContent);
                }
                catch (HttpRequestException ex)
                {
                    // TODO Update View Model Errors

                    Debug.WriteLine(ex.Message);
                    viewModel.Errors.Add("L197" + ex.Message);
                    Utilities.SparkEmail.Send(configuration.AssetNumber + " -L197- " + ex.Message);
                    return false;
                }

                Debug.WriteLine(aResponse.StatusCode);
                if (aResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //   sparkQueue.Dequeue();
                    return true;
                }
                else
                {
                    Utilities.SparkEmail.Send(configuration.AssetNumber + " HTTP Error " + aResponse.StatusCode.ToString());
                }

                Debug.WriteLine("Post Status Code: " + aResponse.StatusCode.ToString());
            }
            else
            {
                blnPostSuccessful = false;
            }
            return blnPostSuccessful;

        }

        // For Testing Only
        //private async Task<bool> ProcessOutboundQueue()
        //{

        //    bool r = await Task.Run(() =>
        //    {

        //        return true;
        //    });

        //    return r;


        //}

        private void TimerDateTime_Tick1(object sender, object e)
        {
            // BELOW IS WHEN WE ARE IN BENCH TESTING MODE ONLY.  IT SIMULATES HEARTBEATS.
            //if (OutPutHeartBeatPinTesting.Read() == GpioPinValue.High)
            //{
            //    OutPutHeartBeatPinTesting.Write(GpioPinValue.Low);
            //}
            //else if (DateTime.Now.Second > 30)
            //{
            //    OutPutHeartBeatPinTesting.Write(GpioPinValue.High);
            //    outPutCounter += 1;
            //}

            txtblockTime.Text = "Time Of Day: " + DateTime.Now.TimeOfDay.ToString();
          
            Debug.WriteLine(inputCounter.ToString());
            Debug.WriteLine(outPutCounter.ToString());
        }

        private void setUpSystem()
        {
            cycleLights = new CycleLights();
            //timeOfSystemStartup = DateTime.UtcNow;
            //txtblockTime.Text = timeOfSystemStartup.time ToShortDateString();
            txtSystemStartTime.Text = DateTime.Now.TimeOfDay.ToString();
            currentSystemState = SystemState.DOWN;

            systemStateMonitor = new System.Threading.Timer(stateMonitorCheck, configuration, 500, 500);
            cycleCountUpdate = new System.Threading.Timer(updateCycleCountEmail, configuration, 60000, 3600000);
            timerTestController = new System.Threading.Timer(TestController, configuration, 5000, 5000);
        }

        private void TestController(object state)
        {
           
        }

        private void updateCycleCountEmail(object state)
        {
            TimeSpan ts = timeOfSystemStartup - DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.Append("Cycle Count Update:");
            sb.Append(totalNumberOfCycles.ToString());
            sb.Append("System Started: " + ts.TotalMinutes.ToString() + " Minutes Ago.");
            Utilities.SparkEmail.Send(sb.ToString());
        }

        private void stateMonitorCheck(object state)
        {
            //Debug.WriteLine("QUEUE SIZE:" + sparkQueue.Count.ToString());
            
            var currTime = DateTime.Now;
            Debug.WriteLine(DateTime.Now.ToString());

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
            //The OutPutHeartBeatPin will be set to high.  When the isolation relay
            //on the machine closes our OutPutHearBeatPin will send 5v to the hearBeatPin


            var gpioController = GpioController.GetDefault();
            heartBeatPin = gpioController.OpenPin(12);
            heartBeatPin.SetDriveMode(GpioPinDriveMode.InputPullDown);

            heartBeatPin.DebounceTimeout = TimeSpan.FromMilliseconds(5);
            heartBeatPin.ValueChanged += HeartBeatPin_ValueChanged;

        }

        private void InputPinTesting_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            Debug.WriteLine("EDGE TESTING PIN: " + args.Edge.ToString());

        }

        private void HeartBeatPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            //Test = "HEART BEAT RECEIVED";
            TimeSpan ts = timeOfLastHeartbeat - DateTime.Now;
            Debug.WriteLine(ts.TotalMilliseconds.ToString());

            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                inputCounter += 1;
                handleHeartBeat(DateTime.Now, controller, configuration, sparkQueue);
            }
            Debug.WriteLine(args.Edge.ToString());
          

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

            //if (currentSystemState == SystemState.DOWN &&
            //numberOfHeartBeatsSinceLastStateChange >  config.HeartbeatsRequiredToChangeState &&
            //totalMillisecondsSinceLastCycle < (config.CycleLengthMs * 1.5))

                if (currentSystemState == SystemState.DOWN)
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
