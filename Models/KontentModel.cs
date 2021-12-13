using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pnbp.Models
{
    public class KontentModel
    {
        public int CekVersi(string id)
        {
            int result = 0;
            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    List<object> lstparams = new List<object>();
                    string sql = "SELECT versi FROM kontenaktif WHERE kontenaktifid = :id";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                    var parameters = lstparams.ToArray();
                    result = ctx.Database.SqlQuery<int>(sql, parameters).FirstOrDefault();
                }
            }
            return result;
        }

        public int JumlahKonten(string id)
        {
            int result = 0;

            string query = "SELECT count(*) FROM kontenaktif WHERE kontenaktifid = :id";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public string GetKontentAktif(string id)
        {
            string result = "";
            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    List<object> lstparams = new List<object>();
                    string sql = "SELECT kontenaktifid FROM kontenaktif WHERE kontenaktifid = :id";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                    var parameters = lstparams.ToArray();
                    result = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
                }
            }
            return result;
        }


        public Entities.TransactionResult SimpanKontenFile(string kantorid, string dokumenid, string judul, string petugas, string tanggaldokumen, string tipedokumen, out int versi)
        {
            versi = 0;
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = false, Pesan = "" };

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        string sql = "";
                        string kontenid = this.GetKontentAktif(dokumenid);
                        if (String.IsNullOrEmpty(kontenid))
                        {
                            // Insert mode
                            sql = "INSERT INTO KONTENAKTIF (KONTENAKTIFID,VERSI,TANGGALSISIP,PETUGASSISIP,TANGGALSUNTING,PETUGASSUNTING,TIPE,KANTORID,JUDUL,EKSTENSI) VALUES (:pId,0,TO_DATE(:pTanggal,'DD/MM/YYYY'),:pNamaPetugas,SYSDATE,:pNamaPetugasSunting,:pTipeDokumen,:pKantorId,:pJudul,'pdf')";
                            Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("pId", dokumenid);
                            Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("pTanggal", tanggaldokumen);
                            Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("pNamaPetugas", petugas);
                            Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("pNamaPetugasSunting", petugas);
                            Oracle.ManagedDataAccess.Client.OracleParameter p5 = new Oracle.ManagedDataAccess.Client.OracleParameter("pTipeDokumen", tipedokumen);
                            Oracle.ManagedDataAccess.Client.OracleParameter p6 = new Oracle.ManagedDataAccess.Client.OracleParameter("pKantorId", kantorid);
                            Oracle.ManagedDataAccess.Client.OracleParameter p7 = new Oracle.ManagedDataAccess.Client.OracleParameter("pJudul", judul);
                            object[] parameters = new object[7] { p1, p2, p3, p4, p5, p6, p7 };
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }
                        else
                        {
                            // Edit mode
                            sql = "INSERT INTO KONTENPASIF SELECT SYS_GUID(), KONTENAKTIFID, KONTEN, VERSI, TANGGALSISIP, PETUGASSISIP, TANGGALSUNTING, PETUGASSUNTING, TIPE, WARKAHID, BERKASID, KANTORID, JUDUL, EKSTENSI, TANGGALSINKRONISASI, :DokumenId, 1 FROM KONTENAKTIF WHERE KONTENAKTIFID = :DokumenId";
                            Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("DokumenId", dokumenid);
                            object[] parameters = new object[1] { p1 };
                            ctx.Database.ExecuteSqlCommand(sql, parameters);

                            p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("DokumenId", dokumenid);
                            parameters = new object[1] { p1 };
                            versi = ctx.Database.SqlQuery<int>("SELECT nvl(max(versi),0)+1 FROM kontenpasif WHERE kontenaktifid = :DokumenId", parameters).FirstOrDefault();

                            sql = "UPDATE KONTENAKTIF SET VERSI = :pVersi, TANGGALSISIP = SYSDATE, PETUGASSISIP = :pNamaPetugas, TANGGALSUNTING = SYSDATE, PETUGASSUNTING = :pNamaPetugasSunting, JUDUL = :pJudul WHERE KONTENAKTIFID = :DokumenId";
                            p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("pVersi", versi);
                            Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("pNamaPetugas", petugas);
                            Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("pNamaPetugasSunting", petugas);
                            Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("pJudul", judul);
                            Oracle.ManagedDataAccess.Client.OracleParameter p5 = new Oracle.ManagedDataAccess.Client.OracleParameter("DokumenId", dokumenid);
                            parameters = new object[5] { p1, p2, p3, p4, p5 };
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tr.Status = true;
                        tc.Commit();
                    }
                    catch (Exception ex)
                    {
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

    }
}