using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Pnbp.Controllers
{
    //[AccessDeniedAuthorize(Roles = "KepalaUrusanKeuangan")]
    [AccessDeniedAuthorize]
    public class PemanfaatanController : Controller
    {
        private static Models.PemanfaatanModel _manfaatanModel = new Models.PemanfaatanModel();
        private static Models.KontentModel kontentm = new Models.KontentModel();

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

        public ActionResult DataManfaat()
        {
            Entities.DataManfaat _data = new Entities.DataManfaat();
            List<Entities.SatkerAlokasi> _lsSatkerALokasi = _manfaatanModel.GetSatkerAlokasi();
            _data.satkermanfaat = _lsSatkerALokasi;
            var useridentity = (User.Identity as Entities.InternalUserIdentity);
            _data.UserAdmin = _manfaatanModel.UsersInRoles(useridentity.UserId, "Administrator Pusat");
            return View(_data);
        }

        public ContentResult GetTahunBerjalan()
        {
            int tahunberjalan = _manfaatanModel.GetServerYear();

            string result = tahunberjalan.ToString();

            return Content(result);
        }

        public ActionResult ListDataManfaat()
        {
            Entities.FindManfaat find = new Entities.FindManfaat();
            find.tahun = _manfaatanModel.GetServerYear().ToString();
            find.lstahun = _manfaatanModel.ListTahun();

            return View(find);
        }

        public ActionResult ListDataManfaatV2()
        {
            Entities.FindManfaat find = new Entities.FindManfaat();
            find.tahun = _manfaatanModel.GetServerYear().ToString();
            find.lstahun = _manfaatanModel.ListTahun();

            return View(find);
        }

        //public ActionResult DaftarDataManfaatOld(int? draw, int? start, int? length)
        //{
        //    List<Entities.SatkerAlokasi> result = new List<Entities.SatkerAlokasi>();
        //    decimal? total = 0;

        //    int recNumber = start ?? 0;
        //    int RecordsPerPage = length ?? 10;
        //    int from = recNumber + 1;
        //    int to = from + RecordsPerPage - 1;

        //    result = _manfaatanModel.GetDataManfaat(from, to);

        //    if (result.Count > 0)
        //    {
        //        total = result[0].Total;
        //    }

        //    return Json(new { data = result, draw = draw, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetTotalAnggaranAlokasi()
        {
            string tahun = ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            List<Entities.TotalAnggaranAlokasi> listTotalAngAlok = _manfaatanModel.GetTotalAnggaranAlokasi(tahun);
            return Json(listTotalAngAlok, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTotalAnggaranAlokasiV2()
        {
            string tahun = ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            List<Entities.TotalAnggaranAlokasi> listTotalAngAlok = _manfaatanModel.GetTotalAnggaranAlokasiV2(tahun);
            return Json(listTotalAngAlok, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTotalAlokasiSatkerV2(string kodesatker)
        {
            decimal item = _manfaatanModel.GetTotalAlokasiSatkerV2(kodesatker);
            return Json(new { success = true, data = item }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTotalAnggaranDipa()
        {
            string tahun = ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            List<Entities.TotalDipaBelanja> listTotalAngDipa = _manfaatanModel.GetTotalAnggaranDipa(tahun);
            return Json(listTotalAngDipa, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarDataManfaat(int? pageNum, Entities.FindManfaat f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 10;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string namasatker = f.NamaSatker;
            decimal? nilaianggaran = f.NilaiAnggaran;
            //string tahun = ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            string tahun = f.tahun;

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            List<Entities.SatkerAlokasi> result = _manfaatanModel.GetDataManfaat(tahun, namasatker, nilaianggaran, tipekantorid, kantorid, from, to);

            int custIndex = from;
            Dictionary<int, Entities.SatkerAlokasi> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarManfaat", dict);
                }
                else
                {
                    return RedirectToAction("ListDataManfaat", "Pemanfaatan");
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

        public ActionResult DaftarDataManfaatV2(int? pageNum, Entities.FindManfaat f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 10;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string namasatker = f.NamaSatker;
            decimal? nilaianggaran = f.NilaiAnggaran;
            //string tahun = ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            string tahun = f.tahun;

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            List<Entities.SatkerAlokasi> result = _manfaatanModel.GetDataManfaatV2(tahun, namasatker, nilaianggaran, tipekantorid, kantorid, from, to);

            int custIndex = from;
            Dictionary<int, Entities.SatkerAlokasi> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarManfaatV2", dict);
                }
                else
                {
                    return RedirectToAction("ListDataManfaatV2", "Pemanfaatan");
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

        public ActionResult UpdateStatusSatker(string kantorid, bool bolCheck)
        {
            var result = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };
            List<Entities.SatkerAlokasi> _lsSatkerALokasi = new List<Entities.SatkerAlokasi>();
            Models.AdmModel _adm = new Models.AdmModel();
            try
            {
                string _status = "Aktif";
                if (!bolCheck) _status = "Tidak Aktif";
                result = _manfaatanModel.UpdateSatker(kantorid, _status);
                _lsSatkerALokasi = _adm.GetSatkerAlokasi();
                //string _strlist = ConstructViewString("ListKantor", _lsSatkerALokasi, null);
                //result.Pesan = _strlist;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Pesan = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePersetujuanRevisi(string kodesatker, string namaprogram, string userbiroperencanaan, string userbirokeuangan, bool bolCheck)
        {
            var result = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                int statuspersetujuan = (bolCheck) ? 1 : 0;
                result = _manfaatanModel.UpdatePersetujuanRevisi(kodesatker, namaprogram, userbiroperencanaan, userbirokeuangan, statuspersetujuan);
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

        public ActionResult AlokasiPemanfaatan()
        {
            Models.PemanfaatanModel model = new Models.PemanfaatanModel();

            Entities.DataPaguAlokasi dataPaguAlokasi = new Entities.DataPaguAlokasi();

            #region Anggaran Alokasi Nasional

            decimal PaguNonOps = 0, PaguOps = 0, AlokasiOps = 0, AlokasiNonOps = 0,
                TotalPagu = 0, TotalAlokasi = 0, BelumAlokasi = 0, PersentaseAlokasi = 0;
            List<Entities.QueryPaguAlokasiNasional> qPaguAlo = model.GetPaguAlokasiNasional();
            foreach (Entities.QueryPaguAlokasiNasional item in qPaguAlo)
            {
                TotalPagu += item.Pagu;
                TotalAlokasi += item.Alokasi;

                if (item.Tipe == "NONOPS")
                {
                    PaguNonOps = item.Pagu;
                    AlokasiNonOps = item.Alokasi;
                }
                if (item.Tipe == "OPS")
                {
                    PaguOps = item.Pagu;
                    AlokasiOps = item.Alokasi;
                }
            }

            BelumAlokasi = TotalPagu - TotalAlokasi;
            PersentaseAlokasi = (TotalAlokasi / TotalPagu) * 100;

            Entities.PaguAlokasiNasional dataPaguAlokasiNasional = new Entities.PaguAlokasiNasional();
            dataPaguAlokasiNasional.PaguOps = PaguOps;
            dataPaguAlokasiNasional.PaguNonOps = PaguNonOps;
            dataPaguAlokasiNasional.AlokasiOps = AlokasiOps;
            dataPaguAlokasiNasional.AlokasiNonOps = AlokasiNonOps;
            dataPaguAlokasiNasional.Persentase = PersentaseAlokasi;
            dataPaguAlokasiNasional.PersentaseString = string.Format("{0:#,##0.##}", PersentaseAlokasi) + " %";
            dataPaguAlokasiNasional.BelumAlokasi = BelumAlokasi;

            dataPaguAlokasi.paguAlokasiNasional = dataPaguAlokasiNasional;

            #endregion

            #region Anggaran Alokasi Provinsi

            TotalPagu = 0; TotalAlokasi = 0; BelumAlokasi = 0; PersentaseAlokasi = 0;

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            List<Entities.Kantor> qKanwil = model.GetKantorKanwil(tipekantorid, kantorid);

            List<Entities.PaguAlokasiProvinsi> paguAlokasiProvinsi = new List<Entities.PaguAlokasiProvinsi>(qKanwil.Count);

            List<Entities.QueryPaguAlokasiProvinsi> qPaguAloProv = model.GetPaguAlokasiProvinsi();

            // PUSAT
            decimal drownums = 0;

            if (tipekantorid == "1")
            {
                Entities.PaguAlokasiProvinsi dataPaguAloPusat = new Entities.PaguAlokasiProvinsi();
                drownums++;

                dataPaguAloPusat.Rownums = drownums;
                dataPaguAloPusat.NamaProvinsi = "Kantor Pusat";

                //Entities.QueryPaguAlokasiProvinsi paguAloOpsPusat = qPaguAloProv.Find(item => (item.Kode == "00") && (item.Tipe == "OPS"));
                Entities.QueryPaguAlokasiProvinsi paguAloNonOpsPusat = qPaguAloProv.Find(item => (item.Kode == "00") && (item.Tipe == "NONOPS"));

                if (paguAloNonOpsPusat != null)
                {
                    dataPaguAloPusat.PaguNonOps = paguAloNonOpsPusat.Pagu;
                    dataPaguAloPusat.AlokasiNonOps = paguAloNonOpsPusat.Alokasi;

                    TotalPagu += paguAloNonOpsPusat.Pagu;
                    TotalAlokasi += paguAloNonOpsPusat.Alokasi;
                }

                BelumAlokasi = TotalPagu - TotalAlokasi;
                PersentaseAlokasi = (TotalAlokasi / TotalPagu) * 100;

                dataPaguAloPusat.KantorId = "980FECFC746D8C80E0400B0A9214067D";
                dataPaguAloPusat.KodeKantor = "00";
                dataPaguAloPusat.TotalPagu = TotalPagu;
                dataPaguAloPusat.TotalAlokasi = TotalAlokasi;
                dataPaguAloPusat.Persentase = PersentaseAlokasi;
                dataPaguAloPusat.PersentaseString = string.Format("{0:#,##0.##}", PersentaseAlokasi) + " %";
                dataPaguAloPusat.BelumAlokasi = BelumAlokasi;

                paguAlokasiProvinsi.Add(dataPaguAloPusat);
            }

            // KANWIL

            decimal row = drownums + 1;
            foreach (Entities.Kantor kanwil in qKanwil)
            {
                TotalPagu = 0; TotalAlokasi = 0; BelumAlokasi = 0; PersentaseAlokasi = 0;

                Entities.PaguAlokasiProvinsi dataPaguAlo = new Entities.PaguAlokasiProvinsi();
                dataPaguAlo.Rownums = row;
                dataPaguAlo.NamaProvinsi = kanwil.NamaKantor;

                Entities.QueryPaguAlokasiProvinsi paguAloOps = qPaguAloProv.Find(item => (item.Kode == kanwil.KodeKantor) && (item.Tipe == "OPS"));
                Entities.QueryPaguAlokasiProvinsi paguAloNonOps = qPaguAloProv.Find(item => (item.Kode == kanwil.KodeKantor) && (item.Tipe == "NONOPS"));

                if (paguAloOps != null)
                {
                    dataPaguAlo.PaguOps = paguAloOps.Pagu;
                    dataPaguAlo.AlokasiOps = paguAloOps.Alokasi;

                    TotalPagu += paguAloOps.Pagu;
                    TotalAlokasi += paguAloOps.Alokasi;
                }
                if (paguAloNonOps != null)
                {
                    dataPaguAlo.PaguNonOps = paguAloNonOps.Pagu;
                    dataPaguAlo.AlokasiNonOps = paguAloNonOps.Alokasi;

                    TotalPagu += paguAloNonOps.Pagu;
                    TotalAlokasi += paguAloNonOps.Alokasi;
                }

                BelumAlokasi = TotalPagu - TotalAlokasi;
                PersentaseAlokasi = (TotalAlokasi / TotalPagu) * 100;

                dataPaguAlo.KantorId = kanwil.KantorId;
                dataPaguAlo.KodeKantor = kanwil.KodeKantor;
                dataPaguAlo.TotalPagu = TotalPagu;
                dataPaguAlo.TotalAlokasi = TotalAlokasi;
                dataPaguAlo.Persentase = PersentaseAlokasi;
                dataPaguAlo.PersentaseString = string.Format("{0:#,##0.##}", PersentaseAlokasi) + " %";
                dataPaguAlo.BelumAlokasi = BelumAlokasi;

                paguAlokasiProvinsi.Add(dataPaguAlo);

                row++;
            }

            dataPaguAlokasi.ListPaguAlokasiProvinsi = paguAlokasiProvinsi;

            #endregion

            return View(dataPaguAlokasi);
        }

        public ActionResult AlokasiPemanfaatanDetail(string Id, string Nama)
        {
            if (String.IsNullOrEmpty(Id))
            {
                return RedirectToAction("AlokasiPemanfaatan");
            }

            decimal TotalPagu = 0, TotalAlokasi = 0, BelumAlokasi = 0, PersentaseAlokasi = 0;

            Models.PemanfaatanModel model = new Models.PemanfaatanModel();

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            if (tipekantorid == "3" || tipekantorid == "4")
            {
                Id = kantorid;
            }

            List<Entities.Kantor> qSatker = model.GetKantorSatker(Id);

            List<Entities.PaguAlokasiSatker> _lsSatkerALokasi = new List<Entities.PaguAlokasiSatker>(qSatker.Count); // _adm.GetPrioritasManfaat(Id);

            List<Entities.QueryPaguAlokasiSatker> qPaguAloSatker = model.GetPaguAlokasiSatker(Id);

            int row = 1;
            foreach (Entities.Kantor saktker in qSatker)
            {
                TotalPagu = 0; TotalAlokasi = 0; BelumAlokasi = 0; PersentaseAlokasi = 0;

                Entities.PaguAlokasiSatker dataPaguAlo = new Entities.PaguAlokasiSatker();
                dataPaguAlo.Rownums = row;
                dataPaguAlo.KantorId = saktker.KantorId;
                dataPaguAlo.KodeKantor = saktker.KodeKantor;
                dataPaguAlo.NamaSatker = saktker.NamaKantor;

                Entities.QueryPaguAlokasiSatker paguAloOps = qPaguAloSatker.Find(item => (item.Kode == saktker.KodeKantor) && (item.Tipe == "OPS"));
                Entities.QueryPaguAlokasiSatker paguAloNonOps = qPaguAloSatker.Find(item => (item.Kode == saktker.KodeKantor) && (item.Tipe == "NONOPS"));

                if (paguAloOps != null)
                {
                    dataPaguAlo.PaguOps = paguAloOps.Pagu;
                    dataPaguAlo.AlokasiOps = paguAloOps.Alokasi;

                    TotalPagu += paguAloOps.Pagu;
                    TotalAlokasi += paguAloOps.Alokasi;
                }
                if (paguAloNonOps != null)
                {
                    dataPaguAlo.PaguNonOps = paguAloNonOps.Pagu;
                    dataPaguAlo.AlokasiNonOps = paguAloNonOps.Alokasi;

                    dataPaguAlo.AnggJan = paguAloNonOps.AnggJan;
                    dataPaguAlo.AnggFeb = paguAloNonOps.AnggFeb;
                    dataPaguAlo.AnggMar = paguAloNonOps.AnggMar;
                    dataPaguAlo.AnggApr = paguAloNonOps.AnggApr;
                    dataPaguAlo.AnggMei = paguAloNonOps.AnggMei;
                    dataPaguAlo.AnggJun = paguAloNonOps.AnggJun;
                    dataPaguAlo.AnggJul = paguAloNonOps.AnggJul;
                    dataPaguAlo.AnggAgt = paguAloNonOps.AnggAgt;
                    dataPaguAlo.AnggSep = paguAloNonOps.AnggSep;
                    dataPaguAlo.AnggOkt = paguAloNonOps.AnggOkt;
                    dataPaguAlo.AnggNov = paguAloNonOps.AnggNov;
                    dataPaguAlo.AnggDes = paguAloNonOps.AnggDes;

                    dataPaguAlo.AlokJan = model.TotalAlokasi("1", saktker.KantorId); // paguAloNonOps.AlokJan;
                    dataPaguAlo.AlokFeb = model.TotalAlokasi("2", saktker.KantorId);
                    dataPaguAlo.AlokMar = model.TotalAlokasi("3", saktker.KantorId);
                    dataPaguAlo.AlokApr = model.TotalAlokasi("4", saktker.KantorId);
                    dataPaguAlo.AlokMei = model.TotalAlokasi("5", saktker.KantorId);
                    dataPaguAlo.AlokJun = model.TotalAlokasi("6", saktker.KantorId);
                    dataPaguAlo.AlokJul = model.TotalAlokasi("7", saktker.KantorId);
                    dataPaguAlo.AlokAgt = model.TotalAlokasi("8", saktker.KantorId);
                    dataPaguAlo.AlokSep = model.TotalAlokasi("9", saktker.KantorId);
                    dataPaguAlo.AlokOkt = model.TotalAlokasi("10", saktker.KantorId);
                    dataPaguAlo.AlokNov = model.TotalAlokasi("11", saktker.KantorId);
                    dataPaguAlo.AlokDes = model.TotalAlokasi("12", saktker.KantorId);

                    TotalPagu += paguAloNonOps.Pagu;
                    TotalAlokasi += paguAloNonOps.Alokasi;
                }

                BelumAlokasi = TotalPagu - TotalAlokasi;
                if (TotalPagu > 0)
                {
                    PersentaseAlokasi = (TotalAlokasi / TotalPagu) * 100;
                }

                dataPaguAlo.TotalPagu = TotalPagu;
                dataPaguAlo.TotalAlokasi = TotalAlokasi;
                dataPaguAlo.Persentase = PersentaseAlokasi;
                dataPaguAlo.PersentaseString = string.Format("{0:#,##0.##}", PersentaseAlokasi) + " %";
                dataPaguAlo.BelumAlokasi = BelumAlokasi;

                _lsSatkerALokasi.Add(dataPaguAlo);

                row++;
            }

            if (Nama != "Kantor Pusat")
            {
                Nama = "Provinsi " + Nama;
            }

            this.ViewData["kantorid"] = Id;
            this.ViewData["namasatker"] = Nama;

            return PartialView("AlokasiPemanfaatanDetail", _lsSatkerALokasi);
        }

        public ActionResult PrioritasManfaat(string Id)
        {
            if (String.IsNullOrEmpty(Id))
            {
                return RedirectToAction("Prioritas");
            }

            Models.AdmModel _adm = new Models.AdmModel();
            List<Entities.PrioritasAlokasi> _lsSatkerALokasi = _manfaatanModel.GetPrioritasManfaat(Id);
            return PartialView("PrioritasDetail", _lsSatkerALokasi);
        }

        public ActionResult DetilSatkerManfaat(string s, string ns)
        {
            Entities.DataPrioritas _data = new Entities.DataPrioritas();
            List<Entities.PrioritasAlokasi> _lsSatkerALokasi = _manfaatanModel.GetManfaatSatker(s, ConfigurationManager.AppSettings["TahunAnggaran"].ToString());
            _data.dataPrioritas = _lsSatkerALokasi;
            var useridentity = (User.Identity as Entities.InternalUserIdentity);
            _data.UserAdmin = _manfaatanModel.UsersInRoles(useridentity.UserId, "Administrator Pusat");
            _data.NamaSatKer = ns;

            return PartialView("PrioritasDetail", _data);
        }

        public ActionResult DataManfaatDetail(string kantorid, string namakantor, string tahun)
        {
            var useridentity = (User.Identity as Entities.InternalUserIdentity);

            string[] userProfiles = _manfaatanModel.GetProfileIdForUser(useridentity.UserId, useridentity.KantorId);
            int indexKaBiroPerencanaan = Array.IndexOf(userProfiles, ConfigurationManager.AppSettings["ProfileBiroPerencanaan"].ToString());
            int indexKaBiroKeuangan = Array.IndexOf(userProfiles, ConfigurationManager.AppSettings["ProfileBiroKeuangan"].ToString());

            Entities.DataPrioritas _data = new Entities.DataPrioritas();
            _data.tahun = tahun;
            //List<Entities.PrioritasAlokasi> _lsSatkerALokasi = _manfaatanModel.GetManfaatSatker(kantorid, ConfigurationManager.AppSettings["TahunAnggaran"].ToString());
            List<Entities.PrioritasAlokasi> _lsSatkerALokasi = _manfaatanModel.GetManfaatSatker(kantorid, tahun);
            //return Json(_lsSatkerALokasi, JsonRequestBehavior.AllowGet);
            _data.dataPrioritas = _lsSatkerALokasi;
            _data.UserAdmin = _manfaatanModel.UsersInRoles(useridentity.UserId, "Administrator Pusat");

            // DITUTUP LAGI....SEMENTARA :)
            // Sementara profile2 tsb bisa edit data manfaat (Alfin, 2 September 2019)
            // ==== dulu cek akses edit manfaat dibuat manual

            // Akses Edit Manfaat (Dari Table KONFIGURASI) - Alfin, 10 Januari 2020
            string nilaiAkses = _manfaatanModel.GetAksesEditManfaat();
            if (nilaiAkses == "Y")
            {
                int indexKasubagTU = Array.IndexOf(userProfiles, "N10000");
                int indexKaurRencana = Array.IndexOf(userProfiles, "N10100");
                int indexKabagTU = Array.IndexOf(userProfiles, "R10000");
                int indexKasubagKeu = Array.IndexOf(userProfiles, "R10300");
                int indexKasubagKeuBMNPusat = Array.IndexOf(userProfiles, "C1020104");
                int indexKasubagPVP = Array.IndexOf(userProfiles, "C1020202");
                if (indexKasubagTU > -1)
                {
                    _data.UserAdmin = true;
                }
                if (indexKaurRencana > -1)
                {
                    _data.UserAdmin = true;
                }
                if (indexKabagTU > -1)
                {
                    _data.UserAdmin = true;
                }
                if (indexKasubagKeu > -1)
                {
                    _data.UserAdmin = true;
                }
                if (indexKasubagKeuBMNPusat > -1)
                {
                    _data.UserAdmin = true;
                }
                if (indexKasubagPVP > -1)
                {
                    _data.UserAdmin = true;
                }
            }


            _data.KantorId = kantorid;
            _data.NamaSatKer = namakantor;
            _data.UserKaBiroPerencanaan = (indexKaBiroPerencanaan == -1) ? false : true;
            _data.UserKaBiroKeuangan = (indexKaBiroKeuangan == -1) ? false : true;

            return PartialView("DataManfaatDetail", _data);
        }

        public ActionResult ListDataManfaatKROV2()
        {
            return View();
        }

        public ActionResult DataManfaatDetailKRO()
        {
            var useridentity = (User.Identity as Entities.InternalUserIdentity);

            var _data = GetDataDetailManfaat(useridentity.KantorId, useridentity.NamaKantor, DateTime.Now.Year.ToString());

            ViewBag.IsAksesEdit = _manfaatanModel.GetAksesEditPagu();
            ViewBag.IsAksesSimpan = _manfaatanModel.GetAksesEditPagu();
            //ViewBag.IsAksesSimpan = (_data.TotalAlokasi - _data.TotalTerAlokasi > 0 ? "Y" : "N");

            return PartialView("DataManfaatDetailV2", _data);
        }

        public ActionResult DataManfaatDetailV2(string kantorid, string namakantor, string tahun)
        {

            var _data = GetDataDetailManfaat(kantorid, namakantor, tahun);

            ViewBag.IsAksesEdit = _manfaatanModel.GetAksesEditPagu();
            ViewBag.IsAksesSimpan = (_data.TotalAlokasi - _data.TotalTerAlokasi > 0 ? "Y" : "N");
            
            return PartialView("DataManfaatDetailV2", _data);
        }

        private Entities.DataPrioritas GetDataDetailManfaat(string kantorid, string namakantor, string tahun)
        {
            var useridentity = (User.Identity as Entities.InternalUserIdentity);

            string[] userProfiles = _manfaatanModel.GetProfileIdForUser(useridentity.UserId, useridentity.KantorId);
            int indexKaBiroPerencanaan = Array.IndexOf(userProfiles, ConfigurationManager.AppSettings["ProfileBiroPerencanaan"].ToString());
            int indexKaBiroKeuangan = Array.IndexOf(userProfiles, ConfigurationManager.AppSettings["ProfileBiroKeuangan"].ToString());

            Entities.DataPrioritas _data = new Entities.DataPrioritas();
            _data.tahun = tahun;
            List<Entities.PrioritasAlokasi> _lsSatkerALokasi = _manfaatanModel.GetManfaatSatkerV2(kantorid, tahun);
            _data.dataPrioritas = _lsSatkerALokasi;
            _data.UserAdmin = _manfaatanModel.UsersInRoles(useridentity.UserId, "Administrator Pusat");

            _data.TotalAnggaran = _lsSatkerALokasi.Select(x => x.Nilaianggaran).Sum();
            _data.TotalAlokasi = _manfaatanModel.GetTotalAlokasiSatkerV2(kantorid);
            _data.TotalTerAlokasi = _lsSatkerALokasi.Select(x => x.JUMLAHALOKASI).Sum();

            string nilaiAkses = _manfaatanModel.GetAksesEditManfaat();
            if (nilaiAkses == "Y")
            {
                int indexKasubagTU = Array.IndexOf(userProfiles, "N10000");
                int indexKaurRencana = Array.IndexOf(userProfiles, "N10100");
                int indexKabagTU = Array.IndexOf(userProfiles, "R10000");
                int indexKasubagKeu = Array.IndexOf(userProfiles, "R10300");
                int indexKasubagKeuBMNPusat = Array.IndexOf(userProfiles, "C1020104");
                int indexKasubagPVP = Array.IndexOf(userProfiles, "C1020202");
                if (indexKasubagTU > -1)
                {
                    _data.UserAdmin = true;
                }
                if (indexKaurRencana > -1)
                {
                    _data.UserAdmin = true;
                }
                if (indexKabagTU > -1)
                {
                    _data.UserAdmin = true;
                }
                if (indexKasubagKeu > -1)
                {
                    _data.UserAdmin = true;
                }
                if (indexKasubagKeuBMNPusat > -1)
                {
                    _data.UserAdmin = true;
                }
                if (indexKasubagPVP > -1)
                {
                    _data.UserAdmin = true;
                }
            }

            _data.KantorId = kantorid;
            _data.NamaSatKer = namakantor;
            _data.UserKaBiroPerencanaan = (indexKaBiroPerencanaan == -1) ? false : true;
            _data.UserKaBiroKeuangan = (indexKaBiroKeuangan == -1) ? false : true;

            //decimal totalAnggaran = _data.dataPrioritas.Sum(a => a.Alokasi);

            return _data;
        }

        public ActionResult SimpanRenaksiSatker(string kantorid, string judul)
        {
            var result = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };

            string tahun = DateTime.Now.Year.ToString();

            string dokumenid = _manfaatanModel.GetRenaksiSatkerId(kantorid, tahun);

            if (String.IsNullOrEmpty(dokumenid))
            {
                Entities.TransactionResult trInsertRenaksi = _manfaatanModel.InsertRenaksiSatker(kantorid, judul);
                dokumenid = trInsertRenaksi.ReturnValue;
            }

            var mfile = Request.Files["filepdf"];
            if (mfile != null && mfile.ContentType == "application/pdf")
            {
                string kantoridUser = (User.Identity as Entities.InternalUserIdentity).KantorId;
                string petugas = (User.Identity as Entities.InternalUserIdentity).NamaPegawai;

                int versi = 0;
                string id = dokumenid;
                if (kontentm.JumlahKonten(id) > 0)
                {
                    versi = kontentm.CekVersi(id) + 1;
                }

                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();

                content.Add(new StringContent(kantoridUser), "kantorId");
                content.Add(new StringContent("RenaksiSatker"), "tipeDokumen");
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

                result = kontentm.SimpanKontenFile(kantoridUser, id, judul, petugas, DateTime.Now.ToShortDateString(), "RenaksiSatker", out versi);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetFileRenaksiSatker(string id)
        {
            var result = new { Status = false, Message = "" };

            if (!String.IsNullOrEmpty(id))
            {
                var reqmessage = new HttpRequestMessage();
                var content = new MultipartFormDataContent();

                string kantorid = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
                string tipe = "RenaksiSatker";
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


        public ActionResult ShowPrioritas(string s, string m)
        {
            Entities.PrioritasAlokasi _lsSatkerALokasi = new Entities.PrioritasAlokasi();
            if (!String.IsNullOrEmpty(s)) _lsSatkerALokasi = _manfaatanModel.GetPrioritasSatker(s, "");
            if (m == "New")
            {
                _lsSatkerALokasi.ListProgram = _manfaatanModel.getProgramList("NONOPS");
                _lsSatkerALokasi.ListSatKer = _manfaatanModel.getSatKerList();
            }
            //_lsSatkerALokasi.PrioritasOrigin = _lsSatkerALokasi.Prioritaskegiatan; // alfin, 29 jan 2019
            if (_lsSatkerALokasi != null)
            {
                _lsSatkerALokasi.Anggaransatker = (_lsSatkerALokasi.Nilaianggaran > 0) ? string.Format("{0:#,##0,##}", _lsSatkerALokasi.Nilaianggaran) : "0";
                _lsSatkerALokasi.Alokasisatker = (_lsSatkerALokasi.Teralokasi > 0) ? string.Format("{0:#,##0,##}", _lsSatkerALokasi.Teralokasi) : "0";
            }
            _lsSatkerALokasi.Mode = m; // alfin, 29 jan 2019
            return View(_lsSatkerALokasi);
        }

        public ActionResult EditDataManfaat(string s, string m)
        {
            Entities.PrioritasAlokasi _lsSatkerALokasi = new Entities.PrioritasAlokasi();
            if (!String.IsNullOrEmpty(s)) _lsSatkerALokasi = _manfaatanModel.GetPrioritasSatker(s, "");
            if (m == "New")
            {
                _lsSatkerALokasi.ListProgram = _manfaatanModel.getProgramList("NONOPS");
                _lsSatkerALokasi.ListSatKer = _manfaatanModel.getSatKerList();
            }
            if (_lsSatkerALokasi != null)
            {
                _lsSatkerALokasi.Anggaransatker = (_lsSatkerALokasi.Nilaianggaran > 0) ? string.Format("{0:#,##0,##}", _lsSatkerALokasi.Nilaianggaran) : "0";
                _lsSatkerALokasi.Alokasisatker = (_lsSatkerALokasi.Teralokasi > 0) ? string.Format("{0:#,##0,##}", _lsSatkerALokasi.Teralokasi) : "0";
            }
            _lsSatkerALokasi.Mode = m;
            return View(_lsSatkerALokasi);
        }

        public ActionResult EntriDataManfaat(string manfaatid, string m, string tahun)
        {
            Entities.PrioritasAlokasi _lsSatkerALokasi = new Entities.PrioritasAlokasi();
            if (!String.IsNullOrEmpty(manfaatid)) _lsSatkerALokasi = _manfaatanModel.GetPrioritasSatker(manfaatid, tahun);
            if (m == "Tambah Data Manfaat")
            {
                _lsSatkerALokasi.ListProgram = _manfaatanModel.getProgramList("NONOPS");
                _lsSatkerALokasi.ListSatKer = _manfaatanModel.getSatKerList();
            }
            if (_lsSatkerALokasi != null)
            {
                _lsSatkerALokasi.Anggaransatker = (_lsSatkerALokasi.Nilaianggaran > 0) ? string.Format("{0:#,##0,##}", _lsSatkerALokasi.Nilaianggaran) : "0";
                _lsSatkerALokasi.Alokasisatker = (_lsSatkerALokasi.Teralokasi > 0) ? string.Format("{0:#,##0,##}", _lsSatkerALokasi.Teralokasi) : "0";
            }
            if (_lsSatkerALokasi != null)
            {
                _lsSatkerALokasi.Mode = m;
            }
            _lsSatkerALokasi.Tahun = tahun;
            //return View(_lsSatkerALokasi);
            return PartialView("EditDataManfaat", _lsSatkerALokasi);
        }

        public ActionResult EntriDataManfaatV2(string manfaatid, string m, string tahun, string kantorId)
        {
            if (tahun == null)
            {
                tahun = DateTime.Now.Year.ToString();
            }
            Entities.PrioritasAlokasi _lsSatkerALokasi = new Entities.PrioritasAlokasi();
            if (!String.IsNullOrEmpty(manfaatid)) _lsSatkerALokasi = _manfaatanModel.GetPrioritasSatker(manfaatid, tahun);
            if (m == "Tambah Data Manfaat")
            {
                _lsSatkerALokasi.ListProgram = _manfaatanModel.getProgramList("NONOPS");
                _lsSatkerALokasi.ListSatKer = _manfaatanModel.getSatKerList();
            }
            if (_lsSatkerALokasi != null)
            {
                _lsSatkerALokasi.Anggaransatker = (_lsSatkerALokasi.Nilaianggaran > 0) ? string.Format("{0:#,##0,##}", _lsSatkerALokasi.Nilaianggaran) : "0";
                _lsSatkerALokasi.Alokasisatker = (_lsSatkerALokasi.Teralokasi > 0) ? string.Format("{0:#,##0,##}", _lsSatkerALokasi.Teralokasi) : "0";
            }
            if (_lsSatkerALokasi != null)
            {
                _lsSatkerALokasi.Mode = m;
            }
            _lsSatkerALokasi.Tahun = tahun;


            List<Entities.PrioritasAlokasi> dataPrioritas = _manfaatanModel.GetManfaatSatkerV2(kantorId, tahun);
            string sisaAlokasi = "0";
            string totalAnggaran = dataPrioritas.Sum(a => a.Nilaianggaran).ToString("N0", new System.Globalization.CultureInfo("id-ID"));
            string totalAlokasi = dataPrioritas.Sum(a => a.JUMLAHALOKASI + a.TOTALALOKASI).ToString("N0", new System.Globalization.CultureInfo("id-ID"));
            decimal totalTerAlokasi = dataPrioritas.Select(x => x.JUMLAHALOKASI).Sum();

            if (dataPrioritas.Count > 0) {
                //sisaAlokasi = (dataPrioritas.Sum(a => a.Nilaianggaran) - dataPrioritas.Sum(a => a.JUMLAHALOKASI)).ToString("N0", new System.Globalization.CultureInfo("id-ID"));
                decimal totalAlokasiSatker = _manfaatanModel.GetTotalAlokasiSatkerV2(kantorId);
                sisaAlokasi = (totalAlokasiSatker - totalTerAlokasi).ToString("N0", new System.Globalization.CultureInfo("id-ID"));
            }

            ViewBag.SisaAlokasi = $"Rp. {sisaAlokasi}";
            ViewBag.TotalAnggaran = $"Rp. {totalAnggaran}";
            ViewBag.TotalAlokasi = $"Rp. {totalAlokasi}";

            _lsSatkerALokasi.NilaianggaranView = "Rp. " + _lsSatkerALokasi.Nilaianggaran.ToString("N0", new System.Globalization.CultureInfo("id-ID"));

            //return View(_lsSatkerALokasi);
            return PartialView("EditDataManfaatV2", _lsSatkerALokasi);
        }

        public ActionResult UpdatePrioritas(Entities.PrioritasAlokasi frm)
        {
            var result = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };
            List<Entities.PrioritasAlokasi> _lsSatkerALokasi = new List<Entities.PrioritasAlokasi>();
            var useridentity = (User.Identity as Entities.InternalUserIdentity);
            try
            {
                if (!Pnbp.Models.AdmModel.GetStatusKantorbyManfaat(frm.Manfaatid))
                {
                    throw new Exception("Satker tidak aktif!");
                }
                if (frm.Mode == "New")
                {
                    result = _manfaatanModel.InsertPrioritas(frm, DateTime.Now.Year.ToString(), useridentity.UserId);
                }
                else
                {
                    result = _manfaatanModel.UpdatePrioritas(frm.Manfaatid, frm.Prioritaskegiatan, frm.PrioritasOrigin, frm.Statusaktif, frm.Mode);
                }
                _lsSatkerALokasi = _manfaatanModel.GetPrioritasManfaat(frm.PrioritasOrigin);
                if (!result.Status)
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


        public ActionResult PersetujuanRevisiManfaat()
        {
            int indexKaBiroPerencanaan = -1;
            int indexKaBiroKeuangan = -1;

            var useridentity = (User.Identity as Entities.InternalUserIdentity);
            if (useridentity != null)
            {
                string[] userProfiles = _manfaatanModel.GetProfileIdForUser(useridentity.UserId, useridentity.KantorId);
                indexKaBiroPerencanaan = Array.IndexOf(userProfiles, ConfigurationManager.AppSettings["ProfileBiroPerencanaan"].ToString());
                indexKaBiroKeuangan = Array.IndexOf(userProfiles, ConfigurationManager.AppSettings["ProfileBiroKeuangan"].ToString());
            }

            Entities.FindManfaat fn = new Entities.FindManfaat();
            fn.UserKaBiroPerencanaan = (indexKaBiroPerencanaan == -1) ? false : true;
            fn.UserKaBiroKeuangan = (indexKaBiroKeuangan == -1) ? false : true;
            return View(fn);
        }

        public ActionResult DaftarRevisiManfaat(int? pageNum, Entities.FindManfaat f)
        {
            int pageNumber = pageNum ?? 0;
            int RecordsPerPage = 10;
            int from = (pageNumber * RecordsPerPage) + 1;
            int to = from + RecordsPerPage - 1;

            string tahun = ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            string kodesatker = f.KodeSatker;
            string namasatker = f.NamaSatker;
            string namaprogram = f.NamaProgram;
            decimal? nilaianggaran = f.NilaiAnggaran;

            this.ViewBag.UserKaBiroPerencanaan = f.UserKaBiroPerencanaan;
            this.ViewBag.UserKaBiroKeuangan = f.UserKaBiroKeuangan;

            List<Entities.PrioritasAlokasi> result = _manfaatanModel.GetManfaatDetail(tahun, kodesatker, namasatker, namaprogram, nilaianggaran, true, from, to);

            int custIndex = from;
            Dictionary<int, Entities.PrioritasAlokasi> dict = result.ToDictionary(x => custIndex++, x => x);

            if (result.Count > 0)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DaftarRevisiManfaat", dict);
                }
                else
                {
                    return RedirectToAction("PersetujuanRevisiManfaat", "Pemanfaatan");
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

        public ActionResult rl_kro ()
        {
            int indexKaBiroPerencanaan = -1;
            int indexKaBiroKeuangan = -1;

            var useridentity = (User.Identity as Entities.InternalUserIdentity);
            if (useridentity != null)
            {
                string[] userProfiles = _manfaatanModel.GetProfileIdForUser(useridentity.UserId, useridentity.KantorId);
                indexKaBiroPerencanaan = Array.IndexOf(userProfiles, ConfigurationManager.AppSettings["ProfileBiroPerencanaan"].ToString());
                indexKaBiroKeuangan = Array.IndexOf(userProfiles, ConfigurationManager.AppSettings["ProfileBiroKeuangan"].ToString());
            }

            Entities.FindManfaat fn = new Entities.FindManfaat();
            fn.UserKaBiroPerencanaan = (indexKaBiroPerencanaan == -1) ? false : true;
            fn.UserKaBiroKeuangan = (indexKaBiroKeuangan == -1) ? false : true;
            return View(fn);
        }

        public ActionResult rl_provinsi()
        {
            int indexKaBiroPerencanaan = -1;
            int indexKaBiroKeuangan = -1;

            var useridentity = (User.Identity as Entities.InternalUserIdentity);
            if (useridentity != null)
            {
                string[] userProfiles = _manfaatanModel.GetProfileIdForUser(useridentity.UserId, useridentity.KantorId);
                indexKaBiroPerencanaan = Array.IndexOf(userProfiles, ConfigurationManager.AppSettings["ProfileBiroPerencanaan"].ToString());
                indexKaBiroKeuangan = Array.IndexOf(userProfiles, ConfigurationManager.AppSettings["ProfileBiroKeuangan"].ToString());
            }

            Entities.FindManfaat fn = new Entities.FindManfaat();
            fn.UserKaBiroPerencanaan = (indexKaBiroPerencanaan == -1) ? false : true;
            fn.UserKaBiroKeuangan = (indexKaBiroKeuangan == -1) ? false : true;
            return View(fn);
        }

        public ActionResult rl_satker()
        {
            int indexKaBiroPerencanaan = -1;
            int indexKaBiroKeuangan = -1;

            var useridentity = (User.Identity as Entities.InternalUserIdentity);
            if (useridentity != null)
            {
                string[] userProfiles = _manfaatanModel.GetProfileIdForUser(useridentity.UserId, useridentity.KantorId);
                indexKaBiroPerencanaan = Array.IndexOf(userProfiles, ConfigurationManager.AppSettings["ProfileBiroPerencanaan"].ToString());
                indexKaBiroKeuangan = Array.IndexOf(userProfiles, ConfigurationManager.AppSettings["ProfileBiroKeuangan"].ToString());
            }

            Entities.FindManfaat fn = new Entities.FindManfaat();
            fn.UserKaBiroPerencanaan = (indexKaBiroPerencanaan == -1) ? false : true;
            fn.UserKaBiroKeuangan = (indexKaBiroKeuangan == -1) ? false : true;
            return View(fn);
        }


        [HttpPost]
        public ActionResult rl_kro(int? start, Entities.FindManfaat f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;



            string tahun = ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            string kodesatker = f.KodeSatker;
            string namasatker = f.NamaSatker;
            string namaprogram = f.NamaProgram;
            decimal? nilaianggaran = f.NilaiAnggaran;

            this.ViewBag.UserKaBiroPerencanaan = f.UserKaBiroPerencanaan;
            this.ViewBag.UserKaBiroKeuangan = f.UserKaBiroKeuangan;

            List<Entities.BelanjaKRO> result = _manfaatanModel.lr_kro(tahun, kodesatker, namasatker, namaprogram, nilaianggaran, true, from, to);


            int custIndex = from;


            decimal? total = 0;
            if (result.Count > 0)
            {
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult rl_satker_list(int? start, Entities.FindManfaat f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;



            string tahun = ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            string kodesatker = f.KodeSatker;
            string namasatker = f.NamaSatker;
            string namaprogram = f.NamaProgram;
            decimal? nilaianggaran = f.NilaiAnggaran;

            this.ViewBag.UserKaBiroPerencanaan = f.UserKaBiroPerencanaan;
            this.ViewBag.UserKaBiroKeuangan = f.UserKaBiroKeuangan;

            List<Entities.BelanjaKRO> result = _manfaatanModel.rl_satker(tahun, kodesatker, namasatker, namaprogram, nilaianggaran, true, from, to);
            //List<Entities.BelanjaKRO> resultSumList = _manfaatanModel.rl_satker_sum(tahun, kodesatker, namasatker, namaprogram, nilaianggaran, true, from, to);

            //var resultSum = resultSumList.First();

            foreach (var item in result)
            {
                //item.PersentaseAlokasi = ((item.Alokasi / resultSum.TotalAlokasi) * 100)?.ToString();
                //var persenAlokasi = ((item.Pagu / resultSum.TotalPagu) * 100);
                //item.PersentaseAlokasi = string.Format("{0:#,##0.##}", persenAlokasi) + " %"; ;
            }

            decimal? total = 0;
            if (result.Count > 0)
            {
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult rl_provinsi(int? start, Entities.FindManfaat f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;



            string tahun = ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            string kodesatker = f.KodeSatker;
            string namasatker = f.NamaSatker;
            string namaprogram = f.NamaProgram;
            decimal? nilaianggaran = f.NilaiAnggaran;

            this.ViewBag.UserKaBiroPerencanaan = f.UserKaBiroPerencanaan;
            this.ViewBag.UserKaBiroKeuangan = f.UserKaBiroKeuangan;

            List<Entities.BelanjaKRO> result = _manfaatanModel.rl_provinsi(tahun, kodesatker, namasatker, namaprogram, nilaianggaran, true, from, to);

            foreach (var item in result)
            {
            }

            decimal? total = 0;
            if (result.Count > 0)
            {
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
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
        public ActionResult RincianAlokasi()
        {
            string currentYear = DateTime.Now.Year.ToString();
            var ctx = new PnbpContext();
            Entities.CariRincianAlokasiSatker find = new Entities.CariRincianAlokasiSatker();
            List<Entities.GetSatkerList> result = _manfaatanModel.GetSatker();
            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            int tipekantorid = _manfaatanModel.GetTipeKantor(kantoriduser);
            ViewData["tipekantorid"] = Convert.ToString(tipekantorid);
            ViewData["datasatker"] = result;

            //new query
            var get_data = ctx
                .Database
                .SqlQuery<Entities.Total>($@"
                    SELECT NVL(SUM( AMOUNT ), 0) AS TOTALPAGU 
                    FROM SPAN_BELANJA 
                    WHERE SUMBER_DANA = 'D' AND KDSATKER <> '524465' AND TAHUN = {currentYear}")
                .FirstOrDefault();
            var get_total_realisasi = ctx
                .Database
                .SqlQuery<Entities.Total>($@"
                    SELECT NVL(SUM( AMOUNT ), 0) AS TOTALREALISASI 
                    FROM SPAN_REALISASI 
                    WHERE SUMBERDANA = 'D' AND KDSATKER <> '524465' AND TAHUN = {currentYear}")
                .FirstOrDefault();
            //var get_total_alokasi = ctx.Database.SqlQuery<Entities.Total>("SELECT SUM( ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT) + SUM( NILAIALOKASI ) AS ALOKASI  FROM MANFAAT WHERE TAHUN = ( SELECT (to_char( SYSDATE, 'YYYY' )) AS Y FROM dual )").FirstOrDefault();
            var get_total_alokasi = ctx
                .Database
                .SqlQuery<Entities.Total>($@"
                    SELECT NVL(SUM(TERALOKASI), 0) AS ALOKASI 
                    FROM REKAPALOKASI 
                    WHERE TAHUN = '" + currentYear +"'")
                .FirstOrDefault();

            ViewData["get_data"] = get_data;
            ViewData["get_total_realisasi"] = get_total_realisasi;
            ViewData["get_total_alokasi"] = get_total_alokasi;
            //return Json(ViewData["get_total_realisasi"], JsonRequestBehavior.AllowGet);


            return View(find);
        }

        public ActionResult RincianAlokasiOutput()
        {
            Entities.CariRincianAlokasiSatker find = new Entities.CariRincianAlokasiSatker();
            List<Entities.GetSatkerList> result = _manfaatanModel.GetSatker();
            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            int tipekantorid = _manfaatanModel.GetTipeKantor(kantoriduser);
            ViewData["tipekantorid"] = Convert.ToString(tipekantorid);
            ViewData["datasatker"] = result;
            return View(find);
        }

        public ActionResult RincianAlokasiDetail(string kantorid)
        {
            Entities.CariRincianAlokasiSatker find = new Entities.CariRincianAlokasiSatker();
            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            Entities.satker satker = new Entities.satker();
            int tipekantorid = _manfaatanModel.GetTipeKantor(kantoriduser);
            satker = _manfaatanModel.GetSatkerByKantorId(kantorid);
            ViewData["tipekantorid"] = Convert.ToString(tipekantorid);
            ViewData["datasatker"] = satker;
            ViewData["kantoridclick"] = kantorid;
            return View(find);
        }

        public ActionResult RincianAlokasiDetailOutput(string programid)
        {
            string currentYear = DateTime.Now.Year.ToString();
            var ctx = new PnbpContext();
            //return Json(programid, JsonRequestBehavior.AllowGet);
            Entities.CariRincianAlokasiSatker find = new Entities.CariRincianAlokasiSatker();
            Entities.program resultprogram = new Entities.program();
            List<Entities.GetSatkerList> result = _manfaatanModel.GetSatker();
            resultprogram = _manfaatanModel.GetLayananById(programid);
            Entities.GetManfaat manfaat = new Entities.GetManfaat();
            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            int tipekantorid = _manfaatanModel.GetTipeKantor(kantoriduser);
            //manfaat = _manfaatanModel.GetLayananById(programid);
            
            var get_data = ctx
                .Database
                .SqlQuery<Entities.Total>($@"
                    SELECT SUM( AMOUNT ) AS TOTALPAGU 
                    FROM SPAN_BELANJA 
                    WHERE TAHUN = {currentYear}")
                .FirstOrDefault();
            var get_total_realisasi = ctx
                .Database
                .SqlQuery<Entities.Total>($@"
                    SELECT SUM( AMOUNT ) AS TOTALREALISASI 
                    FROM SPAN_REALISASI 
                    WHERE TAHUN = {currentYear}")
                .FirstOrDefault();

            ViewData["get_data"] = get_data;
            ViewData["get_total_realisasi"] = get_total_realisasi;

            ViewData["tipekantorid"] = Convert.ToString(tipekantorid);
            ViewData["dataprogram"] = resultprogram;
            ViewData["programid"] = programid;
            ViewData["manfaat"] = manfaat;
            return View(find);
        }
        public ActionResult EntriRenaksi()
        {

            var ctx = new PnbpContext();
            PnbpContext db = new PnbpContext();
            Entities.FindPengembalianPnbp find = new Entities.FindPengembalianPnbp();
            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            //return Json(kantoriduser, JsonRequestBehavior.AllowGet);
            List<Entities.GetSatkerList> result = _manfaatanModel.GetSatker();
            List<Entities.RenaksiList> renaksi = _manfaatanModel.GetRenaksiList(kantoriduser);
            int tipekantorid = _manfaatanModel.GetTipeKantor(kantoriduser);
            string aksesEdit = _manfaatanModel.GetAksesEditPagu();
            string pegawaiid = (HttpContext.User.Identity as Entities.InternalUserIdentity).PegawaiId;


            string query =
                @"
                    SELECT COUNT(*) AS ADA  from PROFILEPEGAWAI WHERE PROFILEID like 'N10000' AND PEGAWAIID ='" + pegawaiid + "'";
            var get_tu = ctx.Database.SqlQuery<Entities.ProfilPegawai>(query).FirstOrDefault();
            ViewData["get_tu"] = get_tu;

            //return Json(aksesEdit, JsonRequestBehavior.AllowGet);
            ViewData["tipekantorid"] = Convert.ToString(tipekantorid);
            ViewData["datasatker"] = result;
            ViewData["renaksi"] = renaksi;
            ViewData["aksesEdit"] = aksesEdit;
            return View(find);
        }

        [HttpPost]
        public ActionResult EntriRenaksi(
            FormCollection form,
            string[] MANFAATID, string[] PROGRAMID,
            string[] ANGGJAN, string[] ANGGFEB, string[] ANGGMAR, string[] ANGGAPR, string[] ANGGMEI, string[] ANGGJUN,
            string[] ANGGJUL, string[] ANGGAGT, string[] ANGGSEP, string[] ANGGOKT, string[] ANGGNOV, string[] ANGGDES, string[] status, string[] revisi,
            string[] HIST_ANGGJAN, string[] HIST_ANGGFEB, string[] HIST_ANGGMAR, string[] HIST_ANGGAPR, string[] HIST_ANGGMEI, string[] HIST_ANGGJUN,
            string[] HIST_ANGGJUL, string[] HIST_ANGGAGT, string[] HIST_ANGGSEP, string[] HIST_ANGGOKT, string[] HIST_ANGGNOV, string[] HIST_ANGGDES
            )
        {
            var ctx = new PnbpContext();
            PnbpContext db = new PnbpContext();
            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            string namakantor = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaKantor;
            string pegawaiid = (HttpContext.User.Identity as Entities.InternalUserIdentity).PegawaiId;
            string namapegawai = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaPegawai;
            var NomorBerkas = ((form.AllKeys.Contains("NomorBerkas")) ? form["NomorBerkas"] : "");

            //return Json(revisi, JsonRequestBehavior.AllowGet);
            //return Json(status, JsonRequestBehavior.AllowGet);
            Entities.GetEviden eviden = new Entities.GetEviden();
            //return Json(ANGGJAN.Count(), JsonRequestBehavior.AllowGet);
            for (var i = 0; i < ANGGJAN.Count(); i++)
            {
                if (status[i] != "")
                {
                    string update_target = "UPDATE MANFAAT SET STATUSREVISI= 1 , PERSETUJUAN1= 0 , PERSETUJUAN2= 0 , ANGGJAN=" + ANGGJAN[i].Replace(".", String.Empty) + " , ANGGFEB=" + ANGGFEB[i].Replace(".", String.Empty) + ",ANGGMAR=" + ANGGMAR[i].Replace(".", String.Empty) + ",ANGGAPR=" + ANGGAPR[i].Replace(".", String.Empty) + ",ANGGMEI=" + ANGGMEI[i].Replace(".", String.Empty) + ",ANGGJUN=" + ANGGJUN[i].Replace(".", String.Empty) + ",ANGGJUL=" + ANGGJUL[i].Replace(".", String.Empty) + ",ANGGAGT=" + ANGGAGT[i].Replace(".", String.Empty) + ",ANGGSEP=" + ANGGSEP[i].Replace(".", String.Empty) + ",ANGGOKT=" + ANGGOKT[i].Replace(".", String.Empty) + ",ANGGNOV=" + ANGGNOV[i].Replace(".", String.Empty) + ",ANGGDES=" + ANGGDES[i].Replace(".", String.Empty) + " WHERE MANFAATID = '" + MANFAATID[i] + "' AND PROGRAMID = '" + PROGRAMID[i] + "'";
                    db.Database.ExecuteSqlCommand(update_target);

                    string manfaatid = NewGuID();
                    string insert_histori = "INSERT INTO MANFAAT_RENAKSI_HISTORY (MANFAATID, STATUSREVISI, ANGGJAN, ANGGFEB, ANGGMAR, ANGGAPR, ANGGMEI, ANGGJUN, ANGGJUL, ANGGAGT, ANGGSEP, ANGGOKT, ANGGNOV, ANGGDES, PERSETUJUAN1, PERSETUJUAN2, MANFAAT_MANFAATID, INSERTDATE) VALUES ('" + manfaatid + "', " + 1 + "," + HIST_ANGGJAN[i] + ", " + HIST_ANGGFEB[i] + ", " + HIST_ANGGMAR[i] + ", " + HIST_ANGGAPR[i] + ", " + HIST_ANGGMEI[i] + ", " + HIST_ANGGJUN[i] + ", " + HIST_ANGGJUL[i] + ", " + HIST_ANGGAGT[i] + ", " + HIST_ANGGSEP[i] + ", " + HIST_ANGGOKT[i] + ", " + HIST_ANGGNOV[i] + ", " + HIST_ANGGDES[i] + ", 0, 0, '" + MANFAATID[i] + "', SYSDATE)";
                    db.Database.ExecuteSqlCommand(insert_histori);

                    string log_id = NewGuID();
                    string insert_log_aktivitas = "INSERT INTO LOG_AKTIFITAS (LOG_ID, LOG_NAME, LOG_CREATE_BY, LOG_CREATE_DATE, LOG_URL, LOG_KANTORID, LOG_DATA_ID, LOG_TIPE) VALUES ('" + log_id + "', 'Melakukan Update Entri Renaksi', '" + pegawaiid + "', SYSDATE, '" + Url.Action("EntriRenaksi", "Pemanfaatan") + "', '" + kantoriduser + "', '" + MANFAATID[i] + "', 'RENAKSI')";
                    db.Database.ExecuteSqlCommand(insert_log_aktivitas);
                }
                else
                {
                    string update_target = "UPDATE MANFAAT SET ANGGJAN=" + ANGGJAN[i].Replace(".", String.Empty) + " , ANGGFEB=" + ANGGFEB[i].Replace(".", String.Empty) + ",ANGGMAR=" + ANGGMAR[i].Replace(".", String.Empty) + ",ANGGAPR=" + ANGGAPR[i].Replace(".", String.Empty) + ",ANGGMEI=" + ANGGMEI[i].Replace(".", String.Empty) + ",ANGGJUN=" + ANGGJUN[i].Replace(".", String.Empty) + ",ANGGJUL=" + ANGGJUL[i].Replace(".", String.Empty) + ",ANGGAGT=" + ANGGAGT[i].Replace(".", String.Empty) + ",ANGGSEP=" + ANGGSEP[i].Replace(".", String.Empty) + ",ANGGOKT=" + ANGGOKT[i].Replace(".", String.Empty) + ",ANGGNOV=" + ANGGNOV[i].Replace(".", String.Empty) + ",ANGGDES=" + ANGGDES[i].Replace(".", String.Empty) + " WHERE MANFAATID = '" + MANFAATID[i] + "' AND PROGRAMID = '" + PROGRAMID[i] + "'";
                    db.Database.ExecuteSqlCommand(update_target);
                }



                eviden = _manfaatanModel.GetevidenById(MANFAATID[i]);
                //return Json(eviden, JsonRequestBehavior.AllowGet);
                //if (eviden.EVIDENTID.Count() > 0)
                if (eviden != null && !String.IsNullOrEmpty(eviden.EVIDENTID))
                {
                    //return Json(eviden, JsonRequestBehavior.AllowGet);
                    var file_name = ((form.AllKeys.Contains("eviden_" + i)) ? form["eviden_" + i] : "");
                    var fileContent = Request.Files["eviden_" + i];
                    //Insert SURATPERMOHONAN
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                        var stream = fileContent.InputStream;
                        var FileSizeByte = fileContent.ContentLength;
                        var FileSize = FileSizeByte / 3000;
                        var Extension = System.IO.Path.GetExtension(fileContent.FileName);
                        var fileName = "ENTRI_RENAKSI_" + tgl + "" + Extension;
                        string folderPath = Server.MapPath("~/Uploads/entrirenaksi/");
                        //Check whether Directory (Folder) exists.
                        if (!Directory.Exists(folderPath))
                        {
                            //If Dir3ectory (Folder) does not exists. Create it.
                            Directory.CreateDirectory(folderPath);
                        }

                        var path = Path.Combine(Server.MapPath("~/Uploads/entrirenaksi/"), fileName);
                        var Filefilepath = "/Uploads/entrirenaksi/" + fileName;
                        using (var fileStream = System.IO.File.Create(path))
                        {
                            stream.CopyTo(fileStream);
                            file_name = fileName;
                            string eviden_data = "UPDATE EVIDEN SET EVIDENTNAME='" + file_name + "' , EVIDENPATH='" + Filefilepath + "' WHERE EVIDENMANFAATID = '" + MANFAATID[i] + "'";
                            db.Database.ExecuteSqlCommand(eviden_data);
                        }
                    }
                    //string eviden_data = "UPDATE PNBPTRAIN.EVIDEN SET ANGGJAN=" + ANGGJAN[i] + " , ANGGFEB=" + ANGGFEB[i] + ",ANGGMAR=" + ANGGMAR[i] + ",ANGGAPR=" + ANGGAPR[i] + ",ANGGMEI=" + ANGGMEI[i] + ",ANGGJUN=" + ANGGJUN[i] + ",ANGGJUL=" + ANGGJUL[i] + ",ANGGAGT=" + ANGGAGT[i] + ",ANGGSEP=" + ANGGSEP[i] + ",ANGGOKT=" + ANGGOKT[i] + ",ANGGNOV=" + ANGGNOV[i] + ",ANGGDES=" + ANGGDES[i] + " WHERE MANFAATID = '" + MANFAATID[i] + "' AND PROGRAMID = '" + PROGRAMID[i] + "'";
                    //db.Database.ExecuteSqlCommand(eviden_data);
                }
                else
                {
                    var file_name = ((form.AllKeys.Contains("eviden_" + i)) ? form["eviden_" + i] : "");
                    var fileContent = Request.Files["eviden_" + i];
                    //Insert SURATPERMOHONAN
                    //return Json(fileContent.FileName, JsonRequestBehavior.AllowGet);
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                        var stream = fileContent.InputStream;
                        var FileSizeByte = fileContent.ContentLength;
                        var FileSize = FileSizeByte / 3000;
                        var Extension = System.IO.Path.GetExtension(fileContent.FileName);
                        var fileName = "ENTRI_RENAKSI_" + tgl + "" + Extension;
                        string folderPath = Server.MapPath("~/Uploads/entrirenaksi/");
                        //Check whether Directory (Folder) exists.
                        if (!Directory.Exists(folderPath))
                        {
                            //If Dir3ectory (Folder) does not exists. Create it.
                            Directory.CreateDirectory(folderPath);
                        }

                        var path = Path.Combine(Server.MapPath("~/Uploads/entrirenaksi/"), fileName);
                        var Filefilepath = "/Uploads/entrirenaksi/" + fileName;
                        using (var fileStream = System.IO.File.Create(path))
                        {
                            stream.CopyTo(fileStream);
                            file_name = fileName;
                            string eviden_data = @"INSERT INTO EVIDEN (EVIDENTID,EVIDENTNAME,EVIDENPATH,EVIDENCREATBY,EVIDENSTATUS,EVIDENMANFAATID)
                                VALUES('" + RandomString(32) + "','" + file_name + "','" + Filefilepath + "','" + pegawaiid + "','1','" + MANFAATID[i] + "')";
                            db.Database.ExecuteSqlCommand(eviden_data);
                        }
                    }
                }

                //Insert Histori

                ////string manfaatid = NewGuID();

                ////string insert_histori = "INSERT INTO MANFAAT_RENAKSI_HISTORY (MANFAATID,  ANGGJAN, ANGGFEB, ANGGMAR, ANGGAPR, ANGGMEI, ANGGJUN, ANGGJUL, ANGGAGT, ANGGSEP, ANGGOKT, ANGGNOV, ANGGDES, PERSETUJUAN1, PERSETUJUAN2, MANFAAT_MANFAATID, INSERTDATE) VALUES ('" + manfaatid + "', " + HIST_ANGGJAN[i] + ", " + HIST_ANGGFEB[i] + ", " + HIST_ANGGMAR[i] + ", " + HIST_ANGGAPR[i] + ", " + HIST_ANGGMEI[i] + ", " + HIST_ANGGJUN[i] + ", " + HIST_ANGGJUL[i] + ", " + HIST_ANGGAGT[i] + ", " + HIST_ANGGSEP[i] + ", " + HIST_ANGGOKT[i] + ", " + HIST_ANGGNOV[i] + ", " + HIST_ANGGDES[i] + ", 0, 0, '" + MANFAATID[i] + "', SYSDATE)";

                //////return Json(insert_histori, JsonRequestBehavior.AllowGet);


                ////db.Database.ExecuteSqlCommand(insert_histori);

            }

            if (ModelState.IsValid)
            {
                TempData["Entri"] = "Data Berhasil Disimpan";
                return RedirectToAction("EntriRenaksi");
            }
            else
            {
                TempData["EntriFail"] = "Data Gagal Disimpan";
                return RedirectToAction("EntriRenaksi");
            }
            return RedirectToAction("EntriRenaksi");
        }
        public ActionResult DaftarRincianAlokasi(int? start, int? length, Entities.CariRincianAlokasiSatker f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            decimal? total = 0;

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            string idsatker = f.KANTORID;
            string tahun = f.TAHUN;
            //return Json(kantorid, JsonRequestBehavior.AllowGet);
            List<Entities.RincianAlokasiList> result = _manfaatanModel.GetRincianAlokasiSatker(tipekantorid, kantorid, idsatker, tahun, from, to);
            Entities.RincianAlokasiTotal resultTotal = _manfaatanModel.GetRincianAlokasiSatkerTotal(tipekantorid, kantorid, idsatker, tahun, from, to);

            if (result.Count > 0)
            {
                total = result[0].Total;
            }
            //return Json(resultTotal, JsonRequestBehavior.AllowGet);
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total, tfoot = resultTotal }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarRincianAlokasiOutput(int? start, int? length, Entities.CariRincianAlokasiSatker f, FormCollection form)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            decimal? total = 0;

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            string kodesatker = f.KODESATKER;
            string tahun = f.TAHUN;
            string programid = f.PROGRAMID;
            //string programid = (form.AllKeys.Contains("kodesatker")) ? form["kodesatker"];
            //string programid = form["PROGRAMID"];
            //return Json(programid, JsonRequestBehavior.AllowGet);
            List<Entities.RincianAlokasiListDetail> result = _manfaatanModel.GetRincianAlokasiSatkerOutput(programid, tipekantorid, kantorid, tahun, from, to);
            //return Json(programid, JsonRequestBehavior.AllowGet);

            if (result.Count > 0)
            {
                total = result[0].Total;
            }
            //return Json(from, JsonRequestBehavior.AllowGet);
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarRincianAlokasiDetail(int? start, int? length, Entities.CariRincianAlokasiSatker f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            decimal? total = 0;

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;

            string kantorid = f.KANTORID;
            //string tahun = f.TAHUN;
            //return Json(to, JsonRequestBehavior.AllowGet);
            List<Entities.RincianAlokasiListDetail> result = _manfaatanModel.GetRincianAlokasiSatkerOutputById(kantorid, from, to);

            if (result.Count > 0)
            {
                total = result[0].Total;
            }
            //return Json(from, JsonRequestBehavior.AllowGet);
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TotalRincianAlokasi()
        {
            var ctx = new PnbpContext();
            string currentYear = DateTime.Now.Year.ToString();
            //return Json(kantorid, JsonRequestBehavior.AllowGet);
            
            var get_data = ctx
                .Database
                .SqlQuery<Entities.Total>($@"
                    SELECT SUM( AMOUNT ) AS TOTALPAGU 
                    FROM SPAN_BELANJA
                    WHERE TAHUN = {currentYear}")
                .ToList();
            ViewData["get_data"] = get_data;

            return View();
        }

        public ActionResult DaftarRincianAlokasiDetailOutput(int? start, int? length, Entities.CariRincianAlokasiSatker f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            decimal? total = 0;

            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            string programid = f.PROGRAMID;
            //string tahun = f.TAHUN;
            //return Json(to, JsonRequestBehavior.AllowGet);
            List<Entities.RincianAlokasiListDetail> result = _manfaatanModel.GetRincianAlokasiDetailByProgramId(tipekantorid, kantorid, programid, from, to);
            //return Json(result, JsonRequestBehavior.AllowGet);

            if (result.Count > 0)
            {
                total = result[0].Total;
            }
            //return Json(from, JsonRequestBehavior.AllowGet);
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public ActionResult EntriRenaksiPusat()
        {
            var ctx = new PnbpContext();
            if ((User as Pnbp.Entities.InternalUserIdentity).PegawaiId == "197804302003122002")
            {
                var get_satker = ctx.Database.SqlQuery<Entities.DataSatker>("SELECT DISTINCT KANTORID, NAMAKANTOR from MANFAAT WHERE PERSETUJUAN2 = 0 OR PERSETUJUAN2 = NULL AND STATUSREVISI = 1").ToList();
                ViewData["get_satker"] = get_satker;

                var get_datapersetujuan = ctx.Database.SqlQuery<Entities.DataRenaksiPusat>(@"
                SELECT DISTINCT 
                    a.NAMAKANTOR, 
                    a.MANFAATID, 
                    a.KANTORID, 
                    a.NAMAPROGRAM, 
                    a.TIPE, 
                    a.NILAIANGGARAN, 
                    nvl(A .ANGGJAN, 0) AS ANGGJAN,
	                nvl(A .ANGGFEB, 0) AS ANGGFEB,
	                nvl(A .ANGGMAR, 0) AS ANGGMAR,
	                nvl(A .ANGGAPR, 0) AS ANGGAPR,
	                nvl(A .ANGGMEI, 0) AS ANGGMEI,
	                nvl(A .ANGGJUN, 0) AS ANGGJUN,
	                nvl(A .ANGGJUL, 0) AS ANGGJUL,
	                nvl(A .ANGGAGT, 0) AS ANGGAGT,
	                nvl(A .ANGGSEP, 0) AS ANGGSEP,
	                nvl(A .ANGGOKT, 0) AS ANGGOKT,
	                nvl(A .ANGGNOV, 0) AS ANGGNOV,
	                nvl(A .ANGGDES, 0) AS ANGGDES, 
                    a.PROGRAMID, 
                    b.KODE, 
                    c.EVIDENPATH, 
                    a.PERSETUJUAN1, 
                    a.PERSETUJUAN2 
                FROM MANFAAT a 
                LEFT JOIN PROGRAM b on a.PROGRAMID = b.PROGRAMID 
                LEFT JOIN EVIDEN c ON A.MANFAATID = c.EVIDENMANFAATID 
                WHERE PERSETUJUAN2 = 0 OR PERSETUJUAN2 = NULL AND STATUSREVISI = 1").ToList();
                ViewData["get_datapersetujuan"] = get_datapersetujuan;
                //return Json(get_datapersetujuan, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var get_satker = ctx.Database.SqlQuery<Entities.DataSatker>("SELECT DISTINCT KANTORID, NAMAKANTOR from MANFAAT WHERE PERSETUJUAN1 = 0 OR PERSETUJUAN1 = NULL AND STATUSREVISI = 1").ToList();
                ViewData["get_satker"] = get_satker;

                var get_datapersetujuan = ctx.Database.SqlQuery<Entities.DataRenaksiPusat>(@"
                SELECT DISTINCT 
                    a.NAMAKANTOR, 
                    a.MANFAATID, 
                    a.KANTORID, 
                    a.NAMAPROGRAM, 
                    a.TIPE, 
                    a.NILAIANGGARAN, 
                    nvl(A .ANGGJAN, 0) AS ANGGJAN,
	                nvl(A .ANGGFEB, 0) AS ANGGFEB,
	                nvl(A .ANGGMAR, 0) AS ANGGMAR,
	                nvl(A .ANGGAPR, 0) AS ANGGAPR,
	                nvl(A .ANGGMEI, 0) AS ANGGMEI,
	                nvl(A .ANGGJUN, 0) AS ANGGJUN,
	                nvl(A .ANGGJUL, 0) AS ANGGJUL,
	                nvl(A .ANGGAGT, 0) AS ANGGAGT,
	                nvl(A .ANGGSEP, 0) AS ANGGSEP,
	                nvl(A .ANGGOKT, 0) AS ANGGOKT,
	                nvl(A .ANGGNOV, 0) AS ANGGNOV,
	                nvl(A .ANGGDES, 0) AS ANGGDES,
                    a.PROGRAMID, 
                    b.KODE, 
                    c.EVIDENPATH, 
                    a.PERSETUJUAN1, 
                    a.PERSETUJUAN2 
                FROM MANFAAT a 
                LEFT JOIN PROGRAM b on a.PROGRAMID = b.PROGRAMID 
                LEFT JOIN EVIDEN c ON A.MANFAATID = c.EVIDENMANFAATID WHERE PERSETUJUAN1 = 0 OR PERSETUJUAN1 = NULL AND STATUSREVISI = 1").ToList();
                ViewData["get_datapersetujuan"] = get_datapersetujuan;

            }

            var get_historis = ctx.Database.SqlQuery<Entities.DataRenaksiHistory>(
                @"SELECT DISTINCT
                    c.KODE,
                    b.MANFAATID,
	                b.KANTORID,
	                b.NAMAKANTOR,
	                b.NAMAPROGRAM,
	                b.TIPE,
	                b.NILAIANGGARAN,
	                nvl(A .ANGGJAN, 0) AS ANGGJAN,
	                nvl(A .ANGGFEB, 0) AS ANGGFEB,
	                nvl(A .ANGGMAR, 0) AS ANGGMAR,
	                nvl(A .ANGGAPR, 0) AS ANGGAPR,
	                nvl(A .ANGGMEI, 0) AS ANGGMEI,
	                nvl(A .ANGGJUN, 0) AS ANGGJUN,
	                nvl(A .ANGGJUL, 0) AS ANGGJUL,
	                nvl(A .ANGGAGT, 0) AS ANGGAGT,
	                nvl(A .ANGGSEP, 0) AS ANGGSEP,
	                nvl(A .ANGGOKT, 0) AS ANGGOKT,
	                nvl(A .ANGGNOV, 0) AS ANGGNOV,
	                nvl(A .ANGGDES, 0) AS ANGGDES, 
                    d.evidenpath,
                    a.persetujuan1,
                    a.keteranganpenolakan
                FROM
                    MANFAAT_RENAKSI_HISTORY a
                LEFT JOIN MANFAAT b on a.MANFAAT_MANFAATID = b.MANFAATID AND a.PROGRAMID = b.PROGRAMID
                LEFT JOIN PROGRAM c ON a.PROGRAMID = c.PROGRAMID
                LEFT JOIN EVIDEN d ON b.MANFAATID = d.EVIDENMANFAATID
                WHERE a.USERINSERT = '" + (User as Pnbp.Entities.InternalUserIdentity).PegawaiId + "'").ToList();
            ViewData["get_historis"] = get_historis;

            return View();
        }

        public ActionResult GetDataRenaksi(string kantorid)
        {
            //return Json((User as Pnbp.Entities.InternalUserIdentity).Pegawa, JsonRequestBehavior.AllowGet);

            var ctx = new PnbpContext();
            if ((User as Pnbp.Entities.InternalUserIdentity).PegawaiId == "197804302003122002")
            {
                var getdata = ctx.Database.SqlQuery<Entities.DataRenaksiPusat>("SELECT DISTINCT a.NAMAKANTOR, a.PERSETUJUAN1, a.PERSETUJUAN2, a.STATUSREVISI, a.MANFAATID, a.KANTORID, a.NAMAPROGRAM, a.TIPE, a.NILAIANGGARAN,  nvl(A .ANGGJAN, 0) AS ANGGJAN, nvl(A.ANGGFEB, 0) AS ANGGFEB, nvl(A.ANGGMAR, 0) AS ANGGMAR, nvl(A.ANGGAPR, 0) AS ANGGAPR, nvl(A.ANGGMEI, 0) AS ANGGMEI, nvl(A.ANGGJUN, 0) AS ANGGJUN, nvl(A.ANGGJUL, 0) AS ANGGJUL, nvl(A.ANGGAGT, 0) AS ANGGAGT, nvl(A.ANGGSEP, 0) AS ANGGSEP, nvl(A.ANGGOKT, 0) AS ANGGOKT, nvl(A.ANGGNOV, 0) AS ANGGNOV, nvl(A.ANGGDES, 0) AS ANGGDES, a.PROGRAMID, b.KODE, c.EVIDENPATH FROM MANFAAT a LEFT JOIN PROGRAM b on a.PROGRAMID = b.PROGRAMID LEFT JOIN EVIDEN c ON A.MANFAATID = c.EVIDENMANFAATID WHERE a.KANTORID = '" + kantorid + "' AND PERSETUJUAN2 = 0 OR PERSETUJUAN2 = NULL AND STATUSREVISI = 1").ToList();
                return Json(getdata, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var getdata = ctx.Database.SqlQuery<Entities.DataRenaksiPusat>("SELECT DISTINCT a.NAMAKANTOR, a.PERSETUJUAN1, a.PERSETUJUAN2, a.STATUSREVISI, a.MANFAATID, a.KANTORID, a.NAMAPROGRAM, a.TIPE, a.NILAIANGGARAN, nvl(a.ANGGJAN, 0) AS ANGGJAN, nvl(a.ANGGFEB, 0) AS ANGGFEB, nvl(a.ANGGMAR, 0) AS ANGGMAR, nvl(a.ANGGAPR, 0) AS ANGGAPR, nvl(a.ANGGMEI, 0) AS ANGGMEI, nvl(a.ANGGJUN, 0) AS ANGGJUN, nvl(a.ANGGJUL, 0) AS ANGGJUL, nvl(a.ANGGAGT, 0) AS ANGGAGT, nvl(a.ANGGSEP, 0) AS ANGGSEP, nvl(a.ANGGOKT, 0) AS ANGGOKT, nvl(a.ANGGNOV, 0) AS ANGGNOV, nvl(a.ANGGDES, 0) AS ANGGDES, a.PROGRAMID, b.KODE, c.EVIDENPATH FROM MANFAAT a LEFT JOIN PROGRAM b on a.PROGRAMID = b.PROGRAMID LEFT JOIN EVIDEN c ON A.MANFAATID = c.EVIDENMANFAATID WHERE a.KANTORID = '" + kantorid + "' AND PERSETUJUAN1 = 0 OR PERSETUJUAN1 = NULL AND STATUSREVISI = 1").ToList();
                return Json(getdata, JsonRequestBehavior.AllowGet);
            }



        }

        [HttpPost]
        public ActionResult UpdateRenaksiAdm(FormCollection form, string[] check, string[] id, string[] PROGRAMID,
            string[] HIST_ANGGJAN, string[] HIST_ANGGFEB, string[] HIST_ANGGMAR, string[] HIST_ANGGAPR, string[] HIST_ANGGMEI, string[] HIST_ANGGJUN,
            string[] HIST_ANGGJUL, string[] HIST_ANGGAGT, string[] HIST_ANGGSEP, string[] HIST_ANGGOKT, string[] HIST_ANGGNOV, string[] HIST_ANGGDES, string[] ketPenolakan
            )
        {
            var ctx = new PnbpContext();
            var kodesatker = ((form.AllKeys.Contains("satker")) ? form["satker"] : "NULL");
            var programid = ((form.AllKeys.Contains("programid")) ? form["programid"] : "NULL");
            var checkbox = ((form.AllKeys.Contains("approvalkaroren")) ? form["approvalkaroren"] : "NULL");

            string[] programidsplit = programid.TrimStart(',').Split(',');
            string[] checkboxsplit = checkbox.TrimStart(',').Split(',');
            string pegawaiid = (HttpContext.User.Identity as Entities.InternalUserIdentity).PegawaiId;

            for (int i = 0; i < programidsplit.Count(); i++)
            {
                //string approve = "update MANFAAT_RENAKSI set STATUSREVISI = (STATUSREVISI+1) , PERSETUJUAN1 = " + check[i] + " where KANTORID = '" + kodesatker + "' and PROGRAMID = '" + programidsplit[i] + "' ";
                string approve = "update MANFAAT set PERSETUJUAN1 = " + check[i] + " where MANFAATID = '" + id[i] + "' ";
                //string approve = "update MANFAAT_RENAKSI PERSETUJUAN1 = " + check[i] + " where MANFAATID = '" + id[i] + "' ";
                //return Json(approve, JsonRequestBehavior.AllowGet);
                ctx.Database.ExecuteSqlCommand(approve);

                var getdata = ctx.Database.SqlQuery<Entities.DataRenaksiPusat>("SELECT DISTINCT a.PERSETUJUAN1,a.PERSETUJUAN2,a.MANFAATID, a.KANTORID, a.NAMAPROGRAM, a.TIPE, a.NILAIANGGARAN, nvl(A .ANGGJAN, 0) AS ANGGJAN, nvl(A.ANGGFEB, 0) AS ANGGFEB, nvl(A.ANGGMAR, 0) AS ANGGMAR, nvl(A.ANGGAPR, 0) AS ANGGAPR, nvl(A.ANGGMEI, 0) AS ANGGMEI, nvl(A.ANGGJUN, 0) AS ANGGJUN, nvl(A.ANGGJUL, 0) AS ANGGJUL, nvl(A.ANGGAGT, 0) AS ANGGAGT, nvl(A.ANGGSEP, 0) AS ANGGSEP, nvl(A.ANGGOKT, 0) AS ANGGOKT, nvl(A.ANGGNOV, 0) AS ANGGNOV, nvl(A.ANGGDES, 0) AS ANGGDES, a.PROGRAMID, b.KODE, c.EVIDENPATH FROM MANFAAT a LEFT JOIN PROGRAM b on a.PROGRAMID = b.PROGRAMID LEFT JOIN EVIDEN c ON A.MANFAATID = c.EVIDENMANFAATID WHERE a.MANFAATID='" + id[i] + "'").FirstOrDefault();
                //return Json(getdata, JsonRequestBehavior.AllowGet);
                if (getdata.PERSETUJUAN1 == 1 && getdata.PERSETUJUAN2 == 1)
                {
                    string updstts = "update MANFAAT set STATUSREVISI = 3 where MANFAATID = '" + id[i] + "' ";
                    ctx.Database.ExecuteSqlCommand(updstts);
                }
                else if (getdata.PERSETUJUAN1 == 2)
                {
                    var getdatahistory = ctx.Database.SqlQuery<Entities.getHistoryRenaksi>("SELECT PERSETUJUAN1,PERSETUJUAN2,MANFAATID,MANFAAT_MANFAATID, KANTORID,NAMAPROGRAM, ANGGJAN, ANGGFEB, ANGGMAR, ANGGAPR, ANGGMEI, ANGGJUN, ANGGJUL, ANGGAGT, ANGGSEP, ANGGOKT, ANGGNOV, ANGGDES, PROGRAMID, INSERTDATE FROM MANFAAT_RENAKSI_HISTORY  WHERE MANFAAT_MANFAATID='" + getdata.manfaatid + "' AND STATUSREVISI=1 ORDER BY INSERTDATE DESC").FirstOrDefault();
                    //return Json(getdatahistory, JsonRequestBehavior.AllowGet);
                    string update_target = "UPDATE MANFAAT SET STATUSREVISI= 0 , ANGGJAN=" + getdatahistory.anggjan + " , ANGGFEB=" + getdatahistory.anggfeb + ",ANGGMAR=" + getdatahistory.anggmar + ",ANGGAPR=" + getdatahistory.anggapr + ",ANGGMEI=" + getdatahistory.anggmei + ",ANGGJUN=" + getdatahistory.anggjun + ",ANGGJUL=" + getdatahistory.anggjul + ",ANGGAGT=" + getdatahistory.anggagt + ",ANGGSEP=" + getdatahistory.anggsep + ",ANGGOKT=" + getdatahistory.anggokt + ",ANGGNOV=" + getdatahistory.anggnov + ",ANGGDES=" + getdatahistory.anggdes + " WHERE MANFAATID = '" + getdatahistory.manfaat_manfaatid + "'";
                    ctx.Database.ExecuteSqlCommand(update_target);
                }
                var manfaatid = NewGuID();
                string insert_histori = "INSERT INTO MANFAAT_RENAKSI_HISTORY (MANFAATID,  PROGRAMID, ANGGJAN, ANGGFEB, ANGGMAR, ANGGAPR, ANGGMEI, ANGGJUN, ANGGJUL, ANGGAGT, ANGGSEP, ANGGOKT, ANGGNOV, ANGGDES, PERSETUJUAN1, PERSETUJUAN2, MANFAAT_MANFAATID, INSERTDATE, USERINSERT, KETERANGANPENOLAKAN) VALUES ('" + manfaatid + "', '" + PROGRAMID[i] + "', " + HIST_ANGGJAN[i] + ", " + HIST_ANGGFEB[i] + ", " + HIST_ANGGMAR[i] + ", " + HIST_ANGGAPR[i] + ", " + HIST_ANGGMEI[i] + ", " + HIST_ANGGJUN[i] + ", " + HIST_ANGGJUL[i] + ", " + HIST_ANGGAGT[i] + ", " + HIST_ANGGSEP[i] + ", " + HIST_ANGGOKT[i] + ", " + HIST_ANGGNOV[i] + ", " + HIST_ANGGDES[i] + ", " + check[i] + ", 0, '" + id[i] + "', SYSDATE, '" + pegawaiid + "','" + ketPenolakan[i] + "')";
                //string insert_histori = "INSERT INTO MANFAAT_RENAKSI_HISTORY (MANFAATID, ANGGJAN, ANGGFEB, ANGGMAR, ANGGAPR, ANGGMEI, ANGGJUN, ANGGJUL, ANGGAGT, ANGGSEP, ANGGOKT, ANGGNOV, ANGGDES) VALUES ('" + manfaatid + "', " + HIST_ANGGJAN[i] + ", " + HIST_ANGGFEB[i] + ", " + HIST_ANGGMAR[i] + ", " + HIST_ANGGAPR[i] + ", " + HIST_ANGGMEI[i] + ", " + HIST_ANGGJUN[i] + ", " + HIST_ANGGJUL[i] + ", " + HIST_ANGGAGT[i] + ", " + HIST_ANGGSEP[i] + ", " + HIST_ANGGOKT[i] + ", " + HIST_ANGGNOV[i] + ", " + HIST_ANGGDES[i] + ")";
                ctx.Database.ExecuteSqlCommand(insert_histori);
            }

            return Json(1, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateRenaksiAdmP2(FormCollection form, string[] check, string[] id, string[] PROGRAMID,
            string[] HIST_ANGGJAN, string[] HIST_ANGGFEB, string[] HIST_ANGGMAR, string[] HIST_ANGGAPR, string[] HIST_ANGGMEI, string[] HIST_ANGGJUN,
            string[] HIST_ANGGJUL, string[] HIST_ANGGAGT, string[] HIST_ANGGSEP, string[] HIST_ANGGOKT, string[] HIST_ANGGNOV, string[] HIST_ANGGDES, string[] ketPenolakan)
        {
            //return Json(check, JsonRequestBehavior.AllowGet);
            var ctx = new PnbpContext();
            var kodesatker = ((form.AllKeys.Contains("satker")) ? form["satker"] : "NULL");
            var programid = ((form.AllKeys.Contains("programid")) ? form["programid"] : "NULL");
            var checkbox = ((form.AllKeys.Contains("approvalkaroren")) ? form["approvalkaroren"] : "NULL");

            string[] programidsplit = programid.TrimStart(',').Split(',');
            string[] checkboxsplit = checkbox.TrimStart(',').Split(',');
            string pegawaiid = (HttpContext.User.Identity as Entities.InternalUserIdentity).PegawaiId;
            string kantorid = (User as Entities.InternalUserIdentity).KantorId;


            for (int i = 0; i < programidsplit.Count(); i++)
            {
                //string approve = "update MANFAAT_RENAKSI set STATUSREVISI = (STATUSREVISI+1) , PERSETUJUAN2 = " + check[i] + " where MANFAATID = '" + id[i] + "' ";
                //return Json(approve, JsonRequestBehavior.AllowGet);
                string approve = "update MANFAAT set PERSETUJUAN2 = " + check[i] + " where MANFAATID = '" + id[i] + "' ";
                ctx.Database.ExecuteSqlCommand(approve);

                string log_id = NewGuID();
                //string insert_log_aktivitas = "INSERT INTO LOG_AKTIFITAS (LOG_ID, LOG_NAME, LOG_CREATE_BY, LOG_CREATE_DATE, LOG_URL, LOG_KANTORID, LOG_DATA_ID) VALUES ('" + log_id + "', 'Melakukan Persetujuan Karokeu Entri Renaksi', '" + pegawaiid + "', SYSDATE, '" + Url.Action("EntriRenaksiPusat", "Pemanfaatan") + "', '" + kantorid + "', '" + id[i] + "')";
                string insert_log_aktivitas = "INSERT INTO LOG_AKTIFITAS (LOG_ID, LOG_NAME, LOG_CREATE_BY, LOG_CREATE_DATE, LOG_URL, LOG_KANTORID, LOG_DATA_ID, LOG_TIPE) VALUES ('" + log_id + "', 'Melakukan Persetujuan Karokeu Entri Renaksi', '" + pegawaiid + "', SYSDATE, '" + Url.Action("EntriRenaksi", "Pemanfaatan") + "', '" + kantorid + "', '" + id[i] + "', 'RENAKSI')";

                ctx.Database.ExecuteSqlCommand(insert_log_aktivitas);

                var getdata = ctx.Database.SqlQuery<Entities.DataRenaksiPusat>("SELECT DISTINCT a.PERSETUJUAN1,a.PERSETUJUAN2,a.MANFAATID, a.KANTORID, a.NAMAPROGRAM, a.TIPE, a.NILAIANGGARAN, nvl(A .ANGGJAN, 0) AS ANGGJAN, nvl(A.ANGGFEB, 0) AS ANGGFEB, nvl(A.ANGGMAR, 0) AS ANGGMAR, nvl(A.ANGGAPR, 0) AS ANGGAPR, nvl(A.ANGGMEI, 0) AS ANGGMEI, nvl(A.ANGGJUN, 0) AS ANGGJUN, nvl(A.ANGGJUL, 0) AS ANGGJUL, nvl(A.ANGGAGT, 0) AS ANGGAGT, nvl(A.ANGGSEP, 0) AS ANGGSEP, nvl(A.ANGGOKT, 0) AS ANGGOKT, nvl(A.ANGGNOV, 0) AS ANGGNOV, nvl(A.ANGGDES, 0) AS ANGGDES, a.PROGRAMID, b.KODE, c.EVIDENPATH FROM MANFAAT a LEFT JOIN PROGRAM b on a.PROGRAMID = b.PROGRAMID LEFT JOIN EVIDEN c ON A.MANFAATID = c.EVIDENMANFAATID WHERE a.MANFAATID='" + id[i] + "'").FirstOrDefault();
                //return Json(getdata.PERSETUJUAN1, JsonRequestBehavior.AllowGet);
                if (getdata.PERSETUJUAN1 == 1 && getdata.PERSETUJUAN2 == 1)
                {
                    string updstts = "update MANFAAT set STATUSREVISI = 3 where MANFAATID = '" + id[i] + "' ";
                    ctx.Database.ExecuteSqlCommand(updstts);
                }
                else if (getdata.PERSETUJUAN2 == 2)
                {
                    var getdatahistory = ctx.Database.SqlQuery<Entities.getHistoryRenaksi>("SELECT PERSETUJUAN1,PERSETUJUAN2,MANFAATID,MANFAAT_MANFAATID, KANTORID,NAMAPROGRAM, ANGGJAN, ANGGFEB, ANGGMAR, ANGGAPR, ANGGMEI, ANGGJUN, ANGGJUL, ANGGAGT, ANGGSEP, ANGGOKT, ANGGNOV, ANGGDES, PROGRAMID, INSERTDATE FROM MANFAAT_RENAKSI_HISTORY  WHERE MANFAAT_MANFAATID='" + getdata.manfaatid + "' AND STATUSREVISI=1 ORDER BY INSERTDATE DESC").FirstOrDefault();
                    //return Json(getdatahistory, JsonRequestBehavior.AllowGet);
                    string update_target = "UPDATE MANFAAT SET STATUSREVISI= 0 , ANGGJAN=" + getdatahistory.anggjan + " , ANGGFEB=" + getdatahistory.anggfeb + ",ANGGMAR=" + getdatahistory.anggmar + ",ANGGAPR=" + getdatahistory.anggapr + ",ANGGMEI=" + getdatahistory.anggmei + ",ANGGJUN=" + getdatahistory.anggjun + ",ANGGJUL=" + getdatahistory.anggjul + ",ANGGAGT=" + getdatahistory.anggagt + ",ANGGSEP=" + getdatahistory.anggsep + ",ANGGOKT=" + getdatahistory.anggokt + ",ANGGNOV=" + getdatahistory.anggnov + ",ANGGDES=" + getdatahistory.anggdes + " WHERE MANFAATID = '" + getdatahistory.manfaat_manfaatid + "'";
                    ctx.Database.ExecuteSqlCommand(update_target);
                }
                var manfaatid = NewGuID();
                string insert_histori = "INSERT INTO MANFAAT_RENAKSI_HISTORY (MANFAATID,  PROGRAMID, ANGGJAN, ANGGFEB, ANGGMAR, ANGGAPR, ANGGMEI, ANGGJUN, ANGGJUL, ANGGAGT, ANGGSEP, ANGGOKT, ANGGNOV, ANGGDES, PERSETUJUAN1, PERSETUJUAN2, MANFAAT_MANFAATID, INSERTDATE, USERINSERT, KETERANGANPENOLAKAN) VALUES ('" + manfaatid + "', '" + PROGRAMID[i] + "', " + HIST_ANGGJAN[i] + ", " + HIST_ANGGFEB[i] + ", " + HIST_ANGGMAR[i] + ", " + HIST_ANGGAPR[i] + ", " + HIST_ANGGMEI[i] + ", " + HIST_ANGGJUN[i] + ", " + HIST_ANGGJUL[i] + ", " + HIST_ANGGAGT[i] + ", " + HIST_ANGGSEP[i] + ", " + HIST_ANGGOKT[i] + ", " + HIST_ANGGNOV[i] + ", " + HIST_ANGGDES[i] + ", " + check[i] + ", 0, '" + id[i] + "', SYSDATE, '" + pegawaiid + "','" + ketPenolakan[i] + "')";
                //string insert_histori = "INSERT INTO MANFAAT_RENAKSI_HISTORY (MANFAATID, ANGGJAN, ANGGFEB, ANGGMAR, ANGGAPR, ANGGMEI, ANGGJUN, ANGGJUL, ANGGAGT, ANGGSEP, ANGGOKT, ANGGNOV, ANGGDES) VALUES ('" + manfaatid + "', " + HIST_ANGGJAN[i] + ", " + HIST_ANGGFEB[i] + ", " + HIST_ANGGMAR[i] + ", " + HIST_ANGGAPR[i] + ", " + HIST_ANGGMEI[i] + ", " + HIST_ANGGJUN[i] + ", " + HIST_ANGGJUL[i] + ", " + HIST_ANGGAGT[i] + ", " + HIST_ANGGSEP[i] + ", " + HIST_ANGGOKT[i] + ", " + HIST_ANGGNOV[i] + ", " + HIST_ANGGDES[i] + ")";
                ctx.Database.ExecuteSqlCommand(insert_histori);
            }

            return Json(1, JsonRequestBehavior.AllowGet);
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

    }
}