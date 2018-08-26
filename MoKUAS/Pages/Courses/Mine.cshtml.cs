using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoKUAS.Pages.Courses
{
    public class MineModel : PageModel
    {
        public List<Models.Comment> Comments { get; private set; }

        private readonly Models.AppSettings appSettings;

        public MineModel(IOptions<Models.AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public async Task OnGetAsync()
        {
            // Message by redirect
            if (TempData.ContainsKey("Success"))
                ModelState.AddModelError("Success", TempData["Success"].ToString());
            if (TempData.ContainsKey("Error"))
                ModelState.AddModelError("Error", TempData["Success"].ToString());

            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();

            // Repositorys
            var classRepository = new Repositorys.ClassRepository(appSettings.ConnectionStrings.DefaultConnection);
            var commentRepository = new Repositorys.CommentRepository(appSettings.ConnectionStrings.DefaultConnection);

            Comments = (await commentRepository.Select(comment: new Models.Comment() { Creator = student.Username })).ToList();
            foreach (var comment in Comments)
            {
                var classes = (await classRepository.Select(@class: new Models.Class() { Guid = comment.ClassGuid })).ToList();
                if (classes.Count > 0)
                {
                    comment.SubjectChineseName = classes.First().SubjectChineseName;
                    comment.Teachers = classes.First().Teachers;
                    comment.ClassShortName = classes.First().ClassShortName;
                }
            }

            // If empty, display alert
            if (Comments.Count <= 0)
                ModelState.AddModelError("Warning", "尚未填寫任何學生教學評量");
        }
    }
}