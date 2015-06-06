using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITChat.Models
{
    /// <summary>
    ///     Message entity
    /// </summary>
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        public virtual List<ConversationUser> ConversationUsers { get; set; }
        public string UserName { get; set; }
    }
}