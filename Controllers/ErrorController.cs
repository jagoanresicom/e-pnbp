using System.Web.Mvc;

namespace Pnbp.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Forbidden()
        {
            return View();
        }

        public ActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            return View();
        }

        public ActionResult InternalServerError()
        {
            return View();
        }
    }
}