using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoKUAS.Models
{
    public class Score
    {
        /// <summary>
        /// 項次
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 課程中文名稱
        /// </summary>
        public string SubjectChineseName { get; set; }

        /// <summary>
        /// 學分數
        /// </summary>
        public string PropertiesCredit { get; set; }

        /// <summary>
        /// 授課時數
        /// </summary>
        public string TeachTime { get; set; }

        /// <summary>
        /// 必選修
        /// </summary>
        public string PropertiesRequiredOrElective { get; set; }
        public bool IsRequired { get { return PropertiesRequiredOrElective.Contains("必修"); } }

        /// <summary>
        /// 開課別
        /// </summary>
        public string ClassType { get; set; }

        /// <summary>
        /// 期中成績
        /// </summary>
        public string MidtermScore { get; set; }

        /// <summary>
        /// 學期成績
        /// </summary>
        public string SemesterScore { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string Remark { get; set; }
    }
}