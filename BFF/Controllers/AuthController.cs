using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Auth0.AspNetCore.Authentication;

namespace BackendForFrontend.Controllers;

public class AuthController : Controller
{
    public ActionResult Login(string returnUrl = "/")
    {
        // the scheme is Auth0, which we passed to => in Startup.cs .AddOpenIdConnect("Auth0", options => ConfigureOpenIdConnect(options));
        var result = new ChallengeResult("Auth0", new AuthenticationProperties() { RedirectUri = returnUrl });
        return result;
    }
    
    // same (what happens under the hood...)
    // public async Task Login(string returnUrl = "/")
    // {
    //     var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
    //         // Indicate here where Auth0 should redirect the user after a login.
    //         // Note that the resulting absolute Uri must be added to the
    //         // **Allowed Callback URLs** settings for the app.
    //         .WithRedirectUri(returnUrl)
    //         .Build();
    //
    //     await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    // }

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