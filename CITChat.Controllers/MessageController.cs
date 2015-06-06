using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CITChat.Controllers.Contexts;
using CITChat.Controllers.DataTransferObjects;
using CITChat.Models;
using CITChat.Translators;

namespace CITChat.Controllers
{
    //[Authorize]
    //[ValidateHttpAntiForgeryToken]
    public class MessageController : ApiController
    {
        // POST api/Message
        /// <summary>
        /// </summary>
        /// <param name="messageDto"></param>
        /// <returns></returns>
        public HttpResponseMessage PostMessage(MessageDto messageDto)
        {
            UpdateMessageDto(messageDto);
            using (ConversationContext db = new ConversationContext())
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                Conversation conversation = db.Conversations.Find(messageDto.ConversationId);
                if (conversation == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                if (!ConversationHasLoginUser(db, conversation))
                {
                    // Trying to add a record that does not belong to the user
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Message message = messageDto.ToEntity();
                conversation.Messages.Add(message);
                // Need to detach to avoid loop reference exception during JSON serialization
                db.Entry(conversation).State = EntityState.Detached;
                db.Entry(conversation).State = EntityState.Modified;
                db.Messages.Add(message);
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                }
                messageDto.MessageId = message.MessageId;
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, messageDto);
                // ReSharper disable AssignNullToNotNullAttribute
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new {id = messageDto.MessageId}));
                // ReSharper restore AssignNullToNotNullAttribute
                return response;
            }
        }

        // PUT api/Message/5
        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="messageDto"></param>
        /// <returns></returns>
        public HttpResponseMessage PutMessage(int id, MessageDto messageDto)
        {
            UpdateMessageDto(messageDto);
            using (ConversationContext db = new ConversationContext())
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                if (id != messageDto.MessageId)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                Conversation conversation = db.Conversations.Find(messageDto.ConversationId);
                if (conversation == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                if (!UserManager.ConversationHasLoginUser(this, db, conversation))
                {
                    // Trying to modify a record that does not belong to the user
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Message message =
                    conversation.Messages.FirstOrDefault(
                        m => (m.ConversationId == messageDto.ConversationId) && (m.MessageId == messageDto.MessageId));
                if (message == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                messageDto.UpdateEntity(message);
                // Need to detach to avoid duplicate primary key exception when SaveChanges is called
                db.Entry(conversation).State = EntityState.Detached;
                db.Entry(message).State = EntityState.Modified;
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

        // DELETE api/Message/5
        public HttpResponseMessage DeleteMessage(int id)
        {
            using (ConversationContext db = new ConversationContext())
            {
                Message message = db.Messages.Find(id);
                if (message == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                Conversation conversation = db.Conversations.Find(message.ConversationId);
                if (conversation == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                if (!ConversationHasLoginUser(db, db.Entry(conversation).Entity))
                {
                    // Trying to delete a record that does not belong to the user
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                MessageDto messageDto = new MessageDto(message);
                db.Messages.Remove(message);
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                }
                return Request.CreateResponse(HttpStatusCode.OK, messageDto);
            }
        }

        private static void UpdateMessageDto(MessageDto messageDto)
        {
            IContentTranslator contentTranslator = ContentTranslatorHelper.ContentTranslator;
            messageDto.TranslatedContent = contentTranslator.TranslateContent(messageDto.Content);
        }

        private bool ConversationHasLoginUser(ConversationContext db, Conversation conversation)
        {
            bool conversationHasLoginUser = UserManager.ConversationHasLoginUser(this, db, conversation);
            return conversationHasLoginUser;
        }
    }
}