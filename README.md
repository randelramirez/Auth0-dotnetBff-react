This repository contains a React SPA supported by an ASP.NET Core backend to implement authentication and authorization with [Auth0](https://auth0.com/) through the Backend For Frontend (BFF) pattern.

The following article describes the implementation details: [Backend For Frontend Authentication Pattern with Auth0 and ASP.NET Core](https://auth0.com/blog/backend-for-frontend-pattern-with-auth0-and-dotnet/)

## 📋  TO DO:
1.) Update to .NET 6 ✅ <br>
2.) Fix dependency issues on ClientApp ✅ <br>
3.) Include startup of ClientApp development server on BFF start up (run/debug) ✅ <br>
4.) Implement Refresh Token ✅ <br> 
5.) Migrate ClientApp from CRA to Vite (React/TypeScript) ✅ <br>
6.) Create separate branch /or change controllers to minimal api (endpoints) <br>
7.) Create a version where the client app and ASP.NET Core(UseProxyToSpaDevelopmentServer) are using the same port ✅<br>
7.) Create a version where the client app is hosted on Razor page<br>


[Spa Proxy](https://www.infoq.com/articles/dotnet-spa-templates-proxy/)

![alt text](image-1.png)

This approach meant that the launch code needed to be specific for each front-end framework, resulting in hard-to-maintain code for each front-end framework that the Microsoft team wanted to support. <br>


![alt text](image-2.png) 

From .NET 6, the new templates for Angular and React switch how the front end and back end communicate. They use the front end’s proxy solutions to send the request to the back end. The popular front-end frameworks already have built-in support for development server proxying, but they must also be configured each specific to the used framework. The ASP.NET app still launches the front-end development server, but the request comes from that server.

Advantages of this new approach include:

    Simpler configuration in the back-end files
    The back end is more independent from the front-end framework but not completely separate as the launch command and URL are still specific
    No more framework-specific code in the back end’s Startup.cs or Program.cs files
    Extensible to other front-end frameworks not included in the templates


ViteProxy = uses front end proxy <br>
UseProxyToSpaDevelopment = front end is serve in the same port as ASP.NET Core <br>
