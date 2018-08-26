using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace MoKUAS.Pages.Student.Settings
{
    public class ThemePlateModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Theme { get; set; }

        public List<string> Themes { get; private set; }

        public IActionResult OnGet()
        {
            Themes = new List<string>()
            {
                "default",
                "cerulean",
                "cosmo",
                "cyborg",
                "darkly",
                "flatly",
                "journal",
                "litera",
                "lumen",
                "lux",
                "materia",
                "minty",
                "pulse",
                "sandstone",
                "simplex",
                "sketchy",
                "slate",
                "solar",
                "spacelab",
                "superhero",
                "united",
                "yeti"
            };

            if (string.IsNullOrEmpty(Theme))
                return Page();
            else
            {
                if (Theme == "default")
                    Response.Cookies.Delete(".MoKUAS.Bootstrap.Theme");
                else
                    Response.Cookies.Append(".MoKUAS.Bootstrap.Theme", Theme);
                return RedirectToPage("/Student/Settings/ThemePlate", new { Theme = "" });
            }
        }
    }
}
