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

            //var ctx = new PnbpContext();

            //string currentYear = DateTime.Now.Year.ToString();

            //var total_penerimaan = ctx.Database.SqlQuery<Entities.TotalPenerimaan>("SELECT SUM (jumlah) as jumlah FROM rekappenerimaandetail where tahun = " + currentYear + " ").FirstOrDefault();
            //total_penerimaan.jumlah = (total_penerimaan.jumlah == null ? 0 : total_penerimaan.jumlah);
            //ViewData["total_penerimaan"] = total_penerimaan;

            //var total_belanja = ctx.Database.SqlQuery<Entities.TotalBelanja>("SELECT SUM (Amount) as jumlah FROM SPAN_BELANJA where SUMBER_DANA = 'D' and KDSATKER != '524465' ").FirstOrDefault();
            //total_belanja.jumlah = (total_belanja.jumlah == null ? 0 : total_belanja.jumlah);
            //ViewData["total_belanja"] = total_belanja;

            //var persentase_penerimaan = ctx.Database.SqlQuery<Entities.TotalPenerimaan>("SELECT ROUND ( sum(JUMLAH) / 2414400030000, 3 ) * 100 AS persentase FROM rekappenerimaandetail where tahun = " + currentYear + " ").FirstOrDefault();
            //persentase_penerimaan.persentase = (persentase_penerimaan.persentase == null ? 0 : persentase_penerimaan.persentase);
            //ViewData["persentase_penerimaan"] = persentase_penerimaan;

            //var total_realisasi = ctx.Database.SqlQuery<Entities.TotalBelanja>("SELECT SUM (Amount) as jumlah FROM SPAN_REALISASI where SUMBERDANA = 'D' and KDSATKER != '524465' ").FirstOrDefault();
            //total_realisasi.jumlah = (total_realisasi.jumlah == null ? 0 : total_realisasi.jumlah);
            //ViewData["datarealisasi"] = total_realisasi;

            //var persentase_belanja = (total_realisasi.jumlah == 0 && total_belanja.jumlah == 0) ? 0 : (total_realisasi.jumlah / total_belanja.jumlah * 100);
            //ViewData["persentase_belanja"] = persentase_belanja;

            //var get_mp = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM (TERALOKASI),0) AS TERALOKASI FROM REKAPALOKASI WHERE STATUSALOKASI = 1 AND TAHUN = " + currentYear + " ").ToList();
            //ViewData["datamp"] = get_mp;

            //var get_pagu = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM(NILAIANGGARAN),0) FROM MANFAAT WHERE TAHUN = " + currentYear + "").ToList();
            //ViewData["datapagu"] = get_pagu;

            //var getmpops = ctx.Database.SqlQuery<Decimal>("select NVL(sum(TERALOKASI), 0) AS MP from REKAPALOKASI where tahun = " + currentYear+" AND TIPEMANFAAT = 'OPS'").ToList();
            //ViewData["getmpops"] = getmpops;

            //var getbelanjaops = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM (a.amount), 0) AS REALISASI FROM SPAN_REALISASI a LEFT JOIN KODESPAN b ON a.KEGIATAN = b.KODE AND a.OUTPUT = b.KEGIATAN WHERE b.TIPE = 'OPS' AND a. SUMBERDANA = 'D' AND SUBSTR(TANGGAL, 8, 2) = 21").ToList();
            //ViewData["getbelanjaops"] = getbelanjaops;

            //var getmpnonops = ctx.Database.SqlQuery<Decimal>("select NVL(sum(TERALOKASI), 0) AS MP from REKAPALOKASI where tahun = " + currentYear+" AND TIPEMANFAAT = 'NONOPS'").ToList();
            //ViewData["getmpnonops"] = getmpnonops;

            //var getbelanjanonops = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM (a.amount), 0) AS REALISASI FROM SPAN_REALISASI a LEFT JOIN KODESPAN b ON a.KEGIATAN = b.KODE AND a.OUTPUT = b.KEGIATAN WHERE b.TIPE = 'NONOPS' AND a. SUMBERDANA = 'D' AND SUBSTR(TANGGAL, 8, 2) = 21").ToList();
            //ViewData["getbelanjanonops"] = getbelanjanonops;

            //Entities.CharDashboard dsboard = new Entities.CharDashboard();


            //List<Entities.AlokasiBelanja> lsAlokasi = Pnbp.Models.HomeModel.dtAlokasiBelanjaNonOps(pTahun, pSatker);

            //List<Entities.AlokasiBelanja> lsAlokasiOps = Pnbp.Models.HomeModel.dtAlokasiBelanjaOps(pTahun, pSatker);
            //dsboard.jumlahops = lsAlokasiOps.Select(v => (decimal)v.jumlahops).ToList();
            //dsboard.bulanAlokOps = lsAlokasiOps.Select(v => (decimal)v.bulanAlokOps).ToList();


            //dsboard.jumlah = lsAlokasi.Select(v => (decimal)v.jumlah).ToList();
            //dsboard.bulan = lsAlokasi.Select(v => (decimal)v.bulan).ToList();
            //dsboard.tahun = pTahun;
            //dsboard.lstahun = Pnbp.Models.HomeModel.lsTahunPenerimaan();
            //var result = dsboard.jumlah;
            //var bulan = dsboard.bulan;
            //ViewBag.RESULT = dsboard.jumlah.ToList();
            //ViewBag.BULAN = dsboard.bulan.ToList();

            //ViewBag.RESULTALOKOPS = dsboard.jumlahops.ToList();
            //ViewBag.BULANALOKOPS = dsboard.bulanAlokOps.ToList();


            ////Get Belanja OPS (Line Chart)
            //List<Entities.BelanjaOps> lsBelanjaOps = Pnbp.Models.HomeModel.dtBelanjaOps(pTahun, pSatker);
            //dsboard.belanjaOps = lsBelanjaOps.Select(v => (decimal)v.belanjaOps).ToList();
            //dsboard.bulanOps = lsBelanjaOps.Select(v => (string)v.bulanOps).ToList();
            ////return Json(new { result = dsboard.bulanOps }, JsonRequestBehavior.AllowGet);
            //dsboard.tahun = pTahun;
            //var belanjaOps = dsboard.belanjaOps;
            //var bulanOps = dsboard.bulanOps;
            //ViewBag.belanjaOps = dsboard.belanjaOps.ToList();
            //ViewBag.bulanOps = dsboard.bulanOps.ToList();

            ////Get Belanja NonOps (Line Chart)
            //List<Entities.BelanjaNonOps> lsBelanjaNonOps = Pnbp.Models.HomeModel.dtBelanjaNonOps(pTahun, pSatker);
            //dsboard.belanjaNonOps = lsBelanjaNonOps.Select(v => (decimal)v.belanjaNonOps).ToList();
            //dsboard.bulanNonOps = lsBelanjaNonOps.Select(v => (string)v.bulanNonOps).ToList();
            ////return Json(new { result = dsboard.bulan }, JsonRequestBehavior.AllowGet);
            //dsboard.tahun = pTahun;
            //var belanjaNonOps = dsboard.belanjaNonOps;
            //ViewBag.belanjaNonOps = dsboard.belanjaNonOps.ToList();
            //ViewBag.bulanNonOps = dsboard.bulanNonOps.ToList();

            //return Json(persentase_belanja, JsonRequestBehavior.AllowGet);
            return View();
        }

        public JsonResult GetTotalPenerimaan()
        {
            var ctx = new PnbpContext();
            string currentYear = DateTime.Now.Year.ToString();

            //var query = $@"
            //            SELECT 
            //                SUM (jumlah) as jumlah 
            //            FROM rekappenerimaandetail 
            //            where tahun = {currentYear}";
            var query = $@"
                        SELECT 
                            TO_NUMBER(VALUE) as jumlah 
                        FROM DASHBOARD_SUMMARY  
                        where kode = 'TOTAL_PENERIMAAN'";
            var total_penerimaan = ctx
                .Database
                .SqlQuery<Entities.TotalPenerimaan>(query)
                .FirstOrDefault();
            total_penerimaan.jumlah = (total_penerimaan.jumlah == null ? 0 : total_penerimaan.jumlah);
            var totalPenerimaan = total_penerimaan.jumlah?.ToString("N0", new System.Globalization.CultureInfo("id-ID"));

            return Json(new { totalPenerimaan }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTotalBelanja()
        {
            var ctx = new PnbpContext();
            string currentYear = DateTime.Now.Year.ToString();
            var total_belanja = ctx
                .Database
                .SqlQuery<Entities.TotalBelanja>(
                //$@"
                //    SELECT SUM (Amount) as jumlah 
                //    FROM SPAN_REALISASI 
                //    WHERE SUMBERDANA = 'D' and KDSATKER != '524465' AND TAHUN = {currentYear}"
                    $@" 
                        SELECT 
                            TO_NUMBER(VALUE) as jumlah 
                        FROM DASHBOARD_SUMMARY  
                        WHERE kode = 'TOTAL_BELANJA'"
                    )
                .FirstOrDefault();
            total_belanja.jumlah = (total_belanja.jumlah == null ? 0 : total_belanja.jumlah);
            var totalBelanja = total_belanja.jumlah?.ToString("N0", new System.Globalization.CultureInfo("id-ID"));

            return Json(new { totalBelanja }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPersentasiPenerimaan()
        {
            var ctx = new PnbpContext();
            string currentYear = DateTime.Now.Year.ToString();
            //string query = $@"
            //                SELECT ROUND ( sum(JUMLAH) / 2414400030000, 3 ) * 100 AS persentase 
            //                FROM rekappenerimaandetail 
            //                where tahun = {currentYear}";

            string query = @"SELECT 
                            ROUND ( TO_NUMBER(VALUE) / 2414400030000, 3 ) * 100 AS persentase 
                        FROM DASHBOARD_SUMMARY  
                        where kode = 'TOTAL_PENERIMAAN'";
            var persentase_penerimaan = ctx
                .Database
                .SqlQuery<Entities.TotalPenerimaan>(query)
                .FirstOrDefault();
            persentase_penerimaan.persentase = (persentase_penerimaan.persentase == null ? 0 : persentase_penerimaan.persentase);
            var persentasePenerimaan = persentase_penerimaan.persentase?.ToString("N0", new System.Globalization.CultureInfo("id-ID"));

            return Json(new { persentasePenerimaan }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPersentaseBelanja()
        {
            var ctx = new PnbpContext();
            string currentYear = DateTime.Now.Year.ToString();

            var total_belanja = ctx
                .Database
                .SqlQuery<Entities.TotalBelanja>($@"
                    SELECT SUM (Amount) as jumlah 
                    FROM SPAN_BELANJA 
                    where SUMBER_DANA = 'D' and KDSATKER != '524465' AND TAHUN = {currentYear}")
                .FirstOrDefault();
            total_belanja.jumlah = (total_belanja.jumlah == null ? 0 : total_belanja.jumlah);

            var total_realisasi = ctx
                .Database
                .SqlQuery<Entities
                .TotalBelanja>($@"
                    SELECT SUM (Amount) as jumlah 
                    FROM SPAN_REALISASI 
                    where SUMBERDANA = 'D' and KDSATKER != '524465' AND TAHUN = {currentYear}")
                .FirstOrDefault();
            total_realisasi.jumlah = (total_realisasi.jumlah == null ? 0 : total_realisasi.jumlah);

            var persentase_belanja = (total_realisasi.jumlah == 0 && total_belanja.jumlah == 0) ? 0 : (total_realisasi.jumlah / total_belanja.jumlah * 100);
            var persentaseBelanja = persentase_belanja?.ToString("N0", new System.Globalization.CultureInfo("id-ID"));

            return Json(new { persentaseBelanja }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTotalRealisasi()
        {
            var ctx = new PnbpContext();
            string currentYear = DateTime.Now.Year.ToString();
            var query = $@"
                        SELECT SUM (Amount) as jumlah 
                        FROM SPAN_REALISASI 
                        where SUMBERDANA = 'D' and KDSATKER != '524465' AND TAHUN = {currentYear}";
            var total_realisasi = ctx
                .Database
                .SqlQuery<Entities
                .TotalBelanja>(query)
                .FirstOrDefault();
            total_realisasi.jumlah = (total_realisasi.jumlah == null ? 0 : total_realisasi.jumlah);

            return Json(total_realisasi, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDataHalfPie()
        {
            var ctx = new PnbpContext();
            string currentYear = DateTime.Now.Year.ToString();
            var query = $@"
                        SELECT 
                            NVL(SUM (TERALOKASI),0) AS TERALOKASI 
                        FROM REKAPALOKASI 
                        WHERE STATUSALOKASI = 1 AND TAHUN = {currentYear}";
            // 
            var get_mp = ctx
                .Database
                .SqlQuery<Decimal>(query)
                .FirstOrDefault();

            query = $@"
                        SELECT 
                            NVL(SUM(NILAIANGGARAN),0) 
                        FROM MANFAAT 
                        WHERE TAHUN = {currentYear}";
            // pagu diambil dari span_belanja dengan tipe D

            var get_pagu = ctx
                .Database
                .SqlQuery<Decimal>(query).FirstOrDefault();

            query = $@"
                    SELECT 
                        SUM (Amount) as jumlah 
                    FROM SPAN_REALISASI 
                    WHERE SUMBERDANA = 'D' and KDSATKER != '524465' AND TAHUN = {currentYear} ";
            var total_realisasi = ctx
                .Database
                .SqlQuery<Entities.TotalBelanja>(query)
                .FirstOrDefault();
            total_realisasi.jumlah = (total_realisasi.jumlah == null ? 0 : total_realisasi.jumlah);
            // span realisasi = belanja


            query = $@"
                    SELECT 
                        SUM (Amount) as jumlah 
                    FROM SPAN_BELANJA 
                    WHERE SUMBER_DANA = 'D' and KDSATKER != '524465' AND TAHUN = {currentYear}";
            var total_belanja = ctx
                .Database
                .SqlQuery<Entities.TotalBelanja>(query)
                .FirstOrDefault();
            total_belanja.jumlah = (total_belanja.jumlah == null ? 0 : total_belanja.jumlah);
            // span belanja = belanja

            query = $@"
                    select 
                        NVL(sum(TERALOKASI), 0) AS MP 
                    from REKAPALOKASI 
                    where tahun = {currentYear} AND TIPEMANFAAT = 'OPS'";
            var getmpops = ctx
                .Database
                .SqlQuery<Decimal>(query)
                .FirstOrDefault();

            query = $@"
                    SELECT 
                        NVL(SUM (a.amount), 0) AS REALISASI 
                    FROM SPAN_REALISASI a 
                    LEFT JOIN KODESPAN b ON a.KEGIATAN = b.KODE AND a.OUTPUT = b.KEGIATAN 
                    WHERE b.TIPE = 'OPS' AND a. SUMBERDANA = 'D' AND SUBSTR(TANGGAL, 8, 2) = 21 AND TAHUN = {currentYear}";
            var getbelanjaops = ctx
                .Database
                .SqlQuery<Decimal>(query)
                .FirstOrDefault();

            query = $@"
                    select 
                        NVL(sum(TERALOKASI), 0) AS MP 
                    from REKAPALOKASI 
                    where tahun = {currentYear} AND TIPEMANFAAT = 'NONOPS'";
            var getmpnonops = ctx
                .Database
                .SqlQuery<Decimal>(query)
                .FirstOrDefault();

            query = $@"
                    SELECT 
                        NVL(SUM (a.amount), 0) AS REALISASI 
                    FROM SPAN_REALISASI a 
                    LEFT JOIN KODESPAN b ON a.KEGIATAN = b.KODE AND a.OUTPUT = b.KEGIATAN 
                    WHERE b.TIPE = 'NONOPS' AND a. SUMBERDANA = 'D' AND TAHUN = {currentYear}";
            var getbelanjanonops = ctx.Database.SqlQuery<Decimal>(query).FirstOrDefault();

            decimal persenMp = 0;
            decimal persenPagu = 0;
            decimal persenMpVsBelanja = 0;
            decimal persenMpVsBelanjaOps = 0;
            decimal persenMpVsBelanjaNonOps = 0;

            decimal mpVsBelanjaNonOps_MP = 0;
            decimal mpVsBelanjaNonOps_BELANJA = 0;

            try
            {
                //persenMp = get_mp * 100 / (get_pagu + get_mp);
                //persenPagu = (get_pagu * 100 / (get_pagu + get_mp))
                persenMp = (get_mp == 0 && get_pagu == 0) ? 0 : (get_mp / get_pagu) * 100;
                persenPagu = 100 - persenMp;
                persenMpVsBelanja = get_mp == 0 ? 0 : ((decimal)total_realisasi.jumlah / get_mp) * 100;
                persenMpVsBelanjaOps = getmpops == 0 ? 0 : (getbelanjaops / getmpops) * 100;
                persenMpVsBelanjaNonOps = getmpnonops == 0 ? 0 : (getbelanjanonops / getmpnonops) * 100;

                

                if (getmpnonops == 0 && getbelanjanonops != 0)
                {
                    mpVsBelanjaNonOps_MP = 0;
                    mpVsBelanjaNonOps_BELANJA = 100;
                } 
                else if (getmpnonops == 0 && getbelanjanonops == 0)
                {
                    mpVsBelanjaNonOps_MP = 0;
                    mpVsBelanjaNonOps_BELANJA = 0;
                } 
                else if (getmpnonops != 0 && getbelanjanonops == 0)
                {
                    mpVsBelanjaNonOps_MP = 100;
                    mpVsBelanjaNonOps_BELANJA = 0;
                } else
                {
                    mpVsBelanjaNonOps_BELANJA = (getbelanjanonops / getmpnonops) * 100;
                    mpVsBelanjaNonOps_MP = 100 - mpVsBelanjaNonOps_MP;
                }
            } 
            catch (Exception e)
            {
                _ = e.StackTrace;
            }
            

            var jsonResult = new
            {
                paguVsMp = new
                {
                    mp = persenMp,
                    pagu = persenPagu
                },
                mpVsBelanja = new
                {
                    mp = 100 - persenMpVsBelanja,
                    belanja = persenMpVsBelanja
                },
                mpVsBelanjaOps = new
                {
                    mp = 100 - persenMpVsBelanjaOps,
                    belanja = persenMpVsBelanjaOps
                },
                mpVsBelanjaNonOps = new
                {
                    mp = mpVsBelanjaNonOps_MP,
                    belanja = mpVsBelanjaNonOps_BELANJA
                }
            };

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getDashboardLine()
        {
            string pTahun = "";
            string pSatker = "";

            Entities.CharDashboard data = new Entities.CharDashboard();

            List<Entities.AlokasiBelanja> lsAlokasi = Pnbp.Models.HomeModel.dtAlokasiBelanjaNonOps(pTahun, pSatker);
            List<Entities.AlokasiBelanja> lsAlokasiOps = Pnbp.Models.HomeModel.dtAlokasiBelanjaOps(pTahun, pSatker);

            data.jumlahops = lsAlokasiOps.Select(v => (decimal)v.jumlahops).ToList();
            data.bulanAlokOps = lsAlokasiOps.Select(v => (decimal)v.bulanAlokOps).ToList();

            data.jumlah = lsAlokasi.Select(v => (decimal)v.jumlah).ToList();
            data.bulan = lsAlokasi.Select(v => (decimal)v.bulan).ToList();
            data.tahun = pTahun;
            data.lstahun = Pnbp.Models.HomeModel.lsTahunPenerimaan();

            //Get Belanja OPS (Line Chart)
            List<Entities.BelanjaOps> lsBelanjaOps = Pnbp.Models.HomeModel.dtBelanjaOps(pTahun, pSatker);
            data.belanjaOps = lsBelanjaOps.Select(v => (decimal)v.belanjaOps).ToList();
            data.bulanOps = lsBelanjaOps.Select(v => (string)v.bulanOps).ToList();
            data.tahun = pTahun;

            //Get Belanja NonOps (Line Chart)
            List<Entities.BelanjaNonOps> lsBelanjaNonOps = Pnbp.Models.HomeModel.dtBelanjaNonOps(pTahun, pSatker);
            data.belanjaNonOps = lsBelanjaNonOps.Select(v => (decimal)v.belanjaNonOps).ToList();
            data.bulanNonOps = lsBelanjaNonOps.Select(v => (string)v.bulanNonOps).ToList();
            data.tahun = pTahun;

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IndexPartial(string pTahun, string pSatker)
        {
            var ctx = new PnbpContext();

            string currentYear = DateTime.Now.Year.ToString();
            var total_penerimaan = ctx.Database.SqlQuery<Entities.TotalPenerimaan>("SELECT SUM (jumlah) as jumlah FROM rekappenerimaandetail where tahun = " + currentYear + " ").FirstOrDefault();
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
