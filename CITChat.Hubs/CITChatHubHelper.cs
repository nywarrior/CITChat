using System;
using Microsoft.AspNet.SignalR;

namespace CITChat.Hubs
{
    public class CITChatHubHelper
    {
        // Singleton instance
        private readonly static Lazy<CITChatHubHelper> Instance = new Lazy<CITChatHubHelper>(
            () => new CITChatHubHelper(GlobalHost.ConnectionManager.GetHubContext<CITChatHub>()));

        private IHubContext Context { get; set; }

        private CITChatHubHelper(IHubContext context)
        {
            Context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="loginUserName"></param>
        public static void SendLogin(string url, string loginUserName)
        {
            CITChatHubHelper citChatHubHelper = Instance.Value;
            citChatHubHelper.Context.Clients.All.login(url, loginUserName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public static void SendLogoff(string url)
        {
            CITChatHubHelper citChatHubHelper = Instance.Value;
            citChatHubHelper.Context.Clients.All.logoff(url);
        }
    }
}
