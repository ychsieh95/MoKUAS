using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoKUAS.Pages.Student
{
    public class CreditsModel : PageModel
    {
        public int GetCredits { get; private set; }
        public int GetRequiredCredits { get; private set; }
        public int GetNotRequiredCredits { get; private set; }
        public int MissRequiredCredits { get; private set; }
        public Models.Credits Credits { get; private set; }

        public void OnGet()
        {
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();
            var sysValueList = kuasAp.GetOptionValueList(student: student);
            var scores = new List<Models.Score>();

            var subject = new Dictionary<string, List<Models.Score>>()
            {
                { "Get", new List<Models.Score>() },
                { "Miss", new List<Models.Score>() }
            };
            var generalEducation = new Dictionary<string, List<Models.Score>>()
            {
                { "Core", new List<Models.Score>() },
                { "Extend", new List<Models.Score>() }
            };

            for (int i = 0, countOfEmpty = 0; countOfEmpty < 10; i++)
            {
                var tmpScores = kuasAp.GetScores(student: student, year: sysValueList[i].SysYear, semester: sysValueList[i].SysSemester).Scores;
                if (tmpScores.Count <= 0)
                    countOfEmpty++;
                else
                {
                    scores.AddRange(tmpScores);
                    countOfEmpty = 0;
                }
            }
            if (scores.Count > 0)
            {
                foreach (Models.Score score in scores)
                    if (!string.IsNullOrEmpty(score.SemesterScore))
                        if (score.SemesterScore == "合格")
                            subject["Get"].Add(score);
                        else if (score.SemesterScore.Contains("*") && score.IsRequired)
                            subject["Miss"].Add(score);
                        else if (score.SemesterScore.Contains("不合格") && score.IsRequired)
                            subject["Miss"].Add(score);
                        else if (float.Parse(score.SemesterScore) < 60)
                            subject["Miss"].Add(score);
                        else
                        {
                            subject["Get"].Add(score);
                            if (score.SubjectChineseName.Contains("核心通識"))
                                generalEducation["Core"].Add(score);
                            else if (score.SubjectChineseName.Contains("延伸通識"))
                                generalEducation["Extend"].Add(score);
                        }
            }

            // 刪除課程中的通識課程
            subject["Get"].RemoveAll(x => x.SubjectChineseName.Contains("通識"));
            subject["Miss"].RemoveAll(x => x.SubjectChineseName.Contains("通識"));

            // 刪除重複已失去課程
            foreach (Models.Score score in subject["Miss"].ToArray())
            {
                // 建立暫存課程
                Models.Score temp = score;
                // 尋找是否有重複 (2 個以上) 項目
                if (subject["Miss"].FindAll(x => x.SubjectChineseName == temp.SubjectChineseName &&
                                                 x.PropertiesCredit == temp.PropertiesCredit &&
                                                 x.PropertiesRequiredOrElective == temp.PropertiesRequiredOrElective).Count > 1)
                {
                    // 若有則全數移除並重新加入
                    subject["Miss"].RemoveAll(x => x.SubjectChineseName == temp.SubjectChineseName &&
                                                   x.PropertiesCredit == temp.PropertiesCredit &&
                                                   x.PropertiesRequiredOrElective == temp.PropertiesRequiredOrElective);
                    subject["Miss"].Add(temp);
                }
            }

            // 重修已及格修正
            foreach (Models.Score score in subject["Get"].ToArray())
            {
                if (subject["Miss"].FindAll(x => x.SubjectChineseName == score.SubjectChineseName &&
                                                 x.PropertiesCredit == score.PropertiesCredit &&
                                                 x.PropertiesRequiredOrElective == score.PropertiesRequiredOrElective).Count > 0)
                {
                    subject["Miss"].RemoveAll(x => x.SubjectChineseName == score.SubjectChineseName &&
                                                   x.PropertiesCredit == score.PropertiesCredit &&
                                                   x.PropertiesRequiredOrElective == score.PropertiesRequiredOrElective);
                }
            }

            // 對 subject 做排序
            foreach (string key in subject.Keys.ToArray())
                subject[key] = subject[key].OrderBy(x => float.Parse(x.PropertiesCredit))
                                           .ThenBy(x => x.SubjectChineseName)
                                           .ThenBy(x => float.Parse(x.TeachTime))
                                           .ThenBy(x => x.PropertiesRequiredOrElective)
                                           .ThenBy(x => x.ClassType)
                                           .ThenBy(x => x.MidtermScore)
                                           .ThenBy(x => x.SemesterScore)
                                           .ThenBy(x => x.Remark).ToList();
            // 對 generalEducation 以自定義做排序
            foreach (string key in generalEducation.Keys.ToArray())
                generalEducation[key].Sort((x, y) => new OrderBySubjectChtName().Compare(x.SubjectChineseName, y.SubjectChineseName));

            Credits = new Models.Credits()
            {
                Subject = subject,
                GeneralEducation = generalEducation
            };
            // 已取得所有學分
            GetCredits = (int)subject["Get"].Sum(x => float.Parse(x.PropertiesCredit));
            // 已取得必修學分
            GetRequiredCredits = (int)subject["Get"].FindAll(x => x.IsRequired).Sum(x => float.Parse(x.PropertiesCredit));
            // 已取得選修學分
            GetNotRequiredCredits = (int)subject["Get"].FindAll(x => !x.IsRequired).Sum(x => float.Parse(x.PropertiesCredit));
            // 已失去必修學分
            MissRequiredCredits = (int)subject["Miss"].Sum(x => float.Parse(x.PropertiesCredit));
        }
    }

    /// <summary>
    /// 自定義班級名稱排序
    /// </summary>
    public class OrderByClassShortName : IComparer<String>
    {
        public int Compare(string x, string y)
        {
            if (x.Substring(0, x.Length - 2) != y.Substring(0, y.Length - 2))
            {
                return x.CompareTo(y);
            }

            string sX = x.Substring(x.Length - 2, 1);
            string sY = y.Substring(y.Length - 2, 1);

            switch (sX)
            {
                case "一":
                    switch (sY)
                    {
                        case "一":
                            return Compare2(x.Substring(x.Length - 1, 1), y.Substring(y.Length - 1, 1));
                        case "二":
                            return -1;
                        case "三":
                            return -1;
                        case "四":
                            return -1;
                        default:
                            return -1;
                    }

                case "二":
                    switch (sY)
                    {
                        case "一":
                            return 1;
                        case "二":
                            return Compare2(x.Substring(x.Length - 1, 1), y.Substring(y.Length - 1, 1));
                        case "三":
                            return -1;
                        case "四":
                            return -1;
                        default:
                            return -1;
                    }

                case "三":
                    switch (sY)
                    {
                        case "一":
                            return 1;
                        case "二":
                            return 1;
                        case "三":
                            return Compare2(x.Substring(x.Length - 1, 1), y.Substring(y.Length - 1, 1));
                        case "四":
                            return -1;
                        default:
                            return -1;
                    }

                case "四":
                    switch (sY)
                    {
                        case "一":
                            return 1;
                        case "二":
                            return 1;
                        case "三":
                            return 1;
                        case "四":
                            return Compare2(x.Substring(x.Length - 1, 1), y.Substring(y.Length - 1, 1));
                        default:
                            return -1;
                    }

                default:
                    return -1;
            }
        }

        public int Compare2(string x, string y)
        {
            //甲乙丙丁
            switch (x)
            {
                case "甲":
                    switch (y)
                    {
                        case "甲":
                            return 0;
                        case "乙":
                            return -1;
                        case "丙":
                            return -1;
                        case "丁":
                            return -1;
                        default:
                            return -1;
                    }

                case "乙":
                    switch (y)
                    {
                        case "甲":
                            return 1;
                        case "乙":
                            return 0;
                        case "丙":
                            return -1;
                        case "丁":
                            return -1;
                        default:
                            return -1;
                    }

                case "丙":
                    switch (y)
                    {
                        case "甲":
                            return 1;
                        case "乙":
                            return 1;
                        case "丙":
                            return 0;
                        case "丁":
                            return -1;
                        default:
                            return -1;
                    }

                case "丁":
                    switch (y)
                    {
                        case "甲":
                            return 1;
                        case "乙":
                            return 1;
                        case "丙":
                            return 1;
                        case "丁":
                            return 0;
                        default:
                            return -1;
                    }

                default:
                    return -1;
            }
        }
    }

    /// <summary>
    /// 自定義課程中文名稱排序
    /// </summary>
    public class OrderBySubjectChtName : IComparer<string>
    {

        public int Compare(string x, string y)
        {

            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    string[] countCht = null;
                    if (x.Contains("核心通識") & x.Contains("核心通識"))
                    {
                        countCht = new string[]
                        {
                            "一",
                            "二",
                            "三",
                            "四",
                            "五"
                        };
                    }
                    else if (x.Contains("延伸通識") & x.Contains("延伸通識"))
                    {
                        countCht = new string[]
                        {
                            "科技",
                            "人文",
                            "社會",
                            "自然"
                        };
                    }
                    else
                    {
                        return 0;
                    }

                    int indexX = Array.FindIndex<string>(countCht, i => x.Substring(5, 1 + (5 - countCht.Count())).SequenceEqual(i));
                    int indexY = Array.FindIndex<string>(countCht, i => y.Substring(5, 1 + (5 - countCht.Count())).SequenceEqual(i));
                    return indexX - indexY;

                }
            }

        }
    }
}