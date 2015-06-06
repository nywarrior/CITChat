using System.ComponentModel.DataAnnotations.Schema;

namespace CITChat.Models
{
    /// <summary>
    ///     Conversation entity
    /// </summary>
    [Table("ConversationUsers")]
    public class ConversationUser
    {
        [ForeignKey("Conversation")]
        public int ConversationId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        //public int[] PrimaryKey
        //{
        //    get { return ConversationContext.CreateConversationUsersKey(ConversationId, UserId); }
        //}

        public virtual Conversation Conversation { get; set; }
        public virtual User User { get; set; }
    }
}