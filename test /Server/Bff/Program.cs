using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSpaStaticFiles(configuration => { configuration.RootPath = "../../client/dist"; });


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpLogging(o =>
{
    o.CombineLogs = true;
    o.LoggingFields = HttpLoggingFields.All;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(o =>
{
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Strict;
    o.Cookie.HttpOnly = true;
}).AddOpenIdConnect("Auth0", options =>
{
    // Set the authority to your Auth0 domain

    options.Authority = $"https://{builder.Configuration.GetSection("Auth0:Domain").Value}";

    // Configure the Auth0 Client ID and Client Secret
    options.ClientId = builder.Configuration.GetSection("Auth0:ClientId").Value;
    options.ClientSecret = builder.Configuration.GetSection("Auth0:ClientSecret").Value;

    // Set response type to code
    options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

    options.ResponseMode = OpenIdConnectResponseMode.FormPost;

    // Configure the scope
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("offline_access"); // For requesting refresh token
    options.Scope.Add("read:weather"); // permission for the weather api(inside the API Project)

    // Set the callback path, so Auth0 will call back to http://localhost:5001/callback
    // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard
    options.CallbackPath = new PathString("/auth/callback");

    // Configure the Claims Issuer to be Auth0
    options.ClaimsIssuer = "Auth0";

    /*
        The SaveTokens option tells the OpenID Connect middleware that all the tokens (id token, refresh token, and access token)
        received from the authorization endpoint during the initial handshake must be persisted for later use
    */
    options.SaveTokens = true;

    options.Events = new OpenIdConnectEvents
    {
        // handle the logout redirection
        OnRedirectToIdentityProviderForSignOut = (context) =>
        {
            var logoutUri =
                $"https://{builder.Configuration.GetSection("Auth0:Domain").Value}/v2/logout?client_id={builder.Configuration.GetSection("Auth0:ClientId").Value}";

            var postLogoutUri = context.Properties.RedirectUri;
            if (!string.IsNullOrEmpty(postLogoutUri))
            {
                if (postLogoutUri.StartsWith("/"))
                {
                    // transform to absolute
                    var request = context.Request;
                    postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                }

                logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
            }

            context.Response.Redirect(logoutUri);
            context.HandleResponse();

            return Task.CompletedTask;
        },
        OnRedirectToIdentityProvider = context =>
        {
            /*
                The OpenID Connect middleware does not have any property to configure the audience parameter that Auth0 requires for returning an authorization code for an API.
                We are attaching some code to the OnRedirectToIdentityProvider event for setting that parameter before the user is redirected to Auth0 for authentication.
            */
            context.ProtocolMessage.SetParameter("audience",
                builder.Configuration.GetSection("Auth0:ApiAudience").Value);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

// In production, the React files will be served from this directory
// builder.Services.sta(configuration => { configuration.RootPath = "ClientApp/build"; });

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpLogging();
// app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action=Index}/{id?}");
});

// app.MapFallbackToFile("/index.html");


var spaPath = "/";
if (app.Environment.IsDevelopment())
{
    app.UseSpa(client =>
    {
        client.UseProxyToSpaDevelopmentServer("https://localhost:5173");

        client.Options.SourcePath = "../../client";
        client.UseReactDevelopmentServer("dev");
    });
}
else
{
    app.Map(new PathString(spaPath), client =>
    {
        client.UseSpaStaticFiles();
        client.UseSpa(spa =>
        {
            spa.Options.SourcePath = "../../client";

            // prevent caching of spa files
            // https://stackoverflow.com/questions/38231739/how-to-disable-browser-cache-in-asp-net-core-rc2/38235096#38235096
            spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ResponseHeaders headers = ctx.Context.Response.GetTypedHeaders();
                    headers.CacheControl = new CacheControlHeaderValue
                    {
                        NoCache = true,
                        NoStore = true,
                        MustRevalidate = true
                    };
                }
            };
        });
    });
}

await app.RunAsync();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}