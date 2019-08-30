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

using AspNetCoreDashboard.Annotations;
using System;
using System.Threading.Tasks;

namespace AspNetCoreDashboard.Dashboard
{
    public class PhysicalFileDispatcher : IDashboardDispatcher
    {
        private readonly string _basePath;
        private readonly Func<string, string> _contentTypeFun;
        private readonly string _staticPath;
        private readonly string _path;

        internal PhysicalFileDispatcher(
            [NotNull] string path,
            [NotNull] Func<string, string> contentTypeFun,
            string basePath)
        {
            _path = path;
            _basePath = basePath;
            _contentTypeFun = contentTypeFun;
        }
        public PhysicalFileDispatcher(
            [NotNull] string contentType,
            string staticPath)
        {
            if (contentType == null) throw new ArgumentNullException(nameof(contentType));

            _staticPath = staticPath;
            _contentTypeFun = (p) => contentType;
        }
        public Task Dispatch(IDashboardContext context)
        {
            if (string.IsNullOrWhiteSpace(_staticPath))
            {
                var path = context.UriMatch.Groups[_path].Value;

                context.Response.ContentType = _contentTypeFun(path);
                context.Response.SetExpire(DateTimeOffset.Now.AddYears(1));

                var resourceName = _basePath + "." + getPath(path);
                WriteResponse(context.Response, resourceName);
            }
            else
            {
                context.Response.ContentType = _contentTypeFun(_staticPath);
                WriteResponse(context.Response, _staticPath);
            }

            return Task.FromResult(true);
        }
        protected virtual void WriteResponse(DashboardResponse response)
        {
            WriteResource(response, _path);
        }
        protected virtual void WriteResponse(DashboardResponse response, string path)
        {
            WriteResource(response, path);
        }
        protected void WriteResource(DashboardResponse response, string path)
        {
            var dllPath = System.IO.Path.Combine(AppContext.BaseDirectory, path);
            if (!System.IO.File.Exists(path) && System.IO.File.Exists(dllPath))
                path = dllPath;

            using (var inputStream = System.IO.File.OpenRead(path))
            {
                if (inputStream == null)
                {
                    throw new ArgumentException($@"Path with name {path} not found in file.");
                }

                inputStream.CopyTo(response.Body);
            }
        }
        public string getPath(string path)
        {
            var fileName = System.IO.Path.GetFileName(path);
            var directoryName = System.IO.Path.GetDirectoryName(path);
            return directoryName.Replace("/", ".").Replace("\\", ".").Replace("-", "_")
                + (string.IsNullOrWhiteSpace(directoryName) ? "" : ".")
                + fileName;
        }
    }
}
