using HtmlAgilityPack;
using MoKUAS.Extensions;
using MoKUAS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace MoKUAS.Services
{
    public class KUASAPService
    {
        // http://blog.miniasp.com/post/2014/01/15/C-Sharp-String-Trim-ZWSP-Zero-width-space.aspx
        public char[] allWhiteSpace = new char[]
        {
            // SpaceSeparator category
            '\u0020', '\u1680', '\u180E', '\u2000', '\u2001', '\u2002', '\u2003',
            '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200A',
            '\u202F', '\u205F', '\u3000',
            // LineSeparator category
            '\u2028',
            // ParagraphSeparator category
            '\u2029',
            // Latin1 characters
            '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u0085', '\u00A0',
            // ZERO WIDTH SPACE (U+200B) & ZERO WIDTH NO-BREAK SPACE (U+FEFF)
            '\u200B', '\uFEFF'
        };

        // KUAS AP host and its hostname
        private const string KUASAP_HOST_URL_1 = "http://webap1.kuas.edu.tw"; // New view
        private const string KUASAP_HOST_URL_2 = "http://webap2.kuas.edu.tw"; // New view
        private const string KUASAP_HOST_URL_3 = "http://webap3.kuas.edu.tw"; // Old view
        private const string KUASAP_HOST_IP_NEW = "http://140.127.113.231";
        private const string KUASAP_HOST_IP_OLD = "http://140.127.113.224";
        private const string KUAS_SELECTED_COURSE_SYSTEM = "http://140.127.113.108";
        private const string KUAS_EVALUATION_SYSTEM = "http://npj.kuas.edu.tw";

        // Pick the kuasap host which in use
        private const string KUASAP_HOST_IN_USE = KUASAP_HOST_URL_1;

        #region Login

        public string Login(ref Student student)
        {
            // Clear Cookies
            CookieContainer cookieContainer = new CookieContainer();

            // Login kuas ap
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = cookieContainer;

            RestRequest request = new RestRequest("kuas/perchk.jsp", Method.POST);
            request.AddParameter("uid", student.Username);
            request.AddParameter("pwd", student.Password);

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            /*
             * Redirect
             *  --> f_index.html : success
             *  --> index.html   : failure
             */
            if (respHTML.Contains("top.location.href='f_index.html'"))
            {
                // Save cookies
                student.Cookies = cookieContainer.GetAllCookies();

                // Get home page
                request = new RestRequest("kuas/f_head.jsp", Method.GET);
                response = client.Execute(request);
                respHTML = response.Content;

                // Convert html content (string) into HtmlDocument
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));

                switch (KUASAP_HOST_IN_USE)
                {
                    case KUASAP_HOST_URL_1:
                    case KUASAP_HOST_URL_2:
                    case KUASAP_HOST_IP_NEW:
                        student.SysYear = htmlDoc.DocumentNode.SelectSingleNode("//*[contains(@class, personal)]/span[1]").InnerText.Split(new string[] { "學" }, StringSplitOptions.None)[0];
                        student.SysSemester = htmlDoc.DocumentNode.SelectSingleNode("//*[contains(@class, personal)]/span[1]").InnerText.Split(new string[] { "第" }, StringSplitOptions.None)[1].Split(new string[] { "學" }, StringSplitOptions.None)[0];
                        student.ChtClass = htmlDoc.DocumentNode.SelectNodes("//*[contains(@class, personal)]/span").Count == 3 ? htmlDoc.DocumentNode.SelectSingleNode("//*[contains(@class, personal)]/span[2]").InnerText : "";
                        student.ChtName = htmlDoc.DocumentNode.SelectNodes("//*[contains(@class, personal)]/span").Last().InnerText;
                        break;

                    case KUASAP_HOST_URL_3:
                    case KUASAP_HOST_IP_OLD:
                        student.SysYear = htmlDoc.DocumentNode.SelectSingleNode("//table[1]/tr[1]/td[3]/span[1]").InnerText.Split(new string[] { "學" }, StringSplitOptions.None)[0];
                        student.SysSemester = htmlDoc.DocumentNode.SelectSingleNode("//table[1]/tr[1]/td[3]/span[1]").InnerText.Split(new string[] { "第" }, StringSplitOptions.None)[1].Split(new string[] { "學" }, StringSplitOptions.None)[0];
                        student.ChtClass = htmlDoc.DocumentNode.SelectNodes("//*[contains(@class, personal)]/span").Count == 3 ? htmlDoc.DocumentNode.SelectSingleNode("//*[contains(@class, personal)]/span[2]").InnerText : "";
                        student.ChtName = htmlDoc.DocumentNode.SelectNodes("//*[contains(@class, personal)]/span").Last().InnerText;
                        break;
                }

                return JsonConvert.SerializeObject(new { state = true });
            }
            else
            {
                try
                {
                    string alertMessage = respHTML.
                        Split(new string[] { "<script language='javascript'>alert('" }, StringSplitOptions.RemoveEmptyEntries)[1].
                        Split(new string[] { "');top.location.href='index.html'" }, StringSplitOptions.RemoveEmptyEntries)[0];

                    if (alertMessage.Contains("系統繁忙"))
                        return JsonConvert.SerializeObject(new { state = false, message = "系統目前繁忙中，請稍後再試" });
                    else if (alertMessage.Contains("無此帳號或密碼不正確"))
                        return JsonConvert.SerializeObject(new { state = false, message = "帳號不存在或是密碼錯誤" });
                    else if (!string.IsNullOrEmpty(alertMessage))
                        return JsonConvert.SerializeObject(new { state = false, message = alertMessage });
                    else
                        return JsonConvert.SerializeObject(new { state = false, message = "系統無回應或連接異常，請稍後再試" });
                }
                catch (IndexOutOfRangeException) { return JsonConvert.SerializeObject(new { state = false, message = "系統登入異常，請重新再試" }); }
            }
        }

        #endregion

        #region Get Profile

        public Profile GetProfile(Student student)
        {
            // Get webpage
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/ag_pro/ag003.jsp", Method.POST);
            request.AddParameter("arg01", student.SysYear);
            request.AddParameter("arg02", student.SysSemester);
            request.AddParameter("arg03", student.Username);
            request.AddParameter("arg04", "");
            request.AddParameter("arg05", "");
            request.AddParameter("arg06", "");
            request.AddParameter("fncid", "AG003");
            request.AddParameter("uid", student.Username);

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            // Convert html content (string) into HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));
            HtmlNode table = htmlDoc.DocumentNode.SelectSingleNode("//body/table[2]/tr/td/table");

            if (table == null)
                return null;
            else
            {
                var profile = new Profile()
                {
                    EductionalSystem = table.SelectSingleNode("./tr[1]/td[1]").InnerText.Split('：')[1],
                    College = table.SelectSingleNode("./tr[1]/td[2]").InnerText.Split('：')[1],
                    ChtClass = table.SelectSingleNode("./tr[2]/td[1]").InnerText.Split('：')[1],
                    StudentIdNumber = table.SelectSingleNode("./tr[2]/td[2]").InnerText.Split('：')[1],
                    ChtName = table.SelectSingleNode("./tr[3]/td[1]").InnerText.Split('：')[1],
                    EngName = table.SelectSingleNode("./tr[3]/td[2]").InnerText.Split('：')[1],
                    /* Birthday */
                    BirthPlace = table.SelectSingleNode("./tr[4]/td[2]").InnerText.Split('：')[1],
                    /* Gender */
                    ServiceRecord = table.SelectSingleNode("./tr[5]/td[2]").InnerText.Split('：')[1],
                    IdNumber = table.SelectSingleNode("./tr[6]/td[1]").InnerText.Split('：')[1],
                    StateOfDuringTheSchool = table.SelectSingleNode("./tr[6]/td[2]").InnerText.Split('：')[1],
                    BeforeEducation = table.SelectSingleNode("./tr[6]/td[3]").InnerText.Split('：')[1],
                    ParantName = table.SelectSingleNode("./tr[7]/td[1]").InnerText.Split('：')[1],
                    RelationshipOfParant = table.SelectSingleNode("./tr[7]/td[2]").InnerText.Split('：')[1],
                    CorrespondenceTele = table.SelectSingleNode("./tr[7]/td[3]").InnerText.Split(':')[1],
                    PermanentAddress = table.SelectSingleNode("./tr[8]/td[1]").InnerText.Split('：')[1],
                    CorrespondenceAddress = table.SelectSingleNode("./tr[9]/td[1]").InnerText.Split('：')[1]
                };

                // Birthday
                var birthday = table.SelectSingleNode("./tr[4]/td[1]").InnerText.Split('：')[1];
                Regex regex = new Regex(@"([^)]*)年([^)]*)月([^)]*)日");
                Match match = regex.Match(table.SelectSingleNode("./tr[4]/td[1]").InnerText.Split('：')[1]);
                if (match.Success)
                    profile.Birthday = DateTime.Parse($"{ (int.Parse(match.Result("$1")) + 1911) }-{ match.Result("$2") }-{ match.Result("$3") }");

                // Gender
                profile.Gender =
                    Enum.GetValues(typeof(Gender))
                    .OfType<Gender>()
                    .ToList()
                    .Find(gender => gender.ToString().Contains(table.SelectSingleNode("./tr[5]/td[1]").InnerText.Split('：')[1]));

                // Personal Picture
                string relativeUrl = table.SelectSingleNode("./tr[1]/td[3]/table/tr/td[2]/img").Attributes["src"].Value;
                request = new RestRequest($"kuas/{ relativeUrl.Substring(relativeUrl.IndexOf('/') + 1) }", Method.GET);
                profile.PersonalPicture = new MemoryStream(client.DownloadData(request));

                return profile;
            }
        }

        #endregion

        #region Get Option Value

        public List<SysValue> optionValueList { get; set; }

        public List<SysValue> GetOptionValueList(Student student)
        {
            optionValueList = new List<SysValue>();

            // Get webpage
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/system/sys001_00.jsp?spath=ag_pro/ag222.jsp", Method.POST);
            request.AddParameter("arg01", student.SysYear);
            request.AddParameter("arg02", student.SysSemester);
            request.AddParameter("arg03", student.Username);
            request.AddParameter("arg04", "");
            request.AddParameter("arg05", "");
            request.AddParameter("arg06", "");
            request.AddParameter("fncid", "AG222");
            request.AddParameter("uid", student.Username);
            request.AddParameter("ls_randnum", "00000");
            request.AddParameter("uid", student.Username);
            request.AddParameter("pwd", student.Password);

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            // Inner text of option tags are blank by default. Just add
            HtmlNode.ElementsFlags.Remove("option");
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));

            // Get innerText and value
            optionValueList.Clear();
            foreach (HtmlNode fnode in htmlDoc.DocumentNode.SelectNodes("//select[starts-with(@name, yms)]").Descendants("option"))
            {
                optionValueList.Add(new SysValue()
                {
                    SysYear = fnode.Attributes["value"].Value.Split(',')[0],
                    SysSemester = fnode.Attributes["value"].Value.Split(',')[1],
                    SysText = fnode.InnerText
                }
                );
            }
            return optionValueList;
        }

        #endregion

        #region Get Course List

        public List<Class> CourseList { get; set; }

        public List<List<Class>> Schedule { get; set; }

        public void GetCourseListAndSchedule(Student student, string year, string semester)
        {
            // Initialization
            CourseList = new List<Class>();
            Schedule = new List<List<Class>>();

            // Get webpage
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/ag_pro/ag222.jsp", Method.POST);
            request.AddParameter("yms", year + "," + semester);
            request.AddParameter("spath", "ag_pro/ag222.jsp?");
            request.AddParameter("arg01", year);
            request.AddParameter("arg02", semester);

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            // Convert html content (string) into HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));

            if (respHTML.Contains("無"))
            {
                // nothing
            }
            else
            {
                foreach (HtmlNode tr in htmlDoc.DocumentNode.SelectNodes("//body/form/table/tr").Skip(1))
                {
                    CourseList.Add(new Class()
                    {
                        CourseId = tr.SelectSingleNode("./td[1]/font[1]").InnerText,
                        SubjectChineseName = tr.SelectSingleNode("./td[2]/font[1]").InnerText,
                        ClassShortName = tr.SelectSingleNode("./td[3]/font[1]").InnerText,
                        Group = tr.SelectSingleNode("./td[4]/font[1]").InnerText,
                        PropertiesCredit = tr.SelectSingleNode("./td[5]/font[1]").InnerText,
                        TeachTime = tr.SelectSingleNode("./td[6]/font[1]").InnerText,
                        PropertiesRequiredOrElective = tr.SelectSingleNode("./td[7]/font[1]").InnerText,
                        ClassType = tr.SelectSingleNode("./td[8]/font[1]").InnerText,
                        Time = tr.SelectSingleNode("./td[9]/font[1]").InnerText,
                        Teachers = tr.SelectSingleNode("./td[10]/font[1]").InnerText,
                        Classroom = tr.SelectSingleNode("./td[11]/font[1]").InnerText,
                        SubjectYMS = year + "," + semester
                    });
                    CourseList.Last().PropertiesRequiredOrElective = CourseList.Last().PropertiesRequiredOrElective.TrimStart('【').TrimEnd('】');
                    CourseList.Last().ClassType = CourseList.Last().ClassType.TrimStart('【').TrimEnd('】');
                }

                // 課程列表
                Schedule = new List<List<Class>>();
                // 節次計數器
                int section = 0;
                foreach (HtmlNode tr in htmlDoc.DocumentNode.SelectNodes("//body/table/tr").Skip(1))
                {
                    // 節課程列表
                    List<Class> periodList = new List<Class>();
                    tr.SelectSingleNode("./td").Remove();
                    string[] sections = new string[] { "", "0910-0900", "0910-1000", "1010-1100", "1110-1200", "1200-1330", "1330-1420", "1430-1520", "1530-1620", "1630-1520", "1520-1830", "1830-1920", "1925-2015", "2020-2110", "2115-2205" };
                    foreach (HtmlNode snode in tr.SelectNodes("./td"))
                    {
                        // 當節資訊
                        Class addClass = new Class();

                        // info: [0] : subjectChtName, [1] : Teachers, [2] : Classroom
                        string[] info = snode.InnerHtml.Split(new string[] { "<br>" }, StringSplitOptions.None);
                        if (info.Count() >= 2)
                        {
                            try
                            {
                                string courseId = CourseList.First(x => x.SubjectChineseName.SequenceEqual(info[0]) && x.Teachers.SequenceEqual(info[1]) && x.Classroom.SequenceEqual(info[2])).CourseId;
                                addClass = new Class()
                                {
                                    CourseId = courseId,
                                    SubjectChineseName = CourseList.First(x => x.CourseId.SequenceEqual(courseId)).SubjectChineseName,
                                    ClassShortName = CourseList.First(x => x.CourseId.SequenceEqual(courseId)).ClassShortName,
                                    Group = CourseList.First(x => x.CourseId.SequenceEqual(courseId)).Group,
                                    PropertiesCredit = CourseList.First(x => x.CourseId.SequenceEqual(courseId)).PropertiesCredit,
                                    TeachTime = CourseList.First(x => x.CourseId.SequenceEqual(courseId)).TeachTime,
                                    PropertiesRequiredOrElective = CourseList.First(x => x.CourseId.SequenceEqual(courseId)).PropertiesRequiredOrElective,
                                    ClassType = CourseList.First(x => x.CourseId.SequenceEqual(courseId)).ClassType,
                                    Time = CourseList.First(x => x.CourseId.SequenceEqual(courseId)).Time,
                                    SectionTime = sections[section],
                                    Teachers = CourseList.First(x => x.CourseId.SequenceEqual(courseId)).Teachers,
                                    Classroom = CourseList.First(x => x.CourseId.SequenceEqual(courseId)).Classroom,
                                    SubjectYMS = year + "," + semester
                                };
                            }
                            catch (InvalidOperationException)
                            {
                                addClass = new Class()
                                {
                                    SubjectChineseName = info[0],
                                    Teachers = info[1],
                                    Classroom = info[2]
                                };
                            }
                        }
                        periodList.Add(addClass);
                    }
                    Schedule.Add(periodList);
                    section++;
                }
            }
        }

        #endregion

        #region Get Scores

        public ScoreList GetScores(Student student, string year, string semester)
        {
            ScoreList scoreList = new ScoreList();
            List<Score> scores = new List<Score>();

            // Get webpage
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/ag_pro/ag008.jsp", Method.POST);
            request.AddParameter("yms", year + "," + semester);
            request.AddParameter("spath", "ag_pro/ag008.jsp?");
            request.AddParameter("arg01", year);
            request.AddParameter("arg02", semester);

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            // Convert html content (string) into HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));

            if (respHTML.Contains("目前無學生個人成績資料"))
            {
                // nothing
            }
            else
            {
                foreach (HtmlNode tr in htmlDoc.DocumentNode.SelectNodes("//body/form/table/tr").Skip(1))
                {
                    scores.Add(new Score()
                    {
                        Index = int.Parse(tr.SelectSingleNode("./td[1]").InnerText.Trim(allWhiteSpace)),
                        SubjectChineseName = tr.SelectSingleNode("./td[2]").InnerText.Trim(allWhiteSpace),
                        PropertiesCredit = tr.SelectSingleNode("./td[3]").InnerText.Trim(allWhiteSpace),
                        TeachTime = tr.SelectSingleNode("./td[4]").InnerText.Trim(allWhiteSpace),
                        PropertiesRequiredOrElective = tr.SelectSingleNode("./td[5]").InnerText.Trim(allWhiteSpace),
                        ClassType = tr.SelectSingleNode("./td[6]").InnerText.Trim(allWhiteSpace),
                        MidtermScore = tr.SelectSingleNode("./td[7]").InnerText.Trim(allWhiteSpace),
                        SemesterScore = tr.SelectSingleNode("./td[8]").InnerText.Trim(allWhiteSpace),
                        Remark = tr.SelectSingleNode("./td[9]").InnerText.Trim(allWhiteSpace)
                    });
                    scores.Last().PropertiesRequiredOrElective = scores.Last().PropertiesRequiredOrElective.TrimStart('【').TrimEnd('】');
                    scores.Last().ClassType = scores.Last().ClassType.TrimStart('【').TrimEnd('】');
                }

                string[] values = htmlDoc.DocumentNode.SelectSingleNode("//caption[1]/div[1]").InnerHtml.
                    Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries).First().
                    Split(new string[] { "　　　　" }, StringSplitOptions.RemoveEmptyEntries);
                //"操行成績：90.50　　　　總平均：94.50　　　　班名次/班人數：1/57　　　　班名次百分比：1.75%"
                scoreList = new ScoreList()
                {
                    PersonalConduct = values[0].Split(new string[] { "：" }, StringSplitOptions.RemoveEmptyEntries).Last(),
                    AverageScore = values[1].Split(new string[] { "：" }, StringSplitOptions.RemoveEmptyEntries).Last(),
                    ClassRank = values[2].Split(new string[] { "：" }, StringSplitOptions.RemoveEmptyEntries).Last().Split('/').First(),
                    ClassSize = values[2].Split(new string[] { "：" }, StringSplitOptions.RemoveEmptyEntries).Last().Split('/').Last(),
                    ClassRankPercent = values[3].Split(new string[] { "：" }, StringSplitOptions.RemoveEmptyEntries).Last().Replace("%", ""),
                    Scores = scores
                };
            }
            return scoreList;
        }

        #endregion

        #region Get Miss Classes

        /// <summary>
        /// 
        /// </summary>
        /// <param name="std"></param>
        /// <param name="year"></param>
        /// <param name="semester"></param>
        /// <param name="fillFullSapce">表示當節數請假資料為空白時是否補上全形空白，預設為 false</param>
        /// <param name="dateAllInLine">表示日期是否皆在同一行，若否則會以 <br /> 表示換行，預設為 true</param>
        /// <returns></returns>
        public List<MissClass> GetMissClasses(Student student, string year, string semester, bool fillFullSapce = false, bool dateAllInLine = true)
        {
            List<MissClass> missClasses = new List<MissClass>();

            // Get webpage
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/ak_pro/ak002_01.jsp", Method.POST);
            request.AddParameter("yms", year + "," + semester);
            request.AddParameter("spath", "ag_pro/Fak002_01.jsp?");
            request.AddParameter("arg01", year);
            request.AddParameter("arg02", semester);

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            // Convert html content (string) into HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));

            if (respHTML.Contains("無"))
            {
                // nothing
            }
            else
            {
                foreach (HtmlNode tr in htmlDoc.DocumentNode.SelectNodes("//body/table[2]/tr").Skip(1))
                {
                    missClasses.Add(new MissClass()
                    {
                        Index = int.Parse(tr.SelectSingleNode("./td[1]").InnerText),
                        AbsenceFormId = tr.SelectSingleNode("./td[2]").InnerText,
                        Date = tr.SelectSingleNode("./td[3]").InnerText,
                        MorningMeeting = tr.SelectSingleNode("./td[4]").InnerText.Replace("缺", "").Replace("假", ""),
                        MorningSelfStudy = tr.SelectSingleNode("./td[5]").InnerText.Replace("缺", "").Replace("假", ""),
                        _1 = tr.SelectSingleNode("./td[6]").InnerText.Replace("缺", "").Replace("假", ""),
                        _2 = tr.SelectSingleNode("./td[7]").InnerText.Replace("缺", "").Replace("假", ""),
                        _3 = tr.SelectSingleNode("./td[8]").InnerText.Replace("缺", "").Replace("假", ""),
                        _4 = tr.SelectSingleNode("./td[9]").InnerText.Replace("缺", "").Replace("假", ""),
                        _A = tr.SelectSingleNode("./td[10]").InnerText.Replace("缺", "").Replace("假", ""),
                        _5 = tr.SelectSingleNode("./td[11]").InnerText.Replace("缺", "").Replace("假", ""),
                        _6 = tr.SelectSingleNode("./td[12]").InnerText.Replace("缺", "").Replace("假", ""),
                        _7 = tr.SelectSingleNode("./td[13]").InnerText.Replace("缺", "").Replace("假", ""),
                        _8 = tr.SelectSingleNode("./td[14]").InnerText.Replace("缺", "").Replace("假", ""),
                        _B = tr.SelectSingleNode("./td[15]").InnerText.Replace("缺", "").Replace("假", ""),
                        _11 = tr.SelectSingleNode("./td[16]").InnerText.Replace("缺", "").Replace("假", ""),
                        _12 = tr.SelectSingleNode("./td[17]").InnerText.Replace("缺", "").Replace("假", ""),
                        _13 = tr.SelectSingleNode("./td[18]").InnerText.Replace("缺", "").Replace("假", ""),
                        _14 = tr.SelectSingleNode("./td[19]").InnerText.Replace("缺", "").Replace("假", ""),
                    });

                    if (fillFullSapce)
                    {
                        missClasses.Last().MorningSelfStudy = string.IsNullOrEmpty(missClasses.Last().MorningSelfStudy) ? "　" : missClasses.Last().MorningSelfStudy;
                        missClasses.Last()._1 = string.IsNullOrEmpty(missClasses.Last()._1) ? "　" : missClasses.Last()._1;
                        missClasses.Last()._2 = string.IsNullOrEmpty(missClasses.Last()._2) ? "　" : missClasses.Last()._2;
                        missClasses.Last()._3 = string.IsNullOrEmpty(missClasses.Last()._3) ? "　" : missClasses.Last()._3;
                        missClasses.Last()._4 = string.IsNullOrEmpty(missClasses.Last()._4) ? "　" : missClasses.Last()._4;
                        missClasses.Last()._A = string.IsNullOrEmpty(missClasses.Last()._A) ? "　" : missClasses.Last()._A;
                        missClasses.Last()._5 = string.IsNullOrEmpty(missClasses.Last()._5) ? "　" : missClasses.Last()._5;
                        missClasses.Last()._6 = string.IsNullOrEmpty(missClasses.Last()._6) ? "　" : missClasses.Last()._6;
                        missClasses.Last()._7 = string.IsNullOrEmpty(missClasses.Last()._7) ? "　" : missClasses.Last()._7;
                        missClasses.Last()._8 = string.IsNullOrEmpty(missClasses.Last()._8) ? "　" : missClasses.Last()._8;
                        missClasses.Last()._B = string.IsNullOrEmpty(missClasses.Last()._B) ? "　" : missClasses.Last()._B;
                        missClasses.Last()._11 = string.IsNullOrEmpty(missClasses.Last()._11) ? "　" : missClasses.Last()._11;
                        missClasses.Last()._12 = string.IsNullOrEmpty(missClasses.Last()._12) ? "　" : missClasses.Last()._12;
                        missClasses.Last()._13 = string.IsNullOrEmpty(missClasses.Last()._13) ? "　" : missClasses.Last()._13;
                        missClasses.Last()._14 = string.IsNullOrEmpty(missClasses.Last()._14) ? "　" : missClasses.Last()._14;
                    }

                    if (!dateAllInLine)
                    {
                        missClasses.Last().Date = missClasses.Last().Date.Replace("/", "<br />");
                    }
                }
            }
            return missClasses;
        }

        #endregion

        #region 歷年成績報表

        public MemoryStream GetScoreReport(Student student)
        {
            // Get webpage
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/ag_pro/ag102.jsp", Method.POST);
            request.AddParameter("arg01", student.SysYear);
            request.AddParameter("arg02", student.SysSemester);
            request.AddParameter("arg03", student.Username);
            request.AddParameter("arg04", "");
            request.AddParameter("arg05", "");
            request.AddParameter("arg06", "");
            request.AddParameter("fncid", "AG102");
            request.AddParameter("uid", student.Username);
            request.AddParameter("ls_randnum", "");

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            try
            {
                // Convert html content (string) into HtmlDocument
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));
                // Get the report url without host
                string file = $"kuas/ag_pro/{ htmlDoc.DocumentNode.SelectSingleNode("//table[1]/tr[1]/td[1]/object[1]").Attributes["data"].Value.Substring(2) }";

                // Download the report
                request = new RestRequest(file);
                return new MemoryStream(client.DownloadData(request));
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        #endregion

        #region 期中預警資料

        public List<MidtermAlert> GetMidtermAlerts(Student student)
        {
            List<MidtermAlert> midtremAlerts = new List<MidtermAlert>();

            // Get webpage
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/ag_pro/ag009.jsp", Method.POST);
            request.AddParameter("arg01", student.SysYear);
            request.AddParameter("arg02", student.SysSemester);
            request.AddParameter("arg03", student.Username);
            request.AddParameter("arg04", "");
            request.AddParameter("arg05", "");
            request.AddParameter("arg06", "");
            request.AddParameter("fncid", "AG009");
            request.AddParameter("uid", student.Username);
            request.AddParameter("ls_randnum", "");

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            // Convert html content (string) into HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));

            if (htmlDoc.DocumentNode.SelectNodes("//table[2]/tr") != null)
            {
                foreach (HtmlNode tr in htmlDoc.DocumentNode.SelectNodes("//table[2]/tr").Skip(1))
                {
                    midtremAlerts.Add(new MidtermAlert()
                    {
                        Index = tr.SelectSingleNode("./td[1]").InnerText,
                        ClassName = tr.SelectSingleNode("./td[2]").InnerText,
                        SubjectChineseName = tr.SelectSingleNode("./td[3]").InnerText,
                        Group = tr.SelectSingleNode("./td[4]").InnerText,
                        Teachers = tr.SelectSingleNode("./td[5]").InnerText,
                        IsAlert = tr.SelectSingleNode("./td[6]").InnerText.Equals("是"),
                        AlertReason = tr.SelectSingleNode("./td[7]").InnerText,
                        Remark = tr.SelectSingleNode("./td[8]").InnerText
                    });
                }
            }
            return midtremAlerts;
        }

        #endregion

        #region 畢業預審報表

        public MemoryStream GetGraduationAuditReport(Student student)
        {
            // Download the report
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/ag_pro/ag650_01.jsp", Method.POST);
            return new MemoryStream(client.DownloadData(request));
        }

        #endregion

        #region 畢業門檻

        public GraduationThreshold GetGraduationThreshold(Student student)
        {
            GraduationThreshold gt = new GraduationThreshold();

            // Get webpage
            RestClient client = new RestClient("http://aength.kuas.edu.tw");
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("AUPersonQ.aspx", Method.POST);
            request.AddParameter("arg01", student.SysYear);
            request.AddParameter("arg02", student.SysSemester);
            request.AddParameter("arg03", student.Username);
            request.AddParameter("arg04", "");
            request.AddParameter("arg05", "");
            request.AddParameter("arg06", "");
            request.AddParameter("fncid", "AG635");
            request.AddParameter("ls_randnum", new Random().Next(1000, 9999).ToString());
            request.AddParameter("uid", student.Username);

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            // Convert html content (string) into HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));

            gt.StdBasicInfo = new StdBasicInfo()
            {
                ClassName = htmlDoc.DocumentNode.SelectSingleNode(@"//span[@id=""LabelclsName""]").InnerText,
                StdId = htmlDoc.DocumentNode.SelectSingleNode(@"//span[@id=""LabelStdId""]").InnerText,
                StdName = htmlDoc.DocumentNode.SelectSingleNode(@"//span[@id=""LabelQStdName""]").InnerText
            };
            gt.EngEngGradPass = htmlDoc.DocumentNode.SelectSingleNode(@"//span[@id=""LabelEngGradPass""]").InnerText;
            gt.EngTrainSta = htmlDoc.DocumentNode.SelectSingleNode(@"//span[@id=""LabelEngTrainSta""]").InnerText;

            gt.OcpClassList = new List<OcpClass>();
            if (htmlDoc.DocumentNode.SelectNodes(@"//table[@id=""GridView2""]/tr") != null)
            {
                foreach (HtmlNode tr in htmlDoc.DocumentNode.SelectNodes(@"//table[@id=""GridView2""]/tr").Skip(1))
                {
                    gt.OcpClassList.Add(new OcpClass()
                    {
                        SysYear = tr.SelectSingleNode("./td[1]").InnerText,
                        SysSemester = tr.SelectSingleNode("./td[2]").InnerText,
                        CourseId = tr.SelectSingleNode("./td[3]").InnerText,
                        SubjectChineseName = tr.SelectSingleNode("./td[4]").InnerText,
                        Score = tr.SelectSingleNode("./td[5]").InnerText
                    });
                }
            }
            return gt;
        }

        #endregion

        #region Payments_Receipts

        public List<string[]> GetPayments(Student student)
        {
            List<string[]> payments = new List<string[]>();

            // Get webpage
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/ay_pro/ay100_00.jsp", Method.POST);
            request.AddParameter("arg01", student.SysYear);
            request.AddParameter("arg02", student.SysSemester);
            request.AddParameter("arg03", student.Username);
            request.AddParameter("arg04", "");
            request.AddParameter("arg05", "");
            request.AddParameter("arg06", "");
            request.AddParameter("fncid", "AY100");
            request.AddParameter("uid", student.Username);
            request.AddParameter("ls_randnum", "");

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            // Convert html content (string) into HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));

            try
            {
                foreach (HtmlNode input in htmlDoc.DocumentNode.SelectNodes("//table[@id='table1']//input"))
                {
                    string label = htmlDoc.DocumentNode.SelectSingleNode($"//table[@id='table1']//label[@for='{ input.GetAttributeValue("id", "") }']").InnerText;
                    string radio = input.GetAttributeValue("onclick", "");
                    payments.Add(new string[]
                    {
                        label,
                        radio.Substring(radio.IndexOf('(') + 1, radio.IndexOf(')') - radio.IndexOf('(') - 1).Replace("'", "")
                    });
                }
            }
            catch (NullReferenceException) { }

            return payments;
        }

        public MemoryStream DownloadPayment(Student student, List<string> urlParams)
        {
            // 產生單據
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/ay_pro/ay100_pdf.jsp", Method.POST);
            request.AddParameter("chk_item", "");
            request.AddParameter("ls_untid", urlParams[5]);
            request.AddParameter("ls_dgrid", urlParams[4]);
            request.AddParameter("ls_clsid", urlParams[3]);
            request.AddParameter("ls_syear", urlParams[0]);
            request.AddParameter("ls_ssms", urlParams[1]);
            request.AddParameter("ls_serlno", urlParams[2]);

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            // Convert html content (string) into HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));

            if (htmlDoc.DocumentNode.SelectSingleNode("//body/input[@id='files']") != null)
            {
                // Get recepit download url
                string downloadUrl = 
                    $"kuas/ay_pro/show.jsp?file={ Uri.EscapeUriString(htmlDoc.DocumentNode.SelectSingleNode("//body/input[@id='files']").GetAttributeValue("value", "")) }";

                // Download
                request = new RestRequest(downloadUrl, Method.GET);
                return new MemoryStream(client.DownloadData(request));
            }
            else
            {
                return null;
            }
        }

        public List<string[]> GetRecepits(Student student)
        {
            List<string[]> recepits = new List<string[]>();

            // Get webpage
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/ay_pro/ay101_00.jsp", Method.POST);
            request.AddParameter("arg01", student.SysYear);
            request.AddParameter("arg02", student.SysSemester);
            request.AddParameter("arg03", student.Username);
            request.AddParameter("arg04", "");
            request.AddParameter("arg05", "");
            request.AddParameter("arg06", "");
            request.AddParameter("fncid", "AY101");
            request.AddParameter("uid", student.Username);
            request.AddParameter("ls_randnum", "");

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            // Convert html content (string) into HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));

            try
            {
                foreach (HtmlNode input in htmlDoc.DocumentNode.SelectNodes("//table[@id='table1']//input"))
                {
                    string label = htmlDoc.DocumentNode.SelectSingleNode($"//table[@id='table1']//label[@for='{ input.GetAttributeValue("id", "") }']").InnerText;
                    string radio = input.GetAttributeValue("onclick", "");
                    recepits.Add(new string[]
                    {
                        label,
                        radio.Substring(radio.IndexOf('(') + 1, radio.IndexOf(')') - radio.IndexOf('(') - 1).Replace("'", "")
                    });
                }
            }
            catch (NullReferenceException) { }

            return recepits;
        }

        public MemoryStream DownloadRecepit(Student student, List<string> urlParams)
        {
            // 產生單據
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/ay_pro/ay101_pdf.jsp", Method.POST);
            request.AddParameter("chk_item", "");
            request.AddParameter("ls_untid", urlParams[5]);
            request.AddParameter("ls_dgrid", urlParams[4]);
            request.AddParameter("ls_clsid", urlParams[3]);
            request.AddParameter("ls_syear", urlParams[0]);
            request.AddParameter("ls_ssms", urlParams[1]);
            request.AddParameter("ls_serlno", urlParams[2]);

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            // Convert html content (string) into HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));

            if (htmlDoc.DocumentNode.SelectSingleNode("//body/form") != null)
            {
                // Get recepit download url
                string downloadUrl =
                    $"kuas/ay_pro{ Uri.EscapeUriString(htmlDoc.DocumentNode.SelectSingleNode("//body/form").GetAttributeValue("action", "").TrimStart('.')) }";

                // Download
                request = new RestRequest(downloadUrl, Method.GET);
                return new MemoryStream(client.DownloadData(request));
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region ParlingPermits Payment

        public List<string[]> GetParlingPermitsPayments(Student student)
        {
            List<string[]> payments = new List<string[]>();

            // Get webpage
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/ck_pro/ck100_00.jsp", Method.POST);
            request.AddParameter("arg01", student.SysYear);
            request.AddParameter("arg02", student.SysSemester);
            request.AddParameter("arg03", student.Username);
            request.AddParameter("arg04", "");
            request.AddParameter("arg05", "");
            request.AddParameter("arg06", "");
            request.AddParameter("fncid", "CK100");
            request.AddParameter("uid", student.Username);
            request.AddParameter("ls_randnum", "");

            IRestResponse response = client.Execute(request);
            string respHTML = response.Content;

            // Convert html content (string) into HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respHTML.Replace("&nbsp;", ""));

            try
            {
                foreach (HtmlNode tr in htmlDoc.DocumentNode.SelectNodes("//table[@id='GridView1']/tr").Skip(1))
                {
                    if (tr.SelectSingleNode("./td[13]/input[1]").Attributes["value"].Value.Contains("列印收據"))
                    {
                        continue;
                    }
                    else
                    {
                        string label = string.Format("{0}學年 {1} ({2}{3})",
                            tr.SelectSingleNode("./td[1]").InnerText /* 學年度 */,
                            tr.SelectSingleNode("./td[8]").InnerText /* 費用名稱 */,
                            tr.SelectSingleNode("./td[4]").InnerText /* 車號一 */,
                            tr.SelectSingleNode("./td[5]").InnerText.Equals("-") ? "," + tr.SelectSingleNode("./td[5]").InnerText.Equals("=") : "");
                        string value = tr.SelectSingleNode("//input[2]").GetAttributeValue("value", "").Replace("§", ",");
                        payments.Add(new string[] { label, value });
                    }
                }
            }
            catch (NullReferenceException) { }

            return payments;
        }

        public MemoryStream DownloadParlingPermitsPayment(Student student, List<string> urlParams)
        {
            RestClient client = new RestClient(KUASAP_HOST_IN_USE);
            client.CookieContainer = new CookieContainer();
            student.Cookies.ForEach(cookie => client.CookieContainer.Add(cookie));

            RestRequest request = new RestRequest("kuas/ck_pro/ck100_print.jsp", Method.POST);
            request.AddParameter("h_year", urlParams[0]);
            request.AddParameter("h_feeid", urlParams[1]);
            request.AddParameter("h_type", urlParams[2]);
            request.AddParameter("h_whichprint", "print_demand");
            return new MemoryStream(client.DownloadData(request));
        }

        #endregion

        #region Auto FillIn Evaluation Form

        public List<EvaluationItem> ListEvaluationForm(string username, string password)
        {
            RestClient client;
            CookieContainer cookieContainer = new CookieContainer();
            RestRequest request;
            IRestResponse response;
            string respHTML;
            HtmlDocument htmlDoc = new HtmlDocument();
            List<EvaluationItem> evaluationItems = new List<EvaluationItem>();

            client = new RestClient(KUAS_SELECTED_COURSE_SYSTEM);
            client.CookieContainer = cookieContainer;

            request = new RestRequest("/Account/LogOn?ReturnUrl=%2f", Method.POST);
            request.AddParameter("UserName", username);
            request.AddParameter("Password", password);
            response = client.Execute(request);
            respHTML = response.Content;

            request = new RestRequest("/Extjs/GetJavascript", Method.POST);
            request.AddParameter("ServicePoint", "load");
            request.AddParameter("moduleId", "kuas_questionnaire");
            response = client.Execute(request);
            respHTML = response.Content;

            request = new RestRequest("/Questionnaire/Process/BrowseCheck", Method.GET);
            response = client.Execute(request);
            respHTML = response.Content;

            JObject jObject = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(respHTML);
            if (jObject["isOpen"] == null /* 無法填寫 */ || !(bool)jObject["isOpen"] /* 未開放填寫 */ ) { return null; }

            client = new RestClient(KUAS_EVALUATION_SYSTEM);
            client.CookieContainer = cookieContainer;

            request = new RestRequest("/Std/QuestionnaireInsert.aspx", Method.GET);
            request.AddQueryParameter("UserId", jObject["message"].ToString());
            response = client.Execute(request);
            respHTML = response.Content;
            htmlDoc.LoadHtml(respHTML);

            foreach (string id in new string[] { "gvInProgress", "GridViewFinalQuestion" })
            {
                try
                {
                    foreach (HtmlNode tr in htmlDoc.DocumentNode.SelectNodes($@"//*[@id=""{id}""]/tr").Skip(1))
                    {
                        // get subject chinese name and teachers
                        HtmlNode course = tr.SelectSingleNode(".//a");
                        string
                            innerText = course.InnerText.Replace("(已完成-Completed)", ""),
                            subjectChineseName = "",
                            teachers = "";

                        // split innerText to { teachers } and { subjectChineseName }
                        for (int i = 0; i < innerText.Length; i++)
                        {
                            teachers += innerText[i];
                            if (System.Text.RegularExpressions.Regex.IsMatch($"{ innerText[i] }{ innerText[i + 1] }", @"[)]+[\u4e00-\u9fa5]"))
                            {
                                subjectChineseName = innerText.Substring(i + 1, innerText.Length - (i + 1));
                                break;
                            }
                        }

                        // match chinese only
                        teachers = teachers.Split('(')[0];
                        subjectChineseName = subjectChineseName.Split('(')[0];

                        // get hidden input name and value
                        List<Parameter> parameters = new List<Parameter>();
                        HtmlNodeCollection inputs = tr.SelectNodes(".//input");
                        foreach (HtmlNode input in inputs)
                        {
                            parameters.Add(new Parameter()
                            {
                                Name = input.Attributes["name"].Value,
                                Value = input.Attributes["value"].Value,
                                Type = ParameterType.GetOrPost
                            });
                        }

                        // add item
                        evaluationItems.Add(new EvaluationItem()
                        {
                            Title = innerText,
                            SubjectChineseName = subjectChineseName,
                            Teachers = teachers,
                            EvaliationType = id.SequenceEqual("gvInProgress") ? EvaliationType.期中評量 : EvaliationType.期末評量,
                            Done = course.InnerText.Contains("已完成"),
                            HiddenInputs = parameters
                        });
                    }
                }
                catch (ArgumentNullException) { }
            }

            return evaluationItems;
        }

        public List<EvaluationItem> FillInMidtermEvaluationForm(string username, string password, Evaluation evaluation)
        {
            RestClient client;
            CookieContainer cookieContainer = new CookieContainer();
            RestRequest request;
            List<Parameter> parameters = new List<Parameter>();
            IRestResponse response;
            string respHTML;
            HtmlDocument htmlDoc = new HtmlDocument();

            client = new RestClient(KUAS_SELECTED_COURSE_SYSTEM);
            client.CookieContainer = cookieContainer;

            request = new RestRequest("/Account/LogOn?ReturnUrl=%2f", Method.POST);
            request.AddParameter("UserName", username);
            request.AddParameter("Password", password);
            response = client.Execute(request);
            respHTML = response.Content;

            request = new RestRequest("/Extjs/GetJavascript", Method.POST);
            request.AddParameter("ServicePoint", "load");
            request.AddParameter("moduleId", "kuas_questionnaire");
            response = client.Execute(request);
            respHTML = response.Content;

            request = new RestRequest("/Questionnaire/Process/BrowseCheck", Method.GET);
            response = client.Execute(request);
            respHTML = response.Content;
            JObject jObject = (JObject)JsonConvert.DeserializeObject(respHTML);
            if (jObject["isOpen"] == null /* 無法填寫 */ || !(bool)jObject["isOpen"] /* 未開放填寫 */ ) { return null; }

            string userId = jObject["message"].ToString();
            string courseName = "";
            List<EvaluationItem> doneList = new List<EvaluationItem>();
            while (true)
            {
                client = new RestClient(KUAS_EVALUATION_SYSTEM);
                client.CookieContainer = cookieContainer;

                request = new RestRequest("/Std/QuestionnaireInsert.aspx", Method.GET);
                request.AddQueryParameter("UserId", userId);
                response = client.Execute(request);
                respHTML = response.Content;
                htmlDoc.LoadHtml(respHTML);

                string __EVENTTARGET = null;
                string __VIEWSTATE = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__VIEWSTATE""]").Attributes["value"].Value);
                string __VIEWSTATEGENERATOR = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__VIEWSTATEGENERATOR""]").Attributes["value"].Value);
                string __EVENTVALIDATION = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__EVENTVALIDATION""]").Attributes["value"].Value);

                if (htmlDoc.DocumentNode.SelectNodes(@"//*[@id=""gvInProgress""]/tr") == null) break;
                foreach (HtmlNode tr in htmlDoc.DocumentNode.SelectNodes(@"//*[@id=""gvInProgress""]/tr").Skip(1))
                {
                    HtmlNode course = tr.SelectSingleNode(".//a");
                    if ((course.InnerText.Contains("已完成")) ||
                        (!evaluation.Subject.Equals("AllEvaluation") && !evaluation.Subject.Equals(course.InnerText))) { continue; }
                    else { courseName = course.InnerText; }

                    string href = course.Attributes["href"].Value;
                    int startPos = href.IndexOf("__doPostBack(&#39;", StringComparison.CurrentCultureIgnoreCase) + 18,
                        endPos = href.IndexOf("&#39", startPos, StringComparison.CurrentCultureIgnoreCase);
                    __EVENTTARGET = href.Substring(startPos, endPos - startPos);
                    foreach (HtmlNode input in tr.SelectNodes(".//input"))
                    {
                        string name = input.Attributes["name"].Value;
                        string value = input.Attributes["value"].Value;
                        parameters.Add(new Parameter() { Name = name, Value = value });
                    }
                    break;
                }

                if (string.IsNullOrEmpty(__EVENTTARGET)) { break; }

                request = new RestRequest("/Std/QuestionnaireInsert.aspx", Method.POST);
                parameters.ForEach(x => request.AddParameter(x.Name, x.Value)); parameters.Clear();
                request.AddQueryParameter("UserId", userId);
                request.AddParameter("__EVENTTARGET", __EVENTTARGET);
                request.AddParameter("__EVENTARGUMENT", "");
                request.AddParameter("__VIEWSTATE", __VIEWSTATE);
                request.AddParameter("__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR);
                request.AddParameter("__EVENTVALIDATION", __EVENTVALIDATION);
                response = client.Execute(request);
                respHTML = response.Content;
                htmlDoc.LoadHtml(respHTML);

                __VIEWSTATE = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__VIEWSTATE""]").Attributes["value"].Value);
                __VIEWSTATEGENERATOR = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__VIEWSTATEGENERATOR""]").Attributes["value"].Value);
                __EVENTVALIDATION = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__EVENTVALIDATION""]").Attributes["value"].Value);

                Console.WriteLine("----------------- Start -----------------");

                foreach (HtmlNode input in htmlDoc.DocumentNode.SelectNodes(@"//*[@id=""PanelMidQuestionnaire""]/table/tr/td[2]/input"))
                {
                    string name = input.Attributes["name"].Value;
                    string value = input.Attributes["value"].Value;
                    parameters.Add(new Parameter() { Name = name, Value = value });
                }

                int questionCounter = 0;
                Dictionary<FavoriteRank, int[]> questionAnswers = new Dictionary<FavoriteRank, int[]>()
                {
                    /*
                                                         { 1, 2, 3, 4, 5, 6, 7, 9,10,11,12,13,14 }
                     */
                    { FavoriteRank.極優, new int[] { 3, 1, 1, 1, 3, 3, 1, 1, 1, 1, 5, 1, 5 } },
                    { FavoriteRank.優良, new int[] { 3, 1, 1, 2, 3, 3, 1, 2, 2, 1, 4, 2, 4 } },
                    { FavoriteRank.普通, new int[] { 3, 2, 2, 3, 3, 3, 1, 3, 3, 2, 3, 3, 3 } },
                    { FavoriteRank.差勁, new int[] { 4, 3, 4, 4, 2, 2, 2, 4, 4, 2, 2, 4, 2 } },
                    { FavoriteRank.極差, new int[] { 4, 4, 4, 5, 2, 1, 5, 5, 5, 3, 1, 5, 1 } }
                };

                foreach (HtmlNode tr in htmlDoc.DocumentNode.SelectNodes(@"//*[@id=""GridViewContent""]/tr").Skip(1))
                {
                    HtmlNode td = tr.SelectSingleNode(".//td[2]/font");
                    foreach (HtmlNode input in td.SelectNodes("./input"))
                    {
                        string name = input.Attributes["name"].Value;
                        string value = input.Attributes["value"].Value;
                        parameters.Add(new Parameter() { Name = name, Value = value });
                    }

                    if (tr.InnerHtml.Contains("textarea"))
                    {
                        string name = td.SelectSingleNode(".//textarea").Attributes["name"].Value;
                        string value = "";
                        parameters.Add(new Parameter() { Name = name, Value = value });
                    }
                    else if (tr.InnerHtml.Contains("checkbox"))
                    {
                        string name = "", value = "";
                        switch (evaluation.FavoriteRank)
                        {
                            case FavoriteRank.優良:
                            case FavoriteRank.普通:
                                HtmlNode input = td.SelectSingleNode(".//span/input");
                                name = input.Attributes["name"].Value;
                                value = questionAnswers[evaluation.FavoriteRank][questionCounter++].ToString();
                                parameters.Add(new Parameter() { Name = name, Value = value });
                                break;
                            case FavoriteRank.差勁:
                                int counter = 2;
                                foreach (HtmlNode checkbox in td.SelectNodes(".//span/input").Skip(1))
                                {
                                    name = checkbox.Attributes["name"].Value;
                                    value = counter++.ToString();
                                    parameters.Add(new Parameter() { Name = name, Value = value });
                                }
                                break;
                        }
                    }
                    else
                    {
                        HtmlNode input = td.SelectSingleNode(".//span/input");
                        string name = input.Attributes["name"].Value;
                        string value = questionAnswers[evaluation.FavoriteRank][questionCounter++].ToString();
                        parameters.Add(new Parameter() { Name = name, Value = value });
                    }
                }

                request = new RestRequest("/Std/QuestionnaireInsert.aspx", Method.POST);
                parameters.ForEach(x => request.AddParameter(x.Name, x.Value)); parameters.Clear();
                request.AddQueryParameter("UserId", userId);
                request.AddParameter("__EVENTTARGET", "");
                request.AddParameter("__EVENTARGUMENT", "");
                request.AddParameter("__VIEWSTATE", __VIEWSTATE);
                request.AddParameter("__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR);
                request.AddParameter("__EVENTVALIDATION", __EVENTVALIDATION);
                request.AddParameter("ButtonSend", "送出(Send)");
                response = client.Execute(request);
                respHTML = response.Content;
                htmlDoc.LoadHtml(respHTML);

                if (respHTML.Contains("存檔成功")) doneList.Add(
                    new EvaluationItem()
                    {
                        SubjectChineseName = courseName,
                        EvaliationType = EvaliationType.期中評量
                    });
                courseName = "";
            }
            return doneList;
        }

        public List<EvaluationItem> FillInFinalEvaluationForm(string username, string password, Evaluation evaluation)
        {
            int ctl02 = 1;
            int ctl04 = 1;
            int ctl05 = 1;
            int ctl06 = 1;
            int answer = 5 - (int)evaluation.FavoriteRank;

            switch (evaluation.Gender)
            {
                case Gender.男性: ctl02 = 1; break;
                case Gender.女性: ctl02 = 2; break;
            }
            switch (evaluation.College)
            {
                case College.電資學院: ctl04 = 1; break;
                case College.工學院: ctl04 = 2; break;
                case College.人文社會學院: ctl04 = 3; break;
                case College.管理學院: ctl04 = 4; break;
            }
            switch (evaluation.EductionalSystem)
            {
                case EductionalSystem.日間部二技: ctl05 = 1; break;
                case EductionalSystem.日間部四技: ctl05 = 2; break;
                case EductionalSystem.日間部碩士班: ctl05 = 3; break;
                case EductionalSystem.日間部博士班: ctl05 = 4; break;
                case EductionalSystem.進推處二技: ctl05 = 5; break;
                case EductionalSystem.進推處四技: ctl05 = 6; break;
                case EductionalSystem.進推處碩專班: ctl05 = 7; break;
                case EductionalSystem.進推處產碩班: ctl05 = 8; break;
                case EductionalSystem.進修學院二技: ctl05 = 9; break;
            }
            switch (evaluation.Grade)
            {
                case Grade.一年級: ctl06 = 1; break;
                case Grade.二年級: ctl06 = 2; break;
                case Grade.三年級: ctl06 = 3; break;
                case Grade.四年級: ctl06 = 4; break;
                case Grade.大學延畢: ctl06 = 5; break;
            }

            RestClient client;
            CookieContainer cookieContainer = new CookieContainer();
            List<Parameter> parameters = new List<Parameter>();
            RestRequest request;
            IRestResponse response;
            string respHTML;
            HtmlDocument htmlDoc = new HtmlDocument();

            client = new RestClient(KUAS_SELECTED_COURSE_SYSTEM);
            client.CookieContainer = cookieContainer;

            request = new RestRequest("/Account/LogOn?ReturnUrl=%2f", Method.POST);
            request.AddParameter("UserName", username);
            request.AddParameter("Password", password);
            response = client.Execute(request);
            respHTML = response.Content;

            request = new RestRequest("/Extjs/GetJavascript", Method.POST);
            request.AddParameter("ServicePoint", "load");
            request.AddParameter("moduleId", "kuas_questionnaire");
            response = client.Execute(request);
            respHTML = response.Content;

            request = new RestRequest("/Questionnaire/Process/BrowseCheck", Method.GET);
            response = client.Execute(request);
            respHTML = response.Content;
            JObject jObject = (JObject)JsonConvert.DeserializeObject(respHTML);
            if (jObject["isOpen"] == null /* 無法填寫 */ || !(bool)jObject["isOpen"] /* 未開放填寫 */ ) { return null; }

            string userId = jObject["message"].ToString();
            string courseName = "";
            List<EvaluationItem> doneList = new List<EvaluationItem>();
            while (true)
            {
                client = new RestClient(KUAS_EVALUATION_SYSTEM);

                request = new RestRequest("/Std/QuestionnaireInsert.aspx", Method.GET);
                request.AddQueryParameter("UserId", userId);
                response = client.Execute(request);
                respHTML = response.Content;
                htmlDoc.LoadHtml(respHTML);

                string __VIEWSTATE = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__VIEWSTATE""]").Attributes["value"].Value);
                string __VIEWSTATEGENERATOR = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__VIEWSTATEGENERATOR""]").Attributes["value"].Value);
                string __EVENTVALIDATION = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__EVENTVALIDATION""]").Attributes["value"].Value);
                string __EVENTTARGET = null;

                if (htmlDoc.DocumentNode.SelectNodes(@"//*[@id=""GridViewFinalQuestion""]/tr") == null) break;
                foreach (HtmlNode tr in htmlDoc.DocumentNode.SelectNodes(@"//*[@id=""GridViewFinalQuestion""]/tr").Skip(1))
                {
                    HtmlNode course = tr.SelectSingleNode(".//a");
                    if ((course.InnerText.Contains("已完成")) ||
                        (!evaluation.Subject.Equals("AllEvaluation") && !evaluation.Subject.Equals(course.InnerText))) { continue; }
                    else { courseName = course.InnerText; }

                    string href = course.Attributes["href"].Value;
                    int startPos = href.IndexOf("__doPostBack(&#39;", StringComparison.CurrentCultureIgnoreCase) + 18,
                        endPos = href.IndexOf("&#39", startPos, StringComparison.CurrentCultureIgnoreCase);
                    __EVENTTARGET = href.Substring(startPos, endPos - startPos);
                    foreach (HtmlNode input in tr.SelectNodes(".//input"))
                    {
                        string name = input.Attributes["name"].Value;
                        string value = input.Attributes["value"].Value;
                        parameters.Add(new Parameter() { Name = name, Value = value });
                    }
                    break;
                }

                if (string.IsNullOrEmpty(__EVENTTARGET)) { break; }

                request = new RestRequest("/Std/QuestionnaireInsert.aspx", Method.POST);
                parameters.ForEach(x => request.AddParameter(x.Name, x.Value)); parameters.Clear();
                request.AddQueryParameter("UserId", userId);
                request.AddParameter("__EVENTTARGET", __EVENTTARGET);
                request.AddParameter("__EVENTARGUMENT", "");
                request.AddParameter("__VIEWSTATE", __VIEWSTATE);
                request.AddParameter("__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR);
                request.AddParameter("__EVENTVALIDATION", __EVENTVALIDATION);
                response = client.Execute(request);
                respHTML = response.Content;
                htmlDoc.LoadHtml(respHTML);

                __VIEWSTATE = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__VIEWSTATE""]").Attributes["value"].Value);
                __VIEWSTATEGENERATOR = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__VIEWSTATEGENERATOR""]").Attributes["value"].Value);
                __EVENTVALIDATION = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__EVENTVALIDATION""]").Attributes["value"].Value);

                Console.WriteLine("-------------- Start Part1 --------------");

                foreach (HtmlNode input in htmlDoc.DocumentNode.SelectNodes(@"//*[@id=""PanelFinalQuestionnaire""]/table/tr/td[1]/input"))
                {
                    string name = input.Attributes["name"].Value;
                    string value = input.Attributes["value"].Value;
                    parameters.Add(new Parameter() { Name = name, Value = value });
                }

                int questionCounter = 0;
                foreach (HtmlNode tr in htmlDoc.DocumentNode.SelectNodes(@"//*[@id=""GridViewGroup1""]/tr").Skip(1))
                {
                    HtmlNode td = tr.SelectSingleNode(".//td[2]/font");
                    foreach (HtmlNode input in td.SelectNodes("./input"))
                    {
                        string name = input.Attributes["name"].Value;
                        string value = input.Attributes["value"].Value;
                        parameters.Add(new Parameter() { Name = name, Value = value });
                    }

                    if (tr != htmlDoc.DocumentNode.SelectNodes(@"//*[@id=""GridViewGroup1""]/tr").Last())
                    {
                        HtmlNode input = td.SelectSingleNode(".//span/input");
                        string name = input.Attributes["name"].Value;
                        string value = "";
                        switch (questionCounter++)
                        {
                            case 0: value = ctl02.ToString(); break;
                            case 1: value = ctl04.ToString(); break;
                            case 2: value = ctl05.ToString(); break;
                            case 3: value = ctl06.ToString(); break;
                            default: value = answer.ToString(); break;
                        }
                        parameters.Add(new Parameter() { Name = name, Value = value });
                    }
                    else
                    {
                        string name = td.SelectSingleNode(".//textarea").Attributes["name"].Value;
                        string value = "";
                        parameters.Add(new Parameter() { Name = name, Value = value });
                    }
                }

                request = new RestRequest("/Std/QuestionnaireInsert.aspx", Method.POST);
                parameters.ForEach(x => request.AddParameter(x.Name, x.Value)); parameters.Clear();
                request.AddQueryParameter("UserId", userId);
                request.AddParameter("__EVENTTARGET", "");
                request.AddParameter("__EVENTARGUMENT", "");
                request.AddParameter("__VIEWSTATE", __VIEWSTATE);
                request.AddParameter("__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR);
                request.AddParameter("__EVENTVALIDATION", __EVENTVALIDATION);
                request.AddParameter("ButtonNextPart", "第二部分(Second Part)");
                response = client.Execute(request);
                respHTML = response.Content;
                htmlDoc.LoadHtml(respHTML);

                __VIEWSTATE = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__VIEWSTATE""]").Attributes["value"].Value);
                __VIEWSTATEGENERATOR = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__VIEWSTATEGENERATOR""]").Attributes["value"].Value);
                __EVENTVALIDATION = WebUtility.HtmlDecode(htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""__EVENTVALIDATION""]").Attributes["value"].Value);

                Console.WriteLine("-------------- Start Part2 --------------");

                foreach (HtmlNode input in htmlDoc.DocumentNode.SelectNodes(@"//*[@id=""PanelFinalQuestionnaire""]/table/tr/td[1]/input"))
                {
                    string name = input.Attributes["name"].Value;
                    string value = input.Attributes["value"].Value;
                    parameters.Add(new Parameter() { Name = name, Value = value });
                }

                foreach (HtmlNode tr in htmlDoc.DocumentNode.SelectNodes(@"//*[@id=""GridViewGroup2""]/tr").Skip(1))
                {
                    string name = "", value = "";
                    HtmlNode td = tr.SelectSingleNode(".//td[2]/font");
                    foreach (HtmlNode input in td.SelectNodes("./input"))
                    {
                        name = input.Attributes["name"].Value;
                        value = input.Attributes["value"].Value;
                        parameters.Add(new Parameter() { Name = name, Value = value });
                    }
                    name = td.SelectSingleNode("./span/input").Attributes["name"].Value;
                    value = answer.ToString();
                    parameters.Add(new Parameter() { Name = name, Value = value });
                }

                request = new RestRequest("/Std/QuestionnaireInsert.aspx", Method.POST);
                parameters.ForEach(x => request.AddParameter(x.Name, x.Value)); parameters.Clear();
                request.AddParameter("UserId", userId);
                request.AddParameter("__EVENTTARGET", "");
                request.AddParameter("__EVENTARGUMENT", "");
                request.AddParameter("__VIEWSTATE", __VIEWSTATE);
                request.AddParameter("__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR);
                request.AddParameter("__EVENTVALIDATION", __EVENTVALIDATION);
                request.AddParameter("Button1", "送出(Send)");
                response = client.Execute(request);
                respHTML = response.Content;

                if (respHTML.Contains("存檔成功")) doneList.Add(
                    new EvaluationItem()
                    {
                        SubjectChineseName = courseName,
                        EvaliationType = EvaliationType.期末評量
                    });
                courseName = "";
            }
            return doneList;
        }

        #endregion
    }
}