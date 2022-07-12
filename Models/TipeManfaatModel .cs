using System;
using System.Collections.Generic;
using System.Linq;

namespace Pnbp.Models
{
    public class TipeManfaatModel
    {
       
        public List<Entities.TipeManfaat> getAll()
        {
            var _dt = new List<Entities.TipeManfaat>();

            try
            {
                List<object> parameters = new List<object>();
                using (var ctx = new PnbpContext())
                {
                    string query = @"SELECT * FROM TIPEMANFAAT";

                    _dt = ctx.Database.SqlQuery<Entities.TipeManfaat>(query).ToList<Entities.TipeManfaat>();
                }
            } 
            catch(Exception e)
            {
                _ = e.StackTrace;
            }

            return _dt;
        }

    }
}