using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bitfox.AzureBroadcast
{
    /// <summary>
    /// A client to send messages via a predefined Azure Functions and Azure SignalR service.
    /// </summary>
    /// <typeparam name="T">string or your custom message class.</typeparam>
    public class BroadcastClient<T>
    {

        private HubConnection connection;
        private HttpClient httpClient;
        private string baseAzureFunctionUrl;
        private string userId;

        /// <summary>
        /// Add a handler to receive your messages.
        /// </summary>
        public Action<T, IBroadcastInfo> onMessage = null;

        /// <summary>
        /// Return the connection status of this client.
        /// </summary>
        public bool IsConnected { 
            get {
                if (connection==null) { return false; }
                return (connection.State == HubConnectionState.Connected);
            }
        }


        /// <summary>
        /// If true then messages originating from this userid, are not notified on the onMessage handler.
        /// </summary>
        public bool FilterOwnMessages { get; set; } = false;

        /// <summary>
        /// Creates a new BroadcastClient
        /// </summary>
        /// <param name="AzureFunctionUrl">The URL with /api of the Azure Functions</param>
        /// <param name="FunctionHostKey">The host key to access the protected Azure Functions</param>
        public BroadcastClient(string AzureFunctionUrl, string FunctionHostKey) :
            this(AzureFunctionUrl, FunctionHostKey, Guid.NewGuid().ToString())
        {
            
        }

        /// <summary>
        /// Creates a new BroadcastClient with a specific userid
        /// </summary>
        /// <param name="AzureFunctionUrl">The URL with /api of the Azure Functions</param>
        /// <param name="FunctionHostKey">The host key to access the protected Azure Functions</param>
        /// <param name="userId">A custom userid</param>
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
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var connectionInfo = JsonConvert.DeserializeObject<SignalRConnectionInfo>(content);
            return connectionInfo;
        }

        /// <summary>
        /// Setup and connect to SignalR. Starts listening for incoming messages.
        /// </summary>
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
                if (!((messageObject.fromUser == userId) & FilterOwnMessages))
                {
                    T message = JsonConvert.DeserializeObject<T>(messageObject.jsonmessage);
                    onMessage?.Invoke(message, messageObject);
                }
           
            });

            await connection.StartAsync();
        }

        /// <summary>
        /// Send message to every connected client.
        /// </summary>
        /// <param name="message"></param>
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

        /// <summary>
        /// Send message to a named group
        /// </summary>
        /// <param name="message"></param>
        /// <param name="groupName">The name of the group</param>
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

        /// <summary>
        /// Send message to specific user(id)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="user">The userid receiving this message. UserId can be (optionally) specified in the constructor of the client</param>
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

        /// <summary>
        /// This client starts listening for messages designated to the group specified.
        /// </summary>
        /// <param name="groupName">The name of the group</param>
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

        /// <summary>
        /// This client stops listening for messages designated to the group specified.
        /// </summary>
        /// <param name="groupName">The name of the group</param>
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
