#if NETSTANDARD
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AspNetCoreDashboard
{
    public abstract class CoreMiddleware //: Microsoft.AspNetCore.Http.IMiddleware
    {
        protected CoreMiddleware(RequestDelegate next)
        {
            this.Next = next;
        }

        public RequestDelegate Next { get; private set; }
        //public Task InvokeAsync(HttpContext context, RequestDelegate next)
        //{
        //    this.Next = next;
        //    return Invoke(context);
        //}
        public abstract Task Invoke(HttpContext context);
    }
}
#endif