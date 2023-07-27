using Microsoft.AspNetCore.Mvc;

namespace MPP.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult UnauthorizedAccess() 
        {
            return View("~/Views/Shared/UnauthorizedAccess.cshtml");
        }
        public ActionResult NotFound()
        {
            return View("~/Views/Shared/FileNotFound.cshtml");
        }
        public ActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }        
    }
}
