using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MoKUAS.Pages.Student
{
    public class ScoresModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SysValue { get; set; }

        public List<SelectListItem> DropDownList { get; private set; }
        public Models.ScoreList ScoreList { get; private set; }

        public void OnGet()
        {
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();

            // Set SysValue default value
            if (string.IsNullOrEmpty(SysValue))
                SysValue = $"{ student.SysYear },{ student.SysSemester }";

            // DropDownList items
            DropDownList = new List<SelectListItem>();
            kuasAp.GetOptionValueList(student: student).ForEach(sysItem =>
                DropDownList.Add(new SelectListItem()
                {
                    Text = sysItem.SysText,
                    Value = $"{ sysItem.SysYear },{ sysItem.SysSemester }",
                    Selected = string.IsNullOrEmpty(SysValue) ?
                               (sysItem.SysYear == student.SysYear && sysItem.SysSemester == student.SysSemester) :
                               (sysItem.SysYear == SysValue.Split(',')[0] && sysItem.SysSemester == SysValue.Split(',')[1])
                }));

            // Score list
            ScoreList = kuasAp.GetScores(
                student: student,
                year: SysValue.Split(',')[0],
                semester: SysValue.Split(',')[1]);

            // If empty, display alert
            if (ScoreList.Scores.Count <= 0)
                ModelState.AddModelError("Warning", $"查無 { DropDownList.First(item => item.Selected).Text } 之學期成績");
        }
    }
}