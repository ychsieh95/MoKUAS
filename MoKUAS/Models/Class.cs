namespace MoKUAS.Models
{
    public class ClassBasic
    {
        /// <summary>
        /// 課程中文名稱
        /// </summary>
        public string SubjectChineseName { get; set; }

        /// <summary>
        /// 班級簡稱
        /// </summary>
        public string ClassShortName { get; set; }

        /// <summary>
        /// 授課教師
        /// </summary>
        public string Teachers { get; set; }
    }

    public class Class
    {
        /// <summary>
        /// 選課代碼
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// 課程中文名稱
        /// </summary>
        public string SubjectChineseName { get; set; }

        /// <summary>
        /// 班級簡稱
        /// </summary>
        public string ClassShortName { get; set; }

        /// <summary>
        /// 分組
        /// </summary>
        public string Group { get; set; }

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

        /// <summary>
        /// 開課別
        /// </summary>
        public string ClassType { get; set; }

        /// <summary>
        /// 上課時間 (周/節)
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// 上課時間 (節)
        /// </summary>
        public string SectionTime { get; set; }

        /// <summary>
        /// 授課教師
        /// </summary>
        public string Teachers { get; set; }

        /// <summary>
        /// 上課教室
        /// </summary>
        public string Classroom { get; set; }

        /// <summary>
        /// 表示該課程之學年學期
        /// </summary>
        public string SubjectYMS { get; set; }

        /// <summary>
        /// 課程 GUID 編碼
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// 表示特定學生是否已寫過此課程之評論
        /// </summary>
        public bool DoneComment { get; set; }

        public string ConvertToCell()
        {
            return (string.Format("{0}\n{1}\n{2}",
                string.IsNullOrWhiteSpace(SubjectChineseName) ? "" : SubjectChineseName,
                string.IsNullOrWhiteSpace(Teachers) ? "" : Teachers,
                string.IsNullOrWhiteSpace(Classroom) ? "" : Classroom));
        }
    }
}