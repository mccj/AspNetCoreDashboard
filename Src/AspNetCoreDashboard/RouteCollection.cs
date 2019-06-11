﻿// This file is part of Hangfire.
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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AspNetCoreDashboard.Annotations;

namespace AspNetCoreDashboard.Dashboard
{
    public class RouteCollection
    {
        private readonly Dictionary<string, IDashboardDispatcher> _dispatchers = new Dictionary<string, IDashboardDispatcher>();

#if NETFULL
        //[Obsolete("Use the Add(string, IDashboardDispatcher) overload instead. Will be removed in 2.0.0.")]
        //public void Add([NotNull] string pathTemplate, [NotNull] IRequestDispatcher dispatcher)
        //{
        //    if (pathTemplate == null) throw new ArgumentNullException(nameof(pathTemplate));
        //    if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));

        //    _dispatchers.Add(new Tuple<string, IDashboardDispatcher>(pathTemplate, new RequestDispatcherWrapper(dispatcher)));
        //}
#endif

        public void Add([NotNull] string pathTemplate, [NotNull] IDashboardDispatcher dispatcher)
        {
            if (pathTemplate == null) throw new ArgumentNullException(nameof(pathTemplate));
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));

            if (!_dispatchers.ContainsKey(pathTemplate))
                _dispatchers.Add(pathTemplate, dispatcher);
            else
                _dispatchers[pathTemplate] = dispatcher;
        }

        public Tuple<IDashboardDispatcher, Match> FindDispatcher(string path)
        {
            if (path.Length == 0&& !_dispatchers.ContainsKey(path)) path = "/";

            foreach (var dispatcher in _dispatchers)
            {
                var pattern = dispatcher.Key;

                if (!pattern.StartsWith("^", StringComparison.OrdinalIgnoreCase))
                    pattern = "^" + pattern;
                if (!pattern.EndsWith("$", StringComparison.OrdinalIgnoreCase))
                    pattern += "$";

                var match = Regex.Match(
                    path,
                    pattern,
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (match.Success)
                {
                    return new Tuple<IDashboardDispatcher, Match>(dispatcher.Value, match);
                }
            }
            
            return null;
        }
    }
}
