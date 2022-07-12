using Pnbp.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Pnbp.Controllers
{
    public class KodeSpanController : Controller
    {
        // GET: KodeSpan
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetData(int status, string search)
        {
            List<Entities.KodeSpan> data = null;
            bool success = false;
            string msg;

            var ksm = new KodeSpanModel();
            data = ksm.dtKodeSpan(status, search);
            if (data == null || data.Count == 0)
            {
                msg = "Data tidak ditemukan";
            } else
            {
                success = true;
                msg = "Berhasil";
            }

            return Json(new { success = success, data = data, message = msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detail(string kodeoutput)
        {
            ViewBag.Judul = "Ubah Data";
            ViewBag.kodeoutput = kodeoutput;
            var ksm = new KodeSpanModel();
            var data = ksm.getKodeSpanByKodeOutput(kodeoutput);

            ViewBag.data = data;
            ViewBag.errorMessage = data == null ? "Data tidak ditemukan." : "";
            return View();
        }

        [HttpGet]
        public ActionResult GetDetail(string kodeoutput)
        {
            Entities.KodeSpan data;
            bool success = false;
            string msg = "";

            if (string.IsNullOrEmpty(kodeoutput))
            {
                msg = "Kode Output tidak boleh kosong";
            }

            var ksm = new KodeSpanModel();
            data = ksm.getKodeSpanByKodeOutput(kodeoutput);
            if (data != null)
            {
                success = true;
            }

            return Json(new { success = success, data = data, message = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Update(string KodeOutput, string NamaProgram, string Tipe)
        {
            var ksm = new KodeSpanModel();
            var isValid = true;
            if (string.IsNullOrEmpty(KodeOutput) || string.IsNullOrEmpty(NamaProgram) || string.IsNullOrEmpty(Tipe) || Tipe.Contains("-"))
            {
                ViewBag.errorMessage = "Data yang diinput tidak valid. Periksa kembali inputan anda.";
                isValid = false;                
            }

            if (isValid)
            {
                var res = ksm.update(KodeOutput, NamaProgram, Tipe);
                if (res)
                {
                    return RedirectToAction("Index");
                } 
                else
                {
                    ViewBag.errorMessage = "Data Kode Span gagal diupdate.";
                }
            }

            var data = ksm.getKodeSpanByKodeOutput(KodeOutput);

            ViewBag.Judul = "Ubah Data";
            ViewBag.kodeoutput = KodeOutput;

            ViewBag.data = data;
            return View("Detail");
        }
    }
}