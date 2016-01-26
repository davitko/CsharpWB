using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using relayr_csharp_sdk;
using Windows.UI.Core;
using System.Net.Http;
using System.Threading.Tasks;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ClientTestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string userId { get; set; }
        private string userName { get; set; }
        private string userEmail { get; set; }
        List<Device> devicesOfUser = new List<Device>();
        Relayr wunderbarSDK = new Relayr();

        public MainPage()
        {
            this.InitializeComponent();


            checkEmail("john.stroux@lanceict.com");
            //checkUserProperties();
            devicesUnderSpecificUser("084257a1-a100-40a6-bd54-ec6018bc181a");

            Thermometer();
            Light();
            Microphone();
            Gyroscope();


            //TestClient();
            //TestClientDevices();
        }

        public async void checkEmail(string email)
        {
            var baseAddress = new Uri("https://api.relayr.io/");

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {

                using (var response = await httpClient.GetAsync("users/validate?email=" + email))
                {

                    string responseData = await response.Content.ReadAsStringAsync();

                }
            }
        }

        //public static async Task checkUserProperties()
        //{

        //    Relayr.OauthToken = "8IBG3kct-JmrRy_cU5-IoLN708Z9_fAn";
        //    Relayr.getAllUserInformation();
        //    userId = Relayr.UserId;
        //    userName = Relayr.Name;
        //    userEmail = Relayr.Email;
        //    outputUserInformation.Text = "ID: " + userId + "Name: " + userName + "Email: " + userEmail;

        //}

        public async void getMeasurements()
        {
            // Set the OAuth token = "YOUR OAUTH TOKEN HERE"
            Relayr.OauthToken = "8IBG3kct-JmrRy_cU5-IoLN708Z9_fAn";


            // Get a list of transmitters and connect to the MQTT broker with the first one in the list
            List<dynamic> transmitters = await Relayr.GetTransmittersAsync();   // "CLIENT ID OF YOUR CHOICE"
            Transmitter transmitter = Relayr.ConnectToBroker(transmitters[0], "4f585a21-f19b-44cc-8874-7afef539dcbb");

            // Get a list of devices associated with the transmitter, 
            // Subscribe to data from the first device on the list
            List<dynamic> devices = await transmitter.GetDevicesAsync();    // "{ADD SENSOR ID}"
            Device[] listOfDevices;
            for (int i = 0; i < devices.Count; i++)
            {
                Device tmpDevice = new Device((devices[i]["id"]).ToString(), (devices[i]["name"]).ToString(), (devices[i]["firmwareVersion"]).ToString());
                devicesOfUser.Add(tmpDevice);
                listOfDevices[i] = await transmitter.SubscribeToDeviceDataAsync((devices[i]["id"]).ToString());
            }




        }

        public async void devicesUnderSpecificUser(string userId)
        {
            // Set the OAuth token = "YOUR OAUTH TOKEN HERE"
            Relayr.OauthToken = "8IBG3kct-JmrRy_cU5-IoLN708Z9_fAn";


            // Get a list of transmitters and connect to the MQTT broker with the first one in the list
            List<dynamic> transmitters = await Relayr.GetTransmittersAsync();   // "CLIENT ID OF YOUR CHOICE"
            Transmitter transmitter = Relayr.ConnectToBroker(transmitters[0], "4f585a21-f19b-44cc-8874-7afef539dcbb");

            // Get a list of devices associated with the transmitter, 
            // Subscribe to data from the first device on the list
            List<dynamic> devices = await transmitter.GetDevicesAsync();    // "{ADD SENSOR ID}"


            for (int i = 0; i < devices.Count; i++)
            {
                Device tmpDevice = new Device((devices[i]["id"]).ToString(), (devices[i]["name"]).ToString(), (devices[i]["firmwareVersion"]).ToString());
                devicesOfUser.Add(tmpDevice);

            }

        }


        public async void Thermometer()
        {
            // Set the OAuth token = "YOUR OAUTH TOKEN HERE"
            Relayr.OauthToken = "8IBG3kct-JmrRy_cU5-IoLN708Z9_fAn";

            // Get a list of transmitters and connect to the MQTT broker with the first one in the list
            List<dynamic> transmitters = await Relayr.GetTransmittersAsync();   // "CLIENT ID OF YOUR CHOICE"
            Transmitter transmitter = Relayr.ConnectToBroker(transmitters[0], "4f585a21-f19b-44cc-8874-7afef539dcbb");

            // Get a list of devices associated with the transmitter, 
            // Subscribe to data from the first device on the list
            List<dynamic> devices = await transmitter.GetDevicesAsync();    // "{ADD SENSOR ID}"
            Device deviceThermometer = await transmitter.SubscribeToDeviceDataAsync("77262bfa-f7ac-4bae-a6fb-c5999e3aaddd"); ;
            //foreach (Device dv in devicesOfUser)
            //{
            //    if (dv.Name == "thermometer")
            //    {

            //    }
            //}
            //deviceThermometer = await transmitter.SubscribeToDeviceDataAsync((dv.DeviceId).toString());
            deviceThermometer.PublishedDataReceived += device_PublishedDataReceived;

        }

        public async void Light()
        {
            // Set the OAuth token
            Relayr.OauthToken = "8IBG3kct-JmrRy_cU5-IoLN708Z9_fAn";

            // Get a list of transmitters and connect to the MQTT broker with the first one in the list
            List<dynamic> transmitters = await Relayr.GetTransmittersAsync();
            Transmitter transmitter = Relayr.ConnectToBroker(transmitters[0], "4f585a21-f19b-44cc-8874-7afef539dcbb");

            // Get a list of devices associated with the transmitter, 
            // Subscribe to data from the first device on the list
            List<dynamic> devices = await transmitter.GetDevicesAsync();
            Device device = await transmitter.SubscribeToDeviceDataAsync("0bae152b-eb10-42ce-b90a-afd6b641c2e6");
            device.PublishedDataReceived += device_PublishedDataReceivedLight;
        }

        public async void Microphone()
        {
            // Set the OAuth token
            Relayr.OauthToken = "8IBG3kct-JmrRy_cU5-IoLN708Z9_fAn";

            // Get a list of transmitters and connect to the MQTT broker with the first one in the list
            List<dynamic> transmitters = await Relayr.GetTransmittersAsync();
            Transmitter transmitter = Relayr.ConnectToBroker(transmitters[0], "4f585a21-f19b-44cc-8874-7afef539dcbb");

            // Get a list of devices associated with the transmitter, 
            // Subscribe to data from the first device on the list
            List<dynamic> devices = await transmitter.GetDevicesAsync();
            Device device = await transmitter.SubscribeToDeviceDataAsync("918bb95a-e156-4761-9b76-45b96689d5f8");
            device.PublishedDataReceived += device_PublishedDataReceivedMicrophone;
        }

        public async void Gyroscope()
        {
            // Set the OAuth token
            Relayr.OauthToken = "8IBG3kct-JmrRy_cU5-IoLN708Z9_fAn";

            // Get a list of transmitters and connect to the MQTT broker with the first one in the list
            List<dynamic> transmitters = await Relayr.GetTransmittersAsync();
            Transmitter transmitter = Relayr.ConnectToBroker(transmitters[0], "4f585a21-f19b-44cc-8874-7afef539dcbb");

            // Get a list of devices associated with the transmitter, 
            // Subscribe to data from the first device on the list
            List<dynamic> devices = await transmitter.GetDevicesAsync();
            Device device = await transmitter.SubscribeToDeviceDataAsync("384a86b9-c602-4b55-8057-51831cf87b05");
            device.PublishedDataReceived += device_PublishedDataReceivedGyroscope;
        }



        // Create Handler for the the sensor's data published event
        void device_PublishedDataReceived(object sender, PublishedDataReceivedEventArgs args)
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                outputThermometer.Text = "The Temperature is " + args.Data["temp"] + " °C - TimeStamp: " + args.Data["ts"];
                outputThermometer2.Text = "The Humidity is " + args.Data["hum"] + " % - TimeStamp:" + args.Data["ts"];
                //outputLight.Text = "The amount of light is " + args.Data["light"] + "lux and amount of red, green and blue: " + args.Data["clr"];
                //outputMicrophone.Text = "The Noise is " + args.Data["hum"] + " dB";
                string temp = args.Data["temp"];
            });
        }

        void device_PublishedDataReceivedMicrophone(object sender, PublishedDataReceivedEventArgs args)
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                outputMicrophone.Text = "The Noise is " + args.Data["snd_level"] + " dB - TimeStamp:" + args.Data["ts"];
            });
        }

        void device_PublishedDataReceivedLight(object sender, PublishedDataReceivedEventArgs args)
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                outputLight.Text = "The amount of light is " + args.Data["light"] + " lux and amount of red, green and blue: " + args.Data["clr"];
            });
        }

        void device_PublishedDataReceivedGyroscope(object sender, PublishedDataReceivedEventArgs args)
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                outputGyroscope.Text = "The angularSpeed is " + args.Data["gyro"] + " and acceleration is: " + args.Data["accel"];
            });
        }
    }
}


































































//public async void TestClient()
//{
//    // Set the OAuth token
//    Relayr.OauthToken = "8IBG3kct-JmrRy_cU5-IoLN708Z9_fAn";
//    var Transmitters = await Relayr.GetTransmittersAsync();

//    // Get a list of transmitters and connect to the MQTT broker with the first one in the list
//    var transmitterInfo = Transmitters[0];
//    var transmitter = Relayr.ConnectToBroker(transmitterInfo, "1ee793be-aba4-422f-a775-ddc448006fd0");
//    var devices = await transmitter.GetDevicesAsync();


//    // Get a list of devices associated with the transmitter, subscribe to data
//    // From the first device on the list
//    //Device d = await transmitter.SubscribeToDeviceDataAsync(devices[3]);
//    //d.PublishedDataReceived += d_PublishedDataReceived;

//    // Get a list of devices associated with the transmitter, 
//    // Subscribe to data from the first device on the list
//    List<dynamic> devices = await transmitter.GetDevicesAsync();
//    Device device = await transmitter.SubscribeToDeviceDataAsync("77262bfa-f7ac-4bae-a6fb-c5999e3aaddd");
//    device.PublishedDataReceived += device_PublishedDataReceived;

//}

//public async void TestClientDevices()
//{
//    Relayr.OauthToken = "8IBG3kct-JmrRy_cU5-IoLN708Z9_fAn";
//    var devices = await Relayr.GetDevicesAsync();
//}