using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoKUAS.Models
{
    public class Credits
    {
        public Dictionary<string, List<Score>> Subject { get; set; } = new Dictionary<string, List<Score>>();

        public Dictionary<string, List<Score>> GeneralEducation { get; set; } = new Dictionary<string, List<Score>>();
    }
}