using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Pnbp.Controllers
{
    public class KontenController : Controller
    {
        public ActionResult GetPdf(string filename)
        {
            var path = Server.MapPath(@"~/Contents/" + filename);

            var fileStream = new FileStream(path,
                                             FileMode.Open,
                                             FileAccess.Read
                                           );
            var fsResult = new FileStreamResult(fileStream, "application/pdf");
            return fsResult;
        }

        public ActionResult DocViewer()
        {
            return PartialView("DocViewer");
        }
    }
}