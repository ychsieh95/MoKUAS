using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MoKUAS.Pages.Student
{
    public class MidtermAlertsModel : PageModel
    {
        public List<Models.MidtermAlert> MidtermAlerts { get; private set; }

        public void OnGet()
        {
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();

            // Get midterm alerts
            MidtermAlerts = kuasAp.GetMidtermAlerts(student: student);
            // Remove item without alert
            MidtermAlerts.RemoveAll(midtermAlert => !midtermAlert.IsAlert);
            // Reset index
            int index = 1;
            MidtermAlerts.ForEach(midtermAlert => midtermAlert.Index = (index++).ToString());

            // If empty, display alert
            if (MidtermAlerts.Count<=0)
                ModelState.AddModelError("Warning", "查無期中預警資料");
        }
    }
}