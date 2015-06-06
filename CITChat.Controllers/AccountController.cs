using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Security;
using CITChat.Controllers.Contexts;
using CITChat.Filters;
using CITChat.Models;
using CITChat.Translators;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;

namespace CITChat.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        private const int NumberOfInitialMessages = 2;

        //
        // POST: /Account/JsonLogin

        [AllowAnonymous]
        [HttpPost]
        public JsonResult JsonLogin(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    return Json(new {success = true, redirect = returnUrl});
                }
                ModelState.AddModelError("", "The user name or password provided is incorrect.");
            }

            // If we got this far, something failed
            return Json(new {errors = GetErrorsFromModelState()});
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();
            //if (Request.UrlReferrer != null)
            //{
            //    string url = Request.UrlReferrer.AbsoluteUri;
            //    CITChatHubHelper.SendLogoff(url);
            //}
            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/JsonRegister
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult JsonRegister(RegisterModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                    WebSecurity.Login(model.UserName, model.Password);
                    InitiateDatabaseForNewUser(model.UserName);
                    FormsAuthentication.SetAuthCookie(model.UserName, createPersistentCookie: false);
                    return Json(new {success = true, redirect = returnUrl});
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed
            return Json(new {errors = GetErrorsFromModelState()});
        }

        /// <summary>
        ///     Initiate a new conversation for new user
        /// </summary>
        /// <param name="userName"></param>
        private void InitiateDatabaseForNewUser(string userName)
        {
            using (ConversationContext db = new ConversationContext())
            {
                Conversation conversation = new Conversation();
                //if (conversation.Users == null)
                //{
                //    conversation.Users = new List<User>();
                //}
                User user = UserManager.FindUserWithUserName(db, userName);
                if (user == null)
                {
                    user = new User
                        {
                            UserName = userName,
                            //ConversationId = conversation.ConversationId
                            //Conversations = new List<Conversation>(),
                            ConversationUsers = new List<ConversationUser>(),
                        };
                    db.Users.Add(user);
                }
#if false
                HttpRequestMessage httpRequestMessage = Request;
                if (httpRequestMessage != null)
                {
                    UserManager.SetLoginUser(httpRequestMessage, user);
                }
#endif
                //User conversationUser = conversation.FindUserWithUserId(user.UserId);
                //if (conversationUser == null)
                //{
                //    conversationUser = user;
                //    conversation.Users.Add(conversationUser);
                //}
                conversation.Title = "Conversation #1";
                conversation.StartDateTime = DateTime.Now;
                conversation.StartDateTimeDisplayString = conversation.StartDateTime.ToShortDateString() + " " +
                                                          conversation.StartDateTime.ToShortTimeString();
                if (conversation.Messages == null)
                {
                    conversation.Messages = new List<Message>();
                }
                db.Conversations.Add(conversation);
                ConversationUser conversationUsers = new ConversationUser
                    {
                        //UserId = conversationUser.UserId,
                        ConversationId = conversation.ConversationId,
                        Conversation = conversation,
                        //User = conversationUser
                    };
                user.ConversationUsers.Add(conversationUsers);
                db.ConversationUsers.Add(conversationUsers);
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                }
                IContentTranslator contentTranslator = ContentTranslatorHelper.ContentTranslator;
                for (int i = 1; i <= NumberOfInitialMessages; i++)
                {
                    string content = "Message #" + i;
                    Message message = new Message
                        {
                            Content = content,
                            ConversationId = conversation.ConversationId,
                            IsDone = false,
                            TranslatedContent = contentTranslator.TranslateContent(content)
                        };
                    conversation.Messages.Add(message);
                    db.Messages.Add(message);
                }
                db.SaveChanges();
            }
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            string loginUserName = User.Identity.Name;
            if (ownerAccount == loginUserName)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (
                    var scope = new TransactionScope
                        (TransactionScopeOption.Required,
                         new TransactionOptions
                             {
                                 IsolationLevel = IsolationLevel.Serializable
                             }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(loginUserName));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(loginUserName).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new {Message = message});
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess
                    ? "Your password has been changed."
                    : message == ManageMessageId.SetPasswordSuccess
                          ? "Your password has been set."
                          : message == ManageMessageId.RemoveLoginSuccess
                                ? "The external login was removed."
                                : "";
            string loginUserName = User.Identity.Name;
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(loginUserName));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            string loginUserName = User.Identity.Name;
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(loginUserName));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(loginUserName, model.OldPassword,
                                                                             model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new {Message = ManageMessageId.ChangePasswordSuccess});
                    }
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(loginUserName, model.NewPassword);
                        return RedirectToAction("Manage", new {Message = ManageMessageId.SetPasswordSuccess});
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new {ReturnUrl = returnUrl}));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result =
                OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new {ReturnUrl = returnUrl}));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }
            string loginUserName = User.Identity.Name;
            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, loginUserName);
                return RedirectToLocal(returnUrl);
            }
            // User is new, ask for their desired membership name
            string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
#if false
            using (ConversationContext db = new ConversationContext())
            {
                User user = UserManager.FindUserWithUserName(db, userName);
                HttpRequestMessage httpRequestMessage = Request;
                if (httpRequestMessage != null)
                {
                    UserManager.SetLoginUser(httpRequestMessage, user);
                }
            }
#endif
            ViewBag.ReturnUrl = returnUrl;
            return View("ExternalLoginConfirmation",
                        new RegisterExternalLoginModel {UserName = result.UserName, ExternalLoginData = loginData});
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider;
            string providerUserId;
            if (User.Identity.IsAuthenticated ||
                !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }
            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user =
                        db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile {UserName = model.UserName});
                        db.SaveChanges();
                        InitiateDatabaseForNewUser(model.UserName);
                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    ModelState.AddModelError("UserName",
                                             "User name already exists. Please enter a different user name.");
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            string loginUserName = User.Identity.Name;
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(loginUserName);
            List<ExternalLogin> externalLogins = (from account in accounts
                                                  let clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider)
                                                  select new ExternalLogin
                                                      {
                                                          Provider = account.Provider,
                                                          ProviderDisplayName = clientData.DisplayName,
                                                          ProviderUserId = account.ProviderUserId,
                                                      }).ToList();

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 ||
                                       OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(loginUserName));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return
                        "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return
                        "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return
                        "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return
                        "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        #endregion
    }
}