using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;

namespace Pnbp.Controllers
{
    [AccessDeniedAuthorize]
    public class PengembalianController : Controller
    {
        Models.PengembalianModel pengembalianmodel = new Models.PengembalianModel();

        public JsonResult GetListKantor()
        {
            List<Entities.ListKantor> result = pengembalianmodel.GetListKantor();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetListBank()
        {
            List<Entities.ListBank> result = pengembalianmodel.GetListBank();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetInfoBerkas(string nomorberkas, string kantorid)
        {
            Entities.BerkasKembalian data = new Entities.BerkasKembalian();

            if (!string.IsNullOrEmpty(nomorberkas) && !string.IsNullOrEmpty(kantorid))
            {
                string[] strNomorBerkas = nomorberkas.Split(new char[] { '/' });
                if (strNomorBerkas.Length == 2)
                {
                    string nomor = strNomorBerkas[0];
                    string tahun = strNomorBerkas[1];

                    string berkasid = pengembalianmodel.GetBerkasIdByNomorTahun(nomor, tahun, kantorid);

                    if (!string.IsNullOrEmpty(berkasid))
                    {
                        data = pengembalianmodel.GetBerkasById(berkasid);

                        if (data != null)
                        {
                            if (data.StatusBerkas == 4)
                            {
                                data.AlertInfo = "";
                            }
                            else
                            {
                                if (data.StatusBerkas == 1)
                                {
                                    data.AlertInfo = "Status berkas no. " + nomorberkas + " masih dalam proses.";
                                }
                                else if (data.StatusBerkas == 0)
                                {
                                    data.AlertInfo = "Status berkas no. " + nomorberkas + " sudah selesai.";
                                }
                            }
                        }
                        else
                        {
                            data = new Entities.BerkasKembalian();
                            data.AlertInfo = "Data pembayaran berkas no. " + nomorberkas + " tidak ditemukan.";
                        }
                    }
                    else
                    {
                        data.AlertInfo = "Data berkas no. " + nomorberkas + " tidak ditemukan.";
                    }
                }
                else
                {
                    data.AlertInfo = "Format nomor berkas yang diinput tidak valid.";
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        #region Pengajuan

        public ActionResult PengajuanPengembalian()
        {
            Entities.FindPengembalianPnbp find = new Entities.FindPengembalianPnbp();

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            int tipekantorid = pengembalianmodel.GetTipeKantor(kantoriduser);

            ViewData["tipekantorid"] = Convert.ToString(tipekantorid);

            return View(find);
        }

        public ActionResult DaftarPengajuanPengembalian(int? start, int? length, Entities.FindPengembalianPnbp f)
        {
            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            decimal? total = 0;

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;

            string namakantor = f.CariNamaKantor;
            string judul = f.CariJudul;
            string nomorberkas = f.CariNomorBerkas;
            string kodebilling = f.CariKodeBilling;
            string ntpn = f.CariNTPN;
            string namapemohon = f.CariNamaPemohon;
            string nikpemohon = f.CariNikPemohon;
            string alamatpemohon = f.CariAlamatPemohon;
            string teleponpemohon = f.CariTeleponPemohon;
            string bankpersepsi = f.CariBankPersepsi;

            List<Entities.PengembalianPnbp> result = pengembalianmodel.GetPengembalianPnbp(kantoriduser, judul, namakantor, nomorberkas, kodebilling, ntpn, namapemohon, nikpemohon, alamatpemohon, teleponpemohon, bankpersepsi, from, to);

            if (result.Count > 0)
            {
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public FileResult ExportDataPengajuanPengembalian(Entities.FindPengembalianPnbp f)
        {
            System.Data.DataTable dt = new System.Data.DataTable("DAFTARPENGAJUAN");
            dt.Columns.AddRange(new System.Data.DataColumn[16] {
                new System.Data.DataColumn("No", typeof(int)),
                new System.Data.DataColumn("Tanggal_Pengajuan"),
                new System.Data.DataColumn("Nama_Kantor"),
                new System.Data.DataColumn("Nomor_Berkas"),
                new System.Data.DataColumn("Nama_Pelayanan"),
                new System.Data.DataColumn("Kode_Billing"),
                new System.Data.DataColumn("Tanggal_Kode_Billing"),
                new System.Data.DataColumn("NTPN"),
                new System.Data.DataColumn("Tanggal_Bayar"),
                new System.Data.DataColumn("Jumlah_Bayar"),
                new System.Data.DataColumn("Nama_Bank_Persepsi"),
                new System.Data.DataColumn("Nama_Pemohon"),
                new System.Data.DataColumn("Nik_Pemohon"),
                new System.Data.DataColumn("Alamat_Pemohon"),
                new System.Data.DataColumn("Nomor_Rekening"),
                new System.Data.DataColumn("Nama_Bank_Rekening")
            });

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;

            string namakantor = f.CariNamaKantor;
            string judul = f.CariJudul;
            string nomorberkas = f.CariNomorBerkas;
            string kodebilling = f.CariKodeBilling;
            string ntpn = f.CariNTPN;
            string namapemohon = f.CariNamaPemohon;
            string nikpemohon = f.CariNikPemohon;
            string alamatpemohon = f.CariAlamatPemohon;
            string teleponpemohon = f.CariTeleponPemohon;
            string bankpersepsi = f.CariBankPersepsi;

            int jumlahdata = pengembalianmodel.JumlahPengembalianPnbp(kantoriduser, judul, namakantor, nomorberkas, kodebilling, ntpn, namapemohon, nikpemohon, alamatpemohon, teleponpemohon, bankpersepsi);

            List<Entities.PengembalianPnbp> result = pengembalianmodel.GetPengembalianPnbp(kantoriduser, judul, namakantor, nomorberkas, kodebilling, ntpn, namapemohon, nikpemohon, alamatpemohon, teleponpemohon, bankpersepsi, 0, jumlahdata);

            foreach (var rw in result)
            {
                dt.Rows.Add(
                    rw.RNumber, rw.TanggalPengaju, rw.NamaKantor, rw.NomorBerkas,
                    rw.NamaProsedur, rw.KodeBilling, rw.TanggalKodeBilling, rw.Ntpn,
                    rw.TanggalBayar, rw.JumlahBayar, rw.NamaBankPersepsi, rw.NamaPemohon, rw.NikPemohon,
                    rw.AlamatPemohon, rw.NomorRekening, rw.NamaBank);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DaftarPengajuanPengembalianPNBP.xlsx");
                }
            }
        }

        public ActionResult EntriPengajuan(string pengembalianpnbpid)
        {
            Entities.DataPengembalianPnbp data = new Entities.DataPengembalianPnbp();

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            string namakantor = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaKantor;

            if (!string.IsNullOrEmpty(pengembalianpnbpid))
            {
                // edit data
                data = pengembalianmodel.GetPengembalianPnbpById(pengembalianpnbpid);
                data.EditMode = "edit";
            }
            else
            {
                // data baru
                data.EditMode = "baru";
            }

            data.KantorIdUser = kantoriduser;
            data.SelectedKantorId = kantoriduser;
            data.NamaKantor = namakantor;

            return View(data);
        }

        public ActionResult EntriLampiran(string pengembalianpnbpid)
        {
            Entities.DataPengembalianPnbp data = new Entities.DataPengembalianPnbp();

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            string namakantor = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaKantor;

            if (!string.IsNullOrEmpty(pengembalianpnbpid))
            {
                // edit data
                data = pengembalianmodel.GetPengembalianPnbpById(pengembalianpnbpid);
                data.EditMode = "edit";
            }

            data.KantorIdUser = kantoriduser;
            data.SelectedKantorId = kantoriduser;
            data.NamaKantor = namakantor;

            return View(data);
        }

        public ActionResult LihatLampiran(string pengembalianpnbpid)
        {
            Entities.DataPengembalianPnbp data = new Entities.DataPengembalianPnbp();

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            string namakantor = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaKantor;

            if (!string.IsNullOrEmpty(pengembalianpnbpid))
            {
                // edit data
                data = pengembalianmodel.GetPengembalianPnbpById(pengembalianpnbpid);
                data.EditMode = "view";
            }

            data.KantorIdUser = kantoriduser;
            data.SelectedKantorId = kantoriduser;
            data.NamaKantor = namakantor;

            return View("EntriLampiran", data);
        }

        [HttpPost]
        public JsonResult SimpanPengembalianPnbp(Entities.DataPengembalianPnbp data)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            string userid = (HttpContext.User.Identity as Entities.InternalUserIdentity).UserId;
            data.UserId = userid;

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            string namakantor = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaKantor;
            data.KantorId = kantoriduser;
            data.NamaKantor = namakantor;

            string pegawaiid = (HttpContext.User.Identity as Entities.InternalUserIdentity).PegawaiId;
            string namapegawai = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaPegawai;
            data.PegawaiIdPengaju = pegawaiid;
            data.NamaPegawaiPengaju = namapegawai;

            tr = pengembalianmodel.SimpanPengembalianPnbp(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SimpanPersetujuanPengembalian(Entities.DataPengembalianPnbp data)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            string userid = (HttpContext.User.Identity as Entities.InternalUserIdentity).UserId;
            data.UserId = userid;

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            string namakantor = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaKantor;
            data.KantorId = kantoriduser;
            data.NamaKantor = namakantor;

            string pegawaiid = (HttpContext.User.Identity as Entities.InternalUserIdentity).PegawaiId;
            string namapegawai = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaPegawai;
            data.PegawaiIdSetuju = pegawaiid;
            data.NamaPegawaiSetuju = namapegawai;


            #region Lampiran Persetujuan
            string ekstensi = (!string.IsNullOrEmpty(data.Ekstensi)) ? data.Ekstensi.Replace(".", "") : "pdf";
            data.Ekstensi = ekstensi;
            data.NipPengupload = pegawaiid;

            var mfile = Request.Files["file"];
            if (mfile != null)
            {
                byte[] byteFile = null;
                using (var binaryReader = new System.IO.BinaryReader(mfile.InputStream))
                {
                    byteFile = binaryReader.ReadBytes(mfile.ContentLength);
                }
                if (byteFile.Length > 0)
                {
                    data.ObjectFile = byteFile;
                }
            }
            #endregion

            tr = pengembalianmodel.SimpanPersetujuanPengembalian(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusPengembalianPnbp()
        {
            var result = new Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string pengembalianpnbpid = Request.Form["pengembalianpnbpid"].ToString();
                if (!string.IsNullOrEmpty(pengembalianpnbpid))
                {
                    result = pengembalianmodel.HapusPengembalianPnbp(pengembalianpnbpid);
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

        #endregion


        #region Lampiran

        public ActionResult DaftarLampiranKembalian(string pengembalianpnbpid, string judul)
        {
            decimal? total = 0;

            List<Entities.LampiranKembalian> result = pengembalianmodel.GetLampiranKembalianForTable(pengembalianpnbpid, judul);

            if (result.Count > 0)
            {
                total = result[0].Total;
            }

            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDaftarLampiranKembalian(string id, string judul)
        {
            List<Entities.LampiranKembalian> result = pengembalianmodel.GetLampiranKembalianForTable(id, judul);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SimpanLampiranKembalian(Entities.DataPengembalianPnbp data)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            string userid = (HttpContext.User.Identity as Entities.InternalUserIdentity).UserId;
            data.UserId = userid;

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            data.KantorId = kantoriduser;

            string pegawaiid = (HttpContext.User.Identity as Entities.InternalUserIdentity).PegawaiId;
            data.NipPengupload = pegawaiid;

            // File Upload
            var filedokumen = data.FileDokumen;
            if (filedokumen == null)
            {
                tr.Pesan = "File wajib diupload";
                return Json(tr, JsonRequestBehavior.AllowGet);
            }

            string ekstensi = (!string.IsNullOrEmpty(data.Ekstensi)) ? data.Ekstensi.Replace(".", "") : "pdf";
            data.Ekstensi = ekstensi;

            var mfile = Request.Files["file"];
            if (mfile != null)
            {
                byte[] byteFile = null;
                using (var binaryReader = new System.IO.BinaryReader(mfile.InputStream))
                {
                    byteFile = binaryReader.ReadBytes(mfile.ContentLength);
                }
                if (byteFile.Length > 0)
                {
                    data.ObjectFile = byteFile;
                }
            }

            tr = pengembalianmodel.SimpanLampiranKembalian(data);

            return Json(tr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HapusLampiranKembalian()
        {
            var result = new Entities.TransactionResult() { Status = false, Pesan = "" };
            try
            {
                string lampirankembalianid = Request.Form["lampirankembalianid"].ToString();
                if (!string.IsNullOrEmpty(lampirankembalianid))
                {
                    result = pengembalianmodel.HapusLampiranKembalian(lampirankembalianid);
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

        public ActionResult GetFileLampiranById(string lampirankembalianid)
        {
            byte[] byteArray = pengembalianmodel.GetFileLampiranById(lampirankembalianid);

            System.IO.MemoryStream mss = new System.IO.MemoryStream();

            mss.Write(byteArray, 0, byteArray.Length);
            mss.Position = 0;

            var docfile = new FileStreamResult(mss, System.Net.Mime.MediaTypeNames.Application.Pdf);
            docfile.FileDownloadName = String.Concat("FileLampiran", ".pdf");

            return docfile;
        }

        #endregion


        #region Persetujuan

        public ActionResult PersetujuanPengembalian()
        {
            Entities.FindPengembalianPnbp find = new Entities.FindPengembalianPnbp();

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            int tipekantorid = pengembalianmodel.GetTipeKantor(kantoriduser);

            ViewData["tipekantorid"] = Convert.ToString(tipekantorid);

            return View(find);
        }

        public ActionResult ProsesPersetujuan(string pengembalianpnbpid)
        {
            Entities.DataPengembalianPnbp data = new Entities.DataPengembalianPnbp();

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            string namakantor = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaKantor;

            if (!string.IsNullOrEmpty(pengembalianpnbpid))
            {
                data = pengembalianmodel.GetPengembalianPnbpById(pengembalianpnbpid);
                data.EditMode = "proses";
                data.EditModeLampiran = "baru";
                data.IsStatusSetuju = (data.StatusSetuju == 1) ? true : false;

                int jumlahlampiran = pengembalianmodel.JumlahLampiran(pengembalianpnbpid, "Persetujuan");
                if (jumlahlampiran > 0)
                {
                    data.EditModeLampiran = "edit";
                }
            }

            data.KantorIdUser = kantoriduser;
            data.SelectedKantorId = kantoriduser;
            data.NamaKantor = namakantor;

            return View(data);
        }

        #endregion

    }
}