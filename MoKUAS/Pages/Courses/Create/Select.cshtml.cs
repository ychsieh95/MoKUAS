using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoKUAS.Pages.Courses.Create
{
    public class SelectModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SysValue { get; set; }
        [BindProperty]
        public Models.Class Class { get; set; }

        public List<SelectListItem> SysValues { get; private set; }
        public List<Models.Class> CourseList { get; private set; }
        public List<List<Models.Class>> Schedule { get; private set; }

        private readonly Models.AppSettings appSettings;

        public SelectModel(IOptions<Models.AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public async Task OnGetAsync()
        {
            // Message by redirect
            if (TempData.ContainsKey("Error"))
                ModelState.AddModelError("Error", TempData["Error"].ToString());

            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();

            // Set SysValue default value
            if (string.IsNullOrEmpty(SysValue))
                SysValue = $"{ student.SysYear },{ student.SysSemester }";

            // DropDownList items
            SysValues = new List<SelectListItem>();
            kuasAp.GetOptionValueList(student: student).ForEach(sysItem =>
                SysValues.Add(new SelectListItem()
                {
                    Text = sysItem.SysText,
                    Value = $"{ sysItem.SysYear },{ sysItem.SysSemester }",
                    Selected = string.IsNullOrEmpty(SysValue) ?
                               (sysItem.SysYear == student.SysYear && sysItem.SysSemester == student.SysSemester) :
                               (sysItem.SysYear == SysValue.Split(',')[0] && sysItem.SysSemester == SysValue.Split(',')[1])
                }));

            // Course list
            kuasAp.GetCourseListAndSchedule(
                student: student,
                year: SysValue.Split(',')[0],
                semester: SysValue.Split(',')[1]);
            CourseList = new List<Models.Class>(kuasAp.CourseList);
            Schedule = new List<List<Models.Class>>(kuasAp.Schedule);

            // If empty, display alert
            if (CourseList.Count <= 0)
                ModelState.AddModelError("Warning", $"查無 { SysValues.First(item => item.Selected).Text } 之課程資訊");
            else
            {
                // 跨校選課不得撰寫評論
                CourseList.ForEach(course => course.DoneComment = course.ClassShortName.Contains("跨外校"));
                Schedule.ForEach(courses => courses.ForEach(course => course.DoneComment = string.IsNullOrEmpty(course.ClassShortName) ? course.DoneComment : course.ClassShortName.Contains("跨外校")));

                // 尋找是否存在已經寫過的評論
                // Repositorys
                var classRepository = new Repositorys.ClassRepository(appSettings.ConnectionStrings.DefaultConnection);
                var commentRepository = new Repositorys.CommentRepository(appSettings.ConnectionStrings.DefaultConnection);
                // // 搜尋使用者的評論
                var comments = (await commentRepository.Select(comment: new Models.Comment() { Creator = student.Username })).ToList();
                // // 巡迴所有評論
                foreach (var comment in comments)
                {
                    // 取得評論之課程資料
                    Models.Class @class = (await classRepository.Select(@class: new Models.Class() { Guid = comment.ClassGuid })).ToList().First();
                    // 選課清單
                    CourseList.ForEach(course => 
                        course.DoneComment = course.DoneComment || 
                        (course.SubjectChineseName == @class.SubjectChineseName && course.Teachers == @class.Teachers && course.ClassShortName == @class.ClassShortName));
                    // 課表
                    Schedule.ForEach(courses => courses.ForEach(course =>
                        course.DoneComment = course.DoneComment ||
                        (course.SubjectChineseName == @class.SubjectChineseName && course.Teachers == @class.Teachers && course.ClassShortName == @class.ClassShortName)));
                }
            }
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();
            kuasAp.GetCourseListAndSchedule(student: student, year: SysValue.Split(',')[0], semester: SysValue.Split(',')[1]);

            if (Class.SubjectChineseName.Contains("跨外校"))
            {
                TempData["Error"] = "跨校選課不得撰寫課程評論";
                return RedirectToPage($"/Courses/Create/Select", new { SysValue = SysValue });
            }
            else if (!kuasAp.CourseList.Any(course =>
                    course.SubjectChineseName == Class.SubjectChineseName &&
                    course.Teachers == Class.Teachers &&
                    course.ClassShortName == Class.ClassShortName))
            {
                TempData["Error"] = $"學期中並未包含 { Class.SubjectChineseName } 課程";
                return RedirectToPage($"/Courses/Create/Select", new { SysValue = SysValue });
            }
            else
            {
                var classRepository = new Repositorys.ClassRepository(appSettings.ConnectionStrings.DefaultConnection);
                var @class = (await classRepository.Select(@class: new Models.Class()
                {
                    SubjectChineseName = Class.SubjectChineseName,
                    Teachers = Class.Teachers,
                    ClassShortName = Class.ClassShortName
                })).FirstOrDefault();
                if (@class != null)
                    return RedirectToPage("/Courses/Create/Content", new { ClassGuid = @class.Guid });
                else
                {
                    Class.Guid = Guid.NewGuid().ToString();
                    if (await classRepository.Insert(@class: Class) > 0)
                        return RedirectToPage("/Courses/Create/Content", new { ClassGuid = Class.Guid });
                    else
                    {
                        TempData["Error"] = "課程加入失敗";
                        return RedirectToPage($"/Courses/Create/Select", new { SysValue = SysValue });
                    }
                }
            }
        }
    }
}