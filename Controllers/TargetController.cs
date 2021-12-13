using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
//using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;
using OfficeOpenXml;

namespace Pnbp.Controllers
{
    public class TargetController : Controller
    {
        //// GET: Target
        //public ActionResult Index()
        //{
        //    return View();
        //}

        //Untuk Reload Tampilan List Target
        public ActionResult TargetIndex()
        {
            var ctx = new PnbpContext();
            string kantorid = (User as Pnbp.Entities.InternalUserIdentity).KantorId;
            string currentYear = DateTime.Now.Year.ToString();
            //return Json(kantorid, JsonRequestBehavior.AllowGet);
            if (@OtorisasiUser.GetJenisKantorUser() == "Pusat")
            {
                var get_data = ctx.Database.SqlQuery<Entities.Target>("SELECT DISTINCT * FROM TARGETPROSEDUR WHERE STATUSTARGET = 1 AND TAHUN = "+ currentYear +" ORDER BY KODETARGET DESC").ToList();
                ViewData["get_data"] = get_data;
            } else
            {
                var get_data = ctx.Database.SqlQuery<Entities.Target>("SELECT DISTINCT * FROM TARGETPROSEDUR WHERE KANTORID = '" + kantorid + "' AND STATUSTARGET = 1 AND TAHUN =" + currentYear + " ORDER BY KODETARGET DESC").ToList();
                ViewData["get_data"] = get_data;
            }
            return View();
        }

        //Untuk Reload Tampilan Create Target
        public ActionResult TargetCreate()
        {
            //get Penerimaan
            string currentYear = DateTime.Now.Year.ToString();
            var ctx = new PnbpContext();
            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            //var get_penerimaan = ctx.Database.SqlQuery<Entities.GetJenisPenerimaan>("SELECT DISTINCT JENISPENERIMAAN FROM REKAPPENERIMAANDETAIL").ToList();
            var get_program = ctx.Database.SqlQuery<Entities.GetProgram>("SELECT DISTINCT NAMAKANTOR, JENISPENERIMAAN, NAMAPROSEDUR, KANTORID FROM REKAPPENERIMAANDETAIL WHERE tahun = "+ currentYear +" and KANTORID = '"+ kantorid + "'").ToList();
            //return Json(get_program, JsonRequestBehavior.AllowGet);
            //ViewData["get_penerimaan"] = get_penerimaan;
            ViewData["get_program"] = get_program;
            return View();
        }

        [HttpPost]
        public ActionResult TargetCreate(FormCollection form)
        {
            string currentYear = DateTime.Now.Year.ToString();
            var ctx = new PnbpContext();
            var kodetarget = NewGuID();
            var namaprogram = ((form.AllKeys.Contains("idprogram")) ? form["idprogram"] : "NULL");
            var nilaitarget = ((form.AllKeys.Contains("nilaitarget")) ? form["nilaitarget"] : "NULL");
            var idkantor = ((form.AllKeys.Contains("idkantor")) ? form["idkantor"] : "NULL");
            var jenislayanan = ((form.AllKeys.Contains("jenislayanan")) ? form["jenislayanan"] : "NULL");
            var namakantor = ((form.AllKeys.Contains("namakantor")) ? form["namakantor"] : "NULL");
            var targetfisik = ((form.AllKeys.Contains("targetfisik")) ? form["targetfisik"] : "NULL");

            string sql = "INSERT INTO TARGETPROSEDUR (KODETARGET, NAMAPROSEDUR, NILAITARGET, STATUSTARGET, KANTORID, JENISLAYANAN, NAMAKANTOR, TARGETFISIK, TAHUN)" +
                "VALUES('" + kodetarget + "','" + namaprogram + "','" + nilaitarget.Replace(".", String.Empty) + "',1,'" + idkantor + "','"+ jenislayanan +"','"+namakantor+"', '" + targetfisik.Replace(".", String.Empty) + "', "+ currentYear +")";
            ctx.Database.ExecuteSqlCommand(sql);


            return RedirectToAction("TargetIndex");
        }

        public ActionResult TargetDetail(string kodetarget)
        {
            var ctx = new PnbpContext();
            var get_data = ctx.Database.SqlQuery<Entities.Target>("SELECT DISTINCT * FROM TARGETPROSEDUR WHERE KODETARGET='" + kodetarget + "' ORDER BY KODETARGET DESC").FirstOrDefault();
            //return Json(new { result = get_data }, JsonRequestBehavior.AllowGet);

            ViewData["get_data"] = get_data;

            return View();
        }

        public ActionResult TargetDelete(string kodetarget)
        {
            var ctx = new PnbpContext();

            //string sql = "UPDATE TARGETPROSEDUR SET STATUSTARGET = 0 WHERE KODETARGET = '" + kodetarget + "'";
            string sql = "DELETE FROM TARGETPROSEDUR WHERE KODETARGET = '" + kodetarget + "'";
            //return Json(new { result = sql }, JsonRequestBehavior.AllowGet);
            ctx.Database.ExecuteSqlCommand(sql);
            return RedirectToAction("TargetIndex");
        }

        public ActionResult TargetUpdate(string kodetarget)
        {
            string currentYear = DateTime.Now.Year.ToString();
            var ctx = new PnbpContext();
            string kantorid = (User as Entities.InternalUserIdentity).KantorId;
            var get_data = ctx.Database.SqlQuery<Entities.Target>("SELECT DISTINCT * FROM TARGETPROSEDUR WHERE KODETARGET='" + kodetarget + "' ORDER BY KODETARGET DESC").FirstOrDefault();
            //return Json(new { result = get_data }, JsonRequestBehavior.AllowGet);

            //get Penerimaan
            //var ctx = new PnbpContext();
            //var get_penerimaan = ctx.Database.SqlQuery<Entities.GetJenisPenerimaan>("SELECT DISTINCT JENISPENERIMAAN FROM REKAPPENERIMAANDETAIL").ToList();
            var get_program = ctx.Database.SqlQuery<Entities.GetProgram>("SELECT DISTINCT NAMAKANTOR, JENISPENERIMAAN, NAMAPROSEDUR, KANTORID FROM REKAPPENERIMAANDETAIL WHERE tahun = " + currentYear + " and KANTORID = '" + kantorid + "'").ToList();
            //return Json(get_penerimaan, JsonRequestBehavior.AllowGet);
            //ViewData["get_penerimaan"] = get_penerimaan;
            ViewData["get_program"] = get_program;
            // get penerimaan

            ViewData["get_data"] = get_data;
            return View();

        }

        [HttpPost]
        public ActionResult TargetUpdate(FormCollection form, string kodetarget)
        {
            var ctx = new PnbpContext();
            var kodetargets = NewGuID();
            var namaprosedur = ((form.AllKeys.Contains("idprogram")) ? form["idprogram"] : "NULL");
            var nilaitarget = ((form.AllKeys.Contains("nilaitarget")) ? form["nilaitarget"] : "NULL");
            var targetfisik = ((form.AllKeys.Contains("targetfisik")) ? form["targetfisik"] : "NULL");
            var jenislayanan = ((form.AllKeys.Contains("jenislayanan")) ? form["jenislayanan"] : "NULL");
            //return Json(new { result = kodetargets, namaprosedur, nilaitarget, jenislayanan }, JsonRequestBehavior.AllowGet);
            //string sql = "UPDATE TARGETPROSEDUR SET JENISLAYANAN='"+jenislayanan+"', NAMAPROSEDUR='" + namaprosedur + "', NILAITARGET=" + nilaitarget.Replace(".", String.Empty) + ", TARGETFISIK=" + targetfisik.Replace(".", String.Empty) + " WHERE KODETARGET = '" + kodetarget + "'";
            string sql = "UPDATE TARGETPROSEDUR SET NAMAPROSEDUR='" + namaprosedur + "', NILAITARGET=" + nilaitarget.Replace(".", String.Empty) + ", TARGETFISIK=" + targetfisik.Replace(".", String.Empty) + " WHERE KODETARGET = '" + kodetarget + "'";
            ctx.Database.ExecuteSqlCommand(sql);
            return RedirectToAction("TargetIndex");
        }

        //public ActionResult FillProgram(string jenislayanan)
        //{
        //    //return Json(jenislayanan, JsonRequestBehavior.AllowGet);
        //    string currentYear = DateTime.Now.Year.ToString();
        //    var ctx = new PnbpContext();
        //    string kantorid = (User as Entities.InternalUserIdentity).KantorId;
        //    //return Json(kantorid, JsonRequestBehavior.AllowGet);

        //    //var get_program = ctx.Database.SqlQuery<Entities.GetProgram>("SELECT DISTINCT NAMAKANTOR, JENISPENERIMAAN, NAMAPROSEDUR, KANTORID FROM REKAPPENERIMAANDETAIL WHERE tahun = "+ currentYear +" and JenisPenerimaan = '"+jenislayanan+ "' and KANTORID = '"+ kantorid + "'").ToList();
        //    var get_program = ctx.Database.SqlQuery<Entities.GetProgram>("SELECT DISTINCT NAMAKANTOR, JENISPENERIMAAN, NAMAPROSEDUR, KANTORID FROM REKAPPENERIMAANDETAIL WHERE tahun = "+ currentYear +" and KANTORID = '"+ kantorid + "'").ToList();
        //    return Json(get_program, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult JenisPenerimaan()
        //{
        //    var ctx = new PnbpContext();
        //    var get_penerimaan = ctx.Database.SqlQuery<Entities.GetJenisPenerimaan>("SELECT DISTINCT JENISPENERIMAAN FROM REKAPPENERIMAANDETAIL").ToList();
        //    return Json(get_penerimaan, JsonRequestBehavior.AllowGet);
        //    ViewData["get_penerimaan"] = get_penerimaan;
        //    return View();
        //}

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