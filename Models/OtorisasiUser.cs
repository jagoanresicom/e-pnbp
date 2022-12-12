namespace System.Web.Mvc
{
    public class OtorisasiUser
    {
        public static string NamaSkema
        {
            get
            {
                string namaSkema = System.Configuration.ConfigurationManager.AppSettings["NamaSkema"].ToString();

                return namaSkema;
            }
        }

        public static string NamaSkemaKKP
        {
            get
            {
                string namaSkema = System.Configuration.ConfigurationManager.AppSettings["NamaSkemaKKP"].ToString();

                return namaSkema;
            }
        }

        public static string GetJenisKantorUser()
        {
            Pnbp.Models.HakAksesModel model = new Pnbp.Models.HakAksesModel();

            string CurrentUserRole = "Pusat";

            try
            {
                var userIdentity = new Pnbp.Codes.Functions().claimUser();
                string kantorid = userIdentity.KantorId;

                int tipekantor = model.GetTipeKantor(kantorid);

                if (tipekantor == 1)
                {
                    CurrentUserRole = "Pusat";
                }
                else if (tipekantor == 2)
                {
                    CurrentUserRole = "Kanwil";
                }
                else if (tipekantor == 3 || tipekantor == 4)
                {
                    CurrentUserRole = "Kantah";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return CurrentUserRole;
        }

        public static string GetJenisKantor(string kantorid)
        {
            Pnbp.Models.HakAksesModel model = new Pnbp.Models.HakAksesModel();

            string CurrentUserRole = "Pusat";

            try
            {
                int tipekantor = model.GetTipeKantor(kantorid);

                if (tipekantor == 1)
                {
                    CurrentUserRole = "Pusat";
                }
                else if (tipekantor == 2)
                {
                    CurrentUserRole = "Kanwil";
                }
                else if (tipekantor == 3 || tipekantor == 4)
                {
                    CurrentUserRole = "Kantah";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return CurrentUserRole;
        }

        public static bool IsAdminRole()
        {
            bool CurrentAdminRole = false;

            try
            {
                var userIdentity = new Pnbp.Codes.Functions().claimUser();
                string pegawaiid = userIdentity.PegawaiId;
                string kantorid = userIdentity.KantorId;
                string profileid = "'A80100','A80300','A80400','A80500','B80100'"; // profile Administrator

                Pnbp.Models.HakAksesModel model = new Pnbp.Models.HakAksesModel();

                CurrentAdminRole = model.CheckUserProfile(pegawaiid, kantorid, profileid);
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return CurrentAdminRole;
        }

        public static bool IsAuthorizedPengembalian()
        {
            bool isAuthorized = false;

            try
            {
                var userIdentity = new Pnbp.Codes.Functions().claimUser();
                string pegawaiid = userIdentity.PegawaiId;
                string kantorid = userIdentity.KantorId;
                string jenisKantor = GetJenisKantor(kantorid);

                Pnbp.Models.HakAksesModel model = new Pnbp.Models.HakAksesModel();

                if (jenisKantor == "Kantah")
                {
                    string roles = "'KepalaSubBagianTataUsaha'";
                    isAuthorized = model.CheckValidUserProfileRoles(pegawaiid, kantorid, roles);
                }
                else if (jenisKantor == "Kanwil")
                { 
                    string roles = "'KepalaUrusanKeuangan'";
                    isAuthorized = model.CheckValidUserProfileRoles(pegawaiid, kantorid, roles);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return isAuthorized;
        }
    }
}