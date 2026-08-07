# ViteProxy React client

The SPA for the ViteProxy variant. See the [root README](../../README.md) for
the Auth0 setup and how the three projects fit together.

React 18 · TypeScript 5.5 · Vite 5 · React Router 6 · reactstrap + Bootstrap 5

## Running it

In this variant **the BFF starts Vite for you**. `Bff.csproj` sets `SpaRoot`,
`SpaProxyLaunchCommand`, and `SpaProxyServerUrl`, and the launch profiles load
the `Microsoft.AspNetCore.SpaProxy` hosting startup assembly:

```powershell
dotnet run --project ..\Server\Bff\Bff.csproj --launch-profile https
```

Then browse **https://localhost:5173**.

To run Vite on its own, set the proxy target first — the fallback in
`vite.config.ts` is `https://localhost:7203`, which is **not** this variant's
BFF port (7119):

```powershell
npm install
$env:ASPNETCORE_HTTPS_PORT = "7119"
npm run dev
```

`ASPNETCORE_URLS` works too; its first `;`-separated entry is used.

## Scripts

- `npm run dev` — Vite dev server on HTTPS port 5173
- `npm run build` — `tsc -b` then `vite build`
- `npm run lint` — ESLint
- `npm run preview` — serve the production build

## Proxy and TLS

`^/api` and `^/auth` are forwarded to the BFF with `secure: false`, so the BFF's
self-signed development certificate is accepted.

On startup the config exports an ASP.NET development certificate named `client`
into `%APPDATA%/ASP.NET/https` (or `~/.aspnet/https`) via `dotnet dev-certs`,
and serves the dev server over HTTPS with it. Vite throws if that export fails,
so the .NET SDK must be installed.

`@` resolves to `./src`.

## Structure

```
src/
  App.tsx                  BrowserRouter and the route table
  context/AuthContext.tsx  Calls /auth/GetUser on mount; holds auth state
  context/useAuth.ts       Consumer hook
  pages/
    Layout.tsx             NavMenu + content container
    NavMenu.tsx            Swaps Login/Logout on auth state
    Home.tsx
    About.tsx
    FetchData.tsx          Weather table from /api/WeatherForecast
    User.tsx               Claims from /auth/GetUser
    NotFound.tsx
```

`/fetch-data` and `/user` are guarded inline in `App.tsx`: when not
authenticated they render a component that calls `login()` and returns `null`.
`login()` and `logout()` are full-page navigations to `/auth/login` and
`/auth/logout` — they have to leave the SPA so the browser can follow the
redirect chain out to Auth0 and back.
