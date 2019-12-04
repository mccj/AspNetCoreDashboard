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
using System.Reflection;
using System.Threading.Tasks;

namespace AspNetCoreDashboard.Dashboard
{
    public class EmbeddedResourceDispatcher : IDashboardDispatcher
    {
        private readonly Assembly _assembly;
        private readonly string _baseNamespace;
        private readonly Func<string, string> _contentTypeFun;
        private readonly string _path;
        private readonly string _resourceName;

        internal EmbeddedResourceDispatcher(
            [NotNull] string path,
            [NotNull] Func<string, string> contentTypeFun,
            [NotNull] Assembly assembly,
            string baseNamespace)
        {
            _path = path;
            _assembly = assembly;
            _baseNamespace = baseNamespace;
            _contentTypeFun = contentTypeFun;
        }
        public EmbeddedResourceDispatcher(
            [NotNull] string contentType,
            [NotNull] Assembly assembly,
            string resourceName)
        {
            if (contentType == null) throw new ArgumentNullException(nameof(contentType));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            _assembly = assembly;
            _resourceName = resourceName;
            _contentTypeFun = (path) => contentType;
        }
        public Task Dispatch(IDashboardContext context)
        {
            if (string.IsNullOrWhiteSpace(_resourceName))
            {
                var path = context.UriMatch.Groups[_path].Value;

                context.Response.ContentType = _contentTypeFun(path);
                context.Response.SetExpire(DateTimeOffset.Now.AddYears(1));

                var resourceName = _baseNamespace + "." + getPath(path);
                WriteResponse(context.Response, resourceName);
            }
            else
            {
                context.Response.ContentType = _contentTypeFun(_resourceName);
                WriteResponse(context.Response, _resourceName);
            }

            return Task.FromResult(true);
        }
        protected virtual void WriteResponse(DashboardResponse response)
        {
            WriteResource(response, _assembly, _resourceName);
        }
        protected virtual void WriteResponse(DashboardResponse response, string resourceName)
        {
            WriteResource(response, _assembly, resourceName);
        }
        protected void WriteResource(DashboardResponse response, Assembly assembly, string resourceName)
        {
            using (var inputStream = assembly.GetManifestResourceStreamIgnoreCase(resourceName))
            {
                if (inputStream == null)
                {
                    throw new ArgumentException($@"Resource with name {resourceName} not found in assembly {assembly}.");
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
