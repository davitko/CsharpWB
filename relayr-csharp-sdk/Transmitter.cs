/*
 * Copyright (c) 2014 iThings4U Gmbh
 * 
 * Author:
 *      Peter Dwersteg      
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace relayr_csharp_sdk
{
    public class Transmitter
    {
        #region fields and properties

        private MqttClient _client;
        private Dictionary<string, Device> _connectedDevices;

        private List<dynamic> _devices;

        public readonly Dictionary<string, Device> Devices;
        public QualityOfService DefaultQualityOfService;

        private string _transmitterId;
        public string TransmitterId
        {
            get
            {
                return _transmitterId;
            }
        }

        private string _clientId;
        public string ClientId
        {
            get
            {
                return _clientId;
            }
        }

        #endregion

        public Transmitter(string transmitterId)
        {
            if (transmitterId == null || transmitterId == "")
            {
                throw new ArgumentException("Transmitter ID cannot be null or empty");
            }

            _client = new MqttClient("mqtt.relayr.io");
            _client.MqttMsgPublishReceived += _client_MqttMsgPublishReceived;
            
            _connectedDevices = new Dictionary<string, Device>();
            DefaultQualityOfService = QualityOfService.AtLeastOnce;

            _transmitterId = transmitterId;
        }

        #region Connect and Disconnect

        // Connect to the MQTT broker using the provided credentials. Returns a bool representing
        // whether the connection was successful or not
        public bool ConnectToBroker(string clientId, string transmitterSecret)
        {
            if (transmitterSecret == null)
            {
                throw new ArgumentException("Transmitter Secret cannot be null");
            }

            if (clientId == null || clientId.Equals(""))
            {
                throw new ArgumentException("ClientId cannot be null or empty");
            }

            _clientId = clientId;

            _client.Connect(clientId, _transmitterId, transmitterSecret);
            return _client.IsConnected;
        }

        // Disconnect from the MQTT broker. Returns a bool representing whether the operation was
        // successful or not
        public Task<bool> DisconnectFromBrokerAsync()
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

        #region Subscribe and Unsubscribe from Device

        // Subscribe to new data coming from the device represented by the deviceInfo, with the
        // default MQTT quality of service
        public async Task<Device> SubscribeToDeviceDataAsync(dynamic deviceInfo)
        {
            return await SubscribeToDeviceDataAsync(deviceInfo, DefaultQualityOfService);
        }

        // Subscribe to new data coming from the device represented by the deviceInfo, with the
        // specified MQTT quality of service
        public async Task<Device> SubscribeToDeviceDataAsync(dynamic deviceInfo, QualityOfService qualityOfService)
        {
            string id = Device.GetDeviceIdFromDynamic(deviceInfo);
            if (id == null)
            {
                throw new ArgumentException("Passed dynamic object must be of a relayr device type");
            }

            return await SubscribeToDeviceDataAsync(id, qualityOfService);
        }

        // Subscribe to new data coming from the device specified by the deviceId, with the
        // default MQTT quality of service
        public async Task<Device> SubscribeToDeviceDataAsync(string deviceId)
        {
            return await SubscribeToDeviceDataAsync(deviceId, DefaultQualityOfService);
        }

        // Subscribe to new data coming from the device specified by the deviceId, with the
        // specified MQTT quality of service. Return a Device isntance representing the device if
        // a device with that ID is registered to this transmitter. Otherwise, return null
        public async Task<Device> SubscribeToDeviceDataAsync(string deviceId, QualityOfService qualityOfService)
        {
            // Argument checking
            if(deviceId == null || deviceId.Equals(""))
            {
                throw new ArgumentException("Device ID cannot be null or empty");
            }

            // Determine whether the device is actually registered to the transmitter
            if (_devices == null)
            {
                await GetDevicesAsync();   
            }

            bool found = false;
            foreach(dynamic deviceObj in _devices) {
                if (((string)deviceObj["id"]).Equals(deviceId))
                {
                    found = true;
                    break;
                }
            }

            // return null if no such device is registered to a transmitter
            if (!found)
            {
                return null;
            }

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

            // Subscribe to the MQTT topic for this device
            string topic = "/v1/" + deviceId + "/data";
            _client.Subscribe(new string[] { topic }, new byte[] { serviceLevel });

            // Create a device and add it to the list
            Device device = new Device(deviceId, topic, qualityOfService);
            _connectedDevices.Add(topic, device);

            // Return a reference to the device
            return device;
        }

        // Unsubscribe from the device specified by the deviceId
        public bool UnsubscribeFromDeviceData(string deviceId)
        {
            string topic = "/v1/" + deviceId + "/data";

            // Remove the device from the set of devices
            if (_connectedDevices.ContainsKey(topic))
            {
                _client.Unsubscribe(new string[] { topic });
                _connectedDevices.Remove(topic);
                return true;
            }
            return false;
        }

        // Unsubscribe from the device represented by the passed dynamic object
        public bool UnsubscribeFromDeviceData(dynamic device)
        {
            string id = Device.GetDeviceIdFromDynamic(device);
            if (id == null)
            {
                throw new ArgumentException("Passed dynamic object must be of a relayr device type");
            }

            return UnsubscribeFromDeviceData(id);
        }

        #endregion

        // Get a list of all devices registered to this transmitter
        // Returns an empty list if there are no devices
        public async Task<List<dynamic>> GetDevicesAsync()
        {
            HttpResponseMessage response = await HttpManager.Manager.PerformHttpOperation(ApiCall.TransmittersListConnectedDevices, 
                                                                                            new String[] { _transmitterId }, null);
            dynamic deviceList = await HttpManager.Manager.ConvertResponseContentToObject(response);

            List<dynamic> devices = new List<dynamic>();
            for (int i = 0; i < deviceList.Length; i++)
            {
                devices.Add(deviceList[i]);
            }

            // Update the list of devices
            _devices = devices;
            return devices;
        }

        // Called whenever a new item of data is published by any topic the manager is subscribed to
        private void _client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if (_connectedDevices.ContainsKey(e.Topic))
            {
                Device device = _connectedDevices[e.Topic];

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
