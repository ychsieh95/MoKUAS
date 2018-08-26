using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoKUAS.Extensions;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace MoKUAS.Pages.Student
{
    public class ScoreReportModel : PageModel
    {
        public string FileUrl { get; private set; }
        
        private readonly IHostingEnvironment hostingEnvironment;

        public ScoreReportModel(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public void OnGet()
        {
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();

            var stream = kuasAp.GetScoreReport(student: student);
            if (stream == null)
                ModelState.AddModelError("Error", "�|�L���~���Z��Ѭd��");
            else
            {
                FileUrl = stream.ToArray().SaveToFile(
                    filename: $"{ student.Username }_{ DateTime.Now.ToString("HHmmss") }_ScoreReport.pdf",
                    saveDir: @"files/",
                    trueDir: $@"{ hostingEnvironment.WebRootPath }\",
                    deleteKey: student.Username);
                if (string.IsNullOrEmpty(FileUrl))
                    ModelState.AddModelError("Error", "�ɮפ��s�b�]�O���^�ΤU������");
                else
                    FileUrl = $"{ Request.Scheme }://{ Request.Host }/{ FileUrl }";
            }
        }
    }
}