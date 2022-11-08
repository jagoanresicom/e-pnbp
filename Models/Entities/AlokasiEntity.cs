using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{
    public class TotalPemanfaatan
    {
        public decimal? Nilaianggaran { get; set; }
        public decimal? Nilaialokasi { get; set; }
    }

    public class CalculateResult
    {
        public decimal? Result { get; set; }    
        public string Tanggal { get; set; }
    }

    public class Satker
    {
        public string kantorid { get; set; }
        public string namakantor { get; set; }
    }

    public class AlokasiSatker
    {
        public string kantorid { get; set; }
        public Decimal? nilaialokasi { get; set; }
    }

    public class getManfaatAlokasi
    {
        public string kantorid { get; set; }
        public Int64 nilaialokasi { get; set; }
    }

    public class getDataSatker
    {
        public string kantorid { get; set; }
        public string kodesatker { get; set; }
        public string namakantor { get; set; }
        public Int64 nilaianggaran { get; set; }
        public Int64 totalalokasi { get; set; }
        public Int64 nilaialokasi { get; set; }
        public Int64 sisaalokasi { get; set; }
        public int urutan { get; set; }
        public decimal mp { get; set; }
    }

    public class getDataSatkerEselon
    {
        public string id { get; set; }
        public string kantorid { get; set; }
        public string kodesatker { get; set; }
        public string nama_satker { get; set; }
        public Int64 pagu { get; set; }
        public Int64 totalalokasi { get; set; }
        public Int64 realisasi { get; set; }
        //public Int64 sisaalokasi { get; set; }
        //public int urutan { get; set; }
        public decimal mp { get; set; }
        public Int64 alokasi { get; set; }
        public string approve { get; set; }
    }

    public class getDetailAnggaran
    {
        public string kode { get; set; }
        public string namaprogram { get; set; }
        public Decimal? nilaialokasi { get; set; }
    }

    public class FormAlokasi
    {
        public string Tahap { get; set; }
        public string Total { get; set; }
        public string Sudahalokasi { get; set; }
        public decimal? Dapatalokasi { get; set; }
        public decimal? Teralokasi { get; set; }

        [Required(ErrorMessage = "Tanggal harus diisi")]
        public string Tglpenerimaan { get; set; }

        [Required(ErrorMessage = "Input Alokasi harus diisi")]
        public decimal? InputAlokasi { get; set; }
        public string JenisAlokasi { get; set; }
    }

    public class FormAlokasiSummaryDetail
    {
        public string id { get; set; }
        public string satker { get; set; }
    }

    public class AlokasiJob
    {
        public string Namaprosedur { get; set; }
        public string Mulai { get; set; }
        public string Selesai { get; set; }
        public string Status { get; set; }
        public decimal? Result { get; set; }
        public decimal? Inputalokasi { get; set; }
        public decimal? Rnumber { get; set; }
    }

    public class ManfaatPrioritas
    {
        public string Tipe { get; set; }
        public int? Prioritaskegiatan { get; set; }
        public decimal Nilaianggaran { get; set; }
        public decimal Nilaialokasi { get; set; }
        public decimal Rnumber { get; set; }
    }

    public class AlokasiRows
    {
        public string ManfaatId { get; set; }
        public int? Tahun { get; set; }
        public string Kode { get; set; }
        public string Kodesatker { get; set; }
        public string NamaKantor { get; set; }
        public string NamaProgram { get; set; }
        public string PrioritasKegiatan { get; set; }
        public decimal? NilaiAnggaran { get; set; }
        public decimal? SudahAlokasi { get; set; }
        public decimal? NilaiAlokasi { get; set; }
        public decimal? Rnumber { get; set; }

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
    }

    public class RekapAlokasiRows
    {
        public string rekapalokasiid { get; set; }
        public string tahun { get; set; }
        public string bulan { get; set; }
        public string tglalokasi { get; set; }
        public string tipemanfaat { get; set; }
        public decimal alokasi { get; set; }
        public int statusalokasi { get; set; }
        public int urutan { get; set; }
    }

    public class DetailRekapAlokasiRows
    {
        public int urutan { get; set; }
        public string manfaatid { get; set; }
        public string namakantor { get; set; }
        public string namaprogram { get; set; }
        public decimal nilaianggaran { get; set; }
        public decimal nilaialokasi { get; set; }
    }

    public class FilterRekapAlokasi
    {
        public string tahun { get; set; }
        public List<Tahun> lstahun { get; set; }
        public List<RekapAlokasiRows> lsrekapalokasi { get; set; }
    }

    public class getDataApprove
    {
        public string kantorid { get; set; }
        public string kodesatker { get; set; }
        public string nama_satker { get; set; }
        public Int64 pagu { get; set; }
        public Int64 totalalokasi { get; set; }
        public Int64 realisasi { get; set; }
        public decimal mp { get; set; }
        public Int64 alokasi { get; set; }
    }

    public class getDataManfaatNonOps
    {
        public string manfaatid { get; set; }
        public decimal? anggjan { get; set; }
        public int? tahun { get; set; }
        public string tipe { get; set; }
    }

    public class getAlokasi
    {
        public string ManfaatId { get; set; }
        public int? Tahun { get; set; }
        public string Kode { get; set; }
        public string Kodesatker { get; set; }
        public string NamaKantor { get; set; }
        public string NamaProgram { get; set; }
        public decimal? PrioritasKegiatan { get; set; }
        public decimal? NilaiAnggaran { get; set; }
        public decimal? SudahAlokasi { get; set; }
        public decimal? NilaiAlokasi { get; set; }
        public decimal? Rnumber { get; set; }

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
    }

















    // new Sangkuriang
    public class TemplateAlokasi
    {
        public string KodeSatker { get; set; }
        public string Amount { get; set; }
    }

    public class TempAlokasi
    {
        public decimal No { get; set; }
        public string KodeSatker { get; set; }
        public string NamaSatker { get; set; }
        public string Pagu { get; set; }
        public string Alokasi { get; set; }
        public decimal Valid { get; set; }
    }

    public class AlokasiSatkerV2
    {
        public decimal No { get; set; }
        public string AlokasiSatkerId { get; set; }
        public string KodeSatker { get; set; }
        public string NamaSatker { get; set; }
        public Decimal Pagu { get; set; }
        public Decimal Alokasi { get; set; }
        public Decimal Tahun { get; set; }
        public string TanggalBuat { get; set; }
        public string TanggalUbah { get; set; }
        public decimal Revisi { get; set; }
        public List<AlokasiSatkerRevisi> DaftarRevisi { get; set; }
        public List<AlokasiSatkerRevisi> DaftarTotalRevisiAlokasi { get; set; }
        public string TempAlokasi { get; set; }
        public bool IsNilaiBaru { get; set; }

    }

    public class AlokasiSatkerDetail
    {
        public List<AlokasiSatkerV2> Data { get; set; }
        public List<AlokasiSatkerRevisi> Total { get; set; }
    }

    public class AlokasiSatkerRevisi
    {
        public decimal Revisi { get; set; }
        public decimal Alokasi { get; set; }
    }

    public class AlokasiSatkerSummary
    {
        public decimal No { get; set; }
        public string AlokasiSatkerSummaryId { get; set; }
        public Decimal Mp { get; set; }
        public Decimal Pagu { get; set; }
        public Decimal Alokasi { get; set; }
        public Decimal Belanja { get; set; }
        public Decimal Tahun { get; set; }
        public string TanggalBuat { get; set; }
        public string TanggalUbah { get; set; }
        public Decimal Revisi { get; set; }
    }

    public class DataProsesAlokasi
    {
        public string KantorId { get; set; }
        public string Tipe { get; set; }
        public string ProgramId { get; set; }
        public string ProgramNama { get; set; }
        public string NamaSatker { get; set; }
        public string KodeSatker { get; set; }
        public string Kegiatan { get; set; }
        public string Output { get; set; }
        public string Akun { get; set; }
        public double Amount { get; set; }

    }

}