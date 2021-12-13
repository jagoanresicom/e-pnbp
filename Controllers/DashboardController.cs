using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pnbp.Controllers
{
    [AccessDeniedAuthorize]
    public class DashboardController : Controller
    {
        public ActionResult StatistikPenerimaan()
        {
            return View();
        }

        public ActionResult StatistikNTPN()
        {
            return View();
        }

        public ActionResult DetilPenerimaan()
        {
            return View();
        }

        public ActionResult DetilAlokasiManfaat()
        {
            return View();
        }

        public ActionResult RekapPaguAlokasi()
        {
            return View();
        }

        public ActionResult RekapPenerimaan()
        {
            return View();
        }

        public ActionResult RekapManfaatPerKantor()
        {
            return View();
        }

        public ActionResult RekapManfaatPerProgram()
        {
            return View();
        }
    }
}