using Newtonsoft.Json.Linq;
using Pnbp.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pnbp.Models
{
    public class HakAksesModel
    {
        public List<Entities.ProfilPengguna> ListProfilPengguna(string pKantorId, string pIdPegawai)
        {
            List<Entities.ProfilPengguna> _lstProfil = new List<Entities.ProfilPengguna>();

            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                string sql =
                    @"SELECT p.profileid,INITCAP(p.nama) nama,DECODE(profilepegawaiid, null, 'False', 'True') aktif, NVL(p.tipeeselonid,0) eselon 
                        FROM profile p
  		                     LEFT JOIN profilepegawai pp
  		 	                     ON pp.profileid = p.profileid AND pp.kantorid = :idKantor AND pp.pegawaiid = :idPegawai AND (pp.validsampai IS NULL OR TO_DATE(TRIM(pp.validsampai)) > TO_DATE(TRIM(SYSDATE)))
                       WHERE p.profileid IN ({0}) 
                         AND (p.validsampai IS NULL OR TO_DATE(TRIM(p.validsampai)) > TO_DATE(TRIM(SYSDATE))) 
                       ORDER BY p.nama";

                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("idKantor", pKantorId);
                Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("idPegawai", pIdPegawai);
                var parameters = new object[2] { p1, p2 };
                sql = sWhitespace.Replace(String.Format(sql, ConfigurationManager.AppSettings["ListProfile"].ToString()), " ");
                _lstProfil = ctx.Database.SqlQuery<Entities.ProfilPengguna>(sql, parameters).ToList<Entities.ProfilPengguna>();
            }

            return _lstProfil;
        }

        public List<Entities.DataPengguna> ListPengguna(string nama, string idkantor)
        {
            List<Entities.DataPengguna> _lstPengguna = new List<Entities.DataPengguna>();

            using (var ctx = new PnbpContext())
            {
                Regex sWhitespace = new Regex(@"\s+");
                string sql =
                    @"SELECT distinct(users.userid) idpengguna, pegawai.pegawaiid as idpegawai, users.username namapengguna, TRIM(pegawai.gelardepan||' '||pegawai.nama||' '||pegawai.gelarbelakang) namalengkap, pegawai.displaynip
                        FROM users JOIN pegawai JOIN profilepegawai ON profilepegawai.pegawaiid = pegawai.pegawaiid AND profilepegawai.kantorid = :idKantor AND (profilepegawai.validsampai IS NULL OR TO_DATE(TRIM(profilepegawai.validsampai)) > TO_DATE(TRIM(SYSDATE)))
			            ON pegawai.userid = users.userid AND (pegawai.validsampai IS NULL OR TO_DATE(TRIM(pegawai.validsampai)) > TO_DATE(TRIM(SYSDATE)))
                        WHERE UPPER(pegawai.nama) like :upNama
                          AND (users.validsampai IS NULL OR TO_DATE(TRIM(users.validsampai)) > TO_DATE(TRIM(SYSDATE)))";
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("idKantor", idkantor);
                Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("upNama", String.Concat("%", nama.ToUpper(), "%"));
                var parameters = new object[2] { p1, p2 };
                sql = sWhitespace.Replace(sql, " ");
                _lstPengguna = ctx.Database.SqlQuery<Entities.DataPengguna>(sql, parameters).ToList<Entities.DataPengguna>();
            }

            return _lstPengguna;
        }

        public Entities.TransactionResult MergeHakAkses(string jsonData, string pIdPegawai, string pIdUser, string pKantorId)
        {
            Entities.TransactionResult tr = new Entities.TransactionResult() { Status = true, Pesan = "Hak Akses berhasil diproses" };

            JObject json = JObject.Parse(jsonData);
            List<JObject> on = new List<JObject>();
            List<JObject> off = new List<JObject>();
            foreach (var item in json)
            {
                string val = json[item.Key].ToString();
                JObject obj = JObject.Parse(val);
                dynamic jobj = new JObject();
                jobj.tipe = item.Key;
                jobj.pejabat = obj["pejabat"];
                jobj.ket = obj["ket"];

                if (obj["aktif"].ToString() == "on")
                {
                    on.Add(jobj);
                }
                else
                {
                    off.Add(jobj);
                }
            }

            using (var ctx = new PnbpContext())
            {
                using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        Regex sWhitespace = new Regex(@"\s+");
                        string sql = string.Empty;
                        var parameters = new object[1] { sWhitespace };

                        foreach (dynamic profile in on)
                        {
                            string tipe = profile.tipe;
                            bool pejabat = profile.pejabat;
                            if (pejabat)
                            {
                                // Cek jika ada pejabat aktif
                                sql = @"SELECT profilepegawaiid FROM profilepegawai WHERE profileid = :idProfile AND kantorid = :idKantor AND (validsampai IS NULL OR validsampai > SYSDATE)";
                                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("idProfile",tipe);
                                Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("idKantor",pKantorId);
                                parameters = new object[2] { p1, p2 };
                                string pejabataktif = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault<string>();
                                if (!String.IsNullOrEmpty(pejabataktif))
                                {
                                    throw new Exception(String.Concat("Jabatan ", profile.ket, " masih dijabat."));
                                }
                            }

                            sql = @"INSERT INTO profilepegawai (PROFILEPEGAWAIID,PROFILEID,PEGAWAIID,KANTORID,VALIDSEJAK) VALUES (SYS_GUID(),:idProfile,:idPegawai,:idKantor,SYSDATE)";
                            Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("idProfile", tipe);
                            Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("idPegawai", pIdPegawai);
                            Oracle.ManagedDataAccess.Client.OracleParameter p5 = new Oracle.ManagedDataAccess.Client.OracleParameter("idKantor", pKantorId);
                            parameters = new object[3] { p3, p4, p5 };
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        foreach (dynamic profile in off)
                        {
                            string tipe = profile.tipe;
                            sql = @"UPDATE profilepegawai SET validsampai = SYSDATE, USERUPDATE = :idUser, LASTUPDATE = SYSDATE 
                                     WHERE profileid = :idProfile 
                                       AND pegawaiid = :idPegawai 
                                       AND kantorid = :idKantor
                                       AND (validsampai IS NULL OR TO_DATE(TRIM(validsampai)) > TO_DATE(TRIM(SYSDATE)))";
                            Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("idUser", pIdUser);
                            Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("idProfile", tipe);
                            Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("idPegawai", pIdPegawai);
                            Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("idKantor", pKantorId);
                            parameters = new object[4] { p1, p2, p3, p4 };
                            sql = sWhitespace.Replace(sql, " ");
                            ctx.Database.ExecuteSqlCommand(sql, parameters);
                        }

                        tc.Commit();
                    }
                    catch (Exception ex)
                    {
                        tc.Rollback();
                        tr.Status = false;
                        tr.Pesan = ex.Message.ToString();
                    }
                    finally
                    {
                        tc.Dispose();
                        ctx.Dispose();
                    }
                }
            }

            return tr;
        }

        public string[] GetRolesForUser(string userid, string kantorid)
        {
            var userRoles = new List<string>();

            try
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", userid);
                Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", kantorid);
                object[] myParams = new object[2] { p1, p2 };

                string sql = "SELECT p1.ROLENAME FROM PROFILE p1, PROFILEPEGAWAI pp, PEGAWAI p2 WHERE p2.USERID = :param1 AND p2.PEGAWAIID = pp.PEGAWAIID AND pp.KANTORID = :param2 AND pp.PROFILEID = p1.PROFILEID AND (pp.VALIDSAMPAI IS NULL OR pp.VALIDSAMPAI > SYSDATE) AND p1.ROLENAME IS NOT NULL";
                using (var ctx = new PnbpContext())
                {
                    userRoles = ctx.Database.SqlQuery<string>(sql, myParams).ToList<string>();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return userRoles.ToArray();
        }

        public int GetTipeKantor(string kantorid)
        {
            int result = 0;

            string query = "SELECT tipekantorid FROM kantor WHERE kantorid = :KantorId";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                result = ctx.Database.SqlQuery<int>(query, parameters).First();
            }

            return result;
        }

        public bool CheckUserProfile(string pegawaiid, string kantorid, string profileid)
        {
            bool result = false;

            string query = "SELECT count(*) FROM profilepegawai WHERE pegawaiid = :PegawaiId AND kantorid = :KantorId AND profileid IN (" + profileid + ")";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PegawaiId", pegawaiid));
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                if (jumlahrecord > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public bool CheckValidUserProfileRoles(string pegawaiid, string kantorid, string roles, CommonSearchOptions searchOptions = null)
        {
            bool result = false;
            if (searchOptions == null)
            {
                searchOptions = new CommonSearchOptions()
                {
                    CekKantor = true,
                };
            }

            string query = "SELECT count(*) FROM profilepegawai pp JOIN PROFILE p ON p.profileid = pp.profileid WHERE pegawaiid = :PegawaiId AND rolename IN (" + roles + ") AND (pp.VALIDSAMPAI IS NULL OR TRUNC(pp.VALIDSAMPAI) > TRUNC(SYSDATE))";

            ArrayList arrayListParameters = new ArrayList();
            arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("PegawaiId", pegawaiid));

            if (searchOptions.CekKantor)
            {
                query += " AND kantorid = :KantorId ";
                arrayListParameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("KantorId", kantorid));
            }

            using (var ctx = new PnbpContext())
            {
                object[] parameters = arrayListParameters.OfType<object>().ToArray();
                int jumlahrecord = ctx.Database.SqlQuery<int>(query, parameters).First();
                if (jumlahrecord > 0)
                {
                    result = true;
                }
            }

            return result;
        }

    }
}