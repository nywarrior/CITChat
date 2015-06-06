using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using CITChat.Controllers.Contexts;
using CITChat.Controllers.DataTransferObjects;
using CITChat.Models;

namespace CITChat.Controllers
{
    //[Authorize]
    public class UserController : ApiController
    {
        // GET api/User
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserDto> GetAllUsers()
        {
            using (ConversationContext db = new ConversationContext())
            {
                //User loginUser = GetLoginUser(db);
                //if (loginUser == null)
                //{
                //    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
                //}
                List<User> users = db.Users.ToList();
                List<UserDto> userDtos =
                    users.Select(user => new UserDto(user)).ToList();
                return userDtos;
            }
        }

        //// GET api/User/5
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public UserDto GetUser(int id)
        //{
        //    using (ConversationContext db = new ConversationContext())
        //    {
        //        User loginUser = UserManager.GetLoginUser(this, db);
        //        if (loginUser == null)
        //        {
        //            throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        //        }
        //        User user = db.Users.Find(id);
        //        if (user == null)
        //        {
        //            throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
        //        }
        //        return new UserDto(user);
        //    }
        //}

        private User GetLoginUser(ConversationContext db)
        {
            User loginUser = UserManager.GetLoginUser(this, db);
            return loginUser;
        }
    }
}