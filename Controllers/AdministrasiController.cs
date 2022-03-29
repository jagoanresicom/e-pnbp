using ClosedXML.Excel;
using System;
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

namespace Pnbp.Controllers
{
    [AccessDeniedAuthorize]
    public class AdministrasiController : Controller
    {
        private static Models.KontentModel kontentm = new Models.KontentModel();
        private static Models.AdmModel admModel = new Models.AdmModel();

        private string ConstructViewString(string viewName, object objModel, Dictionary<string, object> addViewData)
        {
            string strView = "";

            using (var sw = new StringWriter())
            {
                ViewData.Model = objModel;
                if (addViewData != null)
                {
                    foreach (string ky in addViewData.Keys)
                    {
                        ViewData[ky] = addViewData[ky];
                    }
                }

                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewCtx = new ViewContext(this.ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewCtx, sw);
                viewResult.ViewEngine.ReleaseView(this.ControllerContext, viewResult.View);
                strView = sw.ToString();
            }

            return strView;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Kantor()
        {
            Models.AdmModel _adm = new Models.AdmModel();
            List<Entities.SatkerAlokasi> _lsSatkerALokasi = _adm.GetSatkerAlokasi();
            return View(_lsSatkerALokasi);
        }

        public ActionResult ShowSatkerStatus(string s)
        {
            Models.AdmModel _adm = new Models.AdmModel();
            Entities.SatkerAlokasi _lsSatkerALokasi = _adm.GetStatusSatker(s);
            if (_lsSatkerALokasi != null)
            {
                _lsSatkerALokasi.Anggaransatker = (_lsSatkerALokasi.Nilaianggaran > 0) ? string.Format("{0:#,##0,##}", _lsSatkerALokasi.Nilaianggaran) : "0";
                _lsSatkerALokasi.Alokasisatker = (_lsSatkerALokasi.Nilaialokasi > 0) ? string.Format("{0:#,##0,##}", _lsSatkerALokasi.Nilaialokasi) : "0";
            }
            return View(_lsSatkerALokasi);
        }

        public ActionResult UpdateStatus(Entities.SatkerAlokasi frm)
        {
            var result = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };
            List<Entities.SatkerAlokasi> _lsSatkerALokasi = new List<Entities.SatkerAlokasi>();
            Models.AdmModel _adm = new Models.AdmModel();
            try
            {
                result = _adm.UpdateSatker(frm.Kantorid, frm.Statusaktif);
                _lsSatkerALokasi = _adm.GetSatkerAlokasi();
                string _strlist = ConstructViewString("ListKantor", _lsSatkerALokasi, null);
                result.Pesan = _strlist;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Pesan = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Prioritas()
        {
            Models.AdmModel _adm = new Models.AdmModel();
            List<Entities.PrioritasAlokasi> _lsSatkerALokasi = _adm.GetPrioritasAlokasi();
            return View(_lsSatkerALokasi);
        }

        public ActionResult PrioritasManfaat(string Id)
        {
            if (String.IsNullOrEmpty(Id))
            {
                return RedirectToAction("Prioritas");
            }

            Models.AdmModel _adm = new Models.AdmModel();
            List<Entities.PrioritasAlokasi> _lsSatkerALokasi = _adm.GetPrioritasManfaat(Id);
            return PartialView("PrioritasDetail", _lsSatkerALokasi);
        }

        public ActionResult ShowPrioritas(string s)
        {
            Models.AdmModel _adm = new Models.AdmModel();
            Entities.PrioritasAlokasi _lsSatkerALokasi = _adm.GetPrioritasSatker(s);
            _lsSatkerALokasi.PrioritasOrigin = _lsSatkerALokasi.Prioritaskegiatan;
            if (_lsSatkerALokasi != null)
            {
                _lsSatkerALokasi.Anggaransatker = (_lsSatkerALokasi.Nilaianggaran > 0) ? string.Format("{0:#,##0,##}", _lsSatkerALokasi.Nilaianggaran) : "0";
                _lsSatkerALokasi.Alokasisatker = (_lsSatkerALokasi.Teralokasi > 0) ? string.Format("{0:#,##0,##}", _lsSatkerALokasi.Teralokasi) : "0";
            }
            return View(_lsSatkerALokasi);
        }

        public ActionResult UpdatePrioritas(Entities.PrioritasAlokasi frm)
        {
            var result = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };
            List<Entities.PrioritasAlokasi> _lsSatkerALokasi = new List<Entities.PrioritasAlokasi>();
            Models.AdmModel _adm = new Models.AdmModel();
            try
            {
                if (!Pnbp.Models.AdmModel.GetStatusKantorbyManfaat(frm.Manfaatid))
                {
                    throw new Exception("Satker tidak aktif!");
                } 
                result = _adm.UpdatePrioritas(frm.Manfaatid, frm.Prioritaskegiatan, frm.PrioritasOrigin, frm.Statusaktif);
                _lsSatkerALokasi = _adm.GetPrioritasManfaat(frm.PrioritasOrigin);
                if(!result.Status)
                {
                    string _strlist = ConstructViewString("PrioritasDetail", _lsSatkerALokasi, null);
                    result.Pesan = _strlist;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Pesan = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateDataManfaat(Entities.PrioritasAlokasi frm)
        {
            var result = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };
            List<Entities.PrioritasAlokasi> _lsSatkerALokasi = new List<Entities.PrioritasAlokasi>();
            Models.AdmModel _adm = new Models.AdmModel();
            try
            {
                if (!Pnbp.Models.AdmModel.GetStatusKantorbyManfaat(frm.Manfaatid))
                {
                    throw new Exception("Satker tidak aktif!");
                }

                #region Validasi Jumlah Anggaran

                //decimal totalAnggaran = Convert.ToDecimal(frm.Anggaransatker.Replace(".", ""));
                decimal totalAnggaran = frm.Nilaianggaran;
                decimal checkTotalAnggaran = Convert.ToDecimal(frm.AnggJan + frm.AnggFeb + frm.AnggMar + frm.AnggApr + frm.AnggMei + frm.AnggJun + frm.AnggJul + frm.AnggAgt + frm.AnggSep + frm.AnggOkt + frm.AnggNov + frm.AnggDes);
                if (totalAnggaran != checkTotalAnggaran)
                {
                    throw new Exception("Jumlah Anggaran yang diinput tidak valid, yaitu: " + string.Format("{0:#,##0}", totalAnggaran) + " <> " + string.Format("{0:#,##0}", checkTotalAnggaran));
                }

                #endregion

                var mfile = Request.Files["filepdf"];
                
                if (mfile != null && mfile.ContentType == "application/pdf")
                {
                    string kantorid = (User.Identity as Entities.InternalUserIdentity).KantorId;
                    
                    string petugas = (User.Identity as Entities.InternalUserIdentity).NamaPegawai;
                    string judul = frm.Namaprogram + " - " + frm.Namasatker;

                    int versi = 0;
                    string id = frm.Manfaatid;
                    
                    if (kontentm.JumlahKonten(id) > 0)
                    {
                        versi = kontentm.CekVersi(id) + 1;
                    }

                    var reqmessage = new HttpRequestMessage();
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(kantorid), "kantorId");
                    content.Add(new StringContent("Manfaat"), "tipeDokumen");
                    content.Add(new StringContent(id), "dokumenId");
                    content.Add(new StringContent(versi.ToString()), "versionNumber");
                    content.Add(new StreamContent(mfile.InputStream), "file", mfile.FileName);

                    reqmessage.Method = HttpMethod.Post;
                    reqmessage.Content = content;
                    reqmessage.RequestUri = new System.Uri(string.Concat(ConfigurationManager.AppSettings["ServiceBaseUrl"].ToString(), "Store"));

                    //using (var client = new HttpClient())
                    //{
                    //    var reqresult = client.SendAsync(reqmessage).Result;
                    //    result.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK;
                    //    result.Pesan = reqresult.ReasonPhrase;
                    //}

                    result = kontentm.SimpanKontenFile(kantorid, id, judul, petugas, DateTime.Now.ToShortDateString(), "Manfaat", out versi);
                    //return Json(result, JsonRequestBehavior.AllowGet);
                }

                result = _adm.UpdateDataManfaat(frm);
                
                //_lsSatkerALokasi = _adm.GetPrioritasManfaat(frm.PrioritasOrigin);
                //if (!result.Status)
                //{
                //    string _strlist = ConstructViewString("PrioritasDetail", _lsSatkerALokasi, null);
                //    result.Pesan = _strlist;
                //}
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Pesan = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetFileManfaat(string id)
        {
            var result = new { Status = false, Message = "" };

            if (!String.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();

                string kantorid = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
                string tipe = "Manfaat";
                string versi = kontentm.CekVersi(id).ToString();

                content.Add(new StringContent(kantorid), "kantorId");
                content.Add(new StringContent(tipe), "tipeDokumen");
                content.Add(new StringContent(id), "dokumenId");
                content.Add(new StringContent(".pdf"), "fileExtension");
                content.Add(new StringContent(versi), "versionNumber");

                reqmessage.Method = HttpMethod.Post;
                reqmessage.Content = content;
                reqmessage.RequestUri = new System.Uri(string.Concat(ConfigurationManager.AppSettings["ServiceBaseUrl"].ToString(), "Retrieve"));

                try
                {
                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var strm = await reqresult.Content.ReadAsStreamAsync();
                            var docfile = new FileStreamResult(strm, MediaTypeNames.Application.Pdf);
                            docfile.FileDownloadName = String.Concat(tipe, ".pdf");
                            return docfile;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = new { Status = false, Message = ex.Message };
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult ExportPenerimaan(string pTahun)
        {
            //pTahun = "2019";
            //return Json(pTahun, JsonRequestBehavior.AllowGet);
            //int result = 1;
            {
                string kantorid = (User as Entities.InternalUserIdentity).KantorId;
                string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

                PnbpContext db = new PnbpContext();
                Pnbp.Models.AdmModel _pm = new Models.AdmModel();
                Entities.FilterManajemenData _frm = new Entities.FilterManajemenData();
                _frm.tahun = (!string.IsNullOrEmpty(pTahun)) ? pTahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
                DataTable dt = new DataTable(pTahun);
                dt.Columns.AddRange(new DataColumn[17] {
                new DataColumn("No",typeof(int)),
                new DataColumn("Kode_Satker"),
                new DataColumn("Nama_Kantor"),
                new DataColumn("Nama_Prosedur"),
                new DataColumn("Nomor_Berkas"),
                new DataColumn("Tahun_Berkas"),
                new DataColumn("Jenis_Penerimaan"),
                new DataColumn("Tanggal"),
                new DataColumn("Kode_Penerimaan"),
                new DataColumn("Bank_Persepsi_ID"),
                new DataColumn("Tahun"),
                new DataColumn("Bulan"),
                new DataColumn("Kode_Billing"),
                new DataColumn("NTPN"),
                new DataColumn("Jumlah"),
                new DataColumn("Penerimaan",typeof(decimal)),
                new DataColumn("Operasional",typeof(decimal)) });

                //List<Entities.Penerimaan> result = _pm.GetPenerimaanNasional(pTahun);
                string query =
                  @"
                        SELECT DISTINCT
	                        TANGGAL,
	                        KODESATKER,
	                        NAMAKANTOR,
	                        NAMAPROSEDUR,
                            NOMORBERKAS,
                            TAHUNBERKAS,
	                        JENISPENERIMAAN,
	                        KODEPENERIMAAN,
	                        BANKPERSEPSIID,
	                        TAHUN,
	                        BULAN,
	                        KODEBILLING,
	                        NTPN,
	                        JUMLAH,
	                        PENERIMAAN,
	                        OPERASIONAL,
	                        row_number() over (ORDER BY KODESATKER ASC) AS URUTAN
                        FROM
	                        REKAPPENERIMAANDETAIL 
                        WHERE
	                        TAHUN = '" + pTahun + "' AND ROWNUM <= 100000 ORDER BY URUTAN ASC";


                var get = db.Database.SqlQuery<Entities.PenerimaanNTPN>(query).ToList();
                //return Json(pTahun, JsonRequestBehavior.AllowGet);

                foreach (var rw in get)
                {
                    dt.Rows.Add(rw.urutan, rw.kodesatker, rw.namakantor, rw.namaprosedur, rw.nomorberkas, rw.tahunberkas, rw.jenispenerimaan, rw.tanggal, rw.kodepenerimaan, rw.bankpersepsiid, rw.tahun, rw.bulan, rw.kodebilling, rw.ntpn, rw.jumlah, rw.penerimaan, rw.operasional);
                }

                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Penerimaan" + DateTime.Now.ToString("dd-mm-yyyy") + ".xlsx");
                    }
                }
            }
        }

        [Authorize]
        public ActionResult ExportRenaksi(string pTahun)
        {
            //pTahun = "2018";
            //int result = 1;
            //return Json(pTahun, JsonRequestBehavior.AllowGet);
            {
                string kantorid = (User as Entities.InternalUserIdentity).KantorId;
                string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

                //var ctx = new PnbpContext();
                //var getRenaksi = ctx.Database.SqlQuery<Entities.Renaksi>("SELECT MANFAATID,KODESATKER,NAMAKANTOR,TAHUN FROM MANFAAT WHERE TAHUN = 2018");
                //ViewData["getRenaksi"] = getRenaksi;
                //return Json(getRenaksi, JsonRequestBehavior.AllowGet);

                PnbpContext db = new PnbpContext();
                Pnbp.Models.AdmModel _pm = new Models.AdmModel();
                Entities.FilterManajemenData _frm = new Entities.FilterManajemenData();
                _frm.tahun = (!string.IsNullOrEmpty(pTahun)) ? pTahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
                DataTable dt = new DataTable(pTahun);
                dt.Columns.AddRange(new DataColumn[64] {
                //new DataColumn("No",typeof(int)),
                new DataColumn("ManfaatId"),
                new DataColumn("Tahun",typeof(int)),
                new DataColumn("KantorId"),
                new DataColumn("Nama_Kantor"),
                new DataColumn("ProgramId"),
                new DataColumn("Nama_Program"),
                new DataColumn("tipe"),
                new DataColumn("PrioritasKegiatan"),
                new DataColumn("Nilai_Anggaran"),
                new DataColumn("Nilai_Alokasi"),
                new DataColumn("StatusFullAlokasi"),
                new DataColumn("StatusEdit"),
                new DataColumn("StatusAktif"),
                new DataColumn("UserInsert"),
                new DataColumn("InsertDate"),
                new DataColumn("UserUpdate"),
                new DataColumn("LastUpdate"),
                new DataColumn("TotalGroup"),
                new DataColumn("PersenGroup"),
                new DataColumn("PersenProgram"),
                new DataColumn("Total_Alokasi"),
                new DataColumn("Sisa_Alokasi"),
                new DataColumn("kode_satker"),
                new DataColumn("PrioritasAsal"),
                new DataColumn("AnggJan"),
                new DataColumn("RankJan"),
                new DataColumn("AnggFeb"),
                new DataColumn("RankFeb"),
                new DataColumn("AnggMar"),
                new DataColumn("RankMar"),
                new DataColumn("AnggApr"),
                new DataColumn("RankApr"),
                new DataColumn("AnggMei"),
                new DataColumn("RankMei"),
                new DataColumn("AnggJun"),
                new DataColumn("RankJun"),
                new DataColumn("AnggJul"),
                new DataColumn("RankJul"),
                new DataColumn("AnggAgt"),
                new DataColumn("RankAgt"),
                new DataColumn("AnggSep"),
                new DataColumn("RankSep"),
                new DataColumn("AnggOkt"),
                new DataColumn("RankOkt"),
                new DataColumn("AnggNov"),
                new DataColumn("RankNov"),
                new DataColumn("AnggDes"),
                new DataColumn("RankDes"),
                new DataColumn("AlokJan"),
                new DataColumn("AlokFeb"),
                new DataColumn("AlokMar"),
                new DataColumn("AlokApr"),
                new DataColumn("AlokMei"),
                new DataColumn("AlokJun"),
                new DataColumn("AlokJul"),
                new DataColumn("AlokAgt"),
                new DataColumn("AlokSep"),
                new DataColumn("AlokOkt"),
                new DataColumn("AlokNov"),
                new DataColumn("AlokDes"),
                new DataColumn("Kode"),
                new DataColumn("StatusRevisi"),
                new DataColumn("Persetujuan 1"),
                new DataColumn("Persetujuan 2"),
                });

                //List<Entities.Renaksi> result = _pm.GetRenaksi(pTahun);
                string query =
                @"SELECT DISTINCT * FROM MANFAAT WHERE TAHUN = "+ pTahun +" ORDER BY TIPE, RANKJAN ASC";

                var get = db.Database.SqlQuery<Entities.Renaksi>(query).ToList();

                ////return Json(pTahun, JsonRequestBehavior.AllowGet);

                foreach (var rw in get)
                {
                    dt.Rows.Add(rw.manfaatid, rw.tahun, rw.kantorid, rw.namakantor, rw.programid, rw.namaprogram, rw.tipe, rw.prioritaskegiatan, rw.nilaianggaran, rw.nilaialokasi, rw.statusfullalokasi, rw.statusedit, rw.statusaktif, rw.userinsert, rw.insertdate, rw.userupdate, rw.lastupdate, rw.totalgroup, rw.persengroup, rw.persenprogram, rw.totalalokasi, rw.sisaalokasi, rw.kodesatker, rw.prioritasasal, rw.anggjan, rw.rankjan, rw.anggfeb, rw.rankfeb, rw.anggmar, rw.rankmar, rw.anggapr, rw.rankapr, rw.anggmei, rw.rankmei, rw.anggjun, rw.rankjun, rw.anggjul, rw.rankjul, rw.anggagt, rw.rankagt, rw.anggsep, rw.ranksep, rw.anggokt, rw.rankokt, rw.anggnov, rw.ranknov, rw.anggdes, rw.rankdes, rw.alokjan, rw.alokfeb, rw.alokmar, rw.alokapr, rw.alokmei, rw.alokjun, rw.alokjul, rw.alokagt, rw.aloksep, rw.alokokt, rw.aloknov, rw.alokdes, rw.kode, rw.statusrevisi, rw.persetujuan1, rw.persetujuan2);
                }

                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Rencana_Aksi" + DateTime.Now.ToString("dd-mm-yyyy") + ".xlsx");
                    }
                }
            }
        }

        [Authorize]
        public ActionResult Exportntpn(string pTahun, string pBulan)
        {
            //pBulan = "2016";
            //int result = 1;
            {
                string kantorid = (User as Entities.InternalUserIdentity).KantorId;
                string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

                PnbpContext db = new PnbpContext();
                Pnbp.Models.AdmModel _pm = new Models.AdmModel();
                Entities.FilterManajemenData _frm = new Entities.FilterManajemenData();
                _frm.tahun = (!string.IsNullOrEmpty(pBulan)) ? pBulan : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
                DataTable dt = new DataTable(pBulan);
                dt.Columns.AddRange(new DataColumn[17] {
                new DataColumn("No",typeof(int)),
                new DataColumn("Kode_Satker"),
                new DataColumn("Nama_Kantor"),
                new DataColumn("Nama_Prosedur"),
                new DataColumn("Nomor_Berkas"),
                new DataColumn("Tahun_Berkas"),
                new DataColumn("Jenis_Penerimaan"),
                new DataColumn("Tanggal"),
                new DataColumn("Kode_Penerimaan"),
                new DataColumn("Bank_Persepsi_ID"),
                new DataColumn("Tahun"),
                new DataColumn("Bulan"),
                new DataColumn("Kode_Billing"),
                new DataColumn("NTPN"),
                new DataColumn("Jumlah"),
                new DataColumn("Penerimaan",typeof(decimal)),
                new DataColumn("Operasional",typeof(decimal)) });

                //List<Entities.Penerimaan> result = _pm.GetPenerimaanNasional(pTahun, _frm.bulan, tipekantorid, kantorid);
                string query =
                   @"
                        SELECT DISTINCT
	                        TANGGAL,
	                        KODESATKER,
	                        NAMAKANTOR,
	                        NAMAPROSEDUR,
                            NOMORBERKAS,
                            TAHUNBERKAS,
	                        JENISPENERIMAAN,
	                        KODEPENERIMAAN,
	                        BANKPERSEPSIID,
	                        TAHUN,
	                        BULAN,
	                        KODEBILLING,
	                        NTPN,
	                        JUMLAH,
	                        PENERIMAAN,
	                        OPERASIONAL,
	                        row_number() over (ORDER BY KODESATKER ASC) AS URUTAN
                        FROM
	                        REKAPPENERIMAANDETAIL 
                        WHERE
	                        TAHUN = '" + pTahun + "' AND BULAN = '" + pBulan + "' AND ROWNUM <= 100000";

                //query += " group by BULAN, JUMLAHBERKAS ";
                var get = db.Database.SqlQuery<Entities.PenerimaanNTPN>(query).ToList();
                //return Json(new {pTahun= pTahun, pBulan= pBulan}, JsonRequestBehavior.AllowGet);
                //return Json(pTahun, JsonRequestBehavior.AllowGet);

                foreach (var rw in get)
                {
                    dt.Rows.Add(rw.urutan, rw.kodesatker, rw.namakantor, rw.namaprosedur, rw.nomorberkas, rw.jenispenerimaan, rw.tanggal, rw.kodepenerimaan, rw.bankpersepsiid, rw.kodebilling, rw.ntpn, rw.jumlah, rw.penerimaan, rw.operasional);
                }

                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "NTPN" + DateTime.Now.ToString("yyyy-mm-dd") + ".xlsx");
                    }
                }
                //return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult ManajemenData()
        { 
            return View();
        }

        public ActionResult ListTargetHistory(int? start, int? length)
        {
            //List
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;
            //return Json(to, JsonRequestBehavior.AllowGet);

            decimal? total = 0;
            List<Entities.TargetPnbpHistory> result = admModel.GetTargetPnbp(from, to);


            if (result.Count > 0)
            {
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult ManajemenData(FormCollection form)
        {
            PnbpContext db = new PnbpContext();
            var file_id = GetSequence("UPLOAD_FILE");

            var file_name1 = ((form.AllKeys.Contains("file_name1")) ? form["file_name1"] : "NULL");
            var fileContent = Request.Files["file_name1"];

            //Insert Target PNBP
            if (fileContent != null && fileContent.ContentLength > 0)
            {
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent.InputStream;
                var FileSizeByte = fileContent.ContentLength;
                var FileSize = FileSizeByte / 1000;
                var Extension = System.IO.Path.GetExtension(fileContent.FileName);
                var fileName = "TargetPNBPTES1_" + tgl + "" + Extension;

                string folderPath = Server.MapPath("~/Uploads/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }
                var filepathori = "/Uploads/" + fileContent.FileName;
                //return Json(filepathori, JsonRequestBehavior.AllowGet);

                var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);
                var Filefilepath = "/Uploads/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name1 = fileName;
                }
                string insert_target = "INSERT INTO UPLOAD_FILE (FILE_ID, FILE_NAME_TARGET, FILE_PATH_TARGET, FILE_CREATE_DATE) " +
                                           "VALUES ('" + file_id + "','" + file_name1 + "','" + Filefilepath + "', SYSDATE)";
                db.Database.ExecuteSqlCommand(insert_target);
                //return Json(file_name1, JsonRequestBehavior.AllowGet);
            }

            //Insert Pagu Belanja PNBP
            //var file_name2 = ((form.AllKeys.Contains("file_name2")) ? form["file_name2"] : "NULL");
            //var fileContent2 = Request.Files["file_name2"];

            //if (fileContent2 != null && fileContent2.ContentLength > 0)
            //{
            //    var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
            //    var stream = fileContent2.InputStream;
            //    var FileSizeByte = fileContent2.ContentLength;
            //    var FileSize = FileSizeByte / 1000;
            //    var Extension = System.IO.Path.GetExtension(fileContent2.FileName);
            //    var fileName = "Pagu_Belanja_PNBP_" + tgl + "" + Extension;
            //    string folderPath = Server.MapPath("~/Uploads/");
            //    //Check whether Directory (Folder) exists.
            //    if (!Directory.Exists(folderPath))
            //    {
            //        //If Dir3ectory (Folder) does not exists. Create it.
            //        Directory.CreateDirectory(folderPath);
            //    }

            //    var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);
            //    var Filefilepath = "/Uploads/" + fileName;
            //    using (var fileStream = System.IO.File.Create(path))
            //    {
            //        stream.CopyTo(fileStream);
            //        file_name2 = fileName;
            //    }
            //}

            //Insert Realisasi Belanja PNBP
            //var file_name3 = ((form.AllKeys.Contains("file_name3")) ? form["file_name3"] : "NULL");
            //var fileContent3 = Request.Files["file_name3"];

            //if (fileContent3 != null && fileContent3.ContentLength > 0)
            //{
            //    var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
            //    var stream = fileContent3.InputStream;
            //    var FileSizeByte = fileContent3.ContentLength;
            //    var FileSize = FileSizeByte / 1000;
            //    var Extension = System.IO.Path.GetExtension(fileContent3.FileName);
            //    var fileName = "Realisasi_Belanja_PNBP_" + tgl + "" + Extension;
            //    string folderPath = Server.MapPath("~/Uploads/");
            //    //Check whether Directory (Folder) exists.
            //    if (!Directory.Exists(folderPath))
            //    {
            //        //If Dir3ectory (Folder) does not exists. Create it.
            //        Directory.CreateDirectory(folderPath);
            //    }

            //    var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);
            //    var Filefilepath = "/Uploads/" + fileName;
            //    using (var fileStream = System.IO.File.Create(path))
            //    {
            //        stream.CopyTo(fileStream);
            //        file_name3 = fileName;
            //    }
            //}

            //string insert_target = "INSERT INTO UPLOAD_FILE (FILE_ID, FILE_NAME_TARGET, FILE_PATH_TARGET, FILE_NAME_PAGU_BELANJA, FILE_PATH_PAGU_BELANJA, FILE_NAME_REALISASI_BELANJA, FILE_PATH_REALISASI_BELANJA) " +
            //                                "VALUES ('" + file_id + "','" + file_name1 + "','" + file_name1 + "','" + file_name2 + "','" + file_name2 + "','" +file_name3+ "','" + file_name3 + "')";

            //return Json(insert_target, JsonRequestBehavior.AllowGet);
            if (ModelState.IsValid)
            {
                TempData["Upload"] = "Data Berhasil Diunggah";
                return RedirectToAction("ManajemenData");
            }
            else
            {

            }
            return RedirectToAction("ManajemenData");

        }

        public static int GetSequence(String table_)
        {
            using (var db = new PnbpContext())
            {
                int id = db.Database.SqlQuery<int>("SELECT " + table_ + "_SEQ.NEXTVAL FROM DUAL").SingleOrDefault();
                return id;
            }
        }

        //[HttpPost]
        //mati
        //public ActionResult ManajemenData(HttpPostedFileBase[] files)
        //{
        //    Pnbp.Entities.TransactionResult tr = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };

        //    //Ensure model state is valid  
        //    if (ModelState.IsValid)
        //    {   //iterating through multiple file collection   
        //        foreach (HttpPostedFileBase file in files)
        //        {
        //            //Checking file is available to save.  
        //            if (file != null)
        //            {
        //                var InputFileName = Path.GetFileName(file.FileName);
        //                var ServerSavePath = Path.Combine(Server.MapPath("~/UploadedFiles/") + InputFileName);
        //                //Save file to server folder  
        //                file.SaveAs(ServerSavePath);
        //                //assigning file uploaded status to ViewBag for showing message to user.  
        //                ViewBag.UploadStatus = files.Count().ToString() + " files uploaded successfully.";

        //                // save to db
        //                var model = new Pnbp.Entities.File();
        //                model.file_id = "";
        //                model.file_name = InputFileName;
        //                model.file_path = ServerSavePath;

        //                tr = admModel.SimpanFile(model);
        //            }

        //        }
        //    }

        //    return View();
        //}        

            #region InformasiBerkas
        public ActionResult InformasiBerkas()
        {
            return View("InformasiBerkas");
        }

        public ActionResult ShowInformasiBerkas(Entities.QueryInformasiBerkas query)
        {
            Models.PenerimaanModel lm = new Models.PenerimaanModel();
            List<Entities.InformasiBerkas> _dtInfo = new List<Entities.InformasiBerkas>();
            List<Entities.berkaspenerimaanls> _dtBerkas = lm.dtBerkasPenerimaan(query.kodebilling, query.ntpn);
            foreach (Entities.berkaspenerimaanls item in _dtBerkas)
            {
                List<Entities.statusberkasls> _dtStatus = lm.dtStatusBerkas(item.berkasid);
                _dtInfo.Add(new Entities.InformasiBerkas() { berkasid = item.berkasid, ntpn = item.ntpn, kodebilling = item.kodebilling, tglpenerimaan = item.tglpenerimaan, namakantor = item.namakantor, namaprosedur = item.namaprosedur, namawajibbayar = item.namawajibbayar, totalbiaya = item.totalbiaya, detailpenerimaan = item.detailpenerimaan, nomor = _dtStatus[0].nomor, tahun = _dtStatus[0].tahun, nomorkanwil = _dtStatus[0].nomorkanwil, tahunkanwil = _dtStatus[0].tahunkanwil, nomorpusat = _dtStatus[0].nomorpusat, tahunpusat = _dtStatus[0].tahunpusat, tipekantor = _dtStatus[0].tipekantor, kantor = _dtStatus[0].kantor, tipekantorwilayah = _dtStatus[0].tipekantorwilayah, kantorwilayah = _dtStatus[0].kantorwilayah, tipekantortujuan = _dtStatus[0].tipekantortujuan, kantortujuan = _dtStatus[0].kantortujuan, statusberkas = _dtStatus[0].statusberkas });
            }
            return PartialView("InformasiBerkasPartial", _dtInfo);
        }
        #endregion


        #region Pengumuman

        //[AccessDeniedAuthorize(Roles = "Administrator")]
        public ActionResult Pengumuman()
        {
            return View();
        }

        public ActionResult DaftarPengumuman(int? pageNum, Pnbp.Entities.FindPengumuman f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 10;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string judulBerita = f.JudulBerita;
            string isiBerita = f.IsiBerita;
            string tanggalMulai = f.TanggalMulai;
            string tanggalBerakhir = f.TanggalBerakhir;

            List<Pnbp.Entities.Pengumuman> result = admModel.GetPengumuman(judulBerita, isiBerita, tanggalMulai, tanggalBerakhir, from, to);

            int custIndex = from;
            Dictionary<int, Pnbp.Entities.Pengumuman> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarPengumuman", dict);
                }
                else
                {
                    return RedirectToAction("Pengumuman", "Administrasi");
                }
            }
            else
            {
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = "noresults",
                    ContentEncoding = System.Text.Encoding.UTF8
                };
            }
        }

        public ActionResult BuatBaru()
        {
            Pnbp.Entities.Pengumuman pengumuman = new Pnbp.Entities.Pengumuman();

            if (Request.IsAjaxRequest())
            {
                return PartialView("EntriPengumuman", pengumuman);
            }
            else
            {
                return RedirectToAction("Pengumuman", "Administrasi");
            }
        }

        public ActionResult EntriPengumuman(string id, string rencanapenggunaan)
        {
            if (!String.IsNullOrEmpty(id))
            {
                // Edit Mode
                Pnbp.Entities.Pengumuman pengumuman = admModel.GetPengumumanById(id);

                if (Request.IsAjaxRequest())
                {
                    return PartialView("EntriPengumuman", pengumuman);
                }
                else
                {
                    return RedirectToAction("Pengumuman", "Administrasi");
                }
            }
            else
            {
                return RedirectToAction("Pengumuman", "Administrasi");
            }
        }

        [HttpPost]
        public JsonResult SimpanPengumuman(Pnbp.Entities.Pengumuman pengumuman)
        {
            Pnbp.Entities.TransactionResult tr = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };

            // Cek duplikat dokumen
            if (String.IsNullOrEmpty(pengumuman.BeritaAppId))
            {
                string msg = string.Empty;
                int cekrow = admModel.JumlahPengumuman(pengumuman.JudulBerita);
                if (cekrow > 0)
                {
                    msg = String.Concat("Pengumuman ", pengumuman.JudulBerita.ToUpper(), " sudah ada.");
                    return Json(new { Status = false, Pesan = msg }, JsonRequestBehavior.AllowGet);
                }
            }

            tr = admModel.SimpanPengumuman(pengumuman);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusPengumuman()
        {
            var result = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string id = Request.Form["id"].ToString();
                if (!String.IsNullOrEmpty(id))
                {
                    string userid = (User.Identity as Pnbp.Entities.InternalUserIdentity).UserId;
                    result = admModel.HapusPengumuman(id, userid);
                    if (!result.Status)
                    {
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Pesan = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ContentResult GetPengumumanText()
        {
            string result = admModel.GetPengumumanText();

            return Content(result);
        }

        public ActionResult PengumumanViewer()
        {
            return PartialView("PengumumanViewer");
        }

        #endregion


        #region Akses Edit Pagu

        public ActionResult AksesEditPagu()
        {
            return View();
        }

        public ContentResult GetAksesEditPagu()
        {
            string result = admModel.GetAksesEditPagu();

            return Content(result);
        }

        [HttpPost]
        public JsonResult SimpanAksesEditPagu(Pnbp.Entities.Konfigurasi konfigurasi)
        {
            Pnbp.Entities.TransactionResult tr = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };

            tr = admModel.SimpanAksesEditPagu(konfigurasi);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Rincian Biaya
        public ActionResult RincianBiaya()
        {
            var ctx = new PnbpContext();
            var get_data = ctx.Database.SqlQuery<Entities.RincianBiaya>("SELECT DISTINCT tarif, nama, nilaioperasional, tipenilaioperasional, kodepenerimaan FROM KKPWEB.biaya WHERE NILAIOPERASIONAL IS NOT NULL").ToList();
            ViewData["get_data"] = get_data;
            //return Json(get_data, JsonRequestBehavior.AllowGet);
            return View();
        }
        #endregion


        public ActionResult get_belanja_span()
        {
            return View();
        }

        public ActionResult ResetSpanBelanja()
        {
            PnbpContext db = new PnbpContext();
            string truncate_span = "TRUNCATE TABLE SPAN_BELANJA";
            db.Database.ExecuteSqlCommand(truncate_span);
            return Json(1, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateRenaksiBelanja(FormCollection form)
        {
            PnbpContext db = new PnbpContext();

            var KDSATKER = ((form.AllKeys.Contains("KDSATKER")) ? form["KDSATKER"] : "");
            var KPPN = ((form.AllKeys.Contains("KPPN")) ? form["KPPN"] : "");
            var BA = ((form.AllKeys.Contains("BA")) ? form["BA"] : "");
            var BAES1 = ((form.AllKeys.Contains("BAES1")) ? form["BAES1"] : "");
            var AKUN = ((form.AllKeys.Contains("AKUN")) ? form["AKUN"] : "");
            var PROGRAM = ((form.AllKeys.Contains("PROGRAM")) ? form["PROGRAM"] : "");
            var KEGIATAN = ((form.AllKeys.Contains("KEGIATAN")) ? form["KEGIATAN"] : "");
            var OUTPUT = ((form.AllKeys.Contains("OUTPUT")) ? form["OUTPUT"] : "");
            var KEWENANGAN = ((form.AllKeys.Contains("KEWENANGAN")) ? form["KEWENANGAN"] : "");
            var SUMBER_DANA = ((form.AllKeys.Contains("SUMBER_DANA")) ? form["SUMBER_DANA"] : "");
            var CARA_TARIK = ((form.AllKeys.Contains("CARA_TARIK")) ? form["CARA_TARIK"] : "");
            var LOKASI = ((form.AllKeys.Contains("LOKASI")) ? form["LOKASI"] : "");
            var BUDGET_TYPE = ((form.AllKeys.Contains("BUDGET_TYPE")) ? form["BUDGET_TYPE"] : "");
            var AMOUNT = ((form.AllKeys.Contains("AMOUNT")) ? form["AMOUNT"] : "");

            string currentYear = DateTime.Now.Year.ToString();
            string insert_belanja = "INSERT INTO SPAN_BELANJA (KDSATKER, KPPN, BA,BAES1,AKUN,PROGRAM,KEGIATAN,OUTPUT,KEWENANGAN,SUMBER_DANA,CARA_TARIK,LOKASI,BUDGET_TYPE,AMOUNT,TAHUN) " +
                                            "VALUES ('" + KDSATKER + "','" + KPPN + "','" + BA + "','" + BAES1 + "','" + AKUN + "','" + PROGRAM + "','" + KEGIATAN + "','" + OUTPUT + "','" + KEWENANGAN + "','" + SUMBER_DANA + "','" + CARA_TARIK + "','" + LOKASI + "','" + BUDGET_TYPE + "'," + AMOUNT + ","+ currentYear + ")";
            db.Database.ExecuteSqlCommand(insert_belanja);
            return Json(form, JsonRequestBehavior.AllowGet);
        }

    }
}