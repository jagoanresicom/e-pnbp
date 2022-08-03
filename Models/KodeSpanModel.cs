using System;
using System.Collections.Generic;
using System.Linq;

namespace Pnbp.Models
{
    public class KodeSpanModel
    {
       
        public List<Entities.KodeSpan> dtKodeSpan(int status, string search)
        {
            var _dt = new List<Entities.KodeSpan>();

            try
            {
                List<object> parameters = new List<object>();
                using (var ctx = new PnbpContext())
                {
                    string query =
                     @"SELECT 
                        k.* 
                        FROM KODESPAN k 
                        JOIN (
	                        SELECT kegiatan, OUTPUT
	                        FROM SPAN_BELANJA sb 
	                        WHERE 
	                        sb.TAHUN = EXTRACT (YEAR FROM sysdate) AND 
	                        sb.SUMBER_DANA = 'D'
	                        GROUP BY kegiatan, output
                        ) sb ON k.KODEOUTPUT = (sb.KEGIATAN || '.' || sb.OUTPUT)
                     ";

                    //{ id: 1, label: "Semua"}
                    //{ id: 2, label: "Belum Lengkap" }
                    //{ id: 3, label: "Lengkap"}
                    if (status != 1)
                    {
                        if (status == 2)
                        {
                            query += "WHERE NAMAPROGRAM IS NULL AND tipe IS NULL";
                        }
                        else
                        {
                            query += "WHERE NAMAPROGRAM IS NOT NULL AND tipe IS NOT NULL";
                        }
                    }

                    if (!string.IsNullOrEmpty(search))
                    {
                        query += " and kodeoutput like '%'||:param1||'%' ";
                        Oracle.ManagedDataAccess.Client.OracleParameter param1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", search);
                        parameters.Add(param1);
                    }

                    query += " order by k.kodeoutput asc ";

                    _dt = ctx.Database.SqlQuery<Entities.KodeSpan>(query, parameters.ToArray()).ToList<Entities.KodeSpan>();
                }
            } 
            catch(Exception e)
            {
                _ = e.StackTrace;
            }

            return _dt;
        }

        public Entities.KodeSpan getKodeSpanByKodeOutput(string kodeoutput)
        {
            var _result = new Entities.KodeSpan();
            try
            {
                var parameters = new object[1];

                using (var ctx = new PnbpContext())
                {
                    string query =
                     @"SELECT * FROM KODESPAN k WHERE KODEOUTPUT = :param1";
                    Oracle.ManagedDataAccess.Client.OracleParameter param1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", kodeoutput);
                    parameters[0] = param1;
                    _result = ctx.Database.SqlQuery<Entities.KodeSpan>(query, parameters).SingleOrDefault<Entities.KodeSpan>();
                }
            } 
            catch(Exception e)
            {
                _ = e.StackTrace;
            }

            return _result;
        }

        public bool update(string kodeOutput, string namaProgram, string tipe)
        {
            bool _result = false;
            try
            {
                List<object> parameters = new List<object>();

                var detail = getKodeSpanByKodeOutput(kodeOutput);
                if (detail != null)
                {
                    using (var ctx = new PnbpContext())
                    {
                        string query =
                         @"UPDATE KODESPAN SET NAMAPROGRAM = :namaProgram, TIPE = :tipe WHERE KODEOUTPUT = :kodeOutput";
                        Oracle.ManagedDataAccess.Client.OracleParameter pKodeOutput = new Oracle.ManagedDataAccess.Client.OracleParameter("kodeOutput", kodeOutput);
                        Oracle.ManagedDataAccess.Client.OracleParameter pNamaProgram = new Oracle.ManagedDataAccess.Client.OracleParameter("namaProgram", namaProgram);
                        Oracle.ManagedDataAccess.Client.OracleParameter pTipe = new Oracle.ManagedDataAccess.Client.OracleParameter("tipe", tipe);
                        parameters.Add(pNamaProgram);
                        parameters.Add(pTipe);
                        parameters.Add(pKodeOutput);

                        var res = ctx.Database.ExecuteSqlCommand(query, parameters.ToArray());
                        _result = res > 0;
                    }
                }
            }
            catch(Exception e)
            {
                _ = e.StackTrace;
            }

            return _result;
        }
    }
}