using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitfox.AzureBroadcast
{

    /// <summary>
    /// Information needed to get the SignalR Client connected to Azure SignalR service.
    /// </summary>
    public class SignalRConnectionInfo
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
    }
}
