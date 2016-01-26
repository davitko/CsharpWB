# The relayr C# SDK

Welcome to the relayr C# SDK. The SDK allows you to access your WunderBar sensor data and the [relayr Cloud Platform](https://developer.relayr.io/documents/Welcome/Platform) functionality from, you guessed it, apps built in C#!

Because it's built as a portable class library, you'll be able to use the same SDK across Windows 8.1, Windows Phone 8.1, and Windows Desktop Apps.

## Getting Started

The C# SDK has two main functions: 

1. Programatically exposing the relayr API. This is achieved through the HttpManager class.
2. Allowing applications to subscribe to the streams of data published by sensor modules and other connected devices. The Transmitter and Device classes handle this functionality. You will need security strings obtained through the HttpManager to subscribe to data topics.

### Importing NuGet Libraries

Before you can build a project including the C# SDK, you'll need to import two libraries via the [NuGet](https://www.nuget.org/) package manager in Visual Studio.

1. In Visual Studio, go to *Tools > NuGet Package Manager > Manage NuGet Packages for Solution.*

2. Select *Online* in the left hand menu and then search for and install the following two packages:

	**Json.NET** and **M2Mqtt**

### Getting an OAuth token

The HttpManager requires an OAuth token to authorize your HTTP requests. You can obtain a token in two manners:

1. Via an OAuth authentication flow: Due to the platform-specific nature of performing this task, the C# SDK does not have the ability to execute this process for you. If you choose to get a token using this method, you will need to implement it yourself. Documentation on how the relayr OAuth implementation can be found in [our documentation center](https://developer.relayr.io/documents/Welcome/OAuthReference).

2. By generating one through the relayr Dashboard. On the [API Keys](https://developer.relayr.io/dashboard/apps/myApps) page, create an app (or use an app previously created). 
Clicking the "Generate Token" button will create the required OAuth token. See detailed instructions [here](https://developer.relayr.io/documents/WebDev/OAuthToken). This value can be hard-coded into your app, and allows you access to data from sensors and devices associated with your relayr account. This is extremely useful for prototyping and personal-use apps, however, it prevents you from being able to make your app widely available. You will need to use the first method of generating the token, if you wish to distribute your application.

## Using the SDK

For the full SDK documentation please refer to our [C# Documentation](https://developer.relayr.io/documents/CSharp/Reference).