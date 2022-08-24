using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{
    public class ProfilPengguna
    {
        [Key]
        public string profileid { get; set; }
        public string nama { get; set; }
        public string aktif { get; set; }
        public int eselon { get; set; }
    }

    public class DataPengguna
    {
        [Key]
        public string idpengguna { get; set; }
        public string idpegawai { get; set; }
        public string namapengguna { get; set; }
        public string namalengkap { get; set; }
        public string displaynip { get; set; }

    }
}