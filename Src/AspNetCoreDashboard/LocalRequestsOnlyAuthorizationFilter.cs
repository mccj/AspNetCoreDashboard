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

namespace AspNetCoreDashboard.Dashboard
{
    public class LocalRequestsOnlyAuthorizationFilter : IDashboardAuthorizationFilter
    //#if NETFRAMEWORK
    //#pragma warning disable 618
    //        , IAuthorizationFilter
    //#pragma warning restore 618
    //#endif
    {
        public bool Authorize(IDashboardContext context)
        {
            // if unknown, assume not local
            if (String.IsNullOrEmpty(context.Request.RemoteIpAddress))
                return false;

            // check if localhost
            if (context.Request.RemoteIpAddress == "127.0.0.1" || context.Request.RemoteIpAddress == "::1")
                return true;

            // compare with local address
            if (context.Request.RemoteIpAddress == context.Request.LocalIpAddress)
                return true;

            return false;
        }

        //#if NETFRAMEWORK
        //        public bool Authorize(IDictionary<string, object> owinEnvironment)
        //        {
        //            var context = new Microsoft.Owin.OwinContext(owinEnvironment);

        //            // if unknown, assume not local
        //            if (String.IsNullOrEmpty(context.Request.RemoteIpAddress))
        //                return false;

        //            // check if localhost
        //            if (context.Request.RemoteIpAddress == "127.0.0.1" || context.Request.RemoteIpAddress == "::1")
        //                return true;

        //            // compare with local address
        //            if (context.Request.RemoteIpAddress == context.Request.LocalIpAddress)
        //                return true;

        //            return false;
        //        }
        //#endif
    }
}
