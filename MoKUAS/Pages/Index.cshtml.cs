using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoKUAS.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            foreach (var cookie in Request.Cookies.Keys)
                Response.Cookies.Delete(cookie);
        }
    }
}