using System.Collections.Generic;
using CITChat.Models;

namespace CITChat.Controllers
{
    /// <summary>
    /// </summary>
    public class MessageComparer : IComparer<Message>
    {
        /// <summary>
        /// </summary>
        public static readonly MessageComparer Instance = new MessageComparer();

        /// <summary>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(Message x, Message y)
        {
            int compareResult = x.ConversationId.CompareTo(y.ConversationId);
            if (compareResult != 0)
            {
                return compareResult;
            }
            compareResult = x.MessageId.CompareTo(y.MessageId);
            return compareResult;
        }
    }
}