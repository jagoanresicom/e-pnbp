using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Pnbp.Models
{
    public class MonitoringEvaluasiModel
    {
        Regex sWhitespace = new Regex(@"\s+");

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

        public List<Entities.PenerimaanMonev> GetPenerimaan(string pTipeKantorId, string pKantorId, string pgmaxpagu, string pgminpagu, int from, int to)
        {
            List<Entities.PenerimaanMonev> records = new List<Entities.PenerimaanMonev>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"
                 SELECT 
                    row_number() over (ORDER BY KODESATKER ASC) AS RNumber,
                    KODESATKER,
                    KANTORID,
                    NAMAKANTOR,
                    JUMLAH,
                    PENERIMAAN,
                    OPERASIONAL,
                    TAHUN,
                    NVL(NILAITARGET, 0) AS NILAITARGET,
                    ROUND(NVL(PERSEN,0),2) AS PERSEN,
                    EVALUASI,
                    RENAKSI,
                    READEVALUASI,
                    READRENAKSI
                    FROM
                    (SELECT
                    A.KODESATKER,
                    A.KANTORID,
                    A.NAMAKANTOR,
                    A.JUMLAH,
                    A.PENERIMAAN,
                    A.OPERASIONAL,
                    A.TAHUN,
                    B.NILAITARGET, 
                    ((A.PENERIMAAN/B.NILAITARGET)*100) PERSEN,
                    ( SELECT COUNT( EVALUASI ) FROM MONEVPENERIMAAN  WHERE KODESATKER = A.KODESATKER AND RENAKSI IS NULL) AS EVALUASI,
	                ( SELECT COUNT( RENAKSI ) FROM MONEVPENERIMAAN WHERE KODESATKER = A.KODESATKER AND EVALUASI IS NULL) AS RENAKSI,
                    ( SELECT COUNT( * ) FROM MONEVPENERIMAAN WHERE KODESATKER = A.KODESATKER AND RENAKSI IS NULL AND (READ IS NULL OR READ = 0)) AS READEVALUASI,
                    ( SELECT COUNT( * ) FROM MONEVPENERIMAAN WHERE KODESATKER = A.KODESATKER AND EVALUASI IS NULL AND (READ IS NULL OR READ = 0)) AS READRENAKSI
                FROM
                    (SELECT 
                    KODESATKER,
                    KANTORID,
                    NAMAKANTOR,
                    TAHUN,
                    SUM(JUMLAH) JUMLAH,
                    SUM(PENERIMAAN) PENERIMAAN,
                    SUM(OPERASIONAL) OPERASIONAL
                    FROM REKAPPENERIMAANDETAIL
                    GROUP BY
                    KODESATKER,
                    KANTORID,
                    NAMAKANTOR,
                    TAHUN) A
                    LEFT JOIN TARGETPROSEDUR B ON A.KANTORID = B.KANTORID AND A.TAHUN = B.TAHUN 
                    )
                    WHERE TAHUN = ( SELECT ( to_char( SYSDATE, 'YYYY' )) AS Y FROM dual )";

            if (pTipeKantorId == "2" || pTipeKantorId == "3" || pTipeKantorId == "4")
            {
                if (!string.IsNullOrEmpty(pKantorId))
                {
                    query += " AND KANTORID = '" + pKantorId + "'";
                    //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pKantorId));
                }
            }

            if (!string.IsNullOrEmpty(pgminpagu))
            {
                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pgminpagu", String.Concat(pgminpagu)));
                query += " AND PERSEN >= " + pgminpagu;
            }

            if (!string.IsNullOrEmpty(pgmaxpagu))
            {
                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pgminpagu", String.Concat(pgminpagu)));
                query += " AND PERSEN <= " + pgmaxpagu;
            }

            //query += " WHERE RNumber BETWEEN :startCnt AND :limitCnt";

            query = sWhitespace.Replace(query, " ");

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.PenerimaanMonev>(query, parameters).ToList();
            }

            return records;
        }

        public List<Entities.BelanjaMonev> GetBelanja(string pTipeKantorId, string pKantorId, string pgmaxpagu, string pgminpagu, string pgmaxalok, string pgminalok, int opsi, int from, int to)
        {
            List<Entities.BelanjaMonev> records = new List<Entities.BelanjaMonev>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"
                   SELECT
                        row_number() over (ORDER BY KODESATKER ASC) AS RNumber,
	                    KODESATKER,
                        KANTORID,
	                    NAMAKANTOR,
	                    TAHUN,
	                    PAGU,
	                    OPS,
	                    NONOPS,
	                    ALOKASI,
	                    REALISASIBELANJA,
	                    PERSENPAGU,
	                    PERSENALOK,
	                    EVALUASI,
	                    RENAKSI,
	                    READEVALUASI,
	                    READRENAKSI 
                    FROM
                    (
                    SELECT
	                    C.KDSATKER AS KODESATKER,
	                    B.KANTORID,
	                    B.NAMAKANTOR,
	                    B.TAHUN,
	                    A.PAGU,
	                    B.OPS,
	                    B.NONOPS,
	                    B.ALOKASI,
	                    C.REALISASIBELANJA,
	                    ROUND((C.REALISASIBELANJA/A.PAGU)*100, 2) AS PERSENPAGU,
	                    ROUND((C.REALISASIBELANJA/B.ALOKASI)*100, 2) AS PERSENALOK,
	                    ( SELECT COUNT( EVALUASI ) FROM MONEVBELANJA WHERE KODESATKER = B.KODESATKER AND RENAKSI IS NULL ) AS EVALUASI,
	                    ( SELECT COUNT( RENAKSI ) FROM MONEVBELANJA WHERE KODESATKER = B.KODESATKER AND EVALUASI IS NULL ) AS RENAKSI,
	                    (
		                    SELECT
			                    COUNT( * ) 
		                    FROM
			                    MONEVBELANJA 
		                    WHERE
			                    KODESATKER = B.KODESATKER 
			                    AND RENAKSI IS NULL 
		                    AND ( READ IS NULL OR READ = 0 )) AS READEVALUASI,
		                    (
		                    SELECT
			                    COUNT( * ) 
		                    FROM
			                    MONEVBELANJA 
		                    WHERE
			                    KODESATKER = B.KODESATKER 
			                    AND EVALUASI IS NULL 
		                    AND ( READ IS NULL OR READ = 0 )) AS READRENAKSI 
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
	                    GROUP BY
		                    KDSATKER 
	                    ) A
                    LEFT JOIN (
	                    SELECT
		                    KODESATKER,
		                    TAHUN,
		                    NAMAKANTOR,
		                    KANTORID,
		                    SUM( TOTALALOKASI ) AS OPS,
		                    SUM( ANGGJAN + ANGGFEB ) AS NONOPS,
		                    SUM( ANGGJAN + ANGGFEB ) + SUM( TOTALALOKASI ) AS ALOKASI
	                    FROM
		                    MANFAAT 
	                    GROUP BY
		                    KODESATKER,
		                    KANTORID,
		                    TAHUN,
		                    NAMAKANTOR
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
	                    GROUP BY
		                    KDSATKER 
	                    ) C ON B.KODESATKER = C.KDSATKER
	                    )
	                    WHERE TAHUN = ( SELECT (to_char( SYSDATE, 'YYYY' )) AS Y FROM dual )";

            if (pTipeKantorId == "2" || pTipeKantorId == "3" || pTipeKantorId == "4")
            {
                if (!string.IsNullOrEmpty(pKantorId))
                {
                    query += " AND KANTORID = '" + pKantorId + "'";
                    //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pKantorId));
                }
            }
         
            //WHERE PERSENPAGU < 3 AND PERSENALOK < 3";
            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorIdUser", kantoriduser));

            query = sWhitespace.Replace(query, " ");

            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorIdUser", kantoriduser));
            //query += " GROUP BY a.KODESATKER, a.NAMAKANTOR, a.PENERIMAAN,b.NILAITARGET ";


            if (opsi != 0)
            {
                //if (opsi == 1 && pgminpagu != 0)
                //{
                //    //    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pgminpagu", String.Concat(pgminpagu)));mati
                //    query += " AND PERSENPAGU < " + pgminpagu;
                //}

                if (opsi == 1 && !string.IsNullOrEmpty(pgminpagu))
                {
                    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pgminpagu", String.Concat(pgminpagu)));
                    query += " AND PERSENPAGU >= " + pgminpagu;
                }

                if (opsi == 1 && !string.IsNullOrEmpty(pgmaxpagu))
                {
                    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pgminpagu", String.Concat(pgminpagu)));
                    query += " AND PERSENPAGU <= " + pgmaxpagu;
                }

                if (opsi == 2 && !string.IsNullOrEmpty(pgminalok))
                {
                    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pgminpagu", String.Concat(pgminpagu)));
                    query += " AND PERSENALOK >= " + pgminalok;
                }

                if (opsi == 2 && !string.IsNullOrEmpty(pgmaxalok))
                {
                    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pgminpagu", String.Concat(pgminpagu)));
                    query += " AND PERSENALOK <= " + pgmaxalok;
                }

                //if (opsi == 2 && pgminalok != 0)
                //{
                //    query += " AND PERSENALOK < " + pgminalok;
                //}
            }

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.BelanjaMonev>(query, parameters).ToList();
            }
            return records;
        }

        //public List<Entities.BelanjaMonev> GetBelanja(string kantoriduser, string namakantor, string judul, string nomorberkas, string kodebilling, string nptn, string namapemohon, string nikpemohon, string alamatpemohon, string teleponpemohon, string bankpersepsi, string status, string namasatker, int from, int to)
        //{
        //    List<Entities.BelanjaMonev> records = new List<Entities.BelanjaMonev>();

        //    ArrayList arrayListParameters = new ArrayList();

        //    string query =
        //        @"
        //           SELECT
        //             a.KODESATKER,
        //             a.NAMAKANTOR,
        //                a.KANTORID,
        //             TO_CHAR(b.NILAIANGGARAN) AS PAGU,
        //                TO_CHAR(b.TOTALALOKASI) AS ALOKASI,
        //             a.OPERASIONAL AS REALISASI_BELANJA,
        //             row_number () over ( ORDER BY SUM( a.KODESATKER ) ASC ) AS URUTAN 
        //            FROM
        //             REKAPPENERIMAANDETAIL a
        //             LEFT JOIN MANFAAT b ON a.KODESATKER = b.KODESATKER 
        //            WHERE
        //             ROWNUM < 100 
        //            GROUP BY
        //             a.KODESATKER,
        //             a.NAMAKANTOR,
        //             a.KANTORID,
        //             b.NILAIANGGARAN,
        //            b.TOTALALOKASI,
        //             a.OPERASIONAL
        //        ";

        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorIdUser", kantoriduser));
        //    //query += " GROUP BY a.KODESATKER, a.NAMAKANTOR, a.PENERIMAAN,b.NILAITARGET ";

        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

        //    using (var ctx = new PnbpContext())
        //    {
        //        object[] parameters = arrayListParameters.OfType<object>().ToArray();
        //        records = ctx.Database.SqlQuery<Entities.BelanjaMonev>(query, parameters).ToList();
        //    }
        //    return records;
        //}

        //old
        //public List<Entities.PenerimaanMonev> GetPenerimaan(string kantoriduser, string namakantor, string judul, string nomorberkas, string kodebilling, decimal persen, string nptn, string namapemohon, string nikpemohon, string alamatpemohon, string teleponpemohon, string bankpersepsi, string status, string namasatker, int from, int to)
        //{
        //    List<Entities.PenerimaanMonev> records = new List<Entities.PenerimaanMonev>();

        //    ArrayList arrayListParameters = new ArrayList();

        //    string query =
        //        @"
        //            WITH AA as(
        //                SELECT
        //                 a.KODESATKER,
        //                    a.KANTORID,
        //                 a.NAMAKANTOR,
        //                    c.EVALUASI,
        //                 b.nilaitarget,
        //                 a.PENERIMAAN,
        //                 ROUND(SUM(PENERIMAAN/nilaitarget) * 100, 2) AS persen,
        //                 row_number () over ( ORDER BY SUM( a.KODESATKER ) ASC ) AS urutan 
        //                FROM
        //                 REKAPPENERIMAANDETAIL a
        //                 LEFT JOIN TARGETPROSEDUR b ON a.NAMAPROSEDUR = b.NAMAPROSEDUR
        //                    LEFT JOIN MONEVPENERIMAAN c ON a.KODESATKER = c.KODESATKER
        //                WHERE
        //                 ROWNUM < 10
        //                GROUP BY
        //                 a.KODESATKER,
        //                    a.KANTORID,
        //                 a.NAMAKANTOR,
        //                    c.EVALUASI,
        //                 a.PENERIMAAN,
        //                 b.NILAITARGET) SELECT * FROM AA
        //                 WHERE persen < " + persen;

        //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorIdUser", kantoriduser));

        //    //query += " GROUP BY a.KODESATKER, a.NAMAKANTOR, a.PENERIMAAN,b.NILAITARGET ";

        //    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
        //    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));
        //    //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("persen", persen));

        //    using (var ctx = new PnbpContext())
        //    {
        //        object[] parameters = arrayListParameters.OfType<object>().ToArray();
        //        records = ctx.Database.SqlQuery<Entities.PenerimaanMonev>(query).ToList();
        //    }
        //    return records;
        //}


    }
}