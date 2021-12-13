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
            PersentaseAlokasi = (TotalAlokasi/ TotalPagu) * 100;

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

            decimal row = drownums+1;
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
    }
}