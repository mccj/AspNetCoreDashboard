using AspNetCoreDashboard.Dashboard;
using System;
using System.Collections.Generic;
#if NETSTANDARD
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
            System.Action<RouteCollection> routes = null,
            IEnumerable<IDashboardAuthorizationFilter> authorization = null
        )
        {
            var _routes = new AspNetCoreDashboard.Dashboard.RouteCollection();
            routes(_routes);
            return UseMapDashboard(app, pathMatch, _routes, authorization);
        }
        public static IAppBuilder UseMapDashboard(
            this IAppBuilder app,
            string pathMatch = "/dashboard",
            RouteCollection routes = null,
            IEnumerable<IDashboardAuthorizationFilter> authorization = null
        )
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (pathMatch == null) throw new ArgumentNullException(nameof(pathMatch));
#if NETSTANDARD
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
           System.Action<RouteCollection> routes = null,
           IEnumerable<IDashboardAuthorizationFilter> authorization = null
       ) where T : class, new()
        {
            var _routes = new AspNetCoreDashboard.Dashboard.RouteCollection();
            routes(_routes);
            return UseMapDashboard<T>(app, pathMatch, options, _routes, authorization);
        }
        public static IAppBuilder UseMapDashboard<T>(
            this IAppBuilder app,
            string pathMatch = "/dashboard",
            T options = default(T),
            RouteCollection routes = null,
            IEnumerable<IDashboardAuthorizationFilter> authorization = null
            ) where T : class, new()
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (pathMatch == null) throw new ArgumentNullException(nameof(pathMatch));

#if NETSTANDARD
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
