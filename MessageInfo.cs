namespace Bitfox.AzureBroadcast
{
    public interface IMessageInfo {
            string toGroupName {get;set;}
            string toUser {get;set;}
            string fromUser {get;set;}
        }

}