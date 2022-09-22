using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Pnbp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        //public override void Init()
        //{
        //    this.PostAuthenticateRequest += MvcApplication_PostAuthenticateRequest;
        //    base.Init();
        //}

        void MvcApplication_PostAuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                //FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                //Entities.InternalUserIdentity newUser = new Entities.InternalUserIdentity(authTicket);

                //Entities.InternalUserData ud = Newtonsoft.Json.JsonConvert.DeserializeObject<Entities.InternalUserData>(authTicket.UserData);
                //newUser.PegawaiId = ud.pegawaiid;
                //newUser.KantorId = ud.kantorid;
                //newUser.NamaKantor = ud.namakantor;
                //newUser.UserId = ud.userid;
                //newUser.NamaPegawai = ud.namapegawai;
                //HttpContext.Current.User = newUser;
            }
        }
    }
}