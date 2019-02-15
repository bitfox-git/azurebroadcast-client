﻿using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bitfox.AzureBroadcast
{

    public class BroadcastClient<T>
    {

        private HubConnection connection;
        private HttpClient httpClient;
        private string baseAzureFunctionUrl;
        private string userId;


        public Action<T, IBroadcastInfo> onMessage = null;

        public BroadcastClient(string AzureFunctionUrl, string FunctionHostKey) :
            this(AzureFunctionUrl, FunctionHostKey, Guid.NewGuid().ToString())
        {
            
        }

        public BroadcastClient(string AzureFunctionUrl, string FunctionHostKey, string userId)
        {
            this.baseAzureFunctionUrl = urlEndsWithSlash(AzureFunctionUrl);
            this.userId = userId;
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-ms-signalr-userid", userId);
            httpClient.DefaultRequestHeaders.Add("x-functions-key", FunctionHostKey);
        }

        private string urlEndsWithSlash(string url)
        {
            return url.EndsWith("/") ? url : url + "/";
        }

        private async Task<SignalRConnectionInfo> GetSignalRConnectionInfo()
        {
            var url = $"{baseAzureFunctionUrl}api/negotiate";
            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var connectionInfo = JsonConvert.DeserializeObject<SignalRConnectionInfo>(content);
            return connectionInfo;
        }


        public async void Start()
        {
            //Prevent multiple starts.
            if (connection != null) { return; }
           
            //Get the Azure SignalR service Url and Token.
            var connectionInfo = await GetSignalRConnectionInfo();

            //Build normal SignalR connection, with provided Token.
            connection = new HubConnectionBuilder()
                .WithUrl(connectionInfo.Url, (options) =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(connectionInfo.AccessToken);
                })
                .Build();

            //Auto reconnected on disconnect
            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            //Call the delegate when receiving message.
            connection.On<string>("newMessage", (wrappedmessage) =>
            {
                BroadcastMessage messageObject = JsonConvert.DeserializeObject<BroadcastMessage>(wrappedmessage);
                T message = JsonConvert.DeserializeObject<T>(messageObject.jsonmessage);
                onMessage?.Invoke(message, messageObject);
            });

            await connection.StartAsync();
        }

        public async void Send(T message)
        {
            var url = $"{baseAzureFunctionUrl}api/broadcast";
            var wrapped =  new BroadcastMessage() {
                toGroupName="",
                toUser="",
                fromUser=userId,
               jsonmessage = JsonConvert.SerializeObject(message)
            };
            var msg = JsonConvert.SerializeObject(wrapped);
            HttpContent c = new StringContent(msg, Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, c);
        }

        public async void SendToGroup(T message, string groupName)
        {
            var url = $"{baseAzureFunctionUrl}api/broadcast";
            var wrapped = new BroadcastMessage(){
                toGroupName=groupName,
                toUser="",
                fromUser=userId,
                jsonmessage = JsonConvert.SerializeObject(message)
            };
            var msg = JsonConvert.SerializeObject(wrapped);
            HttpContent c = new StringContent(msg, Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, c);
        }

         public async void SendToUser(T message, string user)
        {
            var url = $"{baseAzureFunctionUrl}api/broadcast";
            var wrapped = new BroadcastMessage(){
                toGroupName="",
                toUser=user,
                fromUser=userId,
                jsonmessage = JsonConvert.SerializeObject(message)
            };
            var msg = JsonConvert.SerializeObject(wrapped);
            HttpContent c = new StringContent(msg, Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, c);
        }


        public async void JoinGroup(string groupName){
            var url = $"{baseAzureFunctionUrl}api/groupaction";

            GroupActionMessage gam = new GroupActionMessage() {
                groupAction= GroupAction.Add,
                groupName = groupName
            };
            //convert gam to json
            var msg = JsonConvert.SerializeObject(gam);
            HttpContent c = new StringContent(msg, Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, c);
        }

        public async void LeaveGroup(string groupName){
            var url = $"{baseAzureFunctionUrl}api/groupaction";

            GroupActionMessage gam = new GroupActionMessage() {
                groupAction= GroupAction.Remove,
                groupName = groupName
            };
            //convert gam to json
            var msg = JsonConvert.SerializeObject(gam);
            HttpContent c = new StringContent(msg, Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, c);
        }



    }
}
