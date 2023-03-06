using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json.Linq;
using Owin;
using Owin.Security.Keycloak;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(Pnbp.Startup))]

namespace Pnbp
{
    public class Startup
    {
        internal void Configuration()
        {
            throw new NotImplementedException();
        }

        public void Configuration(IAppBuilder app)
        {
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            string _Realm = ConfigurationManager.AppSettings["Realm"].ToString();
            string _ClientId = ConfigurationManager.AppSettings["ClientId"].ToString();
            string _ClientSecret = ConfigurationManager.AppSettings["ClientSecret"].ToString();
            string _KeycloakUrl = ConfigurationManager.AppSettings["KeycloakUrl"].ToString();
            string _ServerEnv = ConfigurationManager.AppSettings["env"].ToString();
            string _CookieDomain = ConfigurationManager.AppSettings["CookieDomain"].ToString();
            string _CookieName = ConfigurationManager.AppSettings["CookieName"].ToString();
            string _redirectUri = ConfigurationManager.AppSettings["RedirectUri"].ToString();

            string _authority = _KeycloakUrl + "/realms/" + _Realm;
            string _metadataAddress = _authority + "/.well-known/openid-configuration";

            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            if (_ServerEnv == "Production")
            {
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    CookieName = _CookieName,
                    CookieDomain = _CookieDomain
                });
            }
            else
            {
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    CookieName = _CookieName
                });
            }

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                // Sets the ClientId, authority, RedirectUri as obtained from web.config                
                ClientId = _ClientId,
                ClientSecret = _ClientSecret,
                Authority = _authority,
                RedirectUri = _redirectUri,
                PostLogoutRedirectUri = _redirectUri,
                Scope = OpenIdConnectScope.OpenIdProfile,
                ResponseType = OpenIdConnectResponseType.Code,
                RedeemCode = true,
                TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name", RoleClaimType = ClaimTypes.Role },
                Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    SecurityTokenValidated = n =>
                    {
                        n.AuthenticationTicket.Identity.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                        n.AuthenticationTicket.Identity.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken));

                        var handler = new JwtSecurityTokenHandler();
                        var jwtSecurityToken = handler.ReadJwtToken(n.ProtocolMessage.AccessToken);

                        JObject obj = JObject.Parse(jwtSecurityToken.Claims.First(c => c.Type == "resource_access").Value);
                        var roleAccess = obj.GetValue("dotnet-web").ToObject<JObject>().GetValue("roles");
                        foreach (JToken role in roleAccess)
                        {
                            n.AuthenticationTicket.Identity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
                        }

                        JObject obj2 = JObject.Parse(jwtSecurityToken.Claims.First(c => c.Type == "atrbpn-profile").Value);

                        foreach (var x in obj2)
                        {
                            n.AuthenticationTicket.Identity.AddClaim(new Claim(x.Key, x.Value.ToString()));
                        }

                        return Task.FromResult(0);
                    },

                    AuthenticationFailed = OnAuthenticationFailed
                },
                // Disable Https for Development
                RequireHttpsMetadata = false,
                MetadataAddress = _metadataAddress,
                ProtocolValidator = new CustomOpenIdConnectProtocolValidator(false),
                SaveTokens = true
            });
        }

        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            context.HandleResponse();
            context.Response.Redirect("/?errormessage=" + context.Exception.Message);
            return Task.FromResult(0);
        }

        public class CustomOpenIdConnectProtocolValidator : OpenIdConnectProtocolValidator
        {
            public CustomOpenIdConnectProtocolValidator(bool shouldValidateNonce)
            {
                this.ShouldValidateNonce = shouldValidateNonce;
                this.RequireStateValidation = false;
            }
            protected override void ValidateNonce(OpenIdConnectProtocolValidationContext validationContext)
            {
                if (this.ShouldValidateNonce)
                {
                    base.ValidateNonce(validationContext);
                }
            }

            private bool ShouldValidateNonce { get; set; }
        }

    }

}
