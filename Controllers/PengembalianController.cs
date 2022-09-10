using ClosedXML.Excel;
using System.Diagnostics;
using System;
using System.Collections;
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
using System.Text.RegularExpressions;
using Xceed.Words.NET;

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

        public ActionResult GenerateSuratKeteranganPengembalian(string nomorsurat, string nomorberkas, string NamaProsedur, string namapemohon, string SetoranPnbp, string JumlahBayar)
        {
            DateTime dateTime = DateTime.UtcNow.Date;
            using (MemoryStream ms = new MemoryStream())
            {
                string filename = Path.Combine(Server.MapPath(@"~/Format/"), "surat_keterangan.docx");
                DocX doc = DocX.Load(filename);

                doc.ReplaceText("{NOMOR_SURAT}", nomorsurat);
                doc.ReplaceText("{NOMOR_BERKAS}", nomorberkas);
                doc.ReplaceText("{NAMA_PROSEDUR}", NamaProsedur);
                doc.ReplaceText("{NAMA_PEMOHON}", namapemohon);
                doc.ReplaceText("{JUMLAH_SETOR}", SetoranPnbp);
                doc.ReplaceText("{JUMLAH_KELUAR}", JumlahBayar);

                doc.SaveAs(ms);

                Response.ContentType = "application/msword";
                Response.AddHeader("content-disposition", "inline; filename=" + "SuratKeterangan.docx");
                Response.AddHeader("content-length", ms.Length.ToString());
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
            return View();
        }

        public ActionResult GenerateSuratTidakTerlayani(string namapemohon, string AlamatPemohon, string nomorberkas, string NamaProsedur)
        {
            DateTime dateTime = DateTime.UtcNow.Date;
            using (MemoryStream ms = new MemoryStream())
            {
                string filename = Path.Combine(Server.MapPath(@"~/Format/"), "SuratPernyataanTidakTerlayaniNew.docx");
                DocX doc = DocX.Load(filename);

                doc.ReplaceText("{NAMA_PEMOHON}", namapemohon);
                doc.ReplaceText("{ALAMAT}", AlamatPemohon);
                doc.ReplaceText("{NOMOR_BERKAS}", nomorberkas);
                doc.ReplaceText("{UNIT_KERJA}", (User as Pnbp.Entities.InternalUserIdentity).NamaKantor.Replace("Kantor Pertanahan", ""));
                doc.ReplaceText("{NAMA_PROSEDUR}", NamaProsedur);

                doc.SaveAs(ms);

                Response.ContentType = "application/msword";
                Response.AddHeader("content-disposition", "inline; filename=" + "SuratPernyataanTidakTerlayani.docx");
                Response.AddHeader("content-length", ms.Length.ToString());
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
            return View();
        }

        //[HttpPost]
        public ActionResult GeneratePermohonanPengembalian(
            string namapemohon,
            string NomorSurat,
            string PermohonanPengembalian,
            string AlamatPemohon,
            string npwpberkas,
            string NamaRekening,
            string NomorRekening,
            string tanggalpengaju
            )
        {
            //return Json(AlamatPemohon, JsonRequestBehavior.AllowGet);
            DateTime dateTime = DateTime.UtcNow.Date;
            using (MemoryStream ms = new MemoryStream())
            {
                string filename = Path.Combine(Server.MapPath(@"~/Format/"), "PermohonanPengembalianPNBP.docx");
                DocX doc = DocX.Load(filename);

                //doc.ReplaceText("@nosprindik", (data.SprindikNumber != null) ? data.SprindikNumber : "........");
                doc.ReplaceText("{TANGGAL_BULAN_TAUN}", (!String.IsNullOrEmpty(tanggalpengaju) ? tanggalpengaju : dateTime.ToString("dd/MM/yyyy")));
                doc.ReplaceText("{NAMA_PEMOHON}", namapemohon);
                doc.ReplaceText("{UNIT_KERJA}", (User as Pnbp.Entities.InternalUserIdentity).NamaKantor.Replace("Kantor Pertanahan", ""));
                doc.ReplaceText("{ALAMAT}", AlamatPemohon);
                doc.ReplaceText("{NPWP}", npwpberkas);
                doc.ReplaceText("{NAMA_REKENING}", NamaRekening);
                doc.ReplaceText("{NOMOR_REKENING}", NomorRekening);
                doc.ReplaceText("{NOMOR_SURAT}", NomorSurat);
                doc.ReplaceText("{JUMLAH_DIMINTA}", PermohonanPengembalian);

                doc.SaveAs(ms);

                Response.ContentType = "application/msword";
                Response.AddHeader("content-disposition", "inline; filename=" + "Permohonan.docx");
                Response.AddHeader("content-length", ms.Length.ToString());
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
            return View();
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

        public ActionResult PengajuanPengembalianIndex()
        {
            Entities.FindPengembalianPnbp find = new Entities.FindPengembalianPnbp();

            List<Entities.GetSatkerList> result = pengembalianmodel.GetSatker();

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            int tipekantorid = pengembalianmodel.GetTipeKantor(kantoriduser);

            ViewData["tipekantorid"] = Convert.ToString(tipekantorid);
            ViewData["datasatker"] = result;
            ViewData["kantorid"] = kantoriduser;


            //return Json(kantoriduser, JsonRequestBehavior.AllowGet);

            return View(find);
        }

        public ActionResult PengajuanPengembalianForm()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PengajuanPengembalianForm(FormCollection form)
        {
            //return Json(form, JsonRequestBehavior.AllowGet);
            var ctx = new PnbpContext();
            PnbpContext db = new PnbpContext();
            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            string namakantor = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaKantor;
            string pegawaiid = (HttpContext.User.Identity as Entities.InternalUserIdentity).PegawaiId;
            string namapegawai = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaPegawai;

            var NomorBerkas = ((form.AllKeys.Contains("NomorBerkas")) ? form["NomorBerkas"] : "");
            var AtasNama = ((form.AllKeys.Contains("NamaPemohon")) ? form["NamaPemohon"] : "").Replace("'","");
            var Alamat = ((form.AllKeys.Contains("AlamatPemohon")) ? form["AlamatPemohon"] : "").Replace("'","");
            var NPWP = ((form.AllKeys.Contains("Npwp")) ? form["Npwp"] : "NULL");
            var KodeBiling = ((form.AllKeys.Contains("KodeBilling")) ? form["KodeBilling"] : "");
            var NTPN = ((form.AllKeys.Contains("Ntpn")) ? form["Ntpn"] : "NULL");
            var SetoranPnbp = ((form.AllKeys.Contains("SetoranPnbp")) ? form["SetoranPnbp"] : "");
            var jumlahbayar = ((form.AllKeys.Contains("JumlahBayar")) ? form["JumlahBayar"] : "");
            var PermohonanPengembalian = ((form.AllKeys.Contains("PermohonanPengembalian")) ? form["PermohonanPengembalian"] : "");
            var NamaRekening = ((form.AllKeys.Contains("NamaRekening")) ? form["NamaRekening"] : "").Replace("'", "");
            var npwpberkas = ((form.AllKeys.Contains("npwpberkas")) ? form["npwpberkas"] : "");
            var NomorRekening = ((form.AllKeys.Contains("NomorRekening")) ? form["NomorRekening"] : "");
            var NamaBank = ((form.AllKeys.Contains("NamaBank")) ? form["NamaBank"] : "").Replace("'", ""); ;
            var Status = ((form.AllKeys.Contains("Status")) ? form["Status"] : "NULL");
            //return Json(AtasNama, JsonRequestBehavior.AllowGet);
            var NomorSurat = ((form.AllKeys.Contains("NomorSurat")) ? form["NomorSurat"] : "");
            //var pengembalianid = RandomString(32);
            var pengembalianidberkas = RandomString(32);
            var pengembalianid = NewGuID();
            //var pengembalianidberkas = NewGuID();
            var TanggalPengajuan = ConvertDateNow();

            //mati
            string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
            string insert_target = "INSERT INTO PENGEMBALIANPNBP (PENGEMBALIANPNBPID, KANTORID, NAMAPEGAWAIPENGAJU, STATUSPENGEMBALIAN,NAMAKANTOR,PEGAWAIIDPENGAJU,TANGGALPENGAJU,NPWPPEGAWAIPENGAJU) " +
                                            "VALUES ('" + pengembalianid + "','" + kantoriduser + "','" + AtasNama + "','" + Status + "' ,'" + namakantor + "','" + pegawaiid + "'," + TanggalPengajuan + ",'" + NPWP + "')";
            //return Json(insert_target, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand(insert_target);
            string insert_target_berkas = "INSERT INTO BERKASKEMBALIAN (BERKASID, PENGEMBALIANPNBPID, NOMORBERKAS, NAMAPEMOHON,ALAMATPEMOHON,KODEBILLING,NTPN,JUMLAHBAYAR,NOMORREKENING,NAMABANK,NAMAREKENING,NPWP,NOMORSURAT,SETORANPNBP,PERMOHONANPENGEMBALIAN) " +
                                            "VALUES ('" + pengembalianidberkas + "','" + pengembalianid + "','" + NomorBerkas + "','" + AtasNama + "','" + Alamat + "','" + KodeBiling + "','" + NTPN + "'," + jumlahbayar.Replace(".", String.Empty) + ",'" + NomorRekening + "','" + NamaBank + "','" + NamaRekening + "','" + npwpberkas + "','" + NomorSurat + "','" + SetoranPnbp.Replace(".", String.Empty) + "','" + PermohonanPengembalian.Replace(".", String.Empty) + "')";
            db.Database.ExecuteSqlCommand(insert_target_berkas);
            //mati

            //log insert Audit Trail
            string log_id = NewGuID();
            if (Status == "1")
            {
                string insert_log_aktivitas = "INSERT INTO LOG_AKTIFITAS (LOG_ID, LOG_NAME, LOG_NOMOR_SURAT, LOG_CREATE_BY, LOG_CREATE_DATE, LOG_URL, LOG_KANTORID, LOG_DATA_ID, LOG_TIPE) VALUES ('" + log_id + "', 'Pengajuan Pengembalian PNBP Dikirim', '" + NomorSurat + "' , '" + pegawaiid + "', SYSDATE, '" + Url.Action("InputPengajuan", "Pengembalian") + "', '" + kantoriduser + "', '" + pengembalianid + "', 'PENGEMBALIANPNBP')";
                db.Database.ExecuteSqlCommand(insert_log_aktivitas);
            }
            //log insert Audit Trail

            //Lampiran Pengembalian
            var file_id = GetSequence("LAMPIRANPENGEMBALIANPNBP");

            //Surat Wajib Bayar
            var file_name10 = ((form.AllKeys.Contains("SuratWajibBayar")) ? form["SuratWajibBayar"] : "NULL");
            var fileContent10 = Request.Files["SuratWajibBayar"];
            //Insert suratwajibbayar
            if (fileContent10 != null && fileContent10.ContentLength > 0)
            {
                string id10 = RandomString(32);
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent10.InputStream;
                var FileSizeByte = fileContent10.ContentLength;
                var FileSize = FileSizeByte / 50000;
                var Extension = System.IO.Path.GetExtension(fileContent10.FileName);
                var fileName = "SuratWajibBayar" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name10 = fileName;
                    string insert_lampiran10 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES ('" + id10 + "','" + pengembalianid + "','" + Filefilepath + "','1',SYSDATE,'Pengajuan','SURAT WAJIB BAYAR','" + Extension + "')";
                    db.Database.ExecuteSqlCommand(insert_lampiran10);
                }
            }
            else
            {
                string id10 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                string insert_lampiran10 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                          "VALUES ('" + id10 + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT WAJIB BAYAR')";
                db.Database.ExecuteSqlCommand(insert_lampiran10);
                //return Json("disini coy", JsonRequestBehavior.AllowGet);
            }
            //Surat Wajib Bayar

            //Surat Pernyataan Tidak Terlayani
            var file_name11 = ((form.AllKeys.Contains("TidakTerlayani")) ? form["TidakTerlayani"] : "NULL");
            var fileContent11 = Request.Files["TidakTerlayani"];
            //Insert Surat Pernyataan Tidak Terlayani
            if (fileContent11 != null && fileContent11.ContentLength > 0)
            {
                string id11 = RandomString(32);
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent11.InputStream;
                var FileSizeByte = fileContent11.ContentLength;
                var FileSize = FileSizeByte / 50000;
                var Extension = System.IO.Path.GetExtension(fileContent11.FileName);
                var fileName = "SuratPernyataanTidakTerlayani" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name11 = fileName;
                    string insert_lampiran11 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES ('" + id11 + "','" + pengembalianid + "','" + Filefilepath + "','1',SYSDATE,'Pengajuan','SURAT PERNYATAAN TIDAK TERLAYANI','" + Extension + "')";
                    db.Database.ExecuteSqlCommand(insert_lampiran11);
                }
            }
            else
            {
                string id11 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                string insert_lampiran11 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                          "VALUES ('" + id11 + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT PERNYATAAN TIDAK TERLAYANI')";
                db.Database.ExecuteSqlCommand(insert_lampiran11);
                //return Json("disini coy", JsonRequestBehavior.AllowGet);
            }
            //Surat Pernyataan Tidak Terlayani

            var file_name1 = ((form.AllKeys.Contains("SuratPermohonan")) ? form["SuratPermohonan"] : "NULL");
            var fileContent = Request.Files["SuratPermohonan"];
            //Insert SURATPERMOHONAN
            if (fileContent != null && fileContent.ContentLength > 0)
            {
                string id1 = RandomString(32);
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent.InputStream;
                var FileSizeByte = fileContent.ContentLength;
                var FileSize = FileSizeByte / 50000;
                var Extension = System.IO.Path.GetExtension(fileContent.FileName);
                var fileName = "SuratPermohonan_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name1 = fileName;
                    string insert_lampiran1 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES ('" + id1 + "','" + pengembalianid + "','" + Filefilepath + "','1',SYSDATE,'Pengajuan','SURAT PERMOHONAN','" + Extension + "')";
                    db.Database.ExecuteSqlCommand(insert_lampiran1);
                }
            }
            else
            {
                string id1 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                string insert_lampiran1 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                          "VALUES ('" + id1 + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT PERMOHONAN')";
                db.Database.ExecuteSqlCommand(insert_lampiran1);
                //return Json("disini coy", JsonRequestBehavior.AllowGet);
            }

            var file_name2 = ((form.AllKeys.Contains("SuratKeterangan")) ? form["SuratKeterangan"] : "NULL");
            var fileContent2 = Request.Files["SuratKeterangan"];
            //Insert SURATPERMOHONAN
            if (fileContent2 != null && fileContent2.ContentLength > 0)
            {
                string id2 = RandomString(32);
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent2.InputStream;
                var FileSizeByte = fileContent2.ContentLength;
                var FileSize = FileSizeByte / 50000;
                var Extension = System.IO.Path.GetExtension(fileContent2.FileName);
                var fileName = "SuratKeterangan_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath2 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name2 = fileName;
                    string insert_lampiran2 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES ('" + id2 + "','" + pengembalianid + "','" + Filefilepath2 + "','1',SYSDATE,'Pengajuan','SURAT KETERANGAN','" + Extension + "')";
                    db.Database.ExecuteSqlCommand(insert_lampiran2);
                }
            }
            else
            {
                string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                          "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT KETERANGAN')";
                db.Database.ExecuteSqlCommand(insert_lampiran);
            }

            var file_name3 = ((form.AllKeys.Contains("BuktiPenerimaan")) ? form["BuktiPenerimaan"] : "NULL");
            var fileContent3 = Request.Files["BuktiPenerimaan"];
            //Insert SURATPERMOHONAN
            if (fileContent3 != null && fileContent3.ContentLength > 0)
            {
                string id3 = RandomString(32);
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent3.InputStream;
                var FileSizeByte = fileContent3.ContentLength;
                var FileSize = FileSizeByte / 50000;
                var Extension = System.IO.Path.GetExtension(fileContent3.FileName);
                var fileName = "BuktiPenerimaan_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath3 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name3 = fileName;
                    string insert_lampiran3 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES ('" + id3 + "','" + pengembalianid + "','" + Filefilepath3 + "','1',SYSDATE,'Pengajuan','BUKTI PENERIMAAN NEGARA','" + Extension + "')";
                    db.Database.ExecuteSqlCommand(insert_lampiran3);
                }
            }
            else
            {
                string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                          "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','BUKTI PENERIMAAN NEGARA')";
                db.Database.ExecuteSqlCommand(insert_lampiran);
            }

            var file_name4 = ((form.AllKeys.Contains("SuratPerintah")) ? form["SuratPerintah"] : "NULL");
            var fileContent4 = Request.Files["SuratPerintah"];
            //Insert SURATPERMOHONAN
            if (fileContent4 != null && fileContent4.ContentLength > 0)
            {
                string id4 = RandomString(32);
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent4.InputStream;
                var FileSizeByte = fileContent4.ContentLength;
                var FileSize = FileSizeByte / 50000;
                var Extension = System.IO.Path.GetExtension(fileContent4.FileName);
                var fileName = "SuratPerintah_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath4 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name4 = fileName;
                    string insert_lampiran4 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES ('" + id4 + "','" + pengembalianid + "','" + Filefilepath4 + "','1',SYSDATE,'Pengajuan','SURAT PERINTAH SETOR','" + Extension + "')";
                    db.Database.ExecuteSqlCommand(insert_lampiran4);
                }
            }
            else
            {
                string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                          "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT PERINTAH SETOR')";
                db.Database.ExecuteSqlCommand(insert_lampiran);
            }

            var file_name5 = ((form.AllKeys.Contains("BuktiSetor")) ? form["BuktiSetor"] : "NULL");
            var fileContent5 = Request.Files["BuktiSetor"];
            //Insert SURATPERMOHONAN
            if (fileContent5 != null && fileContent5.ContentLength > 0)
            {
                string id5 = RandomString(32);
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent5.InputStream;
                var FileSizeByte = fileContent5.ContentLength;
                var FileSize = FileSizeByte / 50000;
                var Extension = System.IO.Path.GetExtension(fileContent5.FileName);
                var fileName = "BuktiSetor_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath5 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name5 = fileName;
                    string insert_lampiran5 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES ('" + id5 + "','" + pengembalianid + "','" + Filefilepath5 + "','1',SYSDATE,'Pengajuan','SURAT BUKTI SETOR','" + Extension + "')";
                    db.Database.ExecuteSqlCommand(insert_lampiran5);
                }
            }
            else
            {
                string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                          "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT BUKTI SETOR')";
                db.Database.ExecuteSqlCommand(insert_lampiran);
            }

            var file_name6 = ((form.AllKeys.Contains("BuktiRek")) ? form["BuktiRek"] : "NULL");
            var fileContent6 = Request.Files["BuktiRek"];
            //Insert SURATPERMOHONAN
            if (fileContent6 != null && fileContent6.ContentLength > 0)
            {
                string id6 = RandomString(32);
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent6.InputStream;
                var FileSizeByte = fileContent6.ContentLength;
                var FileSize = FileSizeByte / 50000;
                var Extension = System.IO.Path.GetExtension(fileContent6.FileName);
                var fileName = "BuktiRek_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath6 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name6 = fileName;
                    string insert_lampiran6 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES ('" + id6 + "','" + pengembalianid + "','" + Filefilepath6 + "','1',SYSDATE,'Pengajuan','BUKTI KEPEMILIKAN REK TUJUAN','" + Extension + "')";
                    db.Database.ExecuteSqlCommand(insert_lampiran6);
                }
            }
            else
            {
                string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                          "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','BUKTI KEPEMILIKAN REK TUJUAN')";
                db.Database.ExecuteSqlCommand(insert_lampiran);
            }

            var file_name7 = ((form.AllKeys.Contains("NpwpFile")) ? form["NpwpFile"] : "NULL");
            var fileContent7 = Request.Files["NpwpFile"];
            //Insert SURATPERMOHONAN
            if (fileContent7 != null && fileContent7.ContentLength > 0)
            {
                string id7 = RandomString(32);
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent7.InputStream;
                var FileSizeByte = fileContent7.ContentLength;
                var FileSize = FileSizeByte / 50000;
                var Extension = System.IO.Path.GetExtension(fileContent7.FileName);
                var fileName = "NpwpFile_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath7 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name7 = fileName;
                    string insert_lampiran7 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES ('" + id7 + "','" + pengembalianid + "','" + Filefilepath7 + "','1',SYSDATE,'Pengajuan','NPWP','" + Extension + "')";
                    db.Database.ExecuteSqlCommand(insert_lampiran7);
                }
            }
            else
            {
                string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                          "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','NPWP')";
                db.Database.ExecuteSqlCommand(insert_lampiran);
            }

            var file_name8 = ((form.AllKeys.Contains("BuktiDomisili")) ? form["BuktiDomisili"] : "NULL");
            var fileContent8 = Request.Files["BuktiDomisili"];
            //Insert SURATPERMOHONAN
            if (fileContent8 != null && fileContent8.ContentLength > 0)
            {
                string id8 = RandomString(32);
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent8.InputStream;
                var FileSizeByte = fileContent8.ContentLength;
                var FileSize = FileSizeByte / 50000;
                var Extension = System.IO.Path.GetExtension(fileContent8.FileName);
                var fileName = "BuktiDomisili_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath8 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name8 = Filefilepath8;
                    string insert_lampiran8 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES ('" + id8 + "','" + pengembalianid + "','" + Filefilepath8 + "','1',SYSDATE,'Pengajuan','BUKTI DOMISILI PEMOHON','" + Extension + "')";
                    db.Database.ExecuteSqlCommand(insert_lampiran8);
                }
            }
            else
            {
                string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                          "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','BUKTI DOMISILI PEMOHON')";
                db.Database.ExecuteSqlCommand(insert_lampiran);
            }

            var file_name9 = ((form.AllKeys.Contains("SuratKuasa")) ? form["SuratKuasa"] : "NULL");
            var fileContent9 = Request.Files["SuratKuasa"];
            //Insert SURATPERMOHONAN
            if (fileContent9 != null && fileContent9.ContentLength > 0)
            {
                string id9 = RandomString(32);
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent9.InputStream;
                var FileSizeByte = fileContent9.ContentLength;
                var FileSize = FileSizeByte / 50000;
                var Extension = System.IO.Path.GetExtension(fileContent9.FileName);
                var fileName = "SuratKuasa_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath9 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name9 = Filefilepath9;

                    string insert_lampiran9 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES ('" + id9 + "','" + pengembalianid + "','" + Filefilepath9 + "','1',SYSDATE,'Pengajuan','SURAT KUASA BERMATERAI','" + Extension + "')";
                    db.Database.ExecuteSqlCommand(insert_lampiran9);
                }
            }
            else
            {
                string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                          "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT KUASA BERMATERAI')";
                db.Database.ExecuteSqlCommand(insert_lampiran);
            }
            //string insert_lampiran = "INSERT INTO PNBPTRAIN.LAMPIRANPENGEMBALIANPNBP (LAMPIRANID, LAMPIRANPENGEMBALIANID, SURATPERMOHONAN, SURATKETERANGAN, BUKTIPENERIMAAN, PERINTAHSETOR, BUKTISETOR,BUKTIREKENING,BUKTINPWP,BUKTIDOMISILI,SURATKUASA,STATUS) " +
            //                                "VALUES ((SELECT RAWTOHEX(SYS_GUID()) FROM DUAL)" + "','" + pengembalianid + "','" + file_name1 + "','" + file_name2 + "','" + file_name3 + "','" + file_name4 + "','" + file_name5 + "','"+file_name6+"','"+fileContent7+"','"+fileContent8+"','"+fileContent9+"',1)";
            //db.Database.ExecuteSqlCommand(insert_lampiran);

            //mati
            if (ModelState.IsValid)
            {
                TempData["Upload"] = "Data Berhasil Disimpan";
                return RedirectToAction("PengajuanPengembalianIndex");
            }
            else
            {

            }

            return RedirectToAction("PengajuanPengembalianIndex");
            //mati
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static String ConvertDateNow()
        {
            string datanow = "TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd") + "', 'yyyy-mm-dd hh24:mi:ss')";

            return datanow;
        }

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

        public ActionResult DaftarSatker()
        {
            List<Entities.GetSatkerList> result = pengembalianmodel.GetSatker();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarPengajuanPengembalianTrain(int? start, int? length, Entities.FindPengembalianPnbp f)
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
            string status = f.CariStatus;
            string namasatker = f.CariNamaSatker;
            string kodesatker = f.CariKodeSatker;
            int tipekantorid = pengembalianmodel.GetTipeKantor(kantoriduser);

            List<Entities.PengembalianPnbpTrain> result = pengembalianmodel.GetPengembalianPnbpTrain(tipekantorid, kantoriduser, judul, namakantor, nomorberkas, kodebilling, ntpn, namapemohon, nikpemohon, alamatpemohon, teleponpemohon, bankpersepsi, status, namasatker, kodesatker, from, to);

            if (result.Count > 0)
            {
                total = result[0].Total;
            }
            //return Json(result, JsonRequestBehavior.AllowGet);
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        //public ActionResult ExportPengembalian(string pNamaPemohon, string pStatus)
        public ActionResult ExportPengembalian(string pNamaPemohon, string pStatus, string pNamaSatker)
        {


            //pTahun = "2019";
            //int result = 1;
            {
                string kantorid = (User as Entities.InternalUserIdentity).KantorId;
                string NamaKantor = (User as Entities.InternalUserIdentity).NamaKantor;
                string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

                PnbpContext db = new PnbpContext();
                Pnbp.Models.AdmModel _pm = new Models.AdmModel();
                Entities.FilterPengembalian _frm = new Entities.FilterPengembalian();
                _frm.namapemohon = (!string.IsNullOrEmpty(pNamaPemohon)) ? pNamaPemohon : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
                DataTable dt = new DataTable("Sheet1");
                dt.Columns.AddRange(new DataColumn[10] {
                new DataColumn("No",typeof(int)),
                new DataColumn("Kode_Satker"),
                new DataColumn("Nama_Satker"),
                new DataColumn("Nama_Pemohon"),
                new DataColumn("Nomor_Berkas"),
                new DataColumn("Nomor_Surat"),
                new DataColumn("Nama_Bank"),
                new DataColumn("Nominal_Pengajuan",typeof(decimal)),
                new DataColumn("Tanggal_Pengajuan"),
                new DataColumn("Status")});

                //List<Entities.Penerimaan> result = _pm.GetPenerimaanNasional(pTahun);
                ArrayList arrayListParameters = new ArrayList();
                string query =
                  @"
                    SELECT * FROM (SELECT DISTINCT
	                    b.KODE,
	                    b.KODESATKER,
	                    b.NAMA_SATKER,
	                    c.NAMAPEMOHON,
	                    c.NOMORBERKAS,
	                    c.NOMORSURAT,
	                    c.NAMABANK,
	                    c.JUMLAHBAYAR,
	                    a.TANGGALPENGAJU,
	                    a.STATUSPENGEMBALIAN,
	
	                    CASE 
	                        WHEN a.STATUSPENGEMBALIAN = 0 THEN 'Belum Kirim'
	                        WHEN a.STATUSPENGEMBALIAN = 1 THEN 'Usulan Baru'
	                        WHEN a.STATUSPENGEMBALIAN = 2 THEN 'Proses'
	                        WHEN a.STATUSPENGEMBALIAN = 3 THEN 'Berkas Tidak Lengkap'
	                        WHEN a.STATUSPENGEMBALIAN = 4 THEN 'Selesai'
	                    END AS STATUSTEXT,

	                    row_number () over ( ORDER BY b.KODESATKER ASC ) AS URUTAN 
                    FROM
	                    PENGEMBALIANPNBP a
	                    JOIN satker b ON b.kantorid = a.kantorid
	                    JOIN berkaskembalian c ON c.pengembalianpnbpid = a.pengembalianpnbpid
                        WHERE NOMORSURAT IS NOT NULL ";

                //query = sWhitespace.Replace(query, " ");

                if (!String.IsNullOrEmpty(pNamaPemohon))
                {
                    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("namapemohon", String.Concat("%", pNamaPemohon.ToLower(), "%")));
                    //query += " WHERE c.NAMAPEMOHON LIKE '%" + pNamaPemohon + "%'";
                    query += " AND UPPER(c.NAMAPEMOHON) LIKE '%" + pNamaPemohon.ToUpper() + "%'";
                    //query += " WHERE LOWER(c.NAMAPEMOHON) LIKE :namapemohon ";
                    //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pBulan));
                }

                if (!String.IsNullOrEmpty(pStatus))
                {
                    query += " AND a.STATUSPENGEMBALIAN = " + pStatus;
                    //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pBulan));
                }

                if (@OtorisasiUser.GetJenisKantorUser() == "Kantah")
                {
                    query += " AND b.KANTORID ='" + kantorid + "'";
                }
                else if (@OtorisasiUser.GetJenisKantorUser() == "Pusat")
                {
                    if (!String.IsNullOrEmpty(pNamaSatker))
                    {
                        query += " AND b.KODESATKER = " + pNamaSatker;
                        //    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pBulan));
                    }
                }

                query += " ORDER BY URUTAN ASC)";

                //return Json(query, JsonRequestBehavior.AllowGet);

                var get = db.Database.SqlQuery<Entities.ExportPengembalian>(query).ToList();

                foreach (var rw in get)
                {
                    dt.Rows.Add(rw.urutan, rw.kode, rw.nama_satker, rw.namapemohon, rw.nomorberkas, rw.nomorsurat, rw.namabank, rw.jumlahbayar, rw.tanggalpengaju, rw.statustext);
                    //dt.Rows.Add(rw.urutan, rw.kode, rw.namakantor, rw.namaprosedur, rw.nomorberkas, rw.jenispenerimaan, rw.tanggal, rw.kodepenerimaan, rw.bankpersepsiid, rw.kodebilling, rw.ntpn, rw.jumlah, rw.penerimaan, rw.operasional);
                }

                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Daftar_Pengajuan_Permohonan" + DateTime.Now.ToString("yyyy-mm-dd") + ".xlsx");
                    }
                }
            }
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

        public ActionResult InputPengajuan()
        {
            Entities.DataPengembalianPnbpDev data = new Entities.DataPengembalianPnbpDev();

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            string namakantor = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaKantor;


            data.KantorIdUser = kantoriduser;
            data.SelectedKantorId = kantoriduser;
            data.NamaKantor = namakantor;

            return View(data);
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
        public JsonResult SimpanPengembalianPnbpDev(Entities.DataPengembalianPnbpDev data)
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

            tr = pengembalianmodel.SimpanPengembalianPnbpDev(data);

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

        public static int GetSequence(String table_)
        {
            using (var db = new PnbpContext())
            {
                int id = db.Database.SqlQuery<int>("SELECT " + table_ + "_SEQ.NEXTVAL FROM DUAL").SingleOrDefault();
                return id;
            }
        }

        [HttpPost]
        public ActionResult GetBerkasDataByID(FormCollection form)
        {
            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            var noberkas = (form.AllKeys.Contains("noberkas") ? form["noberkas"] : "");


            string tipe = OtorisasiUser.GetJenisKantorUser();
            
            if(tipe == "Kantah")
            {
                Entities.GetDataBerkasForm result = pengembalianmodel.GetDataBerkasByNo(noberkas, kantorid);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else if(tipe == "Kanwil")
            {
                Entities.GetDataBerkasForm result = pengembalianmodel.GetDataBerkasByNoKanwil(noberkas, kantorid);
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return View();
        }

        //public ActionResult PengajuanPengembalianDetail(string pengembalianpnbpid)
        //{
        //    return Json(pengembalianpnbpid, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult PengajuanPengembalianDetail(string pengembalianpnbpid)
        {
            //return Json(pengembalianpnbpid, JsonRequestBehavior.AllowGet);

            Entities.DetailDataBerkas data = new Entities.DetailDataBerkas();
            Entities.LampiranKembalianTrain file1 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file2 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file3 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file4 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file5 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file6 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file7 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file8 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file9 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file10 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file11 = new Entities.LampiranKembalianTrain();
            data = pengembalianmodel.GetDataPengembalianPnbpById(pengembalianpnbpid);
            file1 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT PERMOHONAN");
            file2 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT KETERANGAN");
            file3 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "BUKTI PENERIMAAN NEGARA");
            file4 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT PERINTAH SETOR");
            file5 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT BUKTI SETOR");
            file6 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "BUKTI KEPEMILIKAN REK TUJUAN");
            file7 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "NPWP");
            file8 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "BUKTI DOMISILI PEMOHON");
            file9 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT KUASA BERMATERAI");
            file10 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT WAJIB BAYAR");
            file11 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT PERNYATAAN TIDAK TERLAYANI");
            ViewData["file1"] = file1;
            ViewData["file2"] = file2;
            ViewData["file3"] = file3;
            ViewData["file4"] = file4;
            ViewData["file5"] = file5;
            ViewData["file6"] = file6;
            ViewData["file7"] = file7;
            ViewData["file8"] = file8;
            ViewData["file9"] = file9;
            ViewData["file10"] = file10;
            ViewData["file11"] = file11;
            ViewData["Pengembaliandata"] = data;
            //return View();
            return View("pengembaliandaerah");
        }

        public ActionResult PengajuanPengembalianDetailPusat(string pengembalianpnbpid)
        {
            Entities.DetailDataBerkas data = new Entities.DetailDataBerkas();
            Entities.LampiranKembalianTrain file1 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file2 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file3 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file4 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file5 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file6 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file7 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file8 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file9 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file10 = new Entities.LampiranKembalianTrain();
            Entities.LampiranKembalianTrain file11 = new Entities.LampiranKembalianTrain();
            data = pengembalianmodel.GetDataPengembalianPnbpById(pengembalianpnbpid);
            file1 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT PERMOHONAN");
            file2 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT KETERANGAN");
            file3 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "BUKTI PENERIMAAN NEGARA");
            file4 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT PERINTAH SETOR");
            file5 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT BUKTI SETOR");
            file6 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "BUKTI KEPEMILIKAN REK TUJUAN");
            file7 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "NPWP");
            file8 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "BUKTI DOMISILI PEMOHON");
            file9 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT KUASA BERMATERAI");
            file10 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT WAJIB BAYAR");
            file11 = pengembalianmodel.GetlampiranPengajuanKembalian(pengembalianpnbpid, "SURAT PERNYATAAN TIDAK TERLAYANI");
            //return Json(file10, JsonRequestBehavior.AllowGet);
            ViewData["file1"] = file1;
            ViewData["file2"] = file2;
            ViewData["file3"] = file3;
            ViewData["file4"] = file4;
            ViewData["file5"] = file5;
            ViewData["file6"] = file6;
            ViewData["file7"] = file7;
            ViewData["file8"] = file8;
            ViewData["file9"] = file9;
            ViewData["file10"] = file10;
            ViewData["file11"] = file11;
            ViewData["Pengembaliandata"] = data;
            //return Json(file9, JsonRequestBehavior.AllowGet);
            //return View();
            return View("pengembalianpusat");
        }

        [HttpPost]
        public ActionResult PengajuanPengembalianDetail(FormCollection form)
        {
            //return Json(form, JsonRequestBehavior.AllowGet);
            var ctx = new PnbpContext();
            PnbpContext db = new PnbpContext();
            var pengembalianpnbpid = ((form.AllKeys.Contains("pengembalianpnbpid")) ? form["pengembalianpnbpid"] : "");
            var fileid1 = ((form.AllKeys.Contains("fileid1")) ? form["fileid1"] : "");
            var fileid2 = ((form.AllKeys.Contains("fileid2")) ? form["fileid2"] : "");
            var fileid3 = ((form.AllKeys.Contains("fileid3")) ? form["fileid3"] : "");
            var fileid4 = ((form.AllKeys.Contains("fileid4")) ? form["fileid4"] : "");
            var fileid5 = ((form.AllKeys.Contains("fileid5")) ? form["fileid5"] : "");
            var fileid6 = ((form.AllKeys.Contains("fileid6")) ? form["fileid6"] : "");
            var fileid7 = ((form.AllKeys.Contains("fileid7")) ? form["fileid7"] : "");
            var fileid8 = ((form.AllKeys.Contains("fileid8")) ? form["fileid8"] : "");
            var fileid9 = ((form.AllKeys.Contains("fileid9")) ? form["fileid9"] : "");
            var fileid10 = ((form.AllKeys.Contains("fileid10")) ? form["fileid10"] : "");
            var fileid11 = ((form.AllKeys.Contains("fileid11")) ? form["fileid11"] : "");
            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            string namakantor = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaKantor;
            string pegawaiid = (HttpContext.User.Identity as Entities.InternalUserIdentity).PegawaiId;
            string namapegawai = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaPegawai;

            var NomorBerkas = ((form.AllKeys.Contains("NomorBerkas")) ? form["NomorBerkas"] : "");
            var AtasNama = ((form.AllKeys.Contains("NamaPemohon")) ? form["NamaPemohon"] : "");
            var Alamat = ((form.AllKeys.Contains("AlamatPemohon")) ? form["AlamatPemohon"] : "");
            var NPWP = ((form.AllKeys.Contains("Npwp")) ? form["Npwp"] : "NULL");
            var npwpberkas = ((form.AllKeys.Contains("npwpberkas")) ? form["npwpberkas"] : "NULL");
            var KodeBiling = ((form.AllKeys.Contains("KodeBilling")) ? form["KodeBilling"] : "");
            var NTPN = ((form.AllKeys.Contains("Ntpn")) ? form["Ntpn"] : "NULL");
            var SetoranPnbp = ((form.AllKeys.Contains("SetoranPnbp")) ? form["SetoranPnbp"] : "");
            var BiayaLayanan = ((form.AllKeys.Contains("JumlahBayar")) ? form["JumlahBayar"] : "");
            var PermohonanPengembalian = ((form.AllKeys.Contains("PermohonanPengembalian")) ? form["PermohonanPengembalian"] : "");
            var NamaRekening = ((form.AllKeys.Contains("NamaRekening")) ? form["NamaRekening"] : "");
            var NomorRekening = ((form.AllKeys.Contains("NomorRekening")) ? form["NomorRekening"] : "");
            var NamaBank = ((form.AllKeys.Contains("NamaBank")) ? form["NamaBank"] : "");
            var Status = ((form.AllKeys.Contains("Status")) ? form["Status"] : "");
            var NomorSurat = ((form.AllKeys.Contains("NomorSurat")) ? form["NomorSurat"] : "");
            //var pengembalianid = RandomString(32);
            //var pengembalianidberkas = RandomString(32);
            var pengembalianid = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
            var pengembalianidberkas = db.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
            var TanggalPengajuan = ConvertDateNow();

            string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
            //string insert_target = "UPDATE PNBPTRAIN.PENGEMBALIANPNBP SET NAMAPEGAWAIPENGAJU='"+AtasNama+"', STATUSPENGEMBALIAN='"+ Status + "',NAMAKANTOR='"+namakantor+"',PEGAWAIIDPENGAJU='"+pegawaiid+"',TANGGALPENGAJU='"+TanggalPengajuan+"' WHERE PENGEMBALIANPNBPID = '"+ pengembalianpnbpid + "'";
            string insert_target = "UPDATE PENGEMBALIANPNBP SET NAMAPEGAWAIPENGAJU='" + AtasNama + "' , STATUSPENGEMBALIAN='" + Status + "',NAMAKANTOR='" + namakantor + "',PEGAWAIIDPENGAJU='" + pegawaiid + "',NPWPPEGAWAIPENGAJU='" + NPWP + "' WHERE PENGEMBALIANPNBPID = '" + pengembalianpnbpid + "'";
            db.Database.ExecuteSqlCommand(insert_target);
            string insert_target_berkas = "UPDATE BERKASKEMBALIAN SET JUMLAHBAYAR='" + BiayaLayanan.Replace(".", String.Empty) + "',NOMORREKENING='" + NomorRekening + "',NAMABANK='" + NamaBank + "',NAMAREKENING='" + NamaRekening + "',NPWP='" + npwpberkas + "',NOMORSURAT='" + NomorSurat + "',SETORANPNBP='" + SetoranPnbp.Replace(".", String.Empty) + "',PERMOHONANPENGEMBALIAN='" + PermohonanPengembalian.Replace(".", String.Empty) + "' WHERE PENGEMBALIANPNBPID='" + pengembalianpnbpid + "'";
            db.Database.ExecuteSqlCommand(insert_target_berkas);

            //log insert Audit Trail
            string log_id = NewGuID();
            if (Status == "0")
            {
                string insert_log_aktivitas = "INSERT INTO LOG_AKTIFITAS (LOG_ID, LOG_NAME, LOG_CREATE_BY, LOG_CREATE_DATE, LOG_URL, LOG_KANTORID, LOG_DATA_ID) VALUES ('" + log_id + "', 'Pengajuan Pengembalian PNBP Disimpan', '" + pegawaiid + "', SYSDATE, '" + Url.Action("PengajuanPengembalianDetail", "Pengembalian") + "', '" + kantoriduser + "', '" + pengembalianid + "')";
                db.Database.ExecuteSqlCommand(insert_log_aktivitas);
            }
            else if (Status == "1")
            {
                string insert_log_aktivitas = "INSERT INTO LOG_AKTIFITAS (LOG_ID, LOG_NAME, LOG_CREATE_BY, LOG_CREATE_DATE, LOG_URL, LOG_KANTORID, LOG_DATA_ID) VALUES ('" + log_id + "', 'Pengajuan Pengembalian PNBP Dikirim', '" + pegawaiid + "', SYSDATE, '" + Url.Action("PengajuanPengembalianDetail", "Pengembalian") + "', '" + kantoriduser + "', '" + pengembalianid + "')";
                db.Database.ExecuteSqlCommand(insert_log_aktivitas);
            }
            //log insert Audit Trail


            //Lampiran Pengembalian
            var file_id = GetSequence("LAMPIRANPENGEMBALIANPNBP");

            //Surat Wajib Bayar
            var file_name10 = ((form.AllKeys.Contains("SuratWajibBayar")) ? form["SuratWajibBayar"] : "NULL");
            var fileContent10 = Request.Files["SuratWajibBayar"];
            //Insert suratwajibbayar
            if (fileContent10 != null && fileContent10.ContentLength > 0)
            {
                string id10 = RandomString(32);
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent10.InputStream;
                var FileSizeByte = fileContent10.ContentLength;
                var FileSize = FileSizeByte / 50000;
                var Extension = System.IO.Path.GetExtension(fileContent10.FileName);
                var fileName = "SuratWajibBayar" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name10 = fileName;
                    string insert_lampiran10 = "UPDATE LAMPIRANKEMBALIAN SET NAMAFILE='" + Filefilepath + "', STATUSLAMPIRAN='1',TANGGAL=SYSDATE,EKSTENSI='" + Extension + "' WHERE LAMPIRANKEMBALIANID='" + fileid10 + "'";
                    db.Database.ExecuteSqlCommand(insert_lampiran10);
                }
            }
            else
            {
                string id10 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                string insert_lampiran10 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                          "VALUES ('" + id10 + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT WAJIB BAYAR')";
                db.Database.ExecuteSqlCommand(insert_lampiran10);
                //return Json("disini coy", JsonRequestBehavior.AllowGet);
            }
            //Surat Wajib Bayar

            //Surat Pernyataan Tidak Terlayani
            var file_name11 = ((form.AllKeys.Contains("TidakTerlayani")) ? form["TidakTerlayani"] : "NULL");
            var fileContent11 = Request.Files["TidakTerlayani"];
            //Insert Surat Pernyataan Tidak Terlayani
            if (fileContent11 != null && fileContent11.ContentLength > 0)
            {
                string id11 = RandomString(32);
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent11.InputStream;
                var FileSizeByte = fileContent11.ContentLength;
                var FileSize = FileSizeByte / 50000;
                var Extension = System.IO.Path.GetExtension(fileContent11.FileName);
                var fileName = "SuratPernyataanTidakTerlayani" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name11 = fileName;
                    string insert_lampiran11 = "UPDATE LAMPIRANKEMBALIAN SET NAMAFILE='" + Filefilepath + "', STATUSLAMPIRAN='1',TANGGAL=SYSDATE,EKSTENSI='" + Extension + "' WHERE LAMPIRANKEMBALIANID='" + fileid11 + "'";
                    db.Database.ExecuteSqlCommand(insert_lampiran11);
                }
            }
            else
            {
                string id11 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                string insert_lampiran11 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                          "VALUES ('" + id11 + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT PERNYATAAN TIDAK TERLAYANI')";
                db.Database.ExecuteSqlCommand(insert_lampiran11);
                //return Json("disini coy", JsonRequestBehavior.AllowGet);
            }
            //Surat Pernyataan Tidak Terlayani

            var file_name1 = ((form.AllKeys.Contains("SuratPermohonan")) ? form["SuratPermohonan"] : "NULL");
            var fileContent = Request.Files["SuratPermohonan"];
            //Insert SURATPERMOHONAN
            if (fileContent != null && fileContent.ContentLength > 0)
            {
                string id1 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent.InputStream;
                var FileSizeByte = fileContent.ContentLength;
                var FileSize = FileSizeByte / 3000;
                var Extension = System.IO.Path.GetExtension(fileContent.FileName);
                var fileName = "SuratPermohonan_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name1 = fileName;
                    string insert_lampiran1 = "UPDATE LAMPIRANKEMBALIAN SET NAMAFILE='" + Filefilepath + "', STATUSLAMPIRAN='1',TANGGAL=SYSDATE,EKSTENSI='" + Extension + "' WHERE LAMPIRANKEMBALIANID='" + fileid1 + "'";
                    db.Database.ExecuteSqlCommand(insert_lampiran1);
                }
            }
            else {
                if (String.IsNullOrEmpty(fileid1))
                {
                    string id1 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                    string insert_lampiran1 = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                              "VALUES ('" + id1 + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT PERMOHONAN')";
                    db.Database.ExecuteSqlCommand(insert_lampiran1);
                }
            }

            var file_name2 = ((form.AllKeys.Contains("SuratKeterangan")) ? form["SuratKeterangan"] : "");
            var fileContent2 = Request.Files["SuratKeterangan"];
            //Insert SURATPERMOHONAN
            if (fileContent2 != null && fileContent2.ContentLength > 0)
            {
                string id2 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent2.InputStream;
                var FileSizeByte = fileContent2.ContentLength;
                var FileSize = FileSizeByte / 3000;
                var Extension = System.IO.Path.GetExtension(fileContent2.FileName);
                var fileName = "SuratKeterangan_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath2 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name2 = fileName;
                    string insert_lampiran2 = "UPDATE LAMPIRANKEMBALIAN SET NAMAFILE='" + Filefilepath2 + "', STATUSLAMPIRAN='1',TANGGAL=SYSDATE,EKSTENSI='" + Extension + "' WHERE LAMPIRANKEMBALIANID='" + fileid2 + "'";
                    db.Database.ExecuteSqlCommand(insert_lampiran2);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(fileid2))
                {
                    string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                    string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                              "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT KETERANGAN')";
                    db.Database.ExecuteSqlCommand(insert_lampiran);
                }
            }

            var file_name3 = ((form.AllKeys.Contains("BuktiPenerimaan")) ? form["BuktiPenerimaan"] : "");
            var fileContent3 = Request.Files["BuktiPenerimaan"];
            //Insert SURATPERMOHONAN
            if (fileContent3 != null && fileContent3.ContentLength > 0)
            {
                string id3 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent3.InputStream;
                var FileSizeByte = fileContent3.ContentLength;
                var FileSize = FileSizeByte / 3000;
                var Extension = System.IO.Path.GetExtension(fileContent3.FileName);
                var fileName = "BuktiPenerimaan_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath3 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name3 = fileName;
                    string insert_lampiran3 = "UPDATE LAMPIRANKEMBALIAN SET NAMAFILE = '" + Filefilepath3 + "', STATUSLAMPIRAN = '1', TANGGAL = SYSDATE, EKSTENSI = '" + Extension + "' WHERE LAMPIRANKEMBALIANID = '" + fileid3 + "'";
                    db.Database.ExecuteSqlCommand(insert_lampiran3);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(fileid3))
                {
                    string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                    string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                              "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','BUKTI PENERIMAAN NEGARA')";
                    db.Database.ExecuteSqlCommand(insert_lampiran);
                }
            }

            var file_name4 = ((form.AllKeys.Contains("SuratPerintah")) ? form["SuratPerintah"] : "");
            var fileContent4 = Request.Files["SuratPerintah"];
            //Insert SURATPERMOHONAN
            if (fileContent4 != null && fileContent4.ContentLength > 0)
            {
                string id4 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent4.InputStream;
                var FileSizeByte = fileContent4.ContentLength;
                var FileSize = FileSizeByte / 3000;
                var Extension = System.IO.Path.GetExtension(fileContent4.FileName);
                var fileName = "SuratPerintah_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath4 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name4 = fileName;
                    string insert_lampiran4 = "UPDATE LAMPIRANKEMBALIAN SET NAMAFILE='" + Filefilepath4 + "', STATUSLAMPIRAN='1',TANGGAL=SYSDATE,EKSTENSI='" + Extension + "' WHERE LAMPIRANKEMBALIANID='" + fileid4 + "'";
                    db.Database.ExecuteSqlCommand(insert_lampiran4);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(fileid4))
                {
                    string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                    string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                              "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT PERINTAH SETOR')";
                    db.Database.ExecuteSqlCommand(insert_lampiran);
                }
            }

            var file_name5 = ((form.AllKeys.Contains("BuktiSetor")) ? form["BuktiSetor"] : "");
            var fileContent5 = Request.Files["BuktiSetor"];
            //Insert SURATPERMOHONAN
            if (fileContent5 != null && fileContent5.ContentLength > 0)
            {
                string id5 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent5.InputStream;
                var FileSizeByte = fileContent5.ContentLength;
                var FileSize = FileSizeByte / 3000;
                var Extension = System.IO.Path.GetExtension(fileContent5.FileName);
                var fileName = "BuktiSetor_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath5 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name5 = fileName;
                    string insert_lampiran5 = "UPDATE LAMPIRANKEMBALIAN SET NAMAFILE='" + Filefilepath5 + "', STATUSLAMPIRAN='1',TANGGAL=SYSDATE,EKSTENSI='" + Extension + "' WHERE LAMPIRANKEMBALIANID='" + fileid5 + "'";
                    db.Database.ExecuteSqlCommand(insert_lampiran5);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(fileid5))
                {
                    string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                    string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                              "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT BUKTI SETOR')";
                    db.Database.ExecuteSqlCommand(insert_lampiran);
                }
            }

            var file_name6 = ((form.AllKeys.Contains("BuktiRek")) ? form["BuktiRek"] : "");
            var fileContent6 = Request.Files["BuktiRek"];
            //Insert SURATPERMOHONAN
            if (fileContent6 != null && fileContent6.ContentLength > 0)
            {
                string id6 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent6.InputStream;
                var FileSizeByte = fileContent6.ContentLength;
                var FileSize = FileSizeByte / 3000;
                var Extension = System.IO.Path.GetExtension(fileContent6.FileName);
                var fileName = "BuktiRek_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath6 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name6 = fileName;
                    string insert_lampiran6 = "UPDATE LAMPIRANKEMBALIAN SET NAMAFILE='" + Filefilepath6 + "', STATUSLAMPIRAN='1',TANGGAL=SYSDATE,EKSTENSI='" + Extension + "' WHERE LAMPIRANKEMBALIANID='" + fileid6 + "'";
                    db.Database.ExecuteSqlCommand(insert_lampiran6);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(fileid6))
                {
                    string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                    string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                              "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','BUKTI KEPEMILIKAN REK TUJUAN')";
                    db.Database.ExecuteSqlCommand(insert_lampiran);
                }
            }

            var file_name7 = ((form.AllKeys.Contains("NpwpFile")) ? form["NpwpFile"] : "");
            var fileContent7 = Request.Files["NpwpFile"];
            //Insert SURATPERMOHONAN
            if (fileContent7 != null && fileContent7.ContentLength > 0)
            {
                string id7 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent7.InputStream;
                var FileSizeByte = fileContent7.ContentLength;
                var FileSize = FileSizeByte / 3000;
                var Extension = System.IO.Path.GetExtension(fileContent7.FileName);
                var fileName = "NpwpFile_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath7 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name7 = fileName;
                    string insert_lampiran7 = "UPDATE LAMPIRANKEMBALIAN SET NAMAFILE='" + Filefilepath7 + "', STATUSLAMPIRAN='1',TANGGAL=SYSDATE,EKSTENSI='" + Extension + "' WHERE LAMPIRANKEMBALIANID='" + fileid7 + "'";
                    db.Database.ExecuteSqlCommand(insert_lampiran7);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(fileid7))
                {
                    string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                    string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                              "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','NPWP')";
                    db.Database.ExecuteSqlCommand(insert_lampiran);
                }
            }

            var file_name8 = ((form.AllKeys.Contains("BuktiDomisili")) ? form["BuktiDomisili"] : "");
            var fileContent8 = Request.Files["BuktiDomisili"];
            //Insert SURATPERMOHONAN
            if (fileContent8 != null && fileContent8.ContentLength > 0)
            {
                string id8 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent8.InputStream;
                var FileSizeByte = fileContent8.ContentLength;
                var FileSize = FileSizeByte / 3000;
                var Extension = System.IO.Path.GetExtension(fileContent8.FileName);
                var fileName = "BuktiDomisili_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath8 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name8 = Filefilepath8;
                    string insert_lampiran8 = "UPDATE LAMPIRANKEMBALIAN SET NAMAFILE='" + Filefilepath8 + "', STATUSLAMPIRAN='1',TANGGAL=SYSDATE,EKSTENSI='" + Extension + "' WHERE LAMPIRANKEMBALIANID='" + fileid8 + "'";
                    db.Database.ExecuteSqlCommand(insert_lampiran8);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(fileid8))
                {
                    string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                    string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                              "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','BUKTI DOMISILI PEMOHON')";
                    db.Database.ExecuteSqlCommand(insert_lampiran);
                }
            }

            var file_name9 = ((form.AllKeys.Contains("SuratKuasa")) ? form["SuratKuasa"] : "");
            var fileContent9 = Request.Files["SuratKuasa"];
            //Insert SURATPERMOHONAN
            if (fileContent9 != null && fileContent9.ContentLength > 0)
            {
                string id9 = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                var tgl = DateTime.Now.ToString("yyMMddHHmmssff");
                var stream = fileContent9.InputStream;
                var FileSizeByte = fileContent9.ContentLength;
                var FileSize = FileSizeByte / 3000;
                var Extension = System.IO.Path.GetExtension(fileContent9.FileName);
                var fileName = "SuratKuasa_" + tgl + "" + Extension;
                string folderPath = Server.MapPath("~/Uploads/pengembalian/");
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(folderPath))
                {
                    //If Dir3ectory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(folderPath);
                }

                var path = Path.Combine(Server.MapPath("~/Uploads/pengembalian/"), fileName);
                var Filefilepath9 = "/Uploads/pengembalian/" + fileName;
                using (var fileStream = System.IO.File.Create(path))
                {
                    stream.CopyTo(fileStream);
                    file_name9 = Filefilepath9;

                    string insert_lampiran9 = "UPDATE LAMPIRANKEMBALIAN SET NAMAFILE='" + Filefilepath9 + "', STATUSLAMPIRAN='1',TANGGAL=SYSDATE,EKSTENSI='" + Extension + "' WHERE LAMPIRANKEMBALIANID='" + fileid9 + "'";
                    db.Database.ExecuteSqlCommand(insert_lampiran9);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(fileid9))
                {
                    string idd = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                    string insert_lampiran = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE) " +
                                              "VALUES ('" + idd + "','" + pengembalianid + "','1',SYSDATE,'Pengajuan','SURAT KUASA BERMATERAI')";
                    db.Database.ExecuteSqlCommand(insert_lampiran);
                }
            }
            //string insert_lampiran = "INSERT INTO PNBPTRAIN.LAMPIRANPENGEMBALIANPNBP (LAMPIRANID, LAMPIRANPENGEMBALIANID, SURATPERMOHONAN, SURATKETERANGAN, BUKTIPENERIMAAN, PERINTAHSETOR, BUKTISETOR,BUKTIREKENING,BUKTINPWP,BUKTIDOMISILI,SURATKUASA,STATUS) " +
            //                                "VALUES ((SELECT RAWTOHEX(SYS_GUID()) FROM DUAL)" + "','" + pengembalianid + "','" + file_name1 + "','" + file_name2 + "','" + file_name3 + "','" + file_name4 + "','" + file_name5 + "','"+file_name6+"','"+fileContent7+"','"+fileContent8+"','"+fileContent9+"',1)";
            //db.Database.ExecuteSqlCommand(insert_lampiran);

            if (ModelState.IsValid)
            {
                TempData["Upload"] = "Data Berhasil Disimpan";
                return RedirectToAction("PengajuanPengembalianIndex");
            }
            else
            {

            }

            return RedirectToAction("PengajuanPengembalianIndex");
        }

        [HttpPost]
        public ActionResult PengajuanPengembalianDetailPusat(FormCollection form, string[] qcpusat, string[] fileid)
        {
            //return Json(fileid[4], JsonRequestBehavior.AllowGet);
            var ctx = new PnbpContext();
            PnbpContext db = new PnbpContext();
            var pengembalianpnbpid = ((form.AllKeys.Contains("pengembalianpnbpid")) ? form["pengembalianpnbpid"] : "");

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            string namakantor = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaKantor;
            string pegawaiid = (HttpContext.User.Identity as Entities.InternalUserIdentity).PegawaiId;
            string namapegawai = (HttpContext.User.Identity as Entities.InternalUserIdentity).NamaPegawai;

            var Status = ((form.AllKeys.Contains("Status")) ? form["Status"] : "");
            var NomorSurat = ((form.AllKeys.Contains("NomorSurat")) ? form["NomorSurat"] : "");
            var pengembalianid = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
            var pengembalianidberkas = db.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
            var TanggalPengajuan = ConvertDateNow();

            string insert_target = "UPDATE PENGEMBALIANPNBP SET STATUSPENGEMBALIAN='" + Status + "' WHERE PENGEMBALIANPNBPID = '" + pengembalianpnbpid + "'";
            db.Database.ExecuteSqlCommand(insert_target);

            //log insert Audit Trail
            string log_id = NewGuID();
            if (Status == "2")
            {
                string insert_log_aktivitas = "INSERT INTO LOG_AKTIFITAS (LOG_ID, LOG_NAME, LOG_CREATE_BY, LOG_CREATE_DATE, LOG_URL, LOG_KANTORID, LOG_DATA_ID) VALUES ('" + log_id + "', 'Pengajuan Pengembalian PNBP Diproses', '" + pegawaiid + "', SYSDATE, '" + Url.Action("PengajuanPengembalianDetailPusat", "Pengembalian") + "', '" + kantoriduser + "', '" + pengembalianid + "')";
                db.Database.ExecuteSqlCommand(insert_log_aktivitas);
            }
            else if (Status == "3")
            {
                string insert_log_aktivitas = "INSERT INTO LOG_AKTIFITAS (LOG_ID, LOG_NAME, LOG_CREATE_BY, LOG_CREATE_DATE, LOG_URL, LOG_KANTORID, LOG_DATA_ID) VALUES ('" + log_id + "', 'Pengajuan Pengembalian PNBP Dikembalikan', '" + pegawaiid + "', SYSDATE, '" + Url.Action("PengajuanPengembalianDetailPusat", "Pengembalian") + "', '" + kantoriduser + "', '" + pengembalianid + "')";
                db.Database.ExecuteSqlCommand(insert_log_aktivitas);
            }
            else
            {
                string insert_log_aktivitas = "INSERT INTO LOG_AKTIFITAS (LOG_ID, LOG_NAME, LOG_CREATE_BY, LOG_CREATE_DATE, LOG_URL, LOG_KANTORID, LOG_DATA_ID) VALUES ('" + log_id + "', 'Pengajuan Pengembalian PNBP Selesai', '" + pegawaiid + "', SYSDATE, '" + Url.Action("PengajuanPengembalianDetailPusat", "Pengembalian") + "', '" + kantoriduser + "', '" + pengembalianid + "')";
                db.Database.ExecuteSqlCommand(insert_log_aktivitas);
            }
            //log insert Audit Trail


            //if (fileid.Length > 0)
            //{
            for (int i = 0; i < 9; i++)
            {
                //if (qcpusat[i]!="" || qcpusat[i] != " ")
                //{
                string update_lampiran = "UPDATE LAMPIRANKEMBALIAN SET STATUSLAMPIRAN='" + qcpusat[i] + "' WHERE LAMPIRANKEMBALIANID='" + fileid[i] + "'";
                db.Database.ExecuteSqlCommand(update_lampiran);
                //}

            }
            //}

            if (ModelState.IsValid)
            {
                TempData["Upload"] = "Data Berhasil Disimpan";
                return RedirectToAction("PengajuanPengembalianIndex");
            }
            else
            {

            }

            return RedirectToAction("PengajuanPengembalianIndex");
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



        // V2

        public ActionResult pusat()
        {
            Entities.FindPengembalianPnbp find = new Entities.FindPengembalianPnbp();
            List<Entities.GetSatkerList> result = pengembalianmodel.GetSatker();

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            int tipekantorid = pengembalianmodel.GetTipeKantor(kantoriduser);

            ViewData["tipekantorid"] = Convert.ToString(tipekantorid);
            ViewData["datasatker"] = result;
            ViewData["kantorid"] = kantoriduser;

            return View(find);
        }

        public ActionResult daerah()
        {
            Entities.FindPengembalianPnbp find = new Entities.FindPengembalianPnbp();
            List<Entities.GetSatkerList> result = pengembalianmodel.GetSatker();

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            int tipekantorid = pengembalianmodel.GetTipeKantor(kantoriduser);

            ViewData["tipekantorid"] = Convert.ToString(tipekantorid);
            ViewData["datasatker"] = result;
            ViewData["kantorid"] = kantoriduser;

            return View(find);
        }

        public ActionResult mon_pengembalian()
        {
            Entities.FindPengembalianPnbp find = new Entities.FindPengembalianPnbp();
            List<Entities.GetSatkerList> result = pengembalianmodel.GetSatker();

            string kantoriduser = (HttpContext.User.Identity as Entities.InternalUserIdentity).KantorId;
            int tipekantorid = pengembalianmodel.GetTipeKantor(kantoriduser);

            ViewData["tipekantorid"] = Convert.ToString(tipekantorid);
            ViewData["datasatker"] = result;
            ViewData["kantorid"] = kantoriduser;

            return View(find);
        }

        [HttpPost]
        public ActionResult mon_pengembalian_list(int? start, int? length, Entities.FindPengembalianPnbp f)
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
            string status = f.CariStatus;
            string namasatker = f.CariNamaSatker;
            string kodesatker = f.CariKodeSatker;
            int tipekantorid = pengembalianmodel.GetTipeKantor(kantoriduser);

            List<Entities.PengembalianPnbpTrain> result = pengembalianmodel.mon_pengembalian(tipekantorid, kantoriduser, judul, namakantor, nomorberkas, kodebilling, ntpn, namapemohon, nikpemohon, alamatpemohon, teleponpemohon, bankpersepsi, status, namasatker, kodesatker, from, to);

            if (result.Count > 0)
            {
                total = result[0].Total;
            }
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

    }
}