using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoKUAS.Models
{
    public class MidtermAlert
    {
        public string Index { get; set; }

        public string ClassName { get; set; }

        public string SubjectChineseName { get; set; }

        public string Group { get; set; }

        public string Teachers { get; set; }

        public bool IsAlert { get; set; }

        public string AlertReason { get; set; }

        public string Remark { get; set; }
    }
}