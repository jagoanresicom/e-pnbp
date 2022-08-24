using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{
    public class Satkernomenklatur
    {
        [Key]
        public string kantorid { get; set; }
        //public int tipekantorid { get; set; }
        //public string induk { get; set; }
        public string kode { get; set; }
        public string kodesatker { get; set; }
        public string satker_induk { get; set; }
        public string nama_satker { get; set; }
        public string namaalias { get; set; }
        public int tahun { get; set; }
        public int statusaktif { get; set; }
    }
}
