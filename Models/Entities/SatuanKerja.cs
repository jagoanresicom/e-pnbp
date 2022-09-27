using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{

    public class SatuanKerja
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

}
