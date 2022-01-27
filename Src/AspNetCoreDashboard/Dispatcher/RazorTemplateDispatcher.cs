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

using RazorGenerator.Templating;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AspNetCoreDashboard.Dashboard
{
    public class RazorTemplateDispatcher : IDashboardDispatcher
    {
        private readonly Func<Match, RazorTemplateBase> _pageFunc;

        public RazorTemplateDispatcher(Func<Match, RazorTemplateBase> pageFunc)
        {
            _pageFunc = pageFunc;
        }

        public async Task DispatchAsync(IDashboardContext context)
        {
            context.Response.ContentType = "text/html";

            var page = _pageFunc(context.UriMatch);
            //page.Assign(context);
            var html = page.TransformText();

            await context.Response.WriteAsync(html);
        }
    }
}
