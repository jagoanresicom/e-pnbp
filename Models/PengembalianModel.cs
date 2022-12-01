using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using Pnbp.Entities;

namespace Pnbp.Models
{
    public class PengembalianModel
    {
        Regex sWhitespace = new Regex(@"\s+");
        private string _schemaKKP = OtorisasiUser.NamaSkemaKKP;

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

        public string GetNamaKantor(string kantorid)
        {
            string result = "";

            string query =
                "SELECT " +
                "   nama AS NamaKantor " +
                "FROM " +
                "   kantor " +
                "where kantorid = :KantorId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public string NamaLengkapPegawai(string pegawaiid)
        {
            string result = "";

            string query =
                "SELECT " +
                "   UPPER(decode(pegawai.gelardepan, '', '', pegawai.gelardepan || ' ') || " +
                "   decode(pegawai.nama, '', '', pegawai.nama) || " +
                "   decode(pegawai.gelarbelakang, null, '', ', ' || pegawai.gelarbelakang)) AS NAMALENGKAP " +
                "FROM " +
                "   pegawai " +
                "where pegawaiid = :PegawaiId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PegawaiId", pegawaiid));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public List<Entities.ListKantor> GetListKantor()
        {
            var list = new List<Entities.ListKantor>();

            string query =
                "SELECT " +
                "    kantorid AS data, UPPER(nama) AS value " +
                "FROM " +
                "    kantor " +
                "WHERE " +
                "    validsampai IS NULL " +
                "ORDER BY kode";

            using (var ctx = new PnbpContext())
            {
                list = ctx.Database.SqlQuery<Entities.ListKantor>(query).ToList();
            }

            return list;
        }

        public List<Entities.ListBank> GetListBank()
        {
            var list = new List<Entities.ListBank>();

            string query =
                "SELECT DISTINCT " +
                "    nama AS data, nama AS value " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkemaKKP + ".bank " +
                "WHERE " +
                "    status = 1 " +
                "ORDER BY nama";

            using (var ctx = new PnbpContext())
            {
                list = ctx.Database.SqlQuery<Entities.ListBank>(query).ToList();
            }

            return list;
        }


        #region Info Berkas

        public string GetBerkasIdByNomorTahun(string nomor, string tahun, string kantorid)
        {
            string result = "";

            string query = "SELECT berkasid FROM berkas WHERE nomor = :Nomor and tahun = :Tahun and kantorid = :KantorId";

            ArrayList arrayListParameters = new ArrayList
            {
                new Oracle.ManagedDataAccess.Client.OracleParameter("Nomor", nomor),
                new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun),
                new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid)
            };

            using (var ctx = new PnbpContext())

            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public Entities.BerkasKembalian GetBerkasById(string berkasid)
        {
            Entities.BerkasKembalian data = new Entities.BerkasKembalian();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT
                      berkas.berkasid, berkas.namaprosedur, berkas.statusberkas, berkas.namapemilik NamaPemohon,
                      pemilik.pemilikid, pemilik.nik NikPemohon, pemilik.alamat AlamatPemohon,
                      pemilik.email EmailPemohon, pemilik.nomortelepon,
                      to_char(berkassimponi.tanggalkodebilling, 'dd/mm/yyyy HH24:MI', 'nls_date_language=INDONESIAN') TanggalKodeBilling,
                      berkassimponi.kodebilling, berkassimponi.ntpn,
                      to_char(berkassimponi.tgl_jam_pembayaran, 'dd/mm/yyyy HH24:MI', 'nls_date_language=INDONESIAN') TanggalBayar,
                      to_char(berkassimponi.total_nominal_billing) JumlahBayar, bankpersepsi.namabank NamaBankPersepsi
                  FROM
                      berkas
                      JOIN berkassimponi ON berkassimponi.berkasid = berkas.berkasid
                      JOIN pemilik ON pemilik.pemilikid = berkas.pemilikid
                      LEFT JOIN bankpersepsi ON bankpersepsi.bankpersepsiid = berkassimponi.bankpersepsiid
                  WHERE
                      berkas.berkasid = :BerkasId";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("BerkasId", berkasid));

            query = sWhitespace.Replace(query, " ");

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Entities.BerkasKembalian>(query, parameters).FirstOrDefault();
            }

            return data;
        }

        public BerkasKembalian GetBerkasKembalianPnbpByNomorBerkas(string noBerkas)
        {
            BerkasKembalian data = new BerkasKembalian();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT
                      berkasid, nomorberkas
                  FROM
                      berkaskembalian
                  WHERE
                      nomorBerkas = TRIM(:nomorBerkas)";

            arrayListParameters.Add(new OracleParameter("nomorBerkas", noBerkas));

            query = sWhitespace.Replace(query, " ");

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<BerkasKembalian>(query, parameters).FirstOrDefault();
            }

            return data;
        }

        #endregion


        #region PengembalianPnbp

        public List<Entities.PengembalianPnbp> GetPengembalianPnbp(string kantoriduser, string namakantor, string judul, string nomorberkas, string kodebilling, string nptn, string namapemohon, string nikpemohon, string alamatpemohon, string teleponpemohon, string bankpersepsi, int from, int to)
        {
            List<Entities.PengembalianPnbp> records = new List<Entities.PengembalianPnbp>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT * FROM (
                    SELECT
                        ROW_NUMBER() over (ORDER BY kantor.kode, pengembalianpnbp.tanggalpengaju DESC, berkaskembalian.tanggalbayar, berkaskembalian.namapemohon) RNumber,
                        pengembalianpnbp.pengembalianpnbpid, pengembalianpnbp.kantorid, pengembalianpnbp.namakantor,
                        pengembalianpnbp.pegawaiidpengaju, pengembalianpnbp.namapegawaipengaju, 
                        to_char(pengembalianpnbp.tanggalpengaju, 'dd-mm-yyyy') TanggalPengaju,
                        pengembalianpnbp.pegawaiidsetuju, pengembalianpnbp.namapegawaisetuju, 
                        to_char(pengembalianpnbp.tanggalsetuju, 'dd-mm-yyyy') TanggalSetuju,
                        pengembalianpnbp.statussetuju, pengembalianpnbp.judul, 
                        berkaskembalian.berkasid, berkaskembalian.namaprosedur, berkaskembalian.kodebilling,
                        to_char(berkaskembalian.tanggalkodebilling, 'dd-mm-yyyy') TanggalKodeBilling,
                        to_char(berkaskembalian.tanggalbayar, 'dd-mm-yyyy') TanggalBayar,
                        berkaskembalian.ntpn, berkaskembalian.jumlahbayar, berkaskembalian.namabankpersepsi,
                        berkaskembalian.pemilikid, berkaskembalian.namapemohon, berkaskembalian.nikpemohon,
                        berkaskembalian.alamatpemohon, berkaskembalian.emailpemohon, berkaskembalian.nomortelepon,
                        berkaskembalian.nomorberkas, berkaskembalian.nomorrekening, berkaskembalian.namabank,
                        berkaskembalian.namacabang, to_number(berkaskembalian.jumlahbayar) JumlahBayarNumber,
                        COUNT(1) OVER() Total
                    FROM
                        " + System.Web.Mvc.OtorisasiUser.NamaSkema + @".pengembalianpnbp
                        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + @".berkaskembalian ON berkaskembalian.pengembalianpnbpid = pengembalianpnbp.pengembalianpnbpid 
                        JOIN kantor ON kantor.kantorid = pengembalianpnbp.kantorid 
                        AND pengembalianpnbp.kantorid IN (SELECT kantorid FROM kantor START WITH kantorid = :KantorIdUser CONNECT BY NOCYCLE PRIOR kantorid = induk) ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorIdUser", kantoriduser));

            query = sWhitespace.Replace(query, " ");

            if (!String.IsNullOrEmpty(namakantor))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaKantor", String.Concat("%", namakantor.ToLower(), "%")));
                query += " AND LOWER(pengembalianpnbp.nama) LIKE :NamaKantor ";
            }
            if (!String.IsNullOrEmpty(judul))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", String.Concat("%", judul.ToLower(), "%")));
                query += " AND LOWER(pengembalianpnbp.judul) LIKE :Judul ";
            }
            if (!String.IsNullOrEmpty(nomorberkas))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorBerkas", String.Concat("%", nomorberkas.ToLower(), "%")));
                //query +=
                //    " AND EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".berkaskembalian WHERE berkaskembalian.pengembalianpnbpid = pengembalianpnbp.pengembalianpnbpid " +
                //    "             AND LOWER(berkaskembalian.nomorberkas) LIKE :NomorBerkas)";
                query += " AND LOWER(berkaskembalian.nomorberkas) LIKE :NomorBerkas ";
            }
            if (!String.IsNullOrEmpty(kodebilling))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeBilling", String.Concat("%", kodebilling.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.kodebilling) LIKE :KodeBilling ";
            }
            if (!String.IsNullOrEmpty(nptn))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ntpn", String.Concat("%", nptn.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.ntpn) LIKE :Ntpn ";
            }
            if (!String.IsNullOrEmpty(namapemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPemohon", String.Concat("%", namapemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.namapemohon) LIKE :NamaPemohon ";
            }
            if (!String.IsNullOrEmpty(nikpemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NikPemohon", String.Concat("%", nikpemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.nikpemohon) LIKE :NikPemohon ";
            }
            if (!String.IsNullOrEmpty(alamatpemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AlamatPemohon", String.Concat("%", alamatpemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.alamatpemohon) LIKE :AlamatPemohon ";
            }
            if (!String.IsNullOrEmpty(teleponpemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorTelepon", String.Concat("%", teleponpemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.nomortelepon) LIKE :NomorTelepon ";
            }
            if (!String.IsNullOrEmpty(bankpersepsi))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBank", String.Concat("%", bankpersepsi.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.namabank) LIKE :NamaBank ";
            }

            query +=
                " ) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.PengembalianPnbp>(query, parameters).ToList();
            }

            return records;
        }

        public List<Entities.PengembalianPnbpTrain> GetPengembalianPnbpTrain(int tipekantorid,string kantoriduser, string namakantor, string judul, string nomorberkas, string kodebilling, string nptn, string namapemohon, string nikpemohon, string alamatpemohon, string teleponpemohon, string bankpersepsi, string status, string namasatker, string kodesatker, int from, int to)
        {
            List<Entities.PengembalianPnbpTrain> records = new List<Entities.PengembalianPnbpTrain>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
//                kantor.kode,
                @"SELECT * FROM (
                    SELECT
                        ROW_NUMBER() over (ORDER BY pengembalianpnbp.tanggalpengaju DESC, berkaskembalian.tanggalbayar, berkaskembalian.namapemohon) RNumber,
                        pengembalianpnbp.pengembalianpnbpid, pengembalianpnbp.kantorid, pengembalianpnbp.namakantor,
                        pengembalianpnbp.pegawaiidpengaju, pengembalianpnbp.namapegawaipengaju, 
                        to_char(pengembalianpnbp.tanggalpengaju, 'dd-mm-yyyy') TanggalPengaju,
                        pengembalianpnbp.pegawaiidsetuju, pengembalianpnbp.namapegawaisetuju, 
                        to_char(pengembalianpnbp.tanggalsetuju, 'dd-mm-yyyy') TanggalSetuju,
                        pengembalianpnbp.statussetuju, pengembalianpnbp.judul,pengembalianpnbp.StatusPengembalian, 
                        berkaskembalian.berkasid, berkaskembalian.namaprosedur, berkaskembalian.kodebilling,
                        to_char(berkaskembalian.tanggalkodebilling, 'dd-mm-yyyy') TanggalKodeBilling,
                        to_char(berkaskembalian.tanggalbayar, 'dd-mm-yyyy') TanggalBayar,
                        berkaskembalian.ntpn, berkaskembalian.jumlahbayar, berkaskembalian.namabankpersepsi,
                        berkaskembalian.pemilikid, berkaskembalian.namapemohon, berkaskembalian.nikpemohon,
                        berkaskembalian.alamatpemohon, berkaskembalian.emailpemohon, berkaskembalian.nomortelepon,
                        berkaskembalian.nomorberkas, berkaskembalian.nomorrekening, berkaskembalian.namabank,
                        berkaskembalian.namacabang, to_number(berkaskembalian.jumlahbayar) JumlahBayarNumber,
                        berkaskembalian.nomorsurat,berkaskembalian.permohonanpengembalian,
                        satker.kodesatker KodeSatker,
                        satker.nama_satker NamaSatker,
                        COUNT(1) OVER() Total
                    FROM
                        pengembalianpnbp
                        JOIN berkaskembalian ON berkaskembalian.pengembalianpnbpid = pengembalianpnbp.pengembalianpnbpid 
                        JOIN kantor ON kantor.kantorid = pengembalianpnbp.kantorid
                        JOIN satker ON satker.kantorid = pengembalianpnbp.kantorid 
                        AND pengembalianpnbp.kantorid IN (SELECT kantorid FROM kantor START WITH kantorid = :KantorIdUser CONNECT BY NOCYCLE PRIOR kantorid = induk) 
                   WHERE berkaskembalian.nomorsurat IS NOT NULL AND berkaskembalian.permohonanpengembalian IS NOT NULL";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorIdUser", kantoriduser));

            query = sWhitespace.Replace(query, " ");

            if (!String.IsNullOrEmpty(namakantor))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaKantor", String.Concat("%", namakantor.ToLower(), "%")));
                query += " AND LOWER(pengembalianpnbp.nama) LIKE :NamaKantor ";
            }
            if (tipekantorid == 1)
            {
                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeBilling", String.Concat("%", kodebilling.ToLower(), "%")));
                query += " AND pengembalianpnbp.StatusPengembalian != '0' ";
            }
                if (!String.IsNullOrEmpty(judul))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", String.Concat("%", judul.ToLower(), "%")));
                query += " AND LOWER(pengembalianpnbp.judul) LIKE :Judul ";
            }
            if (!String.IsNullOrEmpty(nomorberkas))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorBerkas", String.Concat("%", nomorberkas.ToLower(), "%")));
                //query +=
                //    " AND EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".berkaskembalian WHERE berkaskembalian.pengembalianpnbpid = pengembalianpnbp.pengembalianpnbpid " +
                //    "             AND LOWER(berkaskembalian.nomorberkas) LIKE :NomorBerkas)";
                query += " AND LOWER(berkaskembalian.nomorberkas) LIKE :NomorBerkas ";
            }
            if (!String.IsNullOrEmpty(kodebilling))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeBilling", String.Concat("%", kodebilling.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.kodebilling) LIKE :KodeBilling ";
            }
            if (!String.IsNullOrEmpty(nptn))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ntpn", String.Concat("%", nptn.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.ntpn) LIKE :Ntpn ";
            }
            if (!String.IsNullOrEmpty(namapemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPemohon", String.Concat("%", namapemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.namapemohon) LIKE :NamaPemohon ";
            }
            if (!String.IsNullOrEmpty(nikpemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NikPemohon", String.Concat("%", nikpemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.nikpemohon) LIKE :NikPemohon ";
            }
            if (!String.IsNullOrEmpty(alamatpemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AlamatPemohon", String.Concat("%", alamatpemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.alamatpemohon) LIKE :AlamatPemohon ";
            }
            if (!String.IsNullOrEmpty(teleponpemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorTelepon", String.Concat("%", teleponpemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.nomortelepon) LIKE :NomorTelepon ";
            }
            if (!String.IsNullOrEmpty(bankpersepsi))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBank", String.Concat("%", bankpersepsi.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.namabank) LIKE :NamaBank ";
            }
            if (!String.IsNullOrEmpty(status))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusPengembalian", String.Concat("%", status.ToLower(), "%")));
                query += " AND LOWER(pengembalianpnbp.StatusPengembalian) LIKE :StatusPengembalian ";
            }
            if (!String.IsNullOrEmpty(namasatker))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaSatker", String.Concat("%", namasatker.ToLower(), "%")));
                query += " AND LOWER(satker.nama_satker) LIKE :NamaSatker ";
            }
            if (!String.IsNullOrEmpty(kodesatker))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeSatker", String.Concat("%", kodesatker.ToLower(), "%")));
                query += " AND LOWER(satker.kodesatker) LIKE :KodeSatker ";
            }

            query +=
                " ORDER BY pengembalianpnbp.tanggalpengaju DESC ) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.PengembalianPnbpTrain>(query, parameters).ToList();
            }

            return records;
        }

        public List<Entities.PengembalianPnbpTrain> ListPengembalian(int tipekantorid, string kantoriduser, string namakantor, string judul, string nomorberkas, string kodebilling, string nptn, string namapemohon, string nikpemohon, string alamatpemohon, string teleponpemohon, string bankpersepsi, string status, string namasatker, string kodesatker, int from, int to)
        {
            List<Entities.PengembalianPnbpTrain> records = new List<Entities.PengembalianPnbpTrain>();

            ArrayList arrayListParameters = new ArrayList();

            string queryOrder = @" CASE WHEN pengembalianpnbp.StatusPengembalian = '2' THEN '1' 
                                    WHEN pengembalianpnbp.StatusPengembalian = '4' THEN '2' 
                                    ELSE '99' END statuspengembalianorder ";

            if (tipekantorid != 1) 
            {
                queryOrder = @" CASE WHEN pengembalianpnbp.StatusPengembalian = '3' THEN '1' 
                                WHEN pengembalianpnbp.StatusPengembalian = '0' THEN '2' 
                                WHEN pengembalianpnbp.StatusPengembalian IN (1, 2) THEN '3' 
                                WHEN pengembalianpnbp.StatusPengembalian = '4' THEN '4' 
                                ELSE '99' END statuspengembalianorder ";
            }

            string query =
            ////                kantor.kode,
            //@"SELECT * FROM (
            //    SELECT
            //        ROW_NUMBER() over (ORDER BY pengembalianpnbp.tanggalpengaju DESC, berkaskembalian.tanggalbayar, berkaskembalian.namapemohon) RNumber,
            //        pengembalianpnbp.pengembalianpnbpid, pengembalianpnbp.kantorid, pengembalianpnbp.namakantor,
            //        pengembalianpnbp.pegawaiidpengaju, pengembalianpnbp.namapegawaipengaju, 
            //        to_char(pengembalianpnbp.tanggalpengaju, 'dd-mm-yyyy') TanggalPengaju,
            //        pengembalianpnbp.pegawaiidsetuju, pengembalianpnbp.namapegawaisetuju, 
            //        to_char(pengembalianpnbp.tanggalsetuju, 'dd-mm-yyyy') TanggalSetuju,
            //        pengembalianpnbp.statussetuju, pengembalianpnbp.judul,pengembalianpnbp.StatusPengembalian, 
            //        berkaskembalian.berkasid, berkaskembalian.namaprosedur, berkaskembalian.kodebilling,
            //        to_char(berkaskembalian.tanggalkodebilling, 'dd-mm-yyyy') TanggalKodeBilling,
            //        to_char(berkaskembalian.tanggalbayar, 'dd-mm-yyyy') TanggalBayar,
            //        berkaskembalian.ntpn, berkaskembalian.jumlahbayar, berkaskembalian.namabankpersepsi,
            //        berkaskembalian.pemilikid, berkaskembalian.namapemohon, berkaskembalian.nikpemohon,
            //        berkaskembalian.alamatpemohon, berkaskembalian.emailpemohon, berkaskembalian.nomortelepon,
            //        berkaskembalian.nomorberkas, berkaskembalian.nomorrekening, berkaskembalian.namabank,
            //        berkaskembalian.namacabang, to_number(berkaskembalian.jumlahbayar) JumlahBayarNumber,
            //        berkaskembalian.nomorsurat,berkaskembalian.permohonanpengembalian,
            //        satker.kodesatker KodeSatker,
            //        satker.nama_satker NamaSatker,
            //        COUNT(1) OVER() Total
            //    FROM
            //        pengembalianpnbp
            //        JOIN berkaskembalian ON berkaskembalian.pengembalianpnbpid = pengembalianpnbp.pengembalianpnbpid 
            //        JOIN kantor ON kantor.kantorid = pengembalianpnbp.kantorid
            //        JOIN satker ON satker.kantorid = pengembalianpnbp.kantorid 
            //        AND pengembalianpnbp.kantorid IN (SELECT kantorid FROM kantor START WITH kantorid = :KantorIdUser CONNECT BY NOCYCLE PRIOR kantorid = induk) 
            //   WHERE berkaskembalian.nomorsurat IS NOT NULL AND berkaskembalian.permohonanpengembalian IS NOT NULL";

            $@"SELECT * FROM (SELECT 
                  ROW_NUMBER() over (
                    ORDER BY 
                      statuspengembalianorder, tanggalpengaju DESC
                  ) RNumber,
                  pengembalianpnbpid, 
                  kantorid, 
                  namakantor, 
                  pegawaiidpengaju, 
                  namapegawaipengaju, 
                  TanggalPengaju, 
                  pegawaiidsetuju, 
                  namapegawaisetuju, 
                  TanggalSetuju, 
                  statussetuju, 
                  judul, 
                  StatusPengembalian, 
                  berkasid, 
                  namaprosedur, 
                  kodebilling, 
                  TanggalKodeBilling, 
                  TanggalBayar, 
                  ntpn, 
                  jumlahbayar, 
                  namabankpersepsi, 
                  pemilikid, 
                  namapemohon, 
                  nikpemohon, 
                  alamatpemohon, 
                  emailpemohon, 
                  nomortelepon, 
                  nomorberkas, 
                  nomorrekening, 
                  namabank, 
                  namacabang, 
                  JumlahBayarNumber, 
                  nomorsurat, 
                  permohonanpengembalian, 
                  KodeSatker, 
                  NamaSatker, 
                  nomorsp2d, 
                  labeltipepengembalian, 
                  statuspengembalianorder,
                  Total 
                FROM (
                    SELECT
                        pengembalianpnbp.pengembalianpnbpid, pengembalianpnbp.kantorid, pengembalianpnbp.namakantor,
                        pengembalianpnbp.pegawaiidpengaju, pengembalianpnbp.namapegawaipengaju, 
                        to_char(pengembalianpnbp.tanggalpengaju, 'dd-mm-yyyy') TanggalPengaju,
                        pengembalianpnbp.pegawaiidsetuju, pengembalianpnbp.namapegawaisetuju, 
                        to_char(pengembalianpnbp.tanggalsetuju, 'dd-mm-yyyy') TanggalSetuju,
                        pengembalianpnbp.statussetuju, pengembalianpnbp.judul,pengembalianpnbp.StatusPengembalian, 
                        berkaskembalian.berkasid, berkaskembalian.namaprosedur, berkaskembalian.kodebilling,
                        to_char(berkaskembalian.tanggalkodebilling, 'dd-mm-yyyy') TanggalKodeBilling,
                        to_char(berkaskembalian.tanggalbayar, 'dd-mm-yyyy') TanggalBayar,
                        berkaskembalian.ntpn, berkaskembalian.jumlahbayar, berkaskembalian.namabankpersepsi,
                        berkaskembalian.pemilikid, berkaskembalian.namapemohon, berkaskembalian.nikpemohon,
                        berkaskembalian.alamatpemohon, berkaskembalian.emailpemohon, berkaskembalian.nomortelepon,
                        berkaskembalian.nomorberkas, berkaskembalian.nomorrekening, berkaskembalian.namabank,
                        berkaskembalian.namacabang, to_number(berkaskembalian.jumlahbayar) JumlahBayarNumber,
                        berkaskembalian.nomorsurat,berkaskembalian.permohonanpengembalian,
                        satker.kodesatker KodeSatker,
                        satker.nama_satker NamaSatker,
                        nomorsp2d,
                        CASE WHEN tipepengembalian = '1' THEN 'Daerah' WHEN tipepengembalian = '2' THEN 'Pusat' END labeltipepengembalian,
                        COUNT(1) OVER() Total,
                        {queryOrder}
                    FROM
                        pengembalianpnbp
                        JOIN berkaskembalian ON berkaskembalian.pengembalianpnbpid = pengembalianpnbp.pengembalianpnbpid 
                        JOIN kantor ON kantor.kantorid = pengembalianpnbp.kantorid
                        JOIN satker ON satker.kantorid = pengembalianpnbp.kantorid 
                        AND pengembalianpnbp.kantorid IN (SELECT kantorid FROM kantor START WITH kantorid = :KantorIdUser CONNECT BY NOCYCLE PRIOR kantorid = induk) ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorIdUser", kantoriduser));

            query = sWhitespace.Replace(query, " ");

            if (!String.IsNullOrEmpty(namakantor))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaKantor", String.Concat("%", namakantor.ToLower(), "%")));
                query += " AND LOWER(pengembalianpnbp.nama) LIKE :NamaKantor ";
            }
            if (tipekantorid == 1)
            {
                //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeBilling", String.Concat("%", kodebilling.ToLower(), "%")));
                query += " AND pengembalianpnbp.StatusPengembalian != '0' ";
            }
            if (!String.IsNullOrEmpty(judul))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", String.Concat("%", judul.ToLower(), "%")));
                query += " AND LOWER(pengembalianpnbp.judul) LIKE :Judul ";
            }
            if (!String.IsNullOrEmpty(nomorberkas))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorBerkas", String.Concat("%", nomorberkas.ToLower(), "%")));
                //query +=
                //    " AND EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".berkaskembalian WHERE berkaskembalian.pengembalianpnbpid = pengembalianpnbp.pengembalianpnbpid " +
                //    "             AND LOWER(berkaskembalian.nomorberkas) LIKE :NomorBerkas)";
                query += " AND LOWER(berkaskembalian.nomorberkas) LIKE :NomorBerkas ";
            }
            if (!String.IsNullOrEmpty(kodebilling))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeBilling", String.Concat("%", kodebilling.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.kodebilling) LIKE :KodeBilling ";
            }
            if (!String.IsNullOrEmpty(nptn))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ntpn", String.Concat("%", nptn.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.ntpn) LIKE :Ntpn ";
            }
            if (!String.IsNullOrEmpty(namapemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPemohon", String.Concat("%", namapemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.namapemohon) LIKE :NamaPemohon ";
            }
            if (!String.IsNullOrEmpty(nikpemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NikPemohon", String.Concat("%", nikpemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.nikpemohon) LIKE :NikPemohon ";
            }
            if (!String.IsNullOrEmpty(alamatpemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AlamatPemohon", String.Concat("%", alamatpemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.alamatpemohon) LIKE :AlamatPemohon ";
            }
            if (!String.IsNullOrEmpty(teleponpemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorTelepon", String.Concat("%", teleponpemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.nomortelepon) LIKE :NomorTelepon ";
            }
            if (!String.IsNullOrEmpty(bankpersepsi))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBank", String.Concat("%", bankpersepsi.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.namabank) LIKE :NamaBank ";
            }
            if (!String.IsNullOrEmpty(status))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusPengembalian", String.Concat("%", status.ToLower(), "%")));
                query += " AND LOWER(pengembalianpnbp.StatusPengembalian) LIKE :StatusPengembalian ";
            }
            if (!String.IsNullOrEmpty(namasatker))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaSatker", String.Concat("%", namasatker.ToLower(), "%")));
                query += " AND LOWER(satker.nama_satker) LIKE :NamaSatker ";
            }
            if (!String.IsNullOrEmpty(kodesatker))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeSatker", String.Concat("%", kodesatker.ToLower(), "%")));
                query += " AND LOWER(satker.kodesatker) LIKE :KodeSatker ";
            }

            query +=
                " ORDER BY statuspengembalianorder, pengembalianpnbp.tanggalpengaju DESC)) WHERE RNumber BETWEEN :startCnt AND :limitCnt";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startCnt", from));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limitCnt", to));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.PengembalianPnbpTrain>(query, parameters).ToList();
            }

            return records;
        }

        public int JumlahPengembalianPnbp(string kantoriduser, string namakantor, string judul, string nomorberkas, string kodebilling, string nptn, string namapemohon, string nikpemohon, string alamatpemohon, string teleponpemohon, string bankpersepsi)
        {
            int result = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT
                      COUNT(*)
                  FROM
                      " + System.Web.Mvc.OtorisasiUser.NamaSkema + @".pengembalianpnbp
                      JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + @".berkaskembalian ON berkaskembalian.pengembalianpnbpid = pengembalianpnbp.pengembalianpnbpid 
                      JOIN kantor ON kantor.kantorid = pengembalianpnbp.kantorid 
                      AND pengembalianpnbp.kantorid IN (SELECT kantorid FROM kantor START WITH kantorid = :KantorIdUser CONNECT BY NOCYCLE PRIOR kantorid = induk) ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorIdUser", kantoriduser));

            query = sWhitespace.Replace(query, " ");

            if (!String.IsNullOrEmpty(namakantor))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaKantor", String.Concat("%", namakantor.ToLower(), "%")));
                query += " AND LOWER(pengembalianpnbp.nama) LIKE :NamaKantor ";
            }
            if (!String.IsNullOrEmpty(judul))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", String.Concat("%", judul.ToLower(), "%")));
                query += " AND LOWER(pengembalianpnbp.judul) LIKE :Judul ";
            }
            if (!String.IsNullOrEmpty(nomorberkas))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorBerkas", String.Concat("%", nomorberkas.ToLower(), "%")));
                //query +=
                //    " AND EXISTS (SELECT 1 FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".berkaskembalian WHERE berkaskembalian.pengembalianpnbpid = pengembalianpnbp.pengembalianpnbpid " +
                //    "             AND LOWER(berkaskembalian.nomorberkas) LIKE :NomorBerkas)";
                query += " AND LOWER(berkaskembalian.nomorberkas) LIKE :NomorBerkas ";
            }
            if (!String.IsNullOrEmpty(kodebilling))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeBilling", String.Concat("%", kodebilling.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.kodebilling) LIKE :KodeBilling ";
            }
            if (!String.IsNullOrEmpty(nptn))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ntpn", String.Concat("%", nptn.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.ntpn) LIKE :Ntpn ";
            }
            if (!String.IsNullOrEmpty(namapemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPemohon", String.Concat("%", namapemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.namapemohon) LIKE :NamaPemohon ";
            }
            if (!String.IsNullOrEmpty(nikpemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NikPemohon", String.Concat("%", nikpemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.nikpemohon) LIKE :NikPemohon ";
            }
            if (!String.IsNullOrEmpty(alamatpemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AlamatPemohon", String.Concat("%", alamatpemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.alamatpemohon) LIKE :AlamatPemohon ";
            }
            if (!String.IsNullOrEmpty(teleponpemohon))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorTelepon", String.Concat("%", teleponpemohon.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.nomortelepon) LIKE :NomorTelepon ";
            }
            if (!String.IsNullOrEmpty(bankpersepsi))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBank", String.Concat("%", bankpersepsi.ToLower(), "%")));
                query += " AND LOWER(berkaskembalian.namabank) LIKE :NamaBank ";
            }

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public Entities.DataPengembalianPnbp GetPengembalianPnbpById(string pengembalianpnbpid)
        {
            Entities.DataPengembalianPnbp data = new Entities.DataPengembalianPnbp();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT
                        ROW_NUMBER() over (ORDER BY kantor.kode, pengembalianpnbp.tanggalpengaju DESC, pengembalianpnbp.judul) RNumber,
                        pengembalianpnbp.pengembalianpnbpid, pengembalianpnbp.kantorid, pengembalianpnbp.namakantor,
                        pengembalianpnbp.pegawaiidpengaju, pengembalianpnbp.namapegawaipengaju, 
                        to_char(pengembalianpnbp.tanggalpengaju, 'dd-mm-yyyy') TanggalPengaju,
                        pengembalianpnbp.pegawaiidsetuju, pengembalianpnbp.namapegawaisetuju, 
                        to_char(pengembalianpnbp.tanggalsetuju, 'dd-mm-yyyy') TanggalSetuju,
                        pengembalianpnbp.statussetuju, pengembalianpnbp.judul, 
                        berkaskembalian.berkasid, berkaskembalian.namaprosedur, berkaskembalian.kodebilling,
                        to_char(berkaskembalian.tanggalkodebilling, 'dd-mm-yyyy') TanggalKodeBilling,
                        to_char(berkaskembalian.tanggalbayar, 'dd-mm-yyyy') TanggalBayar,
                        berkaskembalian.ntpn, berkaskembalian.jumlahbayar, berkaskembalian.namabankpersepsi,
                        berkaskembalian.pemilikid, berkaskembalian.namapemohon, berkaskembalian.nikpemohon,
                        berkaskembalian.alamatpemohon, berkaskembalian.emailpemohon, berkaskembalian.nomortelepon,
                        berkaskembalian.nomorberkas, berkaskembalian.nomorrekening, berkaskembalian.namabank,
                        berkaskembalian.namacabang, 
                        COUNT(1) OVER() Total
                    FROM
                        " + System.Web.Mvc.OtorisasiUser.NamaSkema + @".pengembalianpnbp
                        JOIN " + System.Web.Mvc.OtorisasiUser.NamaSkema + @".berkaskembalian ON berkaskembalian.pengembalianpnbpid = pengembalianpnbp.pengembalianpnbpid 
                        JOIN kantor ON kantor.kantorid = pengembalianpnbp.kantorid 
                    WHERE pengembalianpnbp.pengembalianpnbpid = :PengembalianPnbpId";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", pengembalianpnbpid));

            query = sWhitespace.Replace(query, " ");

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Entities.DataPengembalianPnbp>(query, parameters).FirstOrDefault();
            }

            return data;
        }



        public Entities.TransactionResult SimpanPengembalianPnbp(Entities.DataPengembalianPnbp data)
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
                        object[] parameters = null;

                        //string namapegawai = NamaLengkapPegawai(data.PegawaiId);

                        if (string.IsNullOrEmpty(data.PengembalianPnbpId))
                        {
                            string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();

                            string namakantor = GetNamaKantor(data.KantorId);

                            data.PengembalianPnbpId = id;

                            // Insert PENGEMBALIANPNBP
                            sql =
                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pengembalianpnbp ( " +
                                "            pengembalianpnbpid, tanggalpengaju, kantorid, namakantor, " +
                                "            pegawaiidpengaju, namapegawaipengaju) VALUES " +
                                "( " +
                                "            :PengembalianPnbpId, TO_DATE(:TanggalPengaju,'DD/MM/YYYY'), :KantorId, :NamaKantor, " +
                                "            :PegawaiIdPengaju, :NamaPegawaiPengaju)";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PengembalianPnbpId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalPengaju", data.TanggalPengaju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", data.KantorId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaKantor", data.NamaKantor));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PegawaiIdPengaju", data.PegawaiIdPengaju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawaiPengaju", data.NamaPegawaiPengaju));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            // Insert BERKASKEMBALIAN
                            sql =
                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".berkaskembalian ( " +
                                "            berkasid, pengembalianpnbpid, namaprosedur, kodebilling, tanggalkodebilling, " +
                                "            ntpn, tanggalbayar, jumlahbayar, namabankpersepsi, pemilikid, namapemohon, " +
                                "            nikpemohon, alamatpemohon, emailpemohon, nomortelepon, nomorberkas, " +
                                "            nomorrekening, namabank, namacabang) VALUES " +
                                "( " +
                                "            :BerkasId, :PengembalianPnbpId, :NamaProsedur, :KodeBilling, TO_DATE(:TanggalKodeBilling,'DD/MM/YYYY HH24:MI'), " +
                                "            :Ntpn, TO_DATE(:TanggalBayar,'DD/MM/YYYY HH24:MI'), :JumlahBayar, :NamaBankPersepsi, :PemilikId, :NamaPemohon, " +
                                "            :NikPemohon, :AlamatPemohon, :EmailPemohon, :NomorTelepon, :NomorBerkas, " +
                                "            :NomorRekening, :NamaBank, :NamaCabang)";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("BerkasId", data.BerkasId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PengembalianPnbpId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaProsedur", data.NamaProsedur));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeBilling", data.KodeBilling));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalKodeBilling", data.TanggalKodeBilling));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ntpn", data.Ntpn));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalBayar", data.TanggalBayar));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JumlahBayar", data.JumlahBayar));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBankPersepsi", data.NamaBankPersepsi));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PemilikId", data.PemilikId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPemohon", data.NamaPemohon));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NikPemohon", data.NikPemohon));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AlamatPemohon", data.AlamatPemohon));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("EmailPemohon", data.EmailPemohon));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorTelepon", data.NomorTelepon));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorBerkas", data.NomorBerkas));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorRekening", data.NomorRekening));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBank", data.NamaBank));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaCabang", data.NamaCabang));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else
                        {
                            // Update PENGEMBALIANPNBP
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pengembalianpnbp SET " +
                                "       tanggalpengaju = TO_DATE(:TanggalPengaju,'DD/MM/YYYY'), " +
                                "       pegawaiidpengaju = :PegawaiIdPengaju, " +
                                "       namapegawaipengaju = :NamaPegawaiPengaju " +
                                "WHERE pengembalianpnbpid = :PengembalianPnbpId";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalPengaju", data.TanggalPengaju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PegawaiIdPengaju", data.PegawaiIdPengaju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawaiPengaju", data.NamaPegawaiPengaju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PengembalianPnbpId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            // Update BERKASKEMBALIAN
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".berkaskembalian SET " +
                                "       nomorrekening = :NomorRekening, " +
                                "       namabank = :NamaBank, " +
                                "       namacabang = :NamaCabang " +
                                "WHERE pengembalianpnbpid = :PengembalianPnbpId";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorRekening", data.NomorRekening));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBank", data.NamaBank));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaCabang", data.NamaCabang));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PengembalianPnbpId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.ReturnValue = data.PengembalianPnbpId;
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

        public Entities.TransactionResult SimpanPengembalianPnbpDev(Entities.DataPengembalianPnbpDev data)
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
                        object[] parameters = null;

                        //string namapegawai = NamaLengkapPegawai(data.PegawaiId);

                        if (string.IsNullOrEmpty(data.PengembalianPnbpId))
                        {
                            string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();

                            string namakantor = GetNamaKantor(data.KantorId);

                            data.PengembalianPnbpId = id;

                            // Insert PENGEMBALIANPNBP
                            sql =
                                "INSERT INTO pengembalianpnbp ( " +
                                "            pengembalianpnbpid, tanggalpengaju, kantorid, namakantor, " +
                                "            pegawaiidpengaju, namapegawaipengaju) VALUES " +
                                "( " +
                                "            :PengembalianPnbpId, TO_DATE(:sysdate,'DD/MM/YYYY'), :KantorId, :NamaKantor, " +
                                "            :PegawaiIdPengaju, :NamaPegawaiPengaju)";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PengembalianPnbpId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalPengaju", data.TanggalPengaju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", data.KantorId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaKantor", data.NamaKantor));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PegawaiIdPengaju", data.PegawaiIdPengaju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawaiPengaju", data.NamaPegawaiPengaju));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            // Insert BERKASKEMBALIAN
                            sql =
                                "INSERT INTO berkaskembalian ( " +
                                "            berkasid, pengembalianpnbpid, namaprosedur, kodebilling, tanggalkodebilling, " +
                                "            ntpn, tanggalbayar, jumlahbayar, namabankpersepsi, pemilikid, namapemohon, " +
                                "            nikpemohon, alamatpemohon, emailpemohon, nomortelepon, nomorberkas, " +
                                "            nomorrekening, namabank, namacabang) VALUES " +
                                "( " +
                                "            :BerkasId, :PengembalianPnbpId, :NamaProsedur, :KodeBilling, TO_DATE(:TanggalKodeBilling,'DD/MM/YYYY HH24:MI'), " +
                                "            :Ntpn, TO_DATE(:TanggalBayar,'DD/MM/YYYY HH24:MI'), :JumlahBayar, :NamaBankPersepsi, :PemilikId, :NamaPemohon, " +
                                "            :NikPemohon, :AlamatPemohon, :EmailPemohon, :NomorTelepon, :NomorBerkas, " +
                                "            :NomorRekening, :NamaBank, :NamaCabang)";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("BerkasId", data.BerkasId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PengembalianPnbpId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaProsedur", data.NamaProsedur));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeBilling", data.KodeBilling));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalKodeBilling", data.TanggalKodeBilling));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ntpn", data.Ntpn));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalBayar", data.TanggalBayar));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JumlahBayar", data.JumlahBayar));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBankPersepsi", data.NamaBankPersepsi));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PemilikId", data.PemilikId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPemohon", data.NamaPemohon));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NikPemohon", data.NikPemohon));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AlamatPemohon", data.AlamatPemohon));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("EmailPemohon", data.EmailPemohon));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorTelepon", data.NomorTelepon));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorBerkas", data.NomorBerkas));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorRekening", data.NomorRekening));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBank", data.NamaBank));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaCabang", data.NamaCabang));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Npwp", data.Npwp));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaRekening", data.NamaRekening));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SetoranPnbp", data.SetoranPnbp));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PermohonanPengembalian", data.PermohonanPengembalian));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SetoranPnbp", data.SetoranPnbp));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else
                        {
                            // Update PENGEMBALIANPNBP
                            sql =
                                "UPDATE pengembalianpnbp SET " +
                                "       tanggalpengaju = TO_DATE(:TanggalPengaju,'DD/MM/YYYY'), " +
                                "       pegawaiidpengaju = :PegawaiIdPengaju, " +
                                "       namapegawaipengaju = :NamaPegawaiPengaju " +
                                "WHERE pengembalianpnbpid = :PengembalianPnbpId";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalPengaju", data.TanggalPengaju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PegawaiIdPengaju", data.PegawaiIdPengaju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawaiPengaju", data.NamaPegawaiPengaju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PengembalianPnbpId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            // Update BERKASKEMBALIAN
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".berkaskembalian SET " +
                                "       nomorrekening = :NomorRekening, " +
                                "       namabank = :NamaBank, " +
                                "       namacabang = :NamaCabang " +
                                "WHERE pengembalianpnbpid = :PengembalianPnbpId";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorRekening", data.NomorRekening));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBank", data.NamaBank));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaCabang", data.NamaCabang));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PengembalianPnbpId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.ReturnValue = data.PengembalianPnbpId;
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

        public Entities.TransactionResult InsertPengembalianDaerah(Entities.DetailDataBerkas data, string userid, string kantoriduser, string pegawaiid, string namapegawai, string npwpberkas, string statusPengembalian, string statusSimpan, string nomorTelepon)
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
                        object[] parameters = null;

                        //string namapegawai = NamaLengkapPegawai(data.PegawaiId);

                        string pengembalianPnbpId = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();

                        string namakantor = GetNamaKantor(kantoriduser);

                        // Insert PENGEMBALIANPNBP
                        sql =
                            "INSERT INTO pengembalianpnbp ( " +
                            "            pengembalianpnbpid, tanggalpengaju, kantorid, namakantor, " +
                            "            pegawaiidpengaju, namapegawaipengaju, tipepengembalian, statuspengembalian, statussimpan, nomorTelepon) VALUES " +
                            "( " +
                            "            :PengembalianPnbpId, TO_DATE(SYSDATE,'DD/MM/YYYY'), :KantorId, :NamaKantor, " +
                            "            :PegawaiIdPengaju, :NamaPegawaiPengaju, '1', :statusPengembalian, :statusSimpan, :nomorTelepon)";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", pengembalianPnbpId));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalPengaju", data.TANGGALPENGAJU));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantoriduser));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaKantor", namakantor));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PegawaiIdPengaju", pegawaiid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawaiPengaju", namapegawai));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("statusPengembalian", statusPengembalian));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("statusSimpan", statusSimpan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nomorTelepon", nomorTelepon));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        // Insert BERKASKEMBALIAN
                        sql =
                            "INSERT INTO berkaskembalian ( " +
                            "            berkasid, pengembalianpnbpid, namaprosedur, kodebilling, tanggalkodebilling, " +
                            "            ntpn, tanggalbayar, jumlahbayar, namabankpersepsi, pemilikid, namapemohon, " +
                            "            nikpemohon, alamatpemohon, emailpemohon, nomortelepon, nomorberkas, " +
                            "            nomorrekening, namabank, namacabang, setoranpnbp, npwp, namarekening, permohonanpengembalian) VALUES " +
                            "( " +
                            "            RAWTOHEX(SYS_GUID()), :PengembalianPnbpId, :NamaProsedur, :KodeBilling, TO_DATE(:TanggalKodeBilling,'DD/MM/YYYY HH24:MI'), " +
                            "            :Ntpn, TO_DATE(:TanggalBayar,'DD/MM/YYYY HH24:MI'), :JumlahBayar, :NamaBankPersepsi, :PemilikId, :NamaPemohon, " +
                            "            :NikPemohon, :AlamatPemohon, :EmailPemohon, :NomorTelepon, :NomorBerkas, " +
                            "            :NomorRekening, :NamaBank, :NamaCabang, :setoranpnbp, :npwp, :namarekening, :permohonanpengembalian)";
                        arrayListParameters.Clear();
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("BerkasId", data.BERKASID));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", pengembalianPnbpId));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaProsedur", data.NamaProsedur));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaProsedur", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeBilling", data.KODEBILLING));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalKodeBilling", data.TanggalKodeBilling));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalKodeBilling", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ntpn", data.NTPN));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalBayar", data.TanggalBayar));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalBayar", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JumlahBayar", data.JUMLAHBAYAR));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBankPersepsi", data.NamaBankPersepsi));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBankPersepsi", ""));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PemilikId", data.PemilikId));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PemilikId", ""));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPemohon", data.NamaPemohon));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPemohon", data.NAMAPEMOHON));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NikPemohon", data.NikPemohon));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NikPemohon", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AlamatPemohon", data.ALAMATPEMOHON));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("EmailPemohon", data.EmailPemohon));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("EmailPemohon", ""));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorTelepon", data.NomorTelepon));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorTelepon", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorBerkas", data.NOMORBERKAS));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorRekening", data.NOMORREKENING));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBank", data.NAMABANK));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaCabang", data.NamaCabang));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaCabang", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SetoranPnbp", data.SETORANPNBP));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Npwp", npwpberkas));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaRekening", data.NAMAREKENING));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PermohonanPengembalian", data.PERMOHONANPENGEMBALIAN));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PermohonanPengembalian", ""));
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SetoranPnbp", data.SETORANPNBP));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.ReturnValue = pengembalianPnbpId;
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

        public Entities.TransactionResult UpdatePengembalianDaerah(Entities.DetailDataBerkas data, string userid, string kantoriduser, string pegawaiid, string namapegawai, string npwpberkas, string statusPengembalian, string statusSimpan, string nomorTelepon)
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
                        object[] parameters = null;

                        string namakantor = GetNamaKantor(kantoriduser);

                        sql =
                            "UPDATE pengembalianpnbp " +
                            "            SET kantorid = :KantorId, namakantor = :NamaKantor, " +
                            "            pegawaiidpengaju = :PegawaiIdPengaju, namapegawaipengaju = :NamaPegawaiPengaju, tipepengembalian = '1', statuspengembalian = :statusPengembalian, statussimpan =: statusSimpan, nomortelepon = :nomorTelepon WHERE pengembalianpnbpid = :PengembalianPnbpId";

                        statusSimpan = (statusPengembalian == "2" ? null : statusSimpan);

                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantoriduser));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaKantor", namakantor));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PegawaiIdPengaju", pegawaiid));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawaiPengaju", namapegawai));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("statusPengembalian", statusPengembalian));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("statusSimpan", statusSimpan));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nomorTelepon", nomorTelepon));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PENGEMBALIANPNBPID));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        // Insert BERKASKEMBALIAN
                        sql =
                            @"UPDATE berkaskembalian  
                                SET 
                                    namaprosedur = :NamaProsedur, 
                                    kodebilling = :KodeBilling, 
                                    tanggalkodebilling = TO_DATE(:TanggalKodeBilling,'DD/MM/YYYY HH24:MI'), 
                                    ntpn = :Ntpn, 
                                    tanggalbayar = TO_DATE(:TanggalBayar,'DD/MM/YYYY HH24:MI'), 
                                    jumlahbayar = :JumlahBayar, 
                                    namabankpersepsi = :NamaBankPersepsi, 
                                    pemilikid = :PemilikId, 
                                    namapemohon = :NamaPemohon,            
                                    nikpemohon = :NikPemohon, 
                                    alamatpemohon = :AlamatPemohon, 
                                    emailpemohon = :EmailPemohon, 
                                    nomortelepon = :NomorTelepon, 
                                    nomorrekening = :NomorRekening, 
                                    namabank = :NamaBank, 
                                    namacabang = :NamaCabang, 
                                    setoranpnbp = :SetoranPnbp, 
                                    npwp = :Npwp, 
                                    namarekening = :NamaRekening, 
                                    permohonanpengembalian = :PermohonanPengembalian 
                                WHERE 
                                    pengembalianpnbpid = :PengembalianPnbpId";

                        arrayListParameters.Clear();
                        //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("BerkasId", data.BERKASID));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaProsedur", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KodeBilling", data.KODEBILLING));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalKodeBilling", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ntpn", data.NTPN));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalBayar", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JumlahBayar", data.JUMLAHBAYAR));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBankPersepsi", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PemilikId", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPemohon", data.NAMAPEMOHON));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NikPemohon", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("AlamatPemohon", data.ALAMATPEMOHON));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("EmailPemohon", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorTelepon", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorRekening", data.NOMORREKENING));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaBank", data.NAMABANK));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaCabang", ""));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("SetoranPnbp", data.SETORANPNBP));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Npwp", npwpberkas));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaRekening", data.NAMAREKENING));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PermohonanPengembalian", data.PERMOHONANPENGEMBALIAN));
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PENGEMBALIANPNBPID));

                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.ReturnValue = data.PENGEMBALIANPNBPID;
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

        public Entities.TransactionResult UpdateDataBerkasPengembalian(DetailDataBerkas data, string npwpberkas)
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

                        sql =
                            @"UPDATE berkaskembalian  
                                SET 
                                    jumlahbayar = :JumlahBayar, 
                                    nomorrekening = :NomorRekening, 
                                    namabank = :NamaBank, 
                                    setoranpnbp = :SetoranPnbp, 
                                    npwp = :Npwp, 
                                    permohonanpengembalian = :permohonanPengembalian,
                                    namarekening = :NamaRekening
                                WHERE 
                                    pengembalianpnbpid = :PengembalianPnbpId";

                        List<object> lstParams = new List<object>();
                        lstParams.Add(new OracleParameter("JumlahBayar", data.JUMLAHBAYAR));
                        lstParams.Add(new OracleParameter("NomorRekening", data.NOMORREKENING));
                        lstParams.Add(new OracleParameter("NamaBank", data.NAMABANK));
                        lstParams.Add(new OracleParameter("SetoranPnbp", data.SETORANPNBP));
                        lstParams.Add(new OracleParameter("Npwp", npwpberkas));
                        lstParams.Add(new OracleParameter("permohonanPengembalian", data.PERMOHONANPENGEMBALIAN));
                        lstParams.Add(new OracleParameter("NamaRekening", data.NAMAREKENING));
                        lstParams.Add(new OracleParameter("PengembalianPnbpId", data.PENGEMBALIANPNBPID));

                        ctx.Database.ExecuteSqlCommand(sql, lstParams.ToArray());

                        tc.Commit();
                        tr.ReturnValue = data.PENGEMBALIANPNBPID;
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

        public Entities.TransactionResult SelesaiPengembalianDaerah(string PengembalianPnbpId, string nomorSP2D, string filePath, string ekstensi)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        List<object> lstParams = new List<object>();

                        // update status = selesai
                        string sql = @"UPDATE pengembalianpnbp SET nomorsp2d = :nomorSP2D, STATUSPENGEMBALIAN = 4 WHERE pengembalianpnbpid = :PengembalianPnbpId ";
                        lstParams.Clear();
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nomorSP2D", nomorSP2D));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", PengembalianPnbpId));
                        ctx.Database.ExecuteSqlCommand(sql, lstParams.ToArray());
                        
                        // insert SP2D
                        string idLampiran = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                        sql = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES (:idLampiran, :idPengembalian, :filePath, '1', SYSDATE, 'Pengajuan', 'SP2D', :ekstensi)";

                        lstParams.Clear();
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("idLampiran", idLampiran));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("idPengembalian", PengembalianPnbpId));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("filePath", filePath));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ekstensi", ekstensi));
                        ctx.Database.ExecuteSqlCommand(sql, lstParams.ToArray());


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

        public TransactionResult SelesaiPengembalianPusat(string PengembalianPnbpId, string nomorSP2D, string filePath, string ekstensi, string filePathSPTJM, string ekstensiSPTJM, string filePathPengembalian, string ekstensiPengembalian)
        {
            TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        List<object> lstParams = new List<object>();

                        // update status = selesai
                        string sql = @"UPDATE pengembalianpnbp SET nomorsp2d = :nomorSP2D, STATUSPENGEMBALIAN = 4 WHERE pengembalianpnbpid = :PengembalianPnbpId ";
                        lstParams.Clear();
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nomorSP2D", nomorSP2D));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", PengembalianPnbpId));
                        ctx.Database.ExecuteSqlCommand(sql, lstParams.ToArray());


                        // insert SP2D
                        string idLampiran = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                        sql = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES (:idLampiran, :idPengembalian, :filePath, '1', SYSDATE, 'Pengajuan', 'SP2D', :ekstensi)";

                        lstParams.Clear();
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("idLampiran", idLampiran));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("idPengembalian", PengembalianPnbpId));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("filePath", filePath));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ekstensi", ekstensi));
                        ctx.Database.ExecuteSqlCommand(sql, lstParams.ToArray());

                        // insert SPTJM
                        idLampiran = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                        sql = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES (:idLampiran, :idPengembalian, :filePath, '1', SYSDATE, 'Pengajuan', 'SPTJM', :ekstensi)";

                        lstParams.Clear();
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("idLampiran", idLampiran));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("idPengembalian", PengembalianPnbpId));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("filePath", filePathSPTJM));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ekstensi", ekstensiSPTJM));
                        ctx.Database.ExecuteSqlCommand(sql, lstParams.ToArray());

                        // insert Surat Permohonan Pengembalian
                        idLampiran = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                        sql = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                          "VALUES (:idLampiran, :idPengembalian, :filePath, '1', SYSDATE, 'Pengajuan', 'SURAT PERMOHONAN PENGEMBALIAN', :ekstensi)";

                        lstParams.Clear();
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("idLampiran", idLampiran));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("idPengembalian", PengembalianPnbpId));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("filePath", filePathPengembalian));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ekstensi", ekstensiPengembalian));
                        ctx.Database.ExecuteSqlCommand(sql, lstParams.ToArray());


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

        public Entities.TransactionResult HapusPengembalianPnbp(string pengembalianpnbpid)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".berkaskembalian WHERE pengembalianpnbpid = :PengembalianPnbpId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", pengembalianpnbpid));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampirankembalian WHERE pengembalianpnbpid = :PengembalianPnbpId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", pengembalianpnbpid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pengembalianpnbp WHERE pengembalianpnbpid = :PengembalianPnbpId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", pengembalianpnbpid));
                        parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil dihapus";
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

        public Entities.TransactionResult SimpanPersetujuanPengembalian(Entities.DataPengembalianPnbp data)
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
                        object[] parameters = null;

                        if (!string.IsNullOrEmpty(data.PengembalianPnbpId))
                        {
                            int statussetuju = (data.IsStatusSetuju == true) ? 1 : 0;

                            // Update PENGEMBALIANPNBP
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".pengembalianpnbp SET " +
                                "       tanggalsetuju = TO_DATE(:TanggalSetuju,'DD/MM/YYYY'), " +
                                "       pegawaiidsetuju = :PegawaiIdSetuju, " +
                                "       namapegawaisetuju = :NamaPegawaiSetuju, " +
                                "       statussetuju = :StatusSetuju " +
                                "WHERE pengembalianpnbpid = :PengembalianPnbpId";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalSetuju", data.TanggalSetuju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PegawaiIdSetuju", data.PegawaiIdSetuju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaPegawaiSetuju", data.NamaPegawaiSetuju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("StatusSetuju", statussetuju));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PengembalianPnbpId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);


                            // Simpan Lampiran Persetujuan (1 file). 
                            if (data.ObjectFile != null)
                            {
                                data.TipeFile = "Dokumen Persetujuan";

                                // Cek dulu apakah sudah ada?
                                string query = "SELECT COUNT(*) FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampirankembalian WHERE pengembalianpnbpid = :PengembalianPnbpId AND judul = :Judul";
                                arrayListParameters.Clear();
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PengembalianPnbpId));
                                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", "Persetujuan"));
                                parameters = arrayListParameters.OfType<object>().ToArray();
                                int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                                if (jumlahrecord == 0)
                                {
                                    // Insert LAMPIRANKEMBALIAN
                                    string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();

                                    data.LampiranKembalianId = id;

                                    sql =
                                        "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampirankembalian ( " +
                                        "            lampirankembalianid, pengembalianpnbpid, judul, namafile, " +
                                        "            tanggal, tipefile, ekstensi, objectfile, nip) VALUES " +
                                        "( " +
                                        "            :LampiranKembalianId, :PengembalianPnbpId, :JudulLampiran, :NamaFile, " +
                                        "            TO_DATE(:TanggalFile,'DD/MM/YYYY'), :TipeFile, :Ekstensi, :ObjectFile, :NipPengupload)";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranKembalianId", data.LampiranKembalianId));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PengembalianPnbpId));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JudulLampiran", data.JudulLampiran));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", data.NamaFile));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalFile", data.TanggalSetuju));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeFile", data.TipeFile.ToUpper()));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ekstensi", data.Ekstensi));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ObjectFile", data.ObjectFile));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NipPengupload", data.NipPengupload));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                }
                                else
                                {
                                    // Update LAMPIRANKEMBALIAN
                                    sql =
                                        "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampirankembalian SET " +
                                        "       tanggal = TO_DATE(:TanggalFile,'DD/MM/YYYY'), " +
                                        "       judul = :JudulLampiran, " +
                                        "       namafile = :NamaFile, " +
                                        "       tipefile = :TipeFile, " +
                                        "       ekstensi = :Ekstensi, " +
                                        "       objectfile = :ObjectFile, " +
                                        "       nip = :NipPengupload " +
                                        "WHERE pengembalianpnbpid = :PengembalianPnbpId AND judul = :Judul";
                                    arrayListParameters.Clear();
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalFile", data.TanggalSetuju));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JudulLampiran", data.JudulLampiran));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", data.NamaFile));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeFile", data.TipeFile.ToUpper()));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ekstensi", data.Ekstensi));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ObjectFile", data.ObjectFile));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NipPengupload", data.NipPengupload));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PengembalianPnbpId));
                                    arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", "Persetujuan"));
                                    parameters = arrayListParameters.OfType<object>().ToArray();
                                    ctx.Database.ExecuteSqlCommand(sql, parameters);
                                }
                            }
                        }

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.ReturnValue = data.PengembalianPnbpId;
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

        public string GetStatusPengembalian(string pengembalianPnbpId)
        {
            string result = "";

            string query = @"SELECT to_char(STATUSPENGEMBALIAN) FROM PENGEMBALIANPNBP WHERE PENGEMBALIANPNBPID = :pengembalianPnbpId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pengembalianPnbpId", pengembalianPnbpId));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public string GetTipePengembalian(string pengembalianPnbpId)
        {
            string result = "";

            string query = @"SELECT TIPEPENGEMBALIAN FROM PENGEMBALIANPNBP WHERE PENGEMBALIANPNBPID = :pengembalianPnbpId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pengembalianPnbpId", pengembalianPnbpId));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public string GetKotaKantorById(string kantorId)
        {
            string result = "";

            string query = @"SELECT kota FROM kantor WHERE kantorId = :kantorId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kantorId", kantorId));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public string GetJabatanPegawai(string pegawaiId)
        {
            string result = "";

            string query = $@"SELECT namajabatan FROM {_schemaKKP}.SIAP_VW_PEGAWAI svp WHERE svp.NIPBARU = :nip";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nip", pegawaiId));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<string>(query, parameters).FirstOrDefault();
            }

            return result;
        }

        public string GetNamaLayananBerkasByNo(string nomorBerkas, string kantorId)
        {
            string result = "";

            string query =
                $@"SELECT NAMAPROSEDUR 
                    FROM {_schemaKKP}.BERKAS 
                    WHERE NOMOR = :Nomor AND TAHUN = :Tahun AND KANTORID = :KantorId";

            try
            {
                var noTahun = nomorBerkas.Split('/');
                decimal nomor = decimal.Parse(noTahun[0]);
                decimal tahun = decimal.Parse(noTahun[1]);

                query = sWhitespace.Replace(query, " ");
                List<object> lstParams = new List<object>();
                lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nomor", nomor));
                lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorId));

                using (var ctx = new PnbpContext())
                {
                    result = ctx.Database.SqlQuery<string>(query, lstParams.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                _ = e.Message;
            }

            return result;
        }

        public string GetNamaLayananBerkasByNoKanwil(string nomorBerkas, string kantorId)
        {
            string result = "";

            string query =
                $@"SELECT NAMAPROSEDUR 
                    FROM {_schemaKKP}.BERKAS 
                    WHERE NOMORkanwil = :Nomor AND TAHUNkanwil = :Tahun AND KANTORID = :KantorId";

            try
            {
                var noTahun = nomorBerkas.Split('/');
                decimal nomor = decimal.Parse(noTahun[0]);
                decimal tahun = decimal.Parse(noTahun[1]);

                query = sWhitespace.Replace(query, " ");
                List<object> lstParams = new List<object>();
                lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nomor", nomor));
                lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorId));

                using (var ctx = new PnbpContext())
                {
                    result = ctx.Database.SqlQuery<string>(query, lstParams.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                _ = e.Message;
            }

            return result;
        }


        #endregion


        #region LampiranKembalian

        public List<Entities.LampiranKembalian> GetLampiranKembalian(string pengembalianpnbpid, string judul)
        {
            List<Entities.LampiranKembalian> records = new List<Entities.LampiranKembalian>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT * FROM (
                    SELECT
                        ROW_NUMBER() over (ORDER BY lampirankembalian.tanggal, lampirankembalian.tipefile) RNumber,
                        lampirankembalian.lampirankembalianid, lampirankembalian.pengembalianpnbpid, lampirankembalian.judul JudulLampiran,
                        to_char(lampirankembalian.tanggal, 'dd-mm-yyyy') TanggalFile, lampirankembalian.tipefile, 
                        lampirankembalian.ekstensi, lampirankembalian.nip NipPengupload, pegawai.nama NamaPengupload, 
                        lampirankembalian.objectfile, 
                        COUNT(1) OVER() Total
                    FROM
                        " + System.Web.Mvc.OtorisasiUser.NamaSkema + @".lampirankembalian
                        LEFT JOIN pegawai ON pegawai.pegawaiid = lampirankembalian.nip
                    WHERE
                        lampirankembalian.pengembalianpnbpid = :PengembalianPnbpId ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", pengembalianpnbpid));

            if (!string.IsNullOrEmpty(judul))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", judul));
                query += " AND lampirankembalian.judul = :Judul ";
            }

            query +=
                " )";

            query = sWhitespace.Replace(query, " ");

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.LampiranKembalian>(query, parameters).ToList();
            }

            return records;
        }

        public List<Entities.LampiranKembalian> GetLampiranKembalianForTable(string pengembalianpnbpid, string judul)
        {
            List<Entities.LampiranKembalian> records = new List<Entities.LampiranKembalian>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT * FROM (
                    SELECT
                        ROW_NUMBER() over (ORDER BY lampirankembalian.tanggal, lampirankembalian.tipefile) RNumber,
                        lampirankembalian.lampirankembalianid, lampirankembalian.pengembalianpnbpid, lampirankembalian.judul JudulLampiran,
                        to_char(lampirankembalian.tanggal, 'dd-mm-yyyy') TanggalFile, lampirankembalian.tipefile, 
                        lampirankembalian.ekstensi, lampirankembalian.nip NipPengupload, pegawai.nama NamaPengupload, 
                        COUNT(1) OVER() Total
                    FROM
                        " + System.Web.Mvc.OtorisasiUser.NamaSkema + @".lampirankembalian
                        LEFT JOIN pegawai ON pegawai.pegawaiid = lampirankembalian.nip
                    WHERE
                        lampirankembalian.pengembalianpnbpid = :PengembalianPnbpId ";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", pengembalianpnbpid));

            if (!string.IsNullOrEmpty(judul))
            {
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", judul));
                query += " AND lampirankembalian.judul = :Judul ";
            }

            query +=
                " )";

            query = sWhitespace.Replace(query, " ");

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                records = ctx.Database.SqlQuery<Entities.LampiranKembalian>(query, parameters).ToList();
            }

            return records;
        }

        public Entities.TransactionResult SimpanLampiranKembalian(Entities.DataPengembalianPnbp data)
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
                        object[] parameters = null;

                        if (string.IsNullOrEmpty(data.LampiranKembalianId))
                        {
                            string id = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();

                            data.LampiranKembalianId = id;

                            // Insert LAMPIRANKEMBALIAN
                            sql =
                                "INSERT INTO " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampirankembalian ( " +
                                "            lampirankembalianid, pengembalianpnbpid, judul, namafile, " +
                                "            tanggal, tipefile, ekstensi, objectfile, nip) VALUES " +
                                "( " +
                                "            :LampiranKembalianId, :PengembalianPnbpId, :JudulLampiran, :NamaFile, " +
                                "            TO_DATE(:TanggalFile,'DD/MM/YYYY'), :TipeFile, :Ekstensi, :ObjectFile, :NipPengupload)";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranKembalianId", data.LampiranKembalianId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", data.PengembalianPnbpId));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JudulLampiran", data.JudulLampiran));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", data.NamaFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalFile", data.TanggalFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeFile", data.TipeFile.ToUpper()));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ekstensi", data.Ekstensi));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ObjectFile", data.ObjectFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NipPengupload", data.NipPengupload));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else
                        {
                            // Update LAMPIRANKEMBALIAN
                            sql =
                                "UPDATE " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampirankembalian SET " +
                                "       tanggal = TO_DATE(:TanggalFile,'DD/MM/YYYY'), " +
                                "       judul = :JudulLampiran, " +
                                "       namafile = :NamaFile, " +
                                "       tipefile = :TipeFile, " +
                                "       ekstensi = :Ekstensi, " +
                                "       objectfile = :ObjectFile, " +
                                "       nip = :NipPengupload " +
                                "WHERE lampirankembalianid = :LampiranKembalianId";
                            arrayListParameters.Clear();
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TanggalFile", data.TanggalFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("JudulLampiran", data.JudulLampiran));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NamaFile", data.NamaFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("TipeFile", data.TipeFile.ToUpper()));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Ekstensi", data.Ekstensi));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ObjectFile", data.ObjectFile));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NipPengupload", data.NipPengupload));
                            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranKembalianId", data.LampiranKembalianId));
                            parameters = arrayListParameters.OfType<object>().ToArray();
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                        //tc.Rollback(); // for test
                        tr.ReturnValue = data.LampiranKembalianId;
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

        public TransactionResult InsertLampiranKembalian(LampiranKembalian data)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = "";
                        List<object> lstParams = new List<object>();

                        string idLampiranKembalian = data.LampiranKembalianId;

                        if (String.IsNullOrEmpty(data.LampiranKembalianId)) 
                        {
                            idLampiranKembalian = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault();
                        }

                        sql = "INSERT INTO LAMPIRANKEMBALIAN (LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) " +
                                      "VALUES (:idLampiran, :idPengembalian, :namaFile, '1',SYSDATE,'Pengajuan',:tipefile, :ekstensi)";

                        lstParams.Clear();
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("idLampiran", idLampiranKembalian));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("idPengembalian", data.PengembalianPnbpId));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("namaFile", data.NamaFile));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tipefile", data.TipeFile));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ekstensi", data.Ekstensi));

                        ctx.Database.ExecuteSqlCommand(sql, lstParams.ToArray());

                        tc.Commit();
                        tr.ReturnValue = data.LampiranKembalianId;
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

        public TransactionResult UpdateLampiranKembalian(LampiranKembalian data)
        {
            TransactionResult tr = new TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = "";
                        List<object> lstParams = new List<object>();

                        sql = @"UPDATE LAMPIRANKEMBALIAN 
                                SET 
                                    NAMAFILE = :namaFile, 
                                TANGGAL = SYSDATE, 
                                JUDUL = 'Pengajuan',
                                TIPEFILE = :tipefile,
                                EKSTENSI = :ekstensi
                                WHERE 
                                LAMPIRANKEMBALIANID = :idLampiran";
                        //sql = "UPDATE LAMPIRANKEMBALIAN LAMPIRANKEMBALIANID, PENGEMBALIANPNBPID, NAMAFILE, STATUSLAMPIRAN,TANGGAL,JUDUL,TIPEFILE,EKSTENSI) VALUES (:idLampiran, :idPengembalian, :namaFile, '1',SYSDATE,'Pengajuan',:tipefile, :ekstensi)";

                        lstParams.Clear();
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("namaFile", data.NamaFile));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tipefile", data.TipeFile));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("ekstensi", data.Ekstensi));
                        lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("idLampiran", data.LampiranKembalianId));

                        ctx.Database.ExecuteSqlCommand(sql, lstParams.ToArray());

                        tc.Commit();
                        tr.ReturnValue = data.LampiranKembalianId;
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

        public Entities.TransactionResult HapusLampiranKembalian(string lampirankembalianid)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ArrayList arrayListParameters = new ArrayList();

                        string sql = "DELETE FROM " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampirankembalian WHERE lampirankembalianid = :LampiranKembalianId";
                        arrayListParameters.Clear();
                        arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranKembalianId", lampirankembalianid));
                        object[] parameters = arrayListParameters.OfType<object>().ToArray();
                        ctx.Database.ExecuteSqlCommand(sql, parameters);

                        tc.Commit();
                        tr.Status = true;
                        tr.Pesan = "Data berhasil dihapus";
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

        public byte[] GetFileLampiranById(string lampirankembalianid)
        {
            byte[] theFile = null;
            List<Entities.LampiranKembalian> data = new List<Entities.LampiranKembalian>();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                "SELECT " +
                "    lampirankembalianid, pengembalianpnbpid, judul JudulLampiran, to_char(tanggal, 'dd-mm-yyyy') TanggalFile, " +
                "    tipefile, ekstensi, nip NipPengupload, objectfile " +
                "FROM " +
                "    " + System.Web.Mvc.OtorisasiUser.NamaSkema + ".lampirankembalian " +
                "WHERE " +
                "    lampirankembalianid = :LampiranKembalianId";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("LampiranKembalianId", lampirankembalianid));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Entities.LampiranKembalian>(query, parameters).ToList();

                if (data.Count > 0)
                {
                    theFile = data[0].ObjectFile;
                }
            }

            return theFile;
        }

        public int JumlahLampiran(string pengembalianpnbpid, string judul)
        {
            int result = 0;

            ArrayList arrayListParameters = new ArrayList();

            string query =
                @"SELECT
                      COUNT(*)
                  FROM
                      " + System.Web.Mvc.OtorisasiUser.NamaSkema + @".lampirankembalian
                  WHERE pengembalianpnbpid = :PengembalianPnbpId AND judul = :Judul";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", pengembalianpnbpid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Judul", judul));

            query = sWhitespace.Replace(query, " ");

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public Entities.GetDataBerkasForm GetDataBerkasByNo(string NoTahun, string kantorid)
        {
            ArrayList arrayListParameters = new ArrayList();

            string query =
                $@"WITH AA AS (SELECT 
                    TO_CHAR(CONCAT(
		                    CONCAT( TO_CHAR( BERKAS.NOMOR ), '/' ),
	                    TO_CHAR( BERKAS.TAHUN ))) AS NOTAHUNBERKAS,
                        BERKAS.NAMAPROSEDUR AS NAMAPROSEDUR,
	                    TO_CHAR(BERKAS.NAMAPEMILIK) AS NAMA,
                        TO_CHAR(BERKAS.KANTORID) AS KANTORID,
	                    TO_CHAR(PEMILIK.ALAMAT) AS ALAMAT,
	                    TO_CHAR(PEMILIK.NPWP) AS NPWP,
	                    TO_CHAR(BERKAS.NOMOR) AS NOMOR,
	                    TO_CHAR(BERKAS.BERKASID) AS BERKASID,
	                    TO_CHAR(BERKAS.STATUSBERKAS) AS STATUSBERKAS,
	                    TO_CHAR(BERKAS.TAHUN) AS TAHUN,
	                    TO_CHAR(REKAPPENERIMAANDETAIL.KODEBILLING) AS KODEBILLING,
	                    TO_CHAR(REKAPPENERIMAANDETAIL.NTPN) AS NTPN, 
                        TO_CHAR(REKAPPENERIMAANDETAIL.PENERIMAAN) AS PENERIMAAN
                    FROM {_schemaKKP}.BERKAS 
                     LEFT JOIN REKAPPENERIMAANDETAIL ON BERKAS.BERKASID = REKAPPENERIMAANDETAIL.BERKASID
                     LEFT JOIN {_schemaKKP}.PEMILIK ON BERKAS.PEMILIKID = PEMILIK.PEMILIKID
                    WHERE BERKAS.STATUSBERKAS = 4 AND BERKAS.NOMOR = :Nomor AND BERKAS.TAHUN = :Tahun AND BERKAS.KANTORID = :KantorId
            ) SELECT 
                    NOTAHUNBERKAS,
                    NAMAPROSEDUR,
	                NAMA,
	                ALAMAT,
	                NPWP,
	                NOMOR,
	                BERKASID,
	                STATUSBERKAS,
	                TAHUN,
	                KODEBILLING,
	                NTPN,
                    TO_CHAR(sum(PENERIMAAN)) as PENERIMAAN,
                    KANTORID
                FROM AA 
                GROUP BY NOTAHUNBERKAS, NAMAPROSEDUR, NAMA, ALAMAT, NPWP, NOMOR, BERKASID, STATUSBERKAS, TAHUN, KODEBILLING, NTPN, KANTORID";

            try
            {
                var noTahun = NoTahun.Split('/');
                decimal nomor = decimal.Parse(noTahun[0]);
                decimal tahun = decimal.Parse(noTahun[1]);

                query = sWhitespace.Replace(query, " ");
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Nomor", nomor));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", tahun));
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));

                using (var ctx = new PnbpContext())
                {
                    var records = ctx.Database.SqlQuery<Entities.GetDataBerkasForm>(query, arrayListParameters.ToArray()).FirstOrDefault();
                    return records;
                }
            }
            catch (Exception e)
            {
                _ = e.Message;
            }

            return null;
        }

        public Entities.GetDataBerkasForm GetDataBerkasByNoKanwil(string NoTahun, string kantorid)
        {
            ArrayList arrayListParameters = new ArrayList();

            string query =
                $@"WITH AA AS (SELECT 
                    TO_CHAR(CONCAT(
		                    CONCAT( TO_CHAR( BERKAS.NOMORKANWIL ), '/' ),
	                    TO_CHAR( BERKAS.TAHUNKANWIL ))) AS NOTAHUNBERKAS,
                        BERKAS.NAMAPROSEDUR AS NAMAPROSEDUR,
	                    TO_CHAR(BERKAS.NAMAPEMILIK) AS NAMA,
                        TO_CHAR(BERKAS.KANTORID) AS KANTORID,
	                    TO_CHAR(PEMILIK.ALAMAT) AS ALAMAT,
	                    TO_CHAR(PEMILIK.NPWP) AS NPWP,
	                    TO_CHAR(BERKAS.NOMOR) AS NOMOR,
	                    TO_CHAR(BERKAS.BERKASID) AS BERKASID,
	                    TO_CHAR(BERKAS.STATUSBERKAS) AS STATUSBERKAS,
	                    TO_CHAR(BERKAS.TAHUN) AS TAHUN,
	                    TO_CHAR(REKAPPENERIMAANDETAIL.KODEBILLING) AS KODEBILLING,
	                    TO_CHAR(REKAPPENERIMAANDETAIL.NTPN) AS NTPN, 
                        TO_CHAR(REKAPPENERIMAANDETAIL.PENERIMAAN) AS PENERIMAAN
                    FROM {_schemaKKP}.BERKAS 
                     LEFT JOIN REKAPPENERIMAANDETAIL ON BERKAS.BERKASID = REKAPPENERIMAANDETAIL.BERKASID
                     LEFT JOIN {_schemaKKP}.PEMILIK ON BERKAS.PEMILIKID = PEMILIK.PEMILIKID
                    WHERE BERKAS.STATUSBERKAS = 4
            ) SELECT 
                    NOTAHUNBERKAS,
                    NAMAPROSEDUR,
	                NAMA,
	                ALAMAT,
	                NPWP,
	                NOMOR,
	                BERKASID,
	                STATUSBERKAS,
	                TAHUN,
	                KODEBILLING,
	                NTPN,
                    PENERIMAAN,
                    KANTORID
                FROM AA
            WHERE NOTAHUNBERKAS = :NoTahun AND KANTORID = :kantorid ";

            query = sWhitespace.Replace(query, " ");
            //arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NomorBerkas", String.Concat("%", NomorBerkas.ToLower(), "%")));

            using (var ctx = new PnbpContext())
            {
                List<object> lstParams = new List<object>();
                lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("NoTahun", NoTahun));
                lstParams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kantorid", kantorid));
                var records = ctx.Database.SqlQuery<Entities.GetDataBerkasForm>(query, lstParams.ToArray()).FirstOrDefault();
                return records;
            }
        }

        public Entities.DetailDataBerkas GetDataPengembalianPnbpById(string pengembalianpnbpid)
        {
            Entities.DetailDataBerkas data = new Entities.DetailDataBerkas();

            ArrayList arrayListParameters = new ArrayList();

            string query =
                $@"WITH AA AS(SELECT 
                    TO_CHAR(PENGEMBALIANPNBP.PENGEMBALIANPNBPID) PENGEMBALIANPNBPID,
                    TO_CHAR(PENGEMBALIANPNBP.STATUSPENGEMBALIAN) STATUSPENGEMBALIAN,
                    TO_CHAR(PENGEMBALIANPNBP.NAMAPEGAWAIPENGAJU) NAMAPEGAWAIPENGAJU,
                    TO_CHAR(PENGEMBALIANPNBP.NPWPPEGAWAIPENGAJU) NPWPPEGAWAIPENGAJU,
                    TO_CHAR(pengembalianpnbp.TANGGALPENGAJU, 'dd-mm-yyyy') TANGGALPENGAJU,
                    BERKASKEMBALIAN.NAMAPEMOHON,
                    TO_CHAR(BERKASKEMBALIAN.ALAMATPEMOHON) ALAMATPEMOHON,
                    TO_CHAR(BERKASKEMBALIAN.BERKASID) BERKASID,
                    TO_CHAR(BERKASKEMBALIAN.NOMORBERKAS) NOMORBERKAS,
                    TO_CHAR(BERKASKEMBALIAN.KODEBILLING) KODEBILLING,
                    TO_CHAR(BERKASKEMBALIAN.NTPN) NTPN,
                    TO_CHAR(BERKASKEMBALIAN.JUMLAHBAYAR) JUMLAHBAYAR,
                    TO_CHAR(BERKASKEMBALIAN.NOMORREKENING) NOMORREKENING,
                    TO_CHAR(BERKASKEMBALIAN.NAMABANK) NAMABANK,
                    TO_CHAR(BERKASKEMBALIAN.NPWP) NPWP,
                    TO_CHAR(BERKASKEMBALIAN.NAMAREKENING) NAMAREKENING,
                    TO_CHAR(BERKASKEMBALIAN.SETORANPNBP) SETORANPNBP,
                    TO_CHAR(BERKASKEMBALIAN.PERMOHONANPENGEMBALIAN) PERMOHONANPENGEMBALIAN,
                    TO_CHAR(BERKASKEMBALIAN.NOMORSURAT) NOMORSURAT,
                    PENGEMBALIANPNBP.TIPEPENGEMBALIAN,
                    PENGEMBALIANPNBP.STATUSSIMPAN,
                    PENGEMBALIANPNBP.NOMORTELEPON
                    FROM PENGEMBALIANPNBP
                    LEFT JOIN BERKASKEMBALIAN ON PENGEMBALIANPNBP.PENGEMBALIANPNBPID = BERKASKEMBALIAN.PENGEMBALIANPNBPID
                    LEFT JOIN {_schemaKKP}.BERKAS b ON b.BERKASID = BERKASKEMBALIAN.BERKASID
                    )
                    SELECT * FROM AA 
                    WHERE PENGEMBALIANPNBPID = :PengembalianPnbpId";

            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PengembalianPnbpId", pengembalianpnbpid));

            query = sWhitespace.Replace(query, " ");

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Entities.DetailDataBerkas>(query, parameters).FirstOrDefault();
            }

            return data;
        }

        public Entities.LampiranKembalianTrain GetlampiranPengajuanKembalian(string pengembalianpnbpid, string tipefile)
        {
            Entities.LampiranKembalianTrain data = new Entities.LampiranKembalianTrain();
            ArrayList arrayListParameters = new ArrayList();
            string query = @"SELECT * 
                                FROM LAMPIRANKEMBALIAN WHERE UPPER(TIPEFILE) LIKE UPPER('%" + tipefile + "%') AND PENGEMBALIANPNBPID ='" + pengembalianpnbpid + "'";
            query = sWhitespace.Replace(query, " ");

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                data = ctx.Database.SqlQuery<Entities.LampiranKembalianTrain>(query, parameters).FirstOrDefault();
            }
            return data;
        }
        #endregion


        #region BerkasKembalian

        #endregion

        public Pegawai GetKepalaKantor(string kantorId)
        {
            Pegawai result = null;

            string query = $@"SELECT NVL(TRIM(PG.GELARDEPAN||' '||PG.NAMA||' '||PG.GELARBELAKANG),TRIM(RUP.GELARDEPAN||' '||RUP.NAMA||' '||RUP.GELARBELAKANG)) nama, NVL(jabatan, '') jabatan, pg.pegawaiid nip
                            FROM
                                PROFILEPEGAWAI pp  
                                JOIN PROFILE p ON p.profileid = pp.profileid
                                JOIN pegawai pg ON pp.pegawaiid = pg.pegawaiid
                                JOIN {_schemaKKP}.registeruserpertanahan rup ON rup.pegawaiid = pg.pegawaiid
                            WHERE
	                            (pp.statushapus is null or pp.statushapus = '0')
	                            AND (pp.validsampai is null or TRUNC(CAST(pp.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
	                            AND (pg.validsampai is null or TRUNC(CAST(pg.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
	                            AND (p.validsampai is null or TRUNC(CAST(p.VALIDSAMPAI AS TIMESTAMP)) >= TRUNC(SYSDATE))
	                            AND pp.KANTORID = :kantorId
	                            AND p.profileid = 'N00000'";
            query = sWhitespace.Replace(query, " ");

            try
            {
                using (var ctx = new PnbpContext())
                {
                    List<object> lstParams = new List<object>();
                    lstParams.Add(new OracleParameter("kantorId", kantorId));
                    result = ctx.Database.SqlQuery<Pegawai>(query, lstParams.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                _ = e.Message;
            }

            return result;
        }

        public UnitKerja GetUnitKerjaByPegawaiId(string pegawaiId)
        {
            UnitKerja result = null;

            string query = @"SELECT u.unitkerjaid, namaunitkerja nama FROM registeruserpertanahan rup join unitkerja u on rup.unitkerjaid = u.unitkerjaid
WHERE pegawaiid = :pegawaiId";
            using (var ctx = new PnbpContext())
            {
                try
                {
                    List<object> lstparams = new List<object>();
                    lstparams.Add(new OracleParameter("pegawaiId", pegawaiId));
                    result = ctx.Database.SqlQuery<UnitKerja>(query, lstparams.ToArray()).FirstOrDefault();
                }
                catch (Exception e)
                {
                    _ = e.Message;
                }
            }

            return result;
        }

    }
}