using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web;

namespace Pnbp.Codes
{
    public class Functions
    {

        public class userIdentity
        {
            public string UserId { get; set; }
            public string PegawaiId { get; set; }
            public string NamaPegawai { get; set; }
            public string KantorId { get; set; }
            public string NamaKantor { get; set; }
            public string TipeKantor { get; set; }
            public int TipeKantorId
            {
                get { return int.Parse(TipeKantor); }
                set { TipeKantor = value.ToString(); }
            }
        }

        public userIdentity claimUser()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var userlogin = new userIdentity();
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                string _CookieName = System.Configuration.ConfigurationManager.AppSettings["CookieName"].ToString();
                var authCookie = HttpContext.Current.Request.Cookies[_CookieName];
                if (authCookie != null)
                {
                    var kc = ClaimsPrincipal.Current.Claims;
                    if (kc != null && kc.Count() > 0)
                    {
                        string tot = "";
                        foreach (var cc in kc)
                        {
                            tot += $", {cc.Type}";
                        }

                        var access_token = kc.Where((claim) => claim.Type == "access_token").FirstOrDefault().Value;

                        var handler = new JwtSecurityTokenHandler();
                        var jwtSecurityToken = handler.ReadJwtToken(access_token);
                        JObject obj2 = JObject.Parse(jwtSecurityToken.Claims.First(c => c.Type == "atrbpn-profile").Value);
                        
                        foreach (var x in obj2)
                        {
                            switch (x.Key)
                            {
                                case "userid": userlogin.UserId = x.Value.ToString(); break;
                                case "pegawaiid": userlogin.PegawaiId = x.Value.ToString(); break;
                                case "namapegawai": userlogin.NamaPegawai = x.Value.ToString(); break;
                                case "kantorid": userlogin.KantorId = x.Value.ToString(); break;
                                case "namakantor": userlogin.NamaKantor = x.Value.ToString(); break;
                                case "tipekantorid": userlogin.TipeKantor = x.Value.ToString(); break;
                            }
                        }

                    }
                    else
                    {
                        userlogin = null;
                    }
                }
                else
                {
                    userlogin = null;
                }
            }
            else
            {
                userlogin = null;
            }

            return userlogin;
        }
    }
}