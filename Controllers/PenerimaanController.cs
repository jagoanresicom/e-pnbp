using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Net.Mail;
//using System.Data;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Globalization;
using OfficeOpenXml;
using System.Text;
using System.Threading.Tasks;
using static Pnbp.Models.PenerimaanModel;
//using OfficeOpenXml.Style;

namespace Pnbp.Controllers
{
    [AccessDeniedAuthorize]
    public class PenerimaanController : Controller
    {
        //
        // GET: /Laporan/

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult FillCity(string kode)
        {
            var kab = Pnbp.Models.AdmModel.getKantor(kode);
            return Json(kab, JsonRequestBehavior.AllowGet);
        }

        #region RealisasiPenerimaan
        public ActionResult RealisasiPenerimaan()
        {
            List<Entities.Wilayah> listPropinsi = new List<Entities.Wilayah>();
            var ctx = new PnbpContext();

            var get_propinsi = ctx.Database.SqlQuery<Entities.propinsi>("SELECT kantorId, KODESATKER, NAMA_SATKER FROM satker WHERE tipekantorid in (1,2)").ToList();
            ViewData["get_propinsi"] = get_propinsi;
            //return Json(get_propinsi, JsonRequestBehavior.AllowGet);

            return View("RealisasiPenerimaan_dtable");
        }

        public ActionResult satker(string kode)
        {
            var ctx = new PnbpContext();

            var get_satker = ctx.Database.SqlQuery<Entities.satker>("select kantorid, kode, nama from kantor where induk = '"+ kode +"' and tipekantorid != 2 order by kode asc").ToList();
            return Json(get_satker, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarRealisasiPenerimaanDT(DatatablesRequest req, string tahun, string bulan, string satker, string propinsi)
        {

            Models.PenerimaanModel model = new Models.PenerimaanModel();

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

           var reqTahun = (!string.IsNullOrEmpty(tahun)) ? tahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
       
            var data = model.GetRealisasiPenerimaanDT(
                reqTahun, 
                bulan, 
                propinsi, 
                satker, 
                tipekantorid, 
                kantorid, 
                req.Start, 
                req.Length
            );

            var resp = Json(new {
                draw = req.Draw,
                data = data.daftarRekapan.Select(x =>
                {
                    var operasional = x.operasional.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                    var penerimaan = x.penerimaan.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                    return new
                    {
                        x.urutan,
                        penerimaan,
                        operasional,
                        x.nilaitarget,
                        x.nama_satker,
                        x.namaprosedur,
                        x.kodesatker,
                        x.kodepenerimaan,
                        x.kanwilid,
                        x.kantorid,
                        x.jumlah,
                        x.jenispenerimaan
                    };
                }),
                recordsTotal = data.RecordsTotal,
                recordsFiltered = data.RecordsFiltered
            });

            resp.MaxJsonLength = int.MaxValue;
            return resp;
        }

        public ActionResult DaftarRealisasiPenerimaan(Entities.FilterPenerimaan frm)
        {
            Models.PenerimaanModel model = new Models.PenerimaanModel();
            Entities.FilterPenerimaan _frm = new Entities.FilterPenerimaan();

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            _frm.tahun = (!string.IsNullOrEmpty(frm.tahun)) ? frm.tahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            _frm.bulan = frm.bulan;
            _frm.satker = frm.satker;
            _frm.propinsi = frm.propinsi;
            _frm.lspenerimaandetail = model.GetRealisasiPenerimaan(_frm.tahun, _frm.bulan, _frm.propinsi, _frm.satker, tipekantorid, kantorid);
            return PartialView("RealisasiPenerimaanls", _frm);
        }

        [HttpPost]
        public FileResult Export(Entities.FilterPenerimaan frm)
        {
            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            Pnbp.Models.PenerimaanModel _pm = new Models.PenerimaanModel();
            Entities.FilterPenerimaan _frm = new Entities.FilterPenerimaan();
            _frm.tahun = (!string.IsNullOrEmpty(frm.tahun)) ? frm.tahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            _frm.bulan = frm.bulan;
            _frm.bulan = frm.bulan;
            _frm.satker = frm.satker;
            DataTable dt = new DataTable(_frm.tahun);
            dt.Columns.AddRange(new DataColumn[6] {
                new DataColumn("No",typeof(int)),
                new DataColumn("Nama_Satker"),
                new DataColumn("Kode_Satker"),
                new DataColumn("nilaitarget",typeof(decimal)),
                new DataColumn("Penerimaan",typeof(decimal)),
                new DataColumn("Operasional",typeof(decimal)) });

            List<Entities.DaftarRekapPenerimaanDetail> result = _pm.GetRealisasiPenerimaan(_frm.tahun, _frm.bulan, _frm.propinsi, _frm.satker, tipekantorid, kantorid);

            foreach (var rw in result)
            {
                dt.Rows.Add(rw.urutan, rw.nama_satker, rw.kodesatker, rw.nilaitarget, rw.penerimaan, rw.operasional);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RealisasiPenerimaan" + DateTime.Now.ToString("dd-mm-yyyy") + ".xlsx");
                }
            }
        }

        public ActionResult RealisasiPenerimaanDetail(string Id, string pTahun, string pBulan)
        {
            ViewData["bulan"] = pBulan;
            ViewData["kantorId"] = Id;
            ViewData["tahun"] = pTahun;
            return View();
        }

        public JsonResult RealisasiPenerimaanDetailDT(DatatablesRequest req, string id, string pTahun, string pBulan)
        {
            var ctx = new PnbpContext();

            List<Entities.RealisasiPenerimaanDetail> _list = new List<Entities.RealisasiPenerimaanDetail>();

            string baseQuery =
                @" SELECT
                   {0}
                FROM
	                rekappenerimaandetail r1
                LEFT JOIN TARGETPROSEDUR r2 ON r2.KANTORID = r1.KANTORID and r1.namaprosedur = r2.namaprosedur
                    where r1.kantorid = '" + id + "'";

            if (!String.IsNullOrEmpty(pTahun))
            {
                baseQuery += " and r1.tahun = " + pTahun + " ";
            }

            if (!String.IsNullOrEmpty(pBulan))
            {
                baseQuery += " and r1.bulan = " + pBulan + " ";
                //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pBulan));
            }

            baseQuery += " group by r1.kantorid, r1.berkasid, r1.kodesatker, r1.namakantor, r1.namaprosedur, r2.TARGETFISIK, r2.NILAITARGET";

            
    

            string countQuery = @"SELECT COUNT(*) as count FROM (" + string.Format(baseQuery, "DISTINCT r1.KANTORID, r1.BERKASID, r1.namakantor, r1.namaprosedur, NVL(r2.TARGETFISIK, 0) AS targetfisik") + ")";


            var recordsTotal = ctx.Database.SqlQuery<CountResult>(countQuery).First().Count;

            baseQuery += string.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", req.Start, req.Length);

            string query = string.Format(baseQuery, @"
                DISTINCT r1.kantorid,
                r1.berkasid,
                r1.kodesatker,
	            r1.namakantor,
	            r1.namaprosedur,
	            NVL (r2.TARGETFISIK, 0) AS targetfisik,
	            COUNT (r1.jumlah) AS jumlah,
                ROUND(NVL(count(r1.JUMLAH)/r2.TARGETFISIK*100,0),2) as persentasefisik,
	            NVL(r2 .nilaitarget, 0) AS nilaitarget,
	            SUM (r1.penerimaan) AS penerimaan,
	            ROUND (SUM(r1.operasional), 2) AS operasional,
                round(nvl(sum(r1.penerimaan) / r2.nilaitarget * 100,0),2) as persentasepenerimaan,
	            ROW_NUMBER () OVER (
		            ORDER BY
			            SUM (r1.penerimaan) DESC
	            ) AS urutan
            ");

            _list = ctx.Database.SqlQuery<Entities.RealisasiPenerimaanDetail>(query).ToList<Entities.RealisasiPenerimaanDetail>();
           
            var jsonResult = Json(new
            {
                draw = req.Draw,
               recordsTotal,
               recordsFiltered = recordsTotal,
               data= _list.Select(x => new
               {
                   x.berkasid,
                   x.bulan,
                   x.jumlah,
                   x.kantorid,
                   x.kodesatker,
                   x.namakantor,
                   x.namaprosedur,
                   nilaitarget = x.nilaitarget.Value.ToString("N0", new System.Globalization.CultureInfo("id-ID")),
                   x.operasional,
                   penerimaan = x.penerimaan.ToString("N0", new System.Globalization.CultureInfo("id-ID")),
                   x.persentasefisik,
                   x.persentasepenerimaan,
                   x.tahun,
                   targetfisik = x.targetfisik.ToString("N0", new System.Globalization.CultureInfo("id-ID")),
                   targetpenerimaan = x.targetpenerimaan.ToString("N0", new System.Globalization.CultureInfo("id-ID")),
                   x.urutan
               })
            });

            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;
        }

        public ActionResult RealisasiPenerimaanPerbandingan(string berkasId, string kantorId, int tahun, int bulan)
        {
            var ctx = new PnbpContext();

            var penerimaan = ctx.Database.SqlQuery<Entities.DataPenerimaan>("SELECT * FROM PENERIMAAN p WHERE p.KANTORID = '"+kantorId+"' AND p.BERKASID = '"+berkasId+"' AND p.TAHUN="+tahun+" AND p.BULAN="+bulan+" ").First();
            var list_penerimaan = ctx.Database.SqlQuery<Entities
                .DataPenerimaan>("SELECT p.*, k.namaprogram FROM PENERIMAAN p LEFT JOIN KODESPAN k ON k.kode=SUBSTR(p.KODEPENERIMAAN, 0, 4) AND k.kegiatan=SUBSTR(p.KODEPENERIMAAN, -3) WHERE p.KANTORID = '" + kantorId + "' AND p.KODESPOPP = '" + penerimaan.kodespopp + "' AND p.TAHUN=" + tahun + " AND p.bulan = "+bulan+" ")
                .ToList();


            var alokasi0 = list_penerimaan.Where(x => x.statusalokasi == 0);
            var alokasi1 = list_penerimaan.Where(x => x.statusalokasi == 1);
            ViewData["alokasi0"] = alokasi0;
            ViewData["alokasi1"] = alokasi1;
            ViewData["penerimaan"] = penerimaan;

            return View();
        }

        public struct TestRequest
        {
            public bool statusAlokasi { get; set; }
            public string kantorId { get; set; }
            public int tahun { get; set; }
            public int? bulan { get; set; }
        }

        public struct DatatablesRequest
        {
            public int Draw { get; set; }

            public int Start { get; set; }

            public int Length { get; set; }

            public SearchParam Search { get; set; }

            public ColumnParam[] Columns { get; set; }

            public OrderParam[] Order { get; set; }

            public IDictionary<string, dynamic> Filter { get; set; }

            public struct OrderParam
            {
                public int Column { get; set; }
                public string Dir { get; set; }
            }

            public struct ColumnParam
            {
                public string Data { get; set; }
                public string Name { get; set; }
                public bool Searchable { get; set; }
                public bool Orderable { get; set; }
                public SearchParam SearchParam { get; set; }
            }

            public struct SearchParam
            {
                public string Value { get; set; }
                public bool Regex { get; set; }
            }
        }

        struct CountResult
        {
            public int Count { get; set; }
        }

        public async Task<JsonResult> RealisasiPenerimaanPerbandinganSatkerDt(DatatablesRequest req, bool statusAlokasi, string kantorId, int tahun, int? bulan)
        {
            var ctx = new PnbpContext();

            var queryPenerimaan = "SELECT * FROM PENERIMAAN p WHERE p.KANTORID = '" + kantorId + "' AND p.TAHUN=" + tahun;

            if (bulan != null)
            {
                queryPenerimaan += " AND p.bulan=" + bulan;
            }

            var penerimaan = ctx.Database.SqlQuery<Entities.DataPenerimaan>(queryPenerimaan).First();

            var queryBase = @"
                SELECT 
                    {0}
                FROM PENERIMAAN p
                LEFT JOIN KODESPAN k 
                ON k.kode=SUBSTR(p.KODEPENERIMAAN, 0, 4) 
                    AND k.kegiatan=SUBSTR(p.KODEPENERIMAAN, -3) 
                WHERE p.KANTORID = '" + kantorId + "' AND p.TAHUN=" + tahun + " AND p.STATUSALOKASI=" + (statusAlokasi ? "1": "0");

            if (bulan != null)
            {
                queryBase += " AND p.BULAN = " + bulan;
            }


            var recordsTotal = ctx.Database.SqlQuery<CountResult>(string.Format(queryBase, "COUNT(*) as count")).First().Count;

            queryBase += string.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", req.Start, req.Length);

            var queryList = string.Format(queryBase, "p.*, k.namaprogram");

            var list_penerimaan = await ctx.Database.SqlQuery<Entities
                .DataPenerimaan>(queryList)
                .ToListAsync();

            var recordsFiltered = recordsTotal;

           var jsonResult=  Json(new
            {
                draw = req.Draw,
                recordsFiltered,
                recordsTotal,
                data = list_penerimaan.Select(x => new
                {
                    x.jumlah,
                    x.jenispenerimaan,
                    x.kodebilling,
                    x.kodesatker,
                    x.kodepenerimaan,
                    x.namaprogram,
                    x.namaprosedur,
                    x.tahunberkas,
                    x.statusalokasi,
                    x.nomorberkas,
                    x.nilaiakhir,
                    x.namakantor,
                    tanggal = x.tanggal.ToString("dd/mm/yyyy")
                })
            });

            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;
        }

        public ActionResult RealisasiPenerimaanPerbandinganSatker(string kantorId, int tahun, int? bulan)
        {
            var ctx = new PnbpContext();

            var queryPenerimaan = "SELECT * FROM PENERIMAAN p WHERE p.KANTORID = '" + kantorId + "' AND p.TAHUN=" + tahun;

            if(bulan != null)
            {
                queryPenerimaan += " AND p.bulan=" + bulan;
            }

            var penerimaan = ctx.Database.SqlQuery<Entities.DataPenerimaan>(queryPenerimaan).First();


            ViewData["kantorId"] = kantorId;
            ViewData["tahun"] = tahun;
            ViewData["bulan"] = bulan;
            ViewData["penerimaan"] = penerimaan;

            return View();
        }

        //public ActionResult RealisasiPenerimaanDetailBreakdown(string Id, string pTahun, string pBulan, string namaprosedur)
        //{
        //    if (string.IsNullOrEmpty(Id))
        //    {
        //        return RedirectToAction("RealisasiLayanan");
        //    }
        //    Models.PenerimaanModel model = new Models.PenerimaanModel();
        //    List<Entities.RealisasiPenerimaanDetailBreakdown> databreakdown = model.GetRealisasiPenerimaanDetailBreakdown(Id, pTahun, pBulan, namaprosedur);
        //    return Json(databreakdown, JsonRequestBehavior.AllowGet);

        //    ViewData["data"] = databreakdown;
        //    return PartialView("RealisasiPenerimaanDetailBreakdown", databreakdown);
        //}
        #endregion

        #region RealisasiLayanan

        public ActionResult DaftarRealisasiLayananDT(DatatablesRequest req, string tahun, string bulan)
        {
            Models.PenerimaanModel model = new Models.PenerimaanModel();
            Entities.FilterPenerimaan _frm = new Entities.FilterPenerimaan();

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            var data = model.GetRealisasiLayananDT(tahun, bulan, tipekantorid, kantorid, req.Start, req.Length);

            var result= Json(new
            {
                draw = req.Draw,
                data = data.ListPenerimaan.Select(x => {
                    var penerimaan = x.penerimaan.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                    var jumlah = x.jumlah.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                    return new {
                        x.jenispenerimaan,
                        jumlah,
                        x.kantorid,
                        x.kanwilid,
                        x.kodepenerimaan,
                        x.kodesatker,
                        x.nama,
                        x.namaprosedur,
                        x.operasional,
                        penerimaan,
                        x.urutan
                    };
                }),
                recordsTotal = data.RecordsTotal,
                recordsFiltered=  data.RecordsFiltered
            });

            result.MaxJsonLength = int.MaxValue;

            return result;
        }

        public ActionResult DaftarRealisasiLayanan(Entities.FilterPenerimaan frm)
        {
            Models.PenerimaanModel model = new Models.PenerimaanModel();
            Entities.FilterPenerimaan _frm = new Entities.FilterPenerimaan();

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            _frm.tahun = (!string.IsNullOrEmpty(frm.tahun)) ? frm.tahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            _frm.bulan = frm.bulan;
            //_frm.satker = frm.satker;
            _frm.lspenerimaan = model.GetRealisasiLayanan(_frm.tahun, _frm.bulan, tipekantorid, kantorid);
            //return Json(_frm.lspenerimaan, JsonRequestBehavior.AllowGet);
            return PartialView("RealisasiLayananls", _frm);
        }

        [HttpPost]
        public FileResult ExportLayanan(Entities.FilterPenerimaan frm)
        {
            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            Pnbp.Models.PenerimaanModel _pm = new Models.PenerimaanModel();
            Entities.FilterPenerimaan _frm = new Entities.FilterPenerimaan();
            _frm.tahun = (!string.IsNullOrEmpty(frm.tahun)) ? frm.tahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            _frm.bulan = frm.bulan;
            _frm.bulan = frm.bulan;
            //_frm.satker = frm.satker;
            DataTable dt = new DataTable(_frm.tahun);
            dt.Columns.AddRange(new DataColumn[4] {
                new DataColumn("No",typeof(int)),
                new DataColumn("Jenis_Layanan"),
                new DataColumn("RealisasiFisik",typeof(decimal)),
                new DataColumn("RealisasiPenerimaan",typeof(decimal))});

            List<Entities.StatistikPenerimaan> result = _pm.GetRealisasiLayanan(_frm.tahun, _frm.bulan, tipekantorid, kantorid);
            foreach (var rw in result)
            {
                dt.Rows.Add(rw.urutan, rw.namaprosedur, rw.jumlah, rw.penerimaan);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Realisasi_Layanan" + DateTime.Now.ToString("yyyy-mm-dd") + ".xlsx");
                }
            }
        }

        public ActionResult RealisasiLayananDetail(string Id, string pTahun, string pBulan)
        {
            //return Json(Id, JsonRequestBehavior.AllowGet);
            if (string.IsNullOrEmpty(Id))
            {
                return RedirectToAction("RealisasiLayanan");
            }

            Models.PenerimaanModel model = new Models.PenerimaanModel();

            ViewData["id"] = Id;
            ViewData["tahun"] = pTahun;
            ViewData["bulan"] = pBulan;

            return View();
        }

        public JsonResult RealisasiLayananDetailDT(DatatablesRequest req, string Id, string pTahun, string pBulan)
        {
            Models.PenerimaanModel model = new Models.PenerimaanModel();

            RealisasiLayananDetailDTResult data = model.GetRealisasiLayananDetailDT(Id, pTahun, pBulan, req.Start, req.Length);

            var jsonResult = Json(new
            {
                draw  = req.Draw,
                recordsTotal = data.RecordsTotal,
                recordsFiltered = data.RecordsFiltered,
                data = data.List.Select(x => new
                {
                    x.berkasid,
                    x.namakantor,
                    x.namaprosedur,
                    x.kodesatker,
                    targetfisik = x.targetfisik.ToString("N0", new System.Globalization.CultureInfo("id-ID")),
                    targetpenerimaan = x.targetpenerimaan.ToString("N0", new System.Globalization.CultureInfo("id-ID")),
                    realisasipenerimaan = x.realisasipenerimaan.ToString("N0", new System.Globalization.CultureInfo("id-ID")),
                    x.persentasepenerimaan,
                    x.persentasefisik,
                    x.operasional,
                    x.kantorid,
                    x.jumlah,
                    x.urutan
                })
            });

            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;
        }
        #endregion


        #region StatistikPenerimaan
        public ActionResult StatistikPenerimaan()
        {
            Pnbp.Entities.FilterPenerimaan _frm = new Entities.FilterPenerimaan();
            _frm.lstahun = Pnbp.Models.HomeModel.lsTahunPenerimaan();
            _frm.tahun = (!string.IsNullOrEmpty(_frm.tahun)) ? _frm.tahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            return View(_frm);
        }

        public ActionResult DaftarPenerimaan(Entities.FilterPenerimaan frm)
        {
            Models.PenerimaanModel model = new Models.PenerimaanModel();
            Entities.FilterPenerimaan _frm = new Entities.FilterPenerimaan();

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            _frm.tahun = (!string.IsNullOrEmpty(frm.tahun)) ? frm.tahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            _frm.bulan = frm.bulan;
            _frm.lspenerimaan = model.GetPenerimaanNasional(_frm.tahun, _frm.bulan, tipekantorid, kantorid);
            return PartialView("StatistikPenerimaanls", _frm);
        }

        public ActionResult StatistikPenerimaanDetail(string Id, string pTahun, string pBulan)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return RedirectToAction("StatistikPenerimaan");
            }
            Models.PenerimaanModel model = new Models.PenerimaanModel();
            List<Entities.StatistikPenerimaan> data = model.GetPenerimaanSatker(Id, pTahun, pBulan);
            return PartialView("StatistikPenerimaanDetail", data);
        }
        #endregion

        #region StatistikNTPN
        public ActionResult StatistikNTPN()
        {
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId((User as Entities.InternalUserIdentity).KantorId);
            this.ViewData["tipeKantor"] = tipekantorid;
            Entities.QueryStatistikNTPN _qStatistikNTPN = new Entities.QueryStatistikNTPN();
            if (tipekantorid == "1")
            {
                _qStatistikNTPN.propinsi = Pnbp.Models.AdmModel.getKantor("");
                _qStatistikNTPN.kabupaten = new List<Entities.Wilayah>();
                _qStatistikNTPN.TanggalMulai = DateTime.Now.AddDays(-29).ToShortDateString();
                _qStatistikNTPN.TanggalSampai = DateTime.Now.ToShortDateString();
            }
            else if (tipekantorid == "2")
            {
                _qStatistikNTPN.propinsi = Pnbp.Models.AdmModel.getKantorList((User as Entities.InternalUserIdentity).KantorId);
                _qStatistikNTPN.kabupaten = Pnbp.Models.AdmModel.getKantorKanwil(true, (User as Entities.InternalUserIdentity).KantorId);
                _qStatistikNTPN.TanggalMulai = DateTime.Now.AddDays(-29).ToShortDateString();
                _qStatistikNTPN.TanggalSampai = DateTime.Now.ToShortDateString();
            }
            else
            {
                _qStatistikNTPN.propinsi = Pnbp.Models.AdmModel.getKantorKanwil(false, (User as Entities.InternalUserIdentity).KantorId);
                _qStatistikNTPN.kabupaten = Pnbp.Models.AdmModel.getKantorList((User as Entities.InternalUserIdentity).KantorId);
                _qStatistikNTPN.TanggalMulai = DateTime.Now.AddDays(-29).ToShortDateString();
                _qStatistikNTPN.TanggalSampai = DateTime.Now.ToShortDateString();
            }

            return View("StatistikNTPNList", _qStatistikNTPN);
        }

        public ActionResult StatistikNTPNRows(Entities.QueryStatistikNTPN query, int? draw, int? start, int? length)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 100;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            string pKantorId = String.IsNullOrEmpty(query.selectedKabupaten) ? !String.IsNullOrEmpty(query.pKantorId) ? query.pKantorId : "" : query.selectedKabupaten;
            Models.PenerimaanModel lm = new Models.PenerimaanModel();
            List<Entities.StatistikNTPN> result = new List<Entities.StatistikNTPN>();
            if (String.IsNullOrEmpty(pKantorId) && Pnbp.Models.AdmModel.GetTipeKantorId((User as Entities.InternalUserIdentity).KantorId) == "2")
            {
                result = lm.dtStatistikNTPNKanwil((User as Entities.InternalUserIdentity).KantorId, Convert.ToDateTime(query.TanggalMulai), Convert.ToDateTime(query.TanggalSampai), from, to);
            }
            else
            {
                result = lm.dtStatistikNTPN(query.selectedKabupaten, Convert.ToDateTime(query.TanggalMulai), Convert.ToDateTime(query.TanggalSampai), from, to);
            }

            //return Json(query, JsonRequestBehavior.AllowGet);

            Entities.StatistikNTPN rec = result.FirstOrDefault<Entities.StatistikNTPN>();
            return Json(new { data = result, draw = draw, recordsTotal = (rec == null) ? 0 : rec.totalrec, recordsFiltered = (rec == null) ? 0 : rec.totalrec }, JsonRequestBehavior.AllowGet);
            //int custIndex = from;
            //Dictionary<int, Entities.StatistikNTPN> dict = result.ToDictionary(x => custIndex++, x => x);

            //if (result.Count > 0)
            //{
            //    if (Request.IsAjaxRequest())
            //    {
            //        return PartialView("StatistikNTPNListPartial", dict);
            //    }
            //    else
            //    {
            //        return RedirectToAction("StatistikNTPN", "Laporan");
            //    }
            //}
            //else
            //{
            //    return new ContentResult
            //    {
            //        ContentType = "text/html",
            //        Content = "noresults",
            //        ContentEncoding = System.Text.Encoding.UTF8
            //    };
            //}
        }

        public ActionResult ShowDetailNTPN(Entities.QueryStatistikNTPN query, int? draw, int? start, int? length)
        {
            //return Json(query.idprop, JsonRequestBehavior.AllowGet);

            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            string pKantorId = String.IsNullOrEmpty(query.selectedKabupaten) ? !String.IsNullOrEmpty(query.pKantorId) ? query.pKantorId : "" : query.selectedKabupaten;
            Models.PenerimaanModel brqm = new Models.PenerimaanModel();
            List<Entities.StatistikNTPNDetail> result = brqm.dtStatistikNTPNDetail(pKantorId, query.TanggalMulai, query.TanggalSampai, query.kodeBilling, query.ntpn, from, to);
            //return Json(result, JsonRequestBehavior.AllowGet);

            Entities.StatistikNTPNDetail rec = result.FirstOrDefault<Entities.StatistikNTPNDetail>();


            return Json(new { data = result, draw = draw, recordsTotal = (rec == null) ? 0 : rec.totalrec, recordsFiltered = (rec == null) ? 0 : rec.totalrec }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowDetailModalNTPN(Entities.QueryStatistikNTPN query, int? draw, int? start, int? length)
        {
            //string pKantorId = String.IsNullOrEmpty(query.selectedKabupaten) ? !String.IsNullOrEmpty(query.pKantorId) ? query.pKantorId : "" : query.selectedKabupaten;
            Models.PenerimaanModel brqm = new Models.PenerimaanModel();
            List<Entities.ModalStatistikNTPN> result = brqm.dtModalStatistikNTPNDetail(query.kodeBilling);

            Entities.ModalStatistikNTPN rec = result.FirstOrDefault<Entities.ModalStatistikNTPN>();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Rekonsiliasi
        public ActionResult Rekonsiliasi()
        {
            var ctx = new PnbpContext();

            var get_data = ctx.Database.SqlQuery<Entities.Rekonsimfoni>("SELECT * FROM V_REKONDAMI").ToList();
            ViewData["get_data"] = get_data;

            var get_totalpenerimaan = ctx.Database.SqlQuery<Entities.RekonTotalJumlahPenerimaan>("SELECT NVL(SUM(jumlahpenerimaan),0) AS TOTALPENERIMAAN FROM V_REKONDAMI").FirstOrDefault();
            //return Json(get_totalpenerimaan, JsonRequestBehavior.AllowGet);
            ViewData["get_totalpenerimaan"] = get_totalpenerimaan;

            var get_totalsimfoni = ctx.Database.SqlQuery<Entities.RekonTotalJumlahSimfoni>("SELECT NVL(SUM(jumlahsimfoni),0) AS TOTALSIMFONI FROM V_REKONDAMI").FirstOrDefault();
            //return Json(get_totalpenerimaan, JsonRequestBehavior.AllowGet);
            ViewData["get_totalsimfoni"] = get_totalsimfoni;

            return View();
        }

        [HttpPost]
        public ActionResult import(FormCollection form)
        {
            var ctx = new PnbpContext();

            var deleterekon = "DELETE FROM REKONDAMI";
            ctx.Database.ExecuteSqlCommand(deleterekon);

            //var start = ((form.AllKeys.Contains("start")) ? form["start"] : "NULL");
            //var end = ((form.AllKeys.Contains("end")) ? form["end"] : "NULL");

            this.createDir("Content/Uploads");
            var upload = Request.Files["file_txt"];

            string extension = Path.GetExtension(Request.Files["file_txt"].FileName).ToLower();
            string fileName = createFileName() + "" + extension;
            string path = System.IO.Path.Combine(Server.MapPath("~/Content/Uploads"), fileName);
            upload.SaveAs(path);

            //return Json(extension, JsonRequestBehavior.AllowGet);

            if (extension == ".txt")
            {
                string[] lines = System.IO.File.ReadAllLines(path);

                // Use a tab to indent each line of the file

                for (int j = 0; j < lines.Length; j++)
                {
                    var element = lines[j].Split(';');
                    if (element != null)
                    {
                        var kodentpn = element[0];
                        //var reg = element[1];
                        //var teuing = element[2];
                        //var satker = element[3];
                        var jumlah = element[4];

                        var idrekon = NewGuID();
                        var insertsatker = "INSERT INTO REKONDAMI (IDREKON, KODENTPN, JUMLAH) VALUES ('" + idrekon + "','" + kodentpn + "','" + jumlah + "')";
                        ctx.Database.ExecuteSqlCommand(insertsatker);
                    }
                }
            }
            else if (extension == ".xls" || extension == ".xlsx")
            {
                using (var Stream = new FileStream(Path.Combine(Server.MapPath("~/Content/Uploads"), fileName),
               FileMode.OpenOrCreate,
               FileAccess.ReadWrite,
               FileShare.ReadWrite))
                {
                    using (ExcelPackage package = new ExcelPackage(Stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];

                        int rowCount = worksheet.Dimension.Rows;
                        int ColCount = worksheet.Dimension.Columns;


                        var kodentpn = "";
                        var jumlah = "";

                        for (int row = 2; row <= rowCount; row++)
                        {

                            kodentpn = worksheet.Cells[row, 1].Value.ToString();
                            jumlah = worksheet.Cells[row, 2].Value.ToString();

                            var idrekon = NewGuID();
                            var insertsatker = "INSERT INTO REKONDAMI (IDREKON, KODENTPN, JUMLAH) VALUES ('" + idrekon + "','" + kodentpn + "','" + jumlah + "')";
                            //return Json(insertsatker, JsonRequestBehavior.AllowGet);
                            ctx.Database.ExecuteSqlCommand(insertsatker);
                        }
                    }
                }
            }

            return RedirectToAction("Rekonsiliasi");
        }

        //public ActionResult import(FormCollection Form)
        //{
        //    try
        //    {
        //        var ctx = new PnbpContext();

        //        //provide input folder
        //        string SourceFolder = @"C:\laragon\www\epnbp";
        //        //provide file name
        //        string SourceFileName = "29112019.TXT";
        //        //provide the table name in which you would like to load data
        //        string TableName = "rekondami";
        //        //provide the file delimiter such as comma,pipe
        //        string filedelimiter = ";";
        //        //provide the Archive folder where you would like to move file
        //        string ArchiveFodler = @"~/Content/Uploads";

        //        System.IO.StreamReader SourceFile =
        //       new System.IO.StreamReader(SourceFolder + SourceFileName);

        //        string line = "";
        //        Int32 counter = 0;

        //        while ((line = SourceFile.ReadLine()) != null)
        //        {
        //            //skip the header row
        //            if (counter > 0)
        //            {
        //                //prepare insert query
        //                string insertsatker = "Insert into REKONDAMI"+
        //                       " Values ('" + line.Replace(filedelimiter, "';'") + "')";

        //                //execute sqlcommand to insert record
        //                ctx.Database.ExecuteSqlCommand(insertsatker);
        //            }
        //            counter++;
        //        }

        //        File.Move(SourceFolder + SourceFileName, ArchiveFodler + SourceFileName);
        //    }
        //    catch (IOException Exception)
        //    {
        //        Console.Write(Exception);
        //    }
        //    return RedirectToAction("Rekonsiliasi");
        //}


        public ActionResult Showkkprekonsiliasi(Entities.QueryRekonsiliasi query, int? draw, int? start, int? length, FormCollection form)
        {
            var ctx = new PnbpContext();

            this.createDir("Content/Uploads");
            var upload = Request.Files["FILE_TXT"];
            string extension = Path.GetExtension(Request.Files["FILE_TXT"].FileName).ToLower();
            string fileName = createFileName() + "" + extension;
            string path = System.IO.Path.Combine(Server.MapPath("~/Content/Uploads"), fileName);
            using (var Stream = new FileStream(Path.Combine(Server.MapPath("~/Content/Uploads"), fileName),
               FileMode.OpenOrCreate,
               FileAccess.ReadWrite,
               FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(Stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];


                    //int rowCount = worksheet.Dimension.Rows;
                    //int ColCount = worksheet.Dimension.Columns;
                    var kodentpn = "1";
                    var jumlah = "";

                    for (int row = 2; row <= 4; row++)
                    {
                        kodentpn = worksheet.Cells[row, 1].Value.ToString();
                        //return Json(kodentpn, JsonRequestBehavior.AllowGet);
                        jumlah = worksheet.Cells[row, 2].Value.ToString();

                        var rekonid = NewGuID();
                        var insertsatker = "INSERT INTO REKONDAMI (IDREKON, KODENTPN, JUMLAH) VALUES ('" + rekonid + "','" + kodentpn + "','" + jumlah + "')";
                    }
                }
            }
            //string pKantorId = String.IsNullOrEmpty(query.selectedKabupaten) ? !String.IsNullOrEmpty(query.pKantorId) ? query.pKantorId : "" : query.selectedKabupaten;
            Models.PenerimaanModel brqm = new Models.PenerimaanModel();
            List<Entities.DataRekonsiliasi> result = brqm.dtDatakkprekonsiliasi(query.start, query.end);
            return Json(result, JsonRequestBehavior.AllowGet);


            //Entities.DataRekonsiliasi rec = result.FirstOrDefault<Entities.DataRekonsiliasi>();

            //return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        private void createDir(string foldername)
        {
            string path = Path.Combine(Server.MapPath("~/"), foldername);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private string createFileName()
        {
            return Convert.ToString(DateTime.Now.Ticks);
        }

        public string NewGuID()
        {
            string _result = "";
            using (var ctx = new PnbpContext())
            {
                _result = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault<string>();
            }

            return _result;
        }

        #endregion

        #region berandapenerimaan
        public ActionResult BerandaPenerimaan()
        {
            return View();
        }

        public ActionResult IndexPartial(string pTahun, string pSatker)
        {
            Entities.CharDashboard dsboard = new Entities.CharDashboard();

            pTahun = !String.IsNullOrEmpty(pTahun) ? pTahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
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
            //return Json(dsboard, JsonRequestBehavior.AllowGet);
            return PartialView("BerandaPenerimaan", dsboard);
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
        #endregion


        // V2
        public ActionResult pn_provinsi()
        {
            List<Entities.Wilayah> listPropinsi = new List<Entities.Wilayah>();
            var ctx = new PnbpContext();

            var get_propinsi = ctx.Database.SqlQuery<Entities.propinsi>("select kantorId, kode, replace(nama,'Kantor Wilayah ', '') as nama from kantor where tipekantorid=2").ToList();
            //ViewData["get_propinsi"] = get_propinsi;

            return View();
        }

        public ActionResult pn_satker(string pid)
        {
            List<Entities.Wilayah> listPropinsi = new List<Entities.Wilayah>();
            var ctx = new PnbpContext();

            var get_propinsi = ctx.Database.SqlQuery<Entities.propinsi>("SELECT kantorId, KODESATKER, NAMA_SATKER FROM satker WHERE tipekantorid in (1,2)").ToList();
            ViewData["get_propinsi"] = get_propinsi;
            ViewData["provinsi_id"] = pid;
            //return Json(get_propinsi, JsonRequestBehavior.AllowGet);

            return View();
        }

        public ActionResult pn_layanan(string pid)
        {
            List<Entities.Wilayah> listPropinsi = new List<Entities.Wilayah>();
            var ctx = new PnbpContext();

            var get_propinsi = ctx.Database.SqlQuery<Entities.propinsi>("select kantorId, kode, replace(nama,'Kantor Wilayah ', '') as nama from kantor where tipekantorid=2").ToList();
            //ViewData["get_propinsi"] = get_propinsi;
            ViewData["satker"] = pid;

            return View();
        }

        [HttpPost]
        public ActionResult pn_provinsi_list(DatatablesRequest req, string tahun, string bulan, string satker, string provinsi)
        {

            Models.PenerimaanModel model = new Models.PenerimaanModel();

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            var reqTahun = (!string.IsNullOrEmpty(tahun)) ? tahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();

            var data = model.pn_provinsi(
                reqTahun,
                bulan,
                provinsi,
                satker,
                tipekantorid,
                kantorid,
                req.Start,
                req.Length
            );

            int totalRecord = 0;
            if (data.daftarRekapan.Count > 0)
            {
                totalRecord = data.daftarRekapan.First().RecordsTotal;
            }

            var resp = Json(new
            {
                draw = req.Draw,
                data = data.daftarRekapan.Select(x =>
                {
                    var operasional = x.operasional.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                    var penerimaan = x.penerimaan.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                    return new
                    {
                        x.urutan,
                        penerimaan,
                        operasional,
                        x.nilaitarget,
                        x.nama_satker,
                        x.namaprosedur,
                        x.id_provinsi,
                        x.nama_provinsi,
                        x.kodesatker,
                        x.kodepenerimaan,
                        x.kanwilid,
                        x.kantorid,
                        x.jumlah,
                        x.jenispenerimaan,
                        x.targetfisik
                    };
                }),
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord,
            });

            resp.MaxJsonLength = int.MaxValue;
            return resp;
        }

        [HttpPost]
        public ActionResult pn_satker_list(DatatablesRequest req, string tahun, string bulan, string satker, string propinsi)
        {

            Models.PenerimaanModel model = new Models.PenerimaanModel();

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);
            if (tipekantorid == "1")
            {
                kantorid = "";
            }

            var reqTahun = (!string.IsNullOrEmpty(tahun)) ? tahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();

            var data = model.pn_satker(
                reqTahun,
                bulan,
                propinsi,
                satker,
                tipekantorid,
                kantorid,
                req.Start,
                req.Length
            );

            int totalRecord = 0;
            if (data.daftarRekapan.Count > 0)
            {
                totalRecord = data.daftarRekapan.First().RecordsTotal;
            }

            var resp = Json(new
            {
                draw = req.Draw,
                data = data.daftarRekapan.Select(x =>
                {
                    var operasional = x.operasional.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                    var penerimaan = x.penerimaan.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                    return new
                    {
                        x.urutan,
                        penerimaan,
                        operasional,
                        x.nilaitarget,
                        x.nama_satker,
                        x.namaprosedur,
                        x.kodesatker,
                        x.kodepenerimaan,
                        x.kanwilid,
                        x.kantorid,
                        x.jumlah,
                        x.jenispenerimaan
                    };
                }),
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord
            });

            resp.MaxJsonLength = int.MaxValue;
            return resp;
        }


        [HttpPost]
        public ActionResult pn_layanan_list(DatatablesRequest req, string tahun, string bulan, string pid, string layanan)
        {
            Models.PenerimaanModel model = new Models.PenerimaanModel();
            Entities.FilterPenerimaan _frm = new Entities.FilterPenerimaan();


            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            if (string.IsNullOrEmpty(pid))
            {
                kantorid = (User as Entities.InternalUserIdentity).KantorId;
                if (tipekantorid == "1")
                {
                    kantorid = "";
                }
            }
            else
            {
                kantorid = pid;
            }

            tahun = (!string.IsNullOrEmpty(tahun)) ? tahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            var data = model.pn_layanan(tahun, bulan, tipekantorid, kantorid, layanan, req.Start, req.Length);

            int totalRecord = 0;
            if (data.ListPenerimaan.Count > 0)
            {
                totalRecord = data.ListPenerimaan.First().RecordsTotal;
            }

            var result = Json(new
            {
                draw = req.Draw,
                data = data.ListPenerimaan.Select(x => {
                    var penerimaan = x.penerimaan.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                    var jumlah = x.jumlah.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                    return new
                    {
                        x.jenispenerimaan,
                        jumlah,
                        x.kantorid,
                        x.kanwilid,
                        x.kodepenerimaan,
                        x.targetfisik,
                        x.kodesatker,
                        x.nama,
                        x.namaprosedur,
                        x.operasional,
                        penerimaan,
                        x.urutan,
                    };
                }),
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord
            });

            result.MaxJsonLength = int.MaxValue;

            return result;
        }


    }
}