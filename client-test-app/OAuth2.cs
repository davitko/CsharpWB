//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Net;
//using System.IO;
//using System.Web.Script.Serialization;

//namespace ClientTestApp
//{
//    class OAuth2
//    {
//        // Setup the variables necessary to make the Request 
//      string grantType = "password";
//      string applicationID = "{applicationId}";
//      string clientString = "{clientString}";
//      string username = "{username}";
//      string password = "{userPassword}";
//      string url = "https://{domain}/token";
//      HttpWebResponse response = null;

//      try
//      {
//        // Create the data to send
//        StringBuilder data = new StringBuilder();
//        data.Append("grant_type=" + Uri.EscapeDataString(grantType));
//        data.Append("&client_id=" + Uri.EscapeDataString(applicationID));
//        data.Append("&username=" + Uri.EscapeDataString(clientString + "\\" + username));
//        data.Append("&password=" + Uri.EscapeDataString(password));

//        // Create a byte array of the data to be sent
//        byte[] byteArray = Encoding.UTF8.GetBytes(data.ToString());

//        // Setup the Request
//        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
//        request.Method = "POST";
//        request.ContentType = "application/x-www-form-urlencoded";
//        request.ContentLength = byteArray.Length;

//        // Write data
//        Stream postStream = request.GetRequestStream();
//        postStream.Write(byteArray, 0, byteArray.Length);
//        postStream.Close();

//        // Send Request & Get Response
//        response = (HttpWebResponse)request.GetResponse();

//        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
//        {
//          // Get the Response Stream
//          string json = reader.ReadLine();
//          Console.WriteLine(json);

//          // Retrieve and Return the Access Token
//          JavaScriptSerializer ser = new JavaScriptSerializer();
//          Dictionary<string, object> x = (Dictionary<string, object>)ser.DeserializeObject(json);
//          string accessToken =  x["access_token"].ToString();
//        }
//      }
//      catch (WebException e)
//      {
//        // This exception will be raised if the server didn't return 200 - OK
//        // Retrieve more information about the error
//        if (e.Response != null)
//        {
//          using (HttpWebResponse err = (HttpWebResponse)e.Response)
//          {
//            Console.WriteLine("The server returned '{0}' with the status code '{1} ({2:d})'.",
//              err.StatusDescription, err.StatusCode, err.StatusCode);
//          }
//        }
//      }
//      finally
//      {
//        if (response != null) { response.Close(); }
//      }

//      Console.ReadLine();

//    }
//}
