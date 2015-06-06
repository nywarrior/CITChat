using Microsoft.AspNet.SignalR;

namespace CITChat.Hubs
{
    public class CITChatHub : Hub
    {
        public void SendConversationChanged(int conversationId)
        {
            // Call the SendConversationChanged method to update clients.
            Clients.All.conversationChanged(conversationId);
        }
    }
}