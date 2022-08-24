using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pnbp.Models
{
    public class BerandaPenerimaan
    {
        public static List<Entities.RekapPenerimaan> dtRekapPenerimaan(string pTahun, string pSatker)
        {
            var _result = new List<Entities.RekapPenerimaan>();
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                List<object> lstparams = new List<object>();

                string query =
                   @" select tahun, bulan, jumlahberkas, penerimaan, operasional from rekappenerimaan where tahun = :Tahun ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("Tahun", pTahun));

                if(!string.IsNullOrEmpty(pSatker))
                {
                    query += " and kantorid = :kantorid ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kantorid", pSatker));
                }

                query += "order by bulan ";

                query = sWhitespace.Replace(query, " ");
                var parameters = lstparams.ToArray();
                _result = ctx.Database.SqlQuery<Entities.RekapPenerimaan>(query, parameters).ToList();

            }
            return _result;
        }

        public static List<Entities.RekapAlokasi> dtRekapAlokasi(string pTahun, string pTipe)
        {
            var _result = new List<Entities.RekapAlokasi>();
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                List<object> lstparams = new List<object>();

                string query =
                    @" select
                          to_char(r1.bulan) as bulan, sum(nvl(teralokasi,0)) as alokasi
                        from
                          (SELECT
                             level as bulan
                           FROM
                             DUAL
                           CONNECT BY
                             LEVEL <= 12) r1
                          left join rekapalokasi r2 on
                            r1.bulan = r2.bulan
                            and r2.tahun = :param1
                            and r2.statusalokasi = 1
                            ";
                if(!string.IsNullOrEmpty(pTipe))
                {
                    query += " and r2.tipemanfaat = :param2";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pTipe));
                }
                query += @" group by
                          r1.bulan
                        order by r1.bulan";

                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("param1", pTahun));                

                query = sWhitespace.Replace(query, " ");
                var parameters = lstparams.ToArray();
                _result = ctx.Database.SqlQuery<Entities.RekapAlokasi>(query, parameters).ToList();

            }
            return _result;
        }

        public static List<Pnbp.Entities.Tahun> lsTahunPenerimaan()
        {
            List<Pnbp.Entities.Tahun> result = new List<Pnbp.Entities.Tahun>();
            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");

                string query =
                   @" select distinct to_char(tahun) as value, to_char(tahun) as tahun from rekappenerimaan order by tahun desc";
                result = ctx.Database.SqlQuery<Pnbp.Entities.Tahun>(query).ToList();

            }
            return result;
        }
    }
}