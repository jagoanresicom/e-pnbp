using Pnbp.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Pnbp.Controllers
{
    public class TipeManfaatController : Controller
    {
        // GET: KodeSpan
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetAll()
        {
            List<Entities.TipeManfaat> data;
            bool success = false;
            string msg;

            var tmm = new TipeManfaatModel();
            data = tmm.getAll();
            
            if (data == null || data.Count == 0)
            {
                msg = "Data tidak ditemukan";
            } 
            else
            {
                success = true;
                msg = "Berhasil";
            }

            return Json(new { success = success, data = data, message = msg }, JsonRequestBehavior.AllowGet);
        }
    }
}