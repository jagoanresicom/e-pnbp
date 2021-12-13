using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace Pnbp
{
    public class PnbpContext : DbContext
    {
        static PnbpContext()
        {
            // don't let EF modify the database schema...
            Database.SetInitializer<PnbpContext>(null);
        }

        public PnbpContext()
            : base("KkpWebConnString")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("KKPWEBDEV");
            //modelBuilder.HasDefaultSchema("KKPWEB");
        }

        public DbSet<User> UserData { get; set; }
        public DbSet<Member> MembershipData { get; set; }
        public DbSet<InternalMember> InternalMembershipData { get; set; }
        public DbSet<OfficeMember> OfficeMembershipData { get; set; }
        public DbSet<Application> ApplicationData { get; set; }
        public DbSet<UserRoles> UserRoleData { get; set; }
        public DbSet<Profile> ProfileData { get; set; }
        public DbSet<Password> Passwords { get; set; }
        public DbSet<Entities.JumlahPenerimaan> JumlahPenerimaans { get; set; }
        public DbSet<Entities.StatistikNTPN> StatistikNTPNs { get; set; }
        public DbSet<Entities.StatistikNTPNDetail> StatistikNTPNDetails { get; set; }
        public DbSet<Entities.DetailNTPN> DetailNTPNs { get; set; }
        public DbSet<Entities.Wilayah> DbWilayah { get; set; }
        public DbSet<Entities.RekapPenerimaanDetail> RekapPenerimaanDetail { get; set; }
    }
       
    public class User
    {
        [Key]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime LastActivityDate { get; set; }
    }

    public class OfficeMember
    {
        [Key]
        public string KantorId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordQuestion { get; set; }
        public string Commentar { get; set; }
        public int IsApproved { get; set; }
        public int IsLockedOut { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastActivityDate { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public DateTime LastLockedoutDate { get; set; }
        public string PegawaiId { get; set; }
        public string NamaPegawai { get; set; }
        public string NamaKantor { get; set; }
        public int TipeKantorId { get; set; }
    }

    public class SelectOffice
    {
        public List<OfficeMember> OfficeList { get; set; }
        public string UserName { get; set; }
        public string SelectedOffice { get; set; }
        public string ReturnUrl { get; set; }
        public bool Persistent { get; set; }
    }

    public class InternalMember
    {
        [Key]
        public string UserId { get; set; }
        public string KantorId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordQuestion { get; set; }
        public string Commentar { get; set; }
        public int IsApproved { get; set; }
        public int IsLockedOut { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastActivityDate { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public DateTime LastLockedoutDate { get; set; }
        public string PegawaiId { get; set; }
        public string NamaPegawai { get; set; }
        public string NamaKantor { get; set; }
    }

    public class Member
    {   
        [Key]
        public string UserId { get; set; }
        public string Username { get; set;}
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordQuestion { get; set; }
        public string Commentar { get; set; }
        public int IsApproved { get; set; }
        public int IsLockedOut { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastActivityDate { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public DateTime LastLockedoutDate { get; set; }
    }

    public class Application
    {
        public string ApplicationName { get; set; }
        public string LoweredApplicationName { get; set; }
        [Key]
        public Guid ApplicationId { get; set; }
        public string Description { get; set; }
    }

    public class UserRoles
    {
        [Key]
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }

    public class Profile
    {
        [Key]
        public Guid UserId { get; set; }
        public string PropertyNames { get; set; }
        public string PropertyValuesString { get; set; }
        public byte[] PropertyValuesBinary { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }

    public class Password
    {
        public Password()
        {
        }
        [Key]
        public string UserId { get; set; }
        public string pWord { get; set; }
    }
}