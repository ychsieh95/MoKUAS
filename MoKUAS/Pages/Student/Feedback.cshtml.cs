using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MoKUAS.Pages.Student
{
    public class FeedbackModel : PageModel
    {
        [BindProperty]
        public Models.Feedback Feedback { get; set; }

        public List<SelectListItem> Types { get; private set; }

        private readonly Models.AppSettings appSettings;

        public FeedbackModel(IOptions<Models.AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public void OnGet()
        {
            // Create html droplist item
            Types = new List<SelectListItem>()
            {
                new SelectListItem() { Text = "錯誤回報 (Bugs)", Value = ((int)Models.FeedbackType.Bugs).ToString(), Selected = true },
                new SelectListItem() { Text = "問題 (Question)", Value = ((int)Models.FeedbackType.Question).ToString(), Selected = false },
                new SelectListItem() { Text = "建議 (Suggesstion)", Value = ((int)Models.FeedbackType.Suggestion).ToString(), Selected = false },
                new SelectListItem() { Text = "其它 (Others)", Value = ((int)Models.FeedbackType.Others).ToString(), Selected = false }
            };
            // Get reCAPTHCA key from appsettings.json
            ViewData["ReCaptchaKey"] = appSettings.GoogleReCaptcha.SiteKey;
        }
        
        public async Task OnPostAsync()
        {
            // Create html droplist item
            Types = new List<SelectListItem>()
            {
                new SelectListItem() { Text = "錯誤回報 (Bugs)", Value = ((int)Models.FeedbackType.Bugs).ToString(), Selected = true },
                new SelectListItem() { Text = "問題 (Question)", Value = ((int)Models.FeedbackType.Question).ToString(), Selected = false },
                new SelectListItem() { Text = "建議 (Suggesstion)", Value = ((int)Models.FeedbackType.Suggestion).ToString(), Selected = false },
                new SelectListItem() { Text = "其它 (Others)", Value = ((int)Models.FeedbackType.Others).ToString(), Selected = false }
            };
            // Get reCAPTHCA key from appsettings.json
            ViewData["ReCaptchaKey"] = appSettings.GoogleReCaptcha.SiteKey;

            // Student model
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            Feedback.DateTime = DateTime.Now;
            Feedback.Creator = student.Username;
            Feedback.Guid = Guid.NewGuid().ToString();

            // Check feedback
            if (ModelState.IsValid &&
                ReCaptchaPassed(
                        gRecaptchaResponse: Request.Form["g-recaptcha-response"], // that's how you get it from the Request object
                        secret: appSettings.GoogleReCaptcha.Secret))
            {
                // Check reCaptcha
                if (ReCaptchaPassed(
                        gRecaptchaResponse: Request.Form["g-recaptcha-response"], // that's how you get it from the Request object
                        secret: appSettings.GoogleReCaptcha.Secret))
                {
                    var feedbackRepository = new Repositorys.FeedbackRepository(appSettings.ConnectionStrings.DefaultConnection);
                    if (await feedbackRepository.Insert(feedback: Feedback) <= 0)
                        ModelState.AddModelError("Error", "Feedback 建立失敗");
                    else
                    {
                        Feedback = new Models.Feedback();
                        ModelState.AddModelError("Success", "Feedback 建立成功");
                    }
                }
                else
                    ModelState.AddModelError("Error", "機器人驗證失敗，請重新驗證");
            }
        }

        public bool ReCaptchaPassed(string gRecaptchaResponse, string secret)
        {
            var httpClient = new HttpClient();
            var res = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={gRecaptchaResponse}").Result;
            if (res.StatusCode != HttpStatusCode.OK)
                return false;
            else
            {
                var jsonStr = res.Content.ReadAsStringAsync().Result;
                dynamic jsonObj = JObject.Parse(jsonStr);
                return jsonObj.success == "true";
            }
        }
    }
}