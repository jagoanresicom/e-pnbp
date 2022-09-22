using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Owin.Security.Keycloak;
using System;
using System.Configuration;
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
            string _CookieName = ConfigurationManager.AppSettings["CookieName"].ToString();

            const string persistentAuthType = "keycloak_auth";
            app.SetDefaultSignInAsAuthenticationType(persistentAuthType);

            if (_ServerEnv == "Production")
            {
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = persistentAuthType,
                    CookieName = _CookieName,
                    CookieDomain = ".atrbpn.go.id"
                });
            }
            else
            {
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = persistentAuthType,
                    CookieName = _CookieName
                });
            }

            var desc = new AuthenticationDescription();
            desc.AuthenticationType = "keycloak_auth";
            desc.Caption = "keycloak_auth";
            app.UseKeycloakAuthentication(
                new KeycloakAuthenticationOptions()
                {
                    Description = desc,
                    Realm = _Realm,
                    ClientId = _ClientId,
                    ClientSecret = _ClientSecret,
                    KeycloakUrl = _KeycloakUrl,
                    DisableAudienceValidation = true,
                    AuthenticationType = "keycloak_auth",
                    AllowUnsignedTokens = false,
                    DisableIssuerValidation = false,
                    TokenClockSkew = TimeSpan.FromSeconds(2),
                    DisableAllRefreshTokenValidation = true,
                    DisableRefreshTokenSignatureValidation = true
                });
        }
    }

}
