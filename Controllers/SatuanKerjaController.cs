using ClosedXML.Excel;
using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using Xceed.Words.NET;
using Pnbp.Entities;
using Pnbp.Models;

namespace Pnbp.Controllers
{
    public class SatuanKerjaController : Controller
    {

        public ActionResult ListSatker()
        {
            SatuanKerjaModel mdl = new SatuanKerjaModel();
            List<SatuanKerja> data = mdl.ListSatuanKerja();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

    }
}