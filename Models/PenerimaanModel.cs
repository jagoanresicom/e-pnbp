using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Pnbp.Models
{
    public class PenerimaanModel
    {
        public List<Entities.JumlahPenerimaan> dtJumlahPenerimaan(string pBulan, string pTahun, int pLimit, bool pSortDesc)
        {
            var _dtJumlahPenerimaan = new List<Entities.JumlahPenerimaan>();

            //            using (var ctx = new PnbpContext())
            //            {
            //                string query =
            //                    @"SELECT 
            //                            *
            //                        FROM 
            //                            (SELECT 
            //                                INITCAP(NAMAKANTOR) AS NAMAKANTOR, SUM(JUMLAH) AS JUMLAH, SUM(NILAIAKHIR) AS NILAIAKHIR, 
            //                                SUM(OPERASIONAL) AS OPERASIONAL, 
            //                                ROW_NUMBER() OVER (ORDER BY SUM(JUMLAH)) RNUMBER
            //                            FROM 
            //                                PENERIMAAN
            //                            WHERE
            //                                PENERIMAAN.TAHUN = :param1 ";

            //                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", Convert.ToInt32(pTahun));
            //                object[] parameters = new object[1] { p1 };

            //                if (!String.IsNullOrEmpty(pBulan))
            //                {
            //                    query += " AND penerimaan.bulan = :param2 ";
            //                    Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pBulan);
            //                    parameters = new object[2] { p1, p2 };
            //                }

            //                query +=
            //                    @" GROUP BY namakantor
            //                       ) WHERE RNUMBER BETWEEN 1 AND " + pLimit + "";

            //                if (pSortDesc)
            //                {
            //                    query = query.Replace("ORDER BY SUM(jumlah)", "ORDER BY SUM(jumlah) DESC");
            //                }

            //                _dtJumlahPenerimaan = ctx.JumlahPenerimaans.SqlQuery(query, parameters).ToList<Entities.JumlahPenerimaan>();
            //            }

            return _dtJumlahPenerimaan;
        }

        struct CountResult
        {
            public int Count { get; set; }
        }

        public List<Entities.StatistikNTPN> dtStatistikNTPN(string pKantorId, DateTime pStartDate, DateTime pEndDate, int from, int to)
        {
            var _dtStatistikNTPN = new List<Entities.StatistikNTPN>();
            var parameters = new object[1];

            using (var ctx = new PnbpContext())
            {
                string query =
                 @" SELECT
                      dt.*
                    FROM
                      (SELECT
                         INITCAP(namakantor) as namakantor, kantorid, COUNT(DISTINCT ntpn) JUMLAHNTPN, COUNT(DISTINCT berkasid) JUMLAHBERKAS,
                         sum(JUMLAH) NILAINTPN,
                         ROW_NUMBER() over (ORDER BY sum(JUMLAH) DESC, COUNT(DISTINCT ntpn) DESC, namakantor) RNUMBER, count(distinct KANTORID) over () TOTALREC 
                       FROM
                         (SELECT
                            NAMAKANTOR, BERKASID, NTPN, KANTORID, JUMLAH
                          FROM
                            rekappenerimaandetail
                          WHERE
                            trunc(tanggal) >= TO_DATE(TO_CHAR(:tgldari,'dd/mm/yyyy')||' 00:00:00', 'DD/MM/YYYY HH24:MI:SS') 
                        and trunc(tanggal) <= TO_DATE(TO_CHAR(:tglsampai,'dd/mm/yyyy')||' 23:59:59', 'DD/MM/YYYY HH24:MI:SS') ";
                Oracle.ManagedDataAccess.Client.OracleParameter tgldari = new Oracle.ManagedDataAccess.Client.OracleParameter("tgldari", pStartDate);
                Oracle.ManagedDataAccess.Client.OracleParameter tglsampai = new Oracle.ManagedDataAccess.Client.OracleParameter("tglsampai", pEndDate);

                if (!string.IsNullOrEmpty(pKantorId))
                {
                    query += @" AND KANTORID = :kantorid ) GROUP BY namakantor, kantorid) dt
                            WHERE
                              RNUMBER BETWEEN :dari AND :sampai";
                    Oracle.ManagedDataAccess.Client.OracleParameter kantorid = new Oracle.ManagedDataAccess.Client.OracleParameter("kantorid", pKantorId);
                    Oracle.ManagedDataAccess.Client.OracleParameter dari = new Oracle.ManagedDataAccess.Client.OracleParameter("dari", from);
                    Oracle.ManagedDataAccess.Client.OracleParameter sampai = new Oracle.ManagedDataAccess.Client.OracleParameter("sampai", to);
                    parameters = new object[5] { tgldari, tglsampai, kantorid, dari, sampai };
                }
                else
                {
                    query += @" ) GROUP BY namakantor, kantorid) dt
                            WHERE
                              RNUMBER BETWEEN :dari AND :sampai";
                    Oracle.ManagedDataAccess.Client.OracleParameter dari = new Oracle.ManagedDataAccess.Client.OracleParameter("dari", from);
                    Oracle.ManagedDataAccess.Client.OracleParameter sampai = new Oracle.ManagedDataAccess.Client.OracleParameter("sampai", to);
                    parameters = new object[4] { tgldari, tglsampai, dari, sampai };
                }

                Regex sWhitespace = new Regex(@"\s+");
                query = sWhitespace.Replace(query, " ");
                _dtStatistikNTPN = ctx.Database.SqlQuery<Entities.StatistikNTPN>(query, parameters).ToList<Entities.StatistikNTPN>();
            }

            return _dtStatistikNTPN;
        }

        public List<Entities.StatistikNTPN> dtStatistikNTPNKanwil(string pKantorId, DateTime pStartDate, DateTime pEndDate, int from, int to)
        {
            var _dtStatistikNTPN = new List<Entities.StatistikNTPN>();
            List<object> lstparams = new List<object>();

            using (var ctx = new PnbpContext())
            {
                string query =
                 @" SELECT
                      dt.*
                    FROM
                      (SELECT
                         INITCAP(namakantor) as namakantor, kantorid, COUNT(DISTINCT ntpn) JUMLAHNTPN, COUNT(*) JUMLAHBERKAS,
                         sum(JUMLAH) NILAINTPN,
                         ROW_NUMBER() over (ORDER BY sum(JUMLAH) DESC, COUNT(DISTINCT ntpn) DESC, namakantor) RNUMBER, count(distinct KANTORID) over () TOTALREC 
                       FROM
                         (SELECT
                            NAMAKANTOR, BERKASID, NTPN, KANTORID, JUMLAH
                          FROM
                            penerimaan
                          WHERE
                            TANGGAL BETWEEN TO_DATE(:param2 || ' 00:00:00','dd/mm/yyyy hh24:mi:ss') AND TO_DATE(:param3 || ' 23:59:59','dd/mm/yyyy hh24:mi:ss') ";

                if (!string.IsNullOrEmpty(pKantorId))
                {
                    query += " AND exists(select 1 from kantor k where k.induk = :param5 and k.kantorid = penerimaan.kantorid) ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param5", pKantorId));
                }
                query += @")   
                       GROUP BY
                         namakantor, kantorid) dt
                    WHERE
                      RNUMBER BETWEEN :param3 AND :param4 ";

                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pStartDate));
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pEndDate));
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", from));
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", to));

                var parameters = lstparams.ToArray();
                _dtStatistikNTPN = ctx.Database.SqlQuery<Entities.StatistikNTPN>(query, parameters).ToList<Entities.StatistikNTPN>();
            }

            return _dtStatistikNTPN;
        }

        public List<Entities.StatistikNTPNDetail> dtStatistikNTPNDetail(string pKantorId, string pStartDate, string pEndDate, string kodeBilling, string ntpn, int from, int to)
        {
            var _dtStatistikNTPNDetail = new List<Entities.StatistikNTPNDetail>();
            List<object> lstparams = new List<object>();

            using (var ctx = new PnbpContext())
            {
                string query =
                 @"
                SELECT
                    pn.kantorid, 
                    pn.berkasid, 
                    pn.nomorberkas, 
                    pn.tahunberkas, 
                    pn.kodesatker, 
                    pn.namakantor, 
                    pn.kodebilling, 
                    sm.total_nominal_billing as jumlah, 
                    pn.ntpn, 
                    sr.kode,
                    sum(pn.penerimaan) as penerimaan, 
                    TO_CHAR(SM.TANGGALKODEBILLING,'dd/mm/yyyy') AS TGLBILLING,
                    TO_CHAR(SM.TGL_JAM_PEMBAYARAN,'dd/mm/yyyy') AS TGLNTPN,
                    DI.NOMOR AS NOMOR305, TO_CHAR(DI.TANGGAL,'dd/mm/yyyy') AS TGL305, di.besarnya as jumlahdi305,
                    row_number() over (order by TO_CHAR(SM.TANGGALKODEBILLING,'dd/mm/yyyy'), pn.kodebilling) as rnumber,
                    count(*) over () TOTALREC
                FROM
                    rekappenerimaandetail PN 
                LEFT JOIN KKPWEB.BERKASSIMPONI SM ON PN.KODEBILLING = SM.KODEBILLING 
                LEFT JOIN KKPWEB.DI305 DI ON SM.DI305ID = DI.DI305ID
                LEFT JOIN SATKER SR ON PN.KANTORID = SR.KANTORID
                WHERE
                    PN.TANGGAL BETWEEN TO_DATE(:param2 || ' 00:00:00','dd/mm/yyyy hh24:mi:ss') AND TO_DATE(:param3 || ' 23:59:59','dd/mm/yyyy hh24:mi:ss') ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pStartDate));
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pEndDate));
                //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", from));
                //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param5", to));

                if (!String.IsNullOrEmpty(pKantorId))
                {
                    query += " AND PN.KANTORID = :param1 ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pKantorId));
                }

                if (!String.IsNullOrEmpty(kodeBilling))
                {
                    query += " AND PN.KODEBILLING = :param6 ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param6", kodeBilling));
                }

                if (!String.IsNullOrEmpty(ntpn))
                {
                    query += " AND PN.NTPN = :param7 ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param7", ntpn));
                }

                query += "  GROUP BY pn.kantorid, pn.berkasid, pn.nomorberkas, pn.tahunberkas, pn.kodesatker, pn.namakantor,pn.kodebilling, sm.total_nominal_billing, sr.kode, pn.ntpn, TO_CHAR(SM.TANGGALKODEBILLING,'dd/mm/yyyy'), TO_CHAR(SM.TGL_JAM_PEMBAYARAN,'dd/mm/yyyy'), di.nomor, TO_CHAR(DI.TANGGAL, 'dd/mm/yyyy'), di.besarnya ";

                Regex sWhitespace = new Regex(@"\s+");
                query = sWhitespace.Replace(query, " ");
                var parameters = lstparams.ToArray();
                _dtStatistikNTPNDetail = ctx.Database.SqlQuery<Entities.StatistikNTPNDetail>(query, parameters).ToList<Entities.StatistikNTPNDetail>();
            }

            return _dtStatistikNTPNDetail;
        }

        public List<Entities.ModalStatistikNTPN> dtModalStatistikNTPNDetail(string kodeBillings)
        {
            var _dtStatistikNTPNDetail = new List<Entities.ModalStatistikNTPN>();
            //List<object> lstparams = new List<object>();

            using (var ctx = new PnbpContext())
            {
                string query =
                 @" 
                    SELECT
	                    dt.*
                    FROM
	                    (
		                    SELECT
			                    pn.ntpn,
			                    pn.kodebilling,
                                TO_CHAR(pn.tanggal,'dd/mm/yyyy') as tanggal,
                                pn.namakantor,
                                pn.namaprosedur,
                                sm.nama_wajib_bayar,
                                di.besarnya as jumlahdi305,
                                pn.jenispenerimaan,
                                pn.nomorberkas,
                                pn.tahun,
                                sm.ntb,
                                sts.tipe,
                                di.nomor,
                                TO_CHAR(di.tanggal,'dd/mm/yyyy') AS tgl305
		                    FROM
			                    rekappenerimaandetail PN
		                    LEFT JOIN KKPWEB.BERKASSIMPONI SM ON PN.KODEBILLING = SM.KODEBILLING
                            LEFT JOIN KKPWEB.BERKAS BR ON BR.BERKASID = SM.BERKASID
                            LEFT JOIN KKPWEB.TIPESTATUSBERKAS STS ON STS.TIPESTATUSBERKASID = BR.STATUSBERKAS
		                    LEFT JOIN KKPWEB.DI305 DI ON SM.DI305ID = DI.DI305ID
		                    WHERE
			                    PN.KODEBILLING = :param1
	                    ) dt
                 ";
                //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", kodeBillings));

                Regex sWhitespace = new Regex(@"\s+");
                query = sWhitespace.Replace(query, " ");

                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", (String.IsNullOrEmpty(kodeBillings)) ? "" : kodeBillings);

                object[] parameters = new object[1] { p1 };
                _dtStatistikNTPNDetail = ctx.Database.SqlQuery<Entities.ModalStatistikNTPN>(query, parameters).ToList<Entities.ModalStatistikNTPN>();
            }

            return _dtStatistikNTPNDetail;
        }

        public List<Entities.DetailNTPN> dtDetailNTPN(string pKantorid, string pTahun, string pBulan, int from, int to)
        {
            var _dtDetailNTPN = new List<Entities.DetailNTPN>();

            //            using (var ctx = new PnbpContext())
            //            {
            //                string query =
            //                 @" SELECT
            //                      dt.*
            //                    FROM
            //                      (select
            //                          namakantor, nomorberkas, tahunberkas, namaprosedur, namawajibbayar, jenispenerimaan, jenisbiaya, kodebilling, ntpn, ntb, jumlah,
            //                          row_number() over(order by tanggal) as rnumber
            //                        FROM
            //                          penerimaan
            //                        WHERE
            //                          TRUNC(tanggal)> TRUNC(SYSDATE)-30
            //                          and kantorid = :param1) dt
            //                    WHERE
            //                      RNUMBER BETWEEN :param2 AND :param3 ";

            //                Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pKantorid);
            //                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", from);
            //                Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", to);

            //                object[] parameters = new object[2] { p1, p2 };

            //                _dtDetailNTPN = ctx.DetailNTPNs.SqlQuery(query, parameters).ToList<Entities.DetailNTPN>();
            //            }

            return _dtDetailNTPN;
        }

        public List<Entities.berkaspenerimaanls> dtBerkasPenerimaan(string pKodeBilling, string pNTPN)
        {
            var _result = new List<Entities.berkaspenerimaanls>();
            using (var ctx = new PnbpContext())
            {
                string query =
                @" select
                      pn.berkasid, pn.ntpn, pn.kodebilling, to_char(pn.tanggal,'dd/mm/yyyy') as tglpenerimaan,
                      pn.namakantor, pn.namaprosedur, pn.namawajibbayar,
                      sum(pn.jumlah) as totalbiaya,
                      listagg(pn.jenisbiaya || ' - Rp' || pn.jumlah, '; ') within group (order by pn.tipepenerimaanid) as detailpenerimaan
                    from
                      penerimaan pn
                    where
                      pn.kodebilling = :param1
                      or pn.ntpn = :param2
                    group by
                      pn.berkasid, pn.ntpn, pn.kodebilling, pn.tanggal, pn.namakantor, pn.namaprosedur, pn.namawajibbayar ";
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", (String.IsNullOrEmpty(pKodeBilling)) ? "kosong" : pKodeBilling);
                Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", (String.IsNullOrEmpty(pNTPN)) ? "kosong" : pNTPN);
                object[] parameters = new object[2] { p1, p2 };
                _result = ctx.Database.SqlQuery<Entities.berkaspenerimaanls>(query, parameters).ToList<Entities.berkaspenerimaanls>();

            }
            return _result;
        }

        public List<Entities.statusberkasls> dtStatusBerkas(string pBerkasId)
        {
            var _result = new List<Entities.statusberkasls>();
            using (var ctx = new PnbpContext())
            {
                string query =
                @"  SELECT
                      berkasid, nomor, tahun, nomorkanwil, tahunkanwil, nomorpusat, tahunpusat,
                      kantor.tipekantorid tipekantor, kantor.nama kantor,
                      kantorwilayah.tipekantorid tipekantorwilayah,
                      nvl(kantorwilayah.nama,'-') kantorwilayah, kantortujuan.tipekantorid tipekantortujuan,
                      nvl(kantortujuan.nama,'-') kantortujuan,
                      DECODE(berkas.statusberkas, '1', 'DALAM PROSES', DECODE(berkas.statusberkas, '0', 'SELESAI', DECODE(berkas.statusberkas, '2', 'DIBLOKIR', DECODE(berkas.statusberkas, '3', 'DIBATALKAN', DECODE(berkas.statusberkas, '4', 'DITUTUP', DECODE(berkas.statusberkas, '8', 'SELESAI',DECODE(berkas.statusberkas, '9', 'TAMBAH BIAYA','-'))))))) AS STATUSBERKAS
                    FROM
                      berkas, kantor, kantor kantorwilayah, kantor kantortujuan
                    WHERE
                      kantor.kantorid(+) = berkas.kantorid
                      AND kantorwilayah.kantorid(+) = berkas.kantoridkanwil
                      AND kantortujuan.kantorid(+) = berkas.kantoridtujuan
                      AND berkasid = :param1 ";
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pBerkasId);
                object[] parameters = new object[1] { p1 };
                _result = ctx.Database.SqlQuery<Entities.statusberkasls>(query, parameters).ToList<Entities.statusberkasls>();

            }
            return _result;
        }

        public List<Entities.StatistikPenerimaan> GetPenerimaanNasional(string pTahun, string pBulan, string pTipeKantorId, string pKantorId)
        {
            List<Entities.StatistikPenerimaan> _list = new List<Entities.StatistikPenerimaan>();
            List<object> lstparams = new List<object>();

            string query =
                @" select kanwilid, kantorid, kodesatker, nama, sum(penerimaan) as penerimaan, round(sum(operasional),2) as operasional, row_number() over (order by sum(penerimaan) desc) as urutan
                   from rekappenerimaankanwil r1 where r1.tahun = :param1 ";
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pTahun));

            if (!String.IsNullOrEmpty(pBulan))
            {
                query += " and r1.bulan = :param2 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pBulan));
            }

            if (pTipeKantorId == "2" || pTipeKantorId == "3" || pTipeKantorId == "4")
            {
                if (!string.IsNullOrEmpty(pKantorId))
                {
                    query += " and r1.kantorid IN (SELECT kantorid FROM kantor START WITH kantorid = :param3 CONNECT BY NOCYCLE PRIOR kantorid = induk) ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pKantorId));
                }
            }

            query += " group by kanwilid, kantorid, kodesatker, nama ";

            using (var ctx = new PnbpContext())
            {
                var parameters = lstparams.ToArray();
                _list = ctx.Database.SqlQuery<Entities.StatistikPenerimaan>(query, parameters).ToList<Entities.StatistikPenerimaan>();
            }

            return _list;
        }

        public List<Entities.StatistikPenerimaan> GetPenerimaanSatker(string id, string pTahun, string pBulan)
        {
            List<Entities.StatistikPenerimaan> _list = new List<Entities.StatistikPenerimaan>();
            List<object> lstparams = new List<object>();

            string query =
                @"  select kantorid, nama, jenispenerimaan, kodepenerimaan, sum(penerimaan) as penerimaan, round(sum(operasional),2) as operasional, row_number() over (order by sum(penerimaan) desc) as urutan
                    from rekappenerimaansatker r1  where kantorid = :param1 and tahun = :param2 ";
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", id));
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pTahun));

            if (!String.IsNullOrEmpty(pBulan))
            {
                query += " and r1.bulan = :param3 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pBulan));
            }

            query += " group by kantorid, nama, jenispenerimaan, kodepenerimaan ";

            using (var ctx = new PnbpContext())
            {
                var parameters = lstparams.ToArray();
                _list = ctx.Database.SqlQuery<Entities.StatistikPenerimaan>(query, parameters).ToList<Entities.StatistikPenerimaan>();
            }

            return _list;
        }

        public List<Entities.DaftarRekapPenerimaanDetail> GetRealisasiPenerimaan(string pTahun, string pBulan, string propinsi, string satker, string pTipeKantorId, string pKantorId)
        {
            List<Entities.DaftarRekapPenerimaanDetail> _list = new List<Entities.DaftarRekapPenerimaanDetail>();
            List<object> lstparams = new List<object>();

            string query =
                @" select 
                        c.kantorid, 
                        c.nama_satker, 
                        c.kodesatker,
                        nvl(sum(a.penerimaan),0) as penerimaan, 
                        nvl(round(sum(a.operasional),2),0) as operasional, 
                        row_number() over (order by sum(a.penerimaan) desc) as urutan
                   from rekappenerimaandetail a 
                   LEFT JOIN TARGETPROSEDUR b on a.NAMAPROSEDUR = b.NAMAPROSEDUR AND A .KANTORID = b.KANTORID
                   LEFT JOIN SATKER c ON A.KANTORID = C.KANTORID
                    where a.tahun = :param1 AND c.KODESATKER IS NOT NULL";
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pTahun));
            //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", satker));


            if (!String.IsNullOrEmpty(pBulan))
            {
                query += " and a.bulan = :param2 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pBulan));
            }

            if (!String.IsNullOrEmpty(propinsi))
            {
                query += " and c.induk= :param5 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param5", propinsi));
            }

            if (!String.IsNullOrEmpty(satker))
            {
                query += " and a.kantorid = :param4 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", satker));
            }

            if (pTipeKantorId == "2" || pTipeKantorId == "3" || pTipeKantorId == "4")
            {
                if (!string.IsNullOrEmpty(pKantorId))
                {
                    query += " and a.kantorid IN (SELECT kantorid FROM kantor START WITH kantorid = :param3 CONNECT BY NOCYCLE PRIOR kantorid = induk) ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pKantorId));
                }
            }

            query += " group by c.kantorid, c.kodesatker, c.nama_satker, c.kodesatker ";

            using (var ctx = new PnbpContext())
            {
                var parameters = lstparams.ToArray();
                _list = ctx.Database.SqlQuery<Entities.DaftarRekapPenerimaanDetail>(query, parameters).ToList<Entities.DaftarRekapPenerimaanDetail>();
            }

            return _list;
        }

          
        public struct RealisasiPenerimaanDT
        {
            public List<Entities.DaftarRekapPenerimaanDetail> daftarRekapan { get; set; }
            public int RecordsFiltered { get; set; }
            public int RecordsTotal { get; set; }
        }
        public RealisasiPenerimaanDT GetRealisasiPenerimaanDT(string pTahun, string pBulan, string propinsi, string satker, string pTipeKantorId, string pKantorId, int start, int length)
        {
            List<Entities.DaftarRekapPenerimaanDetail> _list = new List<Entities.DaftarRekapPenerimaanDetail>();
            List<object> lstparams = new List<object>();
            var count = 0;
            string query =
                @" select 
                       {0}
                   from rekappenerimaandetail a 
                   LEFT JOIN TARGETPROSEDUR b on a.NAMAPROSEDUR = b.NAMAPROSEDUR AND A .KANTORID = b.KANTORID
                   LEFT JOIN SATKER c ON A.KANTORID = C.KANTORID
                    where a.tahun = :param1 AND c.KODESATKER IS NOT NULL";
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pTahun));
            //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", satker));


            if (!String.IsNullOrEmpty(pBulan))
            {
                query += " and a.bulan = :param2 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pBulan));
            }

            if (!String.IsNullOrEmpty(propinsi))
            {
                query += " and c.induk= :param5 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param5", propinsi));
            }

            if (!String.IsNullOrEmpty(satker))
            {
                query += " and a.kantorid = :param4 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", satker));
            }

            if (pTipeKantorId == "2" || pTipeKantorId == "3" || pTipeKantorId == "4")
            {
                if (!string.IsNullOrEmpty(pKantorId))
                {
                    query += " and a.kantorid IN (SELECT kantorid FROM kantor START WITH kantorid = :param3 CONNECT BY NOCYCLE PRIOR kantorid = induk) ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pKantorId));
                }
            }

            query += " group by c.kantorid, c.kodesatker, c.nama_satker";

            var queryCount = string.Format(query, @"COUNT(c.kantorid) OVER() as count");

            query = string.Format(query, @" c.kantorid, 
                        c.nama_satker, 
                        c.kodesatker,
                        nvl(sum(a.penerimaan),0) as penerimaan, 
                        nvl(round(sum(a.operasional),2),0) as operasional, 
                        row_number() over (order by sum(a.penerimaan) desc) as urutan");

            query += string.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", start, length);


            using (var ctx = new PnbpContext())
            {
                var parameters = lstparams.ToArray();
                _list = ctx.Database.SqlQuery<Entities.DaftarRekapPenerimaanDetail>(query, parameters).ToList<Entities.DaftarRekapPenerimaanDetail>();
                count = ctx.Database.SqlQuery<CountResult>(queryCount, parameters).First().Count;

            }

            return new RealisasiPenerimaanDT
            {
                daftarRekapan = _list,
                RecordsFiltered = count,
                RecordsTotal = count
            };
        }

        public List<Entities.RealisasiPenerimaanDetail> GetRealisasiPenerimaanDetail(string id, string pTahun, string pBulan)
        {
            List<Entities.RealisasiPenerimaanDetail> _list = new List<Entities.RealisasiPenerimaanDetail>();
            //List<object> lstparams = new List<object>();

            string query =
                @" SELECT DISTINCT
                    r1.kantorid,
                    r1.berkasid,
                    r1.kodesatker,
	                r1.namakantor,
	                r1.namaprosedur,
	                NVL (r2.TARGETFISIK, 0) AS targetfisik,
	                COUNT (r1.jumlah) AS jumlah,
                    ROUND(NVL(count(r1.JUMLAH)/r2.TARGETFISIK*100,0),2) as persentasefisik,
	                NVL(r2 .nilaitarget, 0) AS nilaitarget,
	                SUM (r1.penerimaan) AS penerimaan,
	                ROUND (SUM(r1.operasional), 2) AS operasional,
                    round(nvl(sum(r1.penerimaan) / r2.nilaitarget * 100,0),2) as persentasepenerimaan,
	                ROW_NUMBER () OVER (

		                ORDER BY
			                SUM (r1.penerimaan) DESC
	                ) AS urutan
                FROM
	                rekappenerimaandetail r1
                LEFT JOIN TARGETPROSEDUR r2 ON r2.KANTORID = r1.KANTORID and r1.namaprosedur = r2.namaprosedur
                    where r1.kantorid = '" + id + "'";

            if (!String.IsNullOrEmpty(pTahun))
            {
                query += " and r1.tahun = " + pTahun + " ";
            }

            if (!String.IsNullOrEmpty(pBulan))
            {
                query += " and r1.bulan = " + pBulan + " ";
                //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pBulan));
            }

            query += " group by r1.kantorid, r1.berkasid, r1.kodesatker, r1.namakantor, r1.namaprosedur, r2.TARGETFISIK, r2.NILAITARGET";

            using (var ctx = new PnbpContext())
            {
                //var parameters = lstparams.ToArray();
                _list = ctx.Database.SqlQuery<Entities.RealisasiPenerimaanDetail>(query).ToList<Entities.RealisasiPenerimaanDetail>();
            }

            return _list;
        }

        public List<Entities.RealisasiPenerimaanDetail> GetRealisasiPenerimaanDetailDT(string id, string pTahun, string pBulan)
        {
            List<Entities.RealisasiPenerimaanDetail> _list = new List<Entities.RealisasiPenerimaanDetail>();
            //List<object> lstparams = new List<object>();

            string query =
                @" SELECT DISTINCT
                    r1.kantorid,
                    r1.berkasid,
                    r1.kodesatker,
	                r1.namakantor,
	                r1.namaprosedur,
	                NVL (r2.TARGETFISIK, 0) AS targetfisik,
	                COUNT (r1.jumlah) AS jumlah,
                    ROUND(NVL(count(r1.JUMLAH)/r2.TARGETFISIK*100,0),2) as persentasefisik,
	                NVL(r2 .nilaitarget, 0) AS nilaitarget,
	                SUM (r1.penerimaan) AS penerimaan,
	                ROUND (SUM(r1.operasional), 2) AS operasional,
                    round(nvl(sum(r1.penerimaan) / r2.nilaitarget * 100,0),2) as persentasepenerimaan,
	                ROW_NUMBER () OVER (

		                ORDER BY
			                SUM (r1.penerimaan) DESC
	                ) AS urutan
                FROM
	                rekappenerimaandetail r1
                LEFT JOIN TARGETPROSEDUR r2 ON r2.KANTORID = r1.KANTORID and r1.namaprosedur = r2.namaprosedur
                    where r1.kantorid = '" + id + "'";

            if (!String.IsNullOrEmpty(pTahun))
            {
                query += " and r1.tahun = " + pTahun + " ";
            }

            if (!String.IsNullOrEmpty(pBulan))
            {
                query += " and r1.bulan = " + pBulan + " ";
                //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pBulan));
            }

            query += " group by r1.kantorid, r1.berkasid, r1.kodesatker, r1.namakantor, r1.namaprosedur, r2.TARGETFISIK, r2.NILAITARGET";

            using (var ctx = new PnbpContext())
            {
                //var parameters = lstparams.ToArray();
                _list = ctx.Database.SqlQuery<Entities.RealisasiPenerimaanDetail>(query).ToList<Entities.RealisasiPenerimaanDetail>();
            }

            return _list;
        }

        public struct RealisasiLayananDT
        {
            public List<Entities.StatistikPenerimaan> ListPenerimaan { get; set; }
            public int RecordsTotal { get; set; }
            public int RecordsFiltered { get; set; }
        }

        public RealisasiLayananDT GetRealisasiLayananDT(string pTahun, string pBulan, string pTipeKantorId, string pKantorId, int start, int length)
        {
            List<Entities.StatistikPenerimaan> _list = new List<Entities.StatistikPenerimaan>();
            List<object> lstparams = new List<object>();
            var count = 0;

            string query =
                @"
                        SELECT 
                           {0}
                        FROM
	                        rekappenerimaandetail r1
                        WHERE r1.TAHUN = :param1 ";
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pTahun));

            if (!String.IsNullOrEmpty(pBulan))
            {
                query += " and r1.bulan = :param2 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pBulan));
            }

            query += " group by r1.namaprosedur";

            var queryCount = String.Format(query, "COUNT(r1.NAMAPROSEDUR) OVER() as count");

            query = string.Format(query, @"
                DISTINCT
	            r1.namaprosedur,
	            COUNT (r1.jumlah) AS jumlah,
	            SUM (r1.penerimaan) AS penerimaan,
	            ROW_NUMBER () OVER (

		            ORDER BY
			            SUM (r1.penerimaan) DESC
	            ) AS urutan
            ");

            query += string.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", start, length);

            using (var ctx = new PnbpContext())
            {
                var parameters = lstparams.ToArray();
                _list = ctx.Database.SqlQuery<Entities.StatistikPenerimaan>(query, parameters).ToList<Entities.StatistikPenerimaan>();
                count = ctx.Database.SqlQuery<CountResult>(queryCount, parameters).FirstOrDefault().Count;
            }

            return new RealisasiLayananDT
            {
                ListPenerimaan = _list,
                RecordsFiltered = count,
                RecordsTotal = count
            };
        }

        public List<Entities.StatistikPenerimaan> GetRealisasiLayanan(string pTahun, string pBulan, string pTipeKantorId, string pKantorId)
        {
            List<Entities.StatistikPenerimaan> _list = new List<Entities.StatistikPenerimaan>();
            List<object> lstparams = new List<object>();

            string query =
                @"
                        SELECT DISTINCT
	                        r1.namaprosedur,
	                        COUNT (r1.jumlah) AS jumlah,
	                        SUM (r1.penerimaan) AS penerimaan,
	                        ROW_NUMBER () OVER (

		                        ORDER BY
			                        SUM (r1.penerimaan) DESC
	                        ) AS urutan
                        FROM
	                        rekappenerimaandetail r1
                        WHERE r1.TAHUN = :param1 ";
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pTahun));
            //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", satker));

            if (!String.IsNullOrEmpty(pBulan))
            {
                query += " and r1.bulan = :param2 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pBulan));
            }

            //if (!String.IsNullOrEmpty(satker))
            //{
            //    query += " and r1.kantorid = :param4 ";
            //    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", satker));
            //}

            //if (pTipeKantorId == "2" || pTipeKantorId == "3" || pTipeKantorId == "4")
            //{
            //    if (!string.IsNullOrEmpty(pKantorId))
            //    {
            //        query += " and r1.kantorid IN (SELECT kantorid FROM kantor START WITH kantorid = :param3 CONNECT BY NOCYCLE PRIOR kantorid = induk) ";
            //        lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pKantorId));
            //    }
            //}

            query += " group by r1.namaprosedur";

            using (var ctx = new PnbpContext())
            {
                var parameters = lstparams.ToArray();
                _list = ctx.Database.SqlQuery<Entities.StatistikPenerimaan>(query, parameters).ToList<Entities.StatistikPenerimaan>();
            }

            return _list;
        }

        public List<Entities.RealisasiLayananDetail> GetRealisasiLayananDetail(string id, string pTahun, string pBulan)
        {
            List<Entities.RealisasiLayananDetail> _list = new List<Entities.RealisasiLayananDetail>();
            List<object> lstparams = new List<object>();

            string query =
                @"  SELECT DISTINCT
	                    r1.namaprosedur,
	                    r1.kodesatker,
	                    r1.namakantor,
                        r1.kantorid,
                        r1.berkasid,
	                    NVL (b.TARGETFISIK, 0) AS targetfisik,
	                    COUNT (r1.jumlah) AS jumlah,
	                    ROUND (
		                    NVL (
			                    COUNT (r1.jumlah) / b.targetfisik * 100,
			                    0
		                    ),
		                    2
	                    ) AS persentasefisik,
	                    NVL (b.nilaitarget, 0) AS targetpenerimaan,
	                    SUM (r1.PENERIMAAN) AS realisasipenerimaan,
	                    ROUND (
		                    NVL (
			                    SUM (r1.penerimaan) / b.nilaitarget * 100,
			                    0
		                    ),
		                    2
	                    ) AS persentasepenerimaan
                    FROM
	                    rekappenerimaandetail r1
                    LEFT JOIN TARGETPROSEDUR b ON R1.KANTORID = b.KANTORID
                    AND R1.NAMAPROSEDUR = b.NAMAPROSEDUR
                    WHERE
	                    r1.namaprosedur = :param1 and r1.tahun = :param2 ";
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", id));
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pTahun));

            if (!String.IsNullOrEmpty(pBulan))
            {
                query += " and r1.bulan = :param3 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pBulan));
            }

            query += " group by r1.namaprosedur, r1.kodesatker, r1.namakantor, r1.kantorid, r1.berkasid, b.targetfisik, b.nilaitarget";

            //return Json(query, JsonRequestBehavior.AllowGet);

            using (var ctx = new PnbpContext())
            {
                var parameters = lstparams.ToArray();
                _list = ctx.Database.SqlQuery<Entities.RealisasiLayananDetail>(query, parameters).ToList<Entities.RealisasiLayananDetail>();
            }

            return _list;
        }

        public struct RealisasiLayananDetailDTResult
        {
            public List<Entities.RealisasiLayananDetail> List { get; set; }
            public int RecordsTotal { get; set; }
            public int RecordsFiltered { get; set; }
        }

        public RealisasiLayananDetailDTResult GetRealisasiLayananDetailDT(string id, string pTahun, string pBulan, int? start, int? length)
        {
            List<Entities.RealisasiLayananDetail> _list = new List<Entities.RealisasiLayananDetail>();
            List<object> lstparams = new List<object>();

            string baseQuery =
                @"  SELECT 
                        {0}
                    FROM
	                    rekappenerimaandetail r1
                    LEFT JOIN TARGETPROSEDUR b ON R1.KANTORID = b.KANTORID
                    AND R1.NAMAPROSEDUR = b.NAMAPROSEDUR
                    WHERE
	                    r1.namaprosedur = :param1 and r1.tahun = :param2 ";

            string querySelect = @"DISTINCT
	                    r1.namaprosedur,
	                    r1.kodesatker,
	                    r1.namakantor,
                        r1.kantorid,
                        r1.berkasid,
	                    NVL (b.TARGETFISIK, 0) AS targetfisik,
	                    COUNT (r1.jumlah) AS jumlah,
	                    ROUND (
		                    NVL (
			                    COUNT (r1.jumlah) / b.targetfisik * 100,
			                    0
		                    ),
		                    2
	                    ) AS persentasefisik,
	                    NVL (b.nilaitarget, 0) AS targetpenerimaan,
	                    SUM (r1.PENERIMAAN) AS realisasipenerimaan,
	                    ROUND (
		                    NVL (
			                    SUM (r1.penerimaan) / b.nilaitarget * 100,
			                    0
		                    ),
		                    2
	                    ) AS persentasepenerimaan";

            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", id));
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pTahun));

            if (!String.IsNullOrEmpty(pBulan))
            {
                baseQuery += " and r1.bulan = :param3 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pBulan));
            }

            baseQuery += " group by r1.namaprosedur, r1.kodesatker, r1.namakantor, r1.kantorid, r1.berkasid, b.targetfisik, b.nilaitarget";

            baseQuery += string.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", start, length);

            string query = String.Format(baseQuery, querySelect);

            string countQuery = String.Format(baseQuery, "COUNT(r1.NAMAPROSEDUR ) OVER() as count");

            var parameters = lstparams.ToArray();
            var count = 0;
            using (var ctx = new PnbpContext())
            {
                count = ctx.Database.SqlQuery<CountResult>(countQuery, parameters).FirstOrDefault().Count;
                _list = ctx.Database.SqlQuery<Entities.RealisasiLayananDetail>(query, parameters).ToList<Entities.RealisasiLayananDetail>();
            }

            return new RealisasiLayananDetailDTResult {
                List = _list,
                RecordsFiltered = count,
                RecordsTotal = count
            };
        }

        public List<Entities.DataRekonsiliasi> dtDatakkprekonsiliasi(string start, string end)
        {
            List<Entities.DataRekonsiliasi> _list = new List<Entities.DataRekonsiliasi>();
            List<object> lstparams = new List<object>();

            string query =
                @"SELECT distinct NTPN, TO_CHAR (TANGGAL, 'dd/mm/yyyy') AS TANGGAL, sum (JUMLAH) AS jumlah FROM rekappenerimaandetail WHERE TANGGAL >= TO_DATE ('" + start + "', 'yyyy/mm/dd') AND TANGGAL <= TO_DATE ('" + end + "', 'yyyy/mm/dd') and rownum < 500";
            //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", start));
            //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", end));

            query += " GROUP BY NTPN, TANGGAL ";
            query += " ORDER BY TANGGAL DESC ";

            //return Json(query, JsonRequestBehavior.AllowGet);

            using (var ctx = new PnbpContext())
            {
                //var parameters = lstparams.ToArray();
                _list = ctx.Database.SqlQuery<Entities.DataRekonsiliasi>(query).ToList<Entities.DataRekonsiliasi>();
            }

            return _list;
        }

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

                if (!string.IsNullOrEmpty(pSatker))
                {
                    query += " and kantorid = :kantorid ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kantorid", pSatker));
                }

                query += "order by bulan ";

                query = sWhitespace.Replace(query, " ");
                var parameters = lstparams.ToArray();
                _result = ctx.Database.SqlQuery<Entities.RekapPenerimaan>(query, parameters).ToList();

            }
            return _result;
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
                if (!string.IsNullOrEmpty(pTipe))
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


        // V2
        public RealisasiPenerimaanDT pn_provinsi(string pTahun, string pBulan, string provinsi, string satker, string pTipeKantorId, string pKantorId, int start, int length)
        {
            List<Entities.DaftarRekapPenerimaanDetail> _list = new List<Entities.DaftarRekapPenerimaanDetail>();
            List<object> lstparams = new List<object>();
            var count = 0;
            string query =
                @" select 
                       {0}
                   from rekappenerimaandetail a 
                   LEFT JOIN TARGETPROSEDUR b on a.NAMAPROSEDUR = b.NAMAPROSEDUR AND A .KANTORID = b.KANTORID
                   LEFT JOIN SATKER c ON A.KANTORID = C.KANTORID
                    JOIN kantor k ON k.kodesatker = c.KODESATKER 
                    JOIN wilayah w ON k.kode = w.kode
                    JOIN wilayah prov ON prov.wilayahid = w.induk
                    where c.tipekantorid = 2 AND a.tahun = :param1 AND c.KODESATKER IS NOT NULL";
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pTahun));
            //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", satker));


            if (!String.IsNullOrEmpty(pBulan))
            {
                query += " and a.bulan = :param2 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pBulan));
            }

            if (!String.IsNullOrEmpty(provinsi))
            {
                query += " and lower(w.nama) like '%'||:param5||'%' ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param5", provinsi.ToLower()));
            }

            //if (!String.IsNullOrEmpty(satker))
            //{
            //    query += " and a.kantorid = :param4 ";
            //    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", satker));
            //}

            //if (pTipeKantorId == "2" || pTipeKantorId == "3" || pTipeKantorId == "4")
            //{
            //    if (!string.IsNullOrEmpty(pKantorId))
            //    {
            //        query += " and a.kantorid IN (SELECT kantorid FROM kantor START WITH kantorid = :param3 CONNECT BY NOCYCLE PRIOR kantorid = induk) ";
            //        lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pKantorId));
            //    }
            //}

            query += " group by w.wilayahid, w.nama, k.kantorid, c.kodesatker";

            //var queryCount = string.Format(query, @"COUNT(c.kantorid) OVER() as count");

            query = string.Format(query, @"
                        k.kantorid kantorid,
                        c.kodesatker kodesatker, 
                        w.nama nama_provinsi,
                        w.wilayahid id_provinsi, 
                        nvl(sum(b.TARGETFISIK), 0) AS targetfisik,
                        nvl(sum(a.penerimaan),0) as penerimaan, 
                        nvl(round(sum(a.operasional),2),0) as operasional ");

            query = string.Format(@"SELECT * FROM (SELECT 
                                        kantorid, 
                                        kodesatker,
                                        id_provinsi, 
                                        nama_provinsi, 
                                        penerimaan, 
                                        operasional, 
                                        targetfisik,
                                        row_number() over(order by nama_provinsi asc) as urutan 
                                    FROM 
                                        ({0})
                                    ) WHERE urutan BETWEEN :startCnt AND :limitCnt", query);

            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", start));
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", length));

            try
            {
                using (var ctx = new PnbpContext())
                {
                    var parameters = lstparams.ToArray();
                    _list = ctx.Database.SqlQuery<Entities.DaftarRekapPenerimaanDetail>(query, parameters).ToList<Entities.DaftarRekapPenerimaanDetail>();
                    //count = ctx.Database.SqlQuery<CountResult>(queryCount, parameters).First().Count;

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return new RealisasiPenerimaanDT
            {
                daftarRekapan = _list,
                RecordsFiltered = count,
                RecordsTotal = count
            };
        }

        public RealisasiPenerimaanDT pn_satker(string pTahun, string pBulan, string propinsi, string satker, string pTipeKantorId, string pKantorId, int start, int length)
        {
            List<Entities.DaftarRekapPenerimaanDetail> _list = new List<Entities.DaftarRekapPenerimaanDetail>();
            List<object> lstparams = new List<object>();
            var count = 0;
            string query =
                @" select 
                       {0}
                   from rekappenerimaandetail a 
                   LEFT JOIN TARGETPROSEDUR b on a.NAMAPROSEDUR = b.NAMAPROSEDUR AND A .KANTORID = b.KANTORID
                   LEFT JOIN SATKER c ON A.KANTORID = C.KANTORID
                    where a.tahun = :param1 AND c.KODESATKER IS NOT NULL";
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pTahun));
            //lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", satker));


            if (!String.IsNullOrEmpty(pBulan))
            {
                query += " and a.bulan = :param2 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pBulan));
            }

            if (!String.IsNullOrEmpty(propinsi))
            {
                query += " and c.induk= :param5 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param5", propinsi));
            }

            if (!String.IsNullOrEmpty(satker))
            {
                query += " and lower(c.nama_satker) like '%'||:param4||'%' ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", satker.ToLower()));
            }

            if (pTipeKantorId == "2" || pTipeKantorId == "3" || pTipeKantorId == "4")
            {
                if (!string.IsNullOrEmpty(pKantorId))
                {
                    query += " and a.kantorid IN (SELECT kantorid FROM kantor START WITH kantorid = :param3 CONNECT BY NOCYCLE PRIOR kantorid = induk) ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pKantorId));
                }
            }

            query += " group by c.kantorid, c.kodesatker, c.nama_satker";

            var queryCount = string.Format(query, @"COUNT(c.kantorid) OVER() as count");

            query = string.Format(query, @" c.kantorid, 
                        c.nama_satker, 
                        c.kodesatker,
                        nvl(sum(a.penerimaan),0) as penerimaan, 
                        nvl(sum(b.TARGETFISIK), 0) AS targetfisik,
                        nvl(round(sum(a.operasional),2),0) as operasional, 
                        COUNT(1) OVER() TOTAL,
                        row_number() over (order by sum(a.penerimaan) desc) as urutan");

            //query += string.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", start, length);

            query = string.Format("SELECT * FROM ({0}) WHERE urutan BETWEEN :startCnt AND :limitCnt", query);

            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", start));
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", length));

            try
            {
                using (var ctx = new PnbpContext())
                {
                    var parameters = lstparams.ToArray();
                    _list = ctx.Database.SqlQuery<Entities.DaftarRekapPenerimaanDetail>(query, parameters).ToList<Entities.DaftarRekapPenerimaanDetail>();
                    //count = ctx.Database.SqlQuery<CountResult>(queryCount, parameters).First().Count;

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return new RealisasiPenerimaanDT
            {
                daftarRekapan = _list,
                RecordsFiltered = count,
                RecordsTotal = count
            };
        }

        public RealisasiLayananDT pn_layanan(string pTahun, string pBulan, string pTipeKantorId, string pKantorId, string pLayanan, int start, int length)
        {
            List<Entities.StatistikPenerimaan> _list = new List<Entities.StatistikPenerimaan>();
            List<object> lstparams = new List<object>();
            var count = 0;

            string query =
                @"
                        SELECT 
                           {0}
                        FROM
	                        rekappenerimaandetail r1
                        LEFT JOIN TARGETPROSEDUR b on r1.NAMAPROSEDUR = b.NAMAPROSEDUR AND r1.KANTORID = b.KANTORID
                        WHERE r1.TAHUN = :param1 ";
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pTahun));

            if (!String.IsNullOrEmpty(pBulan))
            {
                query += " and r1.bulan = :param2 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pBulan));
            }

            if (!String.IsNullOrEmpty(pKantorId))
            {
                query += " and r1.kantorid = :param3 ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pKantorId));
            }

            if (!String.IsNullOrEmpty(pLayanan))
            {
                query += " and lower(r1.namaprosedur) like '%'||:param4||'%' ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param4", pLayanan.ToLower()));
            }

            query += " group by r1.namaprosedur";

            var queryCount = String.Format(query, "COUNT(r1.NAMAPROSEDUR) OVER() as count");

            query = string.Format(query, @"
                DISTINCT
	            r1.namaprosedur,
	            COUNT (r1.jumlah) AS jumlah,
	            SUM (r1.penerimaan) AS penerimaan,
                nvl(sum(b.TARGETFISIK), 0) AS targetfisik,
                COUNT(1) OVER() TOTAL,
	            ROW_NUMBER () OVER (ORDER BY SUM (r1.penerimaan) DESC) AS urutan
            ");

            //query += string.Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", start, length);

            query = string.Format("SELECT * FROM ({0}) WHERE urutan BETWEEN :startCnt AND :limitCnt", query);

            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", start));
            lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", length));

            try
            {
                using (var ctx = new PnbpContext())
                {
                    var parameters = lstparams.ToArray();
                    _list = ctx.Database.SqlQuery<Entities.StatistikPenerimaan>(query, parameters).ToList<Entities.StatistikPenerimaan>();
                    //count = ctx.Database.SqlQuery<CountResult>(queryCount, parameters).FirstOrDefault().Count;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return new RealisasiLayananDT
            {
                ListPenerimaan = _list,
                RecordsFiltered = count,
                RecordsTotal = count
            };
        }


    }
}