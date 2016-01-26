/*
 * Copyright (c) 2014 iThings4U Gmbh
 * 
 * Author:
 *      Peter Dwersteg      
 */

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace relayr_csharp_sdk
{

    public class HttpManager
    {
        static private HttpManager _manager;
        
        private string _oauthToken;
        private HttpClient _httpClient;

        private bool _conversionDummyBool;
        private double _conversionDummyDouble;

        // Initialize the HttpClient with the base URI of the relayr.api
        private HttpManager() 
        {
            _httpClient = new HttpClient();
            Uri baseUri = new Uri("https://api.relayr.io/");
            _httpClient.BaseAddress = baseUri;
        }

        #region Fields

        // Create / return reference to singleton manager object
        public static HttpManager Manager
        {
            get
            {
                if (_manager == null)
                {
                    _manager = new HttpManager();
                }

                return _manager;
            }
        }

        // Get or set the Oauth token. On set token will automatically be added to request headers
        public string OauthToken
        {
            get
            {
                return _oauthToken;
            }
            set
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value);
                _oauthToken = value;
            }
        }

        #endregion

        // Perform a relayr API operation. Takes the operation, URI arguments, and a dictionary
        // of key -> value pairs (which will be converted to a JSON string) for operation content
        public async Task<HttpResponseMessage> PerformHttpOperation(ApiCall operation, string[] arguments, Dictionary<string, string> content) {
            
            // Get the URI extension and opration type from the attributes of the operation
            Type type = operation.GetType();
            FieldInfo field = type.GetRuntimeField(operation.ToString());
            string uriExtension = ((UriAttribute) field.GetCustomAttribute(typeof(UriAttribute), false)).Value;
            string operationType = ((OperationType) field.GetCustomAttribute(typeof(OperationType), false)).Value;

            // Add the arguments to the URI, if required
            uriExtension = processArguments(uriExtension, arguments);

            // Create the content json, if required
            StringContent contentJson = null;
            if (content != null)
            {
                contentJson = new StringContent(createContentJson(content));
                contentJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            // Set up the http request
            HttpRequestMessage httpRequest = new HttpRequestMessage();
            httpRequest.Method = new HttpMethod(operationType);
            httpRequest.RequestUri = new Uri(uriExtension, UriKind.Relative);
            httpRequest.Content = contentJson;

            // Send the http request, return the response message
            try 
            { 
                return await _httpClient.SendAsync(httpRequest);
            }
            catch(ProtocolViolationException e)
            {
                throw new InvalidOperationException("Cannot send content with operation of type " + operationType, e);
            }
        }
        
        // Convert the JSON content of the response message into an object whose values can be easily accessed
        public async Task<dynamic> ConvertResponseContentToObject(HttpResponseMessage message)
        {
            // Throw an InvalidOperation exception if the message isn't 200 OK
            if (!message.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Response code is: " 
                    + message.StatusCode.ToString()
                    + ". Cannot create a object from JSON response if the operation was not successful (200 OK).");
            }

            if (message.Content == null)
            {
                return null;
            }

            string returnedJson = await message.Content.ReadAsStringAsync();

            if (returnedJson.Equals(""))
            {
                return null;
            }

            try 
            { 
                // Check if the returned json is an array of objects. If so, parse as one.
                if (returnedJson[0] == '[')
                {
                    JArray responseElements = JArray.Parse(returnedJson);
                    return responseElements.ToArray<dynamic>();
                }

                // Otherwise, parse it as a single json object
                return JObject.Parse(returnedJson);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error when parsing JSON content", e);
            }
        }

#region Helpers

        // Take a uri and arguments, insert the arguments into the URI, return the URI
        private string processArguments(string uri, string[] args)
        {
            if (args == null)
            {
                if (uri.Contains('!'))
                {
                    throw new ArgumentException("Expected arguments, received none");
                }
                else
                {
                    return uri;
                }
            }

            string[] uriPieces = uri.Split('!');

            // Throw an argumentexception if we didn't get the correct number of uri arguments
            if (uriPieces.Length - 1 != args.Length)
            {
                throw new ArgumentException("Expected " + (uriPieces.Length - 1) + " number of arguments. Got " + args.Length + ".");
            }

            StringBuilder uriWithArgs = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                uriWithArgs.Append(uriPieces[i]);
                uriWithArgs.Append(args[i]);
            }
            uriWithArgs.Append(uriPieces[uriPieces.Length - 1]);
            return uriWithArgs.ToString();
        }

        // Turn the dictionary of key -> value into json string of name : value
        // quotation marked bools and numbers will be inserted into json without quotation marks
        private string createContentJson(Dictionary<string, string> content)
        {
            StringBuilder jsonString = new StringBuilder();
            bool first = true;
            jsonString.Append("{ ");
            foreach (KeyValuePair<string, string> pair in content)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    jsonString.Append(", ");
                }

                jsonString.Append("\"").Append(pair.Key).Append("\" : ");

                // ############### GIANT HACK NOT FOR CHILDREN'S EYES ##################

                if (Boolean.TryParse(pair.Value, out _conversionDummyBool) || 
                    Double.TryParse(pair.Value, out _conversionDummyDouble))
                {
                    jsonString.Append(pair.Value);
                }
                else
                {
                    jsonString.Append("\"").Append(pair.Value).Append("\"");
                }

                // #####################################################################
            }
            jsonString.Append(" }");

            return jsonString.ToString();
        }

        #endregion
    }        
}
