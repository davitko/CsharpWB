/*
 * Copyright (c) 2014 iThings4U Gmbh
 * 
 * Author:
 *      Peter Dwersteg      
 */

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace relayr_csharp_sdk
{
    public static class MqttChannelManager
    {
        private static MqttClient _client;
        private static Dictionary<string, Device> _subscribedDevices;
        private static Dictionary<string, string> _deviceIdToTopic;     // for unsubscribing
        private static Dictionary<string, string> _arguments;           // for http message content
        private static QualityOfService DefaultQualityOfService;

        public static bool IsConnected
        {
            get
            {
                return _client.IsConnected;
            }
        }

        public static string ClientId;

        // Set everything up
        private static void Initialize() {
            _client = new MqttClient("mqtt.relayr.io");
            _client.MqttMsgPublishReceived += _client_MqttMsgPublishReceived;

            _subscribedDevices = new Dictionary<string, Device>();
            _deviceIdToTopic = new Dictionary<string, string>();

            _arguments = new Dictionary<string, string>();
            _arguments.Add("transport", "mqtt");
            _arguments.Add("deviceId", "");

            // Assign a random value to the clientId if one hasn't been set yet.
            Random r = new Random();
            if (ClientId == null) 
            { 
                ClientId = r.Next(0, short.MaxValue) + "";
            }

            // Assign the quality of service to be AtLeastOnce if it hasn't been set yet.
            if (DefaultQualityOfService == null)
            {
                DefaultQualityOfService = QualityOfService.AtLeastOnce;
            }
        }

        #region Connect and Disconnect

        // Connect to the broker. Must use the userId and password returned in a MQTT
        // channel request to the relayr API
        public static void ConnectToBroker(string userId, string password, string clientId)
        {
            _client.Connect(ClientId, userId, password);
        }

        // Connect to the broker. Must use the userId and password returned in a MQTT
        // channel request to the relayr API. Uses the default client ID.
        public static void ConnectToBroker(string userId, string password)
        {
            ConnectToBroker(userId, password, ClientId);
        }

        // Attempts to disconnect from the broker. Returns a bool representing success.
        public static Task<bool> DisconnectFromBrokerAsync()
        {
            return Task.Run(() =>
            {
                if (!_client.IsConnected)
                {
                    return true;
                }

                try
                {
                    _client.Disconnect();
                    while (_client.IsConnected)
                    {
                        Task.Delay(100);
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        #endregion

        #region Subscribe and Unsubscribe

        // Does the meat of the work in subscribing to a device (as well as initalization and 
        // connecting if need be)
        public static async Task<Device> SubscribeToDeviceDataAsync(string deviceId, QualityOfService qualityOfService)
        {
            // Initialize, if required
            if (_client == null)
            {
                Initialize();
            }

            // Get the connection information for this device
            _arguments["deviceId"] = deviceId;
            HttpResponseMessage response = await HttpManager.Manager.PerformHttpOperation(ApiCall.CreateChannel, null, _arguments);
            dynamic connectionInfo = await HttpManager.Manager.ConvertResponseContentToObject(response);

            // Use that connection information to attempt to connect to the mqtt server, if necessary
            // Return null if a connection couldn't be created
            if (!_client.IsConnected)
            {
                string userId = (string)connectionInfo["credentials"]["user"];
                string password = (string)connectionInfo["credentials"]["password"];

                ConnectToBroker(userId, password);
                if (!_client.IsConnected)
                {
                    return null;
                }
            }

            // Get the quality of service byte
            byte serviceLevel = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE;
            switch (qualityOfService)
            {
                case QualityOfService.AtMostOnce:
                    serviceLevel = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE;
                    break;
                case QualityOfService.ExactlyOnce:
                    serviceLevel = MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE;
                    break;
                case QualityOfService.GrantedFailure:
                    serviceLevel = MqttMsgBase.QOS_LEVEL_GRANTED_FAILURE;
                    break;
            }

            // Subscribe to data from the device
            string topic = (string)connectionInfo["credentials"]["topic"];
            _client.Subscribe(new string[] { topic }, new byte[] { serviceLevel });

            Device device = new Device(deviceId, topic, qualityOfService);
            _subscribedDevices.Add(topic, device);
            _deviceIdToTopic.Add(deviceId, topic);

            return device;
        }

        // Does the meat of the work in subscribing to a device (as well as initalization and
        // connecting if need be)
        public static async Task<Device> SubscribeToDeviceDataAsync(string deviceId)
        {
            return await SubscribeToDeviceDataAsync(deviceId, DefaultQualityOfService);
        }

        // Does the meat of the work in subscribing to a device (as well as initalization and 
        // connecting if need be)
        public static async Task<Device> SubscribeToDeviceDataAsync(dynamic device, QualityOfService qualityOfService)
        {
            string id = Device.GetDeviceIdFromDynamic(device);
            if (id == null)
            {
                throw new ArgumentException("Passed dynamic object must be of a relayr device type");
            }

            return await SubscribeToDeviceDataAsync(id, qualityOfService);
        }

        // Does the meat of the work in subscribing to a device (as well as initalization and 
        // connecting if need be)
        public static async Task<Device> SubscribeToDeviceDataAsync(dynamic device)
        {
            return await SubscribeToDeviceDataAsync(device, DefaultQualityOfService);
        }

        // Usubscribe from data coming from the device specified by the deviceId
        public static bool UnsubscribeFromDeviceData(string deviceId)
        {
            if (_deviceIdToTopic.ContainsKey(deviceId))
            {
                // Attempt to unsubscribe from the device
                string topic = _deviceIdToTopic[deviceId];
                _client.Unsubscribe(new string[] { topic });

                // Delete the device from records
                _subscribedDevices.Remove(topic);
                _deviceIdToTopic.Remove(deviceId);
                return true;
            }

            return false;
        }

        // Usubscribe from data coming from the device specified by the deviceId
        public static bool UnsubscribeFromDeviceData(dynamic device)
        {
            string id = Device.GetDeviceIdFromDynamic(device);
            if(id == null)
            {
                throw new ArgumentException("Passed dynamic object must be of a relayr device type");
            }

            return UnsubscribeFromDeviceData(id);
        }

        // Usubscribe from data coming from the device specified by the deviceId
        public static bool UnsubscribeFromDeviceData(Device device)
        {
            return UnsubscribeFromDeviceData(device.DeviceId);
        }

        #endregion

        // Raise the new data event in the appropriate device object
        private static void _client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if(_subscribedDevices.ContainsKey(e.Topic)) 
            {
                Device device = _subscribedDevices[e.Topic];

                QualityOfService qosLevel = QualityOfService.AtLeastOnce;
                switch (e.QosLevel)
                {
                    case MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE:
                        qosLevel = QualityOfService.AtMostOnce;
                        break;
                    case MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE:
                        qosLevel = QualityOfService.ExactlyOnce;
                        break;
                    case MqttMsgBase.QOS_LEVEL_GRANTED_FAILURE:
                        qosLevel = QualityOfService.GrantedFailure;
                        break;
                }

                device.NewData(e.Message, e.Retain, e.DupFlag, qosLevel);
            }
        }
    }
}
