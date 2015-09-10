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
        public ViewModel viewModel;
        public SparkQueue sparkQueue;
        //***************************************************************************************************



        /// <summary>
        /// Piface  ***********************************************************************************
        /// </summary>
        PiFaceSpiDriver spi_driver = new PiFaceSpiDriver();
        private DispatcherTimer timer;

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
            viewModel = new ViewModel();



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

            TimerUpdateUI = new DispatcherTimer();
            TimerUpdateUI.Interval = TimeSpan.FromMilliseconds(300);
            TimerUpdateUI.Tick += TimerUpdateUI_Tick;
            TimerUpdateUI.Start();

            
            /// Piface Stuff *************************************************
            Init();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            //timer.Start();

            spi_driver.PiFaceInterrupt += Spi_driver_PiFaceInterrupt;
            ///*******************************************************************



            setUpSystem();
            setUpBoardIO();

            controller = new Controller();
            configuration = new Configuration();
            network = new Network();
            //sparkQueue = new SparkQueue();
            sparkQueue.DataReadyForPickUp += SparkQueue_DataReadyForPickUp;


            //Utilities.SparkEmail.Send("TEST FROM PI");
        }



        /// <summary>
        /// Piface Method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Spi_driver_PiFaceInterrupt(object sender, PiFaceEventArgs e)
        {
            int state = spi_driver.ReadGPB();
            state &= ~e.PinMask;
            state |= e.PinState & e.PinMask;
            spi_driver.WriteGPA((byte)(~(state)));
        }

        /// <summary>
        /// Piface Method
        /// </summary>
        private async void Init()
        {
            await spi_driver.InitHardware();
        }
        /// <summary>
        /// Piface Method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, object e)
        {
            byte state = spi_driver.ReadGPB();
            Status.Text = state.ToString();
            state &= 0xAF;
            spi_driver.WriteGPA(state);
        }


        private async void SetUpMisc()
        {
            sparkQueue = new SparkQueue(this.viewModel);
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
                    return false;
                }

                Debug.WriteLine(aResponse.StatusCode);
                if (aResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //   sparkQueue.Dequeue();
                    return true;
                }

                Debug.WriteLine("Post Status Code: " + aResponse.StatusCode.ToString()); 
            }
            else
            {
                blnPostSuccessful = false;
            }
            return blnPostSuccessful;

        }

        //private async Task<bool> ProcessOutboundQueue()
        //{

        //   bool r = await Task.Run(() => {

        //        return true;
        //    });

        //    return r;


        //}

        private void TimerDateTime_Tick1(object sender, object e)
        {
            
            //BELOW IS WHEN WE ARE IN BENCH TESTING MODE ONLY.  IT SIMULATES HEARTBEATS.
            //if(OutPutHeartBeatPinTesting.Read()==GpioPinValue.High)
            //{
            //    OutPutHeartBeatPinTesting.Write(GpioPinValue.Low);
            //}
            //else
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

            systemStateMonitor = new System.Threading.Timer(stateMonitorCheck, configuration, 1000, 1000);
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
            heartBeatPin = gpioController.OpenPin(5);
            heartBeatPin.SetDriveMode(GpioPinDriveMode.InputPullDown);

            heartBeatPin.DebounceTimeout = TimeSpan.FromMilliseconds(500);
            heartBeatPin.ValueChanged += HeartBeatPin_ValueChanged;

            OutPutHeartBeatPinTesting = gpioController.OpenPin(6);
            OutPutHeartBeatPinTesting.SetDriveMode(GpioPinDriveMode.Output);
            OutPutHeartBeatPinTesting.Write(GpioPinValue.High);

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

            listViewErrors.Items.Clear();

            //We create a seperate list to hold our errors.  If our collection is modified
            //while we are iterating, we will get an error.

            if (viewModel.Errors != null && viewModel.Errors.Count > 0)
            {
                string[] strErrors = viewModel.Errors.ToArray();
                for (var _x = 0; _x < strErrors.Length; _x++)
                {
                    listViewErrors.Items.Add(strErrors[_x]);
                }
            }
           
            switch(viewModel.Cluster.LedInbound)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledInbound.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledInbound.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledInbound.Fill = yellowBrush;
                    break;

                case TroubleshootingCluster.Color.Green:
                    ledInbound.Fill = greenBrush;
                    break;

                default:
                    ledInbound.Fill = grayBrush;
                    break;
            }


            switch (viewModel.Cluster.LedOutbound)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledOutbound.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledOutbound.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledOutbound.Fill = yellowBrush;
                    break;

                default:
                    ledOutbound.Fill = grayBrush;
                    break;
            }


            switch (viewModel.Cluster.LedPosting)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledPosting.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledPosting.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledPosting.Fill = yellowBrush;
                    break;

                default:
                    ledPosting.Fill = grayBrush;
                    break;
            }

            switch (viewModel.Cluster.LedAux1)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledAux1.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledAux1.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledAux1.Fill = yellowBrush;
                    break;

                default:
                    ledAux1.Fill = grayBrush;
                    break;
            }

            switch (viewModel.Cluster.LedAux2)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledAux2.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledAux2.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledAux2.Fill = yellowBrush;
                    break;

                default:
                    ledAux2.Fill = grayBrush;
                    break;
            }


            switch (viewModel.Cluster.LedAux3)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledAux3.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledAux3.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledAux3.Fill = yellowBrush;
                    break;

                default:
                    ledAux3.Fill = grayBrush;
                    break;
            }

            switch (viewModel.Cluster.LedAux4)
            {
                case TroubleshootingCluster.Color.Gray:
                    ledAux4.Fill = grayBrush;
                    break;

                case TroubleshootingCluster.Color.Red:
                    ledAux4.Fill = redBrush;
                    break;

                case TroubleshootingCluster.Color.Yellow:
                    ledAux4.Fill = yellowBrush;
                    break;

                default:
                    ledAux4.Fill = grayBrush;
                    break;
            }


           



            listViewErrors.Background = redBrush;
            listViewErrors.Foreground = greenBrush;
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
