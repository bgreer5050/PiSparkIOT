﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace SparkPiMockUI
{
    public class Network
    {
        private string _macAddress;
        private bool _networkUp;


        public string MacAddress
        {
            get
            {
                if (this._macAddress == null || this._macAddress == "")
                {
                    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                    if (nics == null || nics.Length < 1)
                    {
                        Debug.Print("  No network interfaces found.");
                        return "";
                    }
                    else
                    {
                       // IPInterfaceProperties properties = nics[0].GetIPProperties();
                        PhysicalAddress address = nics[0].GetPhysicalAddress();
                        byte[] bytes = address.GetAddressBytes();
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            // Display the physical address in hexadecimal.
                           Debug.Print("{0}", bytes[i].ToString("X2"));
                            // Insert a hyphen after each byte, unless we are at the end of the  
                            // address. 
                            if (i != bytes.Length - 1)
                            {
                                Debug.Print("-");
                            }
                        }
                    }
                }
                return _macAddress;
            }
            set
            {
                _macAddress = value;
            }
        }

        public bool NetworkUp
        {
            get
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
            set
            {
                _networkUp = value;
            }
        }
    }
}