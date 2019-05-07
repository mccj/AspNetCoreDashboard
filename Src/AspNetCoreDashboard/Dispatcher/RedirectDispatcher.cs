// This file is part of Hangfire.
// Copyright © 2013-2014 Sergey Odinokov.
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

using System;
using System.Reflection;
using System.Threading.Tasks;
using AspNetCoreDashboard.Annotations;

namespace AspNetCoreDashboard.Dashboard
{
    public class RedirectDispatcher : IDashboardDispatcher
    {
        private readonly Func<System.Text.RegularExpressions.Match, string> _redirectLocationFun;

        public RedirectDispatcher(
            [NotNull] string redirectLocation
            ) : this((uriMatch) => redirectLocation)
        {
        }

        public RedirectDispatcher(
            [NotNull] Func<System.Text.RegularExpressions.Match, string> redirectLocationFun
            )
        {
            if (redirectLocationFun == null) throw new ArgumentNullException(nameof(redirectLocationFun));

            _redirectLocationFun = redirectLocationFun;
        }
        public Task Dispatch(IDashboardContext context)
        {
            var uriMatch = context.UriMatch;
            var pathBase = context.Request.PathBase;

            context.Response.Redirect(pathBase+_redirectLocationFun(uriMatch));
            return Task.FromResult(true);
        }
    }
}
