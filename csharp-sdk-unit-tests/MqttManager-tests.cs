using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Threading.Tasks;
using relayr_csharp_sdk;
using System.Net.Http;

namespace csharp_sdk_unit_tests
{
    [TestClass]
    public class MqttManager_tests
    {
        private string transmitterId = string.Empty;
        private string transmitterSecret = string.Empty;

        [TestInitialize]
        public async Task GetHttpValues()
        {
            HttpManager.Manager.OauthToken = "E1cZfqqPbdcZtm1zlC.FIRX5qKQxtdCs";

            HttpResponseMessage userInfoResponse = await HttpManager.Manager.PerformHttpOperation(
                                               ApiCall.UserGetInfo, null, null);
            dynamic userInfo = await HttpManager.Manager.ConvertResponseContentToObject(userInfoResponse);
            string userId = (string)userInfo["id"];

            HttpResponseMessage transmittersResponse = await HttpManager.Manager.PerformHttpOperation(
                                                    ApiCall.TransmittersListByUser,
                                                    new string[] { userId }, null);
            dynamic transmitters = await HttpManager.Manager.ConvertResponseContentToObject(transmittersResponse);

            transmitterId = (string)transmitters[0]["id"];
            transmitterSecret = (string)transmitters[0]["secret"];
        }

        #region ConnectToBroker

        /* ConnectToBroker should return true if the connection was successful, and false if it wasn't
         * The values of the arguments shouldn't matter, but they shouldn't be null.
         *      If any value is null, throw an InvalidArgumentException
         *      If any value is empty string, false
         */
        
        [TestMethod]
        public void ConnectToBroker_ValidCredentials_Success()
        {
            Transmitter transmitter = new Transmitter(transmitterId);
            Assert.IsTrue(transmitter.ConnectToBroker("hello", transmitterSecret));
        }

        [TestMethod]
        public void ConnectToBroker_Incorrect_TransmitterId_Return_False()
        {
            Transmitter transmitter = new Transmitter("asdasdf");
            Assert.IsFalse(transmitter.ConnectToBroker("hello", transmitterSecret));
        }

        [TestMethod]
        public void ConnectToBroker_Empty_TransmitterId_ThrowInvalidArgsException()
        {
            try
            {
                Transmitter transmitter = new Transmitter("");
                Assert.Fail();
            }
            catch(ArgumentException)
            {

            }
        }

        [TestMethod]
        public void ConnectToBroker_Null_TransmitterId_ThrowInvalidArgsException()
        {
            try 
            { 
                Transmitter transmitter = new Transmitter(null);
                Assert.Fail();
            }
            catch(ArgumentException)
            {
            
            }
        }

        [TestMethod]
        public void ConnectToBroker_Null_ClientId_ThrowInvalidArgsException()
        {
            try
            {
                Transmitter transmitter = new Transmitter(transmitterId);
                transmitter.ConnectToBroker(null, transmitterSecret);
                Assert.Fail();
            }
            catch(ArgumentException)
            {

            }
        }

        [TestMethod]
        public void ConnectToBroker_Null_Secret_ThrowInvalidArgsException()
        {
            try
            {
                Transmitter transmitter = new Transmitter(transmitterId);
                transmitter.ConnectToBroker("hello", null);
                Assert.Fail();
            }
            catch (ArgumentException)
            {

            }
        }

        [TestMethod]
        public void ConnectToBroker_Empty_ClientId_False()
        {
            Transmitter transmitter = new Transmitter(transmitterId);
            Assert.IsFalse(transmitter.ConnectToBroker("", transmitterSecret));
        }

        [TestMethod]
        public void ConnectToBroker_Empty_Secret_False()
        {
            Transmitter transmitter = new Transmitter(transmitterId);
            Assert.IsFalse(transmitter.ConnectToBroker("hello", ""));
        }

        #endregion

        #region DisconnectFromBroker NOT IMPLEMENTED

        #endregion

        #region SubscribeToSensor

        /* When deviceId is null or empty, throw an ArgumentException
         * If connection is failed, return a null device and don't add it to the transmitter's internal list
         */

        [TestMethod]
        public async void SubscribeToSensor_Null_DeviceId_ThrowException()
        {
            try 
            { 
                Transmitter transmitter = new Transmitter(transmitterId);
                transmitter.ConnectToBroker("hello", transmitterSecret);
                await transmitter.SubscribeToDeviceDataAsync(null);
                Assert.Fail();
            }
            catch(ArgumentException)
            {

            }
        }

        [TestMethod]
        public async void SubscribeToSensor_Empty_DeviceId_ThrowException()
        {
            try
            {
                Transmitter transmitter = new Transmitter(transmitterId);
                transmitter.ConnectToBroker("hello", transmitterSecret);
                await transmitter.SubscribeToDeviceDataAsync("");
                Assert.Fail();
            }
            catch (ArgumentException)
            {

            }
        }

        [TestMethod]
        public async void SubscribeToSensor_Connection_Fails_ReturnNull()
        {
            Transmitter transmitter = new Transmitter(transmitterId);
            transmitter.ConnectToBroker("hello", transmitterSecret);
            Device device = await transmitter.SubscribeToDeviceDataAsync("asdasd33");

            //Assert.IsNull(device);
        }

        [TestMethod]
        public async void SubscribeToSensor_Connection_Successful_ReturnDevice()
        {
            Transmitter transmitter = new Transmitter(transmitterId);
            transmitter.ConnectToBroker("hello", transmitterSecret);
            Device device = await transmitter.SubscribeToDeviceDataAsync("d1fa0703-36bb-4694-8db1-a71becc9d489");

            Assert.IsNotNull(device);
        }

        #endregion
    }
}
