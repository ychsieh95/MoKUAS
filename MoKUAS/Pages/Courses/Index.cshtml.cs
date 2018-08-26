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

            // ���o�Ҧ��ҵ{������
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
                        Grade = classComments.Sum(comment => comment.Grade) / classComments.Count,  /* Grade ��ܥ������� */
                        Remark = classComments.Sum(comment => comment.Grade).ToString("f2")         /* �ɥ� Remark ���Ӫ���`���� */
                    });
            }

            // ���V�ƧǩΤϦV�Ƨ� && �Ƨ���� (�w�]�H DESC �B�z)
            Comments = OrderBy == "ASC" ?
                Comments.OrderBy(comment => comment.Grade).ThenByDescending(comment => decimal.Parse(comment.Remark)).ThenBy(comment => comment.ClassShortName).ThenBy(comment => comment.Teachers).ThenBy(comment => comment.SubjectChineseName).ToList() :
                Comments.OrderByDescending(comment => comment.Grade).ThenByDescending(comment => decimal.Parse(comment.Remark)).ThenBy(comment => comment.ClassShortName).ThenBy(comment => comment.Teachers).ThenBy(comment => comment.SubjectChineseName).ToList();

            // ��ܨC����ܽҵ{��
            int coursePerPage = 12;
            // ��̫ܳ᭶�X
            LastPage = (int)Math.Ceiling((decimal)Comments.Count / coursePerPage);
            // �ˬd�����O�_�s�b
            if (Pages < 1 || ((Pages - 1) * coursePerPage > (Comments.Count - 1)))
                Comments = Comments.GetRange((Pages - 1) * coursePerPage, coursePerPage);
            else
            {
                // ���o�^�Ǫ����רå[�J�ܶ��X
                if (Comments.Count >= Pages * coursePerPage)
                    Comments = Comments.GetRange((Pages - 1) * coursePerPage, coursePerPage);
                else
                    Comments = Comments.GetRange((Pages - 1) * coursePerPage, Comments.Count - (Pages - 1) * coursePerPage);
            }
        }
    }
}