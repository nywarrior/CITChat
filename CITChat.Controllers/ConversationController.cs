using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CITChat.Controllers.Contexts;
using CITChat.Controllers.DataTransferObjects;
using CITChat.Models;

namespace CITChat.Controllers
{
    //[Authorize]
    public class ConversationController : ApiController
    {
        // GET api/Conversation
        public HttpResponseMessage GetConversations()
        {
            using (ConversationContext db = new ConversationContext())
            {
                User loginUser = GetLoginUser(db);
                if (loginUser == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }
                List<Conversation> conversations =
                    db.Conversations
                      .Where(c =>
                             (from cu in db.ConversationUsers
                              where
                                  (cu.UserId == loginUser.UserId) &&
                                  (cu.ConversationId == c.ConversationId)
                              select c).FirstOrDefault() != null)
                      .OrderByDescending(c => c.ConversationId)
                      .AsEnumerable().ToList();
                foreach (Conversation conversation in conversations)
                {
                    conversation.Messages.Sort(MessageComparer.Instance);
                }
                List<ConversationDto> conversationDtos =
                    conversations.Select(conversation => new ConversationDto(conversation, loginUser)).ToList();
                HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK, conversationDtos);
                return httpResponseMessage;
            }
        }

        // GET api/Conversation/5
        public ConversationDto GetConversation(int id)
        {
            using (ConversationContext db = new ConversationContext())
            {
                // ConversationContext.NonLazyLoadingInstance;
                User loginUser = GetLoginUser(db);
                if (loginUser == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }
                Conversation conversation = db.Conversations.Find(id);
                if (conversation == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }
                if (!UserManager.ConversationHasLoginUser(this, db, conversation))
                {
                    // Trying to modify a record that does not belong to the user
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }
                return new ConversationDto(conversation, loginUser);
            }
        }

        // PUT api/Conversation/5
        //[ValidateHttpAntiForgeryToken]
        public HttpResponseMessage PutConversation(int id, ConversationDto conversationDto)
        {
            using (ConversationContext db = new ConversationContext())
            {
                User loginUser = GetLoginUser(db);
                if (loginUser == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                if (id != conversationDto.ConversationId)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                Conversation conversation = db.Conversations.Find(id);
                if (conversation == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }
                if (!UserManager.ConversationHasLoginUser(this, db, conversation))
                {
                    // Trying to modify a record that does not belong to the user
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                conversationDto.UpdateEntity(conversation);
                db.Entry(conversation).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        // POST api/Conversation
        //[ValidateHttpAntiForgeryToken]
        public HttpResponseMessage PostConversation(ConversationDto conversationDto)
        {
            using (ConversationContext db = new ConversationContext())
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                User loginUser = GetLoginUser(db);
                if (loginUser == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }
                //if (!conversationDto.HasLoginUser)
                //{
                UserDto userDto = new UserDto
                    {
                        UserId = loginUser.UserId,
                        UserName = loginUser.UserName,
                        ConversationId = conversationDto.ConversationId
                    };
                conversationDto.Users.Add(userDto);
                //}
                Conversation conversation = db.Conversations.Find(conversationDto.ConversationId);
                if (conversation == null)
                {
                    conversation = conversationDto.ToEntity();
                    db.Conversations.Add(conversation);
                }
                else
                {
                    conversationDto.UpdateEntity(conversation);
                }
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                }
                conversationDto.ConversationId = conversation.ConversationId;
                int loginUserId = loginUser.UserId;
                ConversationUser conversationUsers =
                    db.ConversationUsers.FirstOrDefault(
                        c => (c.ConversationId == conversation.ConversationId) && (c.UserId == loginUserId));
                if (conversationUsers == null)
                {
                    conversationUsers = new ConversationUser
                        {
                            ConversationId = conversation.ConversationId,
                            UserId = loginUserId
                        };
                    db.ConversationUsers.Add(conversationUsers);
                }
                ConversationUser existingConversationUsers =
                    conversation.ConversationUsers.FirstOrDefault(
                        c => (c.ConversationId == conversation.ConversationId) && (c.UserId == loginUserId));
                if (existingConversationUsers == null)
                {
                    conversation.ConversationUsers.Add(conversationUsers);
                }
                //conversation.Users.Add(loginUser);
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, conversationDto);
                string link = Url.Link("DefaultApi", new {id = conversationDto.ConversationId});
// ReSharper disable ConvertIfStatementToNullCoalescingExpression
                if (link == null)
// ReSharper restore ConvertIfStatementToNullCoalescingExpression
                {
                    link = string.Empty;
                }
                response.Headers.Location = new Uri(link);
                return response;
            }
        }

        // DELETE api/Conversation/5
        //[ValidateHttpAntiForgeryToken]
        public HttpResponseMessage DeleteConversation(int id)
        {
            using (ConversationContext db = new ConversationContext())
            {
                // ConversationContext.NonLazyLoadingInstance;
                User loginUser = GetLoginUser(db);
                if (loginUser == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }
                Conversation conversation = db.Conversations.Find(id);
                if (conversation == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                if (!UserManager.ConversationHasLoginUser(this, db, db.Entry(conversation).Entity))
                {
                    // Trying to delete a record that does not belong to the user
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                //List<ConversationUser> removedConversationUsers = new List<ConversationUser>(conversation.ConversationUsers);
                // Remove all of the conversationUsers that reference the conversation.
                List<ConversationUser> removedConversationUsers =
                    new List<ConversationUser>(
                        db.ConversationUsers.Where(
                            c =>
                            c.ConversationId == conversation.ConversationId &&
                            (c.UserId == loginUser.UserId)));
                removedConversationUsers.ForEach(c => conversation.ConversationUsers.Remove(c));
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                }
                List<ConversationUser> otherConversationUsers =
                    new List<ConversationUser>(
                        db.ConversationUsers.Where(
                            c =>
                            c.ConversationId == conversation.ConversationId &&
                            (c.UserId != loginUser.UserId)));
                if (otherConversationUsers.Count == 0)
                {
                    // Remove all of the messages that reference the conversation.
                    //List<Message> removedMessages = new List<Message>(conversation.Messages);
                    List<Message> removedMessages =
                        conversation.Messages.Select(m => db.Messages.Find(m.MessageId)).ToList();
                    //removedMessages.ForEach(m => conversation.Messages.Remove(m));
                    removedMessages.ForEach(m => db.Messages.Remove(m));
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException e)
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                    }
                    ////if already loaded in existing DBContext then use Set().Remove(entity) to delete it.
                    //db.Set<Conversation>().Remove(conversation);
                    ////Also, you can mark an entity as deleted
                    //db.Entry(conversation).State = EntityState.Deleted;
                    ////if not loaded in existing DBContext then use following.
                    db.Conversations.Remove(conversation);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException e)
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                    }
                }
                ConversationDto conversationDto = new ConversationDto(conversation, loginUser);
                return Request.CreateResponse(HttpStatusCode.OK, conversationDto);
            }
        }

        private User GetLoginUser(ConversationContext db)
        {
            User loginUser = UserManager.GetLoginUser(this, db);
            return loginUser;
        }
    }
}