using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pnbp.Entities
{
    public class FilterRecordResponse
    {
        public dynamic data { get; set; }
        public int? recordsTotal { get; set; } = 0;
        public decimal? recordsFiltered { get; set; } = 0;
    }
}
