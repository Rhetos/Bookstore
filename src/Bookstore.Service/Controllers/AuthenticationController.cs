using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Rhetos;
using Rhetos.Processing;
using Rhetos.Processing.DefaultCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookstore.Service.Controllers
{
    [Route("Authentication/[action]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IProcessingEngine processingEngine;
        private readonly IWebHostEnvironment env;

        public AuthenticationController(IRhetosComponent<IProcessingEngine> rhetosProcessingEngine, IWebHostEnvironment env)
        {
            processingEngine = rhetosProcessingEngine.Value;
            this.env = env;
        }

        [HttpGet]
        public async Task Login(string username)
        {
            // This is overly simplified authentication without a password, for demo purpose only.
            if (!env.IsDevelopment())
                throw new NotSupportedException();

            var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties() { IsPersistent = true });
        }

        [HttpGet]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new AuthenticationProperties() { IsPersistent = true });
        }

        [HttpGet]
        public string DemoProcessingEngine()
        {
            var result = processingEngine.Execute(
                new ReadCommandInfo
                {
                    DataSource = "AuthenticationDemo.UserInfoReport",
                    ReadRecords = true
                });

            var records = (IEnumerable<AuthenticationDemo.UserInfoReport>)result.Records;

            return "UserInfo:" + Environment.NewLine
                + string.Join(Environment.NewLine, records.Select(record => $"{record.Key}: {record.Value}"));
        }
    }
}
