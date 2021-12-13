using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Text;
using System.IO;

namespace Pnbp.Controllers
{
    [AccessDeniedAuthorize]
    public class HomeController : Controller
    {
        public ActionResult Index(string pTahun, string pSatker)
        {
            // Models.HomeModel brqm = new Models.HomeModel();
            //var result = brqm.totalPenerimaan();
            // Entities.JumlahPenerimaanOperasional rec = result.FirstOrDefault<Entities.JumlahPenerimaanOperasional>();
            // ViewData["result"] = result;

            var ctx = new PnbpContext();

            string currentYear = DateTime.Now.Year.ToString();

            var total_penerimaan = ctx.Database.SqlQuery<Entities.TotalPenerimaan>("SELECT SUM (jumlah) as jumlah FROM rekappenerimaandetail where tahun = " + currentYear + " ").FirstOrDefault();
            total_penerimaan.jumlah = (total_penerimaan.jumlah == null ? 0 : total_penerimaan.jumlah);
            ViewData["total_penerimaan"] = total_penerimaan;

            var total_belanja = ctx.Database.SqlQuery<Entities.TotalBelanja>("SELECT SUM (Amount) as jumlah FROM SPAN_BELANJA where SUMBER_DANA = 'D' and KDSATKER != '524465' ").FirstOrDefault();
            total_belanja.jumlah = (total_belanja.jumlah == null ? 0 : total_belanja.jumlah);
            ViewData["total_belanja"] = total_belanja;

            var persentase_penerimaan = ctx.Database.SqlQuery<Entities.TotalPenerimaan>("SELECT ROUND ( sum(JUMLAH) / 2414400030000, 3 ) * 100 AS persentase FROM rekappenerimaandetail where tahun = " + currentYear + " ").FirstOrDefault();
            persentase_penerimaan.persentase = (persentase_penerimaan.persentase == null ? 0 : persentase_penerimaan.persentase);
            ViewData["persentase_penerimaan"] = persentase_penerimaan;

            var total_realisasi = ctx.Database.SqlQuery<Entities.TotalBelanja>("SELECT SUM (Amount) as jumlah FROM SPAN_REALISASI where SUMBERDANA = 'D' and KDSATKER != '524465' ").FirstOrDefault();
            total_realisasi.jumlah = (total_realisasi.jumlah == null ? 0 : total_realisasi.jumlah);
            ViewData["datarealisasi"] = total_realisasi;

            var persentase_belanja = (total_realisasi.jumlah == 0 && total_belanja.jumlah == 0) ? 0 : (total_realisasi.jumlah / total_belanja.jumlah * 100);
            ViewData["persentase_belanja"] = persentase_belanja;

            var get_mp = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM (TERALOKASI),0) AS TERALOKASI FROM REKAPALOKASI WHERE STATUSALOKASI = 1 AND TAHUN = " + currentYear + " ").ToList();
            ViewData["datamp"] = get_mp;

            var get_pagu = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM(NILAIANGGARAN),0) FROM MANFAAT WHERE TAHUN = " + currentYear + "").ToList();
            ViewData["datapagu"] = get_pagu;

            var getmpops = ctx.Database.SqlQuery<Decimal>("select NVL(sum(TERALOKASI), 0) AS MP from REKAPALOKASI where tahun = " + currentYear+" AND TIPEMANFAAT = 'OPS'").ToList();
            ViewData["getmpops"] = getmpops;

            var getbelanjaops = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM (a.amount), 0) AS REALISASI FROM SPAN_REALISASI a LEFT JOIN KODESPAN b ON a.KEGIATAN = b.KODE AND a.OUTPUT = b.KEGIATAN WHERE b.TIPE = 'OPS' AND a. SUMBERDANA = 'D' AND SUBSTR(TANGGAL, 8, 2) = 21").ToList();
            ViewData["getbelanjaops"] = getbelanjaops;

            var getmpnonops = ctx.Database.SqlQuery<Decimal>("select NVL(sum(TERALOKASI), 0) AS MP from REKAPALOKASI where tahun = " + currentYear+" AND TIPEMANFAAT = 'NONOPS'").ToList();
            ViewData["getmpnonops"] = getmpnonops;

            var getbelanjanonops = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM (a.amount), 0) AS REALISASI FROM SPAN_REALISASI a LEFT JOIN KODESPAN b ON a.KEGIATAN = b.KODE AND a.OUTPUT = b.KEGIATAN WHERE b.TIPE = 'NONOPS' AND a. SUMBERDANA = 'D' AND SUBSTR(TANGGAL, 8, 2) = 21").ToList();
            ViewData["getbelanjanonops"] = getbelanjanonops;

            Entities.CharDashboard dsboard = new Entities.CharDashboard();


            List<Entities.AlokasiBelanja> lsAlokasi = Pnbp.Models.HomeModel.dtAlokasiBelanjaNonOps(pTahun, pSatker);

            List<Entities.AlokasiBelanja> lsAlokasiOps = Pnbp.Models.HomeModel.dtAlokasiBelanjaOps(pTahun, pSatker);
            dsboard.jumlahops = lsAlokasiOps.Select(v => (decimal)v.jumlahops).ToList();
            dsboard.bulanAlokOps = lsAlokasiOps.Select(v => (decimal)v.bulanAlokOps).ToList();


            dsboard.jumlah = lsAlokasi.Select(v => (decimal)v.jumlah).ToList();
            dsboard.bulan = lsAlokasi.Select(v => (decimal)v.bulan).ToList();
            dsboard.tahun = pTahun;
            dsboard.lstahun = Pnbp.Models.HomeModel.lsTahunPenerimaan();
            var result = dsboard.jumlah;
            var bulan = dsboard.bulan;
            ViewBag.RESULT = dsboard.jumlah.ToList();
            ViewBag.BULAN = dsboard.bulan.ToList();

            ViewBag.RESULTALOKOPS = dsboard.jumlahops.ToList();
            ViewBag.BULANALOKOPS = dsboard.bulanAlokOps.ToList();


            //Get Belanja OPS (Line Chart)
            List<Entities.BelanjaOps> lsBelanjaOps = Pnbp.Models.HomeModel.dtBelanjaOps(pTahun, pSatker);
            dsboard.belanjaOps = lsBelanjaOps.Select(v => (decimal)v.belanjaOps).ToList();
            dsboard.bulanOps = lsBelanjaOps.Select(v => (string)v.bulanOps).ToList();
            //return Json(new { result = dsboard.bulanOps }, JsonRequestBehavior.AllowGet);
            dsboard.tahun = pTahun;
            var belanjaOps = dsboard.belanjaOps;
            var bulanOps = dsboard.bulanOps;
            ViewBag.belanjaOps = dsboard.belanjaOps.ToList();
            ViewBag.bulanOps = dsboard.bulanOps.ToList();

            //Get Belanja NonOps (Line Chart)
            List<Entities.BelanjaNonOps> lsBelanjaNonOps = Pnbp.Models.HomeModel.dtBelanjaNonOps(pTahun, pSatker);
            dsboard.belanjaNonOps = lsBelanjaNonOps.Select(v => (decimal)v.belanjaNonOps).ToList();
            dsboard.bulanNonOps = lsBelanjaNonOps.Select(v => (string)v.bulanNonOps).ToList();
            //return Json(new { result = dsboard.bulan }, JsonRequestBehavior.AllowGet);
            dsboard.tahun = pTahun;
            var belanjaNonOps = dsboard.belanjaNonOps;
            ViewBag.belanjaNonOps = dsboard.belanjaNonOps.ToList();
            ViewBag.bulanNonOps = dsboard.bulanNonOps.ToList();

            //return Json(persentase_belanja, JsonRequestBehavior.AllowGet);
            return View();
        }

        public ActionResult IndexPartial(string pTahun, string pSatker)
        {
            var ctx = new PnbpContext();

            string currentYear = DateTime.Now.Year.ToString();

            var total_penerimaan = ctx.Database.SqlQuery<Entities.TotalPenerimaan>("SELECT SUM (jumlah) as jumlah FROM rekappenerimaandetail where tahun = " + currentYear + " ").FirstOrDefault();
            ViewData["total_penerimaan"] = total_penerimaan;
            return View("PartialDashboard");
        }

        public ActionResult pagu_mp()
        {
            var ctx = new PnbpContext();
            var get_data = ctx.Database.SqlQuery<Entities.TotalPenerimaan>("SELECT DISTINCT SUM(TOTALALOKASI) AS TOTALALOKASI from MANFAAT WHERE TOTALALOKASI IS NOT NULL").FirstOrDefault();
            return Json(get_data, JsonRequestBehavior.AllowGet);
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

        public ActionResult total_penerimaan(Entities.CharDashboard query, int? start, int? length)
        {
            //string pKantorId = String.IsNullOrEmpty(query.selectedKabupaten) ? !String.IsNullOrEmpty(query.pKantorId) ? query.pKantorId : "" : query.selectedKabupaten;
            Models.HomeModel brqm = new Models.HomeModel();
            List<Entities.JumlahPenerimaanOperasional> result = brqm.totalPenerimaan();
            return Json(result, JsonRequestBehavior.AllowGet);
            //Entities.JumlahPenerimaanOperasional rec = result.FirstOrDefault<Entities.JumlahPenerimaanOperasional>();

            //return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

    }
}
