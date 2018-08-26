using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoKUAS.Extensions;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace MoKUAS.Pages.Student
{
    public class ProfileModel : PageModel
    {
        public string SysYear { get; private set; }
        public string SysSemester { get; private set; }
        public string PersonalPictureUrl { get; private set; }
        public Models.Profile Profile { get; private set; }

        private readonly IHostingEnvironment hostingEnvironment;

        public ProfileModel(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public void OnGet()
        {
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();

            SysYear = student.SysYear;
            SysSemester = student.SysSemester;
            Profile = kuasAp.GetProfile(student: student);

            if (Profile == null)
                Profile = new Models.Profile();
            else
            {
                PersonalPictureUrl = Profile.PersonalPicture.ToArray().SaveToFile(
                    filename: $"{ student.Username }_{ DateTime.Now.ToString("HHmmss") }.jpg",
                    saveDir: @"files/",
                    trueDir: $@"{ hostingEnvironment.WebRootPath }\",
                    deleteKey: student.Username);
                if (string.IsNullOrEmpty(PersonalPictureUrl))
                    ModelState.AddModelError("Error", "圖片不存在或下載失敗");
                else
                    PersonalPictureUrl = $"{ Request.Scheme }://{ Request.Host }/{ PersonalPictureUrl }";
            }
        }
    }
}