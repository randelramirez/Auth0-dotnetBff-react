# SpaProxy React client

The SPA for the SpaProxy variant. See the [root README](../../README.md) for the
Auth0 setup and how the three projects fit together.

React 18 · TypeScript 5.5 · Vite 5 · React Router 6 · reactstrap + Bootstrap 5

## Running it

**You do not browse this dev server directly.** In this variant the BFF hosts
the SPA: in Development it calls `UseProxyToSpaDevelopmentServer` against
`https://localhost:5173` and `UseReactDevelopmentServer("dev")`, which starts
`npm run dev` here for you.

```powershell
dotnet run --project ..\Server\Bff\Bff.csproj --launch-profile https
```

Then browse **https://localhost:7015** — the BFF — not port 5173.

For production the BFF serves the built output instead, from
`../../client/dist` via `UseSpaStaticFiles`, with caching disabled on the
default page. Produce it with `npm run build`.

## Scripts

- `npm run dev` — Vite dev server on HTTPS port 5173
- `npm run build` — `tsc -b` then `vite build`, output to `dist/`
- `npm run lint` — ESLint
- `npm run preview` — serve the production build

## About the proxy config

`vite.config.ts` here is byte-identical to the ViteProxy variant's — it declares
`^/api` and `^/auth` proxy rules and a fallback target of
`https://localhost:7203`. **Those rules are unused in this variant**, because
requests reach the SPA through the BFF rather than through Vite. They are
harmless leftovers from the shared client template.

The certificate handling does apply: on startup the config exports an ASP.NET
development certificate named `client` into `%APPDATA%/ASP.NET/https` (or
`~/.aspnet/https`) and serves HTTPS with it, so the .NET SDK has to be
installed.

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
`/auth/logout`.

Note that the BFF's `AuthController.Logout` hardcodes its post-logout redirect
to `https://localhost:5173`, so logging out of this variant lands you on the
Vite port rather than back on the BFF at 7015.
