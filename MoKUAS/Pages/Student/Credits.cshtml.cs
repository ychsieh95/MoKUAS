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
                        if (score.SemesterScore == "�X��")
                            subject["Get"].Add(score);
                        else if (score.SemesterScore.Contains("*") && score.IsRequired)
                            subject["Miss"].Add(score);
                        else if (score.SemesterScore.Contains("���X��") && score.IsRequired)
                            subject["Miss"].Add(score);
                        else if (float.Parse(score.SemesterScore) < 60)
                            subject["Miss"].Add(score);
                        else
                        {
                            subject["Get"].Add(score);
                            if (score.SubjectChineseName.Contains("�֤߳q��"))
                                generalEducation["Core"].Add(score);
                            else if (score.SubjectChineseName.Contains("�����q��"))
                                generalEducation["Extend"].Add(score);
                        }
            }

            // �R���ҵ{�����q�ѽҵ{
            subject["Get"].RemoveAll(x => x.SubjectChineseName.Contains("�q��"));
            subject["Miss"].RemoveAll(x => x.SubjectChineseName.Contains("�q��"));

            // �R�����Ƥw���h�ҵ{
            foreach (Models.Score score in subject["Miss"].ToArray())
            {
                // �إ߼Ȧs�ҵ{
                Models.Score temp = score;
                // �M��O�_������ (2 �ӥH�W) ����
                if (subject["Miss"].FindAll(x => x.SubjectChineseName == temp.SubjectChineseName &&
                                                 x.PropertiesCredit == temp.PropertiesCredit &&
                                                 x.PropertiesRequiredOrElective == temp.PropertiesRequiredOrElective).Count > 1)
                {
                    // �Y���h���Ʋ����í��s�[�J
                    subject["Miss"].RemoveAll(x => x.SubjectChineseName == temp.SubjectChineseName &&
                                                   x.PropertiesCredit == temp.PropertiesCredit &&
                                                   x.PropertiesRequiredOrElective == temp.PropertiesRequiredOrElective);
                    subject["Miss"].Add(temp);
                }
            }

            // ���פw�ή�ץ�
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

            // �� subject ���Ƨ�
            foreach (string key in subject.Keys.ToArray())
                subject[key] = subject[key].OrderBy(x => float.Parse(x.PropertiesCredit))
                                           .ThenBy(x => x.SubjectChineseName)
                                           .ThenBy(x => float.Parse(x.TeachTime))
                                           .ThenBy(x => x.PropertiesRequiredOrElective)
                                           .ThenBy(x => x.ClassType)
                                           .ThenBy(x => x.MidtermScore)
                                           .ThenBy(x => x.SemesterScore)
                                           .ThenBy(x => x.Remark).ToList();
            // �� generalEducation �H�۩w�q���Ƨ�
            foreach (string key in generalEducation.Keys.ToArray())
                generalEducation[key].Sort((x, y) => new OrderBySubjectChtName().Compare(x.SubjectChineseName, y.SubjectChineseName));

            Credits = new Models.Credits()
            {
                Subject = subject,
                GeneralEducation = generalEducation
            };
            // �w���o�Ҧ��Ǥ�
            GetCredits = (int)subject["Get"].Sum(x => float.Parse(x.PropertiesCredit));
            // �w���o���׾Ǥ�
            GetRequiredCredits = (int)subject["Get"].FindAll(x => x.IsRequired).Sum(x => float.Parse(x.PropertiesCredit));
            // �w���o��׾Ǥ�
            GetNotRequiredCredits = (int)subject["Get"].FindAll(x => !x.IsRequired).Sum(x => float.Parse(x.PropertiesCredit));
            // �w���h���׾Ǥ�
            MissRequiredCredits = (int)subject["Miss"].Sum(x => float.Parse(x.PropertiesCredit));
        }
    }

    /// <summary>
    /// �۩w�q�Z�ŦW�ٱƧ�
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
                case "�@":
                    switch (sY)
                    {
                        case "�@":
                            return Compare2(x.Substring(x.Length - 1, 1), y.Substring(y.Length - 1, 1));
                        case "�G":
                            return -1;
                        case "�T":
                            return -1;
                        case "�|":
                            return -1;
                        default:
                            return -1;
                    }

                case "�G":
                    switch (sY)
                    {
                        case "�@":
                            return 1;
                        case "�G":
                            return Compare2(x.Substring(x.Length - 1, 1), y.Substring(y.Length - 1, 1));
                        case "�T":
                            return -1;
                        case "�|":
                            return -1;
                        default:
                            return -1;
                    }

                case "�T":
                    switch (sY)
                    {
                        case "�@":
                            return 1;
                        case "�G":
                            return 1;
                        case "�T":
                            return Compare2(x.Substring(x.Length - 1, 1), y.Substring(y.Length - 1, 1));
                        case "�|":
                            return -1;
                        default:
                            return -1;
                    }

                case "�|":
                    switch (sY)
                    {
                        case "�@":
                            return 1;
                        case "�G":
                            return 1;
                        case "�T":
                            return 1;
                        case "�|":
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
            //�ҤA���B
            switch (x)
            {
                case "��":
                    switch (y)
                    {
                        case "��":
                            return 0;
                        case "�A":
                            return -1;
                        case "��":
                            return -1;
                        case "�B":
                            return -1;
                        default:
                            return -1;
                    }

                case "�A":
                    switch (y)
                    {
                        case "��":
                            return 1;
                        case "�A":
                            return 0;
                        case "��":
                            return -1;
                        case "�B":
                            return -1;
                        default:
                            return -1;
                    }

                case "��":
                    switch (y)
                    {
                        case "��":
                            return 1;
                        case "�A":
                            return 1;
                        case "��":
                            return 0;
                        case "�B":
                            return -1;
                        default:
                            return -1;
                    }

                case "�B":
                    switch (y)
                    {
                        case "��":
                            return 1;
                        case "�A":
                            return 1;
                        case "��":
                            return 1;
                        case "�B":
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
    /// �۩w�q�ҵ{����W�ٱƧ�
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
                    if (x.Contains("�֤߳q��") & x.Contains("�֤߳q��"))
                    {
                        countCht = new string[]
                        {
                            "�@",
                            "�G",
                            "�T",
                            "�|",
                            "��"
                        };
                    }
                    else if (x.Contains("�����q��") & x.Contains("�����q��"))
                    {
                        countCht = new string[]
                        {
                            "���",
                            "�H��",
                            "���|",
                            "�۵M"
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