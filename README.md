Part of the Serverless Azure-based .Net 2.0 Standard Broadcast solution.

[![Build Status](https://dev.azure.com/bitfox/AzureBroadcast.Client/_apis/build/status/BuildPipeline%20AzureBroadcast.Client?branchName=master)](https://dev.azure.com/bitfox/AzureBroadcast.Client/_build/latest?definitionId=5&branchName=master)

# Introduction

Azure provides a fully managed and scalable SignalR service for real-time communication, but... It still requires a back-end to trigger messages. Fortunately you can use Azure Functions for that to get a full serverless experience! A lot of sample implementations exists, but they are either incomplete, specialized messaging or complex. 

This solution gives you a predefined set of Azure Functions, with corresponding Client library to get you started with real-time communication in any (desktop, web) .net standard 2.0 application with your own custom message structure. 

## AzureBroadcast.Client
A .net standard 2.0 client-side library to be used alongside the Azure Functions and Azure SignalR service for a full serverless setup. 


## Basic usage of client

Simple string based broadcasting:

``` csharp
    var client = new BroadcastClient<string>('endpoint-of-azure-functions');

    client.onMessage = (msg) => { 
        //do something usefull with received messages;
    }

    client.Start();
    client.Broadcast("Hello World!");

```` 

You can also provide your own message structure:

``` csharp
    public class MyMessage() {
        public DateTime timestamp;
        public string message;
    }

    var client = new BroadcastClient<MyMessage>('endpoint-of-azure-functions');
    client.onMessage = (msg) => { 
        //msg.message
        //msg.timestamp ...
    }
```

## Installation

This client library is provided as [NuGet package](https://www.nuget.org/packages/Bitfox.AzureBroadcast.Client/).

Make sure you setup the corresponding Azure Functions and Azure SignalR service as pointed out [here](https://github.com/bitfox-git/azurebroadcast-functions).


