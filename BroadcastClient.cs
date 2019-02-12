using Microsoft.AspNetCore.SignalR.Client;
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


        public Action<T> onMessage = null;

        public BroadcastClient(string AzureFunctionUrl) :
            this(AzureFunctionUrl, new Guid().ToString())
        {
        }

        public BroadcastClient(string AzureFunctionUrl, string userId)
        {
            this.baseAzureFunctionUrl = urlEndsWithSlash(AzureFunctionUrl);
            this.userId = userId;
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-ms-signalr-userid", userId);

            //TODO 1.1 feature; add SharedSecret to public Azure Function.
            //httpClient.DefaultRequestHeaders.Add("x-bf-sharedsecret", "");
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
            connection.On<string>("newMessage", (message) =>
            {
                T messageObject = JsonConvert.DeserializeObject<T>(message);
                onMessage?.Invoke(messageObject);
            });

            await connection.StartAsync();
        }

        public async void Send(T message)
        {
            var url = $"{baseAzureFunctionUrl}api/broadcast";
            var msg = JsonConvert.SerializeObject(message);
            HttpContent c = new StringContent(msg, Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, c);
        }



    }
}
