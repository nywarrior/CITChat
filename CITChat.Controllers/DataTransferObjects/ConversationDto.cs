using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CITChat.Controllers.Contexts;
using CITChat.Models;

namespace CITChat.Controllers.DataTransferObjects
{
    /// <summary>
    ///     Data transfer object for <see cref="Conversation" />
    /// </summary>
    public sealed class ConversationDto
    {
        [Key]
        public int ConversationId { get; set; }

        [Required]
        public string Title { get; set; }

        public DateTime StartDateTime { get; set; }
        public string StartDateTimeDisplayString { get; set; }
        public List<MessageDto> Messages { get; set; }
        public List<UserDto> Users { get; set; }

        /// <summary>
        /// </summary>
        public ConversationDto()
        {
            Messages = new List<MessageDto>();
            Users = new List<UserDto>();
        }

        public ConversationDto(Conversation conversation, User loginUser)
            : this()
        {
            using (ConversationContext db = new ConversationContext())
            {
                ConversationId = conversation.ConversationId;
                Title = conversation.Title;
                StartDateTime = conversation.StartDateTime;
                StartDateTimeDisplayString = conversation.StartDateTimeDisplayString;
                if (conversation.Messages != null)
                {
                    foreach (Message item in conversation.Messages)
                    {
                        Messages.Add(new MessageDto(item));
                    }
                }
                if (conversation.ConversationUsers != null)
                {
                    foreach (ConversationUser conversationUser in conversation.ConversationUsers)
                    {
                        User user = db.Users.Find(conversationUser.UserId);
                        Users.Add(new UserDto(user));
                    }
                }
            }
        }

        public void UpdateEntity(Conversation conversation)
        {
            using (ConversationContext db = new ConversationContext())
            {
                conversation.Title = Title;
                conversation.StartDateTime = StartDateTime;
                conversation.StartDateTimeDisplayString = StartDateTimeDisplayString;
                conversation.ConversationId = ConversationId;
                foreach (MessageDto messageDto in Messages)
                {
                    Message existingMessage =
                        db.Messages.FirstOrDefault(
                            message =>
                            (message.ConversationId == messageDto.ConversationId) &&
                            (message.MessageId == messageDto.MessageId));
                    if (existingMessage == null)
                    {
                        existingMessage = messageDto.ToEntity();
                        conversation.Messages.Add(existingMessage);
                    }
                    else
                    {
                        messageDto.UpdateEntity(existingMessage);
                    }
                }
                foreach (UserDto userDto in Users)
                {
                    User user = db.Users.Find(userDto.UserId);
                    if (user == null)
                    {
                        user = userDto.ToEntity();
                        db.Users.Add(user);
                    }
                    else
                    {
                        userDto.UpdateEntity(user);
                    }
                    //ConversationUser existingConversationUser =
                    //    db.ConversationUsers.FirstOrDefault(c => (c.ConversationId == userDto.ConversationId) &&
                    //                                             (c.UserId == userDto.UserId));
                    //if (existingConversationUser == null)
                    //{
                    //    existingConversationUser = new ConversationUser
                    //        {
                    //            ConversationId = ConversationId,
                    //            UserId = userDto.UserId
                    //        };
                    //    db.ConversationUsers.Add(existingConversationUser);
                    //}
                    //ConversationUser existingConversationUser2 =
                    //    user.ConversationUsers.FirstOrDefault(c => (c.ConversationId == userDto.ConversationId) &&
                    //                                             (c.UserId == userDto.UserId));
                    //if (existingConversationUser2 == null)
                    //{
                    //    user.ConversationUsers.Add(existingConversationUser);
                    //}
                }
            }
        }

        public Conversation ToEntity()
        {
            Conversation conversation = new Conversation
                {
                    Title = Title,
                    StartDateTime = StartDateTime,
                    StartDateTimeDisplayString =
                        StartDateTime.ToShortDateString() + " " + StartDateTime.ToShortTimeString(),
                    ConversationId = ConversationId,
                    ConversationUsers = new List<ConversationUser>(),
                    Messages = new List<Message>()
                };
            foreach (MessageDto messageDto in Messages)
            {
                Message message = messageDto.ToEntity();
                conversation.Messages.Add(message);
            }
            return conversation;
        }
    }
}