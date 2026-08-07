# Auth0 .NET BFF React samples

Two .NET 8 takes on the same idea: an Auth0-authenticated Backend-for-Frontend
that holds the tokens, a React SPA that only holds a cookie, and a separate
resource API that accepts only bearer tokens.

The **authentication code is identical** in both. What differs is which process
serves the SPA in development:

| Variant      | You browse            | The SPA is served by                                          |
| ------------ | --------------------- | ------------------------------------------------------------- |
| `SpaProxy/`  | the **BFF** at :7015  | `Microsoft.AspNetCore.SpaServices.Extensions` — the BFF proxies through to Vite |
| `ViteProxy/` | **Vite** at :5173     | Vite's own dev server, proxying `/api` and `/auth` to the BFF  |

Each variant has three projects: `Server/Bff`, `Server/Api`, and `client`.

## Ports

| Variant     | BFF (https)              | API (https)              | Client                    |
| ----------- | ------------------------ | ------------------------ | ------------------------- |
| `SpaProxy`  | `https://localhost:7015` | `https://localhost:7009` | `https://localhost:5173`  |
| `ViteProxy` | `https://localhost:7119` | `https://localhost:7126` | `https://localhost:5173`  |

Both BFFs also expose HTTP profiles (5167 and 5264) and both APIs an HTTP
profile (5008 and 5177), but the OIDC flow needs HTTPS.

## Configure Auth0

You need a **Regular Web Application** (the BFF is confidential) and an **API**.

| Auth0 setting               | Value                                                     |
| --------------------------- | --------------------------------------------------------- |
| Allowed Callback URLs       | `https://localhost:7015/auth/callback` (SpaProxy) or `https://localhost:5173/auth/callback` (ViteProxy) |
| Allowed Logout URLs         | `https://localhost:7015` (SpaProxy) or `https://localhost:5173` (ViteProxy) |
| API Identifier              | `https://weatherforecast`                                  |
| API Permission              | `read:weather`                                             |

Then fill in the `Auth0` section of the BFF's `appsettings.json`:

```jsonc
"Auth0": {
  "Domain": "your-tenant.us.auth0.com",   // no scheme — the code prepends https://
  "ClientId": "...",
  "ClientSecret": "...",
  "ApiAudience": "https://weatherforecast"
}
```

and the API's `appsettings.json`:

```jsonc
"Auth0": {
  "Domain": "your-tenant.us.auth0.com",
  "Audience": "https://weatherforecast"   // ';'-separated for multiple audiences
}
```

Keep real secrets out of tracked config — use `dotnet user-secrets` or
environment variables. (A GitGuardian workflow runs on this repo:
`.github/workflows/gg-shield.yml`.)

## Run

### ViteProxy

`Bff.csproj` carries `SpaRoot`, `SpaProxyLaunchCommand`, and `SpaProxyServerUrl`,
and its launch profiles set `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES` to
`Microsoft.AspNetCore.SpaProxy`. So starting the BFF also starts Vite:

```powershell
dotnet run --project .\ViteProxy\Server\Api\Api.csproj --launch-profile https
dotnet run --project .\ViteProxy\Server\Bff\Bff.csproj --launch-profile https
```

Then browse **https://localhost:5173**.

If you'd rather start Vite yourself, **set the proxy target explicitly** —
`vite.config.ts` falls back to `https://localhost:7203`, which is not the port
of either BFF in this repo:

```powershell
Set-Location .\ViteProxy\client
npm install
$env:ASPNETCORE_HTTPS_PORT = "7119"
npm run dev
```

### SpaProxy

Here the BFF hosts the SPA, so browse the **BFF**:

```powershell
dotnet run --project .\SpaProxy\Server\Api\Api.csproj --launch-profile https
dotnet run --project .\SpaProxy\Server\Bff\Bff.csproj --launch-profile https
```

Then browse **https://localhost:7015**.

In Development the BFF calls `UseProxyToSpaDevelopmentServer("https://localhost:5173")`
and `UseReactDevelopmentServer("dev")`, which launches `npm run dev` in
`../../client` for you. In any other environment it serves the prebuilt
`../../client/dist` through `UseSpaStaticFiles`, with caching headers disabled.

> **Known misconfiguration:** `SpaProxy/Server/Bff/appsettings.json` sets
> `WeatherApiEndpoint` to `https://localhost:7126/api/WeatherForecast` — that is
> the *ViteProxy* API's port. The SpaProxy API listens on **7009**. Fix the
> setting (or run the ViteProxy API alongside it) before `/api/WeatherForecast`
> will work in the SpaProxy variant.

## How the BFF is wired

Cookie authentication is the default scheme; `AddOpenIdConnect("Auth0", ...)`
handles the challenge.

- `ResponseType` is `code id_token` with `ResponseMode` `form_post`.
- Scopes: `openid`, `offline_access` (for a refresh token), `read:weather`.
- `CallbackPath` is `/auth/callback`.
- `SaveTokens = true`, so the access token is available via `GetTokenAsync`.
- Cookies are `HttpOnly`, `Secure` always, `SameSite=Strict`.

Two OIDC events do the Auth0-specific work:

- **`OnRedirectToIdentityProvider`** appends the `audience` parameter from
  `Auth0:ApiAudience`. The stock OIDC middleware has no option for it, and
  without it Auth0 issues an opaque token instead of a JWT for your API.
- **`OnRedirectToIdentityProviderForSignOut`** replaces the standard end-session
  redirect with Auth0's proprietary `/v2/logout?client_id=...&returnTo=...`.

### BFF endpoints

`AuthController` is routed as `[controller]/[action]` (matching is
case-insensitive, so the client's lowercase paths work):

| Path              | Auth | Behaviour                                              |
| ----------------- | ---- | ------------------------------------------------------ |
| `/auth/login`     | No   | `ChallengeResult("Auth0")` with `returnUrl`, default `/` |
| `/auth/logout`    | Yes  | Cookie sign-out, then `SignOutResult("Auth0")` back to :5173 |
| `/auth/getuser`   | No   | `{ isAuthenticated, claims: [{ type, value }] }`        |
| `/api/weatherforecast` | Yes | Reads the saved `access_token` and forwards to the API as a bearer token |

There is also a minimal-API `GET /weatherforecast` on the BFF that returns
generated data with **no authentication** and never touches the API project. It
is left over from the project template — the real path is
`/api/WeatherForecast` on the controller.

`AuthController.Logout` hardcodes its post-logout redirect to
`https://localhost:5173`, which is right for ViteProxy but sends SpaProxy users
to the wrong origin.

## How the API is wired

Identical in both variants. JWT bearer validation against the Auth0 domain,
with `ValidateAudience`, `ValidateIssuer`, and `ClockSkew = TimeSpan.Zero`.
`Auth0:Audience` is split on `;`, so several audiences can be listed.

Scope checking is custom rather than claim-based: `ScopeRequirement`,
`ScopeHandler`, and the `RequireScope` extension in `Authorization/` build a
`read:weather` policy that splits the space-delimited `scope` claim and checks
for a match. `WeatherForecastController` is decorated with
`[Authorize("read:weather")]`.

## The client

React 18 · TypeScript 5.5 · Vite 5 · React Router 6 · reactstrap + Bootstrap 5

The two `client/` trees are the same application, and the two `vite.config.ts`
files are byte-identical — each proxies `^/api` and `^/auth`, listens on 5173,
and exports an ASP.NET dev certificate named `client` to serve HTTPS. The
`SpaProxy` variant simply doesn't use that proxy config, because requests arrive
via the BFF instead.

```
src/
  App.tsx                  BrowserRouter and the route table
  context/AuthContext.tsx  Calls /auth/GetUser on mount; login/logout navigate away
  context/useAuth.ts
  pages/                   Layout, NavMenu, Home, About, FetchData, User, NotFound
```

| Route         | Behaviour                                              |
| ------------- | ------------------------------------------------------ |
| `/`           | `Home`                                                  |
| `/fetch-data` | `FetchData` if authenticated, else triggers `login()`   |
| `/user`       | `User` if authenticated, else triggers `login()`        |
| `/login`      | Triggers `login()`                                      |
| `/logout`     | Triggers `logout()`                                     |
| `*`           | `NotFound`                                              |

`login()` and `logout()` set `window.location.href`, because the browser has to
leave the SPA to follow the redirect chain to Auth0 and back.

See [`ViteProxy/client/README.md`](ViteProxy/client/README.md) and
[`SpaProxy/client/README.md`](SpaProxy/client/README.md) for the per-variant
notes.
