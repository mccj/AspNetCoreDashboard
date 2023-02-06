//#if NETFRAMEWORK
//using System;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using AspNetCoreDashboard.Dashboard;
//using AspNetCoreDashboard.Annotations;

//using Middleware = Microsoft.Owin.OwinMiddleware;
//using RequestDelegate = Microsoft.Owin.OwinMiddleware;
//using HttpContext = System.Web.HttpContextBase;

//namespace AspNetCoreDashboard
//{
//    public abstract class AbstractAspNetCoreFileManagerMiddlewareMvc : Middleware
//    {
//        private readonly RequestDelegate _next;
//        //private readonly JobStorage _storage;
//        //private readonly DashboardOptions _options;
//        private readonly IEnumerable<IDashboardAuthorizationFilter> _authorization;
//        private readonly RouteCollection _routes;

//        public AbstractAspNetCoreFileManagerMiddlewareMvc(
//            [AspNetCoreDashboard.Annotations.NotNull] RequestDelegate next,
//            //[NotNull] JobStorage storage,
//            //[NotNull] DashboardOptions options,
//            [NotNull] IEnumerable<IDashboardAuthorizationFilter> authorization,
//            [NotNull] RouteCollection routes
//            ) : base(next)
//        {
//            if (next == null) throw new ArgumentNullException(nameof(next));
//            //if (storage == null) throw new ArgumentNullException(nameof(storage));
//            //if (options == null) throw new ArgumentNullException(nameof(options));
//            if (routes == null) throw new ArgumentNullException(nameof(routes));

//            _next = next;
//            //_storage = storage;
//            _authorization = authorization ?? new IDashboardAuthorizationFilter[] { };
//            //_options = options;
//            _routes = routes;
//        }

//        public abstract IDashboardContext GetDashboardContext(HttpContext httpContext);
//        public override Task Invoke(HttpContext httpContext)
//        {
//            var context = GetDashboardContext(httpContext);
//            var findResult = _routes.FindDispatcher(httpContext.Request.Path.Value);

//            if (findResult == null)
//            {
//                return _next.Invoke(httpContext);
//            }

//            // ReSharper disable once LoopCanBeConvertedToQuery
//            foreach (var filter in _authorization)
//            {
//                if (!filter.Authorize(context))
//                {
//#if !NETFRAMEWORK
//                        var isAuthenticated = httpContext.User?.Identity?.IsAuthenticated;
//#else
//                    var isAuthenticated = httpContext.Request?.User?.Identity?.IsAuthenticated;
//#endif
//                    httpContext.Response.StatusCode = isAuthenticated == true
//                        ? (int)System.Net.HttpStatusCode.Forbidden
//                        : (int)System.Net.HttpStatusCode.Unauthorized;

//                    return Task.CompletedTask;
//                }
//            }

//            context.UriMatch = findResult.Item2;

//            var r = findResult.Item1.DispatchAsync(context);
//            return context.NextInvoke ? _next.Invoke(httpContext) : r;
//        }
//    }
//    public sealed class AspNetCoreFileManagerMiddlewareMvc : AbstractAspNetCoreFileManagerMiddlewareMvc
//    {
//        public AspNetCoreFileManagerMiddlewareMvc(
//             [NotNull] RequestDelegate next,
//            [NotNull] IEnumerable<IDashboardAuthorizationFilter> authorization,
//            [NotNull] RouteCollection routes
//            ) : base(next, authorization, routes) { }
//        public override IDashboardContext GetDashboardContext(HttpContext httpContext)
//        {
//            return new AspNetCoreDashboardContextMvc(/*_storage, _options,*/ httpContext);
//        }
//    }
//    public sealed class AspNetCoreFileManagerMiddlewareMvc<T> : AbstractAspNetCoreFileManagerMiddlewareMvc
//    {
//        private readonly T _options;

//        public AspNetCoreFileManagerMiddlewareMvc(
//                [NotNull] RequestDelegate next,
//                [NotNull] T options,
//                [NotNull] IEnumerable<IDashboardAuthorizationFilter> authorization,
//                [NotNull] RouteCollection routes
//            ) : base(next, authorization, routes)
//        {
//            if (options == null) throw new ArgumentNullException(nameof(options));

//            _options = options;
//        }
//        public override IDashboardContext GetDashboardContext(HttpContext httpContext)
//        {
//            return new AspNetCoreDashboardContextOwin<T>(/*_storage,*/ _options, httpContext);
//        }
//    }
//}
//#endif