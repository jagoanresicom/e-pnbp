using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{
    public class Api
    {
        [Key]
        public string MANFAATID { get; set; }
        public int TAHUN { get; set; }
        public string KANTORID { get; set; }
        public string NAMAKANTOR { get; set; }
        public string PROGRAMID { get; set; }
        public string NAMAPROGRAM { get; set; }
        public string TIPE { get; set; }
        public int? PRIORITASKEGIATAN { get; set; }
        public decimal? NILAIANGGARAN { get; set; }
        public decimal? NILAIALOKASI { get; set; }
        public int? STATUSFULLALOKASI { get; set; }
        public int? STATUSEDIT { get; set; }
        public int? STATUSAKTIF { get; set; }
        public string USERINSERT { get; set; }
        public DateTime? INSERTDATE { get; set; }
        public string USERUPDATE { get; set; }
        public DateTime? LASTUPDATE { get; set; }
        public Int64? TOTALGROUP { get; set; }
        public decimal? PERSENGROUP { get; set; }
        public decimal? PERSENPROGRAM { get; set; }
        public decimal? TOTALALOKASI { get; set; }
        public decimal? SISAALOKASI { get; set; }
        public string KODESATKER { get; set; }
        public int? PRIORITASASAL { get; set; }
        public decimal? ANGGJAN { get; set; }
        public Int64? RANKJAN { get; set; }
        public decimal? ANGGFEB { get; set; }
        public Int64? RANKFEB { get; set; }
        public decimal? ANGGMAR { get; set; }
        public Int64? RANKMAR { get; set; }
        public decimal? ANGGAPR { get; set; }
        public Int64? RANKAPR { get; set; }
        public decimal? ANGGMEI { get; set; }
        public Int64? RANKMEI { get; set; }
        public decimal? ANGGJUN { get; set; }
        public Int64? RANKJUN { get; set; }
        public decimal? ANGGJUL { get; set; }
        public Int64? RANKJUL { get; set; }
        public decimal? ANGGAGT { get; set; }
        public Int64? RANKAGT { get; set; }
        public decimal? ANGGSEP { get; set; }
        public Int64? RANKSEP { get; set; }
        public decimal? ANGGOKT { get; set; }
        public Int64? RANKOKT { get; set; }
        public decimal? ANGGNOV { get; set; }
        public Int64? RANKNOV { get; set; }
        public decimal? ANGGDES { get; set; }
        public Int64? RANKDES { get; set; }
        public Int64? ALOKJAN { get; set; }
        public Int64? ALOKFEB { get; set; }
        public Int64? ALOKMAR { get; set; }
        public Int64? ALOKAPR { get; set; }
        public Int64? ALOKMEI { get; set; }
        public Int64? ALOKJUN { get; set; }
        public Int64? ALOKJUL { get; set; }
        public Int64? ALOKAGT { get; set; }
        public Int64? ALOKSEP { get; set; }
        public Int64? ALOKOKT { get; set; }
        public Int64? ALOKNOV { get; set; }
        public Int64? ALOKDES { get; set; }
        public string KODE { get; set; }
        public Int64? STATUSREVISI { get; set; }
        public Int64? PERSETUJUAN1 { get; set; }
        public Int64? PERSETUJUAN2 { get; set; }
    }

    public class apitoken
    {
        [Key]
        public string MANFAATID { get; set; }
        public string APITOKENID { get; set; }
        public string APITOKENKODE { get; set; }
        public DateTime APITOKENVALIDUNTIL { get; set; }
        public string APITOKENIP { get; set; }
    }

    public class apiconfig
    {
        [Key]
        public string CONFIGTOKEN { get; set; }
        public int? CONFIGHOUREXPIRED { get; set; }
        public int? CONFIGSTATUS { get; set; }
    }




}
