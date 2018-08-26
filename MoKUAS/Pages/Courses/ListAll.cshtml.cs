using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoKUAS.Pages.Courses
{
    public class ListAllModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SubjectChineseName { get; set; }
        [BindProperty(SupportsGet = true)]
        public string Teachers { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ClassShortName { get; set; }
        [BindProperty(SupportsGet = true)]
        public bool ByKeyword { get; set; }

        public string Username { get; private set; }
        public List<Models.Comment> Comments { get; private set; }

        private readonly Models.AppSettings appSettings;

        public ListAllModel(IOptions<Models.AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public async Task OnGetAsync()
        {
            // 若搜尋條件為 null (直接點擊分析) 則預設為關鍵字搜尋
            bool paramsNull = new string[] { SubjectChineseName, Teachers, ClassShortName }.All(str => str == null);
            ByKeyword = paramsNull ? true : ByKeyword;

            // Repositorys
            var classRepository = new Repositorys.ClassRepository(appSettings.ConnectionStrings.DefaultConnection);
            var commentRepository = new Repositorys.CommentRepository(appSettings.ConnectionStrings.DefaultConnection);
            
            // 查詢指定課程的相關評論
            Comments = new List<Models.Comment>();
            foreach (var @class in (await classRepository.Select(
                @class: new Models.Class() { SubjectChineseName = SubjectChineseName, Teachers = Teachers, ClassShortName = ClassShortName },
                sqlLike: ByKeyword)))
            {
                var comments = (await commentRepository.Select(comment: new Models.Comment() { ClassGuid = @class.Guid })).ToList();
                comments.ForEach(comment =>
                {
                    comment.SubjectChineseName = @class.SubjectChineseName;
                    comment.Teachers = @class.Teachers;
                    comment.ClassShortName = @class.ClassShortName;
                });
                Comments.AddRange(comments);
            }
            // Sort
            Comments = Comments.OrderByDescending(comment => comment.RecordTime).ToList();
            // Set now username
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);

            // 若無回應物件
            if (Comments.Count == 0)
                ModelState.AddModelError("Error", "無查詢結果");
        }
    }
}