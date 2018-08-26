using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace MoKUAS.Pages.Courses
{
    public class DetailsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string CommentGuid { get; set; }
        
        public bool IsCreator { get; private set; }
        public Models.Class Class { get; private set; }
        public Models.Comment Comment { get; private set; }

        private readonly Models.AppSettings appSettings;

        public DetailsModel(IOptions<Models.AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(CommentGuid))
                return RedirectToPage("/Courses/Index");
            else
            {
                // Message by redirect
                if (TempData.ContainsKey("Success"))
                    ModelState.AddModelError("Success", TempData["Success"].ToString());
                if (TempData.ContainsKey("Error"))
                    ModelState.AddModelError("Error", TempData["Success"].ToString());

                // Repositorys
                var classRepository = new Repositorys.ClassRepository(appSettings.ConnectionStrings.DefaultConnection);
                var commentRepository = new Repositorys.CommentRepository(appSettings.ConnectionStrings.DefaultConnection);

                var comments = (await commentRepository.Select(comment: new Models.Comment() { Guid = CommentGuid })).ToList();
                if (comments.Count <= 0)
                {
                    Comment = new Models.Comment();
                    ModelState.AddModelError("Error", "此評論不存在或已被移除");
                }
                else
                {
                    Comment = comments.FirstOrDefault();
                    IsCreator = Comment.Creator.Equals(
                        JsonConvert.DeserializeObject<Models.Student>(
                            User.Claims.First(claim => claim.Type == "Information").Value).Username);
                    var classes = (await classRepository.Select(@class: new Models.Class() { Guid = Comment.ClassGuid })).ToList();
                    if (classes.Count <= 0)
                        ModelState.AddModelError("Error", $"查無評論 { Comment.Guid } 之課程資訊");
                    Class = classes.FirstOrDefault();
                }

                return Page();
            }
        }
    }
}