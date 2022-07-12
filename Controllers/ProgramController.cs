using System.Web.Mvc;

namespace Pnbp.Controllers
{
    public class ProgramController : Controller
    {
        // GET: Program
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetData(int page, int offset, string namaProgram)
        {
            var prgm = new Models.ProgramModel();
            var data = prgm.getAll(page, offset, namaProgram);

            //return Json(new { success = true, data = list, message = "Berhasil"}, JsonRequestBehavior.AllowGet);
            return Json(new { data = data, recordsTotal = data.Count > 0 ? data[0].Total : 0, recordsFiltered = data.Count > 0 ? data[0].Total : 0 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            var tm = new Models.TipeManfaatModel();
            var listTipeManfaat = tm.getAll();
            ViewBag.listTipeManfaat = listTipeManfaat;

            return View();
        }

        public ActionResult Save(Entities.Programs program)
        {
            #region validation
            if (program.TipeManfaatId <= 0 || string.IsNullOrEmpty(program.TipeOps) || program.TipeOps.Equals("-"))
            {
                return Json(new { success = false, message = "Periksa kembali inputan anda." }, JsonRequestBehavior.DenyGet);
            }
            #endregion


            var pm = new Models.ProgramModel();
            var result =  pm.Insert(program);

            return Json(new { success = result, message = result ? "Program berhasil ditambahkan." : "Program gagal ditambahkan."  }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            var tm = new Models.TipeManfaatModel();
            var listTipeManfaat = tm.getAll();
            ViewBag.listTipeManfaat = listTipeManfaat;

            var program = new Models.ProgramModel();
            var data = program.getById(id);
            ViewBag.data = data;

            return View();
        }

        public ActionResult Update(Entities.Programs program)
        {
            #region validation
            if (program.TipeManfaatId <= 0 || string.IsNullOrEmpty(program.TipeOps) || program.TipeOps.Equals("-"))
            {
                return Json(new { success = false, message = "Periksa kembali inputan anda." }, JsonRequestBehavior.DenyGet);
            }
            #endregion


            var pm = new Models.ProgramModel();
            var result = pm.Update(program);

            return Json(new { success = result, message = result ? "Program berhasil diubah." : "Program gagal diubah." }, JsonRequestBehavior.AllowGet);
        }

    }
}