using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Threading.Tasks;

namespace Pnbp.Models
{
    public class AlokasiModel
    {
        public int GetTahapAlokasi(string pTahun, string pTipe)
        {
            int result = 0;
            //using (var ctx = new PnbpContext())
            //{
            //    //string sql = "SELECT count(*)+1 FROM rekapalokasi WHERE tahun = :vTahun AND tipemanfaat = :vTipe";
            //    string sql = @"SELECT *
            //                    FROM
            //                      (SELECT prioritaskegiatan
            //                       FROM manfaat
            //                       WHERE
            //                         statusfullalokasi = 0
            //                         AND statusaktif = 1
            //                         AND tahun = :vTahun
            //                         and tipe = :vTipe
            //                       GROUP BY prioritaskegiatan
            //                       ORDER BY prioritaskegiatan)
            //                    WHERE
            //                      rownum = 1";
            //    Oracle.ManagedDataAccess.Client.OracleParameter vTahun = new Oracle.ManagedDataAccess.Client.OracleParameter("vTahun", pTahun);
            //    Oracle.ManagedDataAccess.Client.OracleParameter vTipe = new Oracle.ManagedDataAccess.Client.OracleParameter("vTipe", pTipe);
            //    object[] parameters = new object[2] { vTahun, vTipe };
            //    result = ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault();
            //}
            return result;
        }

        public decimal GetCalculateResult()
        {
            decimal result = 0;
            using (var ctx = new PnbpContext())
            {
                string sql =
                    @"select nvl(result,0) as result
                        from alokasijob a
                        where
                          not exists 
                            (select 1
                             from alokasijob b
                             where
                               a.tipe = b.tipe
                               and b.namaprosedur = 'Alokasi'
                               and b.validsampai > a.validsampai)
                          and a.tipe = 'OPS'
                          and a.namaprosedur = 'Calculate'";
                result = ctx.Database.SqlQuery<decimal>(sql).FirstOrDefault();
            }
            return result;
        }

        public List<Entities.TotalPemanfaatan> dtTotalPemanfaatan(string pKantorId, string pProgramId, string pTahun, string pTipe)
        {
            var _result = new List<Entities.TotalPemanfaatan>();
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                List<object> lstparams = new List<object>();

                string query =
                   @"SELECT 
                         NVL(SUM(MANFAAT.nilaianggaran),0) AS NILAIANGGARAN, 
                         NVL(SUM(MA.nilaialokasi),0) AS NILAIALOKASI 
                     FROM MANFAAT LEFT JOIN MANFAATALOKASI MA ON MANFAAT.MANFAATID = MA.MANFAATID AND MA.STATUSEDIT = 0 AND MA.STATUSAKTIF = 1
                     WHERE MANFAAT.kantorid IS NOT NULL AND MANFAAT.statusaktif = 1 ";

                if (!String.IsNullOrEmpty(pKantorId))
                {
                    query += " AND MANFAAT.kantorid = :KantorId ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", pKantorId));
                }
                if (!String.IsNullOrEmpty(pProgramId))
                {
                    query += " AND MANFAAT.programid = :ProgramId ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ProgramId", pProgramId));
                }
                if (!String.IsNullOrEmpty(pTipe))
                {
                    query += " AND MANFAAT.tipe = :Tipe ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tipe", pTipe));
                }
                if (!String.IsNullOrEmpty(pTahun))
                {
                    query += " AND MANFAAT.tahun = :Tahun ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", Convert.ToInt32(pTahun)));
                }
                else
                {
                    query += " AND MANFAAT.tahun = ({0}) ";
                    query = String.Format(query, ConfigurationManager.AppSettings["TahunAnggaran"].ToString());
                }

                query = sWhitespace.Replace(query, " ");
                var parameters = lstparams.ToArray();
                _result = ctx.Database.SqlQuery<Entities.TotalPemanfaatan>(query, parameters).ToList();

            }
            return _result;
        }

        public List<Entities.AlokasiJob> dt_RunningJob(string pProcedureName, string pTipe)
        {
            var _result = new List<Entities.AlokasiJob>();
            using (var ctx = new PnbpContext())
            {
                try
                {
                    Regex sWhitespace = new Regex(@"\s+");
                    List<object> lstparams = new List<object>();

                    string query =
                       @"SELECT * FROM (SELECT namaprosedur, TO_CHAR(validsejak,'dd/mm/yyyy hh24:mi') mulai,
                    TO_CHAR(validsampai,'dd/mm/yyyy hh24:mi') selesai, status, result, inputalokasi, ROW_NUMBER() over (order by validsejak desc, validsampai desc) RNUMBER FROM ALOKASIJOB ";

                    if (!String.IsNullOrEmpty(pProcedureName))
                    {
                        query += " WHERE NAMAPROSEDUR = :procedurename ";
                        lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("procedurename", pProcedureName));
                    }

                    if (!String.IsNullOrEmpty(pTipe))
                    {
                        if(query.Contains("WHERE"))
                        {
                            query += " AND TIPE = :tipe ";
                        }
                        else
                        {
                            query += " WHERE TIPE = :tipe ";
                        }                        
                        lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tipe", pTipe));
                    }

                    query += ")";

                    query = sWhitespace.Replace(query, " ");
                    var parameters = lstparams.ToArray();
                    _result = ctx.Database.SqlQuery<Entities.AlokasiJob>(query, parameters).ToList();
                }
                catch (Exception ex)
                {
                    string Pesan = ex.Message.ToString();
                }
            }
            return _result;
        }

        public List<Entities.ManfaatPrioritas> dt_nonopsbyprioritas()
        {
            var _result = new List<Entities.ManfaatPrioritas>();
            using (var ctx = new PnbpContext())
            {
                try
                {
                    Regex sWhitespace = new Regex(@"\s+");
                    List<object> lstparams = new List<object>();

                    string query =
                      @"select
                          'Non Operasional' as tipe, r1.prioritaskegiatan,
                          nvl(sum(r1.nilaianggaran),0) as nilaianggaran,
                          nvl(sum(r2.nilaialokasi),0) as nilaialokasi,
                          ROW_NUMBER() over (order by r1.prioritaskegiatan asc) RNUMBER
                        from manfaat r1
                          left join (select manfaatid, sum(nilaialokasi) as nilaialokasi from manfaatalokasi where statusedit = 0 and statusaktif = 1 group by manfaatid) r2 on
                            r1.manfaatid = r2.manfaatid
                        where
                          r1.tahun = ({0})
                          and r1.tipe = 'NONOPS'
                          and r1.statusaktif = 1
                        group by r1.prioritaskegiatan ";

                    query = sWhitespace.Replace(String.Format(query, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                    var parameters = lstparams.ToArray();
                    _result = ctx.Database.SqlQuery<Entities.ManfaatPrioritas>(query, parameters).ToList();
                }
                catch (Exception ex)
                {
                    string Pesan = ex.Message.ToString();
                }
            }
            return _result;
        }

        public decimal JumlahSudahAlokasi(string pTahun, string pTipe)
        {
            decimal result = 0;
            using (var ctx = new PnbpContext())
            {
                string sql = @"SELECT NVL(SUM(manfaatalokasi.nilaialokasi),0) AS JUMLAHALOKASI FROM manfaat,manfaatalokasi
                               WHERE manfaatalokasi.manfaatid = manfaat.manfaatid AND manfaat.tipe = :vTipe AND manfaat.tahun = :vTahun AND manfaatalokasi.statusedit = 0 AND manfaat.statusaktif = 1";
                Oracle.ManagedDataAccess.Client.OracleParameter vTipe = new Oracle.ManagedDataAccess.Client.OracleParameter("vTipe", pTipe);
                Oracle.ManagedDataAccess.Client.OracleParameter vTahun = new Oracle.ManagedDataAccess.Client.OracleParameter("vTahun", pTahun);
                object[] parameters = new object[2] { vTipe, vTahun };
                result = ctx.Database.SqlQuery<decimal>(sql, parameters).FirstOrDefault();
            }

            return result;
        }

        public decimal GetResultJobs(string pNamaProsedur, string pTipe)
        {
            decimal result = 0;
            using (var ctx = new PnbpContext())
            {
                string sql = @"select result from alokasijob where namaprosedur = :vNamaProsedur and tipe = :vTipe and validsampai is not null";
                Oracle.ManagedDataAccess.Client.OracleParameter vNamaProsedur = new Oracle.ManagedDataAccess.Client.OracleParameter("vNamaProsedur", pNamaProsedur);
                Oracle.ManagedDataAccess.Client.OracleParameter vTipe = new Oracle.ManagedDataAccess.Client.OracleParameter("vTipe", pTipe);                
                object[] parameters = new object[2] { vNamaProsedur, vTipe };
                result = ctx.Database.SqlQuery<decimal>(sql, parameters).FirstOrDefault();
            }
            return result;
        }

        public bool IsJobRunning(string pNamaProsedur, string pTipe)
        {
            bool result = false;
            using (var ctx = new PnbpContext())
            {
                string sql =
                    " select count(*) jml from alokasijob where validsampai is null and namaprosedur = :vNamaProsedur and tipe = :vTipe ";
                Oracle.ManagedDataAccess.Client.OracleParameter vNamaProsedur = new Oracle.ManagedDataAccess.Client.OracleParameter("vNamaProsedur", pNamaProsedur);
                Oracle.ManagedDataAccess.Client.OracleParameter vTipe = new Oracle.ManagedDataAccess.Client.OracleParameter("vTipe", pTipe);
                object[] parameters = new object[2] { vNamaProsedur, vTipe };
                var objResult = ctx.Database.SqlQuery<decimal>(sql, parameters).FirstOrDefault();
                result = (Convert.ToInt16(objResult) > 0) ? true : false;
            }
            return result;
        }

        public List<Entities.AlokasiRows> DaftarAlokasiOPS(string pTipe)
        {
            string currentYear = DateTime.Now.Year.ToString();
            var list = new List<Entities.AlokasiRows>();
            Regex sWhitespace = new Regex(@"\s+");
            List<object> lstparams = new List<object>();

            string query =
              @"SELECT row_number() over( order by r1.prioritaskegiatan, r1.namakantor, r1.namaprogram, r1.nilaianggaran, r2.nilaialokasi ) as rnumber,
                  r1.kode, r1.tahun, r1.kodesatker, r1.namakantor, r1.namaprogram, case when r1.prioritaskegiatan is null then 'Operasional' else to_char(r1.prioritaskegiatan) end prioritaskegiatan,
                  nvl(r1.nilaianggaran,0) as nilaianggaran, nvl(r2.sudahalokasi,0) as sudahalokasi, nvl(r2.nilaialokasi,0) as nilaialokasi,
                  r1.anggjan, r1.rankjan, r1.alokjan, r1.anggfeb, r1.rankfeb, r1.alokfeb, r1.anggmar, r1.rankmar, r1.alokmar,
                  r1.anggapr, r1.rankapr, r1.alokapr, r1.anggmei, r1.rankmei, r1.alokmei, r1.anggjun, r1.rankjun, r1.alokjun,
                  r1.anggjul, r1.rankjul, r1.alokjul, r1.anggagt, r1.rankagt, r1.alokagt, r1.anggsep, r1.ranksep, r1.aloksep,
                  r1.anggokt, r1.rankokt, r1.alokokt, r1.anggnov, r1.ranknov, r1.aloknov, r1.anggdes, r1.rankdes, r1.alokdes
                FROM MANFAAT R1
                LEFT JOIN (SELECT MANFAATID, SUM(CASE WHEN STATUSEDIT = 0 THEN NILAIALOKASI ELSE NULL END) AS SUDAHALOKASI,SUM(CASE WHEN STATUSEDIT = 1 THEN NILAIALOKASI ELSE NULL END) AS NILAIALOKASI FROM MANFAATALOKASI WHERE STATUSAKTIF = 1 AND EXISTS (SELECT 1 FROM MANFAATALOKASI MA WHERE MA.MANFAATID = MANFAATALOKASI.MANFAATID AND MA.STATUSEDIT = 1) GROUP BY MANFAATID) r2 on
                    r1.manfaatid = r2.manfaatid
                where
                  r1.tahun = 2021
                  and r1.tipe ='OPS' ";

            //string query = "SELECT DISTINCT row_number ( ) over ( ORDER BY r1.rankjan asc ) AS rnumber, r1.manfaatid, r1.kode, r1.tahun, r1.kodesatker, r1.namakantor," +
            //                " r1.namaprogram, nvl( r1.nilaianggaran, 0 ) AS nilaianggaran, r1.anggjan, r1.rankjan, r1.alokjan, r1.anggfeb, r1.rankfeb, r1.alokfeb, r1.anggmar, " +
            //                "r1.rankmar, r1.alokmar, r1.anggapr, r1.rankapr, r1.alokapr, r1.anggmei, r1.rankmei, r1.alokmei, r1.anggjun, r1.rankjun, r1.alokjun, r1.anggjul, r1.rankjul," +
            //                "r1.alokjul, r1.anggagt, r1.rankagt, r1.alokagt, r1.anggsep, r1.ranksep, r1.aloksep, r1.anggokt, r1.rankokt, r1.alokokt, r1.anggnov, r1.ranknov, r1.aloknov, " +
            //                "r1.anggdes, r1.rankdes, r1.alokdes  FROM manfaat r1 WHERE tahun = " + currentYear + " AND tipe = 'OPS' ORDER BY RANKJAN ASC ";
            //query = sWhitespace.Replace(String.Format(query, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");

            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tipe", pTipe));
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tahun", currentYear));
            var parameters = lstparams.ToArray();

            using (var ctx = new PnbpContext())
            {
                list = ctx.Database.SqlQuery<Entities.AlokasiRows>(query, parameters).ToList();
            }

            return list;
        }

        public List<Entities.AlokasiRows> DaftarAlokasi(string pTipe)
        {
            string currentYear = DateTime.Now.Year.ToString();
            var list = new List<Entities.AlokasiRows>();
            Regex sWhitespace = new Regex(@"\s+");
            List<object> lstparams = new List<object>();

            //string query =
            //  @"SELECT row_number() over( order by r1.prioritaskegiatan, r1.namakantor, r1.namaprogram, r1.nilaianggaran, r2.nilaialokasi ) as rnumber,
            //      r1.kode, r1.tahun, r1.kodesatker, r1.namakantor, r1.namaprogram, case when r1.prioritaskegiatan is null then 'Operasional' else to_char(r1.prioritaskegiatan) end prioritaskegiatan,
            //      nvl(r1.nilaianggaran,0) as nilaianggaran, nvl(r2.sudahalokasi,0) as sudahalokasi, nvl(r2.nilaialokasi,0) as nilaialokasi,
            //      r1.anggjan, r1.rankjan, r1.alokjan, r1.anggfeb, r1.rankfeb, r1.alokfeb, r1.anggmar, r1.rankmar, r1.alokmar,
            //      r1.anggapr, r1.rankapr, r1.alokapr, r1.anggmei, r1.rankmei, r1.alokmei, r1.anggjun, r1.rankjun, r1.alokjun,
            //      r1.anggjul, r1.rankjul, r1.alokjul, r1.anggagt, r1.rankagt, r1.alokagt, r1.anggsep, r1.ranksep, r1.aloksep,
            //      r1.anggokt, r1.rankokt, r1.alokokt, r1.anggnov, r1.ranknov, r1.aloknov, r1.anggdes, r1.rankdes, r1.alokdes
            //    FROM MANFAAT R1
            //    LEFT JOIN (SELECT MANFAATID, SUM(CASE WHEN STATUSEDIT = 0 THEN NILAIALOKASI ELSE NULL END) AS SUDAHALOKASI,SUM(CASE WHEN STATUSEDIT = 1 THEN NILAIALOKASI ELSE NULL END) AS NILAIALOKASI FROM MANFAATALOKASI WHERE STATUSAKTIF = 1 AND EXISTS (SELECT 1 FROM MANFAATALOKASI MA WHERE MA.MANFAATID = MANFAATALOKASI.MANFAATID AND MA.STATUSEDIT = 1) GROUP BY MANFAATID) r2 on
            //        r1.manfaatid = r2.manfaatid
            //    where
            //      r1.tahun = 2021
            //      and r1.tipe = :tipe ";

            string query =  "SELECT DISTINCT row_number ( ) over ( ORDER BY r1.rankjan asc ) AS rnumber, r1.manfaatid, r1.kode, r1.tahun, r1.kodesatker, r1.namakantor," +
                            " r1.namaprogram, nvl( r1.nilaianggaran, 0 ) AS nilaianggaran, r1.anggjan, r1.rankjan, r1.alokjan, r1.anggfeb, r1.rankfeb, r1.alokfeb, r1.anggmar, " +
                            "r1.rankmar, r1.alokmar, r1.anggapr, r1.rankapr, r1.alokapr, r1.anggmei, r1.rankmei, r1.alokmei, r1.anggjun, r1.rankjun, r1.alokjun, r1.anggjul, r1.rankjul," +
                            "r1.alokjul, r1.anggagt, r1.rankagt, r1.alokagt, r1.anggsep, r1.ranksep, r1.aloksep, r1.anggokt, r1.rankokt, r1.alokokt, r1.anggnov, r1.ranknov, r1.aloknov, " +
                            "r1.anggdes, r1.rankdes, r1.alokdes  FROM manfaat r1 WHERE tahun = " + currentYear +" AND tipe = 'NONOPS' ORDER BY RANKJAN ASC ";
            query = sWhitespace.Replace(String.Format(query, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");

            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tipe", pTipe));
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tahun", currentYear));
            var parameters = lstparams.ToArray();

            using (var ctx = new PnbpContext())
            {
                list = ctx.Database.SqlQuery<Entities.AlokasiRows>(query, parameters).ToList();
            }

            return list;
        }

        public Entities.TransactionResult RunCalculate(string pUserId, int pTahun, string pTanggal, string pTipe)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = "";
                        List<object> lstparams = new List<object>();
                        if (pTipe == "OPS")
                        {
                            sql = "call CalculateOps(:userid,:tahun,:tanggal)";
                            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("userid", pUserId));
                            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tahun", pTahun));
                            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tanggal", pTanggal));
                        }
                        else if (pTipe == "NONOPS")
                        {
                            sql = "call NonOpsCalculate(:userid,:tahun)";
                            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("userid", pUserId));
                            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tahun", pTahun));
                        }

                        var parameters = lstparams.ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tr.Status = true;
                        tr.Pesan = "OK";

                        tc.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }
        public Entities.TransactionResult RunReset(string pUserId, string pTahun, string pTipe)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = "call ResetAlokasi(:userid,:tahun,:tipe)";
                        Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("userid", pUserId);
                        Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("tahun", pTahun);
                        Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("tipe", pTipe);

                        object[] parameters = new object[3] { p1, p2, p3 };

                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tr.Status = true;
                        tr.Pesan = "OK";

                        tc.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }
        public Entities.TransactionResult RunAllocation(string pUserId, int pTahun, string pTanggal, string pTipe)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = "";
                        if (pTipe == "OPS")
                        {
                            sql = "call AlokasiOps(:userid,:tahun,:tanggal)";
                        }
                        else if (pTipe == "NONOPS")
                        {
                            //sql = "call AlokasiNonOps(:userid,:tahun,:alokasi)";
                            sql = "call AlokasiNonOpsNew(:userid,:tahun,:alokasi)";
                        }
                        Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("userid", pUserId);
                        Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("tahun", pTahun);
                        Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter();
                        if (pTipe == "OPS")
                        {
                            p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("tanggal", pTanggal);
                        }
                        else
                        {
                            p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("alokasi", Convert.ToDecimal(pTanggal));
                        }

                        object[] parameters = new object[3] { p1, p2, p3 };

                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tr.Status = true;
                        tr.Pesan = "OK";

                        tc.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }
        public Entities.TransactionResult RunSaveAllocation(string pUserId, string pTahun, string pTipeAnggaran)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = "";
                        if (pTipeAnggaran == "OPS")
                        {
                            sql = "call SimpanAlokasi(:userid,:tahun,:tipeanggaran)";

                            Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("userid", pUserId);
                            Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("tahun", pTahun);
                            Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("tipeanggaran", pTipeAnggaran);
                            //Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("termin", pTermin);
                            //Oracle.ManagedDataAccess.Client.OracleParameter p5 = new Oracle.ManagedDataAccess.Client.OracleParameter("dapatalokasi", pDapatAlokasi);
                            //Oracle.ManagedDataAccess.Client.OracleParameter p6 = new Oracle.ManagedDataAccess.Client.OracleParameter("inputalokasi", pInputAlokasi);
                            //Oracle.ManagedDataAccess.Client.OracleParameter p7 = new Oracle.ManagedDataAccess.Client.OracleParameter("teralokasi", pTerAlokasi);
                            //Oracle.ManagedDataAccess.Client.OracleParameter p8 = new Oracle.ManagedDataAccess.Client.OracleParameter("tglpenerimaanterakhir", pTanggalPenerimaanTerakhir);
                            //Oracle.ManagedDataAccess.Client.OracleParameter p9 = new Oracle.ManagedDataAccess.Client.OracleParameter("penerimaan", pPenerimaan);

                            object[] parameters = new object[3] { p1, p2, p3 };
                            //object[] parameters = new object[9] { p1, p2, p3, p4, p5, p6, p7, p8, p9 };

                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else if (pTipeAnggaran == "NONOPS")
                        {
                            sql = "call SimpanAlokasi(:userid,:tahun,:tipeanggaran)";

                            Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("userid", pUserId);
                            Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("tahun", pTahun);
                            Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("tipeanggaran", pTipeAnggaran);

                            object[] parameters = new object[3] { p1, p2, p3 };

                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        

                        tr.Status = true;
                        tr.Pesan = "OK";

                        tc.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

        #region RekapAlokasi
        public static List<Pnbp.Entities.Tahun> lsTahunRekapAlokasi()
        {
            List<Pnbp.Entities.Tahun> result = new List<Pnbp.Entities.Tahun>();
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");

                string query =
                   @" select distinct to_char(tahun) as value, to_char(tahun) as tahun from rekapalokasi order by tahun desc";
                result = ctx.Database.SqlQuery<Pnbp.Entities.Tahun>(query).ToList();

            }
            return result;
        }

        public List<Entities.RekapAlokasiRows> GetRekapAlokasi(string pTahun)
        {
            List<Entities.RekapAlokasiRows> _list = new List<Entities.RekapAlokasiRows>();
            List<object> lstparams = new List<object>();

            string query =
                 @" select rekapalokasiid, to_char(tahun) as tahun, 
                    case bulan
                        when 1 then 'Januari'
                        when 2 then 'Februari'
                        when 3 then 'Maret'
                        when 4 then 'April'
                        when 5 then 'Mei'
                        when 6 then 'Juni'
                        when 7 then 'Juli'
                        when 8 then 'Agustus'
                        when 9 then 'September'
                        when 10 then 'Oktober'
                        when 11 then 'November'
                        when 12 then 'Desember'
                    end
                    as bulan, to_char(tanggalalokasi,'dd/mm/yyyy') as tglalokasi, tipemanfaat, teralokasi as alokasi, statusalokasi,
                    row_number() over (order by r1.bulan) as urutan
                    from rekapalokasi r1
                    where r1.tahun = :param1 ";
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pTahun));

            using (var ctx = new PnbpContext())
            {
                var parameters = lstparams.ToArray();
                _list = ctx.Database.SqlQuery<Entities.RekapAlokasiRows>(query, parameters).ToList<Entities.RekapAlokasiRows>();
            }

            return _list;
        }

        public List<Entities.DetailRekapAlokasiRows> GetRekapAlokasiDetail(string pRekapId)
        {
            List<Entities.DetailRekapAlokasiRows> _list = new List<Entities.DetailRekapAlokasiRows>();
            List<object> lstparams = new List<object>();

            string query =
                 @" select r1.manfaatid, r1.namakantor, r1.namaprogram, r1.nilaianggaran, r2.nilaialokasi,
                    row_number() over (order by r1.prioritaskegiatan, r1.nilaianggaran desc) as urutan
                    from manfaat r1 join manfaatalokasi r2 on r1.manfaatid = r2.manfaatid
                    where r2.rekapalokasiid = :param1 ";
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pRekapId));

            using (var ctx = new PnbpContext())
            {
                var parameters = lstparams.ToArray();
                _list = ctx.Database.SqlQuery<Entities.DetailRekapAlokasiRows>(query, parameters).ToList<Entities.DetailRekapAlokasiRows>();
            }

            return _list;
        }
        #endregion

        public Decimal GetAlokasiSatker(string kodesatker, int tahun)
        {
            Decimal value = 0;
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");

                string query =
                   $@" select alokasi From alokasisatker where kdsatker = '{kodesatker}' and tahun = {tahun}";
                value = ctx.Database.SqlQuery<decimal>(query).FirstOrDefault();

            }
            return value;
        }

        public Decimal GetAlokasiSatkerTerAlokasi(string kodesatker, int tahun)
        {
            Decimal value = 0;
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");

                string query =
                   $@"  SELECT sum(NILAIALOKASI) 
                        FROM manfaat 
                        WHERE KODESATKER = '{kodesatker}' and tahun = {tahun} 
                        GROUP BY kodesatker";
                value = ctx.Database.SqlQuery<decimal>(query).FirstOrDefault();

            }
            return value;
        }

    }
}