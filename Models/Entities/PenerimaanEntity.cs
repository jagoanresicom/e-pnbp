using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{
    public class JumlahPenerimaan
    {
        [Key]
        public string namakantor { get; set; }
        public Decimal jumlah { get; set; }
        public Decimal nilaiakhir { get; set; }
        public Decimal operasional { get; set; }
    }

    public class QueryStatistikNTPN
    {
        public List<Wilayah> propinsi { get; set; }
        public List<Wilayah> kabupaten { get; set; }
        public string selectedKabupaten { get; set; }
        public string TanggalMulai { get; set; }
        public string TanggalSampai { get; set; }
        public string pKantorId { get; set; }
        public string kodeBilling { get; set; }
        public string ntpn { get; set; }
    }

    public class QueryRekonsiliasi
    {
        public decimal? jumlah { get; set; }
        public string start { get; set; }
        public string end { get; set; }
    }

    public class Rekonsimfoni
    {
        public string tanggalpenerimaan { get; set; }
        public string ntpn { get; set; }
        public string kodentpn { get; set; }
        public int jumlahpenerimaan { get; set; }
        public int jumlahsimfoni { get; set; }
    }

    public class RekonTotalJumlahPenerimaan
    {
        public decimal jumlahpenerimaan { get; set; }
        public decimal totalpenerimaan { get; set; }
    }

    public class RekonTotalJumlahSimfoni
    {
        public decimal jumlahsimfoni { get; set; }
        public decimal totalsimfoni { get; set; }
    }

    public class DataRekonsiliasi
    {
        public decimal? jumlah { get; set; }
        public string tanggal { get; set; }
        public string ntpn { get; set; }
    }

    public class StatistikNTPN
    {
        [Key]
        public string namakantor { get; set; }
        public string kantorid { get; set; }
        public Decimal jumlahntpn { get; set; }
        public Decimal jumlahberkas { get; set; }
        public Decimal nilaintpn { get; set; }
        public Decimal rnumber { get; set; }
        public Decimal totalrec { get; set; }
    }

    public class StatistikNTPNDetail
    {
        [Key]
        public string namakantor { get; set; }
        public decimal nomorberkas { get; set; }
        public decimal tahunberkas { get; set; }
        public string kodebilling { get; set; }
        public string tglbilling { get; set; }
        public string ntpn { get; set; }
        public string tglntpn { get; set; }
        public string kantorid { get; set; }
        public decimal jumlah { get; set; }
        public decimal? penerimaan { get; set; }
        public decimal? jumlahdi305 { get; set; }
        public string nomor305 { get; set; }
        public string tgl305 { get; set; }
        public decimal rnumber { get; set; }
        public decimal totalrec { get; set; }
        public string kodeBilling { get; set; }

    }

    public class ModalStatistikNTPN
    {
        [Key]
        public string ntpn { get; set; }
        public string kodebilling { get; set; }
        public string tanggal { get; set; }
        public string namakantor { get; set; }
        public string namaprosedur { get; set; }
        public string nama_wajib_bayar { get; set; }
        public decimal? jumlahdi305 { get; set; }
        public string jenispenerimaan { get; set; }
        public int nomorberkas { get; set; }
        public int tahun { get; set; }
        public string tipe { get; set; }
        public string ntb { get; set; }
        public string nomor { get; set; }
        public string tgl305 { get; set; }

    }

    public class DetailNTPN
    {
        [Key]
        public string namakantor { get; set; }
        public string nomorberkas { get; set; }
        public string tahunberkas { get; set; }
        public string namaprosedur { get; set; }
        public string namawajibbayar { get; set; }
        public string jenispenerimaan { get; set; }
        public string jenisbiaya { get; set; }
        public string kodebilling { get; set; }
        public string ntpn { get; set; }
        public string ntb { get; set; }
        public Decimal jumlah { get; set; }
    }

    public class QueryInformasiBerkas
    {
        public string kodebilling { get; set; }
        public string ntpn { get; set; }
    }

    public class berkaspenerimaanls
    {
        public string berkasid { get; set; }
        public string ntpn { get; set; }
        public string kodebilling { get; set; }
        public string tglpenerimaan { get; set; }
        public string namakantor { get; set; }
        public string namaprosedur { get; set; }
        public string namawajibbayar { get; set; }
        public Decimal totalbiaya { get; set; }
        public string detailpenerimaan { get; set; }
    }

    public class statusberkasls
    {
        public string berkasid { get; set; }
        public Decimal? nomor { get; set; }
        public Decimal? tahun { get; set; }
        public Decimal? nomorkanwil { get; set; }
        public Decimal? tahunkanwil { get; set; }
        public Decimal? nomorpusat { get; set; }
        public Decimal? tahunpusat { get; set; }
        public Decimal? tipekantor { get; set; }
        public string kantor { get; set; }
        public Decimal? tipekantorwilayah { get; set; }
        public string kantorwilayah { get; set; }
        public Decimal? tipekantortujuan { get; set; }
        public string kantortujuan { get; set; }
        public string statusberkas { get; set; }
    }

    public class InformasiBerkas
    {
        public string berkasid { get; set; }
        public string ntpn { get; set; }
        public string kodebilling { get; set; }
        public string tglpenerimaan { get; set; }
        public string namakantor { get; set; }
        public string namaprosedur { get; set; }
        public string namawajibbayar { get; set; }
        public Decimal totalbiaya { get; set; }
        public string detailpenerimaan { get; set; }
        public Decimal? nomor { get; set; }
        public Decimal? tahun { get; set; }
        public Decimal? nomorkanwil { get; set; }
        public Decimal? tahunkanwil { get; set; }
        public Decimal? nomorpusat { get; set; }
        public Decimal? tahunpusat { get; set; }
        public Decimal? tipekantor { get; set; }
        public string kantor { get; set; }
        public Decimal? tipekantorwilayah { get; set; }
        public string kantorwilayah { get; set; }
        public Decimal? tipekantortujuan { get; set; }
        public string kantortujuan { get; set; }
        public string statusberkas { get; set; }
    }

    public class StatistikPenerimaan
    {
        [Key]
        public string kanwilid { get; set; }
        public string kantorid { get; set; }
        public string kodesatker { get; set; }
        public string nama { get; set; }
        public string jenispenerimaan { get; set; }
        public string kodepenerimaan { get; set; }
        public decimal penerimaan { get; set; }
        public decimal operasional { get; set; }
        public decimal targetfisik { get; set; }
        public int urutan { get; set; }
        public string namaprosedur { get; set; }
        public int jumlah { get; set; }
        public int RecordsTotal { get; set; }
    }

    public class DaftarRekapPenerimaanDetail
    {
        [Key]
        public string kanwilid { get; set; }
        public string kantorid { get; set; }
        public string kodesatker { get; set; }
        public string nama_satker { get; set; }
        public string id_provinsi { get; set; }
        public string nama_provinsi { get; set; }
        public string jenispenerimaan { get; set; }
        public string kodepenerimaan { get; set; }
        public decimal penerimaan { get; set; }
        public decimal operasional { get; set; }
        public decimal targetfisik { get; set; }
        public int urutan { get; set; }
        public string namaprosedur { get; set; }
        public int jumlah { get; set; }
        public int? nilaitarget { get; set; }
        public int RecordsTotal { get; set; }
    }

    public class DaftarTotal
    {
        [Key]
        public Decimal? totalpenerimaan { get; set; }
        public Decimal? totaloperasional { get; set; }
    }

    public class RekapPenerimaanDetail
    {
        [Key]
        public string kantorid { get; set; }
        public string berkasid { get; set; }
        public string kodesatker { get; set; }
        public string namakantor { get; set; }
        public string namaprosedur { get; set; }
        public int nomorberkas { get; set; }
        public int tahunberkas { get; set; }
        public string jenispenerimaan { get; set; }
        public string kodepenerimaan { get; set; }
        public string berkaspersepsiid { get; set; }
        public int tahun { get; set; }
        public int bulan { get; set; }
        public string kodebilling { get; set; }
        public string ntpn { get; set; }
        public int jumlah { get; set; }
        public int penerimaan { get; set; }
        public int operasional { get; set; }
    }

    public class RealisasiPenerimaanDetail
    {
        [Key]
        public string kantorid { get; set; }
        public string kodesatker { get; set; }
        public string berkasid { get; set; }
        public string namakantor { get; set; }
        public string namaprosedur { get; set; }
        public int jumlah { get; set; }
        public decimal penerimaan { get; set; }
        public decimal operasional { get; set; }
        public int urutan { get; set; }
        public int tahun { get; set; }
        public int bulan { get; set; }
        public int targetfisik { get; set; }
        public int targetpenerimaan { get; set; }
        public Decimal? nilaitarget { get; set; }
        public decimal persentasefisik { get; set; }
        public Decimal? persentasepenerimaan { get; set; }
    }

    public class RealisasiLayananDetail
    {
        [Key]
        public string namaprosedur { get; set; }
        public string kodesatker { get; set; }
        public string namakantor { get; set; }
        public string kantorid { get; set; }
        public string berkasid { get; set; }
        public int targetfisik { get; set; }
        public int jumlah { get; set; }
        public Decimal? persentasefisik { get; set; }
        public decimal targetpenerimaan { get; set; }
        public decimal realisasipenerimaan { get; set; }
        public Decimal? persentasepenerimaan { get; set; }
        public int operasional { get; set; }
        public int urutan { get; set; }
    }

    public class FilterPenerimaan
    {
        public string tahun { get; set; }
        public List<Tahun> lstahun { get; set; }
        public List<StatistikPenerimaan> lspenerimaan { get; set; }
        public List<DaftarRekapPenerimaanDetail> lspenerimaandetail { get; set; }
        public List<RealisasiLayananDetail> lslayanan { get; set; }
        public string bulan { get; set; }
        public string propinsi { get; set; }
        public string satker { get; set; }
    }

    public class propinsi
    {
        [Key]
        public string kantorid { get; set; }
        public string kode { get; set; }
        public string nama_satker { get; set; }
    }

    public class satker
    {
        [Key]
        public string KantorId { get; set; }
        public string Nama { get; set; }
        public string kode { get; set; }
    }

    public class program
    {
        [Key]
        public string ProgramId { get; set; }
        public string Nama { get; set; }
        public string kode { get; set; }
    }
}