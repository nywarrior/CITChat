using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITChat.Models
{
    /// <summary>
    ///     Message entity
    /// </summary>
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageId { get; set; }

        [ForeignKey("Conversation")]
        public int ConversationId { get; set; }

        public virtual Conversation Conversation { get; set; }

        [Required]
        public string Content { get; set; }

        public string TranslatedContent { get; set; }
        public bool IsDone { get; set; }
    }
}