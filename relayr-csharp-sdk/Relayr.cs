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

namespace relayr_csharp_sdk
{
    public class Relayr
    {
        private static string _userId;

        public static string UserId
        {
            get { return Relayr._userId; }
            set { Relayr._userId = value; }
        }
        private static string _name;

        public static string Name
        {
            get { return Relayr._name; }
            set { Relayr._name = value; }
        }
        private static string _email;

        public static string Email
        {
            get { return Relayr._email; }
            set { Relayr._email = value; }
        }

        public static HttpManager Http
        {
            get
            {
                return HttpManager.Manager;
            }
        }

        public static string OauthToken
        {
            get
            {
                return HttpManager.Manager.OauthToken;
            }
            set
            {
                HttpManager.Manager.OauthToken = value;

                // We need to get different credentials if this value has changed
                _userId = null;
            }
        }

        // Get a list of transmitters associated with the given user. Returns a list of dynamics, each
        // representing one of the transmitters returned by the API
        public static async Task<List<dynamic>> GetTransmittersAsync()
        {
            // Get the user information if this hasn't been done already
            if (_userId == null)
            {
                await getUserInformation();
            }

            HttpResponseMessage response = await HttpManager.Manager.PerformHttpOperation(ApiCall.TransmittersListByUser,
                                                                                            new String[] { _userId }, null);
            dynamic transmitterList = await HttpManager.Manager.ConvertResponseContentToObject(response);

            List<dynamic> transmitters = new List<dynamic>();
            for (int i = 0; i < transmitterList.Length; i++)
            {
                transmitters.Add(transmitterList[i]);
            }

            return transmitters;
        }

        public static async Task<List<dynamic>> GetDevicesAsync()
        {
            if (_userId == null)
            {
                await getUserInformation();
            }

            var response = await HttpManager.Manager.PerformHttpOperation(ApiCall.DevicesListUnderSpecificUser, new string[] { _userId }, null);
            dynamic deviceList = await HttpManager.Manager.ConvertResponseContentToObject(response);
            var devices = new List<dynamic>();

            for (int i = 0; i < deviceList.Length; i++)
            {
                devices.Add(deviceList[i]);
            }

            return devices;
        }

        // Try to connect to the broker using the given dynamic representation of a transmitter and
        // the specified clientID. If connection is successful, it will return a Transmitter isntance
        // representing the transmitter. Returns null otherwise.
        public static Transmitter ConnectToBroker(dynamic transmitterInfo, string clientId)
        {

            // Check arguments
            if (transmitterInfo == null)
            {
                throw new ArgumentException("Transmitter Info argument cannot be null");
            }

            if (clientId == null || clientId.Equals(""))
            {
                throw new ArgumentException("ClientId cannot be null or empty");
            }

            // Get required fields, try to open a connection
            string transmitterId = (string)transmitterInfo["id"];
            string transmitterSecret = (string)transmitterInfo["secret"];

            Transmitter transmitter = new Transmitter(transmitterId);
            bool success = transmitter.ConnectToBroker(clientId, transmitterSecret);

            if (!success)
            {
                return null;
            }

            return transmitter;
        }

        // Gets the userId of the logged in user. Necessary for making further API calls.
        private static async Task getUserInformation()
        {
            HttpResponseMessage response = await HttpManager.Manager.PerformHttpOperation(ApiCall.UserGetInfo, null, null);
            dynamic userInfo = await HttpManager.Manager.ConvertResponseContentToObject(response);
            _userId = (string)userInfo["id"];
            _name = (string)userInfo["name"];
            _email = (string)userInfo["email"];
        }

        public static async Task getAllUserInformation()
        {
            HttpResponseMessage response = await HttpManager.Manager.PerformHttpOperation(ApiCall.UserGetInfo, null, null);
            dynamic userInfo = await HttpManager.Manager.ConvertResponseContentToObject(response);
            _userId = (string)userInfo["id"];
            _name = (string)userInfo["name"];
            _email = (string)userInfo["email"];
        }


    }
}
