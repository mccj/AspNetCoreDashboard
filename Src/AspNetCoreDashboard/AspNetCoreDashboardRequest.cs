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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetCoreDashboard.Annotations;
#if NETSTANDARD
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;
#else
using HttpContext = Microsoft.Owin.IOwinContext;
#endif
namespace AspNetCoreDashboard.Dashboard
{
    internal sealed class AspNetCoreDashboardRequest : DashboardRequest
    {
        private readonly HttpContext _context;

        public AspNetCoreDashboardRequest([NotNull] HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _context = context;
        }
       
        public override string Method => _context.Request.Method;
        public override string Path => _context.Request.Path.Value;
        public override string PathBase => _context.Request.PathBase.Value;
        public override string GetQuery(string key) => _context.Request.Query[key];
        public override string LocalIpAddress =>
#if NETSTANDARD
    _context.Connection.LocalIpAddress.ToString();
#else
        _context.Request.LocalIpAddress;
#endif
        public override string RemoteIpAddress =>
#if NETSTANDARD
    _context.Connection.RemoteIpAddress.ToString();
#else
        _context.Request.RemoteIpAddress;
#endif
        public override async Task<IList<string>> GetFormValuesAsync(string key)
        {
#if NETSTANDARD
            return await Task.FromResult(_context.Request.Form[key]);
            //return form[key];
#else
            var form = await _context.Request.ReadFormAsync();
            return form.GetValues(key);
#endif
        }
        public override System.IO.Stream Body => _context.Request.Body;

#if NETSTANDARD
        public override async Task<Microsoft.AspNetCore.Http.IFormFile> GetFileAsync(string key)
        {
            var form = await _context.Request.ReadFormAsync();
            return form.Files[key];
        }
        public override async Task<IEnumerable<Microsoft.AspNetCore.Http.IFormFile>> GetFilesAsync(string key)
        {
            var form = await _context.Request.ReadFormAsync();
            return form.Files.GetFiles(key);
        }
#else
        //public override Task<System.Web.HttpPostedFileBase> GetFileAsync(string key)
        //{
        //    //var form = await _context.Request.ReadFormAsync();
        //    //return form.Files[key];
        //    return null;
        //}
        //public override Task<IEnumerable<System.Web.HttpPostedFileBase>> GetFilesAsync(string key)
        //{
        //    //var form = await _context.Request.ReadFormAsync();
        //    //return form.Files.GetFiles(key);
        //    return null;
        //}
#endif
    }
}
