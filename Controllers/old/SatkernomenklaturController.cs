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
    [AccessDeniedAuthorize]
    public class SatkernomenklaturController : Controller
    {
        public ActionResult Index()
        {
            var ctx = new PnbpContext();
            var get_data = ctx.Database.SqlQuery<Entities.Satkernomenklatur>("select distinct * from satker where statusaktif = 1 order by kode, tipekantorid asc").ToList();
            //return Json(get_data, JsonRequestBehavior.AllowGet);

            ViewData["get_data"] = get_data;
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            var ctx = new PnbpContext();
            var idsatker = NewGuID();
            var kodekantor = ((form.AllKeys.Contains("kodekantor")) ? form["kodekantor"] : "NULL");
            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "NULL");
            var namasatker = ((form.AllKeys.Contains("namasatker")) ? form["namasatker"] : "NULL");
            //var namaalias = ((form.AllKeys.Contains("namaalias")) ? form["namaalias"] : "NULL");
            var tahun = ((form.AllKeys.Contains("tahun")) ? form["tahun"] : "NULL");

            string sql = "INSERT INTO SATKER (KANTORID, KODE, KODESATKER, NAMA_SATKER, TAHUN, STATUSAKTIF)" +
                "VALUES('" + idsatker + "','" + kodekantor + "', '" + kodesatker + "', '" + namasatker + "','" + tahun + "', 1)";
            ctx.Database.ExecuteSqlCommand(sql);
            //return Json(sql, JsonRequestBehavior.AllowGet);
            return RedirectToAction("Index");
        }


        public ActionResult detail(string kantorid)
        {
            var ctx = new PnbpContext();
            var get_data = ctx.Database.SqlQuery<Entities.Satkernomenklatur>("select distinct * from satker where kantorid='" + kantorid + "' order by kode desc").FirstOrDefault();
            //return Json(get_data, JsonRequestBehavior.AllowGet);

            ViewData["get_data"] = get_data;

            return View();
        }

        public ActionResult update(string kantorid)
        {
            var ctx = new PnbpContext();
            var get_data = ctx.Database.SqlQuery<Entities.Satkernomenklatur>("select distinct * from satker where kantorid='" + kantorid + "' order by kode desc").FirstOrDefault();
            //return Json(get_data, JsonRequestBehavior.AllowGet);

            ViewData["get_data"] = get_data;

            return View();
        }
        [HttpPost]
        public ActionResult Update(FormCollection form, string kantorid)
        {
            var ctx = new PnbpContext();
            var idsatker = NewGuID();
            var kodekantor = ((form.AllKeys.Contains("kodekantor")) ? form["kodekantor"] : "NULL");
            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "NULL");
            //return Json(kodesatker, JsonRequestBehavior.AllowGet);
            var namasatker = ((form.AllKeys.Contains("namasatker")) ? form["namasatker"] : "NULL");
            //var namaalias = ((form.AllKeys.Contains("namaalias")) ? form["namaalias"] : "NULL");
            var tahun = ((form.AllKeys.Contains("tahun")) ? form["tahun"] : "NULL");

            string sql = "UPDATE SATKER SET KODE ='" + kodekantor + "',KODESATKER = '" + kodesatker + "', NAMA_SATKER='" + namasatker + "', TAHUN ='" + tahun + "' WHERE KANTORID ='" + kantorid + "' ";
            ctx.Database.ExecuteSqlCommand(sql);
            //return Json(sql, JsonRequestBehavior.AllowGet);
            return RedirectToAction("Index");
        }
        public ActionResult delete(string kantorid)
        {
            var ctx = new PnbpContext();

            string sql = "UPDATE SATKER SET STATUSAKTIF = 0 WHERE KANTORID = '" + kantorid + "' ";
            ctx.Database.ExecuteSqlCommand(sql);
            return RedirectToAction("Index");
        }

        public ActionResult import()
        {
            return View();
        }
        [HttpPost]
        public ActionResult import(FormCollection form)
        {
            var ctx = new PnbpContext();

            this.createDir("Content/Uploads");
            var upload = Request.Files["import"];

            string extension = Path.GetExtension(Request.Files["import"].FileName).ToLower();
            string fileName = createFileName() + "" + extension;
            string path = System.IO.Path.Combine(Server.MapPath("~/Content/Uploads"), fileName);
            upload.SaveAs(path);

            using (var Stream = new FileStream(Path.Combine(Server.MapPath("~/Content/Uploads"), fileName),
               FileMode.OpenOrCreate,
               FileAccess.ReadWrite,
               FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(Stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];

                    int rowCount = worksheet.Dimension.Rows;
                    //return Json(rowCount, JsonRequestBehavior.AllowGet);
                    int ColCount = worksheet.Dimension.Columns;


                    var KodeKantor = "";
                    var KodeSatker = "";
                    var NamaSatker = "";
                    var Tahun = "";

                    for (int row = 2; row <= rowCount; row++)
                    {

                        KodeKantor = worksheet.Cells[row, 2].Value.ToString();
                        KodeSatker = worksheet.Cells[row, 3].Value.ToString();
                        NamaSatker = worksheet.Cells[row, 4].Value.ToString();
                        Tahun = worksheet.Cells[row, 5].Value.ToString();

                        var cekkodesatker = ctx.Database.SqlQuery<int>("SELECT COUNT (KODE) FROM SATKER WHERE kode ='" + KodeKantor + "'").FirstOrDefault();
                        //return Json(cekkodesatker, JsonRequestBehavior.AllowGet);
                        if (cekkodesatker > 0)
                        {
                            var updatesatker = "UPDATE SATKER SET NAMA_SATKER = '" + NamaSatker + "', KODESATKER = '" + KodeSatker + "', TAHUN = '" + Tahun + "' WHERE KODE = '" + KodeKantor + "' ";
                            ctx.Database.ExecuteSqlCommand(updatesatker);
                        }
                        else
                        {
                            var kantorId = NewGuID();
                            var insertsatker = "INSERT INTO SATKER (KANTORID, KODE, NAMA_SATKER, KODESATKER, TAHUN, STATUSAKTIF) VALUES ('" + kantorId + "','" + KodeKantor + "','" + NamaSatker + "', '" + KodeSatker + "', '" + Tahun + "', 1)";
                            ctx.Database.ExecuteSqlCommand(insertsatker);
                        }
                        //return Json(cekkodesatker, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return RedirectToAction("Index");
        }

        private void createDir(string foldername)
        {
            string path = Path.Combine(Server.MapPath("~/"), foldername);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private string createFileName()
        {
            return Convert.ToString(DateTime.Now.Ticks);
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
