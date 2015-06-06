using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using CITChat.Controllers.Contexts;
using CITChat.Models;

namespace CITChat.Controllers
{
    public static class UserManager
    {
        private const string UserNameQueryStringParameterName = "UserName";

        /// <summary>
        /// </summary>
        /// <param name="apiController"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static User GetLoginUser(ApiController apiController, ConversationContext db)
        {
            // Look for the user name on the request query string.
            HttpRequestMessage request = apiController.Request;
            NameValueCollection queryStringParameters = HttpUtility.ParseQueryString(request.RequestUri.Query);
            string loginUserName = queryStringParameters[UserNameQueryStringParameterName];
            if (loginUserName == "undefined")
            {
                loginUserName = null;
            }
            if (string.IsNullOrEmpty(loginUserName))
            {
                return null;
            }
            //if (request.RequestUri != null)
            //{
            //    string url = request.RequestUri.AbsoluteUri;
            //    CITChatHubHelper.SendLogin(url, loginUserName);
            //}
            User user = FindUserWithUserName(db, loginUserName);
            if (user == null)
            {
                user = Enumerable.FirstOrDefault(db.Users, u => u.UserName == loginUserName);
            }
            if (user == null)
            {
                user = new User {UserName = loginUserName};
                db.Users.Add(user);
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                }
            }
            return user;
        }

        /// <summary>
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static ConversationUser FindConversationUsersWithUserId(Conversation conversation, int userId)
        {
            if (conversation.ConversationUsers == null)
            {
                conversation.ConversationUsers = new List<ConversationUser>();
            }
            return conversation.ConversationUsers.FirstOrDefault(c => c.UserId == userId);
        }

        /// <summary>
        /// </summary>
        /// <param name="apiController"></param>
        /// <param name="db"></param>
        /// <param name="conversation"></param>
        /// <returns></returns>
        public static bool ConversationHasLoginUser(ApiController apiController, ConversationContext db,
                                                    Conversation conversation)
        {
            User loginUser = GetLoginUser(apiController, db);
            if (loginUser == null)
            {
                return false;
            }
            bool hasLoginUser = ConversationHasUserWithUserId(conversation, loginUser.UserId);
            return hasLoginUser;
        }

        /// <summary>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static User FindUserWithUserName(ConversationContext db, string userName)
        {
            return Enumerable.FirstOrDefault(db.Users, user => user.UserName == userName);
        }

        /// <summary>
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool ConversationHasUserWithUserId(Conversation conversation, int userId)
        {
            ConversationUser conversationUsers = FindConversationUsersWithUserId(conversation, userId);
            return (conversationUsers != null);
        }

        //private static string GetLoginUserNameFromSession()
        //{
        //    HttpContext httpContext = HttpContext.Current;
        //    if (httpContext != null)
        //    {
        //        HttpSessionState session = httpContext.Session;
        //        if (session != null)
        //        {
        //            string loginUserName = (string)session[UserNameQueryStringParameterName];
        //            return loginUserName;
        //        }
        //    }
        //    return null;
        //}
    }
}