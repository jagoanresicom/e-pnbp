using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace Pnbp.Entities
{
    public class InternalUserIdentity : System.Security.Principal.IIdentity, System.Security.Principal.IPrincipal
    {
        private readonly System.Web.Security.FormsAuthenticationTicket _ticket;

        public InternalUserIdentity(System.Web.Security.FormsAuthenticationTicket ticket)
        {
            _ticket = ticket;
        }

        public string AuthenticationType
        {
            get { return "User"; }
        }

        public bool IsAuthenticated
        {
            get { return true; }
        }

        public string Name
        {
            get { return _ticket.Name; }
        }

        public string UserId { get; set; }

        public string PegawaiId { get; set; }

        public string KantorId { get; set; }

        public string NamaKantor { get; set; }

        public string NamaPegawai { get; set; }

        public string Email { get; set; }

        public bool IsInRole(string role)
        {
            return Roles.IsUserInRole(role);
        }

        public System.Security.Principal.IIdentity Identity
        {
            get { return this; }
        }

    }
}