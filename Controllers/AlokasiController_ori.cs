using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Configuration;

namespace Pnbp.Controllers
{
    [AccessDeniedAuthorize]
    public class AlokasiController : Controller
    {
        private string ConstructViewString(string viewName, object objModel, Dictionary<string, object> addViewData)
        {
            string strView = "";

            using (var sw = new StringWriter())
            {
                ViewData.Model = objModel;
                if (addViewData != null)
                {
                    foreach (string ky in addViewData.Keys)
                    {
                        ViewData[ky] = addViewData[ky];
                    }
                }

                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewCtx = new ViewContext(this.ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewCtx, sw);
                viewResult.ViewEngine.ReleaseView(this.ControllerContext, viewResult.View);
                strView = sw.ToString();
            }

            return strView;
        }

        #region Alokasi
        public ActionResult GetDaftarAlokasi(Entities.FormAlokasi m)
        {
            Pnbp.Models.AlokasiModel _AlokasiModel = new Models.AlokasiModel();

            List<Entities.AlokasiRows> result = _AlokasiModel.DaftarAlokasi(m.JenisAlokasi);

            return PartialView("ListAlokasi", result);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Ops()
        {
            Models.AlokasiModel _AlokasiModel = new Models.AlokasiModel();
            Entities.FormAlokasi _resultdata = new Entities.FormAlokasi();
            //int _tahapalokasi = _AlokasiModel.GetTahapAlokasi(Convert.ToString(DateTime.Now.Year), "OPS");
            //_resultdata.Tahap = "MP. #"+ _tahapalokasi;
            List<Entities.TotalPemanfaatan> _GetTotalPemanfaatan = _AlokasiModel.dtTotalPemanfaatan("", "", Convert.ToString(DateTime.Now.Year), "OPS");
            if (_GetTotalPemanfaatan.Count > 0)
            {
                _resultdata.Total = (_GetTotalPemanfaatan[0].Nilaianggaran > 0) ? string.Format("{0:#,##0,##}", _GetTotalPemanfaatan[0].Nilaianggaran) : "0";
                _resultdata.Sudahalokasi = (_GetTotalPemanfaatan[0].Nilaialokasi > 0) ? string.Format("{0:#,##0,##}", _GetTotalPemanfaatan[0].Nilaialokasi) : "0";
            }
            else
            {
                _resultdata.Total = string.Format("{0:#,##0,##}", 0);
                _resultdata.Sudahalokasi = string.Format("{0:#,##0,##}", 0);
            }
            return View("Ops", _resultdata);
        }

        public ActionResult NonOps()
        {
            Models.AlokasiModel _AlokasiModel = new Models.AlokasiModel();
            Entities.FormAlokasi _resultdata = new Entities.FormAlokasi();
            int _tahapalokasi = _AlokasiModel.GetTahapAlokasi(Convert.ToString(DateTime.Now.Year), "NONOPS");
            _resultdata.Tahap = "Prioritas #" + _tahapalokasi;
            List<Entities.TotalPemanfaatan> _GetTotalPemanfaatan = _AlokasiModel.dtTotalPemanfaatan("", "", Convert.ToString(DateTime.Now.Year), "NONOPS");
            if (_GetTotalPemanfaatan.Count > 0)
            {
                _resultdata.Total = (_GetTotalPemanfaatan[0].Nilaianggaran > 0) ? string.Format("{0:#,##0,##}", _GetTotalPemanfaatan[0].Nilaianggaran) : "0";
                _resultdata.Sudahalokasi = (_GetTotalPemanfaatan[0].Nilaialokasi > 0) ? string.Format("{0:#,##0,##}", _GetTotalPemanfaatan[0].Nilaialokasi) : "0";
            }
            else
            {
                _resultdata.Total = string.Format("{0:#,##0,##}", 0);
                _resultdata.Sudahalokasi = string.Format("{0:#,##0,##}", 0);
            }
            return View("NonOps", _resultdata);
        }

        public ActionResult GetCalc(Entities.FormAlokasi _Parameters)
        {
            Models.AlokasiModel _AlokasiModel = new Models.AlokasiModel();
            _Parameters.Dapatalokasi = _AlokasiModel.GetResultJobs("Calculate", _Parameters.JenisAlokasi);
            return Json(_Parameters, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CalculateAsync(Entities.FormAlokasi _parameter)
        {
            Models.AlokasiModel _data = new Models.AlokasiModel();
            Task.Run(() => 
            { 
                _data.RunCalculate((User as Entities.InternalUserIdentity).UserId, Convert.ToInt16(DateTime.Now.Year), _parameter.Tglpenerimaan, _parameter.JenisAlokasi); 
            });
            
            if (_parameter.JenisAlokasi == "OPS")
            {
                return View("Ops");
            }
            else
            {
                return View("NonOps");
            }
        }

        public ActionResult AllocationAsync(Entities.FormAlokasi _parameter)
        {
            if (_parameter.JenisAlokasi == "OPS")
            {
                Models.AlokasiModel _data = new Models.AlokasiModel();
                Task.Run(() =>
                {
                    _data.RunAllocation((User as Entities.InternalUserIdentity).UserId, Convert.ToInt16(DateTime.Now.Year), _parameter.Tglpenerimaan, _parameter.JenisAlokasi);
                });
                return View("Ops");
            }
            else
            {
                Models.AlokasiModel _data = new Models.AlokasiModel();
                Task.Run(() =>
                {
                    _data.RunAllocation((User as Entities.InternalUserIdentity).UserId, Convert.ToInt16(DateTime.Now.Year), _parameter.InputAlokasi.ToString(), _parameter.JenisAlokasi);
                });
                return View("NonOps");
            }
        }

        public ActionResult SaveAllocationAsync(Entities.FormAlokasi _parameter)
        {
            Models.AlokasiModel _data = new Models.AlokasiModel();
            Task.Run(() =>
            {
                _data.RunSaveAllocation((User as Entities.InternalUserIdentity).UserId, Convert.ToString(DateTime.Now.Year), _parameter.JenisAlokasi);
            });

            Entities.FormAlokasi _resultdata = new Entities.FormAlokasi();
            _resultdata.Tahap = "Tahap 1";
            return View("Ops");
        }

        public ActionResult ResetAllocationAsync(Entities.FormAlokasi _parameter)
        {
            Models.AlokasiModel _data = new Models.AlokasiModel();
            Task.Run(() =>
            {
                _data.RunReset((User as Entities.InternalUserIdentity).UserId, Convert.ToString(DateTime.Now.Year), _parameter.JenisAlokasi);
            });

            _parameter.Dapatalokasi = null;
            _parameter.InputAlokasi = null;
            return Json(_parameter, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CekJobs(string pTipe)
        {
            List<Entities.AlokasiJob> _eAlokasiJob = new List<Entities.AlokasiJob>();
            Models.AlokasiModel _AlokasiModel = new Models.AlokasiModel();
            _eAlokasiJob = _AlokasiModel.dt_RunningJob("", pTipe);
            return PartialView("CekJobs", _eAlokasiJob);
        }

        public ActionResult CekPrioritas()
        {
            List<Entities.ManfaatPrioritas> _ePrioritas = new List<Entities.ManfaatPrioritas>();
            Models.AlokasiModel _AlokasiModel = new Models.AlokasiModel();
            _ePrioritas = _AlokasiModel.dt_nonopsbyprioritas();
            return PartialView("CekPrioritas", _ePrioritas);
        }

        [HttpPost]
        public FileResult Export()
        {
            Pnbp.Models.AlokasiModel _AlokasiModel = new Models.AlokasiModel();
            DataTable dt = new DataTable("OPS");
            dt.Columns.AddRange(new DataColumn[9] { 
                new DataColumn("No",typeof(int)),
                new DataColumn("Tahun"),
                new DataColumn("Kode_Satker"),
                new DataColumn("Nama_Kantor"),
                new DataColumn("Nama_Program"),
                new DataColumn("Prioritas_Kegiatan"),
                new DataColumn("Nilai_Anggaran",typeof(decimal)),
                new DataColumn("Sudah_Alokasi",typeof(decimal)),
                new DataColumn("Nilai_Alokasi",typeof(decimal)) });

            List<Entities.AlokasiRows> result = _AlokasiModel.DaftarAlokasi("OPS");

            foreach (var rw in result)
            {
                dt.Rows.Add(rw.Rnumber, rw.Tahun, rw.Kodesatker, rw.NamaKantor, rw.NamaProgram, rw.PrioritasKegiatan, rw.NilaiAnggaran, rw.SudahAlokasi, rw.NilaiAlokasi);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AlokasiOPS.xlsx");
                }
            }
        }

        [HttpPost]
        public FileResult ExportNON()
        {
            Pnbp.Models.AlokasiModel _AlokasiModel = new Models.AlokasiModel();
            DataTable dt = new DataTable("NONOPS");
            dt.Columns.AddRange(new DataColumn[45] { 
                new DataColumn("No",typeof(int)),
                new DataColumn("Tahun"),
                new DataColumn("Kode_Satker"),
                new DataColumn("Nama_Kantor"),
                new DataColumn("Nama_Program"),
                new DataColumn("Prioritas_Kegiatan"),
                new DataColumn("Nilai_Anggaran",typeof(decimal)),
                new DataColumn("Sudah_Alokasi",typeof(decimal)),
                new DataColumn("Nilai_Alokasi",typeof(decimal)),
                new DataColumn("Anggaran_Januari",typeof(decimal)),
                new DataColumn("Anggaran_Februari",typeof(decimal)),
                new DataColumn("Anggaran_Maret",typeof(decimal)),
                new DataColumn("Anggaran_April",typeof(decimal)),
                new DataColumn("Anggaran_Mei",typeof(decimal)),
                new DataColumn("Anggaran_Juni",typeof(decimal)),
                new DataColumn("Anggaran_Juli",typeof(decimal)),
                new DataColumn("Anggaran_Agustus",typeof(decimal)),
                new DataColumn("Anggaran_September",typeof(decimal)),
                new DataColumn("Anggaran_Oktober",typeof(decimal)),
                new DataColumn("Anggaran_November",typeof(decimal)),
                new DataColumn("Anggaran_Desember",typeof(decimal)),
                new DataColumn("Ranking_Januari",typeof(decimal)),
                new DataColumn("Ranking_Februari",typeof(decimal)),
                new DataColumn("Ranking_Maret",typeof(decimal)),
                new DataColumn("Ranking_April",typeof(decimal)),
                new DataColumn("Ranking_Mei",typeof(decimal)),
                new DataColumn("Ranking_Juni",typeof(decimal)),
                new DataColumn("Ranking_Juli",typeof(decimal)),
                new DataColumn("Ranking_Agustus",typeof(decimal)),
                new DataColumn("Ranking_September",typeof(decimal)),
                new DataColumn("Ranking_Oktober",typeof(decimal)),
                new DataColumn("Ranking_November",typeof(decimal)),
                new DataColumn("Ranking_Desember",typeof(decimal)),
                new DataColumn("Alokasi_Januari",typeof(decimal)),
                new DataColumn("Alokasi_Februari",typeof(decimal)),
                new DataColumn("Alokasi_Maret",typeof(decimal)),
                new DataColumn("Alokasi_April",typeof(decimal)),
                new DataColumn("Alokasi_Mei",typeof(decimal)),
                new DataColumn("Alokasi_Juni",typeof(decimal)),
                new DataColumn("Alokasi_Juli",typeof(decimal)),
                new DataColumn("Alokasi_Agustus",typeof(decimal)),
                new DataColumn("Alokasi_September",typeof(decimal)),
                new DataColumn("Alokasi_Oktober",typeof(decimal)),
                new DataColumn("Alokasi_November",typeof(decimal)),
                new DataColumn("Alokasi_Desember",typeof(decimal))
            });

            List<Entities.AlokasiRows> result = _AlokasiModel.DaftarAlokasi("NONOPS");

            foreach (var rw in result)
            {
                dt.Rows.Add(
                    rw.Rnumber, rw.Tahun, rw.Kodesatker, rw.NamaKantor, rw.NamaProgram, rw.PrioritasKegiatan, rw.NilaiAnggaran, rw.SudahAlokasi, rw.NilaiAlokasi,
                    rw.AnggJan, rw.AnggFeb, rw.AnggMar, rw.AnggApr, rw.AnggMei, rw.AnggJun, rw.AnggJul, rw.AnggAgt, rw.AnggSep, rw.AnggOkt, rw.AnggNov, rw.AnggDes,
                    rw.RankJan, rw.RankFeb, rw.RankMar, rw.RankApr, rw.RankMei, rw.RankJun, rw.RankJul, rw.RankAgt, rw.RankSep, rw.RankOkt, rw.RankNov, rw.RankDes,
                    rw.AlokJan, rw.AlokFeb, rw.AlokMar, rw.AlokApr, rw.AlokMei, rw.AlokJun, rw.AlokJul, rw.AlokAgt, rw.AlokSep, rw.AlokOkt, rw.AlokNov, rw.AlokDes);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AlokasiNON.xlsx");
                }
            }
        } 
        #endregion

        #region Rekap
        public ActionResult RekapAlokasi()
        {
            Pnbp.Entities.FilterRekapAlokasi _frm = new Entities.FilterRekapAlokasi();
            _frm.lstahun = Pnbp.Models.AlokasiModel.lsTahunRekapAlokasi();
            _frm.tahun = (!string.IsNullOrEmpty(_frm.tahun)) ? _frm.tahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            return View(_frm);
        }

        public ActionResult DaftarRekapAlokasi(Entities.FilterRekapAlokasi frm)
        {
            Models.AlokasiModel model = new Models.AlokasiModel();
            Entities.FilterRekapAlokasi _frm = new Entities.FilterRekapAlokasi();
            _frm.tahun = (!string.IsNullOrEmpty(frm.tahun)) ? frm.tahun : ConfigurationManager.AppSettings["TahunAnggaran"].ToString();
            _frm.lsrekapalokasi = model.GetRekapAlokasi(_frm.tahun);
            return PartialView("RekapAlokasils", _frm);
        }

        public ActionResult RekapAlokasiDetail(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return RedirectToAction("RekapAlokasi");
            }
            Models.AlokasiModel model = new Models.AlokasiModel();
            List<Entities.DetailRekapAlokasiRows> data = model.GetRekapAlokasiDetail(Id);
            return PartialView("RekapAlokasiDetail", data);
        }
        #endregion
    }
}