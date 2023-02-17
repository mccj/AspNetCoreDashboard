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

#if NETFRAMEWORK
using AspNetCoreDashboard.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
# if NETSTANDARD
using Microsoft.AspNetCore.Mvc;
#endif
using HttpContext = System.Web.HttpContext;

namespace AspNetCoreDashboard.Dashboard
{
    internal sealed class AspNetCoreDashboardRequestMvc : DashboardRequest
    {
        private readonly HttpContext _context;
        //private readonly Microsoft.AspNetCore.Mvc.MvcOptions _mvcOptions;
        public AspNetCoreDashboardRequestMvc([NotNull] HttpContext context//,
                                                                          //Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Mvc.MvcOptions> optionsAccessor
            )
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _context = context;
            //_mvcOptions = optionsAccessor.Value;
        }

        public override string Method => _context.Request.HttpMethod;
        public override string Host => _context.Request.Url.Authority;
        public override string Path => _context.Request.Path;
        public override string PathBase => _context.Request.ApplicationPath;
        public override string GetQuery(string key) => _context.Request.QueryString[key];
        public override string LocalIpAddress => Dns.GetHostAddresses(Dns.GetHostName())?.FirstOrDefault()?.ToString();
        public override string RemoteIpAddress
        {
            get
            {
                var ip = string.Empty;
                if (!string.IsNullOrEmpty(_context.Request.ServerVariables["HTTP_VIA"]))//HTTP_VIA
                    ip = _context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                else
                    ip = _context.Request.ServerVariables["REMOTE_ADDR"];
                if (string.IsNullOrEmpty(ip))
                    ip = _context.Request.UserHostAddress;
                return ip;
            }
        }
        public override IEnumerable<string> GetHeaders(string key) => _context.Request.Headers.GetValues(key);
        public override async Task<IEnumerable<string>> GetFormValuesAsync(string key)
        {
            var form =  _context.Request.Form.GetValues(key);
            return form;
        }
        public override string GetHeader(string key) => GetHeaders(key)?.FirstOrDefault();
        public override async Task<string> GetFormValueAsync(string key)
        {
            var r = await GetFormValuesAsync(key);
            return r?.FirstOrDefault();
        }
        public override System.IO.Stream Body => _context.Request.InputStream;

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
