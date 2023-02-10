using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Pnbp.Entities;

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
                        if (query.Contains("WHERE"))
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

            string query = "SELECT DISTINCT row_number ( ) over ( ORDER BY r1.rankjan asc ) AS rnumber, r1.manfaatid, r1.kode, r1.tahun, r1.kodesatker, r1.namakantor," +
                            " r1.namaprogram, nvl( r1.nilaianggaran, 0 ) AS nilaianggaran, r1.anggjan, r1.rankjan, r1.alokjan, r1.anggfeb, r1.rankfeb, r1.alokfeb, r1.anggmar, " +
                            "r1.rankmar, r1.alokmar, r1.anggapr, r1.rankapr, r1.alokapr, r1.anggmei, r1.rankmei, r1.alokmei, r1.anggjun, r1.rankjun, r1.alokjun, r1.anggjul, r1.rankjul," +
                            "r1.alokjul, r1.anggagt, r1.rankagt, r1.alokagt, r1.anggsep, r1.ranksep, r1.aloksep, r1.anggokt, r1.rankokt, r1.alokokt, r1.anggnov, r1.ranknov, r1.aloknov, " +
                            "r1.anggdes, r1.rankdes, r1.alokdes  FROM manfaat r1 WHERE tahun = " + currentYear + " AND tipe = 'NONOPS' ORDER BY RANKJAN ASC ";
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






        // new Sangkuriang
        public bool IsTableTempAlokasiExist()
        {
            try
            {
                using (var ctx = new PnbpContext())
                {
                    var tempCount = ctx.Database.SqlQuery<int>("select count(*) from temp_alokasi").FirstOrDefault();
                    return tempCount > 0;
                }
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }
            return false;
        }

        public bool IsKodeSpanAndProgramValid()
        {
            PnbpContext db = new PnbpContext();
            bool result;
            try
            {
                var query = @"
                select count(*) from (
                    WITH sb AS (
	                    SELECT (KEGIATAN || '.' || OUTPUT) AS kodeoutput
                        FROM SPAN_BELANJA sb
                        WHERE sb.SUMBER_DANA = 'D' AND
                        TAHUN = EXTRACT(YEAR FROM sysdate)

                        GROUP BY KEGIATAN, OUTPUT
                    ),
                    k AS(
                        SELECT* FROM KODESPAN k
                    )
                    SELECT sb.kodeoutput, k.KODEOUTPUT, p.KODE
                    FROM sb
                    LEFT JOIN k ON sb.kodeoutput = k.KODEOUTPUT
                    LEFT JOIN program p ON sb.KODEOUTPUT = p.KODE
                    WHERE k.KODEOUTPUT IS NULL
                )
                ";

                var dataCount = db.Database.SqlQuery<int>(query).FirstOrDefault();
                result = dataCount == 0;
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
                result = false;
            }

            return result;
        }

        public bool ResetAlokasi()
        {
            var isError = false;
            try
            {
                PnbpContext db = new PnbpContext();
                var trx = db.Database.BeginTransaction();
                try
                {
                    var query = "DELETE FROM TEMP_ALOKASI";
                    db.Database.ExecuteSqlCommand(query);
                }
                catch (Exception e)
                {
                    isError = true;
                    _ = e.StackTrace;
                }
                finally
                {
                    if (!isError)
                    {
                        trx.Commit();
                    }
                    else
                    {
                        trx.Rollback();
                    }
                }
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
                isError = true;
            }

            return isError;
        }

        public bool ResetAlokasiRevisi()
        {
            var isError = false;
            try
            {
                PnbpContext db = new PnbpContext();
                var trx = db.Database.BeginTransaction();
                try
                {
                    var query = "DELETE FROM TEMP_ALOKASI_REVISI";
                    db.Database.ExecuteSqlCommand(query);
                }
                catch (Exception e)
                {
                    isError = true;
                    _ = e.StackTrace;
                }
                finally
                {
                    if (!isError)
                    {
                        trx.Commit();
                    }
                    else
                    {
                        trx.Rollback();
                    }
                }
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
                isError = true;
            }

            return isError;
        }

        public bool ProsesAlokasi()
        {
            var db = new PnbpContext();
            var trx = db.Database.BeginTransaction();
            List<Entities.DataProsesAlokasi> result = new List<Entities.DataProsesAlokasi>();
            var isProcessAlokasiSuccess = true;

            var idTrx = NewGuID();

            try
            {
                var queryCheckSummary = "select count(*) from alokasisatkersummary where tahun = extract(year from sysdate)";
                var countSummary = db.Database.SqlQuery<int>(queryCheckSummary).FirstOrDefault();

                bool successInsert = true;
                string qInsertOrUpdate = "";

                if (countSummary > 0) // update data ke manfaat
                {
                    qInsertOrUpdate = $@"MERGE INTO MANFAAT MT
                                        using (
                                            SELECT * FROM (
	                                            SELECT 
                                                    s.KANTORID as kantorid, 
	                                                k.TIPE AS tipe, 
	                                                p.PROGRAMID as programid, sb.kegiatan ||'.' ||sb.OUTPUT as kode, sum(sb.amount) as amount
                                                FROM span_belanja sb
                                                    LEFT JOIN satker s ON sb.KDSATKER = s.KODESATKER and s.statusaktif = 1 
                                                    LEFT JOIN KODESPAN k ON sb.KEGIATAN  = k.KODE AND sb.OUTPUT = k.KEGIATAN 
                                                    LEFT JOIN PROGRAM p ON p.KODE = k.KODEOUTPUT AND p.STATUSAKTIF = 1 AND p.TIPEOPS = k.TIPE
                                                WHERE
                                                    sb.KDSATKER = '430210' AND
                                                    sb.KDSATKER != '524465' AND sb.SUMBER_DANA = 'D' 
                                                    GROUP BY
	                                                    s.KANTORID, 
	                                                    k.tipe,
	                                                    s.NAMA_SATKER, 
	                                                    sb.kdsatker, 
	                                                    sb.kegiatan, 
	                                                    sb.OUTPUT, 
	                                                    p.PROGRAMID,
	                                                    p.NAMA
                                                ) WHERE programid IS NOT NULL
                                            ) t2
                                            ON (t2.programid = mt.programid
                                            AND t2.KANTORID =  mt.kantorid
                                            AND t2.tipe = mt.tipe
                                            AND t2.kode = mt.kode
                                            AND mt.tahun = extract(year from sysdate))
                                        WHEN MATCHED THEN UPDATE SET
                                            NILAIANGGARAN = t2.amount";
                        
                }
                else // insert data ke manfaat
                {
                    qInsertOrUpdate = $@"INSERT INTO MANFAAT
                                        (
                                            MANFAATID, 
                                            TAHUN, 
                                            KANTORID, 
                                            NAMAKANTOR, 
                                            PROGRAMID, 
                                            NAMAPROGRAM, 
                                            TIPE, 
                                            NILAIANGGARAN,
                                            KODE,
                                            KODESATKER,
                                            USERINSERT,
                                            INSERTDATE,
                                            statusaktif
                                        )
                                        SELECT * FROM (
                                            select 
    	                                        sys_guid() , 
                                                (extract(year from sysdate)), 
                                                s.KANTORID as kantorid, 
                                                s.NAMA_SATKER as namasatker, 
                                                p.PROGRAMID as programid, 
                                                p.nama AS programnama,
                                                k.TIPE AS tipe, 
                                                sum(sb.amount) as amount,
                                                sb.kegiatan || '.' || sb.OUTPUT, 
                                                sb.KDSATKER as kodesatker, 
                                                'SYSTEM_PNBP',
                                                sysdate,
                                                1
                                            from span_belanja sb
                                            left JOIN satker s ON sb.KDSATKER = s.KODESATKER and s.statusaktif = 1 
                                            LEFT JOIN KODESPAN k ON sb.KEGIATAN  = k.KODE AND sb.OUTPUT = k.KEGIATAN 
                                            LEFT JOIN PROGRAM p ON p.KODE = k.KODEOUTPUT AND p.STATUSAKTIF = 1 AND p.TIPEOPS = k.TIPE
                                            WHERE
                                                sb.KDSATKER = '430210' AND
                                                sb.KDSATKER != '524465' AND sb.SUMBER_DANA = 'D'
                                            GROUP BY
                                                s.KANTORID, 
                                                k.tipe,
                                                s.NAMA_SATKER, 
                                                sb.kdsatker, 
                                                sb.kegiatan, 
                                                sb.OUTPUT, 
                                                p.PROGRAMID,
                                                p.NAMA
                                            ORDER BY sb.kdsatker
                                        )
                                        WHERE programid IS NOT NULL";

                }

                var row = db.Database.ExecuteSqlCommand(qInsertOrUpdate);
                if (row <= 0)
                {
                    successInsert = false;
                }

                if (successInsert)
                {
                    var queryLastAlokasiSummary = @"SELECT 
                            ALOKASISATKERSUMMARYID, 
                            PAGU, 
                            ALOKASI, 
                            TO_CHAR(TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                            TO_CHAR(TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH,
                            MP 
                        FROM ALOKASISATKERSUMMARY a 
                        WHERE tahun = extract(year from sysdate) AND MP = (SELECT * FROM (
                            SELECT a2.mp FROM ALOKASISATKERSUMMARY a2 WHERE a2.tahun = extract(year from sysdate) ORDER BY a2.MP DESC
                        ) WHERE rownum = 1)";

                    AlokasiSatkerSummary getLastAlokasiSummary = db.Database.SqlQuery<Entities.AlokasiSatkerSummary>(queryLastAlokasiSummary).FirstOrDefault();
                    var mp = getLastAlokasiSummary == null ? 1 : (getLastAlokasiSummary.Mp + 1);

                    var isSuccessProccessMove = true;
                    var queryAlokasiSatkerSummary = $@"
                        INSERT INTO ALOKASISATKERSUMMARY(ALOKASISATKERSUMMARYID, PAGU, ALOKASI, MP)
                        SELECT '{idTrx}', sum(pagu), sum(alokasi), {mp} ALOKASI FROM TEMP_ALOKASI
                        ";
                    var rowSummary = db.Database.ExecuteSqlCommand(queryAlokasiSatkerSummary);
                    if (rowSummary <= 0)
                    {
                        isSuccessProccessMove = false;
                    }
                    else
                    {
                        if (getLastAlokasiSummary != null)
                        { 
                            #region update belanja MP sebelumnya
                            if (mp > 1)
                            {
                                decimal currentYearRealisasi = GetCurrentYearRealisasi();
                                string queryUpdateAlokasiSummary = $@"
                                    UPDATE ALOKASISATKERSUMMARY SET BELANJA = {currentYearRealisasi}
                                    WHERE ALOKASISATKERSUMMARYID = '{getLastAlokasiSummary.AlokasiSatkerSummaryId}'
                                ";
                                var rowUpdateAlokasiSummary = db.Database.ExecuteSqlCommand(queryUpdateAlokasiSummary);
                                if (rowUpdateAlokasiSummary <= 0)
                                {
                                    isSuccessProccessMove = false;
                                }
                            }
                            #endregion

                            #region simpan belanja per satker MP sebelumnya
                            if (isSuccessProccessMove)
                            {
                                string qSimpanBelanjaSatker = $@"
                                    INSERT INTO REALISASISATKERSUMMARY(ALOKASISATKERSUMMARYid,amount,kdsatker,tglakhir)
                                 select '{getLastAlokasiSummary.AlokasiSatkerSummaryId}',sum(amount), kdsatker, max(to_date(tanggal, 'DD/MON/RR','NLS_DATE_LANGUAGE=AMERICAN')) from span_realisasi
                                        where 
                                    SUMBERDANA  = 'D'
                                    AND tahun = extract(year from sysdate)
                                    group by kdsatker
                                ";

                                var rowSimpanBelanjaSatker = db.Database.ExecuteSqlCommand(qSimpanBelanjaSatker);
                                if (rowSimpanBelanjaSatker <= 0)
                                {
                                    isSuccessProccessMove = false;
                                }
                            }
                        }
                        #endregion

                        if (isSuccessProccessMove)
                        { 
                            var queryMoveAlokasi = $@"
                                INSERT INTO ALOKASISATKER(ALOKASISATKERID, KDSATKER, PAGU, ALOKASI, ALOKASISATKERSUMMARYID)
                                SELECT sys_guid(), KDSATKER, PAGU, ALOKASI, '{idTrx}' FROM TEMP_ALOKASI
                                ";
                            var rowMoveAlokasi = db.Database.ExecuteSqlCommand(queryMoveAlokasi);
                            if (rowMoveAlokasi <= 0)
                            {
                                isSuccessProccessMove = false;
                            }
                        }
                    }

                    if (isSuccessProccessMove)
                    {
                        // clear
                        var queryDelete = @"delete from TEMP_ALOKASI";
                        var rowDelete = db.Database.ExecuteSqlCommand(queryDelete);
                        if (rowDelete <= 0)
                        {
                            isProcessAlokasiSuccess = false;
                            trx.Rollback();
                        }
                        else
                        {
                            trx.Commit();
                        }
                    }
                    else
                    {
                        isProcessAlokasiSuccess = false;
                        trx.Rollback();
                    }
                }
                else
                {
                    isProcessAlokasiSuccess = false;
                    trx.Rollback();
                }
            }
            catch (Exception e)
            {
                new Codes.Functions.Logging().LogEvent(e.Message.ToString() + "\n" + e.StackTrace.ToString());
                isProcessAlokasiSuccess = false;
                trx.Rollback();
            }

            return isProcessAlokasiSuccess;
        }

        public bool ProsesAlokasiOld()
        {
            var db = new PnbpContext();
            var trx = db.Database.BeginTransaction();
            List<Entities.DataProsesAlokasi> result = new List<Entities.DataProsesAlokasi>();
            var isProcessAlokasiSuccess = true;

            var idTrx = NewGuID();

            try
            {
                string query = @"
                SELECT * FROM (
	                select 
	                    s.KANTORID as kantorid, 
	                    k.TIPE AS tipe, 
	                    p.PROGRAMID as programid, 
	                    p.nama AS programnama,
	                    s.NAMA_SATKER as namasatker, 
	                    sb.KDSATKER as kodesatker, sb.kegiatan as kegiatan, sb.OUTPUT as output, sum(sb.amount) as amount
	                from span_belanja sb
	                left JOIN satker s ON sb.KDSATKER = s.KODESATKER and s.statusaktif = 1 
	                LEFT JOIN KODESPAN k ON sb.KEGIATAN  = k.KODE AND sb.OUTPUT = k.KEGIATAN 
	                LEFT JOIN PROGRAM p ON p.KODE = k.KODEOUTPUT AND p.STATUSAKTIF = 1 AND p.TIPEOPS = k.TIPE
	                WHERE
                        sb.KDSATKER = '430210' AND
                        sb.KDSATKER != '524465'  AND sb.SUMBER_DANA = 'D'
	                GROUP BY
	                    s.KANTORID, 
	                    k.tipe,
	                    s.NAMA_SATKER, 
	                    sb.kdsatker, 
	                    sb.kegiatan, 
	                    sb.OUTPUT, 
	                    p.PROGRAMID,
	                    p.NAMA
	                ORDER BY sb.kdsatker
                )
                WHERE programid IS NOT NULL
                ";
                //sb.KDSATKER = '430210' AND


                result = db.Database.SqlQuery<Entities.DataProsesAlokasi>(query).ToList();

                var queryCheckSummary = "select count(*) from alokasisatkersummary where tahun = extract(year from sysdate)";
                var countSummary = db.Database.SqlQuery<int>(queryCheckSummary).FirstOrDefault();
                
                bool successInsert = true;
                foreach (var item in result)
                {
                    var qInsertOrUpdate = "";
                    if (countSummary > 0)
                    {
                        // update data ke manfaat
                        qInsertOrUpdate = $@"update manfaat set NILAIANGGARAN = {item.Amount} 
                            where tahun = extract(year from sysdate) and kantorid = '{item.KantorId}' 
                            and programid = '{item.ProgramId}' and tipe = '{item.Tipe}' and kode = '{item.Kegiatan + "." + item.Output}'";

                    }
                    else
                    {
                        // insert data ke manfaat
                        qInsertOrUpdate = BuildQuery(
                            item.KantorId,
                            item.NamaSatker,
                            item.ProgramId,
                            item.ProgramNama,
                            item.Tipe,
                            item.Amount,
                            $"{item.Kegiatan}.{item.Output}",
                            item.KodeSatker);

                    }
                    if (string.IsNullOrEmpty(qInsertOrUpdate))
                    {
                        successInsert = false;
                        break;
                    }

                    var row = db.Database.ExecuteSqlCommand(qInsertOrUpdate);
                    if (row <= 0)
                    {
                        successInsert = false;
                        break;
                    }
                }

                if (successInsert)
                {
                    var queryLastAlokasiSummary = @"SELECT 
                            ALOKASISATKERSUMMARYID, 
                            PAGU, 
                            ALOKASI, 
                            TO_CHAR(TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                            TO_CHAR(TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH,
                            MP 
                        FROM ALOKASISATKERSUMMARY a 
                        WHERE tahun = extract(year from sysdate) AND MP = (SELECT * FROM (
                            SELECT a2.mp FROM ALOKASISATKERSUMMARY a2 WHERE a2.tahun = extract(year from sysdate) ORDER BY a2.MP DESC
                        ) WHERE rownum = 1)";

                    var getLastAlokasiSummary = db.Database.SqlQuery<Entities.AlokasiSatkerSummary>(queryLastAlokasiSummary).FirstOrDefault();
                    var mp = getLastAlokasiSummary == null ? 1 : (getLastAlokasiSummary.Mp + 1);

                    var isSuccessProccessMove = true;
                    var queryAlokasiSatkerSummary = $@"
                        INSERT INTO ALOKASISATKERSUMMARY(ALOKASISATKERSUMMARYID, PAGU, ALOKASI, MP)
                        SELECT '{idTrx}', sum(pagu), sum(alokasi), {mp} ALOKASI FROM TEMP_ALOKASI
                        ";
                    var rowSummary = db.Database.ExecuteSqlCommand(queryAlokasiSatkerSummary);
                    if (rowSummary <= 0)
                    {
                        isSuccessProccessMove = false;
                    }
                    else
                    {
                        var queryMoveAlokasi = $@"
                            INSERT INTO ALOKASISATKER(ALOKASISATKERID, KDSATKER, PAGU, ALOKASI, ALOKASISATKERSUMMARYID)
                            SELECT sys_guid(), KDSATKER, PAGU, ALOKASI, '{idTrx}' FROM TEMP_ALOKASI
                            ";
                        var rowMoveAlokasi = db.Database.ExecuteSqlCommand(queryMoveAlokasi);
                        if (rowMoveAlokasi <= 0)
                        {
                            isSuccessProccessMove = false;
                        }
                    }

                    if (isSuccessProccessMove)
                    {
                        // clear
                        var queryDelete = @"delete from TEMP_ALOKASI";
                        var rowDelete = db.Database.ExecuteSqlCommand(queryDelete);
                        if (rowDelete <= 0)
                        {
                            isProcessAlokasiSuccess = false;
                            trx.Rollback();
                        }
                        else
                        {
                            trx.Commit();
                        }
                    }
                    else
                    {
                        isProcessAlokasiSuccess = false;
                        trx.Rollback();
                    }
                }
                else
                {
                    isProcessAlokasiSuccess = false;
                    trx.Rollback();
                }
            }
            catch (Exception e)
            {
                isProcessAlokasiSuccess = false;
                _ = e.StackTrace;
                trx.Rollback();
            }

            return isProcessAlokasiSuccess;
        }

        public bool ProcessRevisiAlokasi()
        {
            var db = new PnbpContext();
            var trx = db.Database.BeginTransaction();
            List<Entities.DataProsesAlokasi> result = new List<Entities.DataProsesAlokasi>();
            var isProcessAlokasiSuccess = true;

            var idTrx = NewGuID();

            try
            {
                var successInsert = true;
                if (successInsert)
                {
                    var queryLastAlokasiSummary = @"SELECT 
                            ALOKASISATKERSUMMARYID, 
                            PAGU, 
                            ALOKASI, 
                            TO_CHAR(TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                            TO_CHAR(TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH,
                            MP,
                            REVISI
                        FROM ALOKASISATKERSUMMARY a 
                        WHERE tahun = extract(year from sysdate) AND MP = (SELECT * FROM (
                            SELECT a2.mp FROM ALOKASISATKERSUMMARY a2 WHERE a2.tahun = extract(year from sysdate) ORDER BY a2.MP DESC
                        ) WHERE rownum = 1)
                        ORDER BY revisi DESC";

                    var getLastAlokasiSummary = db.Database.SqlQuery<Entities.AlokasiSatkerSummary>(queryLastAlokasiSummary).FirstOrDefault();
                    if (getLastAlokasiSummary != null)
                    {
                        var mp = getLastAlokasiSummary.Mp;
                        var revisi = getLastAlokasiSummary.Revisi + 1;

                        var isSuccessProccessMove = true;
                        var queryAlokasiSatkerSummary = $@"
                        INSERT INTO ALOKASISATKERSUMMARY(ALOKASISATKERSUMMARYID, PAGU, ALOKASI, MP, REVISI)
                        SELECT '{idTrx}', sum(pagu), sum(alokasi), {mp}, {revisi} ALOKASI FROM TEMP_ALOKASI_REVISI
                        ";
                        var rowSummary = db.Database.ExecuteSqlCommand(queryAlokasiSatkerSummary);
                        if (rowSummary <= 0)
                        {
                            isSuccessProccessMove = false;
                        }
                        else
                        {
                            var queryMoveAlokasi = $@"
                            INSERT INTO ALOKASISATKER(ALOKASISATKERID, KDSATKER, PAGU, ALOKASI, ALOKASISATKERSUMMARYID)
                            SELECT sys_guid(), KDSATKER, PAGU, ALOKASI, '{idTrx}' FROM TEMP_ALOKASI_REVISI
                            ";
                            var rowMoveAlokasi = db.Database.ExecuteSqlCommand(queryMoveAlokasi);
                            if (rowMoveAlokasi <= 0)
                            {
                                isSuccessProccessMove = false;
                            }
                        }

                        if (isSuccessProccessMove)
                        {
                            // clear
                            var queryDelete = @"delete from TEMP_ALOKASI_REVISI";
                            var rowDelete = db.Database.ExecuteSqlCommand(queryDelete);
                            if (rowDelete <= 0)
                            {
                                isProcessAlokasiSuccess = false;
                                trx.Rollback();
                            }
                            else
                            {
                                trx.Commit();
                            }
                        }
                        else
                        {
                            isProcessAlokasiSuccess = false;
                            trx.Rollback();
                        }
                    }
                    else
                    {
                        isProcessAlokasiSuccess = false;
                        trx.Rollback();
                    }
                }
                else
                {
                    isProcessAlokasiSuccess = false;
                    trx.Rollback();
                }
            }
            catch (Exception e)
            {
                isProcessAlokasiSuccess = false;
                _ = e.StackTrace;
                trx.Rollback();
            }

            return isProcessAlokasiSuccess;
        }

        private string BuildQuery(string kantorId, string namaKantor, string programId,
            string namaProgram, string tipe, double nilaiAnggaran, string kodeoutput, string kodesatker)
        {
            string query = "";
            try
            {
                query = $@"
                INSERT INTO PNBP.MANFAAT
                (
	                MANFAATID, 
	                TAHUN, 
	                KANTORID, 
	                NAMAKANTOR, 
	                PROGRAMID, 
	                NAMAPROGRAM, 
	                TIPE, 
	                NILAIANGGARAN,
                    KODE,
                    KODESATKER,
                    USERINSERT,
                    INSERTDATE,
                    statusaktif
                )
                VALUES(
                    sys_guid() , 
                    (extract(year from sysdate)), 
                    '{kantorId}', 
                    '{namaKantor}', 
                    '{programId}', 
                    '{namaProgram}', 
                    '{tipe}', 
                    {nilaiAnggaran},
                    '{kodeoutput}',
                    '{kodesatker}',
                    'SYSTEM_PNBP',
                    sysdate,
                    1
                )
            ";
            }
            catch (Exception e)
            {
                query = "";
                _ = e.StackTrace;
            }

            return query;
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

        public Entities.AlokasiSatkerDetail GetAlokasiBySummaryId(string id)
        {
            List<Entities.AlokasiSatkerV2> result = new List<Entities.AlokasiSatkerV2>();
            Entities.AlokasiSatkerDetail response = new Entities.AlokasiSatkerDetail();
            var db = new PnbpContext();
            try
            {
                string query = $@"SELECT 
                        a.ALOKASISATKERID,
                        a.KDSATKER as KodeSatker,
                        s.NAMA_SATKER as NamaSatker, 
                        a.PAGU,
                        a.ALOKASI,
                        a.TAHUN,
                        TO_CHAR(a.TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                        TO_CHAR(a.TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH 
                    FROM ALOKASISATKER a 
                    JOIN SATKER s ON a.KDSATKER = s.KODESATKER and s.statusaktif = 1 
                    WHERE ALOKASISATKERSUMMARYID = :id";
                List<object> lstparams = new List<object>();
                lstparams.Add(new OracleParameter("id", id));
                result = db.Database.SqlQuery<Entities.AlokasiSatkerV2>(query, lstparams.ToArray()).ToList();

                string queryGetRevisi = $@"
                    SELECT ass.REVISI, a.kdsatker as KodeSatker, a.pagu, a.alokasi
                    FROM alokasisatker a
                    JOIN ALOKASISATKERSUMMARY ass ON a.ALOKASISATKERSUMMARYID = ass.ALOKASISATKERSUMMARYID 
                    WHERE a.ALOKASISATKERSUMMARYID IN (
	                    SELECT ALOKASISATKERSUMMARYID  
	                    FROM alokasisatkersummary
	                    WHERE tahun = EXTRACT (YEAR FROM sysdate)
                        AND mp = (select mp from ALOKASISATKERSUMMARY where alokasisatkersummaryid = :id) 
	                    AND revisi > 0
                    )
                    ORDER BY kdsatker, revisi asc
                ";

                List<object> lstparamsRevisi = new List<object>();
                lstparamsRevisi.Add(new OracleParameter("id", id));
                var listRevisi = db.Database.SqlQuery<Entities.AlokasiSatkerV2>(queryGetRevisi, lstparamsRevisi.ToArray()).ToList();

                string queryTempRevisi = $@"
                    SELECT 
                        (row_number() OVER (ORDER BY s.KODESATKER)) no,
                        s.KODESATKER AS kodesatker, 
                        s.NAMA_SATKER AS NamaSatker, 
                        to_char(ta.PAGU) AS pagu, 
                        to_char(ta.alokasi) AS alokasi 
                    FROM TEMP_ALOKASI_REVISI ta
                    LEFT JOIN satker s  ON ta.KDSATKER = s.KODESATKER  and s.statusaktif = 1 
                    WHERE ta.KDSATKER != '524465'
                ";
                var listTempRevisi = db.Database.SqlQuery<Entities.TempAlokasi>(queryTempRevisi).ToList();

                if (listRevisi.Count != 0 || listTempRevisi.Count > 0)
                {
                    bool addRevisi = listRevisi.Count > 0;
                    bool addTemp = listTempRevisi.Count > 0;
                    int index = 0;

                    var totalRevisi = listRevisi
                        .GroupBy(y => y.Revisi)
                        .Select(cl => new Entities.AlokasiSatkerRevisi
                        {
                            Alokasi = cl.Sum(y => y.Alokasi),
                            Revisi = cl.Select(y => y.Revisi).FirstOrDefault()
                        }).ToList();

                    if (totalRevisi == null)
                    {
                        totalRevisi = new List<Entities.AlokasiSatkerRevisi>();
                    }

                    totalRevisi.Add(new Entities.AlokasiSatkerRevisi
                    {
                        Revisi = -1,
                        Alokasi = result.Sum(y => y.Pagu)
                    });

                    if (result != null && result.Count > 0)
                    {
                        var totalAlokasiSaatIni = result.Sum(y => y.Alokasi);
                        var alokasiSaatIni = new Entities.AlokasiSatkerRevisi
                        {
                            Revisi = 0,
                            Alokasi = totalAlokasiSaatIni
                        };
                        totalRevisi.Add(alokasiSaatIni);
                    }

                    if (listTempRevisi != null && listTempRevisi.Count > 0)
                    {
                        var ltr = listTempRevisi
                        .Sum(y => Decimal.Parse(y.Alokasi));

                        var tempRevisi = new Entities.AlokasiSatkerRevisi
                        {
                            Revisi = (totalRevisi.Count + 1),
                            Alokasi = ltr
                        };

                        totalRevisi.Add(tempRevisi);
                    }

                    response.Total = totalRevisi
                        .OrderBy(x => x.Revisi)
                        .ToList();

                    foreach (var item in result)
                    {
                        decimal beforeValue = item.Alokasi;
                        if (addRevisi)
                        {
                            var data = new List<Entities.AlokasiSatkerRevisi>();
                            var lRevisi = listRevisi.FindAll(x => x.KodeSatker == item.KodeSatker).ToList();

                            var lastRevisi = lRevisi.OrderByDescending(x => x.Revisi).FirstOrDefault();
                            if (lastRevisi != null)
                            {
                                beforeValue = lastRevisi.Alokasi;
                            }

                            foreach (var itemRevisi in lRevisi)
                            {
                                data.Add(new Entities.AlokasiSatkerRevisi()
                                {
                                    Revisi = itemRevisi.Revisi,
                                    Alokasi = itemRevisi.Alokasi
                                });
                            }
                            result[index].DaftarRevisi = data;
                        }

                        if (addTemp)
                        {
                            var tRevisi = listTempRevisi.FindAll(x => x.KodeSatker == item.KodeSatker).FirstOrDefault();
                            if (tRevisi != null)
                            {
                                result[index].TempAlokasi = tRevisi.Alokasi;
                            }

                            if (beforeValue.ToString() != tRevisi.Alokasi)
                            {
                                result[index].IsNilaiBaru = true;
                            }
                        }
                        index++;
                    }
                }
                else
                {
                    if (result != null && result.Count > 0)
                    {
                        var totalAlokasiSaatIni = result.Sum(y => y.Alokasi);
                        var alokasiSaatIni = new Entities.AlokasiSatkerRevisi
                        {
                            Revisi = 1,
                            Alokasi = totalAlokasiSaatIni
                        };
                        response.Total = new List<Entities.AlokasiSatkerRevisi>() {
                            new Entities.AlokasiSatkerRevisi
                            {
                                Revisi = 0,
                                Alokasi = result.Sum(y => y.Pagu)
                            },
                            alokasiSaatIni
                        };
                    }
                }
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            response.Data = result.OrderBy(x => x.KodeSatker).OrderByDescending(x => x.IsNilaiBaru).ToList();

            return response;
        }

        public List<Entities.AlokasiSatkerV2> GetAlokasiBySummaryId(Entities.FormAlokasiSummaryDetail search)
        {
            List<Entities.AlokasiSatkerV2> result = new List<Entities.AlokasiSatkerV2>();
            var db = new PnbpContext();
            string id = search.id;
            List<object> lstparams = new List<object>();

            try
            {
                string qBelanja = @"select sum(amount) belanjasatker, kdsatker from span_realisasi
	                    where 
		                    SUMBERDANA  = 'D'
		                    and tahun = extract(year from sysdate)
		                    group by kdsatker";


                var currentMPCheckQuery = @"select ALOKASISATKERSUMMARYID from ALOKASISATKERSUMMARY a 
                                            WHERE
	                                            mp = (SELECT MAX(mp) from ALOKASISATKERSUMMARY)
	                                            AND ALOKASISATKERSUMMARYID = :alokasiSatkerSummaryId";
                List<object> lstParamsCheck = new List<object>();
                lstParamsCheck.Add(new OracleParameter("alokasiSatkerSummaryId", search.id));
                var currentMPCheck = db.Database.SqlQuery<string>(currentMPCheckQuery, lstParamsCheck.ToArray()).FirstOrDefault();

                if (string.IsNullOrEmpty(currentMPCheck))
                {
                    qBelanja = @"select amount belanjasatker, KDSATKER from REALISASISATKERSUMMARY 
	                    where 
		                    ALOKASISATKERSUMMARYID = :alokasiSatkerSummaryId ";
                    lstparams.Add(new OracleParameter("alokasiSatkerSummaryId", search.id));
                }

                string query = $@"WITH belanja as ({qBelanja})
                    SELECT 
                        a.ALOKASISATKERID,
                        a.KDSATKER as KodeSatker,
                        s.NAMA_SATKER as NamaSatker, 
                        a.PAGU,
                        a.ALOKASI,
	                    NVL(b.belanjasatker, 0) belanja,
                        a.TAHUN,
                        TO_CHAR(a.TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                        TO_CHAR(a.TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH 
                    FROM ALOKASISATKER a 
                    JOIN SATKER s ON a.KDSATKER = s.KODESATKER
	                LEFT JOIN belanja b on b.kdsatker = a.kdsatker ";

                if (!String.IsNullOrEmpty(search.satker) && search.satker != "--Pilih Satker--")
                {
                    query += " WHERE s.kantorid = :kantorId ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kantorId", search.satker));
                }
                else
                {
                    query += " WHERE s.kantorid IS NOT NULL ";
                }

                query += $" AND ALOKASISATKERSUMMARYID = :id ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                result = db.Database.SqlQuery<Entities.AlokasiSatkerV2>(query, lstparams.ToArray()).ToList();

                string queryGetRevisi = $@"
                    SELECT ass.REVISI, a.kdsatker as KodeSatker, a.pagu, a.alokasi
                    FROM alokasisatker a
                    JOIN ALOKASISATKERSUMMARY ass ON a.ALOKASISATKERSUMMARYID = ass.ALOKASISATKERSUMMARYID 
                    WHERE a.ALOKASISATKERSUMMARYID IN (
	                    SELECT ALOKASISATKERSUMMARYID  
	                    FROM alokasisatkersummary
	                    WHERE tahun = EXTRACT (YEAR FROM sysdate)
                        AND mp = (select mp from ALOKASISATKERSUMMARY where alokasisatkersummaryid = '{id}') 
	                    AND revisi > 0
                    )
                    ORDER BY kdsatker, revisi asc
                ";
                var listRevisi = db.Database.SqlQuery<Entities.AlokasiSatkerV2>(queryGetRevisi).ToList();

                string queryTempRevisi = $@"
                    SELECT 
                        (row_number() OVER (ORDER BY s.KODESATKER)) no,
                        s.KODESATKER AS kodesatker, 
                        s.NAMA_SATKER AS NamaSatker, 
                        to_char(ta.PAGU) AS pagu, 
                        to_char(ta.alokasi) AS alokasi 
                    FROM TEMP_ALOKASI_REVISI ta
                    LEFT JOIN satker s  ON ta.KDSATKER = s.KODESATKER 
                    WHERE ta.KDSATKER != '524465'
                ";
                var listTempRevisi = db.Database.SqlQuery<Entities.TempAlokasi>(queryTempRevisi).ToList();

                if (listRevisi.Count != 0 || listTempRevisi.Count > 0)
                {
                    bool addRevisi = listRevisi.Count > 0;
                    bool addTemp = listTempRevisi.Count > 0;
                    int index = 0;
                    foreach (var item in result)
                    {
                        decimal beforeValue = item.Alokasi;
                        if (addRevisi)
                        {
                            var data = new List<Entities.AlokasiSatkerRevisi>();
                            var lRevisi = listRevisi.FindAll(x => x.KodeSatker == item.KodeSatker).ToList();

                            var lastRevisi = listRevisi.OrderByDescending(x => x.Revisi).FirstOrDefault();
                            if (lastRevisi != null)
                            {
                                beforeValue = lastRevisi.Alokasi;
                            }

                            foreach (var itemRevisi in lRevisi)
                            {
                                data.Add(new Entities.AlokasiSatkerRevisi()
                                {
                                    Revisi = itemRevisi.Revisi,
                                    Alokasi = itemRevisi.Alokasi
                                });
                            }
                            result[index].DaftarRevisi = data;
                        }

                        if (addTemp)
                        {
                            var tRevisi = listTempRevisi.FindAll(x => x.KodeSatker == item.KodeSatker).FirstOrDefault();
                            if (tRevisi != null)
                            {
                                result[index].TempAlokasi = tRevisi.Alokasi;
                            }

                            if (beforeValue.ToString() != tRevisi.Alokasi)
                            {
                                result[index].IsNilaiBaru = true;
                            }
                        }
                        index++;
                    }
                }
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return result;
        }

        public List<Entities.TemplateAlokasi> GetTemplateAlokasi()
        {
            var result = new List<Entities.TemplateAlokasi>();
            try
            {
                PnbpContext db = new PnbpContext();
                string query = @"
                    WITH sb as ( 
                        SELECT sb.kdsatker, sum(sb.amount)  AS amount
                        FROM SPAN_BELANJA sb 
                        WHERE sb.TAHUN = EXTRACT (YEAR FROM SYSDATE) and sb.SUMBER_DANA = 'D'
                        AND kdsatker != '524465' 
                        GROUP BY sb.kdsatker
                    )
                    SELECT 
	                    sb.KDSATKER  AS kodesatker, 
	                    TO_CHAR(sb.AMOUNT) AS amount
                    FROM sb
                    LEFT JOIN satker s ON sb.kdsatker = s.KODESATKER and s.statusaktif = 1 
                    WHERE 
	                    sb.KDSATKER != '524465' AND sb.kdsatker IS NOT NULL 
                    GROUP BY sb.KDSATKER, sb.AMOUNT 
                    ORDER BY sb.KDSATKER 
                ";
                result = db.Database.SqlQuery<Entities.TemplateAlokasi>(query).ToList();
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return result;

        }

        public List<Entities.AlokasiSatkerSummary> GetSummaryAlokasi(string tahun)
        {
            PnbpContext db = new PnbpContext();
            List<Entities.AlokasiSatkerSummary> result = new List<Entities.AlokasiSatkerSummary>();

            try
            {
                string query = @"
                    WITH grp AS (
	                    SELECT a.mp, max(a.revisi) revisi, a.tahun  
	                    FROM ALOKASISATKERSUMMARY a 
	                    GROUP BY a.mp, a.tahun
                    )
                    SELECT 
                        (row_number() OVER (ORDER BY ass.MP)) no,
                        ass.ALOKASISATKERSUMMARYID, 
                        ass.PAGU, 
                        ass.ALOKASI, 
                        NVL(ass.BELANJA, 0) BELANJA, 
                        TO_CHAR(ass.TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                        TO_CHAR(ass.TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH,
                        ass.MP 
                    FROM grp 
                    LEFT JOIN AlokasiSatkerSummary ass ON 
                    grp.mp = ass.MP AND 
                    grp.revisi = ass.revisi AND 
                    grp.tahun = ass.tahun 
                ";

                List<object> lstparams = new List<object>();

                if (!String.IsNullOrEmpty(tahun))
                {
                    query += " WHERE grp.tahun = :tahun ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tahun", tahun));
                }

                query += " ORDER BY ass.MP ";

                var parameters = lstparams.ToArray();
                result = db.Database.SqlQuery<Entities.AlokasiSatkerSummary>(query, parameters).ToList();
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return result;
        }

        public decimal GetCurrentYearRealisasi(string kodeSatker = null)
        {
            PnbpContext db = new PnbpContext();
            decimal result = 0;
            List<object> lstparams = new List<object>();

            try
            {
                string query = @"
                    select NVL(sum(amount),0) 
                    from span_realisasi
                    where SUMBERDANA  = 'D'
                    and KDSATKER != '524465'
                    and TAHUN = EXTRACT(YEAR FROM sysdate) ";

                if (!string.IsNullOrEmpty(kodeSatker))
                {
                    query += " AND KDSATKER = :kodeSatker ";
                    lstparams.Add(new OracleParameter("kodeSatker", kodeSatker));
                }

                result = db.Database.SqlQuery<decimal>(query, lstparams.ToArray()).FirstOrDefault();
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return result;
        }

        public List<Entities.AlokasiSatkerSummary> GetSummaryAlokasiDaerah(string tahun, string kodeSatker)
        {
            PnbpContext db = new PnbpContext();
            List<Entities.AlokasiSatkerSummary> result = new List<Entities.AlokasiSatkerSummary>();

            try
            {
                string query = @"
                    WITH grp AS (
                        SELECT a.mp, max(a.revisi) revisi, a.tahun 
                        FROM 
                            alokasisatker a1 
                            JOIN ALOKASISATKERSUMMARY a on a1.ALOKASISATKERSUMMARYID = a.ALOKASISATKERSUMMARYID 
                        GROUP BY 
                            a.mp, a.tahun
                    ), 
                    pagusatker AS (
                        select 
                        KDSATKER, 
                        SUM(amount) totalpagu 
                        from 
                        span_belanja 
                        where 
                        KDSATKER = :kodeSatker 
                        and SUMBER_DANA = 'D' 
                        and TAHUN = EXTRACT(YEAR FROM sysdate) 
                        GROUP BY kdsatker
                    ) 
                    SELECT 
                        (row_number() OVER (ORDER BY ass.MP)) no, 
                        ass.ALOKASISATKERSUMMARYID, 
                        --NVL(p.totalpagu, 0) PAGU, 
                        NVL(t.pagu, 0) PAGU, 
                        NVL(t.ALOKASI, 0) alokasi, 
                        NVL(ass.BELANJA, 0) BELANJA, 
                        TO_CHAR(ass.TANGGALBUAT, 'DD-MM-YYYY') as TANGGALBUAT, 
                        TO_CHAR(ass.TANGGALUBAH, 'DD-MM-YYYY') as TANGGALUBAH, 
                        ass.MP 
                    FROM 
                        grp 
                        LEFT JOIN AlokasiSatkerSummary ass ON grp.mp = ass.MP 
                        AND grp.revisi = ass.revisi 
                        AND grp.tahun = ass.tahun 
                        JOIN alokasisatker t ON t.ALOKASISATKERSUMMARYID = ass.ALOKASISATKERSUMMARYID 
                        JOIN pagusatker ps on ps.kdsatker = t.KDSATKER 
                        LEFT JOIN (
                        SELECT 
                            KDSATKER, 
                            SUM(amount) amount 
                        FROM 
                            SPAN_REALISASI 
                        WHERE 
                            SUMBERDANA = 'D' 
                            AND tahun = :tahun 
                        GROUP BY 
                            KDSATKER
                        ) sr ON sr.KDSATKER = t.KDSATKER 
                    WHERE 
                        t.KDSATKER = :kodeSatker ";

                List<object> lstparams = new List<object>();
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kodeSatker", kodeSatker));
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tahun", tahun));
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kodeSatker", kodeSatker));

                if (!String.IsNullOrEmpty(tahun))
                {
                    query += " AND grp.tahun = :tahun ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tahun", tahun));
                }

                query += " ORDER BY ass.MP ";

                var parameters = lstparams.ToArray();
                result = db.Database.SqlQuery<Entities.AlokasiSatkerSummary>(query, parameters).ToList();
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return result;
        }

        public List<Entities.AlokasiSatkerSummary> GetSummaryAlokasiRevisi()
        {
            PnbpContext db = new PnbpContext();
            List<Entities.AlokasiSatkerSummary> result = new List<Entities.AlokasiSatkerSummary>();

            try
            {
                string query = @"
                SELECT 
                    (row_number() OVER (ORDER BY a.MP)) no,
                    a.ALOKASISATKERSUMMARYID, 
                    a.PAGU, 
                    a.ALOKASI, 
                    TO_CHAR(a.TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                    TO_CHAR(a.TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH,
                    a.MP 
                FROM ALOKASISATKERSUMMARY a 
                WHERE a.tahun = extract(year from sysdate) 
                ORDER BY MP ASC
                ";

                result = db.Database.SqlQuery<Entities.AlokasiSatkerSummary>(query).ToList();
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return result;

        }

        public List<Entities.TempAlokasi> GetAlokasiSaatIni(bool isRevisi)
        {
            PnbpContext db = new PnbpContext();
            List<Entities.TempAlokasi> result = new List<Entities.TempAlokasi>();
            string tableName = isRevisi ? "temp_alokasi_revisi" : "TEMP_ALOKASI";
            var valid = false;

            try
            {
                string query = $@"
                SELECT * FROM
                (
                    SELECT 
                        (row_number() OVER (ORDER BY s.KODESATKER)) no,
                        s.KODESATKER AS kodesatker, 
                        s.NAMA_SATKER AS NamaSatker, 
                        to_char(ta.PAGU) AS pagu, 
                        to_char(ta.alokasi) AS alokasi, 
                        (CASE WHEN pagu >= alokasi THEN 1 ELSE 0 end) valid
                    FROM {tableName} ta
                    LEFT JOIN satker s  ON ta.KDSATKER = s.KODESATKER  and s.statusaktif = 1 
                    WHERE ta.KDSATKER != '524465'
                ) ORDER BY valid
                ";

                result = db.Database.SqlQuery<Entities.TempAlokasi>(query).ToList();
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return result;
        }

    }
}