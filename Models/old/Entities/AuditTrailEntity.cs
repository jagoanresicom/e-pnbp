using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{
    public class AuditTrail
    {
        [Key]
        public string log_id { get; set; }
        public string log_name { get; set; }
        public string log_create_by { get; set; }
        public DateTime? log_create_date { get; set; }
        public DateTime? log_tanggal_kirim { get; set; }
        public DateTime? log_tanggal_proses { get; set; }
        public DateTime? log_tanggal_kembalikan { get; set; }
        public DateTime? log_tanggal_selesai { get; set; }
        public string log_url { get; set; }
        public string log_nomor_surat { get; set; }
        public string log_kantorid { get; set; }
        public string log_dataid { get; set; }
        public string nama_satker { get; set; }
        public string kodebilling { get; set; }
        public string ntpn { get; set; }
        public string namaprogram { get; set; }


    }
}