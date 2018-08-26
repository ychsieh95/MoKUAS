using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace MoKUAS.Pages.Courses.Delete
{
    public class ContentModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string CommentGuid { get; set; }

        private readonly Models.AppSettings appSettings;

        public ContentModel(IOptions<Models.AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);

            // Repositorys
            var classRepository = new Repositorys.ClassRepository(appSettings.ConnectionStrings.DefaultConnection);
            var commentRepository = new Repositorys.CommentRepository(appSettings.ConnectionStrings.DefaultConnection);

            var comments = (await commentRepository.Select(comment: new Models.Comment() { Guid = CommentGuid })).ToList();
            if (comments.Count <= 0)
                TempData["Error"] = $"���ײ������ѡ]����{ CommentGuid }���s�b�^";
            else
            {
                var comment = comments.First();
                if (comment.Creator != student.Username)
                    TempData["Error"] = "�ȵ��ת̥��H�i�����ӵ���";
                else
                {
                    if (await commentRepository.Delete(comment: new Models.Comment() { Guid = CommentGuid }) > 0)
                        TempData["Success"] = "���ײ������\";
                    else
                        TempData["Error"] = "���ײ�������";
                }
            }

            return RedirectToPage("/Courses/Mine");
        }
    }
}