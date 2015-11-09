// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Windows.System.Threading;
using System.Diagnostics;

namespace BlinkyHeadlessCS
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;
        private string SCRAP_REPORTING_ENDPOINT = "http://dev.sparkhub.metal-matic.com/api/Scrap/Record";
        private string ASSET_ID = "483";
     
        public void Run(IBackgroundTaskInstance taskInstance)
        {

            deferral = taskInstance.GetDeferral();
            InitGpio();

        }

        /// <summary>
        /// Maps out the GPIO Pin Numbers to a panel button number.
        /// </summary>
        private readonly List<ButtonWiringConfig> buttonPinConfig = new List<ButtonWiringConfig> {
            new ButtonWiringConfig { GpioPinNumber = 4, ButtonNumber = 1 },
            new ButtonWiringConfig { GpioPinNumber = 5, ButtonNumber = 2 },
            new ButtonWiringConfig { GpioPinNumber = 6, ButtonNumber = 3 },
            new ButtonWiringConfig { GpioPinNumber = 12, ButtonNumber = 4 },
            new ButtonWiringConfig { GpioPinNumber = 13, ButtonNumber = 5 },
            new ButtonWiringConfig { GpioPinNumber = 16, ButtonNumber = 6 },
            //new ButtonWiringConfig { GpioPinNumber = 17, ButtonNumber = 4 },
            new ButtonWiringConfig { GpioPinNumber = 18, ButtonNumber = 7 },
            //new ButtonWiringConfig { GpioPinNumber = 20, ButtonNumber = 4 },
            //new ButtonWiringConfig { GpioPinNumber = 21, ButtonNumber = 8 }
            new ButtonWiringConfig { GpioPinNumber = 23, ButtonNumber = 8 },
            new ButtonWiringConfig { GpioPinNumber = 24, ButtonNumber = 9 }
            //new ButtonWiringConfig { GpioPinNumber = 25, ButtonNumber = 25 }

        };


        private void InitGpio()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                DebugMsg("There is no GPIO controller on this device.");
                return;
            }

            buttonPinConfig.ForEach(bc => InitializeButtonPin(gpio, bc));

            DebugMsg("GPIO pins initialized.");
        }

        private void gpioPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            DebugMsg(string.Format("gpioPin_ValueChanged: PIN {0}   EDGE: {1}", sender.PinNumber, args.Edge));

            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                var buttonConfig = this.buttonPinConfig.SingleOrDefault(c => c.GpioPinNumber == sender.PinNumber);
                if (buttonConfig != null)
                {
                    var buttonNumber = buttonConfig.ButtonNumber;
                    ButtonPressUp(buttonNumber);
                }
            }
        }

        private void ButtonPressUp(int buttonNumber)
        {
            DebugMsg(string.Format("Button #{0} Pressed", buttonNumber));
            TryPostButtonPressToServer(buttonNumber, DateTime.Now);
        }



        private Boolean TryPostButtonPressToServer(int buttonNumber, DateTime timeOccurred)
        {
            bool postSuccessful = false;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var values = new Dictionary<string, string> {
                        { "assetID", this.ASSET_ID },
                        { "buttonId", buttonNumber.ToString() },
                        { "secondsSinceEvent", (DateTime.Now - timeOccurred).TotalSeconds.ToString() },
                    };
                    using (FormUrlEncodedContent content = new FormUrlEncodedContent(values))
                    {
                        client.Timeout = TimeSpan.FromSeconds(30);
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("multipart/form-data"));

                        DebugMsg("Posting data to server...");

                        HttpResponseMessage result = client.PostAsync(SCRAP_REPORTING_ENDPOINT, content).Result;
                        postSuccessful = result.IsSuccessStatusCode;

                        DebugMsg(string.Format("Posting {0} ({1} {2})",
                            (postSuccessful ? "Successful" : "Failed"),
                            result.StatusCode, result.ReasonPhrase)
                        );
                    }
                }
                catch (Exception ex)
                {
                    DebugMsg(ex.ToString());
                    postSuccessful = false;
                }
            }

            return postSuccessful;
        }

        private void DebugMsg(string message)
        {
            var nowString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.WriteLine(nowString + " | " + message);
        }


        private void InitializeButtonPin(GpioController gpio, ButtonWiringConfig buttonConfig)
        {
            buttonConfig.GpioPin = gpio.OpenPin(buttonConfig.GpioPinNumber);

            // Check if input pull-up resistors are supported
            buttonConfig.GpioPin.SetDriveMode(buttonConfig.GpioPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp) ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);

            // Set a debounce timeout to filter out switch bounce noise from a button press
            buttonConfig.GpioPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            // Register for the ValueChanged event so our buttonPin_ValueChanged 
            // function is called when the button is pressed
            buttonConfig.GpioPin.ValueChanged += gpioPin_ValueChanged;
        }
    }

    public sealed class ButtonWiringConfig
    {
        public int GpioPinNumber { get; set; }
        public int ButtonNumber { get; set; }
        public GpioPin GpioPin { get; set; }
    }
}
