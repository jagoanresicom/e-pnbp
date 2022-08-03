using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Configuration;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

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
            var ctx = new PnbpContext();
            var get_data = ctx.Database.SqlQuery<Entities.getDataSatker>("SELECT DISTINCT KANTORID, KODESATKER, NAMAKANTOR,NVL (SUM (NILAIANGGARAN), 0)AS NILAIANGGARAN, NVL (SUM (TOTALALOKASI), 0) AS TOTALALOKASI, NVL (SUM (NILAIALOKASI), 0) AS NILAIALOKASI, NVL (SUM (SISAALOKASI), 0) AS SISAALOKASI, row_number() over (order by sum(NILAIANGGARAN) desc) as urutan FROM MANFAAT WHERE TIPE = 'NONOPS' GROUP BY KANTORID, NAMAKANTOR, KODESATKER").ToList();
            ViewData["get_data"] = get_data;

            var get_approve = ctx.Database.SqlQuery<Entities.getDataApprove>("SELECT a.KANTORID, b.kodesatker, b.NAMA_SATKER, a.PAGU, a.TOTALALOKASI, a.REALISASI, a.MP, a.ALOKASI FROM ALOKASISATKERNONOPS a LEFT JOIN SATKER b ON a.KANTORID = b.KANTORID WHERE APPROVE = 'on' ").ToList();
            ViewData["get_approve"] = get_approve;

            //return Json(currentMonth, JsonRequestBehavior.AllowGet);


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

        public ActionResult getSatker(string persentase)
        {
            var ctx = new PnbpContext();
            var get_data = ctx.Database.SqlQuery<Entities.Satker>("SELECT DISTINCT KANTORID, NAMAKANTOR FROM manfaat WHERE TIPE = 'NONOPS' AND PERSENGROUP < '" + persentase + "' AND KODESATKER IS NOT NULL ").ToList();
            return Json(get_data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getNilaiAlokasi (string kantorid)
        {
            var ctx = new PnbpContext();
            string currentYear = DateTime.Now.Year.ToString();
            var get_detaildata = ctx.Database.SqlQuery<Entities.getAlokasi>("SELECT DISTINCT * FROM MANFAAT WHERE TAHUN = "+currentYear+"  AND TIPE = 'NONOPS' AND KANTORID = '" + kantorid + "'").ToList();
            var summary = "";
            foreach (var value in get_detaildata)
            {
                if (value.AlokJul == 9)
                {
                    summary = "ANGGJUL";
                }else if(value.AlokAgt == 9)
                {
                    summary = "ANGGAGT";
                }
                //return Json(summary, JsonRequestBehavior.AllowGet);
            }
            var get_data = ctx.Database.SqlQuery<Entities.AlokasiSatker>("SELECT DISTINCT KANTORID, SUM("+ summary +") AS NILAIALOKASI FROM MANFAAT WHERE TAHUN = 2021  AND TIPE = 'NONOPS' AND KANTORID = '"+ kantorid +"' GROUP BY KANTORID").ToList();
            //var get_data = ctx.Database.SqlQuery<Entities.AlokasiSatker>("SELECT DISTINCT KANTORID, SUM(ANGGMEI + ANGGJUN + ANGGJUL) AS NILAIALOKASI FROM MANFAAT WHERE TAHUN = 2021  AND TIPE = 'NONOPS' AND KANTORID = '"+ kantorid +"' GROUP BY KANTORID").ToList();
            //var get_data = ctx.Database.SqlQuery<Entities.AlokasiSatker>(" SELECT DISTINCT KANTORID, ( CASE WHEN ALOKJAN = 9 AND ALOKFEB = 0 THEN SUM (ANGGJAN)  WHEN ALOKFEB = 9  AND ALOKMAR = 0 THEN SUM (ANGGJAN + ALOKFEB) WHEN ALOKMAR = 9 AND ALOKAPR = 0 THEN SUM (ANGGJAN + ANGGFEB + ANGGMAR) WHEN ALOKAPR = 9 AND ALOKMEI = 0 THEN SUM ( ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR ) WHEN ALOKMEI = 9 AND ALOKJUN = 0 THEN SUM ( ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI ) WHEN ALOKJUN = 9 AND ALOKJUL = 0 THEN SUM ( ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN ) WHEN ALOKJUL = 9 AND ALOKAGT = 0 THEN SUM ( ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL ) WHEN ALOKAGT = 9 AND ALOKSEP = 0 THEN SUM ( ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT ) WHEN ALOKSEP = 9 AND ALOKOKT = 0 THEN SUM ( ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT + ANGGSEP ) WHEN ALOKOKT = 9 AND ALOKNOV = 0 THEN SUM ( ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT + ANGGSEP + ANGGOKT ) WHEN ALOKNOV = 9 AND ALOKDES = 0 THEN SUM ( ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT + ANGGSEP + ANGGOKT + ANGGNOV ) WHEN ALOKDES = 9 THEN SUM ( ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT + ANGGSEP + ANGGOKT + ANGGNOV + ANGGDES ) END ) AS NILAIALOKASI FROM MANFAAT WHERE TAHUN = "+currentYear+" AND TIPE = 'NONOPS' AND KANTORID = '"+ kantorid +"' GROUP BY KANTORID, ALOKJAN, ALOKFEB, ALOKMAR, ALOKAPR, ALOKMEI, ALOKJUN, ALOKJUL, ALOKAGT, ALOKSEP, ALOKOKT, ALOKNOV, ALOKDES").ToList();
            return Json(get_data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getDataSatker(string persentase)
        {
            string currentYear = DateTime.Now.Year.ToString();
            var ctx = new PnbpContext();
            var get_data = ctx.Database.SqlQuery<Entities.getDataSatker>(@"
                SELECT
	                *
                FROM
	                (
		                WITH aa AS (
			                SELECT DISTINCT
                                A .KODESATKER,
				                A .KANTORID,
				                A .NAMAKANTOR,
				                NVL (SUM(A .NILAIANGGARAN), 0) AS NILAIANGGARAN,
				                (
					                CASE
					                WHEN A .ALOKJAN = 1
					                AND ALOKFEB = 0
					                OR ALOKFEB = 9 THEN
						                (
							                SELECT DISTINCT
								                NVL (SUM(ANGGJAN), 0) AS SUBTOTAL
							                FROM
								                MANFAAT
							                WHERE
								                KANTORID = A .KANTORID
							                AND TAHUN = 2021
							                AND TIPE = 'NONOPS'
							                GROUP BY
								                NAMAKANTOR
						                )
					                WHEN A .ALOKFEB = 1
					                AND ALOKMAR = 0
					                OR ALOKMAR = 9 THEN
						                (
							                SELECT DISTINCT
								                NVL (SUM(ANGGJAN + ANGGFEB), 0) AS SUBTOTAL
							                FROM
								                MANFAAT
							                WHERE
								                KANTORID = A .KANTORID
							                AND TAHUN = 2021
							                AND TIPE = 'NONOPS'
							                GROUP BY
								                NAMAKANTOR
						                )
					                WHEN A .ALOKMAR = 1
					                AND ALOKAPR = 0
					                OR ALOKAPR = 9 THEN
						                (
							                SELECT DISTINCT
								                NVL (SUM(ANGGJAN + ANGGFEB + ANGGMAR), 0) AS SUBTOTAL
							                FROM
								                MANFAAT
							                WHERE
								                KANTORID = A .KANTORID
							                AND TAHUN = 2021
							                AND TIPE = 'NONOPS'
							                GROUP BY
								                NAMAKANTOR
						                )
					                WHEN A .ALOKAPR = 1
					                AND ALOKMEI = 0
					                OR ALOKMEI = 9 THEN
						                (
							                SELECT DISTINCT
								                NVL (SUM(ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR), 0) AS SUBTOTAL
							                FROM
								                MANFAAT
							                WHERE
								                KANTORID = A .KANTORID
							                AND TAHUN = 2021
							                AND TIPE = 'NONOPS'
							                GROUP BY
								                NAMAKANTOR
						                )
					                WHEN A .ALOKMEI = 1
					                AND ALOKJUN = 0
					                OR ALOKJUN = 9 THEN
						                (
							                SELECT DISTINCT
								                NVL (SUM(ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI), 0) AS SUBTOTAL
							                FROM
								                MANFAAT
							                WHERE
								                KANTORID = A .KANTORID
							                AND TAHUN = 2021
							                AND TIPE = 'NONOPS'
							                GROUP BY
								                NAMAKANTOR
						                )
					                WHEN A .ALOKJUN = 1
					                AND ALOKJUL = 0
					                OR ALOKJUL = 9 THEN
						                (
							                SELECT DISTINCT
								                NVL (SUM(ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN), 0) AS SUBTOTAL
							                FROM
								                MANFAAT
							                WHERE
								                KANTORID = A .KANTORID
							                AND TAHUN = 2021
							                AND TIPE = 'NONOPS'
							                GROUP BY
								                NAMAKANTOR
						                )
						                WHEN A .ALOKJUL = 1
						                AND ALOKAGT = 0
						                OR ALOKAGT = 9 THEN
						                (
							                SELECT DISTINCT
								                NVL (SUM(ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL), 0) AS SUBTOTAL
							                FROM
								                MANFAAT
							                WHERE
								                KANTORID = A .KANTORID
							                AND TAHUN = 2021
							                AND TIPE = 'NONOPS'
							                GROUP BY
								                NAMAKANTOR
						                )
						                WHEN A .ALOKAGT = 1
						                AND ALOKSEP = 0
						                OR ALOKSEP = 9 THEN
						                (
							                SELECT DISTINCT
								                NVL (SUM(ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT), 0) AS SUBTOTAL
							                FROM
								                MANFAAT
							                WHERE
								                KANTORID = A .KANTORID
							                AND TAHUN = 2021
							                AND TIPE = 'NONOPS'
							                GROUP BY
								                NAMAKANTOR
						                )
						                WHEN A .ALOKSEP = 1
						                AND ALOKOKT = 0
						                OR ALOKOKT = 9 THEN
						                (
							                SELECT DISTINCT
								                NVL (SUM(ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT + ANGGSEP), 0) AS SUBTOTAL
							                FROM
								                MANFAAT
							                WHERE
								                KANTORID = A .KANTORID
							                AND TAHUN = 2021
							                AND TIPE = 'NONOPS'
							                GROUP BY
								                NAMAKANTOR
						                )
						                WHEN A .ALOKOKT = 1
						                AND ALOKNOV = 0
						                OR ALOKNOV = 9 THEN
						                (
							                SELECT DISTINCT
								                NVL (SUM(ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT + ANGGSEP + ANGGOKT), 0) AS SUBTOTAL
							                FROM
								                MANFAAT
							                WHERE
								                KANTORID = A .KANTORID
							                AND TAHUN = 2021
							                AND TIPE = 'NONOPS'
							                GROUP BY
								                NAMAKANTOR
						                )
						                WHEN A .ALOKNOV = 1
						                AND ALOKDES = 0
						                OR ALOKDES = 9 THEN
						                (
							                SELECT DISTINCT
								                NVL (SUM(ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT + ANGGSEP + ANGGOKT + ANGGNOV), 0) AS SUBTOTAL
							                FROM
								                MANFAAT
							                WHERE
								                KANTORID = A .KANTORID
							                AND TAHUN = 2021
							                AND TIPE = 'NONOPS'
							                GROUP BY
								                NAMAKANTOR
						                )
						                ELSE
						                (
							                SELECT DISTINCT
								                NVL (SUM(ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT + ANGGSEP + ANGGOKT + ANGGNOV + ANGGDES), 0) AS SUBTOTAL
							                FROM
								                MANFAAT
							                WHERE
								                KANTORID = A .KANTORID
							                AND TAHUN = 2021
							                AND TIPE = 'NONOPS'
							                GROUP BY
								                NAMAKANTOR
						                )
					                END
				                ) AS TOTALALOKASI
			                FROM
				                MANFAAT A
			                WHERE
				                A .TAHUN = 2021
			                AND A .TIPE = 'NONOPS'
			                GROUP BY
                                A .KODESATKER,
				                A .KANTORID,
				                A .NAMAKANTOR,
				                A .ALOKJAN,
				                A .ALOKFEB,
				                A .ALOKMAR,
				                A .ALOKAPR,
				                A .ALOKMEI,
				                A .ALOKJUN,
				                A .ALOKJUL,
				                A .ALOKAGT,
				                A .ALOKSEP,
				                A .ALOKOKT,
				                A .ALOKNOV,
				                A .ALOKDES
		                ) SELECT DISTINCT
                            kodesatker,
			                kantorid,
			                namakantor,
			                nilaianggaran,
			                totalalokasi,
			                NVL (
				                SUM (nilaianggaran - totalalokasi),
				                0
			                ) AS sisaalokasi,
			                ROUND (
				                NVL (
					                SUM (totalalokasi / nilaianggaran) * 100,
					                0
				                ),
				                2
			                ) AS MP
		                FROM
			                aa
		                GROUP BY
                            kodesatker,
			                kantorid,
			                namakantor,
			                nilaianggaran,
			                totalalokasi
	                )
            ").ToList();
            return Json(get_data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult get_alokasi(Decimal? InputAlokasi)
        {
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            var ctx = new PnbpContext();

            //string replace_april = "UPDATE MANFAAT SET ALOKAPR = 0 WHERE ALOKAPR = 9 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
            //ctx.Database.ExecuteSqlCommand(replace_april);

            //string replace_mei = "UPDATE MANFAAT SET ALOKMEI = 0 WHERE ALOKMEI = 9 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
            //ctx.Database.ExecuteSqlCommand(replace_mei);

            //string replace_juni = "UPDATE MANFAAT SET ALOKJUN = 0 WHERE ALOKJUN = 9 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
            //ctx.Database.ExecuteSqlCommand(replace_juni);

            //string replace_juli = "UPDATE MANFAAT SET ALOKJUL = 0 WHERE ALOKJUL = 9 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
            //ctx.Database.ExecuteSqlCommand(replace_juli);

            string replace_agustus = "UPDATE MANFAAT SET ALOKAGT = 0 WHERE ALOKAGT = 9 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
            ctx.Database.ExecuteSqlCommand(replace_agustus);

            string replace_september = "UPDATE MANFAAT SET ALOKSEP = 0 WHERE ALOKSEP = 9 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
            ctx.Database.ExecuteSqlCommand(replace_september);

            string replace_oktober = "UPDATE MANFAAT SET ALOKOKT = 0 WHERE ALOKOKT = 9 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
            ctx.Database.ExecuteSqlCommand(replace_oktober);

            string replace_november = "UPDATE MANFAAT SET ALOKNOV = 0 WHERE ALOKNOV = 9 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
            ctx.Database.ExecuteSqlCommand(replace_november);

            string replace_desember = "UPDATE MANFAAT SET ALOKDES = 0 WHERE ALOKDES = 9 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
            ctx.Database.ExecuteSqlCommand(replace_desember);


            var getAnggApr = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM(ANGGAPR),0) FROM MANFAAT WHERE TAHUN = "+ currentYear +" AND TIPE = 'NONOPS' AND ALOKAPR = 0").FirstOrDefault();
                var kal_apr = (InputAlokasi - getAnggApr);
                
            // cek kalkulasi april
            if (kal_apr > 0)
                {
                    string update_april = "UPDATE MANFAAT SET ALOKAPR = 9 WHERE ALOKAPR = 0 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
                    ctx.Database.ExecuteSqlCommand(update_april);

                    var getAnggMei = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM(ANGGMEI),0) FROM MANFAAT WHERE TAHUN = "+ currentYear +" AND TIPE = 'NONOPS' AND ALOKMEI = 0").FirstOrDefault();
                    var kal_mei = (kal_apr - getAnggMei);

                    //cek kalkulasi
                    if (kal_mei >= 0)
                        {
                        string update_mei = "UPDATE MANFAAT SET ALOKMEI = 9 WHERE ALOKMEI = 0 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
                        ctx.Database.ExecuteSqlCommand(update_mei);

                        var getAnggJun = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM(ANGGJUN),0) FROM MANFAAT WHERE TAHUN = "+ currentYear +" AND TIPE = 'NONOPS' AND ALOKJUN = 0").FirstOrDefault();
                        var kal_jun = (kal_mei - getAnggJun);

                        // cek kalkulasi juni
                        if (kal_jun >= 0)
                        {
                            string update_juni = "UPDATE MANFAAT SET ALOKJUN = 9 WHERE ALOKJUN = 0 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
                            ctx.Database.ExecuteSqlCommand(update_juni);

                            var getAnggJul = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM(ANGGJUL),0) FROM MANFAAT WHERE TAHUN = " + currentYear +" AND TIPE = 'NONOPS' AND ALOKJUL = 0").FirstOrDefault();
                            var kal_Jul = (kal_jun - getAnggJul);

                            //cek kalkulasi juli
                            if (kal_Jul >= 0)
                            {
                                string update_juli = "UPDATE MANFAAT SET ALOKJUL = 9 WHERE ALOKJUL = 0 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
                            ctx.Database.ExecuteSqlCommand(update_juli);

                                var getAnggAgt = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM(ANGGAGT),0) FROM MANFAAT WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKAGT = 0").FirstOrDefault();
                                var kal_agt = (kal_Jul - getAnggAgt);

                                //cek kalkulasi agustus
                                if (kal_agt >= 0)
                                {
                                    string update_agustus = "UPDATE MANFAAT SET ALOKAGT = 9 WHERE ALOKAGT = 0 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
                                ctx.Database.ExecuteSqlCommand(update_agustus);

                                    var getAnggSep = ctx.Database.SqlQuery<Decimal>("SELECT SUM(ANGGSEP) FROM MANFAAT WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKSEP = 0").FirstOrDefault();
                                    var kal_sep = (kal_agt - getAnggSep);

                                    //cek kalkulasi september
                                    if (kal_sep >= 0)
                                    {
                                        string update_september = "UPDATE MANFAAT SET ALOKSEP = 9 WHERE ALOKSEP = 0 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
                                    ctx.Database.ExecuteSqlCommand(update_september);

                                        var getAnggOkt = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM(ANGGOKT),0) FROM MANFAAT WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKOKT = 0").FirstOrDefault();
                                        var kal_okt = (kal_sep - getAnggOkt);

                                        //cek kalkulasi oktober
                                        if (kal_okt >= 0)
                                        {
                                            string update_oktober = "UPDATE MANFAAT SET ALOKOKT = 9 WHERE ALOKOKT = 0 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
                                        ctx.Database.ExecuteSqlCommand(update_oktober);

                                            var getAnggNov = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM(ANGGNOV),0) FROM MANFAAT WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKNOV = 0").FirstOrDefault();
                                            var kal_nov = (kal_okt - getAnggNov);

                                            //cek kalkulasi november
                                            if (kal_nov >= 0)
                                            {
                                                string update_november = "UPDATE MANFAAT SET ALOKNOV = 9 WHERE ALOKNOV = 0 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
                                            ctx.Database.ExecuteSqlCommand(update_november);

                                                var getAnggDes = ctx.Database.SqlQuery<Decimal>("SELECT NVL(SUM(ANGGDES),0) FROM MANFAAT WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKDES = 0").FirstOrDefault();
                                                var kal_des = (kal_okt - getAnggNov);

                                                //cek kalkulasi desember
                                                if (kal_des >= 0)
                                                {
                                                    string update_desember = "UPDATE MANFAAT SET ALOKDES = 9 WHERE ALOKDES = 0 AND TIPE = 'NONOPS' AND TAHUN =" + currentYear + "";
                                                ctx.Database.ExecuteSqlCommand(update_desember);
                                                }
                                                else
                                                {
                                                    //else kalkulasi desember
                                                    var getstatusalokasi = ctx.Database.SqlQuery<Entities.getAlokasi>("SELECT * FROM MANFAAT WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKDES = 0 ORDER BY RANKDES ASC").ToList();
                                                    foreach (var dadad in getstatusalokasi)
                                                    {

                                                        //var anggjun = dadad.AnggJun;

                                                        var hasilkurang = kal_nov - dadad.AnggDes;
                                                        //return Json(hasilkurang, JsonRequestBehavior.AllowGet);

                                                        if (hasilkurang > 0)
                                                        {
                                                            var update_sisaalokdes = "UPDATE MANFAAT SET ALOKDES = 9 WHERE MANFAATID = '" + dadad.ManfaatId + "'";
                                                            ctx.Database.ExecuteSqlCommand(update_sisaalokdes);
                                                        }
                                                        kal_nov = (decimal)hasilkurang;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //else kalkulasi november
                                                var getstatusalokasi = ctx.Database.SqlQuery<Entities.getAlokasi>("SELECT * FROM MANFAAT WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKNOV = 0 ORDER BY RANKNOV ASC").ToList();
                                                foreach (var dadad in getstatusalokasi)
                                                {

                                                    //var anggjun = dadad.AnggJun;

                                                    var hasilkurang = kal_okt - dadad.AnggNov;
                                                    //return Json(hasilkurang, JsonRequestBehavior.AllowGet);

                                                    if (hasilkurang > 0)
                                                    {
                                                        var update_sisaaloknov = "UPDATE MANFAAT SET ALOKNOV = 9 WHERE MANFAATID = '" + dadad.ManfaatId + "'";
                                                        ctx.Database.ExecuteSqlCommand(update_sisaaloknov);
                                                    }
                                                        kal_okt = (decimal)hasilkurang;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //else kalkulasi oktober
                                            var getstatusalokasi = ctx.Database.SqlQuery<Entities.getAlokasi>("SELECT * FROM MANFAAT WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKOKT = 0 ORDER BY RANKOKT ASC").ToList();
                                            foreach (var dadad in getstatusalokasi)
                                            {

                                                //var anggjun = dadad.AnggJun;

                                                var hasilkurang = kal_sep - dadad.AnggOkt;
                                                //return Json(hasilkurang, JsonRequestBehavior.AllowGet);

                                                if (hasilkurang > 0)
                                                {
                                                    var update_sisaalokokt = "UPDATE MANFAAT SET ALOKOKT = 9 WHERE MANFAATID = '" + dadad.ManfaatId + "'";
                                                    ctx.Database.ExecuteSqlCommand(update_sisaalokokt);
                                                }
                                                kal_sep = (decimal)hasilkurang;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //else kalkulasi sepetember
                                        var getstatusalokasi = ctx.Database.SqlQuery<Entities.getAlokasi>("SELECT * FROM MANFAAT WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKSEP = 0 ORDER BY RANKSEP ASC").ToList();
                                        foreach (var dadad in getstatusalokasi)
                                        {

                                            //var anggjun = dadad.AnggJun;

                                            var hasilkurang = kal_agt - dadad.AnggSep;
                                            //return Json(hasilkurang, JsonRequestBehavior.AllowGet);

                                            if (hasilkurang > 0)
                                            {
                                                var update_sisaaloksep = "UPDATE MANFAAT SET ALOKSEP = 9 WHERE MANFAATID = '" + dadad.ManfaatId + "'";
                                                ctx.Database.ExecuteSqlCommand(update_sisaaloksep);
                                            }
                                            kal_agt = (decimal)hasilkurang;
                                        }
                                    }
                                }
                                else
                                {
                                    //else kalkulasi agustus
                                    var getstatusalokasi = ctx.Database.SqlQuery<Entities.getAlokasi>("SELECT * FROM MANFAAT WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKAGT = 0 ORDER BY RANKAGT ASC").ToList();
                                    foreach (var dadad in getstatusalokasi)
                                    {

                                        //var anggjun = dadad.AnggJun;

                                        var hasilkurang = kal_Jul - dadad.AnggAgt;
                                        //return Json(hasilkurang, JsonRequestBehavior.AllowGet);

                                        if (hasilkurang > 0)
                                        {
                                            var update_sisaalokagt = "UPDATE MANFAAT SET ALOKAGT = 9 WHERE MANFAATID = '" + dadad.ManfaatId + "'";
                                            ctx.Database.ExecuteSqlCommand(update_sisaalokagt);
                                        }
                                        kal_Jul = (decimal)hasilkurang;
                                    }
                                }
                            }
                            else
                            {
                                //else kalkulasi juli
                                var getstatusalokasi = ctx.Database.SqlQuery<Entities.getAlokasi>("SELECT * FROM MANFAAT WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKJUL = 0 ORDER BY RANKJUL ASC").ToList();
                                foreach (var dadad in getstatusalokasi)
                                {

                                    //var anggjun = dadad.AnggJun;

                                    var hasilkurang = kal_jun - dadad.AnggJul;
                                    //return Json(hasilkurang, JsonRequestBehavior.AllowGet);

                                    if (hasilkurang > 0)
                                    {
                                        var update_sisaalokjul = "UPDATE MANFAAT SET ALOKJUL = 9 WHERE MANFAATID = '" + dadad.ManfaatId + "'";
                                        ctx.Database.ExecuteSqlCommand(update_sisaalokjul);
                                    }
                                    kal_jun = (decimal)hasilkurang;
                                }
                            }
                        }
                        else
                        {
                            //else kalkulasi juni
                            var getstatusalokasi = ctx.Database.SqlQuery<Entities.getAlokasi>("SELECT * FROM MANFAAT WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKJUN = 0 ORDER BY RANKJUN ASC").ToList();
                            foreach (var dadad in getstatusalokasi)
                            {

                            //var anggjun = dadad.AnggJun;

                            var hasilkurang = kal_mei - dadad.AnggJun;
                            //return Json(hasilkurang, JsonRequestBehavior.AllowGet);

                            if (hasilkurang > 0)
                                {
                                var update_sisaalokjun = "UPDATE MANFAAT SET ALOKJUN = 9 WHERE MANFAATID = '" + dadad.ManfaatId + "'";                                
                                ctx.Database.ExecuteSqlCommand(update_sisaalokjun);
                                }
                                kal_mei = (decimal)hasilkurang;
                            }
                        }
                    }
                    else
                    {
                        //else kalkulasi mei
                        var getstatusalokasi = ctx.Database.SqlQuery<Entities.getAlokasi>("SELECT * FROM MANFAAT WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKMEI = 0 ORDER BY RANKMEI ASC").ToList();
                        foreach (var dadad in getstatusalokasi)
                        {

                            //var anggjun = dadad.AnggJun;

                            var hasilkurang = kal_apr - dadad.AnggMei;
                            //return Json(hasilkurang, JsonRequestBehavior.AllowGet);

                            if (hasilkurang > 0)
                            {
                                var update_sisaalokmei = "UPDATE MANFAAT SET ALOKMEI = 9 WHERE MANFAATID = '" + dadad.ManfaatId + "'";
                                ctx.Database.ExecuteSqlCommand(update_sisaalokmei);
                            }
                            kal_apr = (decimal)hasilkurang;
                        }
                    }

                }

            var sukses = '1';

            //var getalokasi = ctx.Database.SqlQuery<Entities.getManfaatAlokasi>("SELECT DISTINCT KANTORID, NVL(SUM (ANGGJAN + ANGGFEB + ANGGMAR + ANGGAPR + ANGGMEI + ANGGJUN + ANGGJUL + ANGGAGT + ANGGSEP + ANGGNOV + ANGGDES),0) AS NILAIALOKASI FROM MANFAAT WHERE TIPE = 'NONOPS' AND KANTORID ='" + kodesatker + "' AND TAHUN = '" + currentYear + "' GROUP BY KANTORID").ToList();
            return Json(sukses, JsonRequestBehavior.AllowGet);
        }
        
        public class adadad
        {
            string MANFAATID { get; set; }
	        int RANKJUN { get; set; }
            int ANGGJUN { get; set; }
	        int ALOKJUN { get; set; }
        }
        //public ActionResult get_alokasi(string kodesatker)
        //{
        //    var ctx = new PnbpContext();

        //    string update_jan = "UPDATE MANFAAT SET ALOKJAN = 9 WHERE ANGGJAN IS NOT NULL OR ANGGJAN = 0";
        //    ctx.Database.ExecuteSqlCommand(update_jan);

        //    string update_feb = "UPDATE MANFAAT SET ALOKFEB = 9 WHERE ALOKFEB IS NOT NULL OR ALOKFEB = 0";
        //    ctx.Database.ExecuteSqlCommand(update_feb);

        //    var done = '1';

        //    return Json(done, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult getDataSatkerEselon(string persentase)
        {
            var ctx = new PnbpContext();
            string query =
                @"
                    SELECT
                        a.ID,
	                    b.KODESATKER,
	                    b.NAMA_SATKER,
	                    a.PAGU,
	                    a.TOTALALOKASI,
	                    a.REALISASI,
	                    a.MP,
	                    a.ALOKASI,
                        a.APPROVE 
                    FROM
	                    ALOKASISATKERNONOPS a 
	                    LEFT JOIN SATKER b ON a.KANTORID = b.KANTORID
                    WHERE a.APPROVE = 'on'";
            var get_data = ctx.Database.SqlQuery<Entities.getDataSatkerEselon>(query).ToList();
            return Json(get_data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getDetailAnggaran(string kantorid)
        {
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();
            var nilaialokasi = "";
            if (currentMonth == "1")
            {
                nilaialokasi = "ANGGJAN";
            }
            else if (currentMonth == "2")
            {
                nilaialokasi = "ANGGFEB";
            }
            else if (currentMonth == "3")
            {
                nilaialokasi = "ANGGMAR";
            }
            else if (currentMonth == "4")
            {
                nilaialokasi = "ANGGAPR";
            }
            else if (currentMonth == "5")
            {
                nilaialokasi = "ANGGMEI";
            }
            else if (currentMonth == "6")
            {
                nilaialokasi = "ANGGJUN";
            }
            else if (currentMonth == "7")
            {
                nilaialokasi = "ANGGJUL";
            }
            else if (currentMonth == "8")
            {
                nilaialokasi = "ANGGAGT";
            }
            else if (currentMonth == "9")
            {
                nilaialokasi = "ANGGSEP";
            }
            else if (currentMonth == "10")
            {
                nilaialokasi = "ANGGOKT";
            }
            else if (currentMonth == "11")
            {
                nilaialokasi = "ANGGNOV";
            }
            else if (currentMonth == "12")
            {
                nilaialokasi = "ANGGDES";
            }

            //return Json(nilaialokasi, JsonRequestBehavior.AllowGet);


            var ctx = new PnbpContext();
            var get_detailanggaran = ctx.Database.SqlQuery<Entities.getDetailAnggaran>("SELECT DISTINCT KODE, NAMAPROGRAM, NVL(sum(" + nilaialokasi + "), 0) AS NILAIALOKASI FROM MANFAAT WHERE TIPE = 'NONOPS' AND KANTORID ='" + kantorid + "' AND TAHUN = '" + currentYear + "' GROUP BY KANTORID").ToList();
            return Json(get_detailanggaran, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getDataSatkerCustome(string kantorid)
        {
            //return Json(kantorid, JsonRequestBehavior.AllowGet);
            var ctx = new PnbpContext();
            var get_data_custom = ctx.Database.SqlQuery<Entities.getDataSatker>("WITH aa AS ( SELECT DISTINCT A.kantorid, b.KODESATKER, b.NAMA_SATKER, SUM(A.NILAIANGGARAN) AS NILAIANGGARAN, NVL(SUM(A.TOTALALOKASI), 0) AS TOTALALOKASI, SUM(A.NILAIALOKASI) AS NILAIALOKASI, NVL(SUM(A.SISAALOKASI), 0) AS SISAALOKASI, ROUND( SUM(NILAIALOKASI / NILAIANGGARAN), 2 ) mp FROM MANFAAT A " +
               "LEFT JOIN satker b ON A.kantorid = b.kantorid WHERE TIPE = 'NONOPS' GROUP BY A.kantorid, b.KODESATKER, b.NAMA_SATKER ) SELECT kantorid, KODESATKER, NAMA_SATKER, NILAIANGGARAN, TOTALALOKASI, NILAIALOKASI, SISAALOKASI, MP FROM aa WHERE kantorid = '" + kantorid + "' GROUP BY kantorid, KODESATKER, NAMA_SATKER, NILAIANGGARAN, TOTALALOKASI, NILAIALOKASI, SISAALOKASI, mp").ToList();
            return Json(get_data_custom, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public ActionResult UpdateDataSatker(FormCollection form, string[] check, string[] id)
        {

            var ctx = new PnbpContext();
            //var id = NewGuID();
            var kodesatker = ((form.AllKeys.Contains("kodesatker")) ? form["kodesatker"] : "NULL");
            var nilaianggaran = ((form.AllKeys.Contains("nilaianggaran")) ? form["nilaianggaran"] : "NULL");
            var totalalokasi = ((form.AllKeys.Contains("totalalokasi")) ? form["totalalokasi"] : "NULL");
            var nilaialokasi = ((form.AllKeys.Contains("nilaialokasi")) ? form["nilaialokasi"] : "NULL");
            var mp = ((form.AllKeys.Contains("mp")) ? form["mp"] : "NULL");
            var sisaalokasi = ((form.AllKeys.Contains("sisaalokasi")) ? form["sisaalokasi"] : "NULL");
            var checklist = ((form.AllKeys.Contains("check")) ? form["check"] : "NULL");
            var iddata = ((form.AllKeys.Contains("id")) ? form["id"] : "NULL");

            string[] kodesatkersplit = kodesatker.TrimStart(',').Split(',');
            string[] nilaianggaransplit = nilaianggaran.TrimStart(',').Split(',');
            string[] totalalokasisplit = totalalokasi.TrimStart(',').Split(',');
            string[] nilaialokasisplit = nilaialokasi.TrimStart(',').Split(',');
            string[] mpsplit = mp.TrimStart(',').Split(',');
            string[] sisaalokasisplit = sisaalokasi.TrimStart(',').Split(',');
            string[] cheklistsplit = checklist.TrimStart(',').Split(',');
            string[] idsplit = iddata.TrimStart(',').Split(',');
            //return Json(id, JsonRequestBehavior.AllowGet);



            for (int i = 0; i < kodesatkersplit.Count(); i++)
            {
                //string insert = "INSERT INTO ALOKASISATKERNONOPS (ID, KANTORID, PAGU, TOTALALOKASI, REALISASI, MP, ALOKASI, APPROVE)" +
                //    "VALUES('" + id + "', '" + kodesatkersplit[i] + "', " + nilaianggaransplit[i] + ", " + totalalokasisplit[i] + ", " + nilaialokasisplit[i] + ", " + mpsplit[i] + "," + sisaalokasisplit[i] + ",'" + cheklistsplit[i] + "')";
                string update = "UPDATE ALOKASISATKERNONOPS SET APPROVE = '" + check[i] + "' WHERE ID = '" + id[i] + "'";
                //return Json(update, JsonRequestBehavior.AllowGet);
                ctx.Database.ExecuteSqlCommand(update);
            }

            return Json(1, JsonRequestBehavior.AllowGet);
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
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
            dt.Columns.AddRange(new DataColumn[10] {
                new DataColumn("No",typeof(int)),
                new DataColumn("Tahun"),
                new DataColumn("Kode"),
                new DataColumn("Kode_Satker"),
                new DataColumn("Nama_Kantor"),
                new DataColumn("Nama_Program"),
                new DataColumn("Prioritas_Kegiatan"),
                new DataColumn("Nilai_Anggaran",typeof(decimal)),
                new DataColumn("Sudah_Alokasi",typeof(decimal)),
                new DataColumn("Nilai_Alokasi",typeof(decimal)) });

            List<Entities.AlokasiRows> result = _AlokasiModel.DaftarAlokasiOPS("OPS");
            foreach (var rw in result)
            {
                dt.Rows.Add(rw.Rnumber, rw.Tahun, rw.Kode, rw.Kodesatker, rw.NamaKantor, rw.NamaProgram, rw.PrioritasKegiatan, rw.NilaiAnggaran, rw.SudahAlokasi, rw.NilaiAlokasi);
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
            dt.Columns.AddRange(new DataColumn[43] {
                new DataColumn("No",typeof(int)),
                new DataColumn("Manfaat_Id"),
                new DataColumn("Tahun"),
                new DataColumn("Kode_Satker"),
                new DataColumn("Nama_Kantor"),
                new DataColumn("Nama_Program"),
                new DataColumn("Nilai_Anggaran",typeof(decimal)),
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
                    rw.Rnumber, rw.ManfaatId, rw.Tahun, rw.Kodesatker, rw.NamaKantor, rw.NamaProgram, rw.NilaiAnggaran,
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

        public ActionResult RincianAlokasi()
        {
            return View();
        }

        public ActionResult DetailRincianAlokasi()
        {
            return View();
        }

        public ActionResult InsertDataSatker(string InputAlokasi)
        {
            //return Json(InputAlokasi, JsonRequestBehavior.AllowGet);
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();

            var ctx = new PnbpContext();

            // update alok januari
            string update_alokjan = "UPDATE MANFAAT SET ALOKJAN = 1 WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKJAN = 9 AND STATUSAKTIF = 1";
            ctx.Database.ExecuteSqlCommand(update_alokjan);

            // update alok februari
            string update_alokfeb = "UPDATE MANFAAT SET ALOKFEB = 1 WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKFEB = 9 AND STATUSAKTIF = 1";
            ctx.Database.ExecuteSqlCommand(update_alokfeb);

            // update alok maret
            string update_alokmar = "UPDATE MANFAAT SET ALOKMAR = 1 WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKMAR = 9 AND STATUSAKTIF = 1";
            ctx.Database.ExecuteSqlCommand(update_alokmar);

            // update alok april
            string update_alokapr = "UPDATE MANFAAT SET ALOKAPR = 1 WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKAPR = 9 AND STATUSAKTIF = 1";
            ctx.Database.ExecuteSqlCommand(update_alokapr);

            // update alok mei
            string update_alokmei = "UPDATE MANFAAT SET ALOKMEI = 1 WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKMEI = 9 AND STATUSAKTIF = 1";
            ctx.Database.ExecuteSqlCommand(update_alokmei);

            // update alok juni
            string update_alokjun = "UPDATE MANFAAT SET ALOKJUN= 1 WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKJUN = 9 AND STATUSAKTIF = 1";
            ctx.Database.ExecuteSqlCommand(update_alokjun);

            // update alok juli
            string update_alokjul = "UPDATE MANFAAT SET ALOKJUL= 1 WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKJUL = 9 AND STATUSAKTIF = 1";
            ctx.Database.ExecuteSqlCommand(update_alokjul);

            // update alok agustus
            string update_alokagt = "UPDATE MANFAAT SET ALOKAGT= 1 WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKAGT = 9 AND STATUSAKTIF = 1";
            ctx.Database.ExecuteSqlCommand(update_alokagt);

            // update alok september
            string update_aloksep = "UPDATE MANFAAT SET ALOKSEP= 1 WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKSEP = 9 AND STATUSAKTIF = 1";
            ctx.Database.ExecuteSqlCommand(update_aloksep);

            // update alok oktober
            string update_alokokt = "UPDATE MANFAAT SET ALOKOKT= 1 WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKOKT = 9 AND STATUSAKTIF = 1";
            ctx.Database.ExecuteSqlCommand(update_alokokt);

            // update alok november
            string update_aloknov = "UPDATE MANFAAT SET ALOKNOV= 1 WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKNOV = 9 AND STATUSAKTIF = 1";
            ctx.Database.ExecuteSqlCommand(update_aloknov);

            // update alok desember
            string update_alokdes = "UPDATE MANFAAT SET ALOKAGT= 1 WHERE TAHUN = " + currentYear + " AND TIPE = 'NONOPS' AND ALOKDES = 9 AND STATUSAKTIF = 1";
            ctx.Database.ExecuteSqlCommand(update_alokdes);

            var REKAPALOKASIID = NewGuID();

            string insert_rekapalokasi = "INSERT INTO REKAPALOKASI ( REKAPALOKASIID, TANGGALALOKASI, TIPEMANFAAT, DAPATALOKASI, INPUTALOKASI, TERALOKASI, STATUSALOKASI )" +
            //"VALUES ('" + REKAPALOKASIID + "', sysdate, 'NONOPS', " + get_data[0] + ", " + InputAlokasi + ", " + InputAlokasi + ", 1)";
            "VALUES ('" + REKAPALOKASIID + "', sysdate, 'NONOPS', " + InputAlokasi + ", " + InputAlokasi + ", " + InputAlokasi + ", 1)";
            ctx.Database.ExecuteSqlCommand(insert_rekapalokasi);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReadAlokasi()
        {
            var fileContent = Request.Files["FileUpload"];

            List<TempAlokasi> result = new List<TempAlokasi>();
            var isSuccess = true;
            var message = "";

            if (fileContent.ContentLength > 0)
            {
                var stream = fileContent.InputStream;
                stream.Position = 0;

                PnbpContext db = new PnbpContext();
                var trx = db.Database.BeginTransaction();
                try
                {
                    XSSFWorkbook xssWorkbook = new XSSFWorkbook(stream);
                    var sheet = xssWorkbook.GetSheetAt(0);


                    var tempCount = db.Database.SqlQuery<int>("select count(*) from temp_alokasi").FirstOrDefault();
                    if (tempCount > 0)
                    {
                        var deleteCount = db.Database.ExecuteSqlCommand("delete from temp_alokasi");
                        if (deleteCount <= 0)
                        {
                            trx.Rollback();
                            return Json(new { success = false, message = "Proses Import Alokasi bermasalah, silakan hubungi Administrator." }, JsonRequestBehavior.DenyGet);
                        }
                    }

                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow bodyRow = sheet.GetRow(i);
                        var cKodeSatker = bodyRow.GetCell(1);
                        var cPagu = bodyRow.GetCell(2);
                        var cAlokasi = bodyRow.GetCell(3);

                        var valueKodeSatker = cKodeSatker.CellType == CellType.Numeric ? cKodeSatker.NumericCellValue.ToString() : cKodeSatker.StringCellValue;
                        var valuePagu = cPagu.CellType == CellType.Numeric ? 
                            cPagu.NumericCellValue.ToString() : cPagu.StringCellValue;
                        var valueAlokasi = cAlokasi.CellType == CellType.Numeric ? 
                            cAlokasi.NumericCellValue.ToString() : 
                            (string.IsNullOrEmpty(cAlokasi.StringCellValue) ? "0" : cAlokasi.StringCellValue);

                        if (!string.IsNullOrEmpty(valueKodeSatker) &&
                            !string.IsNullOrEmpty(valuePagu) &&
                            !string.IsNullOrEmpty(valueAlokasi))
                        {
                            List<object> param = new List<object>();
                            param.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kdsatker", valueKodeSatker));
                            param.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pagu", Convert.ToDouble(valuePagu)));
                            param.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("alokasi", Convert.ToDouble(valueAlokasi)));
                            var query = "INSERT INTO TEMP_ALOKASI(kdsatker, pagu, alokasi, status) VALUES(:kdsatker, :pagu, :alokasi, 1)";

                            var row = db.Database.ExecuteSqlCommand(query, param.ToArray());
                            isSuccess = row > 0;
                        }
                        else
                        {
                            isSuccess = false;
                        }

                        if (!isSuccess)
                        {
                            break;
                        }
                    }

                    if (!isSuccess)
                    {
                        isSuccess = false;
                        message = "Proses Import Alokasi bermasalah, silakan hubungi Administrator.";
                        return Json(new { success = false, message = "Proses Import Alokasi bermasalah, silakan hubungi Administrator." }, JsonRequestBehavior.DenyGet);
                    } 
                    //else
                    //{
                    //    //var query = "select kdsatker, to_char(pagu), to_char(alokasi) from temp_alokasi";
                    //    //result = db.Database.SqlQuery<TempAlokasi>(query).ToList();
                    //}
                }
                catch(Exception e)
                {
                    _ = e.StackTrace;
                    trx.Rollback();
                    isSuccess = false;
                    message = "Proses Import Alokasi bermasalah, silakan hubungi Administrator.";
                }
                finally
                {
                    if (isSuccess)
                    {
                        trx.Commit();
                    } 
                    else
                    {
                        trx.Rollback();
                    }
                }

                if (isSuccess)
                {
                    return AlokasiSaatIni();
                }
            }
            else
            {
                isSuccess = false;
                message = "Proses Import Alokasi bermasalah, file yang anda upload tidak sesuai.";
            }
            return Json(new { success = isSuccess, data = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReadAlokasiRevisi()
        {
            var fileContent = Request.Files["FileUpload"];

            List<TempAlokasi> result = new List<TempAlokasi>();
            var isSuccess = true;
            var message = "";

            if (fileContent.ContentLength > 0)
            {
                var stream = fileContent.InputStream;
                stream.Position = 0;

                PnbpContext db = new PnbpContext();
                var trx = db.Database.BeginTransaction();
                try
                {
                    XSSFWorkbook xssWorkbook = new XSSFWorkbook(stream);
                    var sheet = xssWorkbook.GetSheetAt(0);


                    var tempCount = db.Database.SqlQuery<int>("select count(*) from temp_alokasi_revisi").FirstOrDefault();
                    if (tempCount > 0)
                    {
                        var deleteCount = db.Database.ExecuteSqlCommand("delete from temp_alokasi_revisi");
                        if (deleteCount <= 0)
                        {
                            trx.Rollback();
                            return Json(new { success = false, message = "Proses Import Alokasi bermasalah, silakan hubungi Administrator." }, JsonRequestBehavior.DenyGet);
                        }
                    }

                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow bodyRow = sheet.GetRow(i);
                        var cKodeSatker = bodyRow.GetCell(1);
                        var cPagu = bodyRow.GetCell(2);
                        var cAlokasi = bodyRow.GetCell(3);

                        var valueKodeSatker = cKodeSatker.CellType == CellType.Numeric ? cKodeSatker.NumericCellValue.ToString() : cKodeSatker.StringCellValue;
                        var valuePagu = cPagu.CellType == CellType.Numeric ?
                            cPagu.NumericCellValue.ToString() : cPagu.StringCellValue;
                        var valueAlokasi = cAlokasi.CellType == CellType.Numeric ?
                            cAlokasi.NumericCellValue.ToString() :
                            (string.IsNullOrEmpty(cAlokasi.StringCellValue) ? "0" : cAlokasi.StringCellValue);

                        if (!string.IsNullOrEmpty(valueKodeSatker) &&
                            !string.IsNullOrEmpty(valuePagu) &&
                            !string.IsNullOrEmpty(valueAlokasi))
                        {
                            List<object> param = new List<object>();
                            param.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kdsatker", valueKodeSatker));
                            param.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("pagu", Convert.ToDouble(valuePagu)));
                            param.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("alokasi", Convert.ToDouble(valueAlokasi)));
                            var query = "INSERT INTO temp_alokasi_revisi(kdsatker, pagu, alokasi, status) VALUES(:kdsatker, :pagu, :alokasi, 1)";

                            var row = db.Database.ExecuteSqlCommand(query, param.ToArray());
                            isSuccess = row > 0;
                        }
                        else
                        {
                            isSuccess = false;
                        }

                        if (!isSuccess)
                        {
                            break;
                        }
                    }

                    if (!isSuccess)
                    {
                        isSuccess = false;
                        message = "Proses Import Alokasi bermasalah, silakan hubungi Administrator.";
                        return Json(new { success = false, message = "Proses Import Alokasi bermasalah, silakan hubungi Administrator." }, JsonRequestBehavior.DenyGet);
                    }
                    //else
                    //{
                    //    //var query = "select kdsatker, to_char(pagu), to_char(alokasi) from temp_alokasi";
                    //    //result = db.Database.SqlQuery<TempAlokasi>(query).ToList();
                    //}
                }
                catch (Exception e)
                {
                    _ = e.StackTrace;
                    trx.Rollback();
                    isSuccess = false;
                    message = "Proses Import Alokasi bermasalah, silakan hubungi Administrator.";
                }
                finally
                {
                    if (isSuccess)
                    {
                        trx.Commit();
                    }
                    else
                    {
                        trx.Rollback();
                    }
                }

                if (isSuccess)
                {
                    bool isRevisi = true;
                    return AlokasiSaatIni(isRevisi);
                }
            }
            else
            {
                isSuccess = false;
                message = "Proses Import Alokasi bermasalah, file yang anda upload tidak sesuai.";
            }
            return Json(new { success = isSuccess, data = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IsKodeSpanValid()
        {

            PnbpContext db = new PnbpContext();
            var result = true;
            try
            {
                var query = @"
                select count(*) from (
                    WITH sb AS (
	                    SELECT (KEGIATAN || '.' || OUTPUT) AS kodeoutput
                        FROM SPAN_BELANJA sb
                        WHERE sb.SUMBER_DANA = 'D' AND
                        TAHUN = EXTRACT(YEAR FROM sysdate)

                        GROUP BY KEGIATAN, OUTPUT
                    ),
                    k AS(
                        SELECT* FROM KODESPAN k
                    )
                    SELECT sb.kodeoutput, k.KODEOUTPUT, p.KODE
                    FROM sb
                    LEFT JOIN k ON sb.kodeoutput = k.KODEOUTPUT
                    LEFT JOIN program p ON sb.KODEOUTPUT = p.KODE
                    WHERE k.KODEOUTPUT IS NULL
                )
                ";

                var dataCount = db.Database.SqlQuery<int>(query).FirstOrDefault();
                result = dataCount == 0;
            }
            catch(Exception e)
            {
                _ = e.StackTrace;
                result = false;
            }

            return Json(new { success = result, data = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ResetAlokasi()
        {
            PnbpContext db = new PnbpContext();
            var trx = db.Database.BeginTransaction();
            var isError = false;
            try
            {
                var query = "DELETE FROM TEMP_ALOKASI";
                db.Database.ExecuteSqlCommand(query);
            }
            catch(Exception e)
            {
                isError = true;
                _ = e.StackTrace;
            } 
            finally
            {
                if (!isError)
                {
                    trx.Commit();
                } 
                else
                {
                    trx.Rollback();
                }
            }
            return Json(new { success = !isError }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ResetAlokasiRevisi()
        {
            PnbpContext db = new PnbpContext();
            var trx = db.Database.BeginTransaction();
            var isError = false;
            try
            {
                var query = "DELETE FROM TEMP_ALOKASI_REVISI";
                db.Database.ExecuteSqlCommand(query);
            }
            catch (Exception e)
            {
                isError = true;
                _ = e.StackTrace;
            }
            finally
            {
                if (!isError)
                {
                    trx.Commit();
                }
                else
                {
                    trx.Rollback();
                }
            }
            return Json(new { success = !isError }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ProsesAlokasi()
        {
            var db = new PnbpContext();
            var trx = db.Database.BeginTransaction();
            List<DataProsesAlokasi> result = new List<DataProsesAlokasi>();
            var isProcessAlokasiSuccess = true;

            var idTrx = NewGuID();

            try
            {
                var user = (HttpContext.User.Identity as Entities.InternalUserIdentity);

                string query = @"
                SELECT * FROM (
	                select 
	                    s.KANTORID as kantorid, 
	                    k.TIPE AS tipe, 
	                    p.PROGRAMID as programid, 
	                    p.nama AS programnama,
	                    s.NAMA_SATKER as namasatker, 
	                    sb.KDSATKER as kodesatker, sb.kegiatan as kegiatan, sb.OUTPUT as output, sum(sb.amount) as amount
	                from span_belanja sb
	                left JOIN satker s ON sb.KDSATKER = s.KODESATKER and s.statusaktif = 1 
	                LEFT JOIN KODESPAN k ON sb.KEGIATAN  = k.KODE AND sb.OUTPUT = k.KEGIATAN 
	                LEFT JOIN PROGRAM p ON p.KODE = k.KODEOUTPUT AND p.STATUSAKTIF = 1 AND p.TIPEOPS = k.TIPE
	                WHERE
	                    sb.KDSATKER = '022209' AND
	                    sb.KDSATKER != '524465'  AND sb.SUMBER_DANA = 'D'
	                GROUP BY
	                    s.KANTORID, 
	                    k.tipe,
	                    s.NAMA_SATKER, 
	                    sb.kdsatker, 
	                    sb.kegiatan, 
	                    sb.OUTPUT, 
	                    p.PROGRAMID,
	                    p.NAMA
	                ORDER BY sb.kdsatker
                )
                WHERE programid IS NOT NULL
                ";

                result = db.Database.SqlQuery<DataProsesAlokasi>(query).ToList();
                bool successInsert = true;
                foreach(var item in result)
                {
                    // insert data ke manfaat
                    var queryInsert = BuildQuery(
                        item.KantorId, 
                        item.NamaSatker, 
                        item.ProgramId, 
                        item.ProgramNama, 
                        item.Tipe, 
                        item.Amount,
                        $"{item.Kegiatan}.{item.Output}",
                        item.KodeSatker);
                    if (string.IsNullOrEmpty(queryInsert))
                    {
                        successInsert = false;
                        break;
                    }

                    var row = db.Database.ExecuteSqlCommand(queryInsert);
                    if (row <= 0)
                    {
                        successInsert = false;
                        break;
                    }
                }

                if (successInsert)
                {
                    var queryLastAlokasiSummary = @"SELECT 
                            ALOKASISATKERSUMMARYID, 
                            PAGU, 
                            ALOKASI, 
                            TO_CHAR(TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                            TO_CHAR(TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH,
                            MP 
                        FROM ALOKASISATKERSUMMARY a 
                        WHERE tahun = extract(year from sysdate) AND MP = (SELECT * FROM (
                            SELECT a2.mp FROM ALOKASISATKERSUMMARY a2 WHERE a2.tahun = extract(year from sysdate) ORDER BY a2.MP DESC
                        ) WHERE rownum = 1)";

                    var getLastAlokasiSummary = db.Database.SqlQuery<AlokasiSatkerSummary>(queryLastAlokasiSummary).FirstOrDefault();
                    var mp = getLastAlokasiSummary == null ? 1 : (getLastAlokasiSummary.Mp + 1);

                    var isSuccessProccessMove = true;
                    var queryAlokasiSatkerSummary = $@"
                        INSERT INTO ALOKASISATKERSUMMARY(ALOKASISATKERSUMMARYID, PAGU, ALOKASI, MP)
                        SELECT '{idTrx}', sum(pagu), sum(alokasi), {mp} ALOKASI FROM TEMP_ALOKASI
                        ";
                    var rowSummary = db.Database.ExecuteSqlCommand(queryAlokasiSatkerSummary);
                    if (rowSummary <= 0)
                    {
                        isSuccessProccessMove = false;
                    }
                    else
                    {
                        var queryMoveAlokasi = $@"
                            INSERT INTO ALOKASISATKER(ALOKASISATKERID, KDSATKER, PAGU, ALOKASI, ALOKASISATKERSUMMARYID)
                            SELECT sys_guid(), KDSATKER, PAGU, ALOKASI, '{idTrx}' FROM TEMP_ALOKASI
                            ";
                        var rowMoveAlokasi = db.Database.ExecuteSqlCommand(queryMoveAlokasi);
                        if (rowMoveAlokasi <= 0)
                        {
                            isSuccessProccessMove = false;
                        }
                    }

                    if (isSuccessProccessMove)
                    {
                        // clear
                        var queryDelete = @"delete from TEMP_ALOKASI";
                        var rowDelete = db.Database.ExecuteSqlCommand(queryDelete);
                        if (rowDelete <= 0)
                        {
                            isProcessAlokasiSuccess = false;
                            trx.Rollback();
                        }
                        else
                        {
                            trx.Commit();
                        }
                    } 
                    else
                    {
                        isProcessAlokasiSuccess = false;
                        trx.Rollback();
                    }
                } 
                else
                {
                    isProcessAlokasiSuccess = false;
                    trx.Rollback();
                }
            }
            catch(Exception e)
            {
                isProcessAlokasiSuccess = false;
                _ = e.StackTrace;
                trx.Rollback();
            }
            
            return Json(new { success = isProcessAlokasiSuccess }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ProsesRevisiAlokasi()
        {
            var db = new PnbpContext();
            var trx = db.Database.BeginTransaction();
            List<DataProsesAlokasi> result = new List<DataProsesAlokasi>();
            var isProcessAlokasiSuccess = true;

            var idTrx = NewGuID();

            try
            {
                var user = (HttpContext.User.Identity as Entities.InternalUserIdentity);
                var successInsert = true;
                if (successInsert)
                {
                    var queryLastAlokasiSummary = @"SELECT 
                            ALOKASISATKERSUMMARYID, 
                            PAGU, 
                            ALOKASI, 
                            TO_CHAR(TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                            TO_CHAR(TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH,
                            MP,
                            REVISI
                        FROM ALOKASISATKERSUMMARY a 
                        WHERE tahun = extract(year from sysdate) AND MP = (SELECT * FROM (
                            SELECT a2.mp FROM ALOKASISATKERSUMMARY a2 WHERE a2.tahun = extract(year from sysdate) ORDER BY a2.MP DESC
                        ) WHERE rownum = 1)";

                    var getLastAlokasiSummary = db.Database.SqlQuery<AlokasiSatkerSummary>(queryLastAlokasiSummary).FirstOrDefault();
                    if (getLastAlokasiSummary != null)
                    {
                        var mp = getLastAlokasiSummary.Mp;
                        var revisi = getLastAlokasiSummary.Revisi + 1;

                        var isSuccessProccessMove = true;
                        var queryAlokasiSatkerSummary = $@"
                        INSERT INTO ALOKASISATKERSUMMARY(ALOKASISATKERSUMMARYID, PAGU, ALOKASI, MP, REVISI)
                        SELECT '{idTrx}', sum(pagu), sum(alokasi), {mp}, {revisi} ALOKASI FROM TEMP_ALOKASI_REVISI
                        ";
                        var rowSummary = db.Database.ExecuteSqlCommand(queryAlokasiSatkerSummary);
                        if (rowSummary <= 0)
                        {
                            isSuccessProccessMove = false;
                        }
                        else
                        {
                            var queryMoveAlokasi = $@"
                            INSERT INTO ALOKASISATKER(ALOKASISATKERID, KDSATKER, PAGU, ALOKASI, ALOKASISATKERSUMMARYID)
                            SELECT sys_guid(), KDSATKER, PAGU, ALOKASI, '{idTrx}' FROM TEMP_ALOKASI_REVISI
                            ";
                            var rowMoveAlokasi = db.Database.ExecuteSqlCommand(queryMoveAlokasi);
                            if (rowMoveAlokasi <= 0)
                            {
                                isSuccessProccessMove = false;
                            }
                        }

                        if (isSuccessProccessMove)
                        {
                            // clear
                            var queryDelete = @"delete from TEMP_ALOKASI_REVISI";
                            var rowDelete = db.Database.ExecuteSqlCommand(queryDelete);
                            if (rowDelete <= 0)
                            {
                                isProcessAlokasiSuccess = false;
                                trx.Rollback();
                            }
                            else
                            {
                                trx.Commit();
                            }
                        }
                        else
                        {
                            isProcessAlokasiSuccess = false;
                            trx.Rollback();
                        }
                    } 
                    else
                    {
                        isProcessAlokasiSuccess = false;
                        trx.Rollback();
                    }
                }
                else
                {
                    isProcessAlokasiSuccess = false;
                    trx.Rollback();
                }
            }
            catch (Exception e)
            {
                isProcessAlokasiSuccess = false;
                _ = e.StackTrace;
                trx.Rollback();
            }

            return Json(new { success = isProcessAlokasiSuccess }, JsonRequestBehavior.AllowGet);
        }

        private string BuildQuery(string kantorId, string namaKantor, string programId, 
            string namaProgram, string tipe, double nilaiAnggaran, string kodeoutput, string kodesatker)
        {
            string query = "";
            try
            {
                query = $@"
                INSERT INTO PNBP.MANFAAT
                (
	                MANFAATID, 
	                TAHUN, 
	                KANTORID, 
	                NAMAKANTOR, 
	                PROGRAMID, 
	                NAMAPROGRAM, 
	                TIPE, 
	                NILAIANGGARAN,
                    KODE,
                    KODESATKER
                )
                VALUES(
                    sys_guid() , 
                    (extract(year from sysdate)), 
                    '{kantorId}', 
                    '{namaKantor}', 
                    '{programId}', 
                    '{namaProgram}', 
                    '{tipe}', 
                    {nilaiAnggaran},
                    '{kodeoutput}',
                    '{kodesatker}'
                )
            ";
            } 
            catch(Exception e)
            {
                query = "";
                _ = e.StackTrace;
            }

            return query;
        }

        public ActionResult ImportAlokasi()
        {
            return View();
        }

        public ActionResult ImportAlokasiSummaryDetail(string id)
        {
            return View();
        }

        public ActionResult GetAlokasiBySummaryId(string id)
        {
            List<AlokasiSatker> result = new List<AlokasiSatker>();
            AlokasiSatkerDetail response = new AlokasiSatkerDetail();
            var db = new PnbpContext();
            try
            {
                string query = $@"SELECT 
                        a.ALOKASISATKERID,
                        a.KDSATKER as KodeSatker,
                        s.NAMA_SATKER as NamaSatker, 
                        a.PAGU,
                        a.ALOKASI,
                        a.TAHUN,
                        TO_CHAR(a.TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                        TO_CHAR(a.TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH 
                    FROM ALOKASISATKER a 
                    JOIN SATKER s ON a.KDSATKER = s.KODESATKER and s.statusaktif = 1 
                    WHERE ALOKASISATKERSUMMARYID = '{id}'";
                result = db.Database.SqlQuery<AlokasiSatker>(query).ToList();

                string queryGetRevisi = $@"
                    SELECT ass.REVISI, a.kdsatker as KodeSatker, a.pagu, a.alokasi
                    FROM alokasisatker a
                    JOIN ALOKASISATKERSUMMARY ass ON a.ALOKASISATKERSUMMARYID = ass.ALOKASISATKERSUMMARYID 
                    WHERE a.ALOKASISATKERSUMMARYID IN (
	                    SELECT ALOKASISATKERSUMMARYID  
	                    FROM alokasisatkersummary
	                    WHERE tahun = EXTRACT (YEAR FROM sysdate)
                        AND mp = (select mp from ALOKASISATKERSUMMARY where alokasisatkersummaryid = '{id}') 
	                    AND revisi > 0
                    )
                    ORDER BY kdsatker, revisi asc
                ";
                var listRevisi = db.Database.SqlQuery<AlokasiSatker>(queryGetRevisi).ToList();

                string queryTempRevisi = $@"
                    SELECT 
                        (row_number() OVER (ORDER BY s.KODESATKER)) no,
                        s.KODESATKER AS kodesatker, 
                        s.NAMA_SATKER AS NamaSatker, 
                        to_char(ta.PAGU) AS pagu, 
                        to_char(ta.alokasi) AS alokasi 
                    FROM TEMP_ALOKASI_REVISI ta
                    LEFT JOIN satker s  ON ta.KDSATKER = s.KODESATKER  and s.statusaktif = 1 
                    WHERE ta.KDSATKER != '524465'
                ";
                var listTempRevisi = db.Database.SqlQuery<TempAlokasi>(queryTempRevisi).ToList();

                if (listRevisi.Count != 0 || listTempRevisi.Count > 0)
                {
                    bool addRevisi = listRevisi.Count > 0;
                    bool addTemp = listTempRevisi.Count > 0;
                    int index = 0;

                    var totalRevisi = listRevisi
                        .GroupBy(y => y.Revisi)
                        .Select(cl => new AlokasiSatkerRevisi {
                            Alokasi = cl.Sum(y => y.Alokasi),
                            Revisi = cl.Select(y => y.Revisi).FirstOrDefault()
                        }).ToList();

                    if (totalRevisi == null)
                    {
                        totalRevisi = new List<AlokasiSatkerRevisi>();
                    }

                    totalRevisi.Add(new AlokasiSatkerRevisi
                    {
                        Revisi = 0,
                        Alokasi = result.Sum(y => y.Pagu)
                    });

                    if (result != null && result.Count > 0)
                    {
                        var totalAlokasiSaatIni = result.Sum(y => y.Alokasi);
                        var alokasiSaatIni = new AlokasiSatkerRevisi
                        {
                            Revisi = (totalRevisi.Count + 1),
                            Alokasi = totalAlokasiSaatIni
                        };
                        totalRevisi.Add(alokasiSaatIni);
                    }

                    if (listTempRevisi != null && listTempRevisi.Count > 0)
                    {
                        var ltr = listTempRevisi
                        .Sum(y => Decimal.Parse(y.Alokasi));

                        var tempRevisi = new AlokasiSatkerRevisi
                        {
                            Revisi = (totalRevisi.Count + 1),
                            Alokasi = ltr
                        };

                        totalRevisi.Add(tempRevisi);
                    }

                    response.Total = totalRevisi;

                    foreach (var item in result)
                    {
                        decimal beforeValue = item.Alokasi;
                        if (addRevisi)
                        {
                            var data = new List<AlokasiSatkerRevisi>();
                            var lRevisi = listRevisi.FindAll(x => x.KodeSatker == item.KodeSatker).ToList();
                            
                            var lastRevisi = lRevisi.OrderByDescending(x => x.Revisi).FirstOrDefault();
                            if (lastRevisi != null)
                            {
                                beforeValue = lastRevisi.Alokasi;
                            }

                            foreach (var itemRevisi in lRevisi)
                            {
                                data.Add(new AlokasiSatkerRevisi()
                                {
                                    Revisi = itemRevisi.Revisi,
                                    Alokasi = itemRevisi.Alokasi
                                });
                            }
                            result[index].DaftarRevisi = data;
                        }

                        if (addTemp)
                        {
                            var tRevisi = listTempRevisi.FindAll(x => x.KodeSatker == item.KodeSatker).FirstOrDefault();
                            if (tRevisi != null)
                            {
                                result[index].TempAlokasi = tRevisi.Alokasi;
                            }

                            if (beforeValue.ToString() !=  tRevisi.Alokasi)
                            {
                                result[index].IsNilaiBaru = true;
                            }
                        }
                        index++;
                    }
                }
                else
                {
                    if (result != null && result.Count > 0)
                    {
                        var totalAlokasiSaatIni = result.Sum(y => y.Alokasi);
                        var alokasiSaatIni = new AlokasiSatkerRevisi
                        {
                            Revisi = 1,
                            Alokasi = totalAlokasiSaatIni
                        };
                        response.Total = new List<AlokasiSatkerRevisi>() {
                            new AlokasiSatkerRevisi
                            {
                                Revisi = 0,
                                Alokasi = result.Sum(y => y.Pagu)
                            },
                            alokasiSaatIni
                        };
                    }
                }
            }
            catch(Exception e)
            {
                _ = e.StackTrace;
            }

            response.Data = result.OrderBy(x => x.KodeSatker).OrderByDescending(x => x.IsNilaiBaru).ToList();
            return Json(new { success = true, data = response}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ListAlokasiBySummaryId(Entities.FormAlokasiSummaryDetail search)
        {
            List<AlokasiSatker> result = new List<AlokasiSatker>();
            var db = new PnbpContext();
            string id = search.id;

            try
            {
                string query = $@"SELECT 
                        a.ALOKASISATKERID,
                        a.KDSATKER as KodeSatker,
                        s.NAMA_SATKER as NamaSatker, 
                        a.PAGU,
                        a.ALOKASI,
                        a.TAHUN,
                        TO_CHAR(a.TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                        TO_CHAR(a.TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH 
                    FROM ALOKASISATKER a 
                    JOIN SATKER s ON a.KDSATKER = s.KODESATKER";

                List<object> lstparams = new List<object>();
                if (!String.IsNullOrEmpty(search.satker))
                {
                    query += " WHERE s.kantorid = :kantorId ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("kantorId", search.satker));
                }
                else
                { 
                    query += " WHERE s.kantorid IS NOT NULL ";
                }

                query += $" AND ALOKASISATKERSUMMARYID = :id ";
                lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("id", id));
                result = db.Database.SqlQuery<AlokasiSatker>(query, lstparams.ToArray()).ToList();

                string queryGetRevisi = $@"
                    SELECT ass.REVISI, a.kdsatker as KodeSatker, a.pagu, a.alokasi
                    FROM alokasisatker a
                    JOIN ALOKASISATKERSUMMARY ass ON a.ALOKASISATKERSUMMARYID = ass.ALOKASISATKERSUMMARYID 
                    WHERE a.ALOKASISATKERSUMMARYID IN (
	                    SELECT ALOKASISATKERSUMMARYID  
	                    FROM alokasisatkersummary
	                    WHERE tahun = EXTRACT (YEAR FROM sysdate)
                        AND mp = (select mp from ALOKASISATKERSUMMARY where alokasisatkersummaryid = '{id}') 
	                    AND revisi > 0
                    )
                    ORDER BY kdsatker, revisi asc
                ";
                var listRevisi = db.Database.SqlQuery<AlokasiSatker>(queryGetRevisi).ToList();

                string queryTempRevisi = $@"
                    SELECT 
                        (row_number() OVER (ORDER BY s.KODESATKER)) no,
                        s.KODESATKER AS kodesatker, 
                        s.NAMA_SATKER AS NamaSatker, 
                        to_char(ta.PAGU) AS pagu, 
                        to_char(ta.alokasi) AS alokasi 
                    FROM TEMP_ALOKASI_REVISI ta
                    LEFT JOIN satker s  ON ta.KDSATKER = s.KODESATKER 
                    WHERE ta.KDSATKER != '524465'
                ";
                var listTempRevisi = db.Database.SqlQuery<TempAlokasi>(queryTempRevisi).ToList();

                if (listRevisi.Count != 0 || listTempRevisi.Count > 0)
                {
                    bool addRevisi = listRevisi.Count > 0;
                    bool addTemp = listTempRevisi.Count > 0;
                    int index = 0;
                    foreach (var item in result)
                    {
                        decimal beforeValue = item.Alokasi;
                        if (addRevisi)
                        {
                            var data = new List<AlokasiSatkerRevisi>();
                            var lRevisi = listRevisi.FindAll(x => x.KodeSatker == item.KodeSatker).ToList();

                            var lastRevisi = listRevisi.OrderByDescending(x => x.Revisi).FirstOrDefault();
                            if (lastRevisi != null)
                            {
                                beforeValue = lastRevisi.Alokasi;
                            }

                            foreach (var itemRevisi in lRevisi)
                            {
                                data.Add(new AlokasiSatkerRevisi()
                                {
                                    Revisi = itemRevisi.Revisi,
                                    Alokasi = itemRevisi.Alokasi
                                });
                            }
                            result[index].DaftarRevisi = data;
                        }

                        if (addTemp)
                        {
                            var tRevisi = listTempRevisi.FindAll(x => x.KodeSatker == item.KodeSatker).FirstOrDefault();
                            if (tRevisi != null)
                            {
                                result[index].TempAlokasi = tRevisi.Alokasi;
                            }

                            if (beforeValue.ToString() != tRevisi.Alokasi)
                            {
                                result[index].IsNilaiBaru = true;
                            }
                        }
                        index++;
                    }
                }
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return Json(new { success = true, data = result.OrderBy(x => x.KodeSatker).OrderByDescending(x => x.IsNilaiBaru) }, JsonRequestBehavior.AllowGet);
        }

        public FileContentResult DownloadTemplateAlokasi()
        {
            PnbpContext db = new PnbpContext();

            DataTable dt = new DataTable($"{DateTime.Now.Year}");
            dt.Columns.AddRange(new DataColumn[4] {
                new DataColumn("No",typeof(int)),
                new DataColumn("Kode Satker"),
                new DataColumn("Pagu"),
                new DataColumn("Alokasi Saat Ini")
            });

            //string query = @"
            //WITH sb as ( 
            // SELECT sb.kdsatker, sum(sb.amount)  AS amount
            // FROM SPAN_BELANJA sb 
            // WHERE sb.TAHUN = EXTRACT (YEAR FROM SYSDATE) and sb.SUMBER_DANA = 'D'
            // AND kdsatker != '524465' 
            // GROUP BY sb.kdsatker
            //)
            //SELECT s.KODESATKER AS kodesatker, TO_CHAR(sb.AMOUNT) AS amount
            //FROM satker s
            //LEFT JOIN sb ON s.KODESATKER = sb.kdsatker
            //WHERE s.STATUSAKTIF = 1 AND s.KODESATKER != '524465'
            //";

            string query = @"
                WITH sb as ( 
                    SELECT sb.kdsatker, sum(sb.amount)  AS amount
                    FROM SPAN_BELANJA sb 
                    WHERE sb.TAHUN = EXTRACT (YEAR FROM SYSDATE) and sb.SUMBER_DANA = 'D'
                    AND kdsatker != '524465' 
                    GROUP BY sb.kdsatker
                )
                SELECT 
	                sb.KDSATKER  AS kodesatker, 
	                TO_CHAR(sb.AMOUNT) AS amount
                FROM sb
                LEFT JOIN satker s ON sb.kdsatker = s.KODESATKER and s.statusaktif = 1 
                WHERE 
	                sb.KDSATKER != '524465' AND sb.kdsatker IS NOT NULL 
                GROUP BY sb.KDSATKER, sb.AMOUNT 
                ORDER BY sb.KDSATKER 
            ";


            var get = db.Database.SqlQuery<TemplateAlokasi>(query).ToList();
            var index = 0;
            foreach (var rw in get)
            {
                dt.Rows.Add(index, rw.KodeSatker, rw.Amount);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template Alokasi.xlsx");
                }
            }
        }

        public ActionResult SummaryAlokasi(string tahun)
        {
            PnbpContext db = new PnbpContext();
            List<AlokasiSatkerSummary> result = new List<AlokasiSatkerSummary>();

            try
            {
                //string query = @"
                //SELECT 
                //    (row_number() OVER (ORDER BY a.MP)) no,
                //    a.ALOKASISATKERSUMMARYID, 
                //    a.PAGU, 
                //    a.ALOKASI, 
                //    TO_CHAR(a.TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                //    TO_CHAR(a.TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH,
                //    a.MP 
                //FROM ALOKASISATKERSUMMARY a 
                //WHERE a.tahun = extract(year from sysdate) 
                //ORDER BY MP ASC
                //";

                string query = @"
                    WITH grp AS (
	                    SELECT a.mp, max(a.revisi) revisi, a.tahun  
	                    FROM ALOKASISATKERSUMMARY a 
	                    GROUP BY a.mp, a.tahun
                    )
                    SELECT 
                        (row_number() OVER (ORDER BY ass.MP)) no,
                        ass.ALOKASISATKERSUMMARYID, 
                        ass.PAGU, 
                        ass.ALOKASI, 
                        TO_CHAR(ass.TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                        TO_CHAR(ass.TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH,
                        ass.MP 
                    FROM grp 
                    LEFT JOIN AlokasiSatkerSummary ass ON 
                    grp.mp = ass.MP AND 
                    grp.revisi = ass.revisi AND 
                    grp.tahun = ass.tahun 
                ";

                List<object> lstparams = new List<object>();

                if (!String.IsNullOrEmpty(tahun)) { 
                    query += " WHERE grp.tahun = :tahun ";
                    lstparams.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("tahun", tahun));
                }

                query += " ORDER BY ass.MP ";

                var parameters = lstparams.ToArray();
                result = db.Database.SqlQuery<AlokasiSatkerSummary>(query, parameters).ToList();
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SummaryAlokasiRevisi()
        {
            PnbpContext db = new PnbpContext();
            List<AlokasiSatkerSummary> result = new List<AlokasiSatkerSummary>();

            try
            {
                string query = @"
                SELECT 
                    (row_number() OVER (ORDER BY a.MP)) no,
                    a.ALOKASISATKERSUMMARYID, 
                    a.PAGU, 
                    a.ALOKASI, 
                    TO_CHAR(a.TANGGALBUAT,'DD-MM-YYYY') as TANGGALBUAT, 
                    TO_CHAR(a.TANGGALUBAH,'DD-MM-YYYY') as TANGGALUBAH,
                    a.MP 
                FROM ALOKASISATKERSUMMARY a 
                WHERE a.tahun = extract(year from sysdate) 
                ORDER BY MP ASC
                ";

                result = db.Database.SqlQuery<AlokasiSatkerSummary>(query).ToList();
            }
            catch (Exception e)
            {
                _ = e.StackTrace;
            }

            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AlokasiSaatIni(bool isRevisi = false)
        {
            PnbpContext db = new PnbpContext();
            List<TempAlokasi> result = new List<TempAlokasi>();
            string tableName = isRevisi ? "temp_alokasi_revisi" : "TEMP_ALOKASI";

            try
            {
                string query = $@"
                SELECT 
                    (row_number() OVER (ORDER BY s.KODESATKER)) no,
                    s.KODESATKER AS kodesatker, 
                    s.NAMA_SATKER AS NamaSatker, 
                    to_char(ta.PAGU) AS pagu, 
                    to_char(ta.alokasi) AS alokasi 
                FROM {tableName} ta
                LEFT JOIN satker s  ON ta.KDSATKER = s.KODESATKER  and s.statusaktif = 1 
                WHERE ta.KDSATKER != '524465'
                ";

                result = db.Database.SqlQuery<TempAlokasi>(query).ToList();
            } 
            catch(Exception e)
            {
                _ = e.StackTrace;   
            }

            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }

        class TemplateAlokasi
        {
            public string KodeSatker { get; set; }
            public string Amount { get; set; }
        }

        class TempAlokasi
        {
            public decimal No { get; set; }
            public string KodeSatker { get; set; }
            public string NamaSatker { get; set; }
            public string Pagu { get; set; }
            public string Alokasi { get; set; }
        }

        class AlokasiSatker
        {
            public decimal No { get; set; }
            public string AlokasiSatkerId { get; set; }
            public string KodeSatker { get; set; }
            public string NamaSatker { get; set; }
            public Decimal Pagu { get; set; }
            public Decimal Alokasi { get; set; }
            public Decimal Tahun { get; set; }
            public string TanggalBuat { get; set; }
            public string TanggalUbah { get; set; }
            public decimal Revisi { get; set; }
            public List<AlokasiSatkerRevisi> DaftarRevisi { get; set; }
            public List<AlokasiSatkerRevisi> DaftarTotalRevisiAlokasi { get; set; }
            public string TempAlokasi { get; set; }
            public bool IsNilaiBaru { get; set; }
            
        }

        class AlokasiSatkerDetail
        {
            public List<AlokasiSatker> Data { get; set; }
            public List<AlokasiSatkerRevisi> Total { get; set; }
        }

        class AlokasiSatkerRevisi
        {
            public decimal Revisi { get; set; }
            public decimal Alokasi { get; set; }
        }

        class AlokasiSatkerSummary
        {
            public decimal No { get; set; }
            public string AlokasiSatkerSummaryId { get; set; }
            public Decimal Mp { get; set; }
            public Decimal Pagu { get; set; }
            public Decimal Alokasi { get; set; }
            public Decimal Tahun { get; set; }
            public string TanggalBuat { get; set; }
            public string TanggalUbah { get; set; }
            public Decimal Revisi { get; set; }
        }

        class DataProsesAlokasi
        {
            public string KantorId { get; set; }
            public string Tipe { get; set; }
            public string ProgramId { get; set; }
            public string ProgramNama { get; set; }
            public string NamaSatker { get; set; }
            public string KodeSatker { get; set; }
            public string Kegiatan { get; set; }
            public string Output { get; set; }
            public string Akun { get; set; }
            public double Amount { get; set; }

        }
    }
}