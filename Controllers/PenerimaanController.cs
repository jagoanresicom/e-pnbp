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

            return View();
        }

        public ActionResult satker(string kode)
        {
            var ctx = new PnbpContext();

            var get_satker = ctx.Database.SqlQuery<Entities.satker>("select kantorid, kode, nama from kantor where induk = '"+ kode +"' and tipekantorid != 2 order by kode asc").ToList();
            return Json(get_satker, JsonRequestBehavior.AllowGet);
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
            //return Json(pTahun, JsonRequestBehavior.AllowGet);

            //if (string.IsNullOrEmpty(Id))
            //{
            //    return RedirectToAction("RealisasiLayanan");
            //}
            Models.PenerimaanModel model = new Models.PenerimaanModel();
            List<Entities.RealisasiPenerimaanDetail> data = model.GetRealisasiPenerimaanDetail(Id, pTahun, pBulan);
            //return Json(data, JsonRequestBehavior.AllowGet);

            ViewData["data"] = data;
            ViewData["tahun"] = pTahun;
            return PartialView("RealisasiPenerimaanDetail", data);
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
        public ActionResult RealisasiLayanan()
        {
            List<Entities.Wilayah> listPropinsi = new List<Entities.Wilayah>();
            var ctx = new PnbpContext();

            var get_propinsi = ctx.Database.SqlQuery<Entities.propinsi>("select kantorId, kode, replace(nama,'Kantor Wilayah ', '') as nama from kantor where tipekantorid=2").ToList();
            //ViewData["get_propinsi"] = get_propinsi;

            return View();
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
            List<Entities.RealisasiLayananDetail> data = model.GetRealisasiLayananDetail(Id, pTahun, pBulan);
            //return Json(data, JsonRequestBehavior.AllowGet);
            ViewData["tahun"] = pTahun;
            ViewData["bulan"] = pBulan;

            return PartialView("RealisasiLayananDetail", data);
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
    }
}