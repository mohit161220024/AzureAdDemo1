
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AzureAdDemo1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string[] initialScopes = Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
            
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApp(Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                    .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
                    .AddInMemoryTokenCaches();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()/*.RequireRole("Global Administrator")*/
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();

            string[] roles = new string[] { "GlobalAdmin" };

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireManagerOnly", policy =>
                policy.RequireAssertion(context => context.User.HasClaim("roles", "GlobalAdmin")));
                /* policy.RequireClaim(ClaimTypes.Role,roles));*/
            });
           /* services.AddRazorPages();*/
            services.AddMvc()
    .AddRazorPagesOptions(options =>
    {
        
        options.Conventions.AuthorizePage("/Claims","RequireManagerOnly");
/*
        options.Conventions.AuthorizeFolder("/Pages/Admin", "Admins"); // with policy
        options.Conventions.AllowAnonymousToPage("/Pages/Admin/Login"); // excluded page

        options.Conventions.AllowAnonymousToFolder("/Public"); // just for completeness*/
    });

        }
      
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
