using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoKUAS.Pages.Courses.Create
{
    public class ContentModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ClassGuid { get; set; }
        [BindProperty]
        public Models.Comment Comment { get; set; }

        private readonly Models.AppSettings appSettings;

        public ContentModel(IOptions<Models.AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public async Task<IActionResult> OnGet()
        {
            // Repositorys
            var classRepository = new Repositorys.ClassRepository(appSettings.ConnectionStrings.DefaultConnection);
            var @class = (await classRepository.Select(@class: new Models.Class() { Guid = ClassGuid })).FirstOrDefault();
            if (string.IsNullOrEmpty(@class.SubjectChineseName) &&
                string.IsNullOrEmpty(@class.Teachers) &&
                string.IsNullOrEmpty(@class.ClassShortName))
            {
                ModelState.AddModelError("Error", "�d�L�ӽҵ{��ơA�Ш̷Ӭy�{��g");
                return Page();
            }
            else
            {
                Comment = new Models.Comment()
                {
                    /* Fixed value */
                    SubjectChineseName = @class.SubjectChineseName,
                    Teachers = @class.Teachers,
                    ClassShortName = @class.ClassShortName,
                    ClassGuid = @class.Guid,
                    /* Default value */
                    RollCallFrequency = 2m, // [0, 1, 2, 3, 4]
                    Grade = 3
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();
            else
            {
                var student = JsonConvert.DeserializeObject<Models.Student>(
                    User.Claims.First(claim => claim.Type == "Information").Value);
                var kuasAp = new Services.KUASAPService();

                // Repositorys
                var classRepository = new Repositorys.ClassRepository(appSettings.ConnectionStrings.DefaultConnection);
                var commentRepository = new Repositorys.CommentRepository(appSettings.ConnectionStrings.DefaultConnection);

                // Check Guid
                var classes = (await classRepository.Select(@class: new Models.Class() { Guid = ClassGuid })).ToList();
                if (classes.Count <= 0)
                {
                    ModelState.AddModelError("Error", "�d�L���ҵ{��ơA�Ш̷Ӭy�{��g");
                    return Page();
                }
                else if (classes.First().SubjectChineseName != Comment.SubjectChineseName ||
                         classes.First().Teachers != Comment.Teachers ||
                         classes.First().ClassShortName != Comment.ClassShortName)
                {
                    ModelState.AddModelError("Error", "���ŦX���ҵ{��T�A�Ш̷Ӭy�{��g");
                    return Page();
                }
                else
                {
                    if ((await commentRepository.Select(comment: new Models.Comment() { ClassGuid = ClassGuid }))
                            .Any(comment => comment.Creator == student.Username))
                    {
                        ModelState.AddModelError("Error", "�w��g�L���ҵ{������");
                        return Page();
                    }
                    else
                    {
                        Comment.ClassGuid = ClassGuid;
                        Comment.Remark = string.IsNullOrEmpty(Comment.Remark) ? "" : (Comment.Remark);
                        Comment.Creator = student.Username;
                        Comment.RecordTime = DateTime.Now;
                        Comment.Guid = Guid.NewGuid().ToString();
                        if (await commentRepository.Insert(comment: Comment) > 0)
                        {
                            TempData["Success"] = "���g���צ��\";
                            return RedirectToPage("/Courses/Details", new { CommentGuid = Comment.Guid });
                        }
                        else
                        {
                            ModelState.AddModelError("Error", "���g���ץ���");
                            return Page();
                        }
                    }
                }
            }
        }
    }
}