using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoKUAS.Pages.Courses
{
    public class IndexPageModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int Pages { get; set; }
        [BindProperty(SupportsGet = true)]
        public string OrderBy { get; set; }

        public int LastPage { get; private set; }
        public List<Models.Comment> Comments { get; private set; }

        private readonly Models.AppSettings appSettings;

        public IndexPageModel(IOptions<Models.AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public async Task OnGetAsync()
        {
            // Default parameters
            if (Pages <= 0) Pages = 1;
            if (string.IsNullOrEmpty(OrderBy)) OrderBy = "DESC";

            // Repositorys
            var classRepository = new Repositorys.ClassRepository(appSettings.ConnectionStrings.DefaultConnection);
            var commentRepository = new Repositorys.CommentRepository(appSettings.ConnectionStrings.DefaultConnection);

            // 取得所有課程之評論
            Comments = new List<Models.Comment>();
            foreach (var @class in (await classRepository.Select(@class: null)).ToList())
            {
                List<Models.Comment> classComments = (await commentRepository.Select(comment: new Models.Comment() { ClassGuid = @class.Guid })).ToList();
                if (classComments.Count > 0)
                    Comments.Add(new Models.Comment()
                    {
                        SubjectChineseName = @class.SubjectChineseName,
                        Teachers = @class.Teachers,
                        ClassShortName = @class.ClassShortName,
                        Grade = classComments.Sum(comment => comment.Grade) / classComments.Count,  /* Grade 表示平均評價 */
                        Remark = classComments.Sum(comment => comment.Grade).ToString("f2")         /* 借用 Remark 欄位來表示總評價 */
                    });
            }

            // 正向排序或反向排序 && 排序欄位 (預設以 DESC 處理)
            Comments = OrderBy == "ASC" ?
                Comments.OrderBy(comment => comment.Grade).ThenByDescending(comment => decimal.Parse(comment.Remark)).ThenBy(comment => comment.ClassShortName).ThenBy(comment => comment.Teachers).ThenBy(comment => comment.SubjectChineseName).ToList() :
                Comments.OrderByDescending(comment => comment.Grade).ThenByDescending(comment => decimal.Parse(comment.Remark)).ThenBy(comment => comment.ClassShortName).ThenBy(comment => comment.Teachers).ThenBy(comment => comment.SubjectChineseName).ToList();

            // 表示每頁顯示課程數
            int coursePerPage = 12;
            // 表示最後頁碼
            LastPage = (int)Math.Ceiling((decimal)Comments.Count / coursePerPage);
            // 檢查頁面是否存在
            if (Pages < 1 || ((Pages - 1) * coursePerPage > (Comments.Count - 1)))
                Comments = Comments.GetRange((Pages - 1) * coursePerPage, coursePerPage);
            else
            {
                // 取得回傳的評論並加入至集合
                if (Comments.Count >= Pages * coursePerPage)
                    Comments = Comments.GetRange((Pages - 1) * coursePerPage, coursePerPage);
                else
                    Comments = Comments.GetRange((Pages - 1) * coursePerPage, Comments.Count - (Pages - 1) * coursePerPage);
            }
        }
    }
}