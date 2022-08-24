using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{
    public class Tahun
    {
        public string value { get; set; }
        public string tahun { get; set; }
    }

    public class CharDashboard
    {
        public List<decimal> Penerimaan { get; set; }
        public List<decimal> Operasional { get; set; }
        public List<decimal> OPS { get; set; }
        public List<decimal> NONOPS { get; set; }
        public List<decimal> MP { get; set; }
        public List<decimal> Alokasi { get; set; }
        public List<Tahun> lstahun { get; set; }
        public string tahun { get; set; }
        public List<decimal> NilaiMp { get; set; }
        public List<decimal> PersenMp { get; set; }
        public List<decimal> jumlah { get; set; }
        public List<decimal> bulan { get; set; }
        public List<decimal> belanjaNonOps { get; set; }
        public List<decimal> belanjaOps { get; set; }
        public List<string> bulanNonOps { get; set; }
        public List<string> bulanOps { get; set; }
        public List<decimal> jumlahops { get; set; }
        public List<decimal> bulanAlokOps { get; set; }
    }

    public class JumlahPenerimaanOperasional
    {
        [Key]
        public decimal? Penerimaan { get; set; }
        public decimal? Operasional { get; set; }
        public decimal? total_penerimaan { get; set; }
        public decimal? total_oprasional { get; set; }
    }

    public class ChartEntities
    {       
        public string ChartName { get; set; }
        public string ChartTitle { get; set; }
        public string ChartMaxValue { get; set; }
        public string ChartValue { get; set; }
    }

    public class RekapPenerimaan
    {
        public int tahun { get; set; }
        public int bulan { get; set; }
        public decimal jumlahberkas { get; set; }
        public decimal penerimaan { get; set; }
        public decimal operasional { get; set; }
    }

    public class RekapAlokasi
    {
        public string tahun { get; set; }
        public string bulan { get; set; }
        public string tipemanfaat { get; set; }
        public decimal alokasi { get; set; }
    }

    public class TotalPenerimaan
    {
        public decimal? jumlah { get; set; }
        public decimal? operasional { get; set; }
        public decimal? persentase { get; set; }
        public decimal? totalalokasi { get; set; }
    }

    public class TotalBelanja
    {
        public decimal? jumlah { get; set; }
    }

    public class mp_belanja
    {
        public decimal operasional { get; set; }
        public decimal totalalokasi { get; set; }
        public int persentase { get; set; }
    }

    public class mp_belanja_nonops
    {
        public decimal operasional { get; set; }
        public decimal totalalokasi { get; set; }
        public int persentase { get; set; }
    }

    public class Mp
    {
        public int tahun { get; set; }
        public int bulan { get; set; }
        public Decimal? NilaiMp { get; set; }
        public Decimal? PersenMp { get; set; }
    }

    public class paguvsmp
    {
        public Decimal? teralokasi { get; set; }
    }

    public class AlokasiBelanja
    {
        public int tahun { get; set; }
        public int bulan { get; set; }
        public decimal jumlah { get; set; }
        public decimal jumlahops { get; set; }
        public int bulanAlokOps { get; set; }
    }

    public class BelanjaNonOps
    {
        public int tahun { get; set; }
        public string bulanNonOps { get; set; }
        public decimal belanjaNonOps { get; set; }
    }

    public class BelanjaOps
    {
        public int tahun { get; set; }
        public string bulanOps { get; set; }
        public decimal belanjaOps { get; set; }
    }
}