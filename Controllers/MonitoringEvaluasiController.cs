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
    public class MonitoringEvaluasiController : Controller
    {
        Models.MonitoringEvaluasiModel monitoringevaluasimodel = new Models.MonitoringEvaluasiModel();
        private string _schemaKKP = OtorisasiUser.NamaSkemaKKP;
        // GET: MonitoringEvaluasi
        //public ActionResult Index()
        //{

        //    return View();
        //}

        struct CountResult
        {
            public int Count { get; set; }
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

        public ActionResult Penerimaan()
        {
            var userIdentity = new Pnbp.Codes.Functions().claimUser();
            Entities.CariPenerimaan find = new Entities.CariPenerimaan();
            //List<Entities.GetSatkerList> result = _manfaatanModel.GetSatker();
            string kantoriduser = userIdentity.KantorId;
            int tipekantorid = monitoringevaluasimodel.GetTipeKantor(kantoriduser);
            ViewData["tipekantorid"] = Convert.ToString(tipekantorid);
            //ViewData["datasatker"] = result;
            return View(find);
        }

        public ActionResult ListPenerimaan(int? start, int? length, Entities.CariPenerimaan f)
        {
            var userIdentity = new Pnbp.Codes.Functions().claimUser();

            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            decimal? total = 0;

            //string kantoriduser = userIdentity.KantorId;

            string kantorid = userIdentity.KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);
            //return Json(tipekantorid, JsonRequestBehavior.AllowGet);

            //string pgmaxpagu = f.PGMAXPAGU != null ? f.PGMAXPAGU : "99999999999999999999";
            //pgmaxpagu = pgmaxpagu.Replace(",", ".");

            //string pgminpagu = f.PGMINPAGU != null ? f.PGMINPAGU : "0";
            //pgminpagu = pgminpagu.Replace(",", ".");

            string pgmaxpagu = f.PGMAXPAGU;

            string pgminpagu = f.PGMINPAGU;


            //return Json(pgminpagu, JsonRequestBehavior.AllowGet);
            List<Entities.PenerimaanMonev> result = monitoringevaluasimodel.GetPenerimaan(tipekantorid, kantorid, pgmaxpagu, pgminpagu, from, to);

            if (result.Count > 0)
            {
                total = result[0].Total;
            }
            //return Json(from, JsonRequestBehavior.AllowGet);
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Belanja()
        {
            var userIdentity = new Pnbp.Codes.Functions().claimUser();
            Entities.CariBelanja find = new Entities.CariBelanja();
            //List<Entities.GetSatkerList> result = _manfaatanModel.GetSatker();
            string kantoriduser = userIdentity.KantorId;
            int tipekantorid = monitoringevaluasimodel.GetTipeKantor(kantoriduser);
            ViewData["tipekantorid"] = Convert.ToString(tipekantorid);
            //ViewData["datasatker"] = result;
            return View(find);
        }

        public ActionResult ListBelanja(int? start, int? length, Entities.CariBelanja f)
        {
            var userIdentity = new Pnbp.Codes.Functions().claimUser();

            int recNumber = start ?? 0;
            int RecordsPerPage = length ?? 10;
            int from = recNumber + 1;
            int to = from + RecordsPerPage - 1;

            decimal? total = 0;

            string kantorid = userIdentity.KantorId;
            string tipekantorid = Pnbp.Models.AdmModel.GetTipeKantorId(kantorid);

            string kantoriduser = userIdentity.KantorId;

            string pgmaxpagu = f.PGMAXPAGU != null ? f.PGMAXPAGU : "100";
            pgmaxpagu = pgmaxpagu.Replace(",", ".");

            string pgminpagu = f.PGMINPAGU != null ? f.PGMINPAGU : "0";
            pgminpagu = pgminpagu.Replace(",", ".");

            string pgmaxalok = f.PGMAXALOK != null ? f.PGMAXALOK : "100";
            pgmaxalok = pgmaxalok.Replace(",", ".");

            string pgminalok = f.PGMINALOK != null ? f.PGMINALOK : "0";
            pgminalok = pgminalok.Replace(",", ".");

            int opsi = f.OPSI;
            //return Json(pgminpagu, JsonRequestBehavior.AllowGet);
            List<Entities.BelanjaMonev> result = monitoringevaluasimodel.GetBelanja(tipekantorid, kantorid, pgmaxpagu, pgminpagu, pgmaxalok, pgminalok, opsi, from, to);

            if (result.Count > 0)
            {
                total = result[0].Total;
            }
            //return Json(from, JsonRequestBehavior.AllowGet);
            return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult CreateEvaluasiPenerimaan(FormCollection form)
        {
            PnbpContext db = new PnbpContext();

            var kantorid = ((form.AllKeys.Contains("kantorid")) ? form["kantorid"] : "0");
            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "0");
            var evaluasitext = ((form.AllKeys.Contains("evaluasitext")) ? form["evaluasitext"] : "0");

            string insert_target = "INSERT INTO MONEVPENERIMAAN (MONEVPENERIMAANID, KODESATKER, EVALUASI,CREATE_DATE) " +
                                            "VALUES ('" + NewGuID() + "','" + kodesatker + "','" + evaluasitext + "',SYSDATE)";
            db.Database.ExecuteSqlCommand(insert_target);
            return Json(new { result = insert_target }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateRenaksiPenerimaan(FormCollection form) {
        
            PnbpContext db = new PnbpContext();

            var kantorid = ((form.AllKeys.Contains("kantorid")) ? form["kantorid"] : "0");
            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "0");
            var renaksitext = ((form.AllKeys.Contains("renaksitext")) ? form["renaksitext"] : "0");

            //string insert_target = "INSERT INTO MONEVPENERIMAAN (MONEVPENERIMAANID, KODESATKER, EVALUASI) " +
            //                                "VALUES ('" + NewGuID() + "','" + kodesatker + "','" + evaluasitext + "')";
            //db.Database.ExecuteSqlCommand(insert_target);
            //return Json(new { result = kodesatker }, JsonRequestBehavior.AllowGet);
            //string update_renaksi = "UPDATE MONEVPENERIMAAN SET RENAKSI = '" + renaksitext + "' WHERE KODESATKER = '" + kodesatker + "'";
            //db.Database.ExecuteSqlCommand(update_renaksi);
            string add_renaksi = "INSERT INTO MONEVPENERIMAAN (MONEVPENERIMAANID, KODESATKER, RENAKSI,CREATE_DATE) " +
                                            "VALUES ('" + NewGuID() + "','" + kodesatker + "','" + renaksitext + "',SYSDATE)";
            db.Database.ExecuteSqlCommand(add_renaksi);

            return Json(new { result = add_renaksi }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateEvaluasiBelanja(FormCollection form)
        {
            PnbpContext db = new PnbpContext();

            var kantorid = ((form.AllKeys.Contains("kantorid")) ? form["kantorid"] : "0");
            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "0");
            var evaluasitext = ((form.AllKeys.Contains("evaluasitext")) ? form["evaluasitext"] : "0");

            string insert_target = "INSERT INTO MONEVBELANJA (MONEVBELANJAID, KODESATKER, EVALUASI,CREATE_DATE) " +
                                            "VALUES ('" + NewGuID() + "','" + kodesatker + "','" + evaluasitext + "',SYSDATE)";
            db.Database.ExecuteSqlCommand(insert_target);
            return Json(new { result = insert_target }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateRenaksiBelanja(FormCollection form)
        {
            PnbpContext db = new PnbpContext();

            var kantorid = ((form.AllKeys.Contains("kantorid")) ? form["kantorid"] : "0");
            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "0");
            var renaksitext = ((form.AllKeys.Contains("renaksitext")) ? form["renaksitext"] : "0");

            string insert_target = "INSERT INTO MONEVBELANJA (MONEVBELANJAID, KODESATKER, RENAKSI,CREATE_DATE) " +
                                            "VALUES ('" + NewGuID() + "','" + kodesatker + "','" + renaksitext + "',SYSDATE)";
            db.Database.ExecuteSqlCommand(insert_target);
            return Json(new { result = kodesatker }, JsonRequestBehavior.AllowGet);

            //string update_renaksi = "UPDATE MONEVBELANJA SET RENAKSI = '" + renaksitext + "' WHERE KODESATKER = '" + kodesatker + "'";
            //db.Database.ExecuteSqlCommand(update_renaksi);

            //return Json(new { result = update_renaksi }, JsonRequestBehavior.AllowGet);
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

        //public ActionResult ListPenerimaan(int? start, int? length, Entities.FindPengembalianPnbp f, int pgmin)
        //{
        //    //return Json(pgmin, JsonRequestBehavior.AllowGet);
        //    int recNumber = start ?? 0;
        //    int RecordsPerPage = length ?? 10;
        //    int from = recNumber + 1;
        //    int to = from + RecordsPerPage - 1;

        //    decimal? total = 0;

        //    string kantoriduser = userIdentity.KantorId;

        //    string namakantor = f.CariNamaKantor;
        //    decimal persen = pgmin;
        //    //return Json(persen, JsonRequestBehavior.AllowGet);

        //    string judul = f.CariJudul;
        //    string nomorberkas = f.CariNomorBerkas;
        //    string kodebilling = f.CariKodeBilling;
        //    string ntpn = f.CariNTPN;
        //    string namapemohon = f.CariNamaPemohon;
        //    string nikpemohon = f.CariNikPemohon;
        //    string alamatpemohon = f.CariAlamatPemohon;
        //    string teleponpemohon = f.CariTeleponPemohon;
        //    string bankpersepsi = f.CariBankPersepsi;
        //    string status = f.CariStatus;
        //    string namasatker = f.CariNamaSatker;

        //    List<Entities.PenerimaanMonev> result = monitoringevaluasimodel.GetPenerimaan(kantoriduser, judul, namakantor, nomorberkas, kodebilling, persen, ntpn, namapemohon, nikpemohon, alamatpemohon, teleponpemohon, bankpersepsi, status, namasatker, from, to);

        //    //return Json(new { data = result, recordsTotal = result.Count, recordsFiltered = total }, JsonRequestBehavior.AllowGet);
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}



        //public ActionResult ListBelanja(int? start, int? length, Entities.FindPengembalianPnbp f)
        //{

        //    int recNumber = start ?? 0;
        //    int RecordsPerPage = length ?? 10;
        //    int from = recNumber + 1;
        //    int to = from + RecordsPerPage - 1;

        //    decimal? total = 0;

        //    string kantoriduser = userIdentity.KantorId;

        //    string namakantor = f.CariNamaKantor;
        //    string judul = f.CariJudul;
        //    string nomorberkas = f.CariNomorBerkas;
        //    string kodebilling = f.CariKodeBilling;
        //    string ntpn = f.CariNTPN;
        //    string namapemohon = f.CariNamaPemohon;
        //    string nikpemohon = f.CariNikPemohon;
        //    string alamatpemohon = f.CariAlamatPemohon;
        //    string teleponpemohon = f.CariTeleponPemohon;
        //    string bankpersepsi = f.CariBankPersepsi;
        //    string status = f.CariStatus;
        //    string namasatker = f.CariNamaSatker;

        //    List<Entities.BelanjaMonev> result = monitoringevaluasimodel.GetBelanja(kantoriduser, judul, namakantor, nomorberkas, kodebilling, ntpn, namapemohon, nikpemohon, alamatpemohon, teleponpemohon, bankpersepsi, status, namasatker, from, to);

        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public ActionResult HistoryEvaluasiPenerimaan(FormCollection form)
        {
            //return Json(new { result = kodesatker }, JsonRequestBehavior.AllowGet);

            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "NULL");

            var ctx = new PnbpContext();
            string query =
                @"
                    SELECT MONEVPENERIMAANID,KODESATKER,EVALUASI,RENAKSI,READ,TO_CHAR(CREATE_DATE,'DD-MM-YYYY') as create_date FROM MONEVPENERIMAAN WHERE KODESATKER = '" + kodesatker + "' AND RENAKSI IS NULL AND to_char( CREATE_DATE, 'YYYY' )=to_char( SYSDATE, 'YYYY' ) ORDER BY CREATE_DATE DESC";
            var get_history_penerimaan = ctx.Database.SqlQuery<Entities.HistoryPenerimaan>(query).ToList();
            //ViewData["get_history_penerimaan"] = get_history_penerimaan;
            return Json(new { result = get_history_penerimaan }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HistoryEvaluasiBelanja(FormCollection form)
        {
            //return Json(new { result = kodesatker }, JsonRequestBehavior.AllowGet);

            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "NULL");

            var ctx = new PnbpContext();
            string query =
                @"
                    SELECT MONEVBELANJAID,KODESATKER,EVALUASI,RENAKSI,READ,TO_CHAR(CREATE_DATE,'DD-MM-YYYY') as create_date FROM MONEVBELANJA WHERE KODESATKER = '" + kodesatker + "' AND RENAKSI IS NULL AND to_char( CREATE_DATE, 'YYYY' )=to_char( SYSDATE, 'YYYY' ) ORDER BY CREATE_DATE DESC";
            var get_history_penerimaan = ctx.Database.SqlQuery<Entities.HistoryPenerimaan>(query).ToList();
            //ViewData["get_history_penerimaan"] = get_history_penerimaan;
            return Json(new { result = get_history_penerimaan }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HistoryRenaksiPenerimaan(FormCollection form)
        {
            //return Json(new { result = kodesatker }, JsonRequestBehavior.AllowGet);

            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "NULL");

            var ctx = new PnbpContext();
            string query =
                @"
                    SELECT MONEVPENERIMAANID,KODESATKER,EVALUASI,RENAKSI,READ,TO_CHAR(CREATE_DATE,'DD-MM-YYYY') as create_date FROM MONEVPENERIMAAN WHERE KODESATKER = '" + kodesatker + "' AND EVALUASI IS NULL AND to_char( CREATE_DATE, 'YYYY' )=to_char( SYSDATE, 'YYYY' ) ORDER BY CREATE_DATE DESC";
            var get_history_penerimaan = ctx.Database.SqlQuery<Entities.HistoryPenerimaan>(query).ToList();
            //ViewData["get_history_penerimaan"] = get_history_penerimaan;
            return Json(new { result = get_history_penerimaan }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult HistoryRenaksiBelanja(FormCollection form)
        {
            //return Json(new { result = kodesatker }, JsonRequestBehavior.AllowGet);

            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "NULL");

            var ctx = new PnbpContext();
            string query =
                @"
                    SELECT MONEVBELANJAID,KODESATKER,EVALUASI,RENAKSI,READ,TO_CHAR(CREATE_DATE,'DD-MM-YYYY') as create_date FROM MONEVBELANJA WHERE KODESATKER = '" + kodesatker + "' AND EVALUASI IS NULL AND to_char( CREATE_DATE, 'YYYY' )=to_char( SYSDATE, 'YYYY' ) ORDER BY CREATE_DATE DESC";
            var get_history_penerimaan = ctx.Database.SqlQuery<Entities.HistoryBelanja>(query).ToList();
            //ViewData["get_history_penerimaan"] = get_history_penerimaan;
            return Json(new { result = get_history_penerimaan }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DoReadMonevPenerimaan(FormCollection form)
        {

            PnbpContext db = new PnbpContext();

            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "NULL");

            var ctx = new PnbpContext();
            string do_read = @"UPDATE MONEVPENERIMAAN SET READ =1 WHERE KODESATKER = '" + kodesatker + "' AND RENAKSI IS NULL AND to_char( CREATE_DATE, 'YYYY' )=to_char( SYSDATE, 'YYYY' )";
            //var hasread = ctx.Database.SqlQuery<Entities.HistoryPenerimaan>(query).ToList();
            db.Database.ExecuteSqlCommand(do_read);
            return Json(new { result = do_read }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DoReadMonevPenerimaanRenaksi(FormCollection form)
        {

            PnbpContext db = new PnbpContext();

            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "NULL");

            var ctx = new PnbpContext();
            string do_read = @"UPDATE MONEVPENERIMAAN SET READ =1 WHERE KODESATKER = '" + kodesatker + "' AND EVALUASI IS NULL AND to_char( CREATE_DATE, 'YYYY' )=to_char( SYSDATE, 'YYYY' )";
            //var hasread = ctx.Database.SqlQuery<Entities.HistoryPenerimaan>(query).ToList();
            db.Database.ExecuteSqlCommand(do_read);
            return Json(new { result = do_read }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DoReadMonevBelanja(FormCollection form)
        {

            PnbpContext db = new PnbpContext();

            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "NULL");

            var ctx = new PnbpContext();
            string do_read = @"UPDATE MONEVBELANJA SET READ =1 WHERE KODESATKER = '" + kodesatker + "' AND RENAKSI IS NULL AND to_char( CREATE_DATE, 'YYYY' )=to_char( SYSDATE, 'YYYY' )";
            //var hasread = ctx.Database.SqlQuery<Entities.HistoryPenerimaan>(query).ToList();
            db.Database.ExecuteSqlCommand(do_read);
            return Json(new { result = do_read }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DoReadMonevBelanjaRenaksi(FormCollection form)
        {

            PnbpContext db = new PnbpContext();

            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "NULL AND to_char( CREATE_DATE, 'YYYY' )=to_char( SYSDATE, 'YYYY' )");

            var ctx = new PnbpContext();
            string do_read = @"UPDATE MONEVBELANJA SET READ =1 WHERE KODESATKER = '" + kodesatker + "' AND EVALUASI IS NULL";
            //var hasread = ctx.Database.SqlQuery<Entities.HistoryPenerimaan>(query).ToList();
            db.Database.ExecuteSqlCommand(do_read);
            return Json(new { result = do_read }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> AlokasiDT(DatatablesRequest req, bool statusAlokasi,string kodeSpopp, string kantorId, int tahun, int? bulan)
        {
            var ctx = new PnbpContext();

            var queryBase = @"
                SELECT 
                    {0}
                FROM PENERIMAAN p
                LEFT JOIN KODESPAN k 
                ON k.kode=SUBSTR(p.KODEPENERIMAAN, 0, 4) 
                    AND k.kegiatan=SUBSTR(p.KODEPENERIMAAN, -3) 
                WHERE p.TAHUN=" + tahun + " AND p.STATUSALOKASI=" + (statusAlokasi ? "1" : "0");

            if(!string.IsNullOrEmpty(kantorId))
            {
               queryBase += "AND p.KANTORID = '" + kantorId + "'";
            }
            if(!string.IsNullOrEmpty(kodeSpopp))
            {
                queryBase += "AND SUBSTR(p.KODESPOPP, 0, 12)= SUBSTR('"+kodeSpopp+"', 0, 12)";
            }
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

            var jsonResult = Json(new
            {
                draw = req.Draw,
                recordsFiltered,
                recordsTotal,
                data = list_penerimaan.Select(x => new
                {
                    jumlah = x.jumlah.ToString("N0", new System.Globalization.CultureInfo("id-ID")),
                    x.jenispenerimaan,
                    x.kodebilling,
                    x.kodesatker,
                    x.kodepenerimaan,
                    x.namaprogram,
                    x.namaprosedur,
                    x.tahunberkas,
                    x.statusalokasi,
                    x.nomorberkas,
                    nilaiakhir = x.nilaiakhir.ToString("N0", new System.Globalization.CultureInfo("id-ID")),
                    x.namakantor,
                    tanggal = x.tanggal.ToString("dd/mm/yyyy")
                })
            });

            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;
        }

        public struct DataPelayanan
        {
            public string KodeSpopp { get; set; }
            public string Nama { get; set; }
        }

        public ActionResult Alokasi()
        {
            //List<Entities.Wilayah> listPropinsi = new List<Entities.Wilayah>();
            //var ctx = new PnbpContext();

            //var sqlQuery = $"SELECT KODESPOPP, NAMA FROM {_schemaKKP}.PROSEDUR WHERE STATUSHAPUS <> 1";
            //var pelayanan = ctx.Database.SqlQuery<DataPelayanan>(sqlQuery).ToList();

            //var get_propinsi = ctx.Database.SqlQuery<Entities.propinsi>("SELECT kantorId, KODESATKER, NAMA_SATKER FROM satker WHERE tipekantorid in (1,2)").ToList();
            //ViewData["get_propinsi"] = get_propinsi;
            //ViewData["pelayanan"] = pelayanan;
            string jenisKantorUser = OtorisasiUser.GetJenisKantorUser();
            if (jenisKantorUser == "Pusat")
            {
                return View("AlokasiV2");
            }

            return View("AlokasiDaerah");
        }

        public ActionResult AlokasiSummaryDetail(string id)
        {
            List<Entities.Wilayah> listPropinsi = new List<Entities.Wilayah>();
            var ctx = new PnbpContext();

            var sqlQuery = $"SELECT KODESPOPP, NAMA FROM {_schemaKKP}.PROSEDUR WHERE STATUSHAPUS <> 1";
            var pelayanan = ctx.Database.SqlQuery<DataPelayanan>(sqlQuery).ToList();

            var get_propinsi = ctx.Database.SqlQuery<Entities.propinsi>("SELECT kantorId, KODESATKER, NAMA_SATKER FROM satker WHERE tipekantorid in (1,2)").ToList();
            ViewData["get_propinsi"] = get_propinsi;
            ViewData["pelayanan"] = pelayanan;

            return View();
        }


    }
}