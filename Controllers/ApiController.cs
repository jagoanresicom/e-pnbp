using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pnbp.Controllers
{
    public class ApiController : Controller
    {
        // GET: Api
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult alokasimp(string kode)
        {
            var ctx = new PnbpContext();
            int status = 0;
            int code = 204;
            var message = "token expire";
            List<Entities.Api> datamanfaat = new List<Entities.Api>();
            var cektoken = ctx.Database.SqlQuery<Entities.apitoken>("SELECT * FROM APITOKEN WHERE APITOKENKODE =  '" + kode + "' AND APITOKENVALIDUNTIL > SYSDATE").FirstOrDefault();
            if (cektoken != null)
            {
                datamanfaat = GetData();
                status = 1;
                code = 200;
                message = "";
            }
            return new JsonResult()
            {
                Data = new {
                    status = status,
                    code = code,
                    message = message,
                    result = datamanfaat
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = Int32.MaxValue
            };
            //return Json(new { status = status, code = code, result = datamanfaat }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAccessToken()
        {
            var ctx = new PnbpContext();

            var kode = ctx.Database.SqlQuery<Entities.apiconfig>("SELECT CONFIGTOKEN FROM APICONFIG WHERE CONFIGSTATUS = 1").FirstOrDefault();
            var hourexpire = ctx.Database.SqlQuery<Entities.apiconfig>("SELECT CONFIGHOUREXPIRED FROM APICONFIG WHERE CONFIGSTATUS = 1").FirstOrDefault(); ;

            int status = 0;
            int code = 204;
            var message = "kode tidak ditemukan";
            var responekode = "";
            var get_token = ctx.Database.SqlQuery<int>("SELECT Count(*) FROM APIACCESS WHERE APIACCESSKODE = '" + kode.CONFIGTOKEN + "' AND APIACCESSSTATUS = 1").FirstOrDefault();
            if (get_token > 0)
            {
                var apitokenid = NewGuID();
                var apitokenkode = NewGuID();
                //var dateexpired = expired();
                var ipaddress = getIPAddress();


                string sql = "INSERT INTO APITOKEN (APITOKENID, APITOKENKODE, APITOKENVALIDUNTIL, APITOKENIP)" +
               "VALUES('" + apitokenid + "','" + apitokenkode + "', (SYSDATE + interval '"+ hourexpire.CONFIGHOUREXPIRED + "' hour), '" + ipaddress + "')";
                ctx.Database.ExecuteSqlCommand(sql);
                status = 1;
                code = 200;
                responekode = apitokenkode;
                message = "";
            }
            return Json(new { status = status, code = code, message = message, result = responekode }, JsonRequestBehavior.AllowGet);
        }

        public List<Entities.Api> GetData()
        {
            var ctx = new PnbpContext();

            List<Entities.Api> Datahasil = new List<Entities.Api>();
           
            string currentYear = DateTime.Now.Year.ToString();
            Datahasil = ctx.Database.SqlQuery<Entities.Api>("SELECT DISTINCT * FROM MANFAAT WHERE TAHUN = " + currentYear + " ").ToList();  

            return Datahasil; //Json(new { status = status, code = code, result = Datahasil }, JsonRequestBehavior.AllowGet);
        }
        

        public static string getIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }
            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

       
        public string NewGuID()
        {
            string _result = "";
            using (var ctx = new PnbpContext())
            {
                _result = ctx.Database.SqlQuery<string>("SELECT RAWTOHEX(SYS_GUID()) FROM DUAL").FirstOrDefault<string>();
            }

            return _result;
        }
    }
}