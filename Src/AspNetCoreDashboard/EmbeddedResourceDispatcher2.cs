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
    internal class EmbeddedResourceDispatcher2 : IDashboardDispatcher
    {
        private readonly Assembly _assembly;
        private readonly string _baseNamespace;
        private readonly string _contentType;
        private readonly string _path;

        public EmbeddedResourceDispatcher2(
            [NotNull] string path,
            [NotNull] string contentType,
            [NotNull] Assembly assembly,
            string baseNamespace)
        {
            _path = path;
            _assembly = assembly;
            _baseNamespace = baseNamespace;
            _contentType = contentType;
        }
        public Task Dispatch(IDashboardContext context)
        {
            context.Response.ContentType = _contentType;
            context.Response.SetExpire(DateTimeOffset.Now.AddYears(1));

            var path = context.UriMatch.Groups[_path].Value;

            WriteResponse(context.Response, _baseNamespace + "." + getPath(path));

            return Task.FromResult(true);
        }
        protected virtual void WriteResponse(DashboardResponse response, string _resourceName)
        {
            WriteResource(response, _assembly, _resourceName);
        }
        protected void WriteResource(DashboardResponse response, Assembly assembly, string resourceName)
        {
            using (var inputStream = assembly.GetManifestResourceStream(resourceName))
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
