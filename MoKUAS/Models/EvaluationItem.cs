using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoKUAS.Models
{
    public class EvaluationItem
    {
        /// <summary>
        /// 教學評量標題
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 教學評量課程中文名稱
        /// </summary>
        public string SubjectChineseName { get; set; }

        /// <summary>
        /// 教學評量授課教師名稱
        /// </summary>
        public string Teachers { get; set; }

        /// <summary>
        /// 教學評量類型
        /// </summary>
        public EvaliationType EvaliationType { get; set; }

        /// <summary>
        /// 是否完成教學評量
        /// </summary>
        public bool Done { get; set; }

        /// <summary>
        /// 表單隱藏欄位
        /// </summary>
        public List<RestSharp.Parameter> HiddenInputs;
    }
}