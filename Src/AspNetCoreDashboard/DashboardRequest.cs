﻿// This file is part of Hangfire.
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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetCoreDashboard.Dashboard
{
    public abstract class DashboardRequest
    {
        public abstract string Method { get; }
        public abstract string Host { get; }
        public abstract string Path { get; }
        public abstract string PathBase { get; }
        public abstract string GetQuery(string key);
        public abstract string LocalIpAddress { get; }
        public abstract string RemoteIpAddress { get; }
        public abstract Task<IEnumerable<string>> GetFormValuesAsync(string key);
        public abstract Task<string> GetFormValueAsync(string key);
        public abstract System.IO.Stream Body { get; }
        public abstract IEnumerable<string> GetHeaders(string key);
        public abstract string GetHeader(string key);
#if NETSTANDARD
        public abstract Task<Microsoft.AspNetCore.Http.IFormFile> GetFileAsync(string key);
        public abstract Task<IEnumerable<Microsoft.AspNetCore.Http.IFormFile>> GetFilesAsync(string key);
        public abstract Task<T> GetBodyModelBinderAsync<T>(string modelName = null);
#else
        //public abstract Task<System.Web.HttpPostedFileBase> GetFileAsync(string key);
        //public abstract Task<IEnumerable<System.Web.HttpPostedFileBase>> GetFilesAsync(string key);
#endif
    }
}