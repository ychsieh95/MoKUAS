using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using MoKUAS.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoKUAS.Pages.Courses
{
    public class OverviewModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SubjectChineseName { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ClassShortName { get; set; }
        [BindProperty(SupportsGet = true)]
        public string Teachers { get; set; }
        [BindProperty(SupportsGet = true)]
        public bool ByKeyword { get; set; }

        public DeviceExtensions.DeviceType DeviceType { get; private set; }
        public Models.Overview Overview { get; private set; }

        private readonly Models.AppSettings appSettings;

        public OverviewModel(IOptions<Models.AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public async Task OnGetAsync()
        {
            // User device type
            DeviceType = Request.Headers["User-Agent"].ToString().GetDeviceType();

            // 若搜尋條件為 null (直接點擊分析) 則預設為關鍵字搜尋
            bool paramsNull = new string[] { SubjectChineseName, ClassShortName, Teachers }.All(str => str == null);
            ByKeyword = paramsNull ? true : ByKeyword;

            // Repositorys
            var classRepository = new Repositorys.ClassRepository(appSettings.ConnectionStrings.DefaultConnection);
            var commentRepository = new Repositorys.CommentRepository(appSettings.ConnectionStrings.DefaultConnection);

            // 所有相關評論
            var comments = new List<Models.Comment>();

            // 取得指定過濾條件的相關評論
            if (!paramsNull)
                foreach (var @class in (await classRepository.Select(@class: new Models.Class() { SubjectChineseName = SubjectChineseName, Teachers = Teachers, Classroom = ClassShortName }, sqlLike: ByKeyword)))
                    comments.AddRange((await commentRepository.Select(comment: new Models.Comment() { ClassGuid = @class.Guid })));
            else if (ByKeyword)
                foreach (var @class in (await classRepository.Select(@class: null)))
                    comments.AddRange((await commentRepository.Select(comment: new Models.Comment() { ClassGuid = @class.Guid })));

            // 若無回應物件
            if (comments.Count == 0)
            {
                Overview = new Models.Overview()
                {
                    ClassInfo = new Models.Class(),
                    CommentCount = 0m,
                    TeachingMethods = new Models.TeachingMethods(),
                    RollCallMethods = new Models.RollCallMethods(),
                    ClassworkExam = new Models.ClassworkExam(),
                    Grades = new Models.Grades()
                };
                ModelState.AddModelError("Error", "無查詢結果");
            }
            else
            {
                Overview = new Models.Overview()
                {
                    ClassInfo = new Models.Class()
                    {
                        SubjectChineseName = SubjectChineseName,
                        Teachers = Teachers,
                        ClassShortName = ClassShortName,
                    },
                    CommentCount = comments.Count,
                    /* 以下百分比小數取到第二位 (Math.Floor(X * 10000) * 100) */
                    TeachingMethods = new Models.TeachingMethods()
                    {
                        IsBlackboard = comments.Count(comment => comment.IsBlackboard),
                        IsBlackboardPercent = Math.Floor((decimal)comments.Count(comment => comment.IsBlackboard) / comments.Count * 10000) / 100,
                        IsBook = comments.Count(comment => comment.IsBook),
                        IsBookPercent = Math.Floor((decimal)comments.Count(comment => comment.IsBook) / comments.Count * 10000) / 100,
                        IsPPT = comments.Count(comment => comment.IsPPT),
                        IsPPTPercent = Math.Floor((decimal)comments.Count(comment => comment.IsPPT) / comments.Count * 10000) / 100,
                        IsBroadcast = comments.Count(comment => comment.IsBroadcast),
                        IsBroadcastPercent = Math.Floor((decimal)comments.Count(comment => comment.IsBroadcast) / comments.Count * 10000) / 100,
                        IsBuild = comments.Count(comment => comment.IsBuild),
                        IsBuildPercent = Math.Floor((decimal)comments.Count(comment => comment.IsBuild) / comments.Count * 10000) / 100,
                        IsInteractive = comments.Count(comment => comment.IsInteractive),
                        IsInteractivePercent = Math.Floor((decimal)comments.Count(comment => comment.IsInteractive) / comments.Count * 10000) / 100
                    },
                    RollCallMethods = new Models.RollCallMethods()
                    {
                        RollCallFrequency = comments.Sum(comment => comment.RollCallFrequency) / comments.Count,
                        RollCallFrequencyPercent = Math.Floor(comments.Sum(comment => comment.RollCallFrequency) / (comments.Count * 5) * 100 + (decimal)0.5),
                        ByInPerson = comments.Count(comment => comment.ByInPerson),
                        ByInPersonPercent = Math.Floor((decimal)comments.Count(comment => comment.ByInPerson) / comments.Count * 10000) / 100,
                        BySignInSheet = comments.Count(comment => comment.BySignInSheet),
                        BySignInSheetPercent = Math.Floor((decimal)comments.Count(comment => comment.BySignInSheet) / comments.Count * 10000) / 100,
                        ByOnline = comments.Count(comment => comment.ByOnline),
                        ByOnlinePercent = Math.Floor((decimal)comments.Count(comment => comment.ByOnline) / comments.Count * 10000) / 100,
                        ByClasswork = comments.Count(comment => comment.ByClasswork),
                        ByClassworkPercent = Math.Floor((decimal)comments.Count(comment => comment.ByClasswork) / comments.Count * 10000) / 100,
                        ByTest = comments.Count(comment => comment.ByTest),
                        ByTestPercent = Math.Floor((decimal)comments.Count(comment => comment.ByTest) / comments.Count * 10000) / 100
                    },
                    ClassworkExam = new Models.ClassworkExam()
                    {
                        HaveClasswork = comments.Count(comment => comment.HaveClasswork),
                        HaveClassworkPercent = Math.Floor((decimal)comments.Count(comment => comment.HaveClasswork) / comments.Count * 10000) / 100,
                        HaveTest = comments.Count(comment => comment.HaveTest),
                        HaveTestPercent = Math.Floor((decimal)comments.Count(comment => comment.HaveTest) / comments.Count * 10000) / 100,
                        HaveMidtermExam = comments.Count(comment => comment.HaveMidtermExam),
                        HaveMidtermExamPercent = Math.Floor((decimal)comments.Count(comment => comment.HaveMidtermExam) / comments.Count * 10000) / 100,
                        HaveFinalExam = comments.Count(comment => comment.HaveFinalExam),
                        HaveFinalExamPercent = Math.Floor((decimal)comments.Count(comment => comment.HaveFinalExam) / comments.Count * 10000) / 100
                    },
                    Grades = new Models.Grades()
                    {
                        Grade1 = comments.Count(comment => comment.Grade == 1),
                        Grade1Percent = Math.Floor((decimal)comments.Count(comment => comment.Grade == 1) / (comments.Count == 0 ? 1 : comments.Count) * 10000) / 100,
                        Grade2 = comments.Count(comment => comment.Grade == 2),
                        Grade2Percent = Math.Floor((decimal)comments.Count(comment => comment.Grade == 2) / (comments.Count == 0 ? 1 : comments.Count) * 10000) / 100,
                        Grade3 = comments.Count(comment => comment.Grade == 3),
                        Grade3Percent = Math.Floor((decimal)comments.Count(comment => comment.Grade == 3) / (comments.Count == 0 ? 1 : comments.Count) * 10000) / 100,
                        Grade4 = comments.Count(comment => comment.Grade == 4),
                        Grade4Percent = Math.Floor((decimal)comments.Count(comment => comment.Grade == 4) / (comments.Count == 0 ? 1 : comments.Count) * 10000) / 100,
                        Grade5 = comments.Count(comment => comment.Grade == 5),
                        Grade5Percent = Math.Floor((decimal)comments.Count(comment => comment.Grade == 5) / (comments.Count == 0 ? 1 : comments.Count) * 10000) / 100,
                        GradeAverage = comments.Sum(comment => comment.Grade) / comments.Count
                    }
                };
            }   
        }
    }
}