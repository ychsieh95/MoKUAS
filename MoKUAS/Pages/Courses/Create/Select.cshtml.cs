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
                ModelState.AddModelError("Warning", $"�d�L { SysValues.First(item => item.Selected).Text } ���ҵ{��T");
            else
            {
                // ��տ�Ҥ��o���g����
                CourseList.ForEach(course => course.DoneComment = course.ClassShortName.Contains("��~��"));
                Schedule.ForEach(courses => courses.ForEach(course => course.DoneComment = string.IsNullOrEmpty(course.ClassShortName) ? course.DoneComment : course.ClassShortName.Contains("��~��")));

                // �M��O�_�s�b�w�g�g�L������
                // Repositorys
                var classRepository = new Repositorys.ClassRepository(appSettings.ConnectionStrings.DefaultConnection);
                var commentRepository = new Repositorys.CommentRepository(appSettings.ConnectionStrings.DefaultConnection);
                // // �j�M�ϥΪ̪�����
                var comments = (await commentRepository.Select(comment: new Models.Comment() { Creator = student.Username })).ToList();
                // // ���j�Ҧ�����
                foreach (var comment in comments)
                {
                    // ���o���פ��ҵ{���
                    Models.Class @class = (await classRepository.Select(@class: new Models.Class() { Guid = comment.ClassGuid })).ToList().First();
                    // ��ҲM��
                    CourseList.ForEach(course => 
                        course.DoneComment = course.DoneComment || 
                        (course.SubjectChineseName == @class.SubjectChineseName && course.Teachers == @class.Teachers && course.ClassShortName == @class.ClassShortName));
                    // �Ҫ�
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

            if (Class.SubjectChineseName.Contains("��~��"))
            {
                TempData["Error"] = "��տ�Ҥ��o���g�ҵ{����";
                return RedirectToPage($"/Courses/Create/Select", new { SysValue = SysValue });
            }
            else if (!kuasAp.CourseList.Any(course =>
                    course.SubjectChineseName == Class.SubjectChineseName &&
                    course.Teachers == Class.Teachers &&
                    course.ClassShortName == Class.ClassShortName))
            {
                TempData["Error"] = $"�Ǵ����å��]�t { Class.SubjectChineseName } �ҵ{";
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
                        TempData["Error"] = "�ҵ{�[�J����";
                        return RedirectToPage($"/Courses/Create/Select", new { SysValue = SysValue });
                    }
                }
            }
        }
    }
}