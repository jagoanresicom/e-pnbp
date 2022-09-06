using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Pnbp.Entities
{
    public class TransactionResult
    {
        public bool Status { get; set; }
        public string Pesan { get; set; }
        public string ReturnValue { get; set; }
        public string ReturnValue2 { get; set; }
    }

    public class File
    {
        [Required(ErrorMessage = "Please select file.")]
        [Display(Name = "Browse File")]
        public HttpPostedFileBase[] files { get; set; }

        public string file_id { get; set; }
        public string file_name { get; set; }
        public string file_path { get; set; }
    }

    public class DataManfaat
    {
        public bool UserAdmin { get; set; }
        public List<Entities.SatkerAlokasi> satkermanfaat { get; set; }
    }

    public class Kantor
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string KantorId { get; set; }
        public string KodeKantor { get; set; }
        public string NamaKantor { get; set; }
        public string Kota { get; set; }
        public string Alamat { get; set; }
        public string Email { get; set; }
    }

    public class SatkerAlokasi
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string Kantorid { get; set; }
        public string Kodekantor { get; set; }
        public string Kodesatker { get; set; }
        public string Namasatker { get; set; }
        public decimal Nilaianggaran { get; set; }
        public decimal Nilaialokasi { get; set; }
        public string Statusaktif { get; set; }
        public string Anggaransatker { get; set; }
        public string Alokasisatker { get; set; }
        public string RenaksiSatkerId { get; set; }
        public int? StatusRenaksi { get; set; }
        public decimal JUMLAHALOKASI { get; set; }
    }

    public class TotalAnggaranAlokasi
    {
        public decimal TotalNilaiAnggaran { get; set; }
        public decimal TotalNilaiAlokasi { get; set; }
    }

    public class TotalDipaBelanja
    {
        public decimal TOTALPAGU { get; set; }
    }

    public class DataPrioritas
    {
        public string KantorId { get; set; }
        public string NamaSatKer { get; set; }
        public bool UserAdmin { get; set; }
        public bool UserKaBiroPerencanaan { get; set; }
        public bool UserKaBiroKeuangan { get; set; }
        public List<PrioritasAlokasi> dataPrioritas { get; set; }
        public string tahun { get; set; }
        public decimal TotalAnggaran { get; set; }
        public decimal TotalAlokasi { get; set; }
        public decimal TotalTerAlokasi { get; set; }
    }

    public class FindManfaat
    {
        public string KodeSatker { get; set; }
        public string NamaSatker { get; set; }
        public string NamaProgram { get; set; }
        public decimal? NilaiAnggaran { get; set; }
        public string Status { get; set; }
        public bool UserKaBiroPerencanaan { get; set; }
        public bool UserKaBiroKeuangan { get; set; }

        public List<Tahun> lstahun { get; set; }
        public string tahun { get; set; }
    }

    public class PrioritasAlokasi
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string Tahun { get; set; }
        public string Kode { get; set; }
        public string Tipe { get; set; }
        public string Prioritaskegiatan { get; set; }
        public string Manfaatid { get; set; }
        public string Kodesatker { get; set; }
        public string Namasatker { get; set; }
        public string Namaprogram { get; set; }
        public decimal Nilaianggaran { get; set; }
        public string NilaianggaranView { get; set; }
        public decimal NilaiAlokasi { get; set; }
        public decimal Teralokasi { get; set; }
        public decimal Alokasi { get; set; }
        public string Anggaransatker { get; set; }
        public string Alokasisatker { get; set; }
        public string PrioritasOrigin { get; set; }
        public string Statusaktif { get; set; }
        public string Mode { get; set; }
        public string ProgramId { get; set; }
        public string KantorId { get; set; }
        public List<Program> ListProgram { get; set; }
        public List<SatuanKerja> ListSatKer { get; set; }

        public decimal? AnggJan { get; set; }
        public decimal? AnggFeb { get; set; }
        public decimal? AnggMar { get; set; }
        public decimal? AnggApr { get; set; }
        public decimal? AnggMei { get; set; }
        public decimal? AnggJun { get; set; }
        public decimal? AnggJul { get; set; }
        public decimal? AnggAgt { get; set; }
        public decimal? AnggSep { get; set; }
        public decimal? AnggOkt { get; set; }
        public decimal? AnggNov { get; set; }
        public decimal? AnggDes { get; set; }

        public decimal? RankJan { get; set; }
        public decimal? RankFeb { get; set; }
        public decimal? RankMar { get; set; }
        public decimal? RankApr { get; set; }
        public decimal? RankMei { get; set; }
        public decimal? RankJun { get; set; }
        public decimal? RankJul { get; set; }
        public decimal? RankAgt { get; set; }
        public decimal? RankSep { get; set; }
        public decimal? RankOkt { get; set; }
        public decimal? RankNov { get; set; }
        public decimal? RankDes { get; set; }

        public decimal? AlokJan { get; set; }
        public decimal? AlokFeb { get; set; }
        public decimal? AlokMar { get; set; }
        public decimal? AlokApr { get; set; }
        public decimal? AlokMei { get; set; }
        public decimal? AlokJun { get; set; }
        public decimal? AlokJul { get; set; }
        public decimal? AlokAgt { get; set; }
        public decimal? AlokSep { get; set; }
        public decimal? AlokOkt { get; set; }
        public decimal? AlokNov { get; set; }
        public decimal? AlokDes { get; set; }

        public decimal? StatusRevisi { get; set; }
        public decimal? Persetujuan1 { get; set; }
        public decimal? Persetujuan2 { get; set; }

        public decimal JUMLAHALOKASI { get; set; }
        public decimal TOTALALOKASI { get; set; }

        public HttpPostedFileBase filepdf { get; set; }
    }
    public class BelanjaKRO
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string KodeKRO { get; set; }
        public string KRO { get; set; }
        public decimal? Pagu { get; set; }
        public decimal? Alokasi { get; set; }
        public decimal? Realisasi { get; set; }
        public decimal? TotalAlokasi { get; set; }
        public decimal? TotalPagu { get; set; }
        public string PersentasePagu { get; set; }
        public string PersentaseAlokasi { get; set; }
    }

    public class DataPaguAlokasi
    {
        public PaguAlokasiNasional paguAlokasiNasional { get; set; }
        public List<PaguAlokasiProvinsi> ListPaguAlokasiProvinsi { get; set; }
    }

    public class QueryPaguAlokasiNasional
    {
        public string Tipe { get; set; }
        public decimal Pagu { get; set; }
        public decimal Alokasi { get; set; }
    }

    public class PaguAlokasiNasional
    {
        public decimal PaguOps { get; set; }
        public decimal PaguNonOps { get; set; }
        public decimal AlokasiOps { get; set; }
        public decimal AlokasiNonOps { get; set; }
        public decimal Persentase { get; set; }
        public string PersentaseString { get; set; }
        public decimal BelumAlokasi { get; set; }
    }

    public class QueryPaguAlokasiProvinsi
    {
        public string Kode { get; set; }
        public string Tipe { get; set; }
        public decimal Pagu { get; set; }
        public decimal Alokasi { get; set; }
    }

    public class PaguAlokasiProvinsi
    {
        public decimal Rownums { get; set; }
        public string KantorId { get; set; }
        public string KodeKantor { get; set; }
        public string NamaProvinsi { get; set; }
        public decimal PaguOps { get; set; }
        public decimal PaguNonOps { get; set; }
        public decimal AlokasiOps { get; set; }
        public decimal AlokasiNonOps { get; set; }
        public decimal TotalPagu { get; set; }
        public decimal TotalAlokasi { get; set; }
        public decimal Persentase { get; set; }
        public string PersentaseString { get; set; }
        public decimal BelumAlokasi { get; set; }
    }

    public class QueryPaguAlokasiSatker
    {
        public string Kode { get; set; }
        public string NamaSatker { get; set; }
        public string Tipe { get; set; }
        public decimal Pagu { get; set; }
        public decimal Alokasi { get; set; }

        public decimal AnggJan { get; set; }
        public decimal AnggFeb { get; set; }
        public decimal AnggMar { get; set; }
        public decimal AnggApr { get; set; }
        public decimal AnggMei { get; set; }
        public decimal AnggJun { get; set; }
        public decimal AnggJul { get; set; }
        public decimal AnggAgt { get; set; }
        public decimal AnggSep { get; set; }
        public decimal AnggOkt { get; set; }
        public decimal AnggNov { get; set; }
        public decimal AnggDes { get; set; }

        public decimal AlokJan { get; set; }
        public decimal AlokFeb { get; set; }
        public decimal AlokMar { get; set; }
        public decimal AlokApr { get; set; }
        public decimal AlokMei { get; set; }
        public decimal AlokJun { get; set; }
        public decimal AlokJul { get; set; }
        public decimal AlokAgt { get; set; }
        public decimal AlokSep { get; set; }
        public decimal AlokOkt { get; set; }
        public decimal AlokNov { get; set; }
        public decimal AlokDes { get; set; }
    }

    public class PaguAlokasiSatker
    {
        public decimal Rownums { get; set; }
        public string KantorId { get; set; }
        public string KodeKantor { get; set; }
        public string NamaSatker { get; set; }
        public decimal PaguOps { get; set; }
        public decimal PaguNonOps { get; set; }
        public decimal AlokasiOps { get; set; }
        public decimal AlokasiNonOps { get; set; }
        public decimal TotalPagu { get; set; }
        public decimal TotalAlokasi { get; set; }
        public decimal Persentase { get; set; }
        public string PersentaseString { get; set; }
        public decimal BelumAlokasi { get; set; }

        public decimal AnggJan { get; set; }
        public decimal AnggFeb { get; set; }
        public decimal AnggMar { get; set; }
        public decimal AnggApr { get; set; }
        public decimal AnggMei { get; set; }
        public decimal AnggJun { get; set; }
        public decimal AnggJul { get; set; }
        public decimal AnggAgt { get; set; }
        public decimal AnggSep { get; set; }
        public decimal AnggOkt { get; set; }
        public decimal AnggNov { get; set; }
        public decimal AnggDes { get; set; }

        public decimal AlokJan { get; set; }
        public decimal AlokFeb { get; set; }
        public decimal AlokMar { get; set; }
        public decimal AlokApr { get; set; }
        public decimal AlokMei { get; set; }
        public decimal AlokJun { get; set; }
        public decimal AlokJul { get; set; }
        public decimal AlokAgt { get; set; }
        public decimal AlokSep { get; set; }
        public decimal AlokOkt { get; set; }
        public decimal AlokNov { get; set; }
        public decimal AlokDes { get; set; }
    }

    public class Program
    {
        public string ProgramId { get; set; }
        public string Nama { get; set; }
    }

    public class SatuanKerja
    {
        public string KantorId { get; set; }
        public string KodeSatker { get; set; }
        public string NamaSatker { get; set; }
    }

    public class FindPengumuman
    {
        public string JudulBerita { get; set; }
        public string IsiBerita { get; set; }
        public string TanggalMulai { get; set; }
        public string TanggalBerakhir { get; set; }
    }

    public class Pengumuman
    {
        public string BeritaAppId { get; set; }
        public string JudulBerita { get; set; }
        public string IsiBerita { get; set; }
        public string TanggalMulai { get; set; }
        public string TanggalBerakhir { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
    }

    public class Konfigurasi
    {
        public string KataKunci { get; set; }
        public string Nilai { get; set; }
    }

    public class ListKantor
    {
        [Key]
        public string value { get; set; }
        public string data { get; set; }
    }

    public class ListBank
    {
        [Key]
        public string value { get; set; }
        public string data { get; set; }
    }

    public class FindPengembalianPnbp
    {
        public string CariNamaKantor { get; set; }
        public string CariJudul { get; set; }
        public string CariNomorBerkas { get; set; }
        public string CariKodeBilling { get; set; }
        public string CariNTPN { get; set; }
        public string CariNamaPemohon { get; set; }
        public string CariNikPemohon { get; set; }
        public string CariAlamatPemohon { get; set; }
        public string CariTeleponPemohon { get; set; }
        public string CariBankPersepsi { get; set; }
        public string CariStatus { get; set; }
        public string CariNamaSatker { get; set; }
        public string CariKodeSatker { get; set; }
        bool? Disetujui;
        [Display(Name = "Disetujui")]
        public bool IsDisetujui
        {
            get { return Disetujui ?? false; }
            set { Disetujui = value; }
        }
    }

    public class PengembalianPnbp
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string PengembalianPnbpId { get; set; }
        public string KantorId { get; set; }
        public string NamaKantor { get; set; }
        public string Judul { get; set; }
        public string PegawaiIdPengaju { get; set; }
        public string NamaPegawaiPengaju { get; set; }
        public string TanggalPengaju { get; set; }
        public string PegawaiIdSetuju { get; set; }
        public string NamaPegawaiSetuju { get; set; }
        public string TanggalSetuju { get; set; }
        public decimal? StatusSetuju { get; set; }


        // Berkas Pengembalian
        public string BerkasId { get; set; }
        public string NamaProsedur { get; set; }
        public string KodeBilling { get; set; }
        public string TanggalKodeBilling { get; set; }
        public string Ntpn { get; set; }
        public string TanggalBayar { get; set; }
        public string JumlahBayar { get; set; }
        public decimal? JumlahBayarNumber { get; set; }
        public string NamaBankPersepsi { get; set; }
        public string PemilikId { get; set; }
        public string NamaPemohon { get; set; }
        public string NikPemohon { get; set; }
        public string AlamatPemohon { get; set; }
        public string EmailPemohon { get; set; }
        public string NomorTelepon { get; set; }
        public string NomorBerkas { get; set; }
        public string NomorRekening { get; set; }
        public string NamaBank { get; set; }
        public string NamaCabang { get; set; }
    }

    public class LampiranKembalian
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string LampiranKembalianId { get; set; }
        public string PengembalianPnbpId { get; set; }
        public string NamaFile { get; set; }
        public string TanggalFile { get; set; }
        public string JudulLampiran { get; set; }
        public string TipeFile { get; set; }
        public string Ekstensi { get; set; }
        public byte[] ObjectFile { get; set; }
        public string NipPengupload { get; set; }
        public string NamaPengupload { get; set; }
    }

    public class LampiranKembalianTrain
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string LampiranKembalianId { get; set; }
        public string PengembalianPnbpId { get; set; }
        public string NamaFile { get; set; }
        public string TanggalFile { get; set; }
        public string JudulLampiran { get; set; }
        public string TipeFile { get; set; }
        public string Ekstensi { get; set; }
        public byte[] ObjectFile { get; set; }
        public string NipPengupload { get; set; }
        public string NamaPengupload { get; set; }
        public string StatusLampiran { get; set; }
    }

    public class BerkasKembalian
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string BerkasId { get; set; }
        public string NamaProsedur { get; set; }
        public decimal? StatusBerkas { get; set; }
        public string PengembalianPnbpId { get; set; }
        public string KodeBilling { get; set; }
        public string TanggalKodeBilling { get; set; }
        public string Ntpn { get; set; }
        public string TanggalBayar { get; set; }
        public string JumlahBayar { get; set; }
        public string NamaBankPersepsi { get; set; }
        public string PemilikId { get; set; }
        public string NamaPemohon { get; set; }
        public string NikPemohon { get; set; }
        public string AlamatPemohon { get; set; }
        public string EmailPemohon { get; set; }
        public string NomorTelepon { get; set; }
        public string NomorBerkas { get; set; }
        public string NomorRekening { get; set; }
        public string NamaBank { get; set; }
        public string NamaCabang { get; set; }

        public string AlertInfo { get; set; }
    }

    public class DataPengembalianPnbp
    {
        public string EditMode { get; set; }
        public string EditModeLampiran { get; set; }
        public string UserId { get; set; }
        public string KantorIdUser { get; set; }
        public string SelectedKantorId { get; set; }


        // PengembalianPnbp
        public string PengembalianPnbpId { get; set; }
        public string KantorId { get; set; }
        public string NamaKantor { get; set; }
        public string Judul { get; set; }
        public string PegawaiIdPengaju { get; set; }
        public string NamaPegawaiPengaju { get; set; }
        public string TanggalPengaju { get; set; }
        public string PegawaiIdSetuju { get; set; }
        public string NamaPegawaiSetuju { get; set; }
        public string TanggalSetuju { get; set; }
        public decimal? StatusSetuju { get; set; }
        bool? Is_StatusSetuju;
        [Display(Name = "Disetujui")]
        public bool IsStatusSetuju
        {
            get { return Is_StatusSetuju ?? false; }
            set { Is_StatusSetuju = value; }
        }
        public byte[] ObjectFileSetuju { get; set; }
        public HttpPostedFileBase FileDokumenSetuju { get; set; }


        // Lampiran Pengembalian
        public string LampiranKembalianId { get; set; }
        public string NamaFile { get; set; }
        public string TanggalFile { get; set; }
        public string JudulLampiran { get; set; }
        public string TipeFile { get; set; }
        public string Ekstensi { get; set; }
        public byte[] ObjectFile { get; set; }
        public HttpPostedFileBase FileDokumen { get; set; }
        public string NipPengupload { get; set; }
        public string NamaPengupload { get; set; }


        // Berkas Pengembalian
        public string BerkasId { get; set; }
        public string NamaProsedur { get; set; }
        public string KodeBilling { get; set; }
        public string TanggalKodeBilling { get; set; }
        public string Ntpn { get; set; }
        public string TanggalBayar { get; set; }
        public string JumlahBayar { get; set; }
        public string NamaBankPersepsi { get; set; }
        public string PemilikId { get; set; }
        public string NamaPemohon { get; set; }
        public string NikPemohon { get; set; }
        public string AlamatPemohon { get; set; }
        public string EmailPemohon { get; set; }
        public string NomorTelepon { get; set; }
        public string NomorBerkas { get; set; }
        public string NomorRekening { get; set; }
        public string NamaBank { get; set; }
        public string NamaCabang { get; set; }
    }

    public class TargetPnbpHistory
    {
        public decimal? Total { get; set; }
        public decimal? RNumber { get; set; }
        public string File_Name_Target { get; set; }
        public string File_Path_Target { get; set; }
        public string FILE_CREATE_DATE { get; set; }
        
    }

    public class FilterPengembalian
    {
        public string tahun { get; set; }
        public List<Tahun> lstahun { get; set; }
        public List<PenerimaanNTPN> lspenerimaan { get; set; }
        public string bulan { get; set; }
        public string namapemohon { get; set; }
    }

    public class ExportPengembalian
    {
        public string kode { get; set; }
        public string nama_satker { get; set; }
        public string namapemohon { get; set; }
        public string nomorberkas { get; set; }
        public string nomorsurat { get; set; }
        public string namabank { get; set; }
        public string jumlahbayar { get; set; }
        public string statustext { get; set; }
        public DateTime tanggalpengaju { get; set; }
        public int statuspengembalian { get; set; }
        public int urutan { get; set; }
    }

    public class FilterManajemenData
    {
        public string tahun { get; set; }
        public List<Tahun> lstahun { get; set; }
        public List<PenerimaanNTPN> lspenerimaan { get; set; }
        public string bulan { get; set; }
    }

    public class PenerimaanNTPN
    {
        [Key]
        public string kantorid { get; set; }
        public string berkasid { get; set; }
        public DateTime tanggal { get; set; }
        public string kodesatker { get; set; }
        public string namakantor { get; set; }
        public string namaprosedur { get; set; }
        public int nomorberkas { get; set; }
        public int tahunberkas { get; set; }
        public string jenispenerimaan { get; set; }
        public string kodepenerimaan { get; set; }
        public string bankpersepsiid { get; set; }
        public int tahun { get; set; }
        public int bulan { get; set; }
        public string kodebilling { get; set; }
        public string ntpn { get; set; }
        public decimal jumlah { get; set; }
        public decimal penerimaan { get; set; }
        public decimal operasional { get; set; }
        public int urutan { get; set; }
    }

    public class Renaksi
    {
        [Key]
        public int tahun { get; set; }
        public string namakantor { get; set; }
        public string namaprogram { get; set; }
        public string tipe { get; set; }
        public decimal? nilaianggaran { get; set; }
        public decimal? nilaialokasi { get; set; }
        public decimal? totalalokasi { get; set; }
        public decimal? sisaalokasi { get; set; }
        public string kodesatker { get; set; }
        public string manfaatid { get; set; }
        public string kantorid { get; set; }
        public string programid { get; set; }
        public int? prioritaskegiatan { get; set; }
        public int? statusfullalokasi { get; set; }
        public int? statusedit { get; set; }
        public int? statusaktif { get; set; }
        public string userinsert { get; set; }
        public DateTime? insertdate { get; set; }
        public string userupdate { get; set; }
        public DateTime? lastupdate { get; set; }
        public int? totalgroup { get; set; }
        public int? persengroup { get; set; }
        public int? persenprogram { get; set; }
        public decimal? anggjan { get; set; }
        public decimal? rankjan { get; set; }
        public decimal? anggfeb { get; set; }
        public decimal? rankfeb { get; set; }
        public decimal? anggmar { get; set; }
        public decimal? rankmar { get; set; }
        public decimal? anggapr { get; set; }
        public decimal? rankapr { get; set; }
        public decimal? anggmei { get; set; }
        public decimal? rankmei { get; set; }
        public decimal? anggjul { get; set; }
        public decimal? rankjul { get; set; }
        public decimal? anggjun { get; set; }
        public decimal? rankjun { get; set; }
        public decimal? anggagt { get; set; }
        public decimal? rankagt { get; set; }
        public decimal? anggsep { get; set; }
        public decimal? ranksep { get; set; }
        public decimal? anggokt { get; set; }
        public decimal? rankokt { get; set; }
        public decimal? anggnov { get; set; }
        public decimal? ranknov { get; set; }
        public decimal? anggdes { get; set; }
        public decimal? rankdes { get; set; }
        public int? prioritasasal { get; set; }
        public int? alokjan { get; set; }
        public int? alokfeb { get; set; }
        public int? alokmar { get; set; }
        public int? alokapr { get; set; }
        public int? alokmei { get; set; }
        public int? alokjun { get; set; }
        public int? alokjul { get; set; }
        public int? alokagt { get; set; }
        public int? aloksep { get; set; }
        public int? alokokt { get; set; }
        public int? aloknov { get; set; }
        public int? alokdes { get; set; }
        public string kode { get; set; }
        public int? statusrevisi { get; set; }
        public int? persetujuan1 { get; set; }
        public int? persetujuan2 { get; set; }
        public int urutan { get; set; }
    }

    public class CariPenerimaan
    {
        public string PGMAXPAGU { get; set; }
        public string PGMINPAGU { get; set; }
    }

    public class CariBelanja
    {
        public string PGMAXPAGU { get; set; }
        public string PGMINPAGU { get; set; }
        public string PGMAXALOK { get; set; }
        public string PGMINALOK { get; set; }
        public int OPSI { get; set; }
    }

    public class PenerimaanMonev
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string KantorId { get; set; }
        public string KodeSatker { get; set; }
        public string NamaKantor { get; set; }
        public decimal? NilaiTarget { get; set; }
        public decimal? Penerimaan { get; set; }
        public decimal? Persen { get; set; }
        public int Evaluasi { get; set; }
        public int Renaksi { get; set; }
        public string Aksi { get; set; }
        public int urutan { get; set; }
        public int? readevaluasi { get; set; }
        public int? readrenaksi { get; set; }
    }

    public class BelanjaMonev
    {
        public int urutan { get; set; }
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string KodeSatker { get; set; }
        public string kantorid { get; set; }
        public string NamaKantor { get; set; }
        public decimal? Pagu { get; set; }
        public decimal? Alokasi { get; set; }
        public int? Evaluasi { get; set; }
        public int? Renaksi { get; set; }
        public decimal? RealisasiBelanja { get; set; }
        public decimal? PERSENPAGU { get; set; }
        public decimal? PersenAlok { get; set; }
        public string Aksi { get; set; }
        public int? readevaluasi { get; set; }
        public int? readrenaksi { get; set; }
    }

    public class PengembalianPnbpTrain
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string PengembalianPnbpId { get; set; }
        public string KantorId { get; set; }
        public string NamaKantor { get; set; }
        public string Judul { get; set; }
        public string PegawaiIdPengaju { get; set; }
        public string NamaPegawaiPengaju { get; set; }
        public string TanggalPengaju { get; set; }
        public string PegawaiIdSetuju { get; set; }
        public string NamaPegawaiSetuju { get; set; }
        public string TanggalSetuju { get; set; }
        public decimal? StatusSetuju { get; set; }
        public decimal? StatusPengembalian { get; set; }


        // Berkas Pengembalian
        public string BerkasId { get; set; }
        public string NamaProsedur { get; set; }
        public string KodeBilling { get; set; }
        public string TanggalKodeBilling { get; set; }
        public string Ntpn { get; set; }
        public string TanggalBayar { get; set; }
        public string JumlahBayar { get; set; }
        public decimal? JumlahBayarNumber { get; set; }
        public string NamaBankPersepsi { get; set; }
        public string PemilikId { get; set; }
        public string NamaPemohon { get; set; }
        public string NikPemohon { get; set; }
        public string AlamatPemohon { get; set; }
        public string EmailPemohon { get; set; }
        public string NomorTelepon { get; set; }
        public string NomorBerkas { get; set; }
        public string NomorRekening { get; set; }
        public string NamaBank { get; set; }
        public string NamaCabang { get; set; }
        public string KodeSatker { get; set; }
        public string NamaSatker { get; set; }
        public string NomorSurat { get; set; }
        public string permohonanpengembalian { get; set; }
    }

    public class DataPengembalianPnbpDev
    {
        public string EditMode { get; set; }
        public string EditModeLampiran { get; set; }
        public string UserId { get; set; }
        public string KantorIdUser { get; set; }
        public string SelectedKantorId { get; set; }


        // PengembalianPnbp
        public string PengembalianPnbpId { get; set; }
        public string KantorId { get; set; }
        public string NamaKantor { get; set; }
        public string Judul { get; set; }
        public string PegawaiIdPengaju { get; set; }
        public string NamaPegawaiPengaju { get; set; }
        public string TanggalPengaju { get; set; }
        public string PegawaiIdSetuju { get; set; }
        public string NamaPegawaiSetuju { get; set; }
        public string TanggalSetuju { get; set; }
        public decimal? StatusSetuju { get; set; }
        bool? Is_StatusSetuju;
        [Display(Name = "Disetujui")]
        public bool IsStatusSetuju
        {
            get { return Is_StatusSetuju ?? false; }
            set { Is_StatusSetuju = value; }
        }
        public byte[] ObjectFileSetuju { get; set; }
        public HttpPostedFileBase FileDokumenSetuju { get; set; }


        // Lampiran Pengembalian
        public string LampiranKembalianId { get; set; }
        public string NamaFile { get; set; }
        public string TanggalFile { get; set; }
        public string JudulLampiran { get; set; }
        public string TipeFile { get; set; }
        public string Ekstensi { get; set; }
        public byte[] ObjectFile { get; set; }
        public HttpPostedFileBase FileDokumen { get; set; }
        public string NipPengupload { get; set; }
        public string NamaPengupload { get; set; }


        // Berkas Pengembalian
        public string BerkasId { get; set; }
        public string NamaProsedur { get; set; }
        public string KodeBilling { get; set; }
        public string TanggalKodeBilling { get; set; }
        public string Ntpn { get; set; }
        public string TanggalBayar { get; set; }
        public string JumlahBayar { get; set; }
        public string NamaBankPersepsi { get; set; }
        public string PemilikId { get; set; }
        public string NamaPemohon { get; set; }
        public string NikPemohon { get; set; }
        public string AlamatPemohon { get; set; }
        public string EmailPemohon { get; set; }
        public string NomorTelepon { get; set; }
        public string NomorBerkas { get; set; }
        public string NomorRekening { get; set; }
        public string NamaBank { get; set; }
        public string NamaCabang { get; set; }
        public string Npwp { get; set; }
        public string NamaRekening { get; set; }
        public string SetoranPnbp { get; set; }
        public string PermohonanPengembalian { get; set; }
        public string NomorSurat { get; set; }
    }

    public class GetDataBerkasForm
    {
        public string NOTAHUNBERKAS { get; set; }
        public string NAMA { get; set; }
        public string ALAMAT { get; set; }
        public string NPWP { get; set; }
        public string NOMOR { get; set; }
        public string BERKASID { get; set; }
        public string STATUSBERKAS { get; set; }
        public string TAHUN { get; set; }
        public string KODEBILLING { get; set; }
        public string NTPN { get; set; }
        public string PENERIMAAN { get; set; }
        public string NAMAPROSEDUR { get; set; }
    }

    public class DetailDataBerkas
    {
        public string PENGEMBALIANPNBPID { get; set; }
        public string NAMAPEGAWAIPENGAJU { get; set; }
        public string ALAMATPEMOHON { get; set; }
        public string BERKASID { get; set; }
        public string NOMORBERKAS { get; set; }
        public string KODEBILLING { get; set; }
        public string NTPN { get; set; }
        public string JUMLAHBAYAR { get; set; }
        public string NOMORREKENING { get; set; }
        public string NAMABANK { get; set; }
        public string NPWP { get; set; }
        public string NAMAREKENING { get; set; }
        public string SETORANPNBP { get; set; }
        public string PERMOHONANPENGEMBALIAN { get; set; }
        public string NOMORSURAT { get; set; }
        public int STATUSPENGEMBALIAN { get; set; }
        public string NPWPPEGAWAIPENGAJU { get; set; }
        public string TANGGALPENGAJU { get; set; }
    }

    public class RincianAlokasiList
    {
        public decimal? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string KANTORID { get; set; }
        public string KODESATKER { get; set; }
        public string NAMASATKER { get; set; }
        public string TIPE { get; set; }
        public decimal? ALOKASI { get; set; }
        public decimal? PAGU { get; set; }
        public decimal? REALISASIBELANJA { get; set; }
        public decimal? PERSENALOKASI { get; set; }
        public decimal? PERSENPAGU { get; set; }
        public decimal? TOTALALOKASI { get; set; }
        public decimal? TOTALPAGU { get; set; }
        public int? TAHUN { get; set; }
        public string NAMAOUTPUT { get; set; }
        public String KODEOUTPUT { get; set; }
        public String PROGRAMID { get; set; }
        public String NAMAPROGRAM { get; set; }
    }

    public class RincianAlokasiListDetail
    {
        public int? RNumber { get; set; }
        public decimal? Total { get; set; }
        public string KANTORID { get; set; }
        public string KODESATKER { get; set; }
        public string NAMASATKER { get; set; }
        public string TIPE { get; set; }
        public decimal? ALOKASI { get; set; }
        public decimal? PAGU { get; set; }
        public decimal? REALISASIBELANJA { get; set; }
        public decimal? PERSENALOKASI { get; set; }
        public decimal? PERSENPAGU { get; set; }
        public decimal? TOTALALOKASI { get; set; }
        public decimal? TOTALPAGU { get; set; }
        public int? TAHUN { get; set; }
        public string NAMAOUTPUT { get; set; }
        public String KODEOUTPUT { get; set; }
        public String PROGRAMID { get; set; }
        public String NAMAPROGRAM { get; set; }
    }

    public class CariRincianAlokasiSatker
    {
        public string KODESATKER { get; set; }
        public string TAHUN { get; set; }
        public string KANTORID { get; set; }
        public string PROGRAMID { get; set; }
    }

    public class GetSatkerList
    {
        public string KantorID { get; set; }
        public int? TipeKantorID { get; set; }
        public string Induk { get; set; }
        public string Kode { get; set; }
        public string KodeSatker { get; set; }
        public string Satker_Induk { get; set; }
        public string Nama_Satker { get; set; }
        public string NamaAlias { get; set; }
        public int? Tahun { get; set; }
        public int? StatusAktif { get; set; }
    }

    public class GetProgramList
    {
        public string ProgramId { get; set; }
        public int? TipemanfaatId { get; set; }
        public string Nama { get; set; }
        public string Kode { get; set; }
        public string TipeOps { get; set; }
        public int? StatusAktif { get; set; }
    }

    public class GetManfaat
    {
        public string KODESATKER { get; set; }
        public string TAHUN { get; set; }
        public string KANTORID { get; set; }
        public string PROGRAMID { get; set; }
        public string NAMAPROGRAM { get; set; }

    }

    public class RincianBiaya
    {
        public int tarif { get; set; }
        public string nama { get; set; }
        public int nilaioperasional { get; set; }
        public string tipenilaioperasional { get; set; }
        public string kodepenerimaan { get; set; }
    }

    public class RenaksiList
    {
        public string KODE { get; set; }
        public string NAMA { get; set; }
        public string KANTORID { get; set; }
        public string KODESATKER { get; set; }
        public string NAMAKANTOR { get; set; }
        public string PROGRAMID { get; set; }
        public string MANFAATID { get; set; }
        public string TIPE { get; set; }
        public decimal PAGU { get; set; }
        public decimal ANGGJAN { get; set; }
        public decimal ANGGFEB { get; set; }
        public decimal ANGGMAR { get; set; }
        public decimal ANGGAPR { get; set; }
        public decimal ANGGMEI { get; set; }
        public decimal ANGGJUN { get; set; }
        public decimal ANGGJUL { get; set; }
        public decimal ANGGAGT { get; set; }
        public decimal ANGGSEP { get; set; }
        public decimal ANGGOKT { get; set; }
        public decimal ANGGNOV { get; set; }
        public decimal ANGGDES { get; set; }
        public string EVIDENTID { get; set; }
        public string EVIDENTNAME { get; set; }
        public string EVIDENPATH { get; set; }
        public string EVIDENCREATBY { get; set; }
        public string EVIDENSTATUS { get; set; }
        public string EVIDENMANFAATID { get; set; }
        public int PERSETUJUAN1 { get; set; }
        public int PERSETUJUAN2 { get; set; }
        public int STATUSREVISI { get; set; }
        public string KETERANGANPENOLAKAN { get; set; }
    }

    public class DataSatker
    {
        public string kantorid { get; set; }
        public string namakantor { get; set; }
    }

    public class DataRenaksiPusat
    {
        public string kantorid { get; set; }
        public string manfaatid { get; set; }
        public string namaprogram { get; set; }
        public string tipe { get; set; }
        public decimal nilaianggaran { get; set; }
        public int rankjan { get; set; }
        //public int rankfeb { get; set; }
        //public int rankmar { get; set; }
        //public int rankapr { get; set; }
        //public int rankmei { get; set; }
        //public int rankjun { get; set; }
        //public int rankjul { get; set; }
        //public int rankagt { get; set; }
        //public int ranksep { get; set; }
        //public int rankokt { get; set; }
        //public int ranknov { get; set; }
        //public int rankdes { get; set; }
        public decimal anggjan { get; set; }
        public decimal anggfeb { get; set; }
        public decimal anggmar { get; set; }
        public decimal anggapr { get; set; }
        public decimal anggmei { get; set; }
        public decimal anggjun { get; set; }
        public decimal anggjul { get; set; }
        public decimal anggagt { get; set; }
        public decimal anggsep { get; set; }
        public decimal anggokt { get; set; }
        public decimal anggnov { get; set; }
        public decimal anggdes { get; set; }
        public string programid { get; set; }
        public string kode { get; set; }
        public string evidenpath { get; set; }
        public int PERSETUJUAN1 { get; set; }
        public int PERSETUJUAN2 { get; set; }
        public string namakantor { get; set; }
    }
    public class getHistoryRenaksi
    {
        public string kantorid { get; set; }
        public string namakantor { get; set; }
        public string manfaatid { get; set; }
        public string manfaat_manfaatid { get; set; }
        public string namaprogram { get; set; }
        public string tipe { get; set; }
        //public string nilaianggaran { get; set; }
        public int rankjan { get; set; }
        public decimal anggjan { get; set; }
        public decimal anggfeb { get; set; }
        public decimal anggmar { get; set; }
        public decimal anggapr { get; set; }
        public decimal anggmei { get; set; }
        public decimal anggjun { get; set; }
        public decimal anggjul { get; set; }
        public decimal anggagt { get; set; }
        public decimal anggsep { get; set; }
        public decimal anggokt { get; set; }
        public decimal anggnov { get; set; }
        public decimal anggdes { get; set; }
        public string programid { get; set; }
        public string kode { get; set; }
        public string evidenpath { get; set; }
        public string keteranganpenolakan { get; set; }
        public int persetujuan1 { get; set; }
        public int persetujuan2 { get; set; }
        public DateTime insertdate { get; set; }
    }


    public class DataRenaksiHistory
    {
        public string kantorid { get; set; }
        public string namakantor { get; set; }
        public string manfaatid { get; set; }
        public string manfaat_manfaatid { get; set; }
        public string namaprogram { get; set; }
        public string tipe { get; set; }
        public decimal nilaianggaran { get; set; }
        public int rankjan { get; set; }
        public decimal anggjan { get; set; }
        public decimal anggfeb { get; set; }
        public decimal anggmar { get; set; }
        public decimal anggapr { get; set; }
        public decimal anggmei { get; set; }
        public decimal anggjun { get; set; }
        public decimal anggjul { get; set; }
        public decimal anggagt { get; set; }
        public decimal anggsep { get; set; }
        public decimal anggokt { get; set; }
        public decimal anggnov { get; set; }
        public decimal anggdes { get; set; }
        public string programid { get; set; }
        public string kode { get; set; }
        public string evidenpath { get; set; }
        public string keteranganpenolakan { get; set; }
        public int persetujuan1 { get; set; }
        public int PERSETUJUAN2 { get; set; }
        public string insertdate { get; set; }
    }

    public class ProfilPegawai
    {
        public int ada { get; set; }
    }

    public class GetEviden
    {
        public string EVIDENTID { get; set; }
        public string EVIDENTNAME { get; set; }
        public string EVIDENPATH { get; set; }
        public string EVIDENCREATBY { get; set; }
        public string EVIDENSTATUS { get; set; }
        public string EVIDENMANFAATID { get; set; }
    }

    public class RincianAlokasiTotal
    {
        public decimal TOTAL_REALISASIBELANJA { get; set; }
        public decimal TOTAL_ALOKASI { get; set; }
        public decimal TOTAL_PAGU { get; set; }
    }

    public class HistoryPenerimaan
    {
        public string no { get; set; }
        public string monevpenerimaanid { get; set; }
        public string kodesatker { get; set; }
        public string evaluasi { get; set; }
        public string renaksi { get; set; }
        public int? read { get; set; }
        public string create_date { get; set; }
    }

    public class HistoryBelanja
    {
        public string no { get; set; }
        public string monevbelanjaid { get; set; }
        public string kodesatker { get; set; }
        public string evaluasi { get; set; }
        public string renaksi { get; set; }
        public int? read { get; set; }
    }

    public class DataPenerimaan
    {
        public string penerimaanid { get; set; }
        public int statusalokasi { get; set; }
        public DateTime tanggal { get; set; }
        public string namaprosedur { get; set; }
        public string kodespopp { get; set; }
        public decimal jumlah { get; set; }
        public decimal nilaiakhir { get; set; }
        public string kodesatker { get; set; }  
        public string namakantor { get; set; }
        public string kodepenerimaan { get; set; }
        public string jenispenerimaan { get; set; }
        public string namaprogram { get; set; }
        public string kodebilling { get; set; }
        public decimal nomorberkas { get; set; }
        public decimal tahunberkas { get; set; }
    }
}
