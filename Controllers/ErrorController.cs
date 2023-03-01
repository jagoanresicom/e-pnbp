using System.Web.Mvc;

namespace Pnbp.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return View();
        }

        public ActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            return View();
        }

        public ActionResult InternalServerError()
        {
            Response.StatusCode = 500;
            return View();
        }
    }
}