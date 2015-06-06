using System.Collections.Generic;
using CITChat.Models;

namespace CITChat.Controllers.DataTransferObjects
{
    /// <summary>
    ///     Data transfer object for <see cref="User" />
    /// </summary>
    public sealed class UserDto
    {
        //[Key]
        public int UserId { get; set; }
        public int ConversationId { get; set; }
        public string UserName { get; set; }

        public UserDto()
        {
        }

        public UserDto(User user)
        {
            UserId = user.UserId;
            UserName = user.UserName;
        }

        public User ToEntity()
        {
            return new User
                {
                    UserId = UserId,
                    UserName = UserName,
                    ConversationUsers = new List<ConversationUser>(),
                };
        }

        public void UpdateEntity(User user)
        {
            user.UserId = UserId;
            user.UserName = UserName;
        }
    }
}