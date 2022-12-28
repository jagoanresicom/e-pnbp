using System.Web;

namespace Pnbp.Entities
{
    public class LampiranKembalianUpload
    {
        public string PengembalianId { get; set; }
        public string FileName { get; set; }
        public string FormRequestFileName { get; set; }
        public string LampiranKembalianId { get; set; }
        public string UploadedFileNamePrefix { get; set; }
        public string TipeFile { get; set; }
        public HttpPostedFileBase RequestFile { get; set; }
    }
}
