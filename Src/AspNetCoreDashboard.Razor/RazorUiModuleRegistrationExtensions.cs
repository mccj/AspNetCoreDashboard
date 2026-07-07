using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboard.Razor
{
  /// <summary>
  /// 可选的 Razor 注册扩展。在可用时提供嵌入式 HTML 模板服务。
  /// </summary>
  public static class RazorUiModuleRegistrationExtensions
  {
    /// <summary>
    /// 注册一条 GET 路由，在存在时提供名为 <c>{PageType.Name}.html</c> 的嵌入式 HTML 模板。
    /// </summary>
    public static IUiModuleRegistration MapRazorPage(
        this IUiModuleRegistration registration,
        string pattern,
        Type pageType)
    {
      if (registration == null) throw new ArgumentNullException(nameof(registration));
      if (pattern == null) throw new ArgumentNullException(nameof(pattern));
      if (pageType == null) throw new ArgumentNullException(nameof(pageType));

      return registration.MapGet(pattern, context => WriteRazorTemplate(context, pageType));
    }

    private static async Task WriteRazorTemplate(IUiContext context, Type pageType)
    {
      var assembly = pageType.Assembly;
      var resourceName = assembly.GetManifestResourceNames()
          .FirstOrDefault(name => name.EndsWith("." + pageType.Name + ".html", StringComparison.OrdinalIgnoreCase));

      if (resourceName != null)
      {
        using (var stream = EmbeddedResourceCache.OpenReadStream(assembly, resourceName))
        {
          if (stream != null)
          {
            using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: false))
            {
              var templateHtml = await reader.ReadToEndAsync();
              await context.WriteAsync(templateHtml, "text/html; charset=utf-8");
              return;
            }
          }
        }
      }

      var html = "<!DOCTYPE html><html lang=\"zh-CN\"><head><meta charset=\"utf-8\" />"
          + "<title>" + pageType.Name + "</title></head><body>"
          + "<p>未找到嵌入式资源模板 <code>" + pageType.Name + ".html</code>。</p>"
          + "</body></html>";
      await context.WriteAsync(html, "text/html; charset=utf-8");
    }
  }
}
