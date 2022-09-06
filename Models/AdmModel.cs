using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Text.RegularExpressions;
using System.Collections;
using System.Configuration;
using System.Web.Mvc;

namespace Pnbp.Models
{
    public class AdmModel
    {
        private string _schemaKKP = OtorisasiUser.NamaSkemaKKP;
        Regex sWhitespace = new Regex(@"\s+");

        public static List<Entities.Wilayah> getPropinsi()
        {
            List<Entities.Wilayah> listPropinsi = new List<Entities.Wilayah>();

            using (var ctx = new PnbpContext())
            {
                string sql = "select kode, replace(nama,'Kantor Wilayah ', '') as nama from kantor where tipekantorid=2 ";
                listPropinsi = ctx.DbWilayah.SqlQuery(sql).ToList<Entities.Wilayah>();
            }


            return listPropinsi;
        }
        public static List<Entities.Wilayah> getKantor(string kode)
        {
            List<Entities.Wilayah> listWilayah = new List<Entities.Wilayah>();
            string sql = "";
            if (String.IsNullOrEmpty(kode))
            {
                sql = "select kantorid id, kode, replace(nama,'Kantor Wilayah ', '') as nama from kantor where tipekantorid=2 ";
            }
            else
            {
                sql = "select kantorid id, kode, replace(nama,'Kantor Pertanahan ', '') as nama from kantor where tipekantorid in(3,4) AND substr(kode,1,2)='" + kode + "' ";
            }
            using (var ctx = new PnbpContext())
            {
                listWilayah = ctx.DbWilayah.SqlQuery(sql).ToList<Entities.Wilayah>();
            }

            return listWilayah;
        }

        public List<Entities.TargetPnbpHistory> GetTargetPnbp(int from, int to)
        {
            List<Entities.TargetPnbpHistory> records = new List<Entities.TargetPnbpHistory>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"
                SELECT * FROM(
                    SELECT
	                    row_number () over ( ORDER BY FILE_NAME_TARGET, FILE_PATH_TARGET, FILE_CREATE_DATE ) AS RNumber,
	                    FILE_NAME_TARGET,
	                    FILE_PATH_TARGET,
	                    FILE_CREATE_DATE 
                    FROM
	                    V_MANAJEMENDATA)
                WHERE RNumber BETWEEN :startCnt AND :limitCnt";

            query = sWhitespace.Replace(query, " ");
            //query += " ORDER BY FILE_CREATE_DATE DESC";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.TargetPnbpHistory>(query, parameters).ToList();
            }
            return records;
        }

        public static string GetTipeKantorId(string pKantorId)
        {
            string tipekantorid = "";

            using (var ctx = new PnbpContext())
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p0 = new Oracle.ManagedDataAccess.Client.OracleParameter("param0", pKantorId);
                object[] parameters = new object[1] { p0 };
                string sql = "SELECT TO_CHAR(TIPEKANTORID) AS TIPEKANTORID FROM KANTOR WHERE KANTORID = :param0 ";
                DbRawSqlQuery<string> tes = ctx.Database.SqlQuery<string>(sql, parameters);

                tipekantorid = (tes.Count() == 0) ? "" : ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
            }

            return tipekantorid;
        }

        public static string GetKanwilIdByKantahId(string kantorid)
        {
            string tipekantorid = "";

            using (var ctx = new PnbpContext())
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p0 = new Oracle.ManagedDataAccess.Client.OracleParameter("param0", kantorid);
                object[] parameters = new object[1] { p0 };
                string sql = "SELECT TO_CHAR(INDUK) AS INDUK FROM KANTOR WHERE KANTORID = :param0 ";
                DbRawSqlQuery<string> tes = ctx.Database.SqlQuery<string>(sql, parameters);

                tipekantorid = (tes.Count() == 0) ? "" : ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
            }

            return tipekantorid;
        }

        public static bool GetStatusKantorbyManfaat(string pManfaatId)
        {
            bool result = false;
            string getstatus = "";

            using (var ctx = new PnbpContext())
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p0 = new Oracle.ManagedDataAccess.Client.OracleParameter("param0", pManfaatId);
                object[] parameters = new object[1] { p0 };
                string sql = "SELECT TO_CHAR(R1.STATUSAKTIF) AS STATUSAKTIF FROM SATKER R1 WHERE EXISTS(SELECT 1 FROM MANFAAT R2 WHERE R2.KANTORID = R1.KANTORID AND R2.MANFAATID = :param0)";
                DbRawSqlQuery<string> tes = ctx.Database.SqlQuery<string>(sql, parameters);

                getstatus = (tes.Count() == 0) ? "" : ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();

                result = (string.IsNullOrEmpty(getstatus) || getstatus == "0")? false : true;
            }

            return result;
        }

        public static List<Entities.Wilayah> getKantorKanwil(bool iskanwil, string kantorid)
        {
            List<Entities.Wilayah> listWilayah = new List<Entities.Wilayah>();
            string sql = "";
            if(iskanwil)
            {
                sql = "select kantorid id, kode, replace(nama,'Kantor Pertanahan ', '') as nama from kantor where tipekantorid in(3,4) AND induk='" + kantorid + "' ";
            }
            else
            {
                sql = "select kantorid id, kode, replace(nama,'Kantor Wilayah ', '') as nama from kantor where tipekantorid=2 and exists(select 1 from kantor kab where kab.induk = kantor.kantorid and kab.kantorid = '" + kantorid + "') ";
            }
            using (var ctx = new PnbpContext())
            {
                listWilayah = ctx.DbWilayah.SqlQuery(sql).ToList<Entities.Wilayah>();
            }

            return listWilayah;
        }

        public static List<Entities.Wilayah> getKantorList(string kantorid)
        {
            List<Entities.Wilayah> listWilayah = new List<Entities.Wilayah>();
            string sql = "select kantorid id, kode, replace(nama,'Kantor Pertanahan ', '') as nama from kantor where kantorid='" + kantorid + "' ";
            using (var ctx = new PnbpContext())
            {
                listWilayah = ctx.DbWilayah.SqlQuery(sql).ToList<Entities.Wilayah>();
            }

            return listWilayah;
        }

        public List<Entities.SatkerAlokasi> GetSatkerAlokasi()
        {
            List<Entities.SatkerAlokasi> satkerAlokasi = new List<Entities.SatkerAlokasi>();
            using (var ctx = new PnbpContext())
            {
                string sql = @"select
                      row_number() over (order by r1.kode) as rnumber,
                      r1.kantorid, r1.kode as kodekantor, r1.kodesatker, r1.nama_satker as namasatker,
                      nvl(sum(r2.nilaianggaran),0) as nilaianggaran, nvl(sum(r3.nilaialokasi),0) as nilaialokasi, case when r1.statusaktif = 1 then 'Aktif' else 'Tidak Aktif' end as statusaktif
                    from
                      satker r1
                      join manfaat r2 on
                        r1.kantorid = r2.kantorid
                      left join (select manfaatid, sum(nilaialokasi) as nilaialokasi from manfaatalokasi where statusedit = 0 and statusaktif = 1 group by manfaatid) r3 on
                        r2.manfaatid = r3.manfaatid
                    where
                      r2.tahun = ({0})
                    group by
                      r1.kode, r1.kantorid, r1.kodesatker, r1.nama_satker, r1.statusaktif";

                sql = sWhitespace.Replace(String.Format(sql, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                satkerAlokasi = ctx.Database.SqlQuery<Entities.SatkerAlokasi>(sql).ToList<Entities.SatkerAlokasi>();
            }

            return satkerAlokasi;
        }

        public Entities.SatkerAlokasi GetStatusSatker(string pKantorId)
        {
            Entities.SatkerAlokasi satkerAlokasi = new Entities.SatkerAlokasi();
            using (var ctx = new PnbpContext())
            {
                List<object> lstparams = new List<object>();

                string sql = @"select
                      row_number() over (order by r1.kode) as rnumber,
                      r1.kantorid, r1.kode as kodekantor, r1.kodesatker, r1.nama_satker as namasatker,
                      sum(r2.nilaianggaran) as nilaianggaran, sum(r3.nilaialokasi) as nilaialokasi, case when r1.statusaktif = 1 then 'Aktif' else 'Tidak Aktif' end as statusaktif
                    from
                      satker r1
                      join manfaat r2 on
                        r1.kantorid = r2.kantorid
                      left join (select manfaatid, sum(nilaialokasi) as nilaialokasi from manfaatalokasi where statusedit = 0 and statusaktif = 1 group by manfaatid) r3 on
                        r2.manfaatid = r3.manfaatid
                    where
                        r2.tahun = ({0})
                        and r1.kantorid = :kantorid
                    group by
                      r1.kode, r1.kantorid, r1.kodesatker, r1.nama_satker, r1.statusaktif";

                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kantorid", pKantorId));
                sql = sWhitespace.Replace(String.Format(sql, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                var parameters = lstparams.ToArray();

                satkerAlokasi = ctx.Database.SqlQuery<Entities.SatkerAlokasi>(sql, parameters).FirstOrDefault();
            }

            return satkerAlokasi;
        }

        public Entities.TransactionResult UpdateSatker(string id, string status)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };
            
            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql1 = "";
                        
                        ArrayList arrayListParameters = new ArrayList();
                        ArrayList arrayListParameters1 = new ArrayList();
                        string currentstatus = "";

                        string sqlstring =
                            @"select case when statusaktif = 1 then 'Aktif' else 'Tidak Aktif' end as statusaktif from satker where kantorid = '" + id + "'";
                            currentstatus = ctx.Database.SqlQuery<string>(sqlstring).FirstOrDefault();

                        if (status != currentstatus)
                        {
                            string sql = @"update satker set statusaktif = :status where kantorid = :id and tahun = ({0})";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("status", (status == "Aktif") ? 1 : 0));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                            object[] parameters = arrayListParameters.OfType<object>().ToArray();
                            sql = sWhitespace.Replace(String.Format(sql, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        
                            if (status == "Aktif")
                            {
                                sql1 = @"update manfaat r1 set r1.statusaktif = 1 where kantorid = :id
                                          and r1.tahun = ({0}) and r1.statusaktif = 0 and r1.tipe = 'NONOPS'";
                                arrayListParameters1.Clear();
                                arrayListParameters1.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                                object[] parameters1 = arrayListParameters1.OfType<object>().ToArray();
                                sql1 = sWhitespace.Replace(String.Format(sql1, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                                ctx.Database.ExecuteSqlCommand(sql1, parameters1);
                            }
                            else
                            {
                                sql1 = @"update (select * from
                                        manfaat r1
                                    where
                                        r1.tahun = ({0})
                                        and r1.tipe = 'NONOPS'
                                        and not exists
                                        (select 1 from
                                            (select
                                                manfaatid, nvl(sum(nilaialokasi),0) as alokasi
                                            from
                                                manfaatalokasi
                                            where
        	                                    statusedit = 0
                                            group by
                                                manfaatid) r2
                                            where
                                            r1.manfaatid = r2.manfaatid
                                            and (r1.nilaianggaran = r2.alokasi or (r1.nilaianggaran > r2.alokasi and r2.alokasi > 0) ))
                                            and r1.kantorid = :id) k1 set k1.statusaktif = 0";
                                arrayListParameters1.Clear();
                                arrayListParameters1.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                                object[] parameters1 = arrayListParameters1.OfType<object>().ToArray();
                                sql1 = sWhitespace.Replace(String.Format(sql1, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                                ctx.Database.ExecuteSqlCommand(sql1, parameters1);
                            } 

                            tc.Commit();
                            tr.Status = true;
                            tr.Pesan = "Data berhasil diubah";
                        }
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

        public List<Entities.PrioritasAlokasi> GetPrioritasAlokasi()
        {
            List<Entities.PrioritasAlokasi> prioritasAlokasi = new List<Entities.PrioritasAlokasi>();
            using (var ctx = new PnbpContext())
            {
                string sql = @"select to_char(r1.prioritaskegiatan) as prioritaskegiatan, nvl(sum(r1.nilaianggaran),0) as nilaianggaran, nvl(sum(r2.sudahalokasi),0) as teralokasi, nvl(sum(r2.nilaialokasi),0) as alokasi
                                from manfaat r1
	                                LEFT JOIN (SELECT MANFAATID, SUM(CASE WHEN STATUSEDIT = 0 THEN NILAIALOKASI ELSE NULL END) AS SUDAHALOKASI,SUM(CASE WHEN STATUSEDIT = 1 THEN NILAIALOKASI ELSE NULL END) AS NILAIALOKASI FROM MANFAATALOKASI WHERE STATUSAKTIF = 1 GROUP BY MANFAATID) r2 on
                                    r1.manfaatid = r2.manfaatid
                                where
                                r1.tahun = ({0})
                                and r1.tipe = 'NONOPS'
                                and r1.statusaktif = 1
                                group by r1.prioritaskegiatan
                                order by r1.prioritaskegiatan";

                sql = sWhitespace.Replace(String.Format(sql, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                prioritasAlokasi = ctx.Database.SqlQuery<Entities.PrioritasAlokasi>(sql).ToList<Entities.PrioritasAlokasi>();
            }

            return prioritasAlokasi;
        }

        public List<Entities.PrioritasAlokasi> GetPrioritasManfaat(string prioritas)
        {
            List<Entities.PrioritasAlokasi> prioritasManfaat = new List<Entities.PrioritasAlokasi>();
            using (var ctx = new PnbpContext())
            {
                List<object> lstparams = new List<object>();
                string sql = @"select r1.manfaatid, to_char(r1.prioritaskegiatan) as prioritaskegiatan, r1.kodesatker, r1.namakantor as namasatker, r1.namaprogram, r1.nilaianggaran as nilaianggaran, nvl(r2.sudahalokasi,0) as teralokasi, nvl(r2.nilaialokasi,0) as alokasi,
                               case when r1.statusaktif = 1 then 'Aktif' else 'Tidak Aktif' end as statusaktif
                                from manfaat r1
	                                LEFT JOIN (SELECT MANFAATID, SUM(CASE WHEN STATUSEDIT = 0 THEN NILAIALOKASI ELSE NULL END) AS SUDAHALOKASI,SUM(CASE WHEN STATUSEDIT = 1 THEN NILAIALOKASI ELSE NULL END) AS NILAIALOKASI FROM MANFAATALOKASI WHERE STATUSAKTIF = 1 GROUP BY MANFAATID) r2 on
                                    r1.manfaatid = r2.manfaatid
                                where
                                    r1.tahun = ({0})
                                    and r1.tipe = 'NONOPS'
                                    and r1.prioritaskegiatan = :prioritas
                                order by r1.nilaianggaran desc";

                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("prioritas", prioritas));
                sql = sWhitespace.Replace(String.Format(sql, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                var parameters = lstparams.ToArray();

                prioritasManfaat = ctx.Database.SqlQuery<Entities.PrioritasAlokasi>(sql, parameters).ToList<Entities.PrioritasAlokasi>();
            }

            return prioritasManfaat;
        }

        public Entities.PrioritasAlokasi GetPrioritasSatker(string pManfaatId)
        {
            Entities.PrioritasAlokasi satkerAlokasi = new Entities.PrioritasAlokasi();
            using (var ctx = new PnbpContext())
            {
                List<object> lstparams = new List<object>();

                string sql = @"select r1.manfaatid, to_char(r1.prioritaskegiatan) as prioritaskegiatan, r1.kodesatker, r1.namakantor as namasatker, r1.namaprogram, r1.nilaianggaran as nilaianggaran, nvl(r2.sudahalokasi,0) as teralokasi, nvl(r2.nilaialokasi,0) as alokasi,
                                case when r1.statusaktif = 1 then 'Aktif' else 'Tidak Aktif' end as statusaktif
                                from manfaat r1
	                                LEFT JOIN (SELECT MANFAATID, SUM(CASE WHEN STATUSEDIT = 0 THEN NILAIALOKASI ELSE NULL END) AS SUDAHALOKASI,SUM(CASE WHEN STATUSEDIT = 1 THEN NILAIALOKASI ELSE NULL END) AS NILAIALOKASI FROM MANFAATALOKASI WHERE STATUSAKTIF = 1 GROUP BY MANFAATID) r2 on
                                    r1.manfaatid = r2.manfaatid
                                where
                                r1.tahun = ({0})
                                and r1.tipe = 'NONOPS'
                                and r1.manfaatid = :manfaatid";

                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("manfaatid", pManfaatId));
                sql = sWhitespace.Replace(String.Format(sql, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                var parameters = lstparams.ToArray();

                satkerAlokasi = ctx.Database.SqlQuery<Entities.PrioritasAlokasi>(sql, parameters).FirstOrDefault();
            }

            return satkerAlokasi;
        }

        public Entities.TransactionResult UpdatePrioritas(string id, string destination, string origin, string status)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = @"update manfaat set prioritaskegiatan = :destination, prioritasasal = :origin ";
                        if (!string.IsNullOrEmpty(status))
                        {
                            if (status == "Aktif")
                            {
                                sql += " ,STATUSAKTIF = 1 ";
                            }
                            else if (status == "Tidak Aktif")
                            {
                                sql += " ,STATUSAKTIF = 0 ";
                            }
                        }
                               sql += " where manfaatid = :id";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("destination", Convert.ToInt32(destination)));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("origin", Convert.ToInt32(origin)));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil diubah";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

        public Entities.TransactionResult UpdateDataManfaat(Entities.PrioritasAlokasi frm)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        Regex sWhitespace = new Regex(@"\s+");

                        ArrayList arrayListParameters = new ArrayList();

                        string sql = @"UPDATE manfaat SET
                                              nilaianggaran = :NilaiAnggaran, 
                                              prioritaskegiatan = :destination, 
                                              prioritasasal = :origin, 
                                              ANGGJAN = :AnggJan, ANGGFEB = :AnggFeb, ANGGMAR = :AnggMar, ANGGAPR = :AnggApr,
                                              ANGGMEI = :AnggMei, ANGGJUN = :AnggJun, ANGGJUL = :AnggJul, ANGGAGT = :AnggAgt,
                                              ANGGSEP = :AnggSep, ANGGOKT = :AnggOkt, ANGGNOV = :AnggNov, ANGGDES = :AnggDes,
                                              RANKJAN = :RankJan, RANKFEB = :RankFeb, RANKMAR = :RankMar, RANKAPR = :RankApr,
                                              RANKMEI = :RankMei, RANKJUN = :RankJun, RANKJUL = :RankJul, RANKAGT = :RankAgt,
                                              RANKSEP = :RankSep, RANKOKT = :RankOkt, RANKNOV = :RankNov, RANKDES = :RankDes,
                                              STATUSREVISI = 1 ";
                        if (!string.IsNullOrEmpty(frm.Statusaktif))
                        {
                            if (frm.Statusaktif == "Aktif")
                            {
                                sql += " ,STATUSAKTIF = 1 ";
                            }
                            else if (frm.Statusaktif == "Tidak Aktif")
                            {
                                sql += " ,STATUSAKTIF = 0 ";
                            }
                        }
                        sql += " WHERE manfaatid = :id";

                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NilaiAnggaran", frm.Nilaianggaran));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("destination", Convert.ToInt32(frm.Prioritaskegiatan)));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("origin", Convert.ToInt32(frm.PrioritasOrigin)));

                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AnggJan", frm.AnggJan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AnggFeb", frm.AnggFeb));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AnggMar", frm.AnggMar));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AnggApr", frm.AnggApr));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AnggMei", frm.AnggMei));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AnggJun", frm.AnggJun));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AnggJul", frm.AnggJul));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AnggAgt", frm.AnggAgt));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AnggSep", frm.AnggSep));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AnggOkt", frm.AnggOkt));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AnggNov", frm.AnggNov));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AnggDes", frm.AnggDes));

                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RankJan", frm.RankJan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RankFeb", frm.RankFeb));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RankMar", frm.RankMar));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RankApr", frm.RankApr));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RankMei", frm.RankMei));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RankJun", frm.RankJun));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RankJul", frm.RankJul));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RankAgt", frm.RankAgt));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RankSep", frm.RankSep));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RankOkt", frm.RankOkt));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RankNov", frm.RankNov));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("RankDes", frm.RankDes));

                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", frm.Manfaatid));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil diubah";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

        public Entities.TransactionResult UpdateDataManfaatV2(Entities.PrioritasAlokasi frm)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        Regex sWhitespace = new Regex(@"\s+");

                        ArrayList arrayListParameters = new ArrayList();

                        string sql = @"UPDATE manfaat SET NilaiAlokasi = :JumlahAlokasi";
                        if (!string.IsNullOrEmpty(frm.Statusaktif))
                        {
                            if (frm.Statusaktif == "Aktif")
                            {
                                sql += " ,STATUSAKTIF = 1 ";
                            }
                            else if (frm.Statusaktif == "Tidak Aktif")
                            {
                                sql += " ,STATUSAKTIF = 0 ";
                            }
                        }
                        sql += " WHERE manfaatid = :id";

                        sql = sWhitespace.Replace(sql, " ");
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JumlahAlokasi", frm.JUMLAHALOKASI));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", frm.Manfaatid));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil diubah";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

        public Entities.TransactionResult KunciDataManfaatV2(string kantorid)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        Regex sWhitespace = new Regex(@"\s+");

                        ArrayList arrayListParameters = new ArrayList();

                        string sql = @"UPDATE manfaat SET statusedit = 0 
                                    where kantorid = :kantorid and statusaktif = 1 and tahun = extract(year from sysdate)";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kantorid", kantorid));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        var row = ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil diubah";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

        public List<Pnbp.Entities.Pengumuman> GetPengumuman(string judulBerita, string isiBerita, string tanggalMulai, string tanggalBerakhir, int from, int to)
        {
            List<Pnbp.Entities.Pengumuman> records = new List<Pnbp.Entities.Pengumuman>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT rnumber,beritaappid,judulberita,isiberita,TanggalMulai,TanggalBerakhir,TOTAL FROM ( " +
                "    SELECT " +
                "        ROW_NUMBER() over (ORDER BY beritaapp.validsejak, beritaapp.judulberita, beritaapp.beritaappid) RNUMBER, " +
                "        beritaapp.beritaappid, beritaapp.judulberita, beritaapp.isiberita, " +
                "        to_char(beritaapp.validsejak, 'DD/MM/YYYY') TanggalMulai, " +
                "        to_char(beritaapp.validsampai, 'DD/MM/YYYY') TanggalBerakhir, " +
                "        COUNT(1) OVER() TOTAL " +
                "    FROM " +
                "        KKPWEB.beritaapp " +
                "    WHERE " +
                "        beritaapp.APPLICATIONNAME = :ApplicationName " +
                "        AND (beritaapp.STATUSHAPUS IS NULL OR beritaapp.STATUSHAPUS = '0') ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ApplicationName", "PNBP"));

            if (!String.IsNullOrEmpty(judulBerita))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JudulBerita", String.Concat("%", judulBerita.ToLower(), "%")));
                query += " AND LOWER(beritaapp.judulberita) LIKE :JudulBerita ";
            }
            if (!String.IsNullOrEmpty(isiBerita))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("IsiBerita", String.Concat("%", isiBerita.ToLower(), "%")));
                query += " AND LOWER(beritaapp.isiberita) LIKE :IsiBerita ";
            }
            if (!String.IsNullOrEmpty(tanggalMulai))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalDari", tanggalMulai));
                query += " AND beritaapp.validsejak >= TO_DATE( :TanggalDari, 'DD/MM/YYYY')  ";
            }
            if (!String.IsNullOrEmpty(tanggalBerakhir))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalSampai", tanggalBerakhir));
                query += " AND beritaapp.validsampai < (TO_DATE( :TanggalSampai, 'DD/MM/YYYY')+1) ";
            }

            query +=
                " ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Pnbp.Entities.Pengumuman>(query, parameters).ToList<Pnbp.Entities.Pengumuman>();
            }

            return records;
        }

        public Pnbp.Entities.Pengumuman GetPengumumanById(string id)
        {
            Pnbp.Entities.Pengumuman records = new Pnbp.Entities.Pengumuman();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    beritaapp.beritaappid, beritaapp.judulberita, beritaapp.isiberita, " +
                "    to_char(beritaapp.validsejak, 'DD/MM/YYYY') TanggalMulai, " +
                "    to_char(beritaapp.validsampai, 'DD/MM/YYYY') TanggalBerakhir " +
                "FROM " +
                "    KKPWEB.beritaapp " +
                "WHERE " +
                "    beritaapp.beritaappid = :Id ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Pnbp.Entities.Pengumuman>(query, parameters).FirstOrDefault();
            }

            return records;
        }

        public int JumlahPengumuman(string judulBerita)
        {
            int result = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query = "SELECT count(*) FROM KKPWEB.beritaapp WHERE (beritaapp.STATUSHAPUS IS NULL OR beritaapp.STATUSHAPUS = '0') ";

            if (!String.IsNullOrEmpty(judulBerita))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JudulBerita", judulBerita.ToLower()));
                query += " AND LOWER(judulberita) = :JudulBerita ";
            }

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public Pnbp.Entities.TransactionResult SimpanPengumuman(Pnbp.Entities.Pengumuman pengumuman)
        {
            Pnbp.Entities.TransactionResult tr = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string messfooter = " berhasil dibuat.";
                        Regex sWhitespace = new Regex(@"\s+");

                        string sql = "";
                        ArrayList arrayListParameters = new ArrayList();
                        object[] parameters = null;

                        if (!String.IsNullOrEmpty(pengumuman.BeritaAppId))
                        {
                            // Edit mode
                            sql =
                                @"UPDATE KKPWEB.beritaapp SET
                                    judulberita = :JudulBerita, isiberita = :IsiBerita, 
                                    validsejak = TO_DATE(:TanggalMulai,'DD/MM/YYYY'), 
                                    validsampai = TO_DATE(:TanggalBerakhir,'DD/MM/YYYY')
                                WHERE beritaappid = :Id";
                            sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JudulBerita", pengumuman.JudulBerita.ToUpper()));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("IsiBerita", pengumuman.IsiBerita));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalMulai", pengumuman.TanggalMulai));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalBerakhir", pengumuman.TanggalBerakhir));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", pengumuman.BeritaAppId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            messfooter = " berhasil disimpan.";
                        }
                        else
                        {
                            // Insert Mode
                            sql =
                                @"INSERT INTO KKPWEB.beritaapp (
                                    beritaappid, judulberita, isiberita, validsejak, validsampai, APPLICATIONNAME) VALUES 
                                (
                                    :Id,:JudulBerita,:IsiBerita,
                                    TO_DATE(:TanggalMulai,'DD/MM/YYYY'),
                                    TO_DATE(:TanggalBerakhir,'DD/MM/YYYY'),
                                    :ApplicationName)";

                            sql = sWhitespace.Replace(sql, " ");

                            string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault<string>();
                            pengumuman.BeritaAppId = id;

                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JudulBerita", pengumuman.JudulBerita.ToUpper()));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("IsiBerita", pengumuman.IsiBerita));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalMulai", pengumuman.TanggalMulai));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalBerakhir", pengumuman.TanggalBerakhir));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ApplicationName", "PNBP"));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = pengumuman.BeritaAppId;
                        tr.Pesan = "Data Pengumuman " + pengumuman.JudulBerita.ToUpper() + messfooter;
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

        public Entities.TransactionResult HapusPengumuman(string id, string userid)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        // Hapus beritaapp
                        //string sql = @"DELETE FROM beritaapp WHERE beritaappid = :id";
                        string sql = @"UPDATE KKPWEB.beritaapp SET STATUSHAPUS = '1', USERHAPUS = :userid WHERE beritaappid = :id";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("userid", userid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Pengumuman berhasil dihapus";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

        public string GetPengumumanText()
        {
            string result = "";

            using (var ctx = new PnbpContext())
            {
                List<Pnbp.Entities.Pengumuman> records = new List<Pnbp.Entities.Pengumuman>();

                string tanggalsekarang = ctx.Database.SqlQuery<string>("SELECT to_char(SYSDATE, 'DD/MM/YYYY', 'nls_date_language=INDONESIAN') FROM DUAL").FirstOrDefault<string>();

                ArrayList arrayListParameters = new ArrayList();

                string query =
                    "SELECT " +
                    "    rnumber,beritaappid,judulberita,isiberita,TanggalMulai,TanggalBerakhir,TOTAL FROM ( " +
                    "    SELECT " +
                    "        ROW_NUMBER() over (ORDER BY beritaapp.validsejak, beritaapp.judulberita, beritaapp.beritaappid) RNUMBER, " +
                    "        beritaapp.beritaappid, beritaapp.judulberita, beritaapp.isiberita, " +
                    "        to_char(beritaapp.validsejak, 'dd MONTH yyyy', 'nls_date_language=INDONESIAN') TanggalMulai, " +
                    "        to_char(beritaapp.validsampai, 'dd MONTH yyyy', 'nls_date_language=INDONESIAN') TanggalBerakhir, " +
                    "        COUNT(1) OVER() TOTAL " +
                    "    FROM " +
                    $"        {_schemaKKP}.beritaapp " +
                    "    WHERE " +
                    "        beritaapp.APPLICATIONNAME = :ApplicationName " +
                    "        AND (beritaapp.STATUSHAPUS IS NULL OR beritaapp.STATUSHAPUS = '0') " +
                    "        AND beritaapp.validsejak < (TO_DATE( :TanggalDari, 'DD/MM/YYYY')+1) " +
                    "        AND beritaapp.validsampai >= TO_DATE( :TanggalSampai, 'DD/MM/YYYY') ";

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ApplicationName", "PNBP"));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalDari", tanggalsekarang));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalSampai", tanggalsekarang));

                query += " ) ";

                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Pnbp.Entities.Pengumuman>(query, parameters).ToList<Pnbp.Entities.Pengumuman>();

                foreach (Pnbp.Entities.Pengumuman pengumuman in records)
                {
                    result +=
                        "<b style='color:#26b99a;'>" + pengumuman.JudulBerita + "</b><br />" +
                        "<b style='font-size:smaller;'>" + pengumuman.TanggalMulai + "</b>" +
                        "<br /><br />" +
                        pengumuman.IsiBerita + "<br /><br />";
                }
            }

            return result;
        }

        public string GetAksesEditPagu()
        {
            string result = "N";

            string query =
                "SELECT nilai " +
                "FROM konfigurasi " +
                "WHERE katakunci = 'AKSESEDITMANFAAT'";

            using (var ctx = new PnbpContext())
            {
                result = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
            }

            return result;
        }

        public Pnbp.Entities.TransactionResult SimpanAksesEditPagu(Pnbp.Entities.Konfigurasi konfigurasi)
        {
            Pnbp.Entities.TransactionResult tr = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = "";
                        ArrayList arrayListParameters = new ArrayList();
                        object[] parameters = null;

                        if (!String.IsNullOrEmpty(konfigurasi.KataKunci))
                        {
                            sql =
                                @"UPDATE konfigurasi SET
                                    nilai = :Nilai
                                WHERE katakunci = :KataKunci";
                            sql = sWhitespace.Replace(sql, " ");
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nilai", konfigurasi.Nilai.ToUpper()));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KataKunci", konfigurasi.KataKunci));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil disimpan";
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

        public Pnbp.Entities.TransactionResult SimpanFile(Pnbp.Entities.File file)
        {
            Pnbp.Entities.TransactionResult tr = new Pnbp.Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string messfooter = " berhasil dibuat.";
                        Regex sWhitespace = new Regex(@"\s+");

                        string sql = "";
                        ArrayList arrayListParameters = new ArrayList();
                        object[] parameters = null;

                       
                        // Insert Mode
                        sql =
                            @"INSERT INTO upload_file (
                                file_id, file_name, file_path) VALUES 
                            (
                                :file_id,:file_name,:file_path)";

                        sql = sWhitespace.Replace(sql, " ");

                        string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault<string>();
                        file.file_id = id;

                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("file_id", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("file_name", file.file_name));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("file_path", file.file_path));
                       
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = file.file_id;
                        tr.Pesan = "Data Image " + file.file_name.ToUpper() + messfooter;
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }
    }
}
