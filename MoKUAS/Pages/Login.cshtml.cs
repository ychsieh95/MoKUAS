using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MoKUAS.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return new PageResult();

            // Login
            var student = new Models.Student() { Username = Username, Password = Password };
            JObject loginState = JObject.Parse(
                new Services.KUASAPService().Login(student: ref student));
            if (bool.Parse(loginState["state"].ToString()))
            {
                var claims = new[] {
                    new Claim(ClaimTypes.Name, student.Username),
                    new Claim(ClaimTypes.Role, "Students"),
                    new Claim("Information", JsonConvert.SerializeObject(student)) };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));

                if (string.IsNullOrEmpty(ReturnUrl))
                    return Redirect("/Student/Index");
                else
                    return Redirect(ReturnUrl);
            }
            else
            {
                ModelState.AddModelError("Error", loginState["message"].ToString());
                return Page();
            }
        }
    }
}