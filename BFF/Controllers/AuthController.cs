using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BackendForFrontend.Controllers;

public class AuthController : Controller
{
    public ActionResult Login(string returnUrl = "/")
    {
        // the scheme is Auth0, which we passed to => in Startup.cs .AddOpenIdConnect("Auth0", options => ConfigureOpenIdConnect(options));
        var result = new ChallengeResult("Auth0", new AuthenticationProperties() { RedirectUri = returnUrl });
        return result;
    }

    [Authorize]
    public async Task<ActionResult> Logout()
    {
        await HttpContext.SignOutAsync();

        return new SignOutResult("Auth0", new AuthenticationProperties
        {
            RedirectUri = Url.Action("Index", "Home")
        });
    }


    public ActionResult GetUser()
    {
        if (User.Identity.IsAuthenticated)
        {
            var claims = ((ClaimsIdentity)User.Identity).Claims.Select(c =>
                    new { type = c.Type, value = c.Value })
                .ToArray();

            return Json(new { isAuthenticated = true, claims = claims });
        }

        return Json(new { isAuthenticated = false });
    }
}