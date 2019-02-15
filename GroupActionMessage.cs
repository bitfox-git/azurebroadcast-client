using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bitfox.AzureBroadcast
{


    public class GroupActionMessage
    {
        public GroupAction groupAction { get; set; }
        public string groupName { get; set; }
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum GroupAction
    {
        Add = 0,
        Remove = 1
    }

}