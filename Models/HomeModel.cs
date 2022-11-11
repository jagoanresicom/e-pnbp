using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pnbp.Models
{
    public class HomeModel
    {
        public static List<Entities.RekapPenerimaan> dtRekapPenerimaan(string pTahun, string pSatker)
        {
            var _result = new List<Entities.RekapPenerimaan>();
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                List<object> lstparams = new List<object>();

                string query =
                   @" select tahun, bulan, jumlahberkas, penerimaan, operasional from rekappenerimaan where tahun = :Tahun ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", pTahun));

                if(!string.IsNullOrEmpty(pSatker))
                {
                    query += " and kantorid = :kantorid ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kantorid", pSatker));
                }

                query += "order by bulan desc ";

                query = sWhitespace.Replace(query, " ");
                var parameters = lstparams.ToArray();
                _result = ctx.Database.SqlQuery<Entities.RekapPenerimaan>(query, parameters).ToList();

            }
            return _result;
        }

        public static Entities.TransactionResult ListRealisasiSatker(string tahun, string kodeProvinsi)
        {
            var tr = new Entities.TransactionResult()
            {
                Status = false,
                Data = null,
            };

            try
            {
                using (var ctx = new PnbpContext())
                {
                    Regex sWhitespace = new Regex(@"\s+");
                    List<object> lstparams = new List<object>();

                    string queryCondsString = "";
                    List<string> queryConds = new List<string>();
                    if (!string.IsNullOrEmpty(tahun))
                    {
                        queryConds.Add(" sr.TAHUN = :tahun ");
                        lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tahun", tahun));
                    }
                    if (!string.IsNullOrEmpty(kodeProvinsi))
                    {
                        queryConds.Add(" prov.kode = :kodeProvinsi");
                        lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kodeProvinsi", kodeProvinsi));
                    }
                    if (queryConds.Count > 0)
                    {
                        queryCondsString = " AND " + string.Join(" AND ", queryConds);
                    }

                    string query = $@"SELECT
	                                kodesatker,
	                                namasatker,
	                                SUM(realisasi) realisasi,
	                                COUNT(DISTINCT OUTPUT) jumlahlayanan,
	                                kodeprovinsi,
	                                bulan,
	                                tahun
                                FROM
	                                (
	                                SELECT
		                                sr.KDSATKER kodesatker,
		                                s.NAMA_SATKER namasatker,
		                                amount realisasi,
		                                OUTPUT,
		                                prov.kode kodeprovinsi,
		                                SUBSTR(sr.TANGGAL, 4, 3) bulan,
		                                sr.tahun
	                                FROM
		                                SPAN_REALISASI sr
	                                JOIN satker s ON
		                                s.KODESATKER = sr.KDSATKER
	                                JOIN kantor k ON
		                                k.kodesatker = s.KODESATKER
	                                JOIN wilayah w ON
		                                k.kode = w.kode
	                                JOIN wilayah prov ON
		                                prov.wilayahid = w.induk
	                                WHERE
		                                SUMBERDANA = 'D'
		                                AND KDSATKER != '524465'
                                        {queryCondsString}
		                                )
                                GROUP BY
	                                (kodesatker,namasatker,kodeprovinsi,bulan,tahun)
                                ORDER BY namasatker";
                    query = sWhitespace.Replace(query, " ");

                    tr.Data = ctx.Database.SqlQuery<Entities.RealisasiSatker>(query, lstparams.ToArray()).ToList();
                    tr.Status = true;
                }
            }
            catch (Exception ex)
            {
                tr.Pesan = "Terjadi kesalahan";
            }

            return tr;
        }

        public static List<Entities.RekapAlokasi> dtRekapAlokasi(string pTahun, string pTipe)
        {
            var _result = new List<Entities.RekapAlokasi>();
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                List<object> lstparams = new List<object>();

                string query =
                    @" select
                          to_char(r1.bulan) as bulan, sum(nvl(teralokasi,0)) as alokasi
                        from
                          (SELECT
                             level as bulan
                           FROM
                             DUAL
                           CONNECT BY
                             LEVEL <= 12) r1
                          left join rekapalokasi r2 on
                            r1.bulan = r2.bulan
                            and r2.tahun = :param1
                            and r2.statusalokasi = 1
                            ";
                if(!string.IsNullOrEmpty(pTipe))
                {
                    query += " and r2.tipemanfaat = :param2";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pTipe));
                }
                query += @" group by
                          r1.bulan
                        order by r1.bulan";

                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pTahun));                

                query = sWhitespace.Replace(query, " ");
                var parameters = lstparams.ToArray();
                _result = ctx.Database.SqlQuery<Entities.RekapAlokasi>(query, parameters).ToList();

            }
            return _result;
        }

        public static List<Pnbp.Entities.Tahun> lsTahunPenerimaan()
        {
            List<Pnbp.Entities.Tahun> result = new List<Pnbp.Entities.Tahun>();
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");

                string query =
                   @" select distinct to_char(tahun) as value, to_char(tahun) as tahun from rekappenerimaan order by tahun desc";
                result = ctx.Database.SqlQuery<Pnbp.Entities.Tahun>(query).ToList();

            }
            return result;
        }

        public List<Entities.JumlahPenerimaanOperasional> totalPenerimaan()
        {
            var total = new List<Entities.JumlahPenerimaanOperasional>();
            //List<object> lstparams = new List<object>();

            using (var ctx = new PnbpContext())
            {
                string query =
                 @" 
                    SELECT
	                    SUM( PENERIMAAN ) AS total_penerimaan,
	                    SUM( OPERASIONAL ) AS total_oprasional 
                    FROM
	                    REKAPPENERIMAANDETAIL
                 ";
                //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", kodeBillings));

                //Regex sWhitespace = new Regex(@"\s+");
                //query = sWhitespace.Replace(query, " ");

                //Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", (String.IsNullOrEmpty(kodeBillings)) ? "" : kodeBillings);

                //object[] parameters = new object[1] { p1 };
                total = ctx.Database.SqlQuery<Entities.JumlahPenerimaanOperasional>(query).ToList<Entities.JumlahPenerimaanOperasional>();
            }

            return total;
        }

        public static List<Entities.Mp> dtMp(string pTahun, string pSatker)
        {
            var _result = new List<Entities.Mp>();
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                List<object> lstparams = new List<object>();

                string query =
                    //@"SELECT SUM(TOTALALOKASI) AS NILAIMP, ROUND((SUM(TOTALALOKASI) / 7302304508000) * 100, 2) AS PERSENMP FROM MANFAAT WHERE TAHUN =2018 ";
                    @"
                        SELECT DISTINCT
	                        NVL(NULLIF(SUM(b.OPERASIONAL),0),0) AS OPERASIONAL,
	                        NVL(NULLIF(SUM(a.TOTALALOKASI),0),0) AS NILAIMP,
	                        ROUND (
		                        (
			                        NULLIF(SUM (b.OPERASIONAL),0) / NULLIF(SUM (A .TOTALALOKASI),0)
		                        ) * 100,
		                        5
	                        ) AS PERSENMP 
	                        FROM
		                        MANFAAT a
		                        LEFT JOIN REKAPPENERIMAANDETAIL b ON a.KODESATKER = b.KODESATKER
	                        WHERE
		                        a.TAHUN = 2018 AND ROWNUM < 600000 AND b.OPERASIONAL IS NOT NULL AND a.TOTALALOKASI IS NOT NULL
		
		
                     ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", pTahun));

                query = sWhitespace.Replace(query, " ");
                var parameters = lstparams.ToArray();
                _result = ctx.Database.SqlQuery<Entities.Mp>(query, parameters).ToList();
            }
            return _result;
        }

        public static List<Entities.AlokasiBelanja> dtAlokasiBelanjaNonOps(string pTahun, string pSatker)
        {
            var _result = new List<Entities.AlokasiBelanja>();
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                List<object> lstparams = new List<object>();

                string query =
                    @" SELECT BULAN, SUM( TERALOKASI ) AS Jumlah FROM REKAPALOKASI WHERE TIPEMANFAAT = 'NONOPS' AND TAHUN = ( SELECT ( to_char( SYSDATE, 'YYYY' )) AS Y FROM dual )  GROUP BY BULAN ";
                //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", pTahun));

                //if (!string.IsNullOrEmpty(pSatker))
                //{
                //    query += " and kantorid = :kantorid ";
                //    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kantorid", pSatker));
                //}

                query += "ORDER BY BULAN ";

                query = sWhitespace.Replace(query, " ");
                var parameters = lstparams.ToArray();
                _result = ctx.Database.SqlQuery<Entities.AlokasiBelanja>(query, parameters).ToList();
            }
            return _result;
        }

        public static List<Entities.AlokasiBelanja> dtAlokasiBelanjaOps(string pTahun, string pSatker)
        {
            var _result = new List<Entities.AlokasiBelanja>();
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                List<object> lstparams = new List<object>();

                string query =
                    @" SELECT BULAN AS BULANALOKOPS, SUM( TERALOKASI ) AS JUMLAHOPS FROM REKAPALOKASI WHERE TIPEMANFAAT = 'OPS' AND TAHUN = ( SELECT ( to_char( SYSDATE, 'YYYY' )) AS Y FROM dual )  GROUP BY BULAN ";
                //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", pTahun));

                //if (!string.IsNullOrEmpty(pSatker))
                //{
                //    query += " and kantorid = :kantorid ";
                //    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kantorid", pSatker));
                //}

                query += "ORDER BY BULAN ";

                query = sWhitespace.Replace(query, " ");
                var parameters = lstparams.ToArray();
                _result = ctx.Database.SqlQuery<Entities.AlokasiBelanja>(query, parameters).ToList();
            }
            return _result;
        }

        public static List<Entities.BelanjaNonOps> dtBelanjaNonOps(string pTahun, string pSatker)
        {
            var _result = new List<Entities.BelanjaNonOps>();
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                List<object> lstparams = new List<object>();

                if (string.IsNullOrEmpty(pTahun))
                {
                    pTahun = DateTime.Now.Year.ToString();
                }

                string query =
                    $@"SELECT DISTINCT
	                        SUBSTR (A .TANGGAL, 4, 3) AS Bulan,
	                        (
		                        CASE
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'JAN' THEN
			                        '1'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'FEB' THEN
			                        '2'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'MAR' THEN
			                        '3'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'APR' THEN
			                        '4'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'MEI' THEN
			                        '5'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'JUN' THEN
			                        '6'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'JUL' THEN
			                        '7'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'AGT' THEN
			                        '8'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'SEP' THEN
			                        '9'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'OKT' THEN
			                        '10'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'NOV' THEN
			                        '11'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'DES' THEN
			                        '12'
		                        END
	                        ) AS BULANNONOPS, 
                        SUM (A .AMOUNT) AS BELANJANONOPS
                        FROM
	                        SPAN_REALISASI A
                        LEFT JOIN KODESPAN b ON A .KEGIATAN = b.KODE
                        AND A .OUTPUT = b.KEGIATAN
                        WHERE
	                        b.TIPE = 'NONOPS' AND
                            A.TAHUN = {pTahun} 
                        GROUP BY
	                        SUBSTR (A .TANGGAL, 4, 3)";
                //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", pTahun));

                //query += "ORDER BY BULAN ";

                query = sWhitespace.Replace(query, " ");
                var parameters = lstparams.ToArray();
                _result = ctx.Database.SqlQuery<Entities.BelanjaNonOps>(query, parameters).ToList();
            }
            return _result;
        }

        public static List<Entities.BelanjaOps> dtBelanjaOps(string pTahun, string pSatker)
        {
            var _result = new List<Entities.BelanjaOps>();
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                List<object> lstparams = new List<object>();
                
                if (string.IsNullOrEmpty(pTahun))
                {
                    pTahun = DateTime.Now.Year.ToString();
                }

                string query =
                    $@"SELECT DISTINCT
	                        SUBSTR (A .TANGGAL, 4, 3) AS Bulan,
	                        (
		                        CASE
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'JAN' THEN
			                        '1'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'FEB' THEN
			                        '2'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'MAR' THEN
			                        '3'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'APR' THEN
			                        '4'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'MEI' THEN
			                        '5'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'JUN' THEN
			                        '6'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'JUL' THEN
			                        '7'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'AGT' THEN
			                        '8'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'SEP' THEN
			                        '9'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'OKT' THEN
			                        '10'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'NOV' THEN
			                        '11'
		                        WHEN SUBSTR (A .TANGGAL, 4, 3) = 'DES' THEN
			                        '12'
		                        END
	                        ) AS BULANOPS, 
                        SUM (A .AMOUNT) AS BELANJAOPS
                        FROM
	                        SPAN_REALISASI A
                        LEFT JOIN KODESPAN b ON A .KEGIATAN = b.KODE
                        AND A .OUTPUT = b.KEGIATAN
                        WHERE
	                        b.TIPE = 'OPS' AND TAHUN = {pTahun}
                        GROUP BY
	                        SUBSTR (A .TANGGAL, 4, 3)";
                //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", pTahun));

                //query += "ORDER BY BULAN ";

                query = sWhitespace.Replace(query, " ");
                var parameters = lstparams.ToArray();
                _result = ctx.Database.SqlQuery<Entities.BelanjaOps>(query, parameters).ToList();
            }
            return _result;
        }
    }
}