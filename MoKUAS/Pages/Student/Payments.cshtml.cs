using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MoKUAS.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MoKUAS.Pages.Student
{
    public class PaymentsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string PaymentParams { get; set; }

        public List<SelectListItem> DropDownList { get; private set; }
        public string FileUrl { get; private set; }
        
        private readonly IHostingEnvironment hostingEnvironment;

        public PaymentsModel(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public void OnGet()
        {
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();

            // DropDownList items
            DropDownList = new List<SelectListItem>();
            foreach (string[] payment in kuasAp.GetPayments(student: student))
                DropDownList.Add(new SelectListItem()
                {
                    Text = payment[0],
                    Value = $"{ payment[0] },{ payment[1] }"
                });
            foreach (string[] payment in kuasAp.GetParlingPermitsPayments(student: student))
                DropDownList.Add(new SelectListItem()
                {
                    Text = payment[0],
                    Value = $"{ payment[0] },{ payment[1] }"
                });

            if (DropDownList.Count <= 0)
                ModelState.AddModelError("Warning", "目前無任何可檢視的繳費單");
            else
            {
                if (string.IsNullOrEmpty(PaymentParams))
                    DropDownList.Insert(0, new SelectListItem()
                    {
                        Text = "--- 請選擇繳費單 ---",
                        Value = "",
                        Disabled = true,
                        Selected = true
                    });
                else
                {
                    // Download first item receipt bytes
                    List<string> urlParams = PaymentParams.Split(',').Skip(1).ToList();
                    MemoryStream stream = urlParams.Count != 3 ?
                        kuasAp.DownloadPayment(student: student, urlParams: urlParams) :
                        kuasAp.DownloadParlingPermitsPayment(student: student, urlParams: urlParams);
                    if (stream == null)
                        ModelState.AddModelError("Error", "檔案不存在（逾期）或下載失敗");
                    else
                    {
                        FileUrl = stream.ToArray().SaveToFile(
                            filename: $"{ student.Username }_{ DateTime.Now.ToString("HHmmss") }_{ PaymentParams.Split(',').First() }.pdf",
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
    }
}