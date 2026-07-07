using System.Web;

namespace AspNetCoreDashboard.Owin.Hosting
{
  internal static class OwinContentTypeResolver
  {
    public static string GetContentType(string path)
    {
      if (string.IsNullOrEmpty(path))
        return "text/html";

      return MimeMapping.GetMimeMapping(path);
    }
  }
}
