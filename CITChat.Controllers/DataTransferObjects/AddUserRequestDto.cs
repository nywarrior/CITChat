using CITChat.Models;

namespace CITChat.Controllers.DataTransferObjects
{
    /// <summary>
    ///     Data transfer object for <see cref="User" />
    /// </summary>
    public sealed class AddUserRequestDto
    {
        //[Key]
        public int UserId { get; set; }
        public int ConversationId { get; set; }
        public string UserName { get; set; }
    }
}