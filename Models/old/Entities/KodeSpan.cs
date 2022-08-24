using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{
    public class KodeSpan
    {
        [Key]
        public string KodeOutput { get; set; }
        public string NamaProgram { get; set; }
        public string Tipe { get; set; }
        public string Kode { get; set; }
        public string Kegiatan { get; set; }
    }
}
