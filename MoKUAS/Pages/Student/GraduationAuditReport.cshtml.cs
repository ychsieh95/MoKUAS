using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoKUAS.Extensions;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace MoKUAS.Pages.Student
{
    public class GraduationAuditReportModel : PageModel
    {
        public string FileUrl { get; private set; }
        
        private readonly IHostingEnvironment hostingEnvironment;

        public GraduationAuditReportModel(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public void OnGet()
        {
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();

            var stream = kuasAp.GetGraduationAuditReport(student: student);
            if (stream == null)
                ModelState.AddModelError("Error", "尚無畢業預審報表供查詢");
            else
            {
                FileUrl = stream.ToArray().SaveToFile(
                    filename: $"{ student.Username }_{ DateTime.Now.ToString("HHmmss") }_GraduationAuditReport.pdf",
                    saveDir: @"files/",
                    trueDir: $@"{ hostingEnvironment.WebRootPath }\",
                    deleteKey: student.Username);
                if (string.IsNullOrEmpty(FileUrl))
                    ModelState.AddModelError("Error", "檔案不存在（逾期）或下載失敗");
                else
                    FileUrl = $"{ Request.Scheme }://{ Request.Host }/{ FileUrl }";
            }
        }
    }
}