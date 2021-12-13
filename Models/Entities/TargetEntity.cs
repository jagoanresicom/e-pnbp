using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{
    public class Target
    {
        [Key]
        public string kantorid { get; set; }
        public string KodeTarget { get; set; }
        public string NamaProsedur { get; set; }
        public decimal? NilaiTarget { get; set; }
        public string NamaKantor { get; set; }
        public string StatusTarget { get; set; }
        //public string KantorId { get; set; }
        public string JenisLayanan { get; set; }
        public decimal? targetfisik { get; set; }

    }

    public class Total
    {
        [Key]
        public decimal? totalpagu { get; set; }
        public decimal? totalrealisasi { get; set; }
        public decimal? alokasi { get; set; }
        public decimal? teralokasi { get; set; }

    }

    public class GetJenisPenerimaan
    {
        public string JenisPenerimaan { get; set; }
        public string namakantor { get; set; }
    }

    public class GetProgram
    {
        public string NamaKantor { get; set; }
        public string JenisPenerimaan { get; set; }
        public string NamaProsedur { get; set; }
        public string KantorId { get; set; }
    }
}
