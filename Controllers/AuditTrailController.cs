using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pnbp.Controllers
{
    public class AuditTrailController : Controller
    {
        // GET: AuditTrail
        //public ActionResult Index()
        //{
        //    return View();
        //}

        public ActionResult AuditTrailIndex()
        {
            var userIdentity = new Pnbp.Codes.Functions().claimUser();

            var ctx = new PnbpContext();
            string kantorid = userIdentity.KantorId;

            string query =
                @"
                   SELECT
	                    a.LOG_NAME,
	                    a.LOG_CREATE_BY,
	                    a.LOG_CREATE_DATE,
	                    a.LOG_KANTORID,
	                    b.NAMA_SATKER
                    FROM
	                    LOG_AKTIFITAS a
	                    JOIN KANTOR b ON a.LOG_KANTORID = b.KANTORID 
                    ORDER BY
	                    a.LOG_CREATE_BY ASC";
            var get_data = ctx.Database.SqlQuery<Entities.AuditTrail>(query).ToList();
            //return Json(get_data, JsonRequestBehavior.AllowGet);
            ViewData["get_data"] = get_data;
            return View();
        }

        public ActionResult AuditTrailPengembalian()
        {
            var userIdentity = new Pnbp.Codes.Functions().claimUser();
            var ctx = new PnbpContext();
            string kantorid = userIdentity.KantorId;

            string query =
                @"
                  SELECT
	                a.LOG_NAME,
	                b.NAMA_SATKER,
	                a.LOG_NOMOR_SURAT,
	                a.LOG_CREATE_BY,
	                a.LOG_create_date,
	                a.LOG_KANTORID,
	                c.KODEBILLING,
	                c.NTPN 
                FROM
	                LOG_AKTIFITAS a
	                LEFT JOIN KANTOR b ON a.LOG_KANTORID = b.KANTORID
	                LEFT JOIN BERKASKEMBALIAN c ON a.LOG_DATA_ID = c.PENGEMBALIANPNBPID 
                WHERE
	                LOG_TIPE = 'PENGEMBALIANPNBP'";
            var get_data = ctx.Database.SqlQuery<Entities.AuditTrail>(query).ToList();
            //return Json(get_data, JsonRequestBehavior.AllowGet);
            ViewData["get_data"] = get_data;
            return View();
        }

        public ActionResult AuditTrailEntri()
        {
            var userIdentity = new Pnbp.Codes.Functions().claimUser();
            var ctx = new PnbpContext();
            string kantorid = userIdentity.KantorId;

            string query =
                @"
                  SELECT
	                    a.LOG_NAME,
	                    b.NAMA_SATKER,
	                    c.namaprogram,
	                    a.LOG_CREATE_BY,
	                    a.LOG_CREATE_DATE,
	                    a.LOG_KANTORID,
	                    b.NAMA_SATKER
                    FROM
	                    LOG_AKTIFITAS a
	                    LEFT JOIN KANTOR b ON a.LOG_KANTORID = b.KANTORID 
	                    LEFT JOIN MANFAAT c ON a.LOG_DATA_ID = c.MANFAATID
	                    LEFT JOIN PROGRAM d ON c.PROGRAMID = d.PROGRAMID
                    WHERE LOG_TIPE = 'RENAKSI'
                    ORDER BY
	                    a.LOG_CREATE_BY ASC";
            var get_data = ctx.Database.SqlQuery<Entities.AuditTrail>(query).ToList();
            //return Json(get_data, JsonRequestBehavior.AllowGet);
            ViewData["get_data"] = get_data;
            return View();
        }

        public ActionResult AuditTrailAlokasi()
        {
            var userIdentity = new Pnbp.Codes.Functions().claimUser();
            var ctx = new PnbpContext();
            string kantorid = userIdentity.KantorId;

            string query =
                @"
                   SELECT
	                    a.LOG_NAME,
	                    a.LOG_CREATE_BY,
	                    a.LOG_CREATE_DATE,
	                    a.LOG_KANTORID,
	                    b.NAMA_SATKER
                    FROM
	                    LOG_AKTIFITAS a
	                    JOIN KANTOR b ON a.LOG_KANTORID = b.KANTORID 
                    ORDER BY
	                    a.LOG_CREATE_BY ASC";
            var get_data = ctx.Database.SqlQuery<Entities.AuditTrail>(query).ToList();
            //return Json(get_data, JsonRequestBehavior.AllowGet);
            ViewData["get_data"] = get_data;
            return View();
        }
    }
}