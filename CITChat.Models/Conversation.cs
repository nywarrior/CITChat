using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITChat.Models
{
    /// <summary>
    ///     Conversation entity
    /// </summary>
    public class Conversation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConversationId { get; set; }

        public virtual List<ConversationUser> ConversationUsers { get; set; }
        public virtual List<Message> Messages { get; set; }

        [Required]
        public string Title { get; set; }

        //[Required]
        public DateTime StartDateTime { get; set; }
        public string StartDateTimeDisplayString { get; set; }
    }
}