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

namespace MoKUAS.Pages
{
    public class ReceiptsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string ReceiptParams { get; set; }

        public List<SelectListItem> DropDownList { get; private set; }
        public string FileUrl { get; private set; }
        
        private readonly IHostingEnvironment hostingEnvironment;

        public ReceiptsModel(IHostingEnvironment hostingEnvironment)
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
            foreach (string[] receipt in kuasAp.GetRecepits(student: student))
                DropDownList.Add(new SelectListItem()
                {
                    Text = receipt[0],
                    Value = $"{ receipt[0] },{ receipt[1] }"
                });

            if (DropDownList.Count <= 0)
                ModelState.AddModelError("Warning", "目前無任何可檢視的繳費單收據");
            else
            {
                if (string.IsNullOrEmpty(ReceiptParams))
                    DropDownList.Insert(0, new SelectListItem()
                    {
                        Text = "--- 請選擇繳費單收據 ---",
                        Value = "",
                        Disabled = true,
                        Selected = true
                    });
                else
                {
                    // Download first item receipt bytes
                    List<string> urlParams = ReceiptParams.Split(',').Skip(1).ToList();
                    MemoryStream stream = kuasAp.DownloadRecepit(student: student, urlParams: urlParams);
                    if (stream == null)
                        ModelState.AddModelError("Error", "檔案不存在（逾期）或下載失敗");
                    else
                    {
                        FileUrl = stream.ToArray().SaveToFile(
                            filename: $"{ student.Username }_{ DateTime.Now.ToString("HHmmss") }_{ ReceiptParams.Split(',').First() }.pdf",
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