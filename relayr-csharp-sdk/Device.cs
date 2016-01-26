/*
 * Copyright (c) 2014 iThings4U Gmbh
 * 
 * Author:
 *      Peter Dwersteg      
 */

using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace relayr_csharp_sdk
{
    public class Device
    {
        private string _deviceId;
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _firmwareVersion;

        public string FirmwareVersion
        {
            get { return _firmwareVersion; }
            set { _firmwareVersion = value; }
        }
        private string _description;

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        private string _topic;
        private QualityOfService _qosLevel;

        public string DeviceId
        {
            get
            {
                return _deviceId;
            }
        }

        public string Topic
        {
            get
            {
                return _topic;
            }
        }

        public QualityOfService QosLevel
        {
            get
            {
                return _qosLevel;
            }
        }

        public event PublishedDataReceivedEventHandler PublishedDataReceived;

        public Device(string deviceId, string topic, QualityOfService qosLevel)
        {
            _deviceId = deviceId;
            _topic = topic;
            _qosLevel = qosLevel;
        }

        public Device(string deviceId, string name, string firmwareVersion)
        {
            _deviceId = deviceId;
            _name = name;
            _firmwareVersion = firmwareVersion;
        }

        public Device(dynamic wbDevice)
        {
            _deviceId = wbDevice.Data["id"];
            _name = wbDevice.Data["name"];
            _firmwareVersion = wbDevice.Data["firmwareVersion"];
        }

        // Returns an ID if the dynamic is a valid relayr device. Return null if not
        public static string GetDeviceIdFromDynamic(dynamic device)
        {
            try
            {
                string test = (string)device["public"];
                string id = (string)device["id"];
                return id;
            }
            catch
            {
                return null;
            }
        }

        // Provide new data to the device, process the data into a dynamic object, and raise the data received
        // event. The user could call this, but why would they?
        public virtual void NewData(byte[] rawData, bool retain, bool dupFlag, QualityOfService qosLevel)
        {
            if (PublishedDataReceived != null)
            {
                string json = System.Text.Encoding.UTF8.GetString(rawData, 0, rawData.Length);

                dynamic dataObject;

                try
                {
                    // If the Json data is an array, parse it as one
                    if (json[0] == '[')
                    {
                        JArray responseElements = JArray.Parse(json);
                        dataObject = responseElements.ToArray<dynamic>();
                    }
                    // Otherise, parse it as a single object
                    else
                    {
                        dataObject = JObject.Parse(json);
                    }
                }
                catch (Exception e)
                {
                    throw new ArgumentException("There was an error processing the JSON, it is probably improperly formatted", e);
                }

                PublishedDataReceivedEventArgs args = new PublishedDataReceivedEventArgs(dataObject, dupFlag, retain, qosLevel);
                PublishedDataReceived(this, args);
            }
        }
    }
}
