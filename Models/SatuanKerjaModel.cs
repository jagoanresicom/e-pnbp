using Pnbp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pnbp.Models
{
    public class SatuanKerjaModel
    {

        public List<SatuanKerja> ListSatuanKerja()
        {
            var list = new List<SatuanKerja>();
            List<object> lstparams = new List<object>();
            var sql = "SELECT * FROM PNBP.SATKER WHERE STATUSAKTIF = '1'";
            try
            {
                using (var ctx = new PnbpContext())
                {
                    list = ctx.Database.SqlQuery<SatuanKerja>(sql, lstparams.ToArray()).ToList();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return list;
        }


    }
}