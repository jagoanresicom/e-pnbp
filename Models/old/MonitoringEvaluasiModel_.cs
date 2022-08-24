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
                @"SELECT
	                    *
                    FROM
	                    (
		                    SELECT DISTINCT
			                    ROW_NUMBER () OVER (

				                    ORDER BY
					                    A .KODESATKER,
					                    A .KANTORID,
					                    A .NAMAKANTOR,
					                    c.EVALUASI,
					                    c.RENAKSI
			                    ) AS RNumber,
			                    A .KODESATKER,
			                    A .KANTORID,
			                    A .NAMAKANTOR,
			                    c.EVALUASI,
			                    c.RENAKSI,
			                    NVL((
				                    SELECT
					                    SUM (NILAITARGET)
				                    FROM
					                    TARGETPROSEDUR
				                    WHERE
					                    KANTORID = A .KANTORID
			                    ),0) AS NILAITARGET,
			                    NVL((
				                    SELECT DISTINCT
					                    SUM (PENERIMAAN)
				                    FROM
					                    REKAPPENERIMAANDETAIL
				                    WHERE
					                    KANTORID = A .KANTORID
			                    ),0) AS PENERIMAAN,
			                    ROUND (
				                    SUM (PENERIMAAN / NILAITARGET) * 100,
				                    2
			                    ) AS persen,
			                    ROW_NUMBER () OVER (

				                    ORDER BY
					                    SUM (A .KODESATKER) ASC
			                    ) AS urutan
		                    FROM
			                    REKAPPENERIMAANDETAIL A
		                    LEFT JOIN TARGETPROSEDUR b ON A .NAMAPROSEDUR = b.NAMAPROSEDUR
		                    LEFT JOIN MONEVPENERIMAAN c ON A .KODESATKER = c.KODESATKER
		                    WHERE
			                    ROWNUM < 60000";

            if (pTipeKantorId == "2" || pTipeKantorId == "3" || pTipeKantorId == "4")
            {
                if (!string.IsNullOrEmpty(pKantorId))
                {
                    query += " AND a.KANTORID = '" + pKantorId + "'";
                    //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pKantorId));
                }
            }

            query += @"GROUP BY
		                a.KODESATKER,
		                a.KANTORID,
		                a.NAMAKANTOR,
		                c.EVALUASI,
                        c.RENAKSI)
                        WHERE RNumber BETWEEN :startCnt AND :limitCnt";

            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KANTORID", pTipeKantorId));
            query = sWhitespace.Replace(query, " ");


            //if (!String.IsNullOrEmpty(kodesatker))
            //{
            //    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeSatker", String.Concat("%", kodesatker.ToLower(), "%")));
            //    //query += " AND LOWER(KODESATKER) LIKE :KodeSatker ";
            //}

            if (!string.IsNullOrEmpty(pgminpagu))
            {
                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pgminpagu", String.Concat(pgminpagu)));
                query += " AND persen >= " + pgminpagu;
            }

            if (!string.IsNullOrEmpty(pgmaxpagu))
            {
                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pgminpagu", String.Concat(pgminpagu)));
                query += " AND persen <= " + pgmaxpagu;
            }

            //if (!string.IsNullOrEmpty(pgmaxpagu) && !string.IsNullOrEmpty(pgminpagu))
            //{
            //    query += "";
            //}

            //query +=
            //    " ) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

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
                   SELECT * FROM (
                        SELECT
                         row_number () over( ORDER BY
	                        a.KODESATKER,
	                        a.KANTORID,
	                        a.NAMAKANTOR,
	                        c.EVALUASI) AS RNumber,
	                        a.KODESATKER,
	                        a.KANTORID,
	                        a.NAMAKANTOR,
	                        c.EVALUASI,
                            c.RENAKSI,
	                        NVL 
                            (
				                (
					                SELECT SUM (NILAIANGGARAN) FROM MANFAAT WHERE KANTORID = A.KANTORID
				                ), 0
			                ) PAGU,
	                        NVL 
                            (
				                (
					                SELECT SUM (TOTALALOKASI) FROM MANFAAT WHERE KANTORID = A.KANTORID
				                ), 0
			                ) ALOKASI,
	                        ROUND ( NVL 
                            (
				                (
					                SELECT SUM (OPERASIONAL) FROM REKAPPENERIMAANDETAIL WHERE KANTORID = A.KANTORID
				                ), 0
			                ),2) REALISASIBELANJA,
	                        ROUND((NVL(SUM(NULLIF(a.OPERASIONAL,0)),0)/NVL(SUM(NULLIF(b.NILAIANGGARAN,0)),0))*100, 5) AS PERSENPAGU,
                            NVL(ROUND(SUM(NULLIF(a.OPERASIONAL,0))/SUM(NULLIF(b.TOTALALOKASI,0)),5)*100,0) PERSENALOK
	                        
                        FROM
	                        REKAPPENERIMAANDETAIL a
                        LEFT JOIN MANFAAT b ON a.KODESATKER = b.KODESATKER
                        LEFT JOIN MONEVBELANJA c ON a.KODESATKER = c.KODESATKER 
                        WHERE
	                        ROWNUM < 5000";
            if (pTipeKantorId == "2" || pTipeKantorId == "3" || pTipeKantorId == "4")
            {
                if (!string.IsNullOrEmpty(pKantorId))
                {
                    query += " AND a.KANTORID = '" + pKantorId + "'";
                    //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pKantorId));
                }
            }
            query += @"GROUP BY
	                        a.KODESATKER,
	                        a.KANTORID,
	                        a.NAMAKANTOR,
	                        c.EVALUASI,
                            c.RENAKSI
	                        ) 
                            WHERE RNumber BETWEEN: startCnt AND :limitCnt";
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