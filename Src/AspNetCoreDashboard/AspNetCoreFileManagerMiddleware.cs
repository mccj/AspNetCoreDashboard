using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AspNetCoreDashboard.Dashboard;
using AspNetCoreDashboard.Annotations;
#if NETSTANDARD
using Microsoft.AspNetCore.Http;
using Middleware = AspNetCoreDashboard.CoreMiddleware;
using RequestDelegate = Microsoft.AspNetCore.Http.RequestDelegate;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;
#else
using Middleware = Microsoft.Owin.OwinMiddleware;
using RequestDelegate = Microsoft.Owin.OwinMiddleware;
using HttpContext = Microsoft.Owin.IOwinContext;
#endif
namespace AspNetCoreDashboard
{
    public abstract class AbstractAspNetCoreFileManagerMiddleware : Middleware
    {
        private readonly RequestDelegate _next;
        //private readonly JobStorage _storage;
        //private readonly DashboardOptions _options;
        private readonly IEnumerable<IDashboardAuthorizationFilter> _authorization;
        private readonly RouteCollection _routes;

        public AbstractAspNetCoreFileManagerMiddleware(
            [AspNetCoreDashboard.Annotations.NotNull]RequestDelegate next,
            //[NotNull] JobStorage storage,
            //[NotNull] DashboardOptions options,
            [NotNull] IEnumerable<IDashboardAuthorizationFilter> authorization,
            [NotNull] RouteCollection routes
            ) : base(next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            //if (storage == null) throw new ArgumentNullException(nameof(storage));
            //if (options == null) throw new ArgumentNullException(nameof(options));
            if (routes == null) throw new ArgumentNullException(nameof(routes));

            _next = next;
            //_storage = storage;
            _authorization = authorization??new IDashboardAuthorizationFilter[] { };
            //_options = options;
            _routes = routes;
        }

        public abstract IDashboardContext GetDashboardContext(HttpContext httpContext);
        public override Task Invoke(HttpContext httpContext)
        {
            var context = GetDashboardContext(httpContext);
            var findResult = _routes.FindDispatcher(httpContext.Request.Path.Value);

            if (findResult == null)
            {
                return _next.Invoke(httpContext);
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var filter in _authorization)
            {
                if (!filter.Authorize(context))
                {
#if NETSTANDARD
                    var isAuthenticated = httpContext.User?.Identity?.IsAuthenticated;
#else
                    var isAuthenticated = httpContext.Request?.User?.Identity?.IsAuthenticated;
#endif
                    httpContext.Response.StatusCode = isAuthenticated == true
                        ? (int)System.Net.HttpStatusCode.Forbidden
                        : (int)System.Net.HttpStatusCode.Unauthorized;

                    return Task.FromResult(0);
                }
            }

            context.UriMatch = findResult.Item2;

            var r= findResult.Item1.Dispatch(context);
            return context.NextInvoke ? _next.Invoke(httpContext):r ;
        }
    }
    public sealed class AspNetCoreFileManagerMiddleware : AbstractAspNetCoreFileManagerMiddleware
    {
        public AspNetCoreFileManagerMiddleware(
             [NotNull]RequestDelegate next,
            [NotNull] IEnumerable<IDashboardAuthorizationFilter> authorization,
            [NotNull] RouteCollection routes
            ) : base(next, authorization, routes) { }
        public override IDashboardContext GetDashboardContext(HttpContext httpContext)
        {
            return new AspNetCoreDashboardContext(/*_storage, _options,*/ httpContext);
        }
    }
    public sealed class AspNetCoreFileManagerMiddleware<T> : AbstractAspNetCoreFileManagerMiddleware
    {
        private readonly T _options;

        public AspNetCoreFileManagerMiddleware(
                [NotNull]RequestDelegate next,
                [NotNull] T options,
                [NotNull] IEnumerable<IDashboardAuthorizationFilter> authorization,
                [NotNull] RouteCollection routes
            ) : base(next, authorization, routes)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _options = options;
        }
        public override IDashboardContext GetDashboardContext(HttpContext httpContext)
        {
            return new AspNetCoreDashboardContext<T>(/*_storage,*/ _options, httpContext);
        }
    }
}