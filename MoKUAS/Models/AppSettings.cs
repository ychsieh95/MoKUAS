using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoKUAS.Models
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        
        public GoogleReCaptcha GoogleReCaptcha { get; set; }

        public JWT JWT { get; set; }
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; }
    }

    public class GoogleReCaptcha
    {
        public string SiteKey { get; set; }
        public string Secret { get; set; }
    }

    public class JWT
    {
        public string Secret { get; set; }
    }
}
