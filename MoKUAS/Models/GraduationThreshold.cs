using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoKUAS.Models
{
    public class GraduationThreshold
    {
        /// <summary>
        /// 學生基本資訊
        /// </summary>
        public StdBasicInfo StdBasicInfo { get; set; } = new StdBasicInfo();

        /// <summary>
        /// 英語畢業門檻通過狀態(英語能力訓練課程修習通過即通過英語畢業門檻)
        /// </summary>
        public string EngEngGradPass { get; set; }

        /// <summary>
        /// 英語能力訓練課程修習狀態
        /// </summary>
        public string EngTrainSta { get; set; }

        /// <summary>
        /// 英語能力訓練課程修習狀態
        /// </summary>
        public List<OcpClass> OcpClassList { get; set; } = new List<OcpClass>();
    }

    public class StdBasicInfo
    {
        /// <summary>
        /// 班級
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 學號
        /// </summary>
        public string StdId { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string StdName { get; set; }
    }

    public class OcpClass
    {
        public string SysYear { get; set; }

        public string SysSemester { get; set; }

        public string CourseId { get; set; }

        public string SubjectChineseName { get; set; }

        public string Score { get; set; }
    }
}