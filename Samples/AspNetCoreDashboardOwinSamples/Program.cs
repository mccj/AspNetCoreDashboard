using System;
using Microsoft.Owin.Hosting;
using Topshelf;

namespace AspNetCoreDashboardOwinSamples
{
  class Program
  {
    static void Main(string[] args)
    {
      HostFactory.Run(x =>
      {
        x.Service<WebAppService>(sc =>
              {
                sc.ConstructUsing(name => new WebAppService());
                sc.WhenStarted((s, hostControl) => s.Start(hostControl));
                sc.WhenStopped((s, hostControl) => s.Stop(hostControl));
              });
        x.RunAsLocalSystem();
        x.OnException(ex =>
              {
                System.Console.WriteLine("发生异常 - " + ex.Message);
              });
      });
    }
  }

  class WebAppService : ServiceControl
  {
    public bool Start(HostControl hostControl)
    {
      System.Console.WriteLine("http://localhost:1101/Dashboard/  (Diagnostics: /Diagnostics/api/status)");
      var options = new StartOptions()
      {
        Port = 1101
      };
      WebApp.Start<Startup>(options);
      return true;
    }

    public bool Stop(HostControl hostControl)
    {
      return true;
    }
  }
}
