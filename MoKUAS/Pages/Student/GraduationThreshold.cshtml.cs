using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Linq;

namespace MoKUAS.Pages.Student
{
    public class GraduationThresholdModel : PageModel
    {
        public Models.GraduationThreshold GraduationThreshold { get; private set; }
        public bool IsEngEngGradPass { get; private set; }
        public bool IsEngTrainSta { get; private set; }
        public bool IsOcpClassList { get; private set; }

        public void OnGet()
        {
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();

            if (student.ChtClass.Length > 0 && student.ChtClass.Substring(0, 1) == "四")
            {
                GraduationThreshold = kuasAp.GetGraduationThreshold(student: student);
                IsEngEngGradPass = GraduationThreshold.EngEngGradPass.IndexOfAny(new char[] { '不', '未' }) < 0;
                IsEngTrainSta = GraduationThreshold.EngTrainSta.IndexOfAny(new char[] { '不', '未' }) < 0;
                IsOcpClassList = GraduationThreshold.OcpClassList.Count > 0;
            }
            else
            {
                GraduationThreshold = new Models.GraduationThreshold()
                {
                    StdBasicInfo = new Models.StdBasicInfo()
                    {
                        ClassName = student.ChtClass,
                        StdId = student.Username,
                        StdName = student.ChtName
                    }
                };
                IsEngEngGradPass = false;
                IsEngTrainSta = false;
                IsOcpClassList = false;
                ModelState.AddModelError("Error", "僅日間部四技學生可使用此功能");
            }
        }
    }
}