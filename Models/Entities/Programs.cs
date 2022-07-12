using System;
using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{
    public class Programs
    {
        [Key]
        public string ProgramId { get; set; }
        public int RNumber { get; set; }
        public int? TipeManfaatId { get; set; }
        public string TipeManfaat { get; set; }
        public string Nama { get; set; }
        public string Kode { get; set; }
        public int? StatusAktif { get; set; }
        public string UserInsert { get; set; }
        public DateTime? InsertDate { get; set; }
        public string UserUpdate { get; set; }
        public DateTime? LastUpdate { get; set; }
        public int? PrioritasWilayahId { get; set; }
        public int? PrioritasKegiatan { get; set; }
        public string TipeOps { get; set; }
        public string Induk { get; set; }
        public int? Tipe { get; set; }
        public int? Tahun { get; set; }
        public string NamaKegiatan { get; set; }
        public string NamaProgram { get; set; }
        public int Total { get; set; }
    }
}
