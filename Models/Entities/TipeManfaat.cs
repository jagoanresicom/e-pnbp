using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{
    public class TipeManfaat
    {
        [Key]
        public int TipeManfaatId { get; set; }
        public string Tipe { get; set; }
    }
}
