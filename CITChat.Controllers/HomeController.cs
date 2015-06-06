using System.Web.Mvc;

namespace CITChat.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public ActionResult Chat()
        {
            return View();
        }
    }
}