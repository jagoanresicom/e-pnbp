using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Pnbp.Entities
{
    public class CommonResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }
    }

    public class ChartData
    {
        public string name { get; set; }
        public dynamic y { get; set; }
        public dynamic legendIndex { get; set; }
        public dynamic data1 { get; set; }
        public dynamic data2 { get; set; }
    }

}
