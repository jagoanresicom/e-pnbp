using Microsoft.Owin.Security.Cookies;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Pnbp.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login(Models.LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //if(WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe.Value)) {
                //    return Redirect(returnUrl ?? "/");
                //}
                //ModelState.AddModelError("", "The user name or password provided is incorrect.");
                OracleMembershipProvider provider = (OracleMembershipProvider)System.Web.Security.Membership.Providers["OracleMembershipProvider"];

                if (provider.ValidateUser(model.UserName, model.Password)) //(passwordList.Count > 0 && (passwordList[0].pWord == passWord))
                {
                    //Session["UserName"] = model.UserName;
                    //HttpCookie cookie = System.Web.Security.FormsAuthentication.GetAuthCookie(model.UserName, true);
                    //var ticket = System.Web.Security.FormsAuthentication.Decrypt(cookie.Value);

                    //var newTicket = new System.Web.Security.FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate, ticket.Expiration, true, new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(model.UserName), ticket.CookiePath);
                    //cookie.Value = System.Web.Security.FormsAuthentication.Encrypt(newTicket);
                    //cookie.Expires = newTicket.Expiration.AddHours(24);
                    //Response.Cookies.Set(cookie);

                    //var ID = new UserIdentity(newTicket);
                    //var userRoles = System.Web.Security.Roles.GetRolesForUser(model.UserName);
                    //var prin = new System.Security.Principal.GenericPrincipal(ID, userRoles);
                    //System.Web.HttpContext.Current.User = prin;

                    //return Redirect(returnUrl ?? "/");

                    return SetupFormsAuthTicket(model.UserName, model.RememberMe, returnUrl);
                }
            }

            // If we got this far, something failed, redisplay form
            return View("Index");
        }

        private ActionResult SetupFormsAuthTicket(string userName, bool persistanceFlag, string returnUrl)
        {
            List<OfficeMember> user;

            Models.InternalUser cu = new Models.InternalUser();
            user = cu.GetOffices(userName);

            if (user.Count == 1)
            {
                Models.HakAksesModel ham = new Models.HakAksesModel();
                var userRoles = ham.GetRolesForUser(user[0].UserId, user[0].KantorId);
                var userData = "{\"userid\":\"" + user[0].UserId + "\",\"pegawaiid\":\"" + user[0].PegawaiId + "\",\"kantorid\":\"" + user[0].KantorId + "\",\"namakantor\":\"" + user[0].NamaKantor + "\",\"tipekantorid\":" + user[0].TipeKantorId + ",\"namapegawai\":\"" + user[0].NamaPegawai + "\", \"userroles\":" + JsonConvert.SerializeObject(userRoles) + "}";


                #region Validasi Otorisasi User

                //string jeniskantor = OtorisasiUser.GetJenisKantor(user[0].KantorId);
                //if (jeniskantor == "Kanwil")
                //{
                //    var filterprofile = userRoles.FirstOrDefault(c => c.Equals("Kakanwil") || c.Equals("KabagTU") || c.Equals("KepalaUrusanKeuangan"));
                //    if (string.IsNullOrEmpty(filterprofile))
                //    {
                //        System.Web.Security.FormsAuthentication.SignOut();
                //        Session.Abandon();
                //        return RedirectToAction("Index", "Home");
                //    }
                //}
                //else if (jeniskantor == "Kantah")
                //{
                //    var filterprofile = userRoles.FirstOrDefault(c => c.Equals("KepalaKantorPertanahan") || c.Equals("KepalaSubBagianTataUsaha") || c.Equals("KaurRenEvLap"));
                //    if (string.IsNullOrEmpty(filterprofile))
                //    {
                //        System.Web.Security.FormsAuthentication.SignOut();
                //        Session.Abandon();
                //        return RedirectToAction("Index", "Home");
                //    }
                //}

                #endregion

                HttpCookie cookie = System.Web.Security.FormsAuthentication.GetAuthCookie(userName, true);
                var ticket = System.Web.Security.FormsAuthentication.Decrypt(cookie.Value);
                var newTicket = new FormsAuthenticationTicket(ticket.Version,
                    ticket.Name,
                    ticket.IssueDate,
                    ticket.Expiration,
                    persistanceFlag,
                    userData,
                    ticket.CookiePath);
                cookie.Value = System.Web.Security.FormsAuthentication.Encrypt(newTicket);
                cookie.Expires = newTicket.Expiration.AddHours(24);
                Response.Cookies.Set(cookie);

                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("BerandaPenerimaan", "Penerimaan");
            }
            else if (user.Count > 1)
            {
                // tes a = new tes();
                // a.user = user;
                // a.selected = 
                SelectOffice a = new SelectOffice();
                a.OfficeList = user;
                a.SelectedOffice = user[0].KantorId;
                a.ReturnUrl = returnUrl;
                a.Persistent = persistanceFlag;
                a.UserName = userName;
                return View("ListKantor", a);
                // Keluarkan popup pilihan kantor berdasarkan user
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }

            //var authTicket = new FormsAuthenticationTicket(1, //version
            //                                            userName, // user name
            //                                            DateTime.Now,             //creation
            //                                            DateTime.Now.AddMinutes(30), //Expiration
            //                                            persistanceFlag, //Persistent
            //                                            userData);

            //var encTicket = FormsAuthentication.Encrypt(authTicket);
            //Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        //public ActionResult LogOff()
        //{
        //    //WebSecurity.Logout();
        //    System.Web.Security.FormsAuthentication.SignOut();
        //    Session.Abandon();
        //    return RedirectToAction("Index", "Home");
        //}

        public ActionResult LogOff()
        {
            HttpContext.User = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity(string.Empty), null);

            Request.GetOwinContext().Authentication.SignOut(HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes().Select(o => o.AuthenticationType).ToArray());
            HttpContext.Response.Headers.Remove("Set-Cookie");
            Request.GetOwinContext().Authentication.SignOut();
            this.HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);

            // clear authentication cookie
            string _CookieName = ConfigurationManager.AppSettings["CookieName"].ToString();
            HttpCookie cookie1 = new HttpCookie(_CookieName);
            cookie1.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie1);

            return RedirectToAction("Index", "Home");

        }

        public ActionResult SetKantor(SelectOffice m)
        {
            string username = m.UserName;
            string kantorid = m.SelectedOffice;

            Models.InternalUser cu = new Models.InternalUser();
            OfficeMember user = cu.GetOffice(username, kantorid);

            Models.HakAksesModel ham = new Models.HakAksesModel();
            var userRoles = ham.GetRolesForUser(user.UserId, user.KantorId);
            //userRoles = new string[] { "Administrator" };


            #region Validasi Otorisasi User

            //string jeniskantor = OtorisasiUser.GetJenisKantor(kantorid);
            //if (jeniskantor == "Kanwil")
            //{
            //    var filterprofile = userRoles.FirstOrDefault(c => c.Equals("Kakanwil") || c.Equals("KabagTU") || c.Equals("KepalaUrusanKeuangan"));
            //    if (string.IsNullOrEmpty(filterprofile))
            //    {
            //        System.Web.Security.FormsAuthentication.SignOut();
            //        Session.Abandon();
            //        return RedirectToAction("Index", "Home");
            //    }
            //}
            //else if (jeniskantor == "Kantah")
            //{
            //    var filterprofile = userRoles.FirstOrDefault(c => c.Equals("KepalaKantorPertanahan") || c.Equals("KepalaSubBagianTataUsaha") || c.Equals("KepalaSubBagianTataUsaha"));
            //    if (string.IsNullOrEmpty(filterprofile))
            //    {
            //        System.Web.Security.FormsAuthentication.SignOut();
            //        Session.Abandon();
            //        return RedirectToAction("Index", "Home");
            //    }
            //}

            #endregion


            //var userData = "{\"userid\":\"" + user.UserId + "\",\"ppatid\":\"" + user.PegawaiId + "\",\"kantorid\":\"" + user.KantorId + "\",\"namakantor\":\"" + user.NamaKantor + "\",\"namappat\":\"" + user.NamaPegawai + "\"}";
            //var userData = "{\"userid\":\"" + user.UserId + "\",\"pegawaiid\":\"" + user.PegawaiId + "\",\"kantorid\":\"" + user.KantorId + "\",\"namakantor\":\"" + user.NamaKantor + "\",\"namapegawai\":\"" + user.NamaPegawai + "\"}";
            var userData = "{\"userid\":\"" + user.UserId + "\",\"pegawaiid\":\"" + user.PegawaiId + "\",\"kantorid\":\"" + user.KantorId + "\",\"namakantor\":\"" + user.NamaKantor + "\",\"tipekantorid\":" + user.TipeKantorId + ",\"namapegawai\":\"" + user.NamaPegawai + "\", \"userroles\":" + JsonConvert.SerializeObject(userRoles.Where(x => x.ToLower() == "administrator").ToList()) + "}";

            HttpCookie cookie = System.Web.Security.FormsAuthentication.GetAuthCookie(username, true);
            var ticket = System.Web.Security.FormsAuthentication.Decrypt(cookie.Value);
            var newTicket = new FormsAuthenticationTicket(ticket.Version,
                ticket.Name,
                ticket.IssueDate,
                ticket.Expiration,
                m.Persistent,
                userData,
                ticket.CookiePath);
            cookie.Value = System.Web.Security.FormsAuthentication.Encrypt(newTicket);
            cookie.Expires = newTicket.Expiration.AddHours(24);
            Response.Cookies.Set(cookie);

            if (Url.IsLocalUrl(m.ReturnUrl) && m.ReturnUrl.Length > 1 && m.ReturnUrl.StartsWith("/")
                    && !m.ReturnUrl.StartsWith("//") && !m.ReturnUrl.StartsWith("/\\"))
            {
                return Redirect(m.ReturnUrl);
            }

            return RedirectToAction("BerandaPenerimaan", "Penerimaan");
        }
    }
}

//OLD SYNTX
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Security;

//namespace Pnbp.Controllers
//{
//    public class AccountController : Controller
//    {
//        //
//        // GET: /Account/

//        public ActionResult Index()
//        {
//            return View();
//        }

//        public ActionResult Login(Models.LoginModel model, string returnUrl)
//        {
//            if (ModelState.IsValid)
//            {
//                //if(WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe.Value)) {
//                //    return Redirect(returnUrl ?? "/");
//                //}
//                //ModelState.AddModelError("", "The user name or password provided is incorrect.");
//                OracleMembershipProvider provider = (OracleMembershipProvider)System.Web.Security.Membership.Providers["OracleMembershipProvider"];

//                if (provider.ValidateUser(model.UserName, model.Password)) //(passwordList.Count > 0 && (passwordList[0].pWord == passWord))
//                {
//                    //Session["UserName"] = model.UserName;
//                    //HttpCookie cookie = System.Web.Security.FormsAuthentication.GetAuthCookie(model.UserName, true);
//                    //var ticket = System.Web.Security.FormsAuthentication.Decrypt(cookie.Value);

//                    //var newTicket = new System.Web.Security.FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate, ticket.Expiration, true, new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(model.UserName), ticket.CookiePath);
//                    //cookie.Value = System.Web.Security.FormsAuthentication.Encrypt(newTicket);
//                    //cookie.Expires = newTicket.Expiration.AddHours(24);
//                    //Response.Cookies.Set(cookie);

//                    //var ID = new UserIdentity(newTicket);
//                    //var userRoles = System.Web.Security.Roles.GetRolesForUser(model.UserName);
//                    //var prin = new System.Security.Principal.GenericPrincipal(ID, userRoles);
//                    //System.Web.HttpContext.Current.User = prin;

//                    //return Redirect(returnUrl ?? "/");

//                    return SetupFormsAuthTicket(model.UserName, model.RememberMe, returnUrl);
//                }
//            }

//            // If we got this far, something failed, redisplay form
//            return View("Index");
//        }

//        private ActionResult SetupFormsAuthTicket(string userName, bool persistanceFlag, string returnUrl)
//        {
//            List<OfficeMember> user;

//            Models.InternalUser cu = new Models.InternalUser();
//            user = cu.GetOffices(userName);

//            if (user.Count == 1)
//            {
//                Models.HakAksesModel ham = new Models.HakAksesModel();
//                var userRoles = ham.GetRolesForUser(user[0].UserId, user[0].KantorId);
//                var userData = "{\"userid\":\"" + user[0].UserId + "\",\"pegawaiid\":\"" + user[0].PegawaiId + "\",\"kantorid\":\"" + user[0].KantorId + "\",\"namakantor\":\"" + user[0].NamaKantor + "\",\"tipekantorid\":" + user[0].TipeKantorId + ",\"namapegawai\":\"" + user[0].NamaPegawai + "\", \"userroles\":" + JsonConvert.SerializeObject(userRoles) + "}";


//                #region Validasi Otorisasi User

//                //string jeniskantor = OtorisasiUser.GetJenisKantor(user[0].KantorId);
//                //if (jeniskantor == "Kanwil")
//                //{
//                //    var filterprofile = userRoles.FirstOrDefault(c => c.Equals("Kakanwil") || c.Equals("KabagTU") || c.Equals("KepalaUrusanKeuangan"));
//                //    if (string.IsNullOrEmpty(filterprofile))
//                //    {
//                //        System.Web.Security.FormsAuthentication.SignOut();
//                //        Session.Abandon();
//                //        return RedirectToAction("Index", "Home");
//                //    }
//                //}
//                //else if (jeniskantor == "Kantah")
//                //{
//                //    var filterprofile = userRoles.FirstOrDefault(c => c.Equals("KepalaKantorPertanahan") || c.Equals("KepalaSubBagianTataUsaha") || c.Equals("KaurRenEvLap"));
//                //    if (string.IsNullOrEmpty(filterprofile))
//                //    {
//                //        System.Web.Security.FormsAuthentication.SignOut();
//                //        Session.Abandon();
//                //        return RedirectToAction("Index", "Home");
//                //    }
//                //}

//                #endregion

//                HttpCookie cookie = System.Web.Security.FormsAuthentication.GetAuthCookie(userName, true);
//                var ticket = System.Web.Security.FormsAuthentication.Decrypt(cookie.Value);
//                var newTicket = new FormsAuthenticationTicket(ticket.Version,
//                    ticket.Name,
//                    ticket.IssueDate,
//                    ticket.Expiration,
//                    persistanceFlag,
//                    userData,
//                    ticket.CookiePath);
//                cookie.Value = System.Web.Security.FormsAuthentication.Encrypt(newTicket);
//                cookie.Expires = newTicket.Expiration.AddHours(24);
//                Response.Cookies.Set(cookie);

//                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
//                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
//                {
//                    return Redirect(returnUrl);
//                }

//                return RedirectToAction("Index", "Home");
//            }
//            else if (user.Count > 1)
//            {
//                // tes a = new tes();
//                // a.user = user;
//                // a.selected = 
//                SelectOffice a = new SelectOffice();
//                a.OfficeList = user;
//                a.SelectedOffice = user[0].KantorId;
//                a.ReturnUrl = returnUrl;
//                a.Persistent = persistanceFlag;
//                a.UserName = userName;
//                return View("ListKantor", a);
//                // Keluarkan popup pilihan kantor berdasarkan user
//            }
//            else
//            {
//                return RedirectToAction("Index", "Account");
//            }

//            //var authTicket = new FormsAuthenticationTicket(1, //version
//            //                                            userName, // user name
//            //                                            DateTime.Now,             //creation
//            //                                            DateTime.Now.AddMinutes(30), //Expiration
//            //                                            persistanceFlag, //Persistent
//            //                                            userData);

//            //var encTicket = FormsAuthentication.Encrypt(authTicket);
//            //Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

//        }

//        public ActionResult ForgotPassword()
//        {
//            return View();
//        }

//        public ActionResult Register()
//        {
//            return View();
//        }

//        public ActionResult LogOff()
//        {
//            //WebSecurity.Logout();
//            System.Web.Security.FormsAuthentication.SignOut();
//            Session.Abandon();
//            return RedirectToAction("Index", "Home");
//        }

//        public ActionResult SetKantor(SelectOffice m)
//        {
//            string username = m.UserName;
//            string kantorid = m.SelectedOffice;

//            Models.InternalUser cu = new Models.InternalUser();
//            OfficeMember user = cu.GetOffice(username, kantorid);

//            Models.HakAksesModel ham = new Models.HakAksesModel();
//            var userRoles = ham.GetRolesForUser(user.UserId, user.KantorId);


//            #region Validasi Otorisasi User

//            //string jeniskantor = OtorisasiUser.GetJenisKantor(kantorid);
//            //if (jeniskantor == "Kanwil")
//            //{
//            //    var filterprofile = userRoles.FirstOrDefault(c => c.Equals("Kakanwil") || c.Equals("KabagTU") || c.Equals("KepalaUrusanKeuangan"));
//            //    if (string.IsNullOrEmpty(filterprofile))
//            //    {
//            //        System.Web.Security.FormsAuthentication.SignOut();
//            //        Session.Abandon();
//            //        return RedirectToAction("Index", "Home");
//            //    }
//            //}
//            //else if (jeniskantor == "Kantah")
//            //{
//            //    var filterprofile = userRoles.FirstOrDefault(c => c.Equals("KepalaKantorPertanahan") || c.Equals("KepalaSubBagianTataUsaha") || c.Equals("KepalaSubBagianTataUsaha"));
//            //    if (string.IsNullOrEmpty(filterprofile))
//            //    {
//            //        System.Web.Security.FormsAuthentication.SignOut();
//            //        Session.Abandon();
//            //        return RedirectToAction("Index", "Home");
//            //    }
//            //}

//            #endregion


//            //var userData = "{\"userid\":\"" + user.UserId + "\",\"ppatid\":\"" + user.PegawaiId + "\",\"kantorid\":\"" + user.KantorId + "\",\"namakantor\":\"" + user.NamaKantor + "\",\"namappat\":\"" + user.NamaPegawai + "\"}";
//            //var userData = "{\"userid\":\"" + user.UserId + "\",\"pegawaiid\":\"" + user.PegawaiId + "\",\"kantorid\":\"" + user.KantorId + "\",\"namakantor\":\"" + user.NamaKantor + "\",\"namapegawai\":\"" + user.NamaPegawai + "\"}";
//            var userData = "{\"userid\":\"" + user.UserId + "\",\"pegawaiid\":\"" + user.PegawaiId + "\",\"kantorid\":\"" + user.KantorId + "\",\"namakantor\":\"" + user.NamaKantor + "\",\"tipekantorid\":" + user.TipeKantorId + ",\"namapegawai\":\"" + user.NamaPegawai + "\", \"userroles\":" + JsonConvert.SerializeObject(userRoles) + "}";

//            HttpCookie cookie = System.Web.Security.FormsAuthentication.GetAuthCookie(username, true);
//            var ticket = System.Web.Security.FormsAuthentication.Decrypt(cookie.Value);
//            var newTicket = new FormsAuthenticationTicket(ticket.Version,
//                ticket.Name,
//                ticket.IssueDate,
//                ticket.Expiration,
//                m.Persistent,
//                userData,
//                ticket.CookiePath);
//            cookie.Value = System.Web.Security.FormsAuthentication.Encrypt(newTicket);
//            cookie.Expires = newTicket.Expiration.AddHours(24);
//            Response.Cookies.Set(cookie);

//            if (Url.IsLocalUrl(m.ReturnUrl) && m.ReturnUrl.Length > 1 && m.ReturnUrl.StartsWith("/")
//                    && !m.ReturnUrl.StartsWith("//") && !m.ReturnUrl.StartsWith("/\\"))
//            {
//                return Redirect(m.ReturnUrl);
//            }

//            return RedirectToAction("Index", "Home");
//        }
//    }
//}
