using Microsoft.Extensions.DependencyInjection;
using Owin;
using System;
using System.Linq;

namespace AspNetCoreDashboardOwinSamples
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseDashboardSamples();
        }
    }
}
