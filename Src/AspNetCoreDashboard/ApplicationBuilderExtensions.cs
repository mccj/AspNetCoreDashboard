using AspNetCoreDashboard.Dashboard;
using System;
using System.Collections.Generic;
#if !NETFULL
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using IAppBuilder = Microsoft.AspNetCore.Builder.IApplicationBuilder;
#else
using Microsoft.Owin;
using Owin;
using IAppBuilder= Owin.IAppBuilder;
#endif
namespace AspNetCoreDashboard
{
    public static class ApplicationBuilderExtensions
    {
        public static IAppBuilder UseMapDashboard(
            this IAppBuilder app,
            string pathMatch = "/dashboard",
            IEnumerable<IDashboardAuthorizationFilter> authorization = null,
            RouteCollection routes = null
            )
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (pathMatch == null) throw new ArgumentNullException(nameof(pathMatch));
#if !NETFULL
            var services = app.ApplicationServices;

            routes = routes ?? services.GetRequiredService<RouteCollection>();
            authorization = authorization ?? services.GetServices<IDashboardAuthorizationFilter>();

            app.Map(new PathString(pathMatch), x => x.UseMiddleware<AspNetCoreFileManagerMiddleware>(authorization, routes));
#else
            if (routes == null) throw new ArgumentNullException(nameof(routes));
            authorization = authorization ?? new IDashboardAuthorizationFilter[] { };
            app.Map(new PathString(pathMatch), x => x.Use<AspNetCoreFileManagerMiddleware>(authorization, routes));
#endif
            return app;
        }
        public static IAppBuilder UseMapDashboard<T>(
            this IAppBuilder app,
            string pathMatch = "/dashboard",
            T options = default(T),
            IEnumerable<IDashboardAuthorizationFilter> authorization = null,
            RouteCollection routes = null
            ) where T : class, new()
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (pathMatch == null) throw new ArgumentNullException(nameof(pathMatch));

#if !NETFULL
            var services = app.ApplicationServices;

            options = options ?? services.GetService<T>() ?? new T();
            routes = routes ?? services.GetRequiredService<RouteCollection>();
            authorization = authorization ?? services.GetServices<IDashboardAuthorizationFilter>();

            app.Map(new PathString(pathMatch), x => x.UseMiddleware<AspNetCoreFileManagerMiddleware<T>>(options, authorization, routes));
#else
            if (routes == null) throw new ArgumentNullException(nameof(routes));
            authorization = authorization ?? new IDashboardAuthorizationFilter[] { };
            app.Map(new PathString(pathMatch), x => x.Use<AspNetCoreFileManagerMiddleware<T>>(options, authorization, routes));
#endif
            return app;
        }
    }
}
