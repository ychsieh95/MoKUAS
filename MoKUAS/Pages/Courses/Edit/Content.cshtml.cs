using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoKUAS.Pages.Courses.Edit
{
    public class ContentModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string CommentGuid { get; set; }
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
            var commentRepository = new Repositorys.CommentRepository(appSettings.ConnectionStrings.DefaultConnection);
            var comment = (await commentRepository.Select(comment: new Models.Comment() { Guid = CommentGuid })).FirstOrDefault();
            if (comment == null)
            {
                TempData["Error"] = $"查無評論 { CommentGuid } 的課程資訊";
                return RedirectToPage("/Courses/Details", new { CommentGuid = CommentGuid });
            }
            else
            {
                var student = JsonConvert.DeserializeObject<Models.Student>(
                    User.Claims.First(claim => claim.Type == "Information").Value);
                if (comment.Creator == student.Username)
                {
                    // Check comment's class info
                    var classRepository = new Repositorys.ClassRepository(appSettings.ConnectionStrings.DefaultConnection);
                    var classes = (await classRepository.Select(@class: new Models.Class() { Guid = comment.ClassGuid })).ToList();
                    if (classes.Count <= 0)
                    {
                        TempData["Error"] = $"查無評論 { CommentGuid } 的課程資訊";
                        return RedirectToPage("/Courses/Details", new { CommentGuid = CommentGuid });
                    }
                    else
                    {
                        comment.SubjectChineseName = classes.First().SubjectChineseName;
                        comment.Teachers = classes.First().Teachers;
                        comment.ClassShortName = classes.First().ClassShortName;
                        Comment = comment;
                        return Page();
                    }
                }
                else
                {
                    TempData["Error"] = "僅評論者本人可修改此評論";
                    return RedirectToPage("/Courses/Details", new { CommentGuid = CommentGuid });
                }
            }
        }

        public async Task<IActionResult> OnPost()
        {
            // Repositorys
            var commentRepository = new Repositorys.CommentRepository(appSettings.ConnectionStrings.DefaultConnection);
            var comment = (await commentRepository.Select(comment: new Models.Comment() { Guid = CommentGuid })).FirstOrDefault();
            if (comment == null)
            {
                TempData["Error"] = $"查無評論 { CommentGuid } 的課程資訊";
                return RedirectToPage("/Courses/Details", new { CommentGuid = CommentGuid });
            }
            else
            {
                var student = JsonConvert.DeserializeObject<Models.Student>(
                    User.Claims.First(claim => claim.Type == "Information").Value);
                if (comment.Creator == student.Username)
                {
                    // Check comment's class info
                    var classRepository = new Repositorys.ClassRepository(appSettings.ConnectionStrings.DefaultConnection);
                    var classes = (await classRepository.Select(@class: new Models.Class() { Guid = comment.ClassGuid })).ToList();

                    // Insert class info to comment
                    if (classes.Count > 0)
                    {
                        Comment.SubjectChineseName = classes.First().SubjectChineseName;
                        Comment.Teachers = classes.First().Teachers;
                        Comment.ClassShortName = classes.First().ClassShortName;
                        Comment.RecordTime = DateTime.Now;
                    }

                    // Insert comment value
                    Comment.Creator = student.Username;
                    Comment.Guid = CommentGuid;

                    // Update comment
                    if (await commentRepository.Update(comment: Comment) > 0)
                    {
                        TempData["Success"] = "修改評論成功";
                        return RedirectToPage("/Courses/Details", new { CommentGuid = CommentGuid });
                    }
                    else
                    {
                        ModelState.AddModelError("Error", "修改評論失敗");
                        return Page();
                    }
                }
                else
                {
                    TempData["Error"] = "僅評論者本人可修改此評論";
                    return RedirectToPage("/Courses/Details", new { CommentGuid = CommentGuid });
                }
            }
        }
    }
}