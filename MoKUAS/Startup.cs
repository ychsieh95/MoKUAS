using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace MoKUAS
{
    public class Startup
    {
        private IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IHtmlGenerator, AlertHtmlGenerator>();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.ExpireTimeSpan = new System.TimeSpan(hours: 0, minutes: 15, seconds: 0);
                    options.LoginPath = "/Login";
                });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton(HtmlEncoder.Create(System.Text.Unicode.UnicodeRanges.All));
            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Courses");
                    options.Conventions.AuthorizeFolder("/Student");
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.Configure<Models.AppSettings>(configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Error");

            // Use static files
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            provider.Mappings[".properties"] = "text/plain";
            app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

            // Use and redirect to HTTPS and
            app.UseRewriter(new RewriteOptions().AddRedirectToHttps(301, 443));
            app.UseHttpsRedirection();
            app.UseHsts();

            // Use authentication
            app.UseAuthentication();

            // Use mvc
            app.UseMvc();
        }
    }

    // Source code
    // https://github.com/aspnet/Mvc/blob/release/2.2/src/Microsoft.AspNetCore.Mvc.ViewFeatures/ViewFeatures/DefaultHtmlGenerator.cs
    public class AlertHtmlGenerator : DefaultHtmlGenerator
    {
        public AlertHtmlGenerator(IAntiforgery antiforgery, IOptions<MvcViewOptions> optionsAccessor, IModelMetadataProvider metadataProvider, IUrlHelperFactory urlHelperFactory, HtmlEncoder htmlEncoder, ValidationHtmlAttributeProvider validationAttributeProvider)
            : base(antiforgery, optionsAccessor, metadataProvider, urlHelperFactory, htmlEncoder, validationAttributeProvider)
        {
        }
        public override TagBuilder GenerateValidationMessage(ViewContext viewContext, ModelExplorer modelExplorer, string expression, string message, string tag, object htmlAttributes)
        {
            return base.GenerateValidationMessage(viewContext, modelExplorer, expression, message, tag, htmlAttributes);
        }
        public override TagBuilder GenerateValidationSummary(ViewContext viewContext, bool excludePropertyErrors, string message, string headerTag, object htmlAttributes)
        {
            var htmlSummary = new TagBuilder("div");
            if (!viewContext.ModelState.IsValid)
            {
                foreach (var modelState in viewContext.ModelState)
                {
                    if (modelState.Value.ValidationState != ModelValidationState.Valid)
                    {
                        foreach (var error in modelState.Value.Errors)
                        {
                            switch (modelState.Key.Split(new char[] { '_' })[0])
                            {
                                case "Info":
                                    {
                                        var div = new TagBuilder("div");
                                        var icon = new TagBuilder("i");
                                        icon.AddCssClass("fa fa-info-circle");
                                        div.InnerHtml.AppendHtml(icon);
                                        div.InnerHtml.AppendLine(" " + error.ErrorMessage);
                                        div.AddCssClass("alert alert-info");
                                        htmlSummary.InnerHtml.AppendHtml(div);
                                        break;
                                    }
                                case "Success":
                                    {
                                        var div = new TagBuilder("div");
                                        var icon = new TagBuilder("i");
                                        icon.AddCssClass("fa fa-check-circle");
                                        div.InnerHtml.AppendHtml(icon);
                                        div.InnerHtml.AppendLine(" " + error.ErrorMessage);
                                        div.AddCssClass("alert alert-success");
                                        htmlSummary.InnerHtml.AppendHtml(div);
                                        break;
                                    }
                                case "Warning":
                                    {
                                        var div = new TagBuilder("div");
                                        var icon = new TagBuilder("i");
                                        icon.AddCssClass("fa fa-exclamation-triangle");
                                        div.InnerHtml.AppendHtml(icon);
                                        div.InnerHtml.AppendLine(" " + error.ErrorMessage);
                                        div.AddCssClass("alert alert-warning");
                                        htmlSummary.InnerHtml.AppendHtml(div);
                                        break;
                                    }
                                default:
                                    {
                                        var div = new TagBuilder("div");
                                        var icon = new TagBuilder("i");
                                        icon.AddCssClass("fa fa-exclamation-circle");
                                        div.InnerHtml.AppendHtml(icon);
                                        div.InnerHtml.AppendLine(" " + error.ErrorMessage);
                                        div.AddCssClass("alert alert-danger");
                                        htmlSummary.InnerHtml.AppendHtml(div);
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            else htmlSummary.MergeAttribute("display", "none");
            return htmlSummary;
        }
    }
}
