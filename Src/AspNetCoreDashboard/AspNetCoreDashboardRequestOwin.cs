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

#if NETFULL
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetCoreDashboard.Annotations;
#if !NETFULL
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;
#else
using HttpContext = Microsoft.Owin.IOwinContext;
#endif
namespace AspNetCoreDashboard.Dashboard
{
    internal sealed class AspNetCoreDashboardRequestOwin : DashboardRequest
    {
        private readonly HttpContext _context;
        //private readonly Microsoft.AspNetCore.Mvc.MvcOptions _mvcOptions;
        public AspNetCoreDashboardRequestOwin([NotNull] HttpContext context//,
                                                                           //Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Mvc.MvcOptions> optionsAccessor
            )
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _context = context;
            //_mvcOptions = optionsAccessor.Value;
        }

        public override string Method => _context.Request.Method;
        public override string Path => _context.Request.Path.Value;
        public override string PathBase => _context.Request.PathBase.Value;
        public override string GetQuery(string key) => _context.Request.Query[key];
        public override string LocalIpAddress =>
#if !NETFULL
            _context.Connection.LocalIpAddress.ToString();
#else
            _context.Request.LocalIpAddress;
#endif
        public override string RemoteIpAddress =>
#if !NETFULL
            _context.Connection.RemoteIpAddress.ToString();
#else
            _context.Request.RemoteIpAddress;
#endif
        public override IEnumerable<string> GetHeaders(string key) =>
#if !NETFULL
            _context.Request.Headers[key];
#else
            _context.Request.Headers.GetValues(key);
#endif
        public override async Task<IEnumerable<string>> GetFormValuesAsync(string key)
        {
#if !NETFULL
            return await Task.FromResult(_context.Request.Form[key]);
            //return form[key];
#else
            var form = await _context.Request.ReadFormAsync();
            return form.GetValues(key);
#endif
        }
        public override string GetHeader(string key) => GetHeaders(key)?.FirstOrDefault();
        public override async Task<string> GetFormValueAsync(string key)
        {
            var r = await GetFormValuesAsync(key);
            return r?.FirstOrDefault();
        }
        public override System.IO.Stream Body => _context.Request.Body;

#if NETSTANDARD
        public override Task<Microsoft.AspNetCore.Http.IFormFile> GetFileAsync(string key)
        {
            throw new NotImplementedException();
            //var form = await _context.Request.ReadFormAsync();
            //return form.Files[key];
        }
        public override Task<IEnumerable<Microsoft.AspNetCore.Http.IFormFile>> GetFilesAsync(string key)
        {
            throw new NotImplementedException();
            //var form = await _context.Request.ReadFormAsync();
            //return form.Files.GetFiles(key);
        }
        public override Task<T> GetBodyModelBinderAsync<T>(string modelName = null)
        {
            throw new NotImplementedException();

            //var optionsAccessor = _context.RequestServices.GetService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Mvc.MvcOptions>>();

            //var modelType = typeof(T);
            //var contentTypes = new MediaTypeCollection();

            //var provider = new EmptyModelMetadataProvider();
            //var metadata = provider.GetMetadataForType(modelType);
            //var formatterContext = new InputFormatterContext(_context.Request.HttpContext,
            //         modelName: modelName ?? string.Empty,
            //         modelState: new ModelStateDictionary(),
            //         metadata: metadata,
            //         readerFactory: (stream, encoding) => new System.IO.StreamReader(stream, encoding));

            //foreach (var formatter in optionsAccessor.Value.InputFormatters)
            //{
            //    var canRead = formatter.CanRead(formatterContext);
            //    if (canRead)
            //    {
            //        var result = await formatter.ReadAsync(formatterContext);
            //        if (result.HasError)
            //        {
            //            // Formatter encountered an error. Do not use the model it returned.
            //            //_logger?.DoneAttemptingToBindModel(bindingContext);
            //            return default(T);
            //        }

            //        if (result.IsModelSet)
            //        {
            //            var model = result.Model;
            //            return (T)model;
            //            //bindingContext.Result = ModelBindingResult.Success(model);
            //        }
            //    }
            //}
            //return default(T);
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
#endif