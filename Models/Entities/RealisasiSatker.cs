using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Pnbp.Entities
{

    public class RealisasiSatker
    {
        public string kodeSatker { get; set; }
        public string namaSatker { get; set; }
        public decimal? realisasi { get; set; }
        public decimal? jumlahLayanan { get; set; }
        public string kodeProvinsi { get; set; }
        public string bulan { get; set; }
        public decimal? tahun { get; set; }
    }

    public class RealisasiSatkerResponse
    {
        public string kodeSatker { get; set; }
        public string namaSatker { get; set; }
        public string realisasi { get; set; }
        public decimal? jumlahLayanan { get; set; }
        public string kodeProvinsi { get; set; }
        public string bulan { get; set; }
        public decimal? tahun { get; set; }
    }

}
