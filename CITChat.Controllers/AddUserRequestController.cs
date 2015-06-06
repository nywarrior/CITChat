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

namespace CITChat.Controllers
{
    //[Authorize]
    //[ValidateHttpAntiForgeryToken]
    public class AddUserRequestController : ApiController
    {
        // POST api/AddUserRequest
        /// <summary>
        /// </summary>
        /// <param name="addUserRequestDto"></param>
        /// <returns></returns>
        public HttpResponseMessage PostAddUserRequest(AddUserRequestDto addUserRequestDto)
        {
            using (ConversationContext db = new ConversationContext())
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                Conversation conversation = db.Conversations.Find(addUserRequestDto.ConversationId);
                if (conversation == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                User user = db.Users.Find(addUserRequestDto.UserId);
                if (user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                if (!ConversationHasLoginUser(db, conversation))
                {
                    // Trying to add a record that does not belong to the user
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                ConversationUser conversationUser =
                    db.ConversationUsers.FirstOrDefault(
                        c => (c.ConversationId == conversation.ConversationId) && (c.UserId == addUserRequestDto.UserId));
                if (conversationUser != null)
                {
                    // Trying to add a record that does not belong to the user
                    return Request.CreateResponse(HttpStatusCode.NotModified);
                }
                conversationUser = new ConversationUser
                    {
                        ConversationId = conversation.ConversationId,
                        UserId = addUserRequestDto.UserId
                    };
                db.ConversationUsers.Add(conversationUser);
                // Need to detach to avoid loop reference exception during JSON serialization
                db.Entry(conversation).State = EntityState.Detached;
                db.Entry(conversation).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                }
                addUserRequestDto.UserName = user.UserName;
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, addUserRequestDto);
                // ReSharper disable AssignNullToNotNullAttribute
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new {id = addUserRequestDto.UserId}));
                // ReSharper restore AssignNullToNotNullAttribute
                return response;
            }
        }

        private bool ConversationHasLoginUser(ConversationContext db, Conversation conversation)
        {
            bool conversationHasLoginUser = UserManager.ConversationHasLoginUser(this, db, conversation);
            return conversationHasLoginUser;
        }
    }
}