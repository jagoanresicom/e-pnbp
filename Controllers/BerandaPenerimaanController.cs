using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pnbp.Controllers
{
    [AccessDeniedAuthorize]
    public class BerandaPenerimaanController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult IndexPartial(string pTahun, string pSatker)
        {
            Entities.CharDashboard dsboard = new Entities.CharDashboard();
            
            pTahun =  !String.IsNullOrEmpty(pTahun) ? pTahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            List<Entities.RekapPenerimaan> lsRekapPenerimaan = Pnbp.Models.HomeModel.dtRekapPenerimaan(pTahun, pSatker);
            List<Entities.RekapAlokasi> lsAlokasiOPS = Pnbp.Models.HomeModel.dtRekapAlokasi(pTahun, "OPS");
            List<Entities.RekapAlokasi> lsAlokasiNONOPS = Pnbp.Models.HomeModel.dtRekapAlokasi(pTahun, "NONOPS");
            List<Entities.RekapAlokasi> lsMP = Pnbp.Models.HomeModel.dtRekapAlokasi(pTahun, "");
            dsboard.Penerimaan = lsRekapPenerimaan.Select(v => (decimal)v.penerimaan).ToList();
            dsboard.Operasional = lsRekapPenerimaan.Select(v => (decimal)v.operasional).ToList();
            dsboard.OPS = lsAlokasiOPS.Select(v => (decimal)v.alokasi).ToList();
            dsboard.NONOPS = lsAlokasiNONOPS.Select(v => (decimal)v.alokasi).ToList();
            dsboard.MP = lsMP.Select(v => (decimal)v.alokasi).ToList();
            dsboard.tahun = pTahun;
            dsboard.lstahun = Pnbp.Models.HomeModel.lsTahunPenerimaan();
            return PartialView("Index",dsboard);
        }

        public JsonResult getPenerimaan(string pTahun, string pSatker)
        {
            var result = new Entities.CharDashboard() { Penerimaan = new List<decimal>(), Operasional = new List<decimal>() };
            List<Entities.RekapPenerimaan> lsRekapPenerimaan = Pnbp.Models.HomeModel.dtRekapPenerimaan(pTahun, pSatker);
            List<Entities.RekapAlokasi> lsAlokasiOPS = Pnbp.Models.HomeModel.dtRekapAlokasi(pTahun, "OPS");
            List<Entities.RekapAlokasi> lsAlokasiNONOPS = Pnbp.Models.HomeModel.dtRekapAlokasi(pTahun, "NONOPS");
            List<Entities.RekapAlokasi> lsMP = Pnbp.Models.HomeModel.dtRekapAlokasi(pTahun, "");
            result.Penerimaan = lsRekapPenerimaan.Select(v => (decimal)v.penerimaan).ToList();
            result.Operasional = lsRekapPenerimaan.Select(v => (decimal)v.operasional).ToList();
            result.OPS = lsAlokasiOPS.Select(v => (decimal)v.alokasi).ToList();
            result.NONOPS = lsAlokasiNONOPS.Select(v => (decimal)v.alokasi).ToList();
            result.MP = lsMP.Select(v => (decimal)v.alokasi).ToList();
            result.tahun = pTahun;
            result.lstahun = Pnbp.Models.HomeModel.lsTahunPenerimaan();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
