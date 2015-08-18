﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace SparkPi
{
    public sealed partial class StartPage : Page
    {
        public static DateTime timeOfSystemStartup;

        private const int LED_PIN = 5;
      
        private GpioPin pin;
        private GpioPinValue pinValue;

        private DispatcherTimer timer;
        private DispatcherTimer timerDateTime;
        int intShowDateTimeFlag = 1;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush yellowBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush greenBrush = new SolidColorBrush(Windows.UI.Colors.Green);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
		Utilities.PiDateTime piDateTime = new Utilities.PiDateTime();

        System.Threading.SynchronizationContext _uiSyncContext;


        public StartPage()
        {
            InitializeComponent();
            _uiSyncContext = System.Threading.SynchronizationContext.Current;
            DateTime dt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
            txtblockTime.Text = dt.TimeOfDay.ToString();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;

            timerDateTime = new DispatcherTimer();
            timerDateTime.Interval = TimeSpan.FromMilliseconds(3500);
            //timerDateTime.Tick += TimerDateTime_Tick;
            timerDateTime.Tick += TimerDateTime_Tick1;
            timerDateTime.Start();
            InitGPIO();
            if (pin != null)
            {
                timer.Start();
			setUpSystem();
            }
            
            //txtblockTime.Text = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();
            txtblockTime.Text = DateTime.UtcNow.ToString() + " ---- " + DateTime.Now.ToUniversalTime() + "----" + DateTime.Now.ToLocalTime();

            Utilities.SparkEmail.Send("TEST FROM PI");
        }

        private void TimerDateTime_Tick1(object sender, object e)
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

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                pin = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            pin = gpio.OpenPin(LED_PIN);
            pinValue = GpioPinValue.High;
            pin.Write(pinValue);
            pin.SetDriveMode(GpioPinDriveMode.Output);


            GpioStatus.Text = "GPIO pin initialized correctly.";

        }

        private void Timer_Tick(object sender, object e)
        {
            if (pinValue == GpioPinValue.High)
            {
                pinValue = GpioPinValue.Low;
                pin.Write(pinValue);
                redLED.Fill = redBrush;
                greenLED.Fill = grayBrush;
            }
            else
            {
                pinValue = GpioPinValue.High;
                pin.Write(pinValue);
                redLED.Fill = grayBrush;
                greenLED.Fill = greenBrush;
            }
        }


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
            txtBlockURLLength.Text = result.ToString();
        }

        private void btnSetMachineStatusRUN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSetMachineStatusDOWN_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
