Part of the Serverless Azure-based .Net 2.0 Standard Broadcast solution.

[![Build Status](https://dev.azure.com/bitfox/AzureBroadcast.Client/_apis/build/status/BuildPipeline%20AzureBroadcast.Client?branchName=master)](https://dev.azure.com/bitfox/AzureBroadcast.Client/_build/latest?definitionId=5&branchName=master)

# Introduction

Azure provides a fully managed and scalable SignalR service for real-time communication, but... It still requires a back-end to trigger messages. Fortunately you can use Azure Functions for that to get a full serverless experience! A lot of sample implementations of these functions exists, but they are either incomplete or complex. 

This solution gives you a [predefined set of Azure Functions](https://github.com/bitfox-git/azurebroadcast-functions), along with this corresponding client library to get you started with real-time communication in any (desktop, web) .net standard 2.0 application. With your own custom message structure and grouping.

 
## AzureBroadcast.Client 
A .net standard 2.0 client-side library to be used alongside the Azure Functions and Azure SignalR service for a full serverless setup. 


## Basic usage of client

Simple string based broadcasting:

``` csharp
    var client = new BroadcastClient<string>('endpoint-of-azure-functions','hostkey');

    client.onMessage = (msg, info) => { 
        //do something usefull with received messages;
    }

    client.Start();
    client.Send("Hello World!");

```` 

You can also provide your own message structure:

``` csharp
    public class MyMessage() {
        public DateTime timestamp;
        public string message;
    }

    var client = new BroadcastClient<MyMessage>('endpoint-of-azure-functions');
    client.onMessage = (msg, info) => { 
        //msg.message
        //msg.timestamp ...
    }
```

## Features

Seperating on channels or groups or topics:
``` csharp
client.JoinGroup('my-test-channel');
client.LeaveGroup('my-test-channel'); 
```

Sending to specific groups or a specific user

``` csharp
client.SendToGroup(msg, 'my-test-channel');
client.SendToUser(msg, 'some user id'); 
```





## Installation

This client library is provided as [NuGet package](https://www.nuget.org/packages/Bitfox.AzureBroadcast.Client/).

Make sure you setup the corresponding Azure Functions and Azure SignalR service as pointed out [here](https://github.com/bitfox-git/azurebroadcast-functions).


