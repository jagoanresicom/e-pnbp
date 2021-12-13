using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;

namespace Pnbp.Models
{
    public class InternalUser
    {
        //string tableName = "users";
        string pApplicationName = "Ptsl";

        public List<OfficeMember> GetOffices(string userName)
        {
            var officeList = new List<OfficeMember>();

            using (var ctx = new PnbpContext())
            {
                string sql = 
                    "SELECT DISTINCT " + 
                    "    u.UserId, u.Username, u.Email, u.Password, u.PasswordQuestion, u.Commentar, u.IsApproved, u.IsLockedOut, " + 
                    "    NVL(u.CreationDate,SYSDATE) CreationDate, NVL(u.LastLoginDate,SYSDATE) LastLoginDate, " + 
                    "    NVL(u.LastActivityDate,SYSDATE) LastActivityDate, NVL(u.LastPasswordChangedDate,SYSDATE) LastPasswordChangedDate, " + 
                    "    NVL(u.LastLockedOutDate,SYSDATE) LastLockedOutDate, p.PegawaiId, p.NAMA NAMAPEGAWAI, k.KANTORID, k.NAMA NAMAKANTOR, " + 
                    "    k.TIPEKANTORID " + 
                    "FROM " + 
                    "    USERS u, PEGAWAI p, PROFILEPEGAWAI pp, KANTOR k " + 
                    "WHERE " + 
                    "    pp.VALIDSAMPAI IS NULL " +
                    "    AND p.VALIDSAMPAI IS NULL " +
                    "    AND u.USERNAME = :param1 " + 
                    "    AND u.USERID = p.USERID " + 
                    "    AND p.PEGAWAIID = pp.PEGAWAIID " + 
                    "    AND pp.KANTORID = k.KANTORID";
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", userName);
                object[] parameters = new object[1] { p1 };
                officeList = ctx.OfficeMembershipData.SqlQuery(sql, parameters).ToList<OfficeMember>();
            }

            return officeList;
        }

        public OfficeMember GetOffice(string userName, string kantorId)
        {
            var officeList = new OfficeMember();

            using (var ctx = new PnbpContext())
            {
                string sql = "SELECT DISTINCT u.UserId, u.Username, u.Email, u.Password, u.PasswordQuestion, u.Commentar, u.IsApproved, u.IsLockedOut, u.CreationDate, u.LastLoginDate, u.LastActivityDate, u.LastPasswordChangedDate, u.LastLockedOutDate, p.PegawaiId, p.NAMA NAMAPEGAWAI, k.KANTORID, k.NAMA NAMAKANTOR, k.TIPEKANTORID FROM USERS u, PEGAWAI p, PROFILEPEGAWAI pp, KANTOR k WHERE pp.VALIDSAMPAI IS NULL AND u.USERNAME = :param1 AND k.KANTORID = :param2 AND u.USERID = p.USERID AND p.PEGAWAIID = pp.PEGAWAIID and pp.KANTORID = k.KANTORID";
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", userName);
                Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", kantorId);
                object[] parameters = new object[2] { p1, p2 };
                officeList = ctx.OfficeMembershipData.SqlQuery(sql, parameters).FirstOrDefault<OfficeMember>();
            }

            return officeList;
        }

        public InternalMember GetUser(string userName)
        {
            var customerMemberList = new InternalMember();

            using (var ctx = new PnbpContext())
            {
                string sql = "SELECT u.UserId, u.Username, u.Email, u.Password, u.PasswordQuestion, u.Commentar, u.IsApproved, u.IsLockedOut, u.CreationDate, u.LastLoginDate, u.LastActivityDate, u.LastPasswordChangedDate, u.LastLockedOutDate FROM USERS u WHERE u.USERNAME = :param1";
                //string sql = "SELECT u.UserId, u.Username, u.Email, u.Password, u.PasswordQuestion, u.Commentar, u.IsApproved, u.IsLockedOut, u.CreationDate, u.LastLoginDate, u.LastActivityDate, u.LastPasswordChangedDate, u.LastLockedOutDate, p.PegawaiId, p.NAMA NAMAPEGAWAI, k.KANTORID, k.NAMA NAMAKANTOR FROM USERS u, PPAT p, KANTORMITRAKERJA kmk, KANTOR k WHERE u.USERNAME = :param1 AND u.USERID = mk.USERID AND mk.MITRAKERJAID = p.PPATID and p.PPATID = kmk.MITRAKERJAID AND kmk.KANTORID = k.KANTORID";
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", userName);
                object[] parameters = new object[1] { p1 };
                customerMemberList = ctx.InternalMembershipData.SqlQuery(sql, parameters).FirstOrDefault<InternalMember>();

                //if (!String.IsNullOrEmpty(customerMemberList.Member.UserId))
                //{
                //    sql = "SELECT p.Pegawaiid, p.GelarDepan, p.Nama, p.GelarBelakang FROM PEGAWAI p WHERE p.Userid = :param1";
                //    p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", customerMemberList.Member.UserId);
                //    parameters = new object[1] { p1 };
                //    customerMemberList.Pegawai = ctx.Database.SqlQuery<Pegawai>(sql, parameters).FirstOrDefault<Pegawai>();

                //    if (!String.IsNullOrEmpty(customerMemberList.Pegawai.PegawaiId))
                //    {
                //        sql = "SELECT distinct k.Kantorid, k.Nama FROM PROFILEPEGAWAI p JOIN KANTOR k ON k.Kantorid = p.Kantorid WHERE (p.Validsampai is null or p.Validsampai > sysdate) AND pegawaiid = :param1 ORDER BY k.Nama";
                //        p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", customerMemberList.Pegawai.PegawaiId);
                //        parameters = new object[1] { p1 };
                //        customerMemberList.Kantor = ctx.Database.SqlQuery<Kantor>(sql, parameters).ToList<Kantor>();
                //    }
                //}
            }

            return customerMemberList;
        }

        public InternalMember GetUser(string userName, bool userIsOnline)
        {
            var memberList = new List<InternalMember>();

            using (var ctx = new PnbpContext())
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", userName);
                object[] parameters = new object[1] { p1 };

                //string sql = "SELECT u.UserId, u.Username, u.Email, u.Password, u.PasswordQuestion, u.Commentar, u.IsApproved, u.IsLockedOut, u.CreationDate, u.LastLoginDate, u.LastActivityDate, u.LastPasswordChangedDate, u.LastLockedOutDate, p.NAMA NAMAPPAT, k.KANTORID, k.NAMA NAMAKANTOR FROM USERS u, MITRAKERJA mk, PPAT p, KANTORMITRAKERJA kmk, KANTOR k WHERE u.USERNAME = :param1 AND u.USERID = mk.USERID AND mk.MITRAKERJAID = p.PPATID and p.PPATID = kmk.MITRAKERJAID AND kmk.KANTORID = k.KANTORID";
                string sql = "SELECT u.UserId, u.Username, u.Email, u.Password, u.PasswordQuestion, u.Commentar, u.IsApproved, u.IsLockedOut, u.CreationDate, u.LastLoginDate, u.LastActivityDate, u.LastPasswordChangedDate, u.LastLockedOutDate, s.NAMA NAMASKB, s.SURVEYORID, s.URLPROFILE FOTOPROFIL FROM USERS u, MITRAKERJA mk, SURVEYOR s WHERE u.USERNAME = :param1 AND u.USERID = mk.USERID AND mk.MITRAKERJAID = s.SURVEYORID";
                memberList = ctx.InternalMembershipData.SqlQuery(sql, parameters).ToList<InternalMember>();

                if (memberList.Count > 0)
                {
                    if (userIsOnline)
                    {
                        Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", DateTime.Now);
                        Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pApplicationName);
                        Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("param4", userName);

                        object[] parameters2 = new object[3] { p1, p2, p3 };
                        string sql2 = "UPDATE users SET LastActivityDate = :param2, ApplicationName = :param3 WHERE Username = :param4";

                        int rowsAffected = ctx.Database.ExecuteSqlCommand(sql2, parameters2);
                    }
                }
            }

            return memberList.FirstOrDefault();
        }

        public InternalMember GetUser(object providerUserKey, bool userIsOnline)
        {
            var memberList = new List<InternalMember>();

            using (var ctx = new PnbpContext())
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", providerUserKey);
                object[] parameters = new object[1] { p1 };

                //string sql = "SELECT u.UserId, u.Username, u.Email, u.Password, u.PasswordQuestion, u.Commentar, u.IsApproved, u.IsLockedOut, u.CreationDate, u.LastLoginDate, u.LastActivityDate, u.LastPasswordChangedDate, u.LastLockedOutDate, p.NAMA NAMAPPAT, k.KANTORID, k.NAMA NAMAKANTOR FROM USERS u, MITRAKERJA mk, PPAT p, KANTORMITRAKERJA kmk, KANTOR k WHERE u.USERNAME = :param1 AND u.USERID = mk.USERID AND mk.MITRAKERJAID = p.PPATID and p.PPATID = kmk.MITRAKERJAID AND kmk.KANTORID = k.KANTORID";
                string sql = "SELECT u.UserId, u.Username, u.Email, u.Password, u.PasswordQuestion, u.Commentar, u.IsApproved, u.IsLockedOut, u.CreationDate, u.LastLoginDate, u.LastActivityDate, u.LastPasswordChangedDate, u.LastLockedOutDate, s.NAMA NAMASKB, s.SURVEYORID, s.URLPROFILE FOTOPROFIL FROM USERS u, MITRAKERJA mk, SURVEYOR s WHERE u.USERNAME = :param1 AND u.USERID = mk.USERID AND mk.MITRAKERJAID = s.SURVEYORID";
                memberList = ctx.InternalMembershipData.SqlQuery(sql, parameters).ToList<InternalMember>();

                if (memberList.Count > 0)
                {
                    if (userIsOnline)
                    {
                        Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", DateTime.Now);
                        Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pApplicationName);
                        Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("param4", providerUserKey);

                        object[] parameters2 = new object[3] { p2, p3, p4 };
                        string sql2 = "UPDATE users SET LastActivityDate = :param2, ApplicationName = :param3 WHERE UserId = :param4";

                        int rowsAffected = ctx.Database.ExecuteSqlCommand(sql2, parameters);

                    }
                }
            }

            return memberList.FirstOrDefault();
        }

        public string GetUserNameByEmail(string email)
        {
            string result = "";

            //Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", email);
            //object[] parameters = new object[1] { p1 };

            ////var memberList = new List<Member>();

            //using (var ctx = new PnbpContext())
            //{
            //    string sql = "SELECT Username FROM users WHERE Email = :param1 ORDER BY Username Asc";
            //    DbRawSqlQuery<string> tes = ctx.Database.SqlQuery<string>(sql, parameters);
            //    result = (tes.Count() == 0) ? "" : ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
            //}

            return result;
        }

        public string GetPasswordAnswer(string userName)
        {
            string passwordAnswer = "";

            //using (var ctx = new PnbpContext())
            //{
            //    Oracle.ManagedDataAccess.Client.OracleParameter p0 = new Oracle.ManagedDataAccess.Client.OracleParameter("param0", userName);
            //    object[] parameters = new object[1] { p0 };
            //    string sql = "SELECT PasswordAnswer FROM users WHERE Username = :param0 AND ISLOCKEDOUT = 0";
            //    DbRawSqlQuery<string> tes = ctx.Database.SqlQuery<string>(sql, parameters);

            //    passwordAnswer = (tes.Count() == 0) ? "" : ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
            //}

            return passwordAnswer;
        }

        public int ResetPassword(string userName, string applicationName, string newPassword)
        {
            int rowsAffected = 0;

            using (var ctx = new PnbpContext())
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", newPassword);
                Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", applicationName);
                Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", userName);

                object[] parameters = new object[3] { p1, p2, p3 };
                string sql = "UPDATE users SET Password = :param1, LastPasswordChangedDate = SYSDATE, ApplicationName = :param2 WHERE username = :param3";

                rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters);
            }

            return rowsAffected;
        }

        public int ChangePassword(string username, string newPwd, string tableName)
        {
            Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", newPwd);
            Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", username);

            object[] myParams = new object[2] { p1, p2 };
            int rowsAffected = 0;
            using (var ctx = new PnbpContext())
            {
                string sql = "UPDATE " + tableName + " SET Password = :param1, LastPasswordChangedDate = sysdate WHERE username = :param2";
                rowsAffected = ctx.Database.ExecuteSqlCommand(sql, myParams);
            }

            return rowsAffected;
        }

        public int ChangePasswordQuestionAndAnswer(string username, string newPwdQuestion, string newPwdAnswer, string pApplicationName, string tableName)
        {
            Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", newPwdQuestion);
            Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", newPwdAnswer);
            Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", username);
            Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("param4", pApplicationName);

            object[] myParams = new object[4] { p1, p2, p3, p4 };
            int rowsAffected;
            using (var ctx = new PnbpContext())
            {
                string sql = "UPDATE " + tableName + " SET PasswordQuestion = :param1, PasswordAnswer = :param2, ApplicationName = :param4 WHERE Username = :param3";
                rowsAffected = ctx.Database.ExecuteSqlCommand(sql, myParams);
            }

            return rowsAffected;
        }

        public int UnlockUser(string username)
        {
            int rowsAffected = 0;

            using (var ctx = new PnbpContext())
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", DateTime.Now);
                Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", pApplicationName);
                Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", username);

                object[] parameters = new object[3] { p1, p2, p3 };
                string sql = "UPDATE users SET IsLockedOut = 0, LastLockedOutDate = :param1, ApplicationName = :param2 WHERE Username = :param3";

                rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters);
            }

            return rowsAffected;
        }

        public int UpdateUser(string userName)
        {
            int rowsAffected = 0;

            using (var ctx = new PnbpContext())
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", userName);
                //Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", user.Comment);
                //Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", user.IsApproved);
                //Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", pApplicationName);
                //Oracle.ManagedDataAccess.Client.OracleParameter p5 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", user.UserName);

                object[] parameters = new object[1] { p1 };
                string sql = "UPDATE users SET IsApproved = 1 WHERE Username = :param1";

                rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters);
            }

            return rowsAffected;
        }

        public int CreateUser(object providerUserKey, string username, string password, string email, string passwordQuestion, string passwordAnswer, int isApproved, string comment, DateTime createDate, string pApplicationName, string tableName)
        {
            int recAdded = 0;

            Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", providerUserKey.ToString());
            Oracle.ManagedDataAccess.Client.OracleParameter p2 = new Oracle.ManagedDataAccess.Client.OracleParameter("param2", username);
            Oracle.ManagedDataAccess.Client.OracleParameter p3 = new Oracle.ManagedDataAccess.Client.OracleParameter("param3", password);
            Oracle.ManagedDataAccess.Client.OracleParameter p4 = new Oracle.ManagedDataAccess.Client.OracleParameter("param4", email);
            Oracle.ManagedDataAccess.Client.OracleParameter p5 = new Oracle.ManagedDataAccess.Client.OracleParameter("param5", passwordQuestion);
            Oracle.ManagedDataAccess.Client.OracleParameter p6 = new Oracle.ManagedDataAccess.Client.OracleParameter("param6", passwordAnswer);
            Oracle.ManagedDataAccess.Client.OracleParameter p7 = new Oracle.ManagedDataAccess.Client.OracleParameter("param7", isApproved);
            Oracle.ManagedDataAccess.Client.OracleParameter p8 = new Oracle.ManagedDataAccess.Client.OracleParameter("param8", comment);
            Oracle.ManagedDataAccess.Client.OracleParameter p9 = new Oracle.ManagedDataAccess.Client.OracleParameter("param9", createDate);
            Oracle.ManagedDataAccess.Client.OracleParameter p10 = new Oracle.ManagedDataAccess.Client.OracleParameter("param10", pApplicationName);
            object[] myParams = new object[8] { p1, p2, p3, p4, p5, p6, p7, p8 };

            using (var ctx = new PnbpContext())
            {
                string sql = "INSERT INTO " + tableName + " (USERID, Username, Password, Email, PasswordQuestion, PasswordAnswer, IsApproved, Commentar, CreationDate, LastPasswordChangedDate, LastActivityDate, LastLockedOutDate, FailedPwdAttemptWindowStart, FailedPwdAnswerAttWindowStart, LastLoginDate, FailedPwdAttemptCount, FailedPwdAnswerAttCount) Values (:param1, :param2, :param3, :param4, :param5, :param6, :param7, :param8, sysdate, sysdate, sysdate, sysdate, sysdate, sysdate, sysdate, 0, 0)";
                recAdded = ctx.Database.ExecuteSqlCommand(sql, myParams);
            }

            return recAdded;
        }

        public int DeleteUser(string username, bool deleteAllRelatedData, string tableName)
        {
            int rowsAffected = 0;

            Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", username);
            object[] myParams = new object[1] { p1 };

            using (var ctx = new PnbpContext())
            {
                string sql = "DELETE FROM " + tableName + " WHERE Username = :param1";
                rowsAffected = ctx.Database.ExecuteSqlCommand(sql, myParams);
            }

            if (deleteAllRelatedData)
            {
                // Process commands to delete all data for the user in the database.
            }

            return rowsAffected;
        }

        public Member ValidateUser(string username, string tableName)
        {
            Member m = new Member();
            using (var ctx = new PnbpContext())
            {
                string sql = "SELECT u.UserId, u.Username, u.Email, u.Password, u.PasswordQuestion, u.Commentar, u.IsApproved, u.IsLockedOut, u.CreationDate, u.LastLoginDate, u.LastActivityDate, u.LastPasswordChangedDate, u.LastLockedOutDate FROM USERS u WHERE u.USERNAME = :param1";
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", username);
                object[] parameters = new object[1] { p1 };
                m = ctx.Database.SqlQuery<Member>(sql, parameters).FirstOrDefault<Member>();
            }

            return m;
        }

        public int SetOnline(string userName, string tableName)
        {
            int rowsAffected = 0;

            using (var ctx = new PnbpContext())
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", userName);

                object[] parameters = new object[1] { p1 };
                string sql = "UPDATE " + tableName + " SET LastLoginDate = sysdate, LastActivityDate = sysdate WHERE Username = :param1";

                rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters);
            }

            return rowsAffected;
        }

        public int SetFailedLogin(string userName, string tableName)
        {
            int rowsAffected = 0;

            using (var ctx = new PnbpContext())
            {
                Oracle.ManagedDataAccess.Client.OracleParameter p1 = new Oracle.ManagedDataAccess.Client.OracleParameter("param1", userName);

                object[] parameters = new object[1] { p1 };
                string sql = "UPDATE " + tableName + " SET FAILEDPWDATTEMPTWINDOWSTART = sysdate WHERE Username = :param1";

                rowsAffected = ctx.Database.ExecuteSqlCommand(sql, parameters);
            }

            return rowsAffected;
        }
    }
}