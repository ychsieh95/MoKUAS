using MoKUAS.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MoKUAS.Models
{
    public class Comment : ClassBasic, IValidatableObject
    {
        public string ClassGuid { get; set; }

        #region Teach method

        /// <summary>
        /// 黑板式教學
        /// </summary>
        public bool IsBlackboard { get; set; }

        /// <summary>
        /// 課本教學
        /// </summary>
        public bool IsBook { get; set; }

        /// <summary>
        /// PPT 教學
        /// </summary>
        public bool IsPPT { get; set; }

        /// <summary>
        /// 廣播教學
        /// </summary>
        public bool IsBroadcast { get; set; }

        /// <summary>
        /// 實作教學
        /// </summary>
        public bool IsBuild { get; set; }

        /// <summary>
        /// 互動式教學
        /// </summary>
        public bool IsInteractive { get; set; }

        #endregion

        #region Roll call

        /// <summary>
        /// 點名頻率
        /// </summary>
        public decimal RollCallFrequency { get; set; }

        /// <summary>
        /// 親自點名
        /// </summary>
        public bool ByInPerson { get; set; }

        /// <summary>
        /// 簽到點名
        /// </summary>
        public bool BySignInSheet { get; set; }

        /// <summary>
        /// 線上點名
        /// </summary>
        public bool ByOnline { get; set; }

        /// <summary>
        /// 作業點名
        /// </summary>
        public bool ByClasswork { get; set; }

        /// <summary>
        /// 測驗點名
        /// </summary>
        public bool ByTest { get; set; }

        #endregion

        #region Classwork

        /// <summary>
        /// 有平時作業
        /// </summary>
        public bool HaveClasswork { get; set; }

        #endregion

        #region Exam

        /// <summary>
        /// 有平時測驗
        /// </summary>
        public bool HaveTest { get; set; }

        /// <summary>
        /// 有期中考試
        /// </summary>
        public bool HaveMidtermExam { get; set; }

        /// <summary>
        /// 有期末考試
        /// </summary>
        public bool HaveFinalExam { get; set; }

        #endregion

        #region Final comment

        /// <summary>
        /// 總評價 (1-5)
        /// </summary>
        public decimal Grade { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string Remark { get; set; }

        #endregion

        /// <summary>
        /// 評論建立者
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// 評論建立時間
        /// </summary>
        public DateTime RecordTime { get; set; }

        /// <summary>
        /// Comment guid
        /// </summary>
        public string Guid { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> validationResults = new List<ValidationResult>();

            // 檢查教學方式
            if (!(IsBlackboard || IsBook || IsPPT || IsBuild || IsInteractive))
                validationResults.Add(new ValidationResult("請選擇老師教學方式"));

            // 檢查點名
            if (RollCallFrequency == 0 && (ByInPerson || BySignInSheet || ByOnline || ByClasswork || ByTest))
                validationResults.Add(new ValidationResult("老師不點名為什麼要選點名方式？"));
            else if (RollCallFrequency > 0 && !(ByInPerson || BySignInSheet || ByOnline || ByClasswork || ByTest))
                validationResults.Add(new ValidationResult("請選擇老師點名方式"));

            // 檢查作業與考試
            if (!(HaveClasswork || HaveTest || HaveMidtermExam || HaveFinalExam))
                validationResults.Add(new ValidationResult("請選擇作業與考試"));

            // 檢查備註說明長度
            if (Remark.GetUTF8BytesCount() > 450)
                validationResults.Add(new ValidationResult("備註長度限制為 450 半形字元以內，請縮短後再重新提交"));

            // 檢查總評價
            if (Grade <= 0)
                validationResults.Add(new ValidationResult("老師至少值得一分吧？"));
            else if (Grade >= 6)
                validationResults.Add(new ValidationResult("最多五分不能再高囉！"));
            
            return validationResults;
        }
    }
}