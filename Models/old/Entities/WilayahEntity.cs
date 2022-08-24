using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{
    public class Wilayah
    {
        [Key]
        public string id { get; set; }
        public string kode { get; set; }
        public string nama { get; set; }
    }
}
