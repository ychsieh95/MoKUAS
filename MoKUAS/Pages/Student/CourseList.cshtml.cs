﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MoKUAS.Pages.Student
{
    public class CourseListModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SysValue { get; set; }

        public List<SelectListItem> SysValues { get; private set; }
        public List<Models.Class> CourseList { get; private set; }
        public List<List<Models.Class>> Schedule { get; private set; }

        public void OnGet()
        {
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();

            // Set SysValue default value
            if (string.IsNullOrEmpty(SysValue))
                SysValue = $"{ student.SysYear },{ student.SysSemester }";

            // DropDownList items
            SysValues = new List<SelectListItem>();
            kuasAp.GetOptionValueList(student: student).ForEach(sysItem =>
                SysValues.Add(new SelectListItem()
                {
                    Text = sysItem.SysText,
                    Value = $"{ sysItem.SysYear },{ sysItem.SysSemester }",
                    Selected = string.IsNullOrEmpty(SysValue) ?
                               (sysItem.SysYear == student.SysYear && sysItem.SysSemester == student.SysSemester) :
                               (sysItem.SysYear == SysValue.Split(',')[0] && sysItem.SysSemester == SysValue.Split(',')[1])
                }));

            // Course list
            kuasAp.GetCourseListAndSchedule(
                student: student,
                year: SysValue.Split(',')[0],
                semester: SysValue.Split(',')[1]);
            CourseList = new List<Models.Class>(kuasAp.CourseList);
            Schedule = new List<List<Models.Class>>(kuasAp.Schedule);

            // If empty, display alert
            if (CourseList.Count <= 0)
                ModelState.AddModelError("Warning", $"查無 { SysValues.First(item => item.Selected).Text } 之課程資訊");
        }
    }
}