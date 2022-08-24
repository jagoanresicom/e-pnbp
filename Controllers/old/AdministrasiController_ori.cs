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

                    using (var client = new HttpClient())
                    {
                        var reqresult = client.SendAsync(reqmessage).Result;
                        result.Status = reqresult.IsSuccessStatusCode && reqresult.StatusCode == System.Net.HttpStatusCode.OK;
                        result.Pesan = reqresult.ReasonPhrase;
                    }

                    result = kontentm.SimpanKontenFile(kantorid, id, judul, petugas, DateTime.Now.ToShortDateString(), "Manfaat", out versi);
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

    }
}