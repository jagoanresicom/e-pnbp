using System;
using System.Collections.Generic;
using System.Linq;

namespace Pnbp.Models
{
    public class ProgramModel
    {
       
        public List<Entities.Programs> getAll(int page, int offset, string namaProgram)
        {
            var _dt = new List<Entities.Programs>();

            try
            {
                List<object> parameters = new List<object>();
                using (var ctx = new PnbpContext())
                {
                    var conditionQuery = "";
                    if (!string.IsNullOrEmpty(namaProgram))
                    {
                        conditionQuery = $" and lower(nama) like '%{namaProgram}%'";
                        //parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nama", ));
                    }

                    parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("limit", ((page - 1) * offset) + 1));
                    parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("offset", (page * offset)));

                    string query = $@" 
                                        SELECT d.*, t.tipe tipemanfaat FROM (
	                                        SELECT 
	                                        ROW_NUMBER() OVER (ORDER BY Tahun DESC, NAMA ASC) RNUMBER, 
                                            (SELECT count(*) FROM PROGRAM WHERE STATUSAKTIF = 1 {conditionQuery}) Total, 
	                                        p.* 
	                                        FROM PROGRAM p 
                                            WHERE p.STATUSAKTIF = 1 {conditionQuery} 
                                        ) d 
                                        LEFT JOIN TIPEMANFAAT t ON d.tipemanfaatid = t.TIPEMANFAATID 
                                        WHERE d.rnumber BETWEEN :limit AND :offset 
                                    ";
                    
                    _dt = ctx.Database.SqlQuery<Entities.Programs>(query, parameters.ToArray()).ToList();
                }
            } 
            catch(Exception e)
            {
                _ = e.StackTrace;
            }

            return _dt;
        }

        public Entities.Programs getById(string programId)
        {
            var _dt = new Entities.Programs();

            try
            {
                List<object> parameters = new List<object>();
                using (var ctx = new PnbpContext())
                {
                    string query = $@"SELECT * FROM PROGRAM WHERE PROGRAMID = :programId";
                    parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("programId", programId));
                    _dt = ctx.Database.SqlQuery<Entities.Programs>(query, parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return _dt;
        }

        public bool Insert(Entities.Programs program)
        {
            bool res = false;
            
            try
            {
                using (var ctx = new PnbpContext())
                {
                    var parameters = new List<object>();
                    parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tipeManfaatId", program.TipeManfaatId));
                    parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("nama", program.Nama));
                    parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kode", program.Kode));
                    parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tipeOps", program.TipeOps));

                    var row = ctx.Database.ExecuteSqlCommand(@"
                    INSERT INTO PROGRAM 
                    (PROGRAMID, TIPEMANFAATID, NAMA, KODE, STATUSAKTIF, TIPEOPS, TAHUN) 
                    VALUES(RAWTOHEX(SYS_GUID()), :tipeManfaatId, :nama, :kode, 1, :tipeOps, EXTRACT(YEAR FROM SYSDATE))
                ", parameters.ToArray());
                    res = row > 0;
                }
            } 
            catch(Exception e)
            {
                _ = e.StackTrace;
            }
            
            return res;
        }

        public bool Update(Entities.Programs program)
        {
            bool res = false;

            try
            {
                using (var ctx = new PnbpContext())
                {
                    var row = ctx.Database.ExecuteSqlCommand($@"
                    UPDATE PROGRAM 
                    SET TIPEMANFAATID = {program.TipeManfaatId}, NAMA = '{program.Nama}', KODE = '{program.Kode}', TIPEOPS = '{program.TipeOps}' 
                    WHERE PROGRAMID = '{program.ProgramId}' 
                ");
                    res = row > 0;
                }
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return res;
        }

    }
}