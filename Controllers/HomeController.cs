using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AzureAdDemo1.Models;
using System.Threading;
using Microsoft.Identity.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;

namespace AzureAdDemo1.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        /*string[] Scopes = new string[] { "User.Read" };*/
        //ITokenAcquisition _tokenAcquisition;
        private readonly ILogger<HomeController> _logger;

        private readonly GraphServiceClient _graphServiceClient;

        //private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        private string[] _graphScopes = new[] { "user.readbasic.all" };

        public HomeController(IConfiguration configuration,
                            GraphServiceClient graphServiceClient, ILogger<HomeController> logger)
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
            //this._consentHandler = consentHandler;
            _graphScopes = configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
        }
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public IActionResult Index()
        {            
            return View();
        }
        [AuthorizeForScopes(Scopes = new[] { "user.ReadBasic.All" })]
        [Authorize(Policy = "RequireManagerOnly")]
        public async Task<IActionResult> ClaimsAsync()
        {
                
            var users = await _graphServiceClient.Users
    .Request().Select(u => new
    {
        u.DisplayName,
        u.UserPrincipalName,
        u.Mail
    })
    .GetAsync();
            List<Claims> Userclaims = new List<Claims>();
            foreach (var user in users)
            {
                Claims abc = new Claims
                {
                    DisplayName = user.DisplayName,
                    PrincipalName = user.UserPrincipalName,
                    Email = user.Mail
                };
                Userclaims.Add(abc);
            }
            return View(Userclaims);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
