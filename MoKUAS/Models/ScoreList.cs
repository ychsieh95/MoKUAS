using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoKUAS.Models
{
    public class ScoreList
    {
        /// <summary>
        /// 操行成績
        /// </summary>
        public string PersonalConduct { get; set; }

        /// <summary>
        /// 總平均
        /// </summary>
        public string AverageScore { get; set; }

        /// <summary>
        /// 班級排名
        /// </summary>
        public string ClassRank { get; set; }

        /// <summary>
        /// 班級學生人數
        /// </summary>
        public string ClassSize { get; set; }

        /// <summary>
        /// 班名次百分比
        /// </summary>
        public string ClassRankPercent { get; set; }

        /// <summary>
        /// 成績明細
        /// </summary>
        public List<Score> Scores { get; set; } = new List<Score>();
    }
}