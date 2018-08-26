using System;
using System.IO;

namespace MoKUAS.Models
{
    public class Profile
    {
        /// <summary>
        /// 學制
        /// </summary>
        public string EductionalSystem { get; set; }

        /// <summary>
        /// 科系
        /// </summary>
        public string College { get; set; }

        /// <summary>
        /// 班級
        /// </summary>
        public string ChtClass { get; set; }

        /// <summary>
        /// 學號
        /// </summary>
        public string StudentIdNumber { get; set; }

        /// <summary>
        /// 中文姓名
        /// </summary>
        public string ChtName { get; set; }

        /// <summary>
        /// 英文姓名
        /// </summary>
        public string EngName { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime Birthday { get; set; }

        /// <summary>
        /// 出生地
        /// </summary>
        public string BirthPlace { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// 服役紀錄
        /// </summary>
        public string ServiceRecord { get; set; }

        /// <summary>
        /// 身分證字號
        /// </summary>
        public string IdNumber { get; set; }

        /// <summary>
        /// 在學狀態
        /// </summary>
        public string StateOfDuringTheSchool { get; set; }

        /// <summary>
        /// 入學前學歷
        /// </summary>
        public string BeforeEducation { get; set; }

        /// <summary>
        /// 家長姓名
        /// </summary>
        public string ParantName { get; set; }

        /// <summary>
        /// 與家長關係
        /// </summary>
        public string RelationshipOfParant { get; set; }

        /// <summary>
        /// 聯絡電話
        /// </summary>
        public string CorrespondenceTele { get; set; }

        /// <summary>
        /// 戶籍地址
        /// </summary>
        public string PermanentAddress { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        public string CorrespondenceAddress { get; set; }

        /// <summary>
        /// 個人照片
        /// </summary>
        public MemoryStream PersonalPicture { get; set; }
    }
}
