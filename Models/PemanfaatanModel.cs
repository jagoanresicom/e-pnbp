using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Text.RegularExpressions;
using System.Collections;
using System.Configuration;

namespace Pnbp.Models
{
    public class PemanfaatanModel
    {
        Regex sWhitespace = new Regex(@"\s+");

        public string GetUID()
        {
            string id = "";

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault<string>();
                    }
                    finally
                    {
                        ctx.Dispose();
                    }
                }
            }

            return id;
        }

        public int GetServerYear()
        {
            int retval = DateTime.Now.Year;

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string result = ctx.Database.SqlQuery<string>("SELECT to_char(sysdate,'YYYY') FROM DUAL").FirstOrDefault<string>();
                        retval = Convert.ToInt32(result);
                    }
                    finally
                    {
                        ctx.Dispose();
                    }
                }
            }

            return retval;
        }

        public List<Pnbp.Entities.Tahun> ListTahun()
        {
            List<Pnbp.Entities.Tahun> lstTahun = new List<Pnbp.Entities.Tahun>();

            int tahunberjalan = GetServerYear();
            int tahunsebelumnya = tahunberjalan - 1;
            int tahunsetelahnya = tahunberjalan + 1;

            Entities.Tahun tahun = new Entities.Tahun();
            tahun.value = tahunsebelumnya.ToString();
            tahun.tahun = tahunsebelumnya.ToString();
            lstTahun.Add(tahun);

            tahun = new Entities.Tahun();
            tahun.value = tahunberjalan.ToString();
            tahun.tahun = tahunberjalan.ToString();
            lstTahun.Add(tahun);

            tahun = new Entities.Tahun();
            tahun.value = tahunsetelahnya.ToString();
            tahun.tahun = tahunsetelahnya.ToString();
            lstTahun.Add(tahun);

            return lstTahun;
        }

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
        public bool UsersInRoles(string username, string rolename)
        {
            bool bolvalue = false;

            using (var ctx = new PnbpContext())
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p0 = new Oracle.ManagedDataAccess.Client.OracleParameter("param0", username);
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", rolename);
                object[] parameters = new object[2] { p0, p1 };
                string sql = "SELECT count(*) FROM simpeg.UsersInRoles a join users b on a.username = b.username WHERE b.userid = :param0 AND a.Rolename =:param1 ";
                int hasil = ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault();

                bolvalue = hasil > 0;
            }

            return bolvalue;
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

                result = (string.IsNullOrEmpty(getstatus) || getstatus == "0") ? false : true;
            }

            return result;
        }

        public string GetAksesEditManfaat()
        {
            string result = "";

            string query =
                "select nilai from konfigurasi " +
                "where katakunci = 'AKSESEDITMANFAAT'";

            using (var ctx = new PnbpContext())
            {
                result = ctx.Database.SqlQuery<string>(query).FirstOrDefault();
            }

            return result;
        }

        public static List<Entities.Wilayah> getKantorKanwil(bool iskanwil, string kantorid)
        {
            List<Entities.Wilayah> listWilayah = new List<Entities.Wilayah>();
            string sql = "";
            if (iskanwil)
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
        public List<Entities.Program> getProgramList(string tipeops)
        {
            List<Entities.Program> listProgram = new List<Entities.Program>();
            string sql = "select programid, nama from program where statusaktif=1 and tipeops= '" + tipeops + "' ";
            using (var ctx = new PnbpContext())
            {
                listProgram = ctx.Database.SqlQuery<Entities.Program>(sql).ToList<Entities.Program>();
            }

            return listProgram;
        }

        public List<Entities.SatuanKerja> getSatKerList()
        {
            List<Entities.SatuanKerja> listSatker = new List<Entities.SatuanKerja>();
            string sql = "select kantorid, kodesatker, nama_satker namasatker from satker where statusaktif=1 and tahun=2018";
            using (var ctx = new PnbpContext())
            {
                listSatker = ctx.Database.SqlQuery<Entities.SatuanKerja>(sql).ToList<Entities.SatuanKerja>();
            }

            return listSatker;
        }

        public List<Entities.TotalAnggaranAlokasi> GetTotalAnggaranAlokasi(string tahun)
        {
            List<Entities.TotalAnggaranAlokasi> satkerAlokasi = new List<Entities.TotalAnggaranAlokasi>();

            ArrayList arrayListParameters = new ArrayList();
            try {
                using (var ctx = new PnbpContext())
                {
                    //string sql = @"SELECT sum(nilaianggaran) TotalNilaiAnggaran, sum(nilaialokasi) TotalNilaiAlokasi FROM (
                    //                  select
                    //                     r1.kantorid, r1.kode as kodekantor, r1.kodesatker, r1.nama_satker as namasatker,
                    //                     nvl(sum(r2.nilaianggaran),0) as nilaianggaran, nvl(sum(r3.nilaialokasi),0) as nilaialokasi, case when r1.statusaktif = 1 then 'Aktif' else 'Tidak Aktif' end as statusaktif
                    //                  from
                    //                     satker r1
                    //                     join manfaat r2 on
                    //                        r1.kantorid = r2.kantorid and r2.tipe='NONOPS'
                    //                     left join (select manfaatid, sum(nilaialokasi) as nilaialokasi from manfaatalokasi where statusedit = 0 and statusaktif = 1 group by manfaatid) r3 on
                    //                        r2.manfaatid = r3.manfaatid
                    //                  where
                    //                     r2.tahun = :Tahun
                    //                  group by
                    //                     r1.kode, r1.kantorid, r1.kodesatker, r1.nama_satker, r1.statusaktif
                    //               ) ";

                    string sql = @"SELECT SUM( ANGGJAN + ANGGFEB + ANGGMAR) + SUM( NILAIALOKASI ) TotalNilaiAlokasi FROM MANFAAT WHERE TAHUN = 2021 ";
                    //string sql = @"SELECT SUM( ANGGJAN + ANGGFEB + ANGGMAR) + SUM( NILAIALOKASI ) AS ALOKASI  FROM MANFAAT WHERE TAHUN = :Tahun ";
                    //SELECT SUM(ANGGJAN + ANGGFEB + ANGGMAR) + SUM(NILAIALOKASI) AS ALOKASI  FROM MANFAAT WHERE TAHUN = (SELECT(to_char(SYSDATE, 'YYYY')) AS Y FROM dual

                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));

                    sql = sWhitespace.Replace(sql, " ");

                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    satkerAlokasi = ctx.Database.SqlQuery<Entities.TotalAnggaranAlokasi>(sql, parameters).ToList<Entities.TotalAnggaranAlokasi>();
                }
            } 
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return satkerAlokasi;
        }

        public List<Entities.TotalAnggaranAlokasi> GetTotalAnggaranAlokasiV2(string tahun)
        {
            List<Entities.TotalAnggaranAlokasi> satkerAlokasi = new List<Entities.TotalAnggaranAlokasi>();

            ArrayList arrayListParameters = new ArrayList();
            try
            {
                using (var ctx = new PnbpContext())
                {
                    string sql = @"SELECT SUM( NILAIALOKASI ) TotalNilaiAlokasi FROM MANFAAT WHERE TAHUN = 2021 ";
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                    sql = sWhitespace.Replace(sql, " ");
                    object[] parameters = arrayListParameters.OfType<object>().ToArray();
                    satkerAlokasi = ctx.Database.SqlQuery<Entities.TotalAnggaranAlokasi>(sql, parameters).ToList<Entities.TotalAnggaranAlokasi>();
                }
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return satkerAlokasi;
        }

        public List<Entities.TotalDipaBelanja> GetTotalAnggaranDipa(string tahun)
        {
            List<Entities.TotalDipaBelanja> satkerAlokasi = new List<Entities.TotalDipaBelanja>();

            ArrayList arrayListParameters = new ArrayList();

            using (var ctx = new PnbpContext())
            {
                //string sql = @"SELECT sum(nilaianggaran) TotalNilaiAnggaran, sum(nilaialokasi) TotalNilaiAlokasi FROM (
                //                  select
                //                     r1.kantorid, r1.kode as kodekantor, r1.kodesatker, r1.nama_satker as namasatker,
                //                     nvl(sum(r2.nilaianggaran),0) as nilaianggaran, nvl(sum(r3.nilaialokasi),0) as nilaialokasi, case when r1.statusaktif = 1 then 'Aktif' else 'Tidak Aktif' end as statusaktif
                //                  from
                //                     satker r1
                //                     join manfaat r2 on
                //                        r1.kantorid = r2.kantorid and r2.tipe='NONOPS'
                //                     left join (select manfaatid, sum(nilaialokasi) as nilaialokasi from manfaatalokasi where statusedit = 0 and statusaktif = 1 group by manfaatid) r3 on
                //                        r2.manfaatid = r3.manfaatid
                //                  where
                //                     r2.tahun = :Tahun
                //                  group by
                //                     r1.kode, r1.kantorid, r1.kodesatker, r1.nama_satker, r1.statusaktif
                //               ) ";

                if (string.IsNullOrEmpty(tahun))
                {
                    tahun = DateTime.Now.Year.ToString();
                }
                string sql = $@"
                SELECT 
	                CASE 
	                WHEN totalpagu IS NULL THEN 0
	                ELSE totalpagu
	                END
                FROM (
                    SELECT SUM( AMOUNT ) AS TOTALPAGU FROM SPAN_BELANJA WHERE SUMBER_DANA = 'D' AND KDSATKER <> '524465' AND TAHUN = {tahun}
                )";
                //string sql = @"SELECT SUM( ANGGJAN + ANGGFEB + ANGGMAR) + SUM( NILAIALOKASI ) AS ALOKASI  FROM MANFAAT WHERE TAHUN = :Tahun ";
                //SELECT SUM(ANGGJAN + ANGGFEB + ANGGMAR) + SUM(NILAIALOKASI) AS ALOKASI  FROM MANFAAT WHERE TAHUN = (SELECT(to_char(SYSDATE, 'YYYY')) AS Y FROM dual

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));

                sql = sWhitespace.Replace(sql, " ");

                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                satkerAlokasi = ctx.Database.SqlQuery<Entities.TotalDipaBelanja>(sql, parameters).ToList<Entities.TotalDipaBelanja>();
            }

            return satkerAlokasi;
        }

        public List<Entities.SatkerAlokasi> GetDataManfaat(string tahun, string namasatker, decimal? nilaianggaran, string tipekantorid, string kantorid, int from, int to)
        {
            List<Entities.SatkerAlokasi> satkerAlokasi = new List<Entities.SatkerAlokasi>();

            ArrayList arrayListParameters = new ArrayList();
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();

            string summary = "";
            if (currentMonth == "1")
            {
                summary = "SUM(ANGGJAN)";
            }
            else if (currentMonth == "2")
            {
                summary = "SUM(ANGGJAN + ANGGFEB)";
            }
            else if (currentMonth == "3")
            {
                summary = "SUM(ANGGJAN + ANGGFEB + ANGGMAR)";
            }
            using (var ctx = new PnbpContext())
            {
                string sql = @"SELECT * FROM (
                                  select
                                     row_number() over (order by r1.kodesatker asc) as RNumber, COUNT(1) OVER() Total,
                                     r1.kantorid, r1.kode as kodekantor, r1.kodesatker, r1.nama_satker as namasatker,
                                     r4.renaksisatkerid, decode(r4.renaksisatkerid, null, 0, 1) AS StatusRenaksi,
                                     nvl(sum(r2.nilaianggaran),0) as nilaianggaran, nvl(sum(r3.nilaialokasi),0) as nilaialokasi, case when r1.statusaktif = 1 then 'Aktif' else 'Tidak Aktif' end as statusaktif,
                                     NVL(SUM(r2.TOTALALOKASI),0) + SUM(ANGGJAN + ANGGFEB + ANGGMAR) AS JUMLAHALOKASI
                                  from
                                     satker r1
                                     left join renaksisatker r4 on
                                        r4.kantorid = r1.kantorid and r4.tahun = :Tahun1
                                     join manfaat r2 on
                                        r1.kantorid = r2.kantorid
                                     left join (select manfaatid, sum(nilaialokasi) as nilaialokasi from manfaatalokasi where statusedit = 0 and statusaktif = 1 group by manfaatid) r3 on
                                        r2.manfaatid = r3.manfaatid
                                  where
                                     r2.tahun = :Tahun2";

                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("jmlsum", string.Concat(summary)));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun1", tahun));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun2", tahun));
                

                if (!String.IsNullOrEmpty(namasatker))
                {
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaSatker", String.Concat("%", namasatker.ToLower(), "%")));
                    sql += " AND LOWER(r2.namakantor) LIKE :NamaSatker ";
                }

                if (tipekantorid == "2" || tipekantorid == "3" || tipekantorid == "4")
                {
                    if (!string.IsNullOrEmpty(kantorid))
                    {
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));
                        sql += " and r2.kantorid IN (SELECT kantorid FROM kantor START WITH kantorid = :KantorId CONNECT BY NOCYCLE PRIOR kantorid = induk) ";
                    }
                }

                if (nilaianggaran != null)
                {
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NilaiAnggaran", nilaianggaran));
                    sql += " AND r2.nilaianggaran >= :NilaiAnggaran ";
                }

                sql += @"
                                  group by
                                     r1.kode, r1.kantorid, r1.kodesatker, r1.nama_satker, r1.statusaktif, r4.renaksisatkerid
                                     ORDER BY r1.kodesatker ASC
                               ) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

                sql = sWhitespace.Replace(sql, " ");

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                satkerAlokasi = ctx.Database.SqlQuery<Entities.SatkerAlokasi>(sql, parameters).ToList<Entities.SatkerAlokasi>();
            }

            return satkerAlokasi;
        }

        public List<Entities.SatkerAlokasi> GetDataManfaatV2(string tahun, string namasatker, decimal? nilaianggaran, string tipekantorid, string kantorid, int from, int to)
        {
            List<Entities.SatkerAlokasi> satkerAlokasi = new List<Entities.SatkerAlokasi>();

            ArrayList arrayListParameters = new ArrayList();
            using (var ctx = new PnbpContext())
            {
                string sql = @"
                    SELECT * FROM ( 
                        SELECT 
	                        row_number() over (order by s.kodesatker asc) as RNumber, 
	                        s.KODESATKER as Kodesatker, s.kantorid, s.NAMA_SATKER as Namasatker, ta.pagu as Nilaianggaran, ta.alokasi as JUMLAHALOKASI
	                        FROM satker s
	                        JOIN (
	                         SELECT t.kdsatker, t.pagu, t.alokasi
	                         FROM ALOKASISATKER t
	                        ) ta ON s.KODESATKER = ta.kdsatker
                    ) WHERE RNumber BETWEEN :startCnt AND :limitCnt
                ";

                sql = sWhitespace.Replace(sql, " ");

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                satkerAlokasi = ctx.Database.SqlQuery<Entities.SatkerAlokasi>(sql, parameters).ToList<Entities.SatkerAlokasi>();
            }

            return satkerAlokasi;
        }

        public string GetDataManfaatTest(string tahun, string namasatker, decimal? nilaianggaran, string tipekantorid, string kantorid, int from, int to)
        {
            string sql = "";
            List<Entities.SatkerAlokasi> satkerAlokasi = new List<Entities.SatkerAlokasi>();

            ArrayList arrayListParameters = new ArrayList();
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();

            string summary = "";
            if (currentMonth == "1")
            {
                summary = "SUM(ANGGJAN)";
            }
            else if (currentMonth == "2")
            {
                summary = "SUM(ANGGJAN + ANGGFEB)";
            }
            else if (currentMonth == "3")
            {
                summary = "SUM(ANGGJAN + ANGGFEB + ANGGMAR)";
            }
            using (var ctx = new PnbpContext())
            {
                sql = @"SELECT * FROM (
                                  select
                                     row_number() over (order by r1.kode) as RNumber, COUNT(1) OVER() Total,
                                     r1.kantorid, r1.kode as kodekantor, r1.kodesatker, r1.nama_satker as namasatker,
                                     r4.renaksisatkerid, decode(r4.renaksisatkerid, null, 0, 1) AS StatusRenaksi,
                                     nvl(sum(r2.nilaianggaran),0) as nilaianggaran, nvl(sum(r3.nilaialokasi),0) as nilaialokasi, case when r1.statusaktif = 1 then 'Aktif' else 'Tidak Aktif' end as statusaktif,
                                     NVL(SUM(r2.TOTALALOKASI),0) + :jmlsum AS JUMLAHALOKASI
                                  from
                                     satker r1
                                     left join renaksisatker r4 on
                                        r4.kantorid = r1.kantorid and r4.tahun = :Tahun1
                                     join manfaat r2 on
                                        r1.kantorid = r2.kantorid
                                     left join (select manfaatid, sum(nilaialokasi) as nilaialokasi from manfaatalokasi where statusedit = 0 and statusaktif = 1 group by manfaatid) r3 on
                                        r2.manfaatid = r3.manfaatid
                                  where
                                     r2.tahun = :Tahun2 ";

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun1", tahun));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun2", tahun));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("jmlsum", summary));

                if (!String.IsNullOrEmpty(namasatker))
                {
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaSatker", String.Concat("%", namasatker.ToLower(), "%")));
                    sql += " AND LOWER(r2.namakantor) LIKE :NamaSatker ";
                }

                if (tipekantorid == "2" || tipekantorid == "3" || tipekantorid == "4")
                {
                    if (!string.IsNullOrEmpty(kantorid))
                    {
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));
                        sql += " and r2.kantorid IN (SELECT kantorid FROM kantor START WITH kantorid = :KantorId CONNECT BY NOCYCLE PRIOR kantorid = induk) ";
                    }
                }

                if (nilaianggaran != null)
                {
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NilaiAnggaran", nilaianggaran));
                    sql += " AND r2.nilaianggaran >= :NilaiAnggaran ";
                }

                sql += @"
                                  group by
                                     r1.kode, r1.kantorid, r1.kodesatker, r1.nama_satker, r1.statusaktif, r4.renaksisatkerid
                               ) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

                sql = sWhitespace.Replace(sql, " ");

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                //satkerAlokasi = ctx.Database.SqlQuery<Entities.SatkerAlokasi>(sql, parameters).ToList<Entities.SatkerAlokasi>();
            }

            return sql;
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
                            @"select case when statusaktif = 1 then 'Aktif' else 'Tidak Aktif' end as statusaktif from satker where kodesatker = '" + id + "'";
                        currentstatus = ctx.Database.SqlQuery<string>(sqlstring).FirstOrDefault();

                        if (status != currentstatus)
                        {
                            string sql = @"update satker set statusaktif = :status where kodesatker = :id and tahun = ({0})";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("status", (status == "Aktif") ? 1 : 0));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                            object[] parameters = arrayListParameters.OfType<object>().ToArray();
                            sql = sWhitespace.Replace(String.Format(sql, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            if (status == "Aktif")
                            {
                                sql1 = @"update manfaat r1 set r1.statusaktif = 1 where kodesatker = :id
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
                                            and r1.kodesatker = :id) k1 set k1.statusaktif = 0";
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

        public Entities.TransactionResult UpdatePersetujuanRevisi(string kodesatker, string namaprogram, string userbiroperencanaan, string userbirokeuangan, int statuspersetujuan)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "";

                        if (userbiroperencanaan == "True" && userbirokeuangan == "False")
                        {
                            sql = @"UPDATE manfaat SET persetujuan1 = :StatusPersetujuan WHERE kodesatker = :KodeSatker AND namaprogram = :NamaProgram AND statusrevisi = 1";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusPersetujuan", statuspersetujuan));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeSatker", kodesatker));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaProgram", namaprogram));
                            object[] parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else if (userbiroperencanaan == "False" && userbirokeuangan == "True")
                        {
                            sql = @"UPDATE manfaat SET persetujuan2 = :StatusPersetujuan WHERE kodesatker = :KodeSatker AND namaprogram = :NamaProgram AND statusrevisi = 1";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusPersetujuan", statuspersetujuan));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeSatker", kodesatker));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaProgram", namaprogram));
                            object[] parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

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

        public List<Entities.PrioritasAlokasi> GetManfaatDetail(string tahun, string kodesatker, string namasatker, string namaprogram, decimal? nilaianggaran, bool statusrevisi, int from, int to)
        {
            List<Entities.PrioritasAlokasi> records = new List<Entities.PrioritasAlokasi>();

            ArrayList arrayListParameters = new ArrayList();

            string query = @"SELECT * FROM (
                                 select r1.manfaatid, to_char(r1.prioritaskegiatan) as prioritaskegiatan,
                                        r1.kodesatker, r1.namakantor as namasatker, TRIM(r1.namaprogram) namaprogram, r1.nilaianggaran as nilaianggaran,
                                        nvl(r2.sudahalokasi,0) as teralokasi, nvl(r2.nilaialokasi,0) as alokasi,
                                        r1.STATUSREVISI, r1.PERSETUJUAN1, r1.PERSETUJUAN2,
                                        case when r1.statusaktif = 1 then 'Aktif' else 'Tidak Aktif' end as statusaktif,
                                        ROW_NUMBER() over (ORDER BY ktr.kode, r1.namaprogram, r1.nilaianggaran) RNUMBER,
                                        COUNT(1) OVER() TOTAL
                                 from manfaat r1
	                                  LEFT JOIN (SELECT MANFAATID,
                                                       SUM(CASE WHEN STATUSEDIT = 0 THEN NILAIALOKASI ELSE NULL END) AS SUDAHALOKASI,
                                                       SUM(CASE WHEN STATUSEDIT = 1 THEN NILAIALOKASI ELSE NULL END) AS NILAIALOKASI
                                                 FROM MANFAATALOKASI WHERE STATUSAKTIF = 1 GROUP BY MANFAATID) r2 on
                                    r1.manfaatid = r2.manfaatid
                                    JOIN kantor ktr ON ktr.kantorid = r1.kantorid
                                 where
                                      r1.tahun =  :Tahun ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));

            if (statusrevisi)
            {
                query += " AND r1.statusrevisi = 1 ";
            }

            if (!String.IsNullOrEmpty(kodesatker))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeSatker", String.Concat("%", kodesatker, "%")));
                query += " AND r1.kodesatker LIKE :KodeSatker ";
            }
            if (!String.IsNullOrEmpty(namasatker))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaSatker", String.Concat("%", namasatker.ToLower(), "%")));
                query += " AND LOWER(r1.namakantor) LIKE :NamaSatker ";
            }
            if (!String.IsNullOrEmpty(namaprogram))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaProgram", String.Concat("%", namaprogram.ToLower(), "%")));
                query += " AND LOWER(r1.namaprogram) LIKE :NamaProgram ";
            }
            if (nilaianggaran != null)
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NilaiAnggaran", nilaianggaran));
                query += " AND r1.nilaianggaran >= :NilaiAnggaran ";
            }

            query += @"               and r1.tipe = 'NONOPS'
                             ) WHERE RNUMBER BETWEEN :startCnt AND :limitCnt";

            query = sWhitespace.Replace(query, " ");

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.PrioritasAlokasi>(query, parameters).ToList<Entities.PrioritasAlokasi>();
            }

            return records;
        }

        public List<Entities.PrioritasAlokasi> GetManfaatSatker(string kantorid, string tahun)
        {
            List<Entities.PrioritasAlokasi> prioritasManfaat = new List<Entities.PrioritasAlokasi>();
            using (var ctx = new PnbpContext())
            {
                string currentMonth = DateTime.Now.Month.ToString();

                List<object> lstparams = new List<object>();
                string sql = @"select   r1.kode, r1.tipe, r1.manfaatid, 
                                        to_char(r1.prioritaskegiatan) as prioritaskegiatan, 
                                        r1.kodesatker, 
                                        r1.namakantor as namasatker, 
                                        r1.namaprogram, r1.nilaianggaran as nilaianggaran, 
                                        nvl(r2.sudahalokasi,0) as teralokasi, nvl(r2.nilaialokasi,0) as alokasi,
                                        r1.STATUSREVISI, r1.PERSETUJUAN1, r1.PERSETUJUAN2,
                                        case when r1.statusaktif = 1 then 'Aktif' 
                                        else 
                                        'Tidak Aktif' end as statusaktif, 
                                        sum(ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT) AS JUMLAHALOKASI,
                                        NVL(sum(r1.NILAIALOKASI),0) AS NILAIALOKASI
                                from manfaat r1
                                LEFT JOIN (SELECT MANFAATID, SUM(CASE WHEN STATUSEDIT = 0 THEN NILAIALOKASI ELSE NULL END) AS SUDAHALOKASI,
                                SUM(CASE WHEN STATUSEDIT = 1 THEN NILAIALOKASI ELSE NULL END) AS NILAIALOKASI FROM MANFAATALOKASI WHERE STATUSAKTIF = 1 GROUP BY MANFAATID) r2 on
                                    r1.manfaatid = r2.manfaatid
                               where
                                    r1.tahun = :tahunanggaran
                                    and r1.kantorid = :kdsatker ";

                sql += @"group by r1.manfaatid, r1.kode, r1.tipe, r1.prioritaskegiatan, r1.kodesatker, r1.namakantor, r1.namaprogram, r1.nilaianggaran, sudahalokasi, r2.nilaialokasi, r1.STATUSREVISI, r1.PERSETUJUAN1, r1.PERSETUJUAN2, r1.statusaktif
                       ORDER BY R1.KODE ASC ";

                //sql += @"ORDER BY R1.KODE ASC";

                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tahunanggaran", tahun));
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kdsatker", kantorid));
                //sql = sWhitespace.Replace(String.Format(sql, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                sql = sWhitespace.Replace(sql, " ");

                var parameters = lstparams.ToArray();

                prioritasManfaat = ctx.Database.SqlQuery<Entities.PrioritasAlokasi>(sql, parameters).ToList<Entities.PrioritasAlokasi>();
            }

            return prioritasManfaat;
        }

        public List<Entities.PrioritasAlokasi> GetManfaatSatkerV2(string kantorid, string tahun)
        {
            List<Entities.PrioritasAlokasi> prioritasManfaat = new List<Entities.PrioritasAlokasi>();
            using (var ctx = new PnbpContext())
            {
                string currentMonth = DateTime.Now.Month.ToString();

                List<object> lstparams = new List<object>();
                string sql = @"select   r1.kode, r1.tipe, r1.manfaatid, 
                                        to_char(r1.prioritaskegiatan) as prioritaskegiatan, 
                                        r1.kodesatker, 
                                        r1.namakantor as namasatker, 
                                        r1.namaprogram, r1.nilaianggaran as nilaianggaran, 
                                        nvl(r2.sudahalokasi,0) as teralokasi, nvl(r2.nilaialokasi,0) as alokasi,
                                        r1.STATUSREVISI, r1.PERSETUJUAN1, r1.PERSETUJUAN2,
                                        case when r1.statusaktif = 1 then 'Aktif' 
                                        else 
                                        'Tidak Aktif' end as statusaktif, 
                                        NVL(sum(r1.NILAIALOKASI),0) AS JUMLAHALOKASI,
                                        NVL(sum(r1.NILAIALOKASI),0) AS NILAIALOKASI
                                from manfaat r1
                                LEFT JOIN (SELECT MANFAATID, SUM(CASE WHEN STATUSEDIT = 0 THEN NILAIALOKASI ELSE NULL END) AS SUDAHALOKASI,
                                SUM(CASE WHEN STATUSEDIT = 1 THEN NILAIALOKASI ELSE NULL END) AS NILAIALOKASI FROM MANFAATALOKASI WHERE STATUSAKTIF = 1 GROUP BY MANFAATID) r2 on
                                    r1.manfaatid = r2.manfaatid
                               where
                                    r1.tahun = :tahunanggaran
                                    and r1.kantorid = :kdsatker ";

                sql += @"group by r1.manfaatid, r1.kode, r1.tipe, r1.prioritaskegiatan, r1.kodesatker, r1.namakantor, r1.namaprogram, r1.nilaianggaran, sudahalokasi, r2.nilaialokasi, r1.STATUSREVISI, r1.PERSETUJUAN1, r1.PERSETUJUAN2, r1.statusaktif
                       ORDER BY R1.KODE ASC ";

                //sql += @"ORDER BY R1.KODE ASC";

                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tahunanggaran", tahun));
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kdsatker", kantorid));
                //sql = sWhitespace.Replace(String.Format(sql, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                sql = sWhitespace.Replace(sql, " ");

                var parameters = lstparams.ToArray();

                prioritasManfaat = ctx.Database.SqlQuery<Entities.PrioritasAlokasi>(sql, parameters).ToList<Entities.PrioritasAlokasi>();
            }

            return prioritasManfaat;
        }

        public Entities.PrioritasAlokasi GetPrioritasSatker(string pManfaatId, string tahun)
        {
            if (string.IsNullOrEmpty(tahun))
            {
                tahun = ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            }

            Entities.PrioritasAlokasi satkerAlokasi = new Entities.PrioritasAlokasi();
            using (var ctx = new PnbpContext())
            {
                string currentMonth = DateTime.Now.Month.ToString();
                List<object> lstparams = new List<object>();

                string sql =
                    @"select 
                          r1.manfaatid, to_char(r1.prioritaskegiatan) as prioritaskegiatan, r1.kodesatker, 
                          r1.KantorId, r1.namakantor as namasatker, r1.namaprogram, r1.nilaianggaran as nilaianggaran, 
                          r1.ANGGJAN, r1.ANGGFEB, r1.ANGGMAR, r1.ANGGAPR, r1.ANGGMEI, r1.ANGGJUN, 
                          r1.ANGGJUL, r1.ANGGAGT, r1.ANGGSEP, r1.ANGGOKT, r1.ANGGNOV, r1.ANGGDES,
                          r1.RANKJAN, r1.RANKFEB, r1.RANKMAR, r1.RANKAPR, r1.RANKMEI, r1.RANKJUN, 
                          r1.RANKJUL, r1.RANKAGT, r1.RANKSEP, r1.RANKOKT, r1.RANKNOV, r1.RANKDES,
                          r1.ALOKJAN, r1.ALOKFEB, r1.ALOKMAR, r1.ALOKAPR, r1.ALOKMEI, r1.ALOKJUN, 
                          r1.ALOKJUL, r1.ALOKAGT, r1.ALOKSEP, r1.ALOKOKT, r1.ALOKNOV, r1.ALOKDES,
                          r1.STATUSREVISI, r1.PERSETUJUAN1, r1.PERSETUJUAN2,
                          nvl(r2.sudahalokasi,0) as teralokasi, nvl(r2.nilaialokasi,0) as alokasi,
                          case when r1.statusaktif = 1 then 'Aktif' else 'Tidak Aktif' end as statusaktif,
                          sum(ANGGJAN + ANGGFEB) AS JUMLAHALOKASI
                      from manfaat r1
	                       LEFT JOIN (
                               SELECT MANFAATID, SUM(CASE WHEN STATUSEDIT = 0 THEN NILAIALOKASI ELSE NULL END) AS SUDAHALOKASI,
                                      SUM(CASE WHEN STATUSEDIT = 1 THEN NILAIALOKASI ELSE NULL END) AS NILAIALOKASI 
                               FROM MANFAATALOKASI WHERE STATUSAKTIF = 1 GROUP BY MANFAATID) r2 on
                               r1.manfaatid = r2.manfaatid
                      where
                          r1.tahun = ({0})
                          and r1.manfaatid = :manfaatid";

                //and r1.tipe = 'NONOPS'
                sql += @"
                            GROUP BY
                            r1.manfaatid,
	                        r1.prioritaskegiatan,
	                        r1.kodesatker,
	                        r1.KantorId,
	                        r1.namakantor,
	                        r1.namaprogram,
	                        r1.nilaianggaran,
	                        r1.ANGGJAN,
	                        r1.ANGGFEB,
	                        r1.ANGGMAR,
	                        r1.ANGGAPR,
	                        r1.ANGGMEI,
	                        r1.ANGGJUN,
	                        r1.ANGGJUL,
	                        r1.ANGGAGT,
	                        r1.ANGGSEP,
	                        r1.ANGGOKT,
	                        r1.ANGGNOV,
	                        r1.ANGGDES,
	                        r1.RANKJAN,
	                        r1.RANKFEB,
	                        r1.RANKMAR,
	                        r1.RANKAPR,
	                        r1.RANKMEI,
	                        r1.RANKJUN,
	                        r1.RANKJUL,
	                        r1.RANKAGT,
	                        r1.RANKSEP,
	                        r1.RANKOKT,
	                        r1.RANKNOV,
	                        r1.RANKDES,
	                        r1.ALOKJAN,
	                        r1.ALOKFEB,
	                        r1.ALOKMAR,
	                        r1.ALOKAPR,
	                        r1.ALOKMEI,
	                        r1.ALOKJUN,
	                        r1.ALOKJUL,
	                        r1.ALOKAGT,
	                        r1.ALOKSEP,
	                        r1.ALOKOKT,
	                        r1.ALOKNOV,
	                        r1.ALOKDES,
	                        r1.STATUSREVISI,
	                        r1.PERSETUJUAN1,
	                        r1.PERSETUJUAN2,
	                        r2.sudahalokasi,
	                        r2.nilaialokasi,
	                        r1.statusaktif
                        ";
                //if (currentMonth == "1")
                //{
                //    sql += " and alokjan = 1";
                //}

                //if (currentMonth == "2")
                //{
                //    sql += " and alokjan = 1";
                //}

                //if (currentMonth == "3")
                //{
                //    sql += " and alokjan = 1 and alokfeb = 1";
                //}
                //if (currentMonth == "4")
                //{
                //    sql += " and alokjan = 1 and alokfeb = 1 and alokmar = 1";
                //}

                //if (currentMonth == "5")
                //{
                //    sql += " and alokjan = 1 and alokfeb = 1 and alokmar = 1 and alokapr = 1";
                //}

                //if (currentMonth == "6")
                //{
                //    sql += " and alokjan = 1 and alokfeb = 1 and alokmar = 1 and alokapr = 1 and alokmar = 1";
                //}

                //if (currentMonth == "7")
                //{
                //    sql += " and alokjan = 1 and alokfeb = 1 and alokmar = 1 and alokapr = 1 and alokmei = 1 and alokjun = 1";
                //}

                //if (currentMonth == "8")
                //{
                //    sql += " and alokjan = 1 and alokfeb = 1 and alokmar = 1 and alokapr = 1 and alokmei = 1 and alokjun = 1 and alokjul = 1";
                //}

                //if (currentMonth == "9")
                //{
                //    sql += " and alokjan = 1 and alokfeb = 1 and alokmar = 1 and alokapr = 1 and alokmei = 1 and alokjun = 1 and alokjul = 1 and alokagt = 1";
                //}

                //if (currentMonth == "10")
                //{
                //    sql += " and alokjan = 1 and alokfeb = 1 and alokmar = 1 and alokapr = 1 and alokmei = 1 and alokjun = 1 and alokjul = 1 and alokagt = 1 and aloksep = 1";
                //}

                //if (currentMonth == "11")
                //{
                //    sql += " and alokjan = 1 and alokfeb = 1 and alokmar = 1 and alokapr = 1 and alokmei = 1 and alokjun = 1 and alokjul = 1 and alokagt = 1 and aloksep = 1 and alokokt = 1";
                //}

                //if (currentMonth == "12")
                //{
                //    sql += " and alokjan = 1 and alokfeb = 1 and alokmar = 1 and alokapr = 1 and alokmei = 1 and alokjun = 1 and alokjul = 1 and alokagt = 1 and aloksep = 1 and alokokt = 1 and aloknov = 1";
                //}

                //sql += @"GROUP BY
	               //     r1.manfaatid,
	               //     r1.prioritaskegiatan,
	               //     r1.kodesatker,
	               //     r1.KantorId,
	               //     r1.namakantor,
	               //     r1.namaprogram,
	               //     r1.nilaianggaran,
	               //     r1.ANGGJAN,
	               //     r1.ANGGFEB,
	               //     r1.ANGGMAR,
	               //     r1.ANGGAPR,
	               //     r1.ANGGMEI,
	               //     r1.ANGGJUN,
	               //     r1.ANGGJUL,
	               //     r1.ANGGAGT,
	               //     r1.ANGGSEP,
	               //     r1.ANGGOKT,
	               //     r1.ANGGNOV,
	               //     r1.ANGGDES,
	               //     r1.RANKJAN,
	               //     r1.RANKFEB,
	               //     r1.RANKMAR,
	               //     r1.RANKAPR,
	               //     r1.RANKMEI,
	               //     r1.RANKJUN,
	               //     r1.RANKJUL,
	               //     r1.RANKAGT,
	               //     r1.RANKSEP,
	               //     r1.RANKOKT,
	               //     r1.RANKNOV,
	               //     r1.RANKDES,
	               //     r1.ALOKJAN,
	               //     r1.ALOKFEB,
	               //     r1.ALOKMAR,
	               //     r1.ALOKAPR,
	               //     r1.ALOKMEI,
	               //     r1.ALOKJUN,
	               //     r1.ALOKJUL,
	               //     r1.ALOKAGT,
	               //     r1.ALOKSEP,
	               //     r1.ALOKOKT,
	               //     r1.ALOKNOV,
	               //     r1.ALOKDES,
	               //     r1.STATUSREVISI,
	               //     r1.PERSETUJUAN1,
	               //     r1.PERSETUJUAN2,
	               //     r2.sudahalokasi,
	               //     r2.nilaialokasi,
	               //     r1.statusaktif"; 
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("manfaatid", pManfaatId));
                //sql = sWhitespace.Replace(String.Format(sql, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                sql = sWhitespace.Replace(String.Format(sql, tahun), " ");
                var parameters = lstparams.ToArray();

                satkerAlokasi = ctx.Database.SqlQuery<Entities.PrioritasAlokasi>(sql, parameters).FirstOrDefault();
            }

            return satkerAlokasi;
        }

        public Entities.PrioritasAlokasi GetPrioritasSatkerV2(string pManfaatId, string tahun)
        {
            if (string.IsNullOrEmpty(tahun))
            {
                tahun = ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            }

            Entities.PrioritasAlokasi satkerAlokasi = new Entities.PrioritasAlokasi();
            using (var ctx = new PnbpContext())
            {
                string currentMonth = DateTime.Now.Month.ToString();
                List<object> lstparams = new List<object>();

                string sql =
                    @"select 
                          r1.manfaatid, to_char(r1.prioritaskegiatan) as prioritaskegiatan, r1.kodesatker, 
                          r1.KantorId, r1.namakantor as namasatker, r1.namaprogram, r1.nilaianggaran as nilaianggaran, 
                          r1.ANGGJAN, r1.ANGGFEB, r1.ANGGMAR, r1.ANGGAPR, r1.ANGGMEI, r1.ANGGJUN, 
                          r1.ANGGJUL, r1.ANGGAGT, r1.ANGGSEP, r1.ANGGOKT, r1.ANGGNOV, r1.ANGGDES,
                          r1.RANKJAN, r1.RANKFEB, r1.RANKMAR, r1.RANKAPR, r1.RANKMEI, r1.RANKJUN, 
                          r1.RANKJUL, r1.RANKAGT, r1.RANKSEP, r1.RANKOKT, r1.RANKNOV, r1.RANKDES,
                          r1.ALOKJAN, r1.ALOKFEB, r1.ALOKMAR, r1.ALOKAPR, r1.ALOKMEI, r1.ALOKJUN, 
                          r1.ALOKJUL, r1.ALOKAGT, r1.ALOKSEP, r1.ALOKOKT, r1.ALOKNOV, r1.ALOKDES,
                          r1.STATUSREVISI, r1.PERSETUJUAN1, r1.PERSETUJUAN2,
                          nvl(r2.sudahalokasi,0) as teralokasi, nvl(r2.nilaialokasi,0) as alokasi,
                          case when r1.statusaktif = 1 then 'Aktif' else 'Tidak Aktif' end as statusaktif,
                          sum(ANGGJAN + ANGGFEB) AS JUMLAHALOKASI
                      from manfaat r1
	                       LEFT JOIN (
                               SELECT MANFAATID, SUM(CASE WHEN STATUSEDIT = 0 THEN NILAIALOKASI ELSE NULL END) AS SUDAHALOKASI,
                                      SUM(CASE WHEN STATUSEDIT = 1 THEN NILAIALOKASI ELSE NULL END) AS NILAIALOKASI 
                               FROM MANFAATALOKASI WHERE STATUSAKTIF = 1 GROUP BY MANFAATID) r2 on
                               r1.manfaatid = r2.manfaatid
                      where
                          r1.tahun = ({0})
                          and r1.tipe = 'NONOPS'
                          and r1.manfaatid = :manfaatid";

                sql += @"
                            GROUP BY
                            r1.manfaatid,
	                        r1.prioritaskegiatan,
	                        r1.kodesatker,
	                        r1.KantorId,
	                        r1.namakantor,
	                        r1.namaprogram,
	                        r1.nilaianggaran,
	                        r1.ANGGJAN,
	                        r1.ANGGFEB,
	                        r1.ANGGMAR,
	                        r1.ANGGAPR,
	                        r1.ANGGMEI,
	                        r1.ANGGJUN,
	                        r1.ANGGJUL,
	                        r1.ANGGAGT,
	                        r1.ANGGSEP,
	                        r1.ANGGOKT,
	                        r1.ANGGNOV,
	                        r1.ANGGDES,
	                        r1.RANKJAN,
	                        r1.RANKFEB,
	                        r1.RANKMAR,
	                        r1.RANKAPR,
	                        r1.RANKMEI,
	                        r1.RANKJUN,
	                        r1.RANKJUL,
	                        r1.RANKAGT,
	                        r1.RANKSEP,
	                        r1.RANKOKT,
	                        r1.RANKNOV,
	                        r1.RANKDES,
	                        r1.ALOKJAN,
	                        r1.ALOKFEB,
	                        r1.ALOKMAR,
	                        r1.ALOKAPR,
	                        r1.ALOKMEI,
	                        r1.ALOKJUN,
	                        r1.ALOKJUL,
	                        r1.ALOKAGT,
	                        r1.ALOKSEP,
	                        r1.ALOKOKT,
	                        r1.ALOKNOV,
	                        r1.ALOKDES,
	                        r1.STATUSREVISI,
	                        r1.PERSETUJUAN1,
	                        r1.PERSETUJUAN2,
	                        r2.sudahalokasi,
	                        r2.nilaialokasi,
	                        r1.statusaktif
                        ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("manfaatid", pManfaatId));
                //sql = sWhitespace.Replace(String.Format(sql, ConfigurationManager.AppSettings["TahunAnggaran"].ToString()), " ");
                sql = sWhitespace.Replace(String.Format(sql, tahun), " ");
                var parameters = lstparams.ToArray();

                satkerAlokasi = ctx.Database.SqlQuery<Entities.PrioritasAlokasi>(sql, parameters).FirstOrDefault();
            }

            return satkerAlokasi;
        }

        public Entities.TransactionResult UpdatePrioritas(string id, string destination, string origin, string status, string mode)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (mode == "New")
                        {
                        }
                        else
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
                        }

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
        public Entities.TransactionResult InsertPrioritas(Entities.PrioritasAlokasi frm, string pTahun, string userid)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (frm.Mode == "New")
                        {
                            //                            SELECT sys_guid(), 2018, kantorid, nama, :programid, :namaprogram, 
                            string sql =
                                @"INSERT INTO PEMANFAATAN(PEMANFAATANID, TANGGAL, KANTORID, NAMAKANTOR, PROGRAMID, NAMAPROGRAM, PRIORITASKEGIATAN, PRIORITASWILAYAHID, TERMIN, NILAIANGGARAN, NILAIBLOKIR, USERINSERT, INSERTDATE, USERUPDATE, LASTUPDATE)
                                            SELECT sys_guid(), sysdate, a.kantorid, a.NAMA_SATKER, b.programid, b.nama, :prior1, :prior2,0, :nilaianggaran ,0, :iduser, sysdate, :iduser1, sysdate
                                             from satker a join program b on b.programid = :idprogram
                                              where a.kantorid = :idkantor";

                            //@"INSERT INTO MANFAAT (MANFAATID,TAHUN,KANTORID,NAMAKANTOR,PROGRAMID,NAMAPROGRAM,TIPE,PRIORITASKEGIATAN,NILAIANGGARAN,NILAIALOKASI,STATUSFULLALOKASI,KODESATKER,PRIORITASASAL,STATUSAKTIF,TOTALALOKASI) 
                            //    VALUES (SYS_GUID(),:pTAHUN,:pKANTORID,:pNamaTabel,:pNAMAKANTOR,:pPROGRAMID,pNAMAPROGRAM,pTIPE,:pPRIORITASKEGIATAN,:pNILAIANGGARAN,:pNILAIALOKASI,:pSTATUSFULLALOKASI,:pKODESATKER, 1,:pTOTALALOKASI)";
                            Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("prior1", frm.Prioritaskegiatan);
                            Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("prior2", frm.Prioritaskegiatan);
                            Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("nilaianggaran", frm.Nilaianggaran);
                            Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("iduser", userid);
                            Oracle.ManagedDataAccess.Client.OracleParameter p5 = new Oracle.ManagedDataAccess.Client.OracleParameter("iduser1", userid);
                            Oracle.ManagedDataAccess.Client.OracleParameter p6 = new Oracle.ManagedDataAccess.Client.OracleParameter("idprogram", frm.ProgramId);
                            Oracle.ManagedDataAccess.Client.OracleParameter p7 = new Oracle.ManagedDataAccess.Client.OracleParameter("idkantor", frm.KantorId);
                            object[] parameters = new object[7] { p1, p2, p3, p4, p5, p6, p7 };
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }


                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil di tambahkan";
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

        public string[] GetProfileIdForUser(string userid, string kantorid)
        {
            var userRoles = new List<string>();

            try
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", kantorid);
                Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", userid);
                object[] myParams = new object[2] { p1, p2 };

                string sql =
                    "SELECT distinct profileid FROM PROFILEPEGAWAI JOIN PEGAWAI ON PEGAWAI.PEGAWAIID = PROFILEPEGAWAI.PEGAWAIID, users " +
                    "WHERE (CURRENT_DATE >= PROFILEPEGAWAI.VALIDSEJAK AND (CURRENT_DATE <= PROFILEPEGAWAI.VALIDSAMPAI OR PROFILEPEGAWAI.VALIDSAMPAI IS NULL)) " +
                    "AND PROFILEPEGAWAI.KANTORID = :param1 " +
                    "AND users.userid = pegawai.userid and users.userid =  :param2 ";
                using (var ctx = new PnbpContext())
                {
                    userRoles = ctx.Database.SqlQuery<string>(sql, myParams).ToList<string>();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return userRoles.ToArray();
        }

        public List<Entities.Kantor> GetKantorKanwil(string tipekantorid, string kantorid)
        {
            List<Entities.Kantor> records = new List<Entities.Kantor>();

            ArrayList arrayListParameters = new ArrayList();

            using (var ctx = new PnbpContext())
            {
                string sql = @"select
                                   row_number() over (order by kode) as RNumber, COUNT(1) OVER() Total,
                                   kantorid, kode kodekantor, replace(nama, 'Kantor Wilayah Provinsi ', '') namakantor, kota, alamat, email
                               from kantor
                               where tipekantorid = 2
                               order by kode";

                if (tipekantorid == "2")
                {
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));
                    sql = sql.Replace("where tipekantorid = 2", "where tipekantorid = 2 and kantorid = :KantorId ");
                }
                else if (tipekantorid == "3" || tipekantorid == "4")
                {
                    string kanwilid = Pnbp.Models.AdmModel.GetKanwilIdByKantahId(kantorid);
                    sql = @"select
                                row_number() over (order by kode) as RNumber, COUNT(1) OVER() Total,
                                kantorid, kode kodekantor, replace(nama, 'Kantor Wilayah Provinsi ', '') namakantor, kota, alamat, email
                            from kantor
                            where kantorid = :KantorId";
                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kanwilid));
                }

                sql = sWhitespace.Replace(sql, " ");

                object[] parameters = arrayListParameters.OfType<object>().ToArray();

                //records = ctx.Database.SqlQuery<Entities.Kantor>(sql).ToList<Entities.Kantor>();
                records = ctx.Database.SqlQuery<Entities.Kantor>(sql, parameters).ToList<Entities.Kantor>();
            }

            return records;
        }

        public List<Entities.Kantor> GetKantorSatker(string induk)
        {
            List<Entities.Kantor> records = new List<Entities.Kantor>();

            ArrayList arrayListParameters = new ArrayList();

            using (var ctx = new PnbpContext())
            {
                string sql = @"select
                                   row_number() over (order by kode) as RNumber, COUNT(1) OVER() Total,
                                   kantorid, kode kodekantor, replace(replace(nama, 'Kantor Pertanahan ', ''), 'Kantor Wilayah ', '') namakantor, kota, alamat, email
                               from kantor
                               where kantorid in (SELECT KANTORID FROM KANTOR START WITH KANTORID = :Induk
                                                  CONNECT BY NOCYCLE PRIOR KANTORID = INDUK)
                               order by kode";

                sql = sWhitespace.Replace(sql, " ");

                // Bila induk = kantor pusat, ambil sub kantor Dirjen-dirjen
                if (induk == "980FECFC746D8C80E0400B0A9214067D")
                {
                    sql = sql.Replace("where kantorid in", "where kantor.tipekantorid = 5 and kantorid in");
                }

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Induk", induk));

                object[] parameters = arrayListParameters.OfType<object>().ToArray();

                records = ctx.Database.SqlQuery<Entities.Kantor>(sql, parameters).ToList<Entities.Kantor>();
            }

            return records;
        }

        public List<Entities.Kantor> GetKantorById(string id)
        {
            List<Entities.Kantor> records = new List<Entities.Kantor>();

            ArrayList arrayListParameters = new ArrayList();

            using (var ctx = new PnbpContext())
            {
                string sql = @"select
                                   row_number() over (order by kode) as RNumber, COUNT(1) OVER() Total,
                                   kantorid, kode kodekantor, replace(replace(nama, 'Kantor Pertanahan ', ''), 'Kantor Wilayah ', '') namakantor, kota, alamat, email
                               from kantor
                               where kantorid = :Id";
                sql = sWhitespace.Replace(sql, " ");

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));

                object[] parameters = arrayListParameters.OfType<object>().ToArray();

                records = ctx.Database.SqlQuery<Entities.Kantor>(sql, parameters).ToList<Entities.Kantor>();
            }

            return records;
        }

        public List<Entities.QueryPaguAlokasiNasional> GetPaguAlokasiNasional()
        {
            List<Entities.QueryPaguAlokasiNasional> records = new List<Entities.QueryPaguAlokasiNasional>();

            ArrayList arrayListParameters = new ArrayList();

            using (var ctx = new PnbpContext())
            {
                string sql = @"select tipe, sum(nilaianggaran) pagu, sum(nilaialokasi) alokasi
                               from manfaat
                               where tahun = :Tahun
                               and statusaktif = 1
                               group by tipe";
                sql = sWhitespace.Replace(sql, " ");

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", ConfigurationManager.AppSettings["TahunAnggaran"].ToString()));

                object[] parameters = arrayListParameters.OfType<object>().ToArray();

                records = ctx.Database.SqlQuery<Entities.QueryPaguAlokasiNasional>(sql, parameters).ToList<Entities.QueryPaguAlokasiNasional>();
            }

            return records;
        }

        public List<Entities.QueryPaguAlokasiProvinsi> GetPaguAlokasiProvinsi()
        {
            List<Entities.QueryPaguAlokasiProvinsi> records = new List<Entities.QueryPaguAlokasiProvinsi>();

            ArrayList arrayListParameters = new ArrayList();

            using (var ctx = new PnbpContext())
            {
                string sql = @"select substr(ktr.kode,1,2) kode, tipe, sum(man.nilaianggaran) pagu, sum(man.nilaialokasi) alokasi
                               from manfaat man
                                    join kantor ktr on ktr.kantorid = man.kantorid
                                         and ktr.tipekantorid in (2,3,4,5)
                               where man.tahun = :Tahun
                                     and man.statusaktif = 1
                               group by substr(ktr.kode,1,2), man.tipe
                               order by substr(ktr.kode,1,2), man.tipe";
                sql = sWhitespace.Replace(sql, " ");

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", ConfigurationManager.AppSettings["TahunAnggaran"].ToString()));

                object[] parameters = arrayListParameters.OfType<object>().ToArray();

                records = ctx.Database.SqlQuery<Entities.QueryPaguAlokasiProvinsi>(sql, parameters).ToList<Entities.QueryPaguAlokasiProvinsi>();
            }

            return records;
        }

        public decimal TotalAlokasi(string pBulan, string pKantorId)
        {
            decimal result = 0;
            using (var ctx = new PnbpContext())
            {
                string sql = @"select nvl(sum(nilaialokasi),0) as jumlahalokasi from manfaatalokasi where manfaatid in (
                               select manfaatid from manfaat where tahun = :Tahun1 and kantorid = :KantorId
                               ) and statusaktif = 1 and tahun = :Tahun2 and bulan = :Bulan";
                Oracle.ManagedDataAccess.Client.OracleParameter vTahun1 = new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun1", ConfigurationManager.AppSettings["TahunAnggaran"].ToString());
                Oracle.ManagedDataAccess.Client.OracleParameter vKantorId = new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", pKantorId);
                Oracle.ManagedDataAccess.Client.OracleParameter vTahun2 = new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun2", ConfigurationManager.AppSettings["TahunAnggaran"].ToString());
                Oracle.ManagedDataAccess.Client.OracleParameter vBulan = new Oracle.ManagedDataAccess.Client.OracleParameter("Bulan", pBulan);
                object[] parameters = new object[4] { vTahun1, vKantorId, vTahun2, vBulan };
                result = ctx.Database.SqlQuery<decimal>(sql, parameters).FirstOrDefault();
            }

            return result;
        }

        public List<Entities.QueryPaguAlokasiSatker> GetPaguAlokasiSatker(string induk)
        {
            List<Entities.QueryPaguAlokasiSatker> records = new List<Entities.QueryPaguAlokasiSatker>();

            ArrayList arrayListParameters = new ArrayList();

            using (var ctx = new PnbpContext())
            {
                string sql = @"select 
                                   kantor.kode, kantor.nama NamaSatker, tipe, nvl(sum(nilaianggaran),0) pagu, nvl(sum(nilaialokasi),0) alokasi,
                                   nvl(sum(anggjan),0) anggjan, nvl(sum(alokjan),0) alokjan,
                                   nvl(sum(anggfeb),0) anggfeb, nvl(sum(alokfeb),0) alokfeb,
                                   nvl(sum(anggmar),0) anggmar, nvl(sum(alokmar),0) alokmar,
                                   nvl(sum(anggapr),0) anggapr, nvl(sum(alokapr),0) alokapr,
                                   nvl(sum(anggmei),0) anggmei, nvl(sum(alokmei),0) alokmei,
                                   nvl(sum(anggjun),0) anggjun, nvl(sum(alokjun),0) alokjun,
                                   nvl(sum(anggjul),0) anggjul, nvl(sum(alokjul),0) alokjul,
                                   nvl(sum(anggagt),0) anggagt, nvl(sum(alokagt),0) alokagt,
                                   nvl(sum(anggsep),0) anggsep, nvl(sum(aloksep),0) aloksep,
                                   nvl(sum(anggokt),0) anggokt, nvl(sum(alokokt),0) alokokt,
                                   nvl(sum(anggnov),0) anggnov, nvl(sum(aloknov),0) aloknov,
                                   nvl(sum(anggdes),0) anggdes, nvl(sum(alokdes),0) alokdes
                               from manfaat
                                    join kantor on kantor.kantorid = manfaat.kantorid
                                         and kantor.kantorid in (SELECT KANTORID FROM KANTOR START WITH KANTORID = :Induk
                                                                 CONNECT BY NOCYCLE PRIOR KANTORID = INDUK)
                               where tahun = :Tahun
                                     and statusaktif = 1
                               group by kantor.kode, kantor.nama, tipe
                               order by tipe, kantor.kode";

                // Bila induk = kantor pusat, ambil sub kantor Dirjen-dirjen
                if (induk == "980FECFC746D8C80E0400B0A9214067D")
                {
                    sql = sql.Replace("join kantor on kantor.kantorid = manfaat.kantorid", "join kantor on kantor.kantorid = manfaat.kantorid and kantor.tipekantorid = 5");
                }

                sql = sWhitespace.Replace(sql, " ");

                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Induk", induk));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", ConfigurationManager.AppSettings["TahunAnggaran"].ToString()));

                object[] parameters = arrayListParameters.OfType<object>().ToArray();

                records = ctx.Database.SqlQuery<Entities.QueryPaguAlokasiSatker>(sql, parameters).ToList<Entities.QueryPaguAlokasiSatker>();
            }

            return records;
        }


        #region Renaksi

        public string GetRenaksiSatkerId(string kantorid, string tahun)
        {
            string result = "";

            string query =
                "select renaksisatkerid " +
                "from renaksisatker where kantorid = :KantorId and tahun = :Tahun";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public Entities.TransactionResult InsertRenaksiSatker(string kantorid, string judul)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = "";
                        ArrayList arrayListParameters = new ArrayList();
                        object[] parameters = null;

                        string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault<string>();

                        sql =
                            @"INSERT INTO renaksisatker (
                                renaksisatkerid, kantorid, tahun, judul) VALUES 
                                (
                                    :Id,:KantorId,:Tahun,:Judul)";

                        sql = sWhitespace.Replace(sql, " ");

                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Id", id));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", DateTime.Now.Year));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", judul));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.ReturnValue = id;
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

        public List<Entities.GetSatkerList> GetSatker()
        {
            var ctx = new PnbpContext();
            List<Entities.GetSatkerList> records = new List<Entities.GetSatkerList>();
            var query = "SELECT * FROM SATKER WHERE STATUSAKTIF = '1'";
            return records = ctx.Database.SqlQuery<Entities.GetSatkerList>(query).ToList();
        }

        //public Entities.GetProgramList GetProgram()
        //{
        //    Entities.GetProgramList records = new Entities.GetProgramList();

        //    ArrayList arrayListParameters = new ArrayList();

        //    string query =
        //        @"SELECT * FROM PROGRAM";

        //    using (var ctx = new PnbpContext())
        //    {
        //        //object[] parameters = arrayListParameters.OfType<object>().ToArray();
        //        records = ctx.Database.SqlQuery<Entities.GetProgramList>(query).FirstOrDefault();
        //    }

        //    return records;
        //}

        public List<Entities.GetProgramList> GetProgram()
        {
            var ctx = new PnbpContext();
            //Entities.GetProgramList records = new Entities.GetProgramList();
            List<Entities.GetProgramList> records = new List<Entities.GetProgramList>();
            var query = "SELECT * FROM PROGRAM";
            return records = ctx.Database.SqlQuery<Entities.GetProgramList>(query).ToList();
            //records = ctx.Database.SqlQuery<Entities.RincianAlokasiTotal>(query, parameters).FirstOrDefault();
        }

        public int GetTipeKantor(string kantorid)
        {
            int result = 0;

            string query = "SELECT tipekantorid FROM kantor WHERE kantorid = :KantorId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public List<Entities.RincianAlokasiList> GetRincianAlokasiSatker(string pTipeKantorId, string pKantorId, string idsatker, string tahun, int from, int to)
        {
            List<Entities.RincianAlokasiList> records = new List<Entities.RincianAlokasiList>();

            ArrayList arrayListParameters = new ArrayList();

            if (string.IsNullOrEmpty(tahun))
            {
                tahun = DateTime.Now.Year.ToString();
            }

            string query =
                $@"
                  SELECT
                        row_number() over (ORDER BY C.KDSATKER ASC) AS RNumber,
	                    C.KDSATKER AS KODESATKER,
	                    B.KANTORID,
	                    B.NAMAKANTOR AS NAMASATKER,
	                    B.TAHUN,
	                    A.PAGU,
	                    B.OPS,
	                    B.NONOPS,
	                    B.ALOKASI,
	                    C.REALISASIBELANJA,
                        ROUND((C.REALISASIBELANJA/A.PAGU)*100, 2) AS PERSENPAGU,
	                    ROUND((C.REALISASIBELANJA/B.ALOKASI)*100, 2) AS PERSENALOKASI
                    FROM
                    ( 
	                    SELECT
		                    KDSATKER,
		                    SUM( AMOUNT ) AS PAGU 
	                    FROM
		                    SPAN_BELANJA 
	                    WHERE
		                    SUMBER_DANA = 'D' 
		                    AND KDSATKER <> '524465' 
                            AND TAHUN = {tahun}
	                    GROUP BY
		                    KDSATKER 
	                    ) A
                    LEFT JOIN (
	                    SELECT
		                    M.KODESATKER,
		                    M.TAHUN,
		                    M.NAMAKANTOR,
		                    M.KANTORID,
		                    SUM( TOTALALOKASI ) AS OPS,
		                    SUM( ANGGJAN + ANGGFEB + ANGGMAR) AS NONOPS,
		                    SUM( ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT) + NVL(SUM( MA.NILAIALOKASI ),0) AS ALOKASI 
	                    FROM
		                    MANFAAT M
                            LEFT JOIN MANFAATALOKASI MA ON MA.MANFAATID = M.MANFAATID AND MA.STATUSEDIT = 0
	                    GROUP BY
		                    M.KODESATKER,
		                    M.KANTORID,
		                    M.TAHUN,
		                    M.NAMAKANTOR
	                    ) B ON A.KDSATKER = B.KODESATKER
                    LEFT JOIN (
	                    SELECT
		                    KDSATKER,
		                    SUM( AMOUNT ) AS REALISASIBELANJA 
	                    FROM
		                    SPAN_REALISASI 
	                    WHERE
		                    SUMBERDANA = 'D' 
		                    AND KDSATKER <> '524465' 
                            AND TAHUN = {tahun}
	                    GROUP BY
		                    KDSATKER 
	                    ) C ON B.KODESATKER = C.KDSATKER
	                    WHERE B.TAHUN = ( SELECT (to_char( SYSDATE, 'YYYY' )) AS Y FROM dual )";
            query += @" AND TO_CHAR(B.KANTORID) LIKE '%" + idsatker + "%' AND TO_CHAR(B.TAHUN) LIKE '%" + tahun + "%'";
            //WHERE RNumber BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.RincianAlokasiList>(query, parameters).ToList();
            }

            return records;
        }

        //public List<Entities.RincianAlokasiList> GetRincianAlokasiSatker(string pTipeKantorId, string pKantorId, string idsatker, string tahun, int from, int to)
        //{
        //    List<Entities.RincianAlokasiList> records = new List<Entities.RincianAlokasiList>();

        //    ArrayList arrayListParameters = new ArrayList();

        //    string query =
        //        @"
        //          SELECT
        //                row_number () over ( ORDER BY KODESATKER ASC ) AS RNumber,
        //                KODESATKER,
        //                KANTORID,
        //                NAMASATKER,
        //                REALISASIBELANJA,
        //                PAGU,
        //                TAHUN,
        //                ALOKASI,
        //                ROUND(((REALISASIBELANJA / PAGU) * 100), 2) AS PERSENPAGU,
        //                ROUND(NVL(((NULLIF(REALISASIBELANJA, 0) / NULLIF(ALOKASI, 0)) * 100), 0), 2) AS PERSENALOKASI
        //          FROM
        //                ( 
        //                SELECT
        //                 A.KODESATKER,
        //                 A.KANTORID,
        //                 A.NAMAKANTOR AS NAMASATKER,
        //                 A.TAHUN,
        //                 SUM( A.OPERASIONAL ) REALISASIBELANJA,
        //                 SUM( B.NILAIANGGARAN ) PAGU,
        //                 SUM( B.TOTALALOKASI ) ALOKASI 
        //                 FROM
        //                  REKAPPENERIMAANDETAIL A
        //                  LEFT JOIN MANFAAT B ON A.KANTORID = B.KANTORID 
        //                  AND A.TAHUN = B.TAHUN 
        //                 GROUP BY
        //                  A.KODESATKER,
        //                  A.NAMAKANTOR,
        //                  A.KANTORID,
        //                  A.TAHUN
        //                ) A
        //                WHERE A.TAHUN = ( SELECT (to_char( SYSDATE, 'YYYY' )) AS Y FROM dual )";
        //                //WHERE RNumber BETWEEN :startCnt AND :limitCnt";

        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

        //    using (var ctx = new PnbpContext())
        //    {
        //        object[] parameters = arrayListParameters.OfType<object>().ToArray();
        //        records = ctx.Database.SqlQuery<Entities.RincianAlokasiList>(query, parameters).ToList();
        //    }

        //    return records;
        //}

        //public List<Entities.RincianAlokasiList> GetRincianAlokasiSatker1(string pTipeKantorId, string pKantorId, string idsatker, string tahun, int from, int to)
        //{
        //    List<Entities.RincianAlokasiList> records = new List<Entities.RincianAlokasiList>();

        //    ArrayList arrayListParameters = new ArrayList();

        //    string query =
        //        @"SELECT * FROM (SELECT DISTINCT    
        //          row_number () over ( ORDER BY 
        //		A.KODESATKER,
        //		A.KANTORID,
        //		A.NAMAKANTOR
        //          ) AS RNumber,
        //    A.KODESATKER,
        //    A.KANTORID,
        //    A.NAMAKANTOR NAMASATKER,
        //(SELECT DISTINCT SUM(NILAIANGGARAN) FROM MANFAAT WHERE KANTORID = A.KANTORID) AS PAGU,
        //      (SELECT DISTINCT SUM(OPERASIONAL) FROM REKAPPENERIMAANDETAIL WHERE KANTORID = A.KANTORID) AS REALISASIBELANJA,
        //NVL((SELECT DISTINCT SUM(TOTALALOKASI) FROM MANFAAT WHERE KANTORID = A.KANTORID), 0) AS ALOKASI,
        //ROUND(NVL(ROUND(SUM(NULLIF(a.OPERASIONAL,0))/SUM(NULLIF(b.NILAIANGGARAN,0)),5)*100,0),0) PERSENPAGU,
        //ROUND(NVL(ROUND(SUM(NULLIF(a.OPERASIONAL,0))/SUM(NULLIF(b.TOTALALOKASI,0)),5)*100,0),0) PERSENALOOKASI

        //   FROM
        //    REKAPPENERIMAANDETAIL A
        //   LEFT JOIN MANFAAT b ON A .KANTORID = b.KANTORID";
        //    query += " WHERE ROWNUM < 100000 AND A.TAHUN = ( SELECT ( to_char( SYSDATE, 'YYYY' )) AS Y FROM dual )";
        //    if (pTipeKantorId != "1")
        //    {
        //        if (!string.IsNullOrEmpty(pTipeKantorId))
        //        {
        //            query += " AND A.KANTORID = '" + pKantorId + "'";
        //        }
        //    }
        //    query += @" AND TO_CHAR(A.KANTORID) LIKE '%" + idsatker + "%' AND TO_CHAR(A.TAHUN) LIKE '%" + tahun + "%'";
        //    query += @"GROUP BY
        //A .KODESATKER,
        //A .NAMAKANTOR,
        //A.KANTORID) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

        //    using (var ctx = new PnbpContext())
        //    {
        //        object[] parameters = arrayListParameters.OfType<object>().ToArray();
        //        records = ctx.Database.SqlQuery<Entities.RincianAlokasiList>(query, parameters).ToList();
        //    }

        //    return records;
        //}

        public Entities.RincianAlokasiTotal GetRincianAlokasiSatkerTotal(string pTipeKantorId, string pKantorId, string idsatker, string tahun, int from, int to)
        {
            Entities.RincianAlokasiTotal records = new Entities.RincianAlokasiTotal();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT 
                NVL(SUM(A.OPERASIONAL),0) TOTAL_REALISASIBELANJA,
                NVL(SUM(B.TOTALALOKASI),0) TOTAL_ALOKASI,
                NVL(SUM(B.NILAIANGGARAN),0) TOTAL_PAGU
                FROM REKAPPENERIMAANDETAIL A 
                LEFT JOIN MANFAAT B ON A.KANTORID = B.KANTORID
                WHERE ROWNUM < 100000";
            //if (pTipeKantorId != "1")
            //{
            //    if (!string.IsNullOrEmpty(pTipeKantorId))
            //    {
            //        query += " AND A.KANTORID = '" + pKantorId + "'";
            //    }

            //}
            query += @" AND TO_CHAR(A.KANTORID) LIKE '%" + idsatker + "%' AND TO_CHAR(A.TAHUN) LIKE '%" + tahun + "%'";
            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.RincianAlokasiTotal>(query, parameters).FirstOrDefault();
            }

            return records;
        }

        public List<Entities.RincianAlokasiListDetail> GetRincianAlokasiSatkerOutput(string programid, string pTipeKantorId, string pKantorId, string tahun, int from, int to)
        {
            List<Entities.RincianAlokasiListDetail> records = new List<Entities.RincianAlokasiListDetail>();

            ArrayList arrayListParameters = new ArrayList();

            if (string.IsNullOrEmpty(tahun))
            {
                tahun = DateTime.Now.Year.ToString();
            }

            string query =
                $@"SELECT
	                    row_number () over ( ORDER BY C.KODESATKER ASC ) AS RNumber,
	                    C.KODESATKER,
	                    C.KANTORID,
	                    C.KODEOUTPUT,
	                    C.PROGRAMID,
	                    C.NAMAPROGRAM AS NAMAOUTPUT,
	                    C.TIPE,
	                    C.TAHUN,
	                    C.TOTALALOKASI,
	                    D.SUMBERDANA,
	                    D.REALISASIBELANJA,
	                    D.PAGU,
                        ROUND((D.REALISASIBELANJA/D.PAGU)*100, 2) AS PERSENPAGU
                    FROM
	                    (
	                    SELECT
		                    KODESATKER,
		                    KANTORID,
		                    KODE KODEOUTPUT,
		                    PROGRAMID,
		                    NAMAPROGRAM,
		                    TIPE,
		                    TAHUN,
		                    SUM( TOTALALOKASI ) TOTALALOKASI 
	                    FROM
		                    MANFAAT 
	                    GROUP BY
		                    KODESATKER,
		                    KANTORID,
		                    KODE,
		                    PROGRAMID,
		                    NAMAPROGRAM,
		                    TIPE,
		                    TAHUN 
	                    ) C
	                    LEFT JOIN (
	                    SELECT
		                    A.KDSATKER,
		                    A.SUMBERDANA,
		                    TO_CHAR( TO_DATE( A.TANGGAL, 'DD-MM-YY' ), 'YYYY' ) TAHUN,
		                    SUM( A.AMOUNT ) REALISASIBELANJA,
		                    SUM( B.AMOUNT ) PAGU 
	                    FROM
		                    SPAN_REALISASI A
		                    LEFT JOIN SPAN_BELANJA B ON A.KDSATKER = B.KDSATKER
		                WHERE A.TAHUN = {tahun} AND B.TAHUN = {tahun}
	                    GROUP BY
		                    A.KDSATKER,
		                    B.KDSATKER,
		                    A.SUMBERDANA,
		                    TO_CHAR( TO_DATE( A.TANGGAL, 'DD-MM-YY' ), 'YYYY' ) 
	                    ) D ON C.KODESATKER = D.KDSATKER 
                    WHERE
	                    D.SUMBERDANA = 'D' 
	                    AND C.KODESATKER <> '524465' 
	                    AND C.TAHUN = ( SELECT ( to_char( SYSDATE, 'YYYY' )) AS Y FROM dual ) 
	                    AND ROWNUM <= 4500";

            query = sWhitespace.Replace(query, " ");

            //if (!String.IsNullOrEmpty(kodesatker))
            //{
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeSatker", String.Concat("%", kodesatker.ToLower(), "%")));
            //    //query += " AND LOWER(KODESATKER) LIKE :KodeSatker ";
            //}

            //if (!String.IsNullOrEmpty(tahun))
            //{
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", String.Concat("%", tahun.ToLower(), "%")));
            //    //query += " AND LOWER(TAHUN) LIKE :Tahun ";
            //}

            //query +=
            //    " ) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.RincianAlokasiListDetail>(query, parameters).ToList();
            }

            return records;
        }


        //public List<Entities.RincianAlokasiListDetail> GetRincianAlokasiSatkerOutput(string pTipeKantorId, string pKantorId, string tahun, int from, int to)
        //{
        //    List<Entities.RincianAlokasiListDetail> records = new List<Entities.RincianAlokasiListDetail>();

        //    ArrayList arrayListParameters = new ArrayList();

        //    string query =
        //        @"SELECT
        //         row_number () over ( ORDER BY PROGRAMID ASC ) AS RNUMBER,
        //         KODEOUTPUT,
        //         NAMAOUTPUT,
        //         PROGRAMID,
        //         TIPE,
        //         TAHUN,
        //         ALOKASI,
        //         PAGU,
        //         REALISASIBELANJA,
        //         ROUND(NVL(((NULLIF(REALISASIBELANJA, 0) / NULLIF(ALOKASI, 0)) * 100), 0), 2) AS PERSENALOKASI,
        //         ROUND(((REALISASIBELANJA / PAGU) * 100), 2) AS PERSENPAGU
        //         FROM
        //        (
        //        SELECT
        //         A.KODEPENERIMAAN AS KODEOUTPUT,
        //         B.NAMAPROGRAM AS NAMAOUTPUT,
        //         B.PROGRAMID,
        //         B.TIPE,
        //         A.TAHUN,
        //         SUM( B.TOTALALOKASI ) ALOKASI,
        //         SUM( B.NILAIANGGARAN ) PAGU,
        //         SUM( A.OPERASIONAL ) REALISASIBELANJA
        //        FROM
        //         REKAPPENERIMAANDETAIL A
        //         LEFT JOIN MANFAAT B ON A.KANTORID = B.KANTORID AND A.TAHUN = B.TAHUN
        //         GROUP BY 
        //         A.KODEPENERIMAAN,
        //         B.NAMAPROGRAM,
        //         B.PROGRAMID,
        //         B.TIPE,
        //         A.TAHUN
        //         ) A
        //         WHERE A.TAHUN = ( SELECT (to_char( SYSDATE, 'YYYY' )) AS Y FROM dual )";

        //    query = sWhitespace.Replace(query, " ");

        //    //if (!String.IsNullOrEmpty(kodesatker))
        //    //{
        //    //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeSatker", String.Concat("%", kodesatker.ToLower(), "%")));
        //    //    //query += " AND LOWER(KODESATKER) LIKE :KodeSatker ";
        //    //}

        //    //if (!String.IsNullOrEmpty(tahun))
        //    //{
        //    //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", String.Concat("%", tahun.ToLower(), "%")));
        //    //    //query += " AND LOWER(TAHUN) LIKE :Tahun ";
        //    //}

        //    //query +=
        //    //    " ) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

        //    using (var ctx = new PnbpContext())
        //    {
        //        object[] parameters = arrayListParameters.OfType<object>().ToArray();
        //        records = ctx.Database.SqlQuery<Entities.RincianAlokasiListDetail>(query, parameters).ToList();
        //    }

        //    return records;
        //}

        //original
        //public List<Entities.RincianAlokasiListDetail> GetRincianAlokasiSatkerOutput1(string pTipeKantorId, string pKantorId, string tahun, int from, int to)
        //{
        //    List<Entities.RincianAlokasiListDetail> records = new List<Entities.RincianAlokasiListDetail>();

        //    ArrayList arrayListParameters = new ArrayList();

        //    string query =
        //        @"SELECT DISTINCT * FROM
        //        (SELECT 
        //        row_number () over ( ORDER BY 
        //        REKAPPENERIMAANDETAIL.KANTORID,
        //        REKAPPENERIMAANDETAIL.KODEPENERIMAAN,

        //        MANFAAT.PROGRAMID,
        //        MANFAAT.NAMAPROGRAM,
        //        MANFAAT.TIPE

        //        ) AS RNUMBER,
        //        TO_CHAR(REKAPPENERIMAANDETAIL.KANTORID) KANTORID,
        //        TO_CHAR(REKAPPENERIMAANDETAIL.KODEPENERIMAAN) KODEOUTPUT,
        //        NVL(SUM(NULLIF(MANFAAT.TOTALALOKASI,0)),0) ALOKASI,
        //        NVL(SUM(NULLIF(MANFAAT.NILAIANGGARAN,0)),0) PAGU,
        //        TO_CHAR(MANFAAT.NAMAPROGRAM) NAMAOUTPUT,
        //        TO_CHAR(MANFAAT.PROGRAMID) PROGRAMID,
        //        TO_CHAR(MANFAAT.TIPE) TIPE,
        //        TO_CHAR(REKAPPENERIMAANDETAIL.TAHUN) TAHUN,
        //        NVL(SUM(NULLIF(REKAPPENERIMAANDETAIL.OPERASIONAL,0)),0) REALISASIBELANJA,
        //        ROUND(NVL(ROUND(SUM(NULLIF(REKAPPENERIMAANDETAIL.OPERASIONAL,0))/SUM(NULLIF(MANFAAT.TOTALALOKASI,0)),5)*100,0),0) PERSENALOOKASI,
        //        ROUND(NVL(ROUND(SUM(NULLIF(REKAPPENERIMAANDETAIL.OPERASIONAL,0))/SUM(NULLIF(MANFAAT.NILAIANGGARAN,0)),5)*100,0),0) PERSENPAGU,
        //        COUNT(1) OVER() Total
        //        FROM REKAPPENERIMAANDETAIL
        //        JOIN MANFAAT ON REKAPPENERIMAANDETAIL.KANTORID = MANFAAT.KANTORID
        //        WHERE REKAPPENERIMAANDETAIL.TAHUN = ( SELECT ( to_char( SYSDATE, 'YYYY' )) AS Y FROM dual ) ";
        //    if (pTipeKantorId != "1")
        //    {
        //        if (!string.IsNullOrEmpty(pTipeKantorId))
        //        {
        //            query += " AND REKAPPENERIMAANDETAIL.KANTORID = '" + pKantorId + "'";
        //        }
        //    }
        //    query += @"GROUP BY
        //        REKAPPENERIMAANDETAIL.KANTORID,
        //        REKAPPENERIMAANDETAIL.KODEPENERIMAAN,
        //        REKAPPENERIMAANDETAIL.TAHUN,
        //        MANFAAT.PROGRAMID,
        //        MANFAAT.NAMAPROGRAM,
        //        MANFAAT.TIPE


        //        ) WHERE RNumber BETWEEN :startCnt AND :limitCnt
        //        AND LOWER(TAHUN) LIKE '%" + tahun + "%' ORDER BY RNumber ASC";



        //    query = sWhitespace.Replace(query, " ");


        //    //if (!String.IsNullOrEmpty(kodesatker))
        //    //{
        //    //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeSatker", String.Concat("%", kodesatker.ToLower(), "%")));
        //    //    //query += " AND LOWER(KODESATKER) LIKE :KodeSatker ";
        //    //}

        //    //if (!String.IsNullOrEmpty(tahun))
        //    //{
        //    //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", String.Concat("%", tahun.ToLower(), "%")));
        //    //    //query += " AND LOWER(TAHUN) LIKE :Tahun ";
        //    //}

        //    //query +=
        //    //    " ) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

        //    using (var ctx = new PnbpContext())
        //    {
        //        object[] parameters = arrayListParameters.OfType<object>().ToArray();
        //        records = ctx.Database.SqlQuery<Entities.RincianAlokasiListDetail>(query, parameters).ToList();
        //    }

        //    return records;
        //}

        public List<Entities.RincianAlokasiListDetail> GetRincianAlokasiSatkerOutputById(string kantorid, int from, int to)
        {
            List<Entities.RincianAlokasiListDetail> records = new List<Entities.RincianAlokasiListDetail>();

            ArrayList arrayListParameters = new ArrayList();

            string currentYear = DateTime.Now.Year.ToString();
            string query =
                $@"SELECT
                        row_number () over ( ORDER BY A.KODE ASC ) AS RNumber,
	                    A.MANFAATID,
	                    A.KANTORID,
	                    A.KODESATKER,
	                    A.KODE AS KODEOUTPUT,
	                    A.NAMAPROGRAM AS NAMAOUTPUT,
	                    A.TIPE,
	                    A.PAGU,
	                    A.ALOKASI,
	                    NVL(B.REALISASIBELANJA, 0) AS REALISASIBELANJA,
                        NVL(ROUND((NULLIF(B.REALISASIBELANJA, 0)/NULLIF(A.ALOKASI, 0))*100, 2),0) AS PERSENALOKASI
                    FROM
                    (
                    SELECT
	                    M.MANFAATID,
	                    M.KANTORID,
	                    M.KODESATKER,
	                    M.KODE,
	                    M.NAMAPROGRAM,
	                    M.TIPE,
	                    SUM(M.NILAIANGGARAN) AS PAGU,
                        (
		                CASE
				                WHEN M.TIPE = 'OPS' THEN
				                (SELECT SUM(NILAIALOKASI) FROM MANFAATALOKASI MA WHERE MA.MANFAATID = M.MANFAATID AND STATUSEDIT = 0)
				                WHEN M.TIPE = 'NONOPS' THEN
				                SUM( M.ANGGJAN + M.ANGGFEB + M.ANGGMAR + M.ANGGAPR + M.ANGGMEI + M.ANGGJUN + M.ANGGJUL + M.ANGGAGT) 
			                END 
			                ) AS ALOKASI
	                    
                    FROM
	                    MANFAAT M
	                    
                    WHERE
                     M.TAHUN = 2021
                   
                    GROUP BY
                    M.MANFAATID,
                    M.KANTORID,
                    M.KODESATKER,
                    M.KODE,
                    M.NAMAPROGRAM,
                    M.TIPE
                    ) A
                    LEFT JOIN
                    (
                    SELECT
	                    KDSATKER,
	                    KEGIATAN,
	                    OUTPUT,
	                    CONCAT(KEGIATAN, CONCAT('.', OUTPUT)) KODECONCAT,
	                    SUM( AMOUNT ) AS REALISASIBELANJA
                    FROM
	                    SPAN_REALISASI
                    WHERE
						SUMBERDANA = 'D'
						AND KDSATKER <> '524465'
                        AND TAHUN = {currentYear} 
                    GROUP BY
	                    KDSATKER,
	                    KEGIATAN,
	                    OUTPUT
                    )B ON A.KODESATKER = B.KDSATKER AND A.KODE = B.KODECONCAT
                    WHERE A.KANTORID = '" + kantorid + "'";
            query = sWhitespace.Replace(query, " ");


            //if (!String.IsNullOrEmpty(kantorid))
            //{
            //    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", String.Concat("%", kantorid.ToLower(),"%")));
            //query += " AND KANTORID = '"+kantorid+"'";
            //}

            //if (!String.IsNullOrEmpty(tahun))
            //{
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", String.Concat("%", tahun.ToLower(), "%")));
            //    //query += " AND LOWER(TAHUN) LIKE :Tahun ";
            //}

            //query +=
            //    " ) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.RincianAlokasiListDetail>(query, parameters).ToList();
            }

            return records;
        }

        //public List<Entities.RincianAlokasiListDetail> GetRincianAlokasiSatkerOutputById(string kantorid, int from, int to)
        //{
        //    List<Entities.RincianAlokasiListDetail> records = new List<Entities.RincianAlokasiListDetail>();

        //    ArrayList arrayListParameters = new ArrayList();

        //    string query =
        //        @"SELECT
        //         row_number () over ( ORDER BY PROGRAMID ASC ) AS RNUMBER,
        //            KANTORID,
        //         KODEOUTPUT,
        //         NAMAOUTPUT,
        //         PROGRAMID,
        //         TIPE,
        //         TAHUN,
        //         ALOKASI,
        //         PAGU,
        //         REALISASIBELANJA,
        //         ROUND(NVL(((NULLIF(REALISASIBELANJA, 0) / NULLIF(ALOKASI, 0)) * 100), 0), 2) AS PERSENALOKASI,
        //         ROUND(((REALISASIBELANJA / PAGU) * 100), 2) AS PERSENPAGU
        //         FROM
        //        (
        //        SELECT
        //            A.KANTORID,
        //         A.KODEPENERIMAAN AS KODEOUTPUT,
        //         B.NAMAPROGRAM AS NAMAOUTPUT,
        //         B.PROGRAMID,
        //         B.TIPE,
        //         A.TAHUN,
        //         SUM( B.TOTALALOKASI ) ALOKASI,
        //         SUM( B.NILAIANGGARAN ) PAGU,
        //         SUM( A.OPERASIONAL ) REALISASIBELANJA
        //        FROM
        //         REKAPPENERIMAANDETAIL A
        //         LEFT JOIN MANFAAT B ON A.KANTORID = B.KANTORID AND A.TAHUN = B.TAHUN
        //         GROUP BY 
        //            A.KANTORID,
        //         A.KODEPENERIMAAN,
        //         B.NAMAPROGRAM,
        //         B.PROGRAMID,
        //         B.TIPE,
        //         A.TAHUN
        //         ) A
        //         WHERE A.TAHUN = ( SELECT (to_char( SYSDATE, 'YYYY' )) AS Y FROM dual ) AND A.KANTORID = '" + kantorid + "'";
        //    query = sWhitespace.Replace(query, " ");


        //    //if (!String.IsNullOrEmpty(kantorid))
        //    //{
        //    //    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", String.Concat("%", kantorid.ToLower(),"%")));
        //    //query += " AND KANTORID = '"+kantorid+"'";
        //    //}

        //    //if (!String.IsNullOrEmpty(tahun))
        //    //{
        //    //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", String.Concat("%", tahun.ToLower(), "%")));
        //    //    //query += " AND LOWER(TAHUN) LIKE :Tahun ";
        //    //}

        //    //query +=
        //    //    " ) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

        //    using (var ctx = new PnbpContext())
        //    {
        //        object[] parameters = arrayListParameters.OfType<object>().ToArray();
        //        records = ctx.Database.SqlQuery<Entities.RincianAlokasiListDetail>(query, parameters).ToList();
        //    }

        //    return records;
        //}

        //public List<Entities.RincianAlokasiListDetail> GetRincianAlokasiSatkerOutputById(string kantorid, int from, int to)
        //{
        //    List<Entities.RincianAlokasiListDetail> records = new List<Entities.RincianAlokasiListDetail>();

        //    ArrayList arrayListParameters = new ArrayList();

        //    string query =
        //        @"SELECT * FROM
        //        (SELECT 
        //        row_number () over ( ORDER BY 
        //        REKAPPENERIMAANDETAIL.KANTORID,
        //        REKAPPENERIMAANDETAIL.KODEPENERIMAAN,

        //        MANFAAT.NAMAPROGRAM,
        //        MANFAAT.TIPE

        //        ) AS RNUMBER,
        //        TO_CHAR(REKAPPENERIMAANDETAIL.KANTORID) KANTORID,
        //        TO_CHAR(REKAPPENERIMAANDETAIL.KODEPENERIMAAN) KODEOUTPUT,
        //        NVL(SUM(NULLIF(MANFAAT.TOTALALOKASI,0)),0) ALOKASI,
        //        NVL(SUM(NULLIF(MANFAAT.NILAIANGGARAN,0)),0) PAGU,
        //        TO_CHAR(MANFAAT.NAMAPROGRAM) NAMAOUTPUT,
        //        TO_CHAR(MANFAAT.TIPE) TIPE,
        //        TO_CHAR(REKAPPENERIMAANDETAIL.TAHUN) TAHUN,
        //        NVL(SUM(NULLIF(REKAPPENERIMAANDETAIL.OPERASIONAL,0)),0) REALISASIBELANJA,
        //        ROUND(NVL(ROUND(SUM(NULLIF(REKAPPENERIMAANDETAIL.OPERASIONAL,0))/SUM(NULLIF(MANFAAT.TOTALALOKASI,0)),5)*100,0),0) PERSENALOOKASI,
        //        ROUND(NVL(ROUND(SUM(NULLIF(REKAPPENERIMAANDETAIL.OPERASIONAL,0))/SUM(NULLIF(MANFAAT.NILAIANGGARAN,0)),5)*100,0),0) PERSENPAGU,
        //        COUNT(1) OVER() Total
        //        FROM PNBPTRAIN.REKAPPENERIMAANDETAIL
        //        JOIN PNBPTRAIN.MANFAAT ON REKAPPENERIMAANDETAIL.KANTORID = MANFAAT.KANTORID
        //        WHERE TO_CHAR(REKAPPENERIMAANDETAIL.KANTORID) LIKE '%" + kantorid + "%'";
        //    query += @"GROUP BY
        //        REKAPPENERIMAANDETAIL.KANTORID,
        //        REKAPPENERIMAANDETAIL.KODEPENERIMAAN,

        //        REKAPPENERIMAANDETAIL.TAHUN,
        //        MANFAAT.NAMAPROGRAM,
        //        MANFAAT.TIPE

        //        ) WHERE RNumber BETWEEN :startCnt AND :limitCnt ";



        //    query = sWhitespace.Replace(query, " ");


        //    //if (!String.IsNullOrEmpty(kantorid))
        //    //{
        //    //    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", String.Concat("%", kantorid.ToLower(),"%")));
        //    //query += " AND KANTORID = '"+kantorid+"'";
        //    //}

        //    //if (!String.IsNullOrEmpty(tahun))
        //    //{
        //    //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", String.Concat("%", tahun.ToLower(), "%")));
        //    //    //query += " AND LOWER(TAHUN) LIKE :Tahun ";
        //    //}

        //    //query +=
        //    //    " ) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

        //    using (var ctx = new PnbpContext())
        //    {
        //        object[] parameters = arrayListParameters.OfType<object>().ToArray();
        //        records = ctx.Database.SqlQuery<Entities.RincianAlokasiListDetail>(query, parameters).ToList();
        //    }

        //    return records;
        //}

        //List data satker level 2 dari rincian alokasi output
        public List<Entities.RincianAlokasiListDetail> GetSatkerOutputById(string kantorid, int from, int to)
        {
            List<Entities.RincianAlokasiListDetail> records = new List<Entities.RincianAlokasiListDetail>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT * FROM
                (SELECT 
                row_number () over ( ORDER BY 
                REKAPPENERIMAANDETAIL.KANTORID,
                REKAPPENERIMAANDETAIL.KODEPENERIMAAN,
                
                MANFAAT.NAMAPROGRAM,
                MANFAAT.TIPE
                
                ) AS RNUMBER,
                TO_CHAR(REKAPPENERIMAANDETAIL.KANTORID) KANTORID,
                TO_CHAR(REKAPPENERIMAANDETAIL.KODEPENERIMAAN) KODEOUTPUT,
                NVL(SUM(NULLIF(MANFAAT.TOTALALOKASI,0)),0) ALOKASI,
                NVL(SUM(NULLIF(MANFAAT.NILAIANGGARAN,0)),0) PAGU,
                TO_CHAR(MANFAAT.NAMAPROGRAM) NAMAOUTPUT,
                TO_CHAR(MANFAAT.TIPE) TIPE,
                TO_CHAR(REKAPPENERIMAANDETAIL.TAHUN) TAHUN,
                NVL(SUM(NULLIF(REKAPPENERIMAANDETAIL.OPERASIONAL,0)),0) REALISASIBELANJA,
                ROUND(NVL(ROUND(SUM(NULLIF(REKAPPENERIMAANDETAIL.OPERASIONAL,0))/SUM(NULLIF(MANFAAT.TOTALALOKASI,0)),5)*100,0),0) PERSENALOOKASI,
                ROUND(NVL(ROUND(SUM(NULLIF(REKAPPENERIMAANDETAIL.OPERASIONAL,0))/SUM(NULLIF(MANFAAT.NILAIANGGARAN,0)),5)*100,0),0) PERSENPAGU,
                COUNT(1) OVER() Total
                FROM PNBPTRAIN.REKAPPENERIMAANDETAIL
                JOIN PNBPTRAIN.MANFAAT ON REKAPPENERIMAANDETAIL.KANTORID = MANFAAT.KANTORID
                WHERE TO_CHAR(REKAPPENERIMAANDETAIL.KANTORID) LIKE '%" + kantorid + "%'";
            query += @"GROUP BY
                REKAPPENERIMAANDETAIL.KANTORID,
                REKAPPENERIMAANDETAIL.KODEPENERIMAAN,
                
                REKAPPENERIMAANDETAIL.TAHUN,
                MANFAAT.NAMAPROGRAM,
                MANFAAT.TIPE
                
                ) WHERE RNumber BETWEEN :startCnt AND :limitCnt ";



            query = sWhitespace.Replace(query, " ");


            //if (!String.IsNullOrEmpty(kantorid))
            //{
            //    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", String.Concat("%", kantorid.ToLower(),"%")));
            //query += " AND KANTORID = '"+kantorid+"'";
            //}

            //if (!String.IsNullOrEmpty(tahun))
            //{
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", String.Concat("%", tahun.ToLower(), "%")));
            //    //query += " AND LOWER(TAHUN) LIKE :Tahun ";
            //}

            //query +=
            //    " ) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.RincianAlokasiListDetail>(query, parameters).ToList();
            }

            return records;
        }

        //List data satker level 2 dari rincian alokasi output


        public Entities.satker GetSatkerByKantorId(string kantorid)
        {
            string KantorId = kantorid;
            Entities.satker records = new Entities.satker();
            ArrayList arrayListParameters = new ArrayList();
            string query = @"SELECT KANTORID,KODESATKER Kode,NAMA_SATKER AS Nama,NAMAALIAS FROM SATKER WHERE KANTORID = '" + KantorId + "'";
            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.satker>(query).FirstOrDefault();
            }

            return records;
        }

        public Entities.program GetLayananById(string programid)
        {
            string ProgramId = programid;
            Entities.program records = new Entities.program();
            ArrayList arrayListParameters = new ArrayList();
            string query = @"SELECT PROGRAMID,NAMAPROGRAM as Nama FROM MANFAAT WHERE PROGRAMID = '" + ProgramId + "'";
            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.program>(query).FirstOrDefault();
            }

            return records;
        }

        public List<Entities.RincianAlokasiListDetail> GetRincianAlokasiDetailByProgramId(string pTipeKantorId, string pKantorId, string programid, int from, int to)
        {
            List<Entities.RincianAlokasiListDetail> records = new List<Entities.RincianAlokasiListDetail>();

            ArrayList arrayListParameters = new ArrayList();

            string currentYear = DateTime.Now.Year.ToString();
            string query =
                $@"SELECT
	                    row_number () over ( ORDER BY C.KODESATKER ASC ) AS RNumber,
	                    C.KODESATKER,
	                    C.KANTORID,
	                    C.NAMAKANTOR AS NAMASATKER,
	                    C.TAHUN,
	                    C.TOTALALOKASI,
	                    D.SUMBERDANA,
	                    D.REALISASIBELANJA,
	                    D.PAGU,
	                    C.PROGRAMID,
                        ROUND((D.REALISASIBELANJA/D.PAGU)*100, 2) AS PERSENPAGU 
                    FROM
	                    (
	                    SELECT
		                    KODESATKER,
		                    KANTORID,
		                    NAMAKANTOR,
		                    PROGRAMID,
		                    TAHUN,
		                    SUM( TOTALALOKASI ) TOTALALOKASI,
		                    SUM( ANGGJAN ) ANGGJAN,
		                    SUM( ANGGFEB ) ANGGFEB,
		                    SUM( ANGGMAR ) ANGGMAR,
		                    SUM( ANGGAPR ) ANGGAPR,
		                    SUM( ANGGMEI ) ANGGMEI,
		                    SUM( ANGGJUN ) ANGGJUN,
		                    SUM( ANGGJUL ) ANGGJUL,
		                    SUM( ANGGAGT ) ANGGAGT,
		                    SUM( ANGGSEP ) ANGGSEP,
		                    SUM( ANGGOKT ) ANGGOKT,
		                    SUM( ANGGNOV ) ANGGNOV,
		                    SUM( ANGGDES ) ANGGDES 
	                    FROM
		                    MANFAAT 
	                    GROUP BY
		                    KODESATKER,
		                    KANTORID,
		                    NAMAKANTOR,
		                    PROGRAMID,
		                    TAHUN 
	                    ) C
	                    LEFT JOIN (
	                    SELECT
		                    A.KDSATKER,
		                    A.SUMBERDANA,
		                    TO_CHAR( TO_DATE( A.TANGGAL, 'DD-MM-YY' ), 'YYYY' ) TAHUN,
		                    SUM( A.AMOUNT ) REALISASIBELANJA,
		                    SUM( B.AMOUNT ) PAGU 
	                    FROM
		                    SPAN_REALISASI A
		                    LEFT JOIN SPAN_BELANJA B ON A.KDSATKER = B.KDSATKER 
                        WHERE A.TAHUN = {currentYear} AND B.TAHUN = {currentYear}  
	                    GROUP BY
		                    A.KDSATKER,
		                    B.KDSATKER,
		                    A.SUMBERDANA,
		                    TO_CHAR( TO_DATE( A.TANGGAL, 'DD-MM-YY' ), 'YYYY' ) 
	                    ) D ON C.KODESATKER = D.KDSATKER 
                    WHERE
	                    D.SUMBERDANA = 'D' 
	                    AND C.KODESATKER <> '524465' 
	                    AND C.TAHUN = ( SELECT ( to_char( SYSDATE, 'YYYY' )) AS Y FROM dual ) 
	                    AND C.PROGRAMID = '" + programid + "'";

            query = sWhitespace.Replace(query, " ");
            //if (!String.IsNullOrEmpty(programid))
            //{
            //    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", String.Concat("%", kantorid.ToLower(),"%")));
            //    query += " AND PROGRAMID = '" + programid + "'";
            //}
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.RincianAlokasiListDetail>(query, parameters).ToList();
            }

            return records;
        }

        //Original
        //public List<Entities.RincianAlokasiList> GetRincianAlokasiDetailByProgramId(string pTipeKantorId, string pKantorId, string programid, int from, int to)
        //{
        //    List<Entities.RincianAlokasiList> records = new List<Entities.RincianAlokasiList>();

        //    ArrayList arrayListParameters = new ArrayList();

        //    string query =
        //        @"SELECT * FROM (SELECT DISTINCT
        //        row_number () over ( ORDER BY 
        //                A .KODESATKER,
        //                A.KANTORID,
        //                A .NAMAKANTOR,

        //                                        B.PROGRAMID
        //                        ) AS RNumber,
        //          A .KODESATKER,
        //          A.KANTORID,
        //          A .NAMAKANTOR NAMASATKER,

        //                TO_CHAR(B.PROGRAMID),
        //          NVL (SUM(b.TOTALALOKASI), 0) ALOKASI,
        //          NVL (SUM(b.NILAIANGGARAN), 0) AS PAGU,
        //          NVL (SUM(A .OPERASIONAL), 0) AS REALISASIBELANJA,
        //          ROUND(NVL(ROUND(SUM(NULLIF(a.OPERASIONAL,0))/SUM(NULLIF(b.TOTALALOKASI,0)),5)*100,0),0) PERSENALOOKASI,
        //          ROUND(NVL(ROUND(SUM(NULLIF(a.OPERASIONAL,0))/SUM(NULLIF(b.NILAIANGGARAN,0)),5)*100,0),0) PERSENPAGU
        //         FROM
        //          REKAPPENERIMAANDETAIL A
        //         LEFT JOIN MANFAAT b ON A .KANTORID = b.KANTORID
        //         WHERE
        //          ROWNUM < 100000
        //            AND TO_CHAR(B.PROGRAMID) LIKE '%" + programid + "%'";
        //    if (pTipeKantorId != "1")
        //    {
        //        if (!string.IsNullOrEmpty(pTipeKantorId))
        //        {
        //            query += " AND A.KANTORID = '" + pKantorId + "'";
        //        }
        //    }
        //    query += @"GROUP BY
        //          A .KODESATKER,
        //          A .NAMAKANTOR,
        //          A.KANTORID,

        //                B.PROGRAMID) WHERE RNumber BETWEEN :startCnt AND :limitCnt";



        //    query = sWhitespace.Replace(query, " ");
        //    //if (!String.IsNullOrEmpty(programid))
        //    //{
        //    //    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", String.Concat("%", kantorid.ToLower(),"%")));
        //    //    query += " AND PROGRAMID = '" + programid + "'";
        //    //}
        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

        //    using (var ctx = new PnbpContext())
        //    {
        //        object[] parameters = arrayListParameters.OfType<object>().ToArray();
        //        records = ctx.Database.SqlQuery<Entities.RincianAlokasiList>(query, parameters).ToList();
        //    }

        //    return records;
        //}

        public List<Entities.RenaksiList> GetRenaksiList(string kantorid)
        {
            List<Entities.RenaksiList> records = new List<Entities.RenaksiList>();

            ArrayList arrayListParameters = new ArrayList();
            string query = @"
                        SELECT 
                        PROGRAM.KODE,
                        PROGRAM.NAMA,
                        MANFAAT.PROGRAMID,
                        MANFAAT.MANFAATID,
                        MANFAAT.KANTORID,
                        MANFAAT.KODESATKER,
                        MANFAAT.NAMAKANTOR,
                        MANFAAT.TIPE,
                        NVL(MANFAAT.NILAIANGGARAN,0) PAGU,
                        NVL(MANFAAT.ANGGJAN,0) ANGGJAN,
                        NVL(MANFAAT.ANGGFEB,0) ANGGFEB,
                        NVL(MANFAAT.ANGGMAR,0) ANGGMAR,
                        NVL(MANFAAT.ANGGAPR,0) ANGGAPR,
                        NVL(MANFAAT.ANGGMEI,0) ANGGMEI,
                        NVL(MANFAAT.ANGGJUN,0) ANGGJUN,
                        NVL(MANFAAT.ANGGJUL,0) ANGGJUL,
                        NVL(MANFAAT.ANGGAGT,0) ANGGAGT,
                        NVL(MANFAAT.ANGGSEP,0) ANGGSEP,
                        NVL(MANFAAT.ANGGOKT,0) ANGGOKT,
                        NVL(MANFAAT.ANGGNOV,0) ANGGNOV,
                        NVL(MANFAAT.ANGGDES,0) ANGGDES,
                        MANFAAT_RENAKSI_HISTORY.KETERANGANPENOLAKAN AS KETERANGANPENOLAKAN,
                        EVIDEN.EVIDENTID,
                        EVIDEN.EVIDENPATH,
                        NVL(MANFAAT.PERSETUJUAN1,0) AS PERSETUJUAN1,
                        NVL(MANFAAT.PERSETUJUAN2,0) AS PERSETUJUAN2,
                        NVL(MANFAAT.STATUSREVISI,0) AS STATUSREVISI
                        FROM MANFAAT
                        LEFT JOIN PNBP.PROGRAM ON MANFAAT.PROGRAMID = PROGRAM.PROGRAMID
                        LEFT JOIN PNBP.EVIDEN ON MANFAAT.MANFAATID = EVIDEN.EVIDENMANFAATID 
                        LEFT JOIN PNBP.MANFAAT_RENAKSI_HISTORY ON MANFAAT.MANFAATID = MANFAAT_RENAKSI_HISTORY.MANFAAT_MANFAATID
                        WHERE MANFAAT.KANTORID = '" + kantorid + "' AND MANFAAT.TAHUN = 2021";
            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.RenaksiList>(query, parameters).ToList();
            }

            return records;
        }

        public Entities.GetEviden GetevidenById(string manfaatid)
        {
            Entities.GetEviden records = new Entities.GetEviden();
            ArrayList arrayListParameters = new ArrayList();
            string query = @"SELECT * FROM PNBPTRAIN.EVIDEN WHERE EVIDENMANFAATID = '" + manfaatid + "'";
            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.GetEviden>(query).FirstOrDefault();
            }

            return records;
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
        #endregion

    }
}
