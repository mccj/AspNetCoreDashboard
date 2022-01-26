#if NETFRAMEWORK
// This file is part of Hangfire.
// Copyright © 2016 Sergey Odinokov.
// 
// Hangfire is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as 
// published by the Free Software Foundation, either version 3 
// of the License, or any later version.
// 
// Hangfire is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public 
// License along with Hangfire. If not, see <http://www.gnu.org/licenses/>.

using AspNetCoreDashboard.Annotations;
//using Microsoft.AspNetCore.Http;
using System;
//using Microsoft.Extensions.DependencyInjection;
#if !NETFRAMEWORK
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;
#else
using HttpContext = Microsoft.Owin.IOwinContext;
#endif
namespace AspNetCoreDashboard.Dashboard
{
    public class AspNetCoreDashboardContextOwin : DashboardContext
    {
        public AspNetCoreDashboardContextOwin(
            //[NotNull] JobStorage storage,
            //[NotNull] DashboardOptions options,
            [NotNull] HttpContext httpContext)
            : base(/*storage, options*/)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            HttpContext = httpContext;
            Request = new AspNetCoreDashboardRequestOwin(httpContext);
            Response = new AspNetCoreDashboardResponseOwin(httpContext);
        }

        public HttpContext HttpContext { get; }

        //public override IBackgroundJobClient GetBackgroundJobClient()
        //{
        //    return HttpContext.RequestServices.GetService<IBackgroundJobClient>() ?? base.GetBackgroundJobClient();
        //}

        //public override IRecurringJobManager GetRecurringJobManager()
        //{
        //    return HttpContext.RequestServices.GetService<IRecurringJobManager>() ?? base.GetRecurringJobManager();
        //}
    }
    public class AspNetCoreDashboardContextOwin<T> : AspNetCoreDashboardContextOwin, IDashboardContext<T>
    {
        public AspNetCoreDashboardContextOwin(
            //[NotNull] JobStorage storage,
            [NotNull] T options,
            [NotNull] HttpContext httpContext)
            : base(httpContext)
        {
            Options = options;
        }
        public T Options { get; }

    }
}
#endif