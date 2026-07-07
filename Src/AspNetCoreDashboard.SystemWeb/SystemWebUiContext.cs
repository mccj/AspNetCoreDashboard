using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboard.SystemWeb
{
  internal sealed class SystemWebUiContext : IUiContext
  {
    private readonly HttpContext _context;

    public SystemWebUiContext(HttpContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IServiceProvider? Services => null;
    public string Method => _context.Request.HttpMethod;
    public string Path => GetRelativePath();
    public Match? RouteMatch { get; set; }
    public CancellationToken RequestAborted => CancellationToken.None;
    public ClaimsPrincipal? User => _context.User as ClaimsPrincipal;
    public string? LocalIpAddress => _context.Request.ServerVariables["LOCAL_ADDR"];
    public string? RemoteIpAddress => _context.Request.UserHostAddress;

    public int StatusCode
    {
      get => _context.Response.StatusCode;
      set => _context.Response.StatusCode = value;
    }

    public Task<string?> GetQueryAsync(string name)
    {
      return Task.FromResult<string?>(_context.Request.QueryString[name]);
    }

    public Task<string?> GetFormValueAsync(string name)
    {
      return Task.FromResult<string?>(_context.Request.Form[name]);
    }

    public Task<IUiFormFile?> GetFormFileAsync(string name)
    {
      var file = _context.Request.Files[name];
      if (file == null || file.ContentLength <= 0)
        return Task.FromResult<IUiFormFile?>(null);

      return Task.FromResult<IUiFormFile?>(new SystemWebFormFile(file));
    }

    public Task<string> ReadBodyAsStringAsync()
    {
      using (var reader = new StreamReader(_context.Request.InputStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
      {
        return reader.ReadToEndAsync();
      }
    }

    public Task<Stream> OpenRequestBodyAsync()
    {
      return Task.FromResult<Stream>(_context.Request.InputStream);
    }

    public async Task<T?> ReadJsonAsync<T>()
    {
      var json = await ReadBodyAsStringAsync();
      if (string.IsNullOrWhiteSpace(json))
        return default;

      return JsonSerializer.Deserialize<T>(json);
    }

    public string? GetRequestHeader(string name)
    {
      return string.IsNullOrEmpty(name) ? null : _context.Request.Headers[name];
    }

    public string? GetRequestCookie(string name)
    {
      return string.IsNullOrEmpty(name) ? null : _context.Request.Cookies[name]?.Value;
    }

    public void SetResponseHeader(string name, string value)
    {
      if (!string.IsNullOrEmpty(name))
        _context.Response.Headers[name] = value;
    }

    public void SetCookie(
        string name,
        string value,
        DateTimeOffset? expires = null,
        string? path = null,
        bool httpOnly = false,
        bool secure = false,
        UiCookieSameSite sameSite = UiCookieSameSite.Unspecified)
    {
      var cookie = new HttpCookie(name, value ?? string.Empty)
      {
        Path = path ?? "/",
        HttpOnly = httpOnly,
        Secure = secure
      };

      if (expires.HasValue)
        cookie.Expires = expires.Value.UtcDateTime;

      if (sameSite != UiCookieSameSite.Unspecified)
        cookie.SameSite = MapSameSite(sameSite);

      _context.Response.Cookies.Add(cookie);
    }

    public Task WriteAsync(string content, string contentType = "text/plain")
    {
      _context.Response.ContentType = contentType;
      _context.Response.Write(content ?? string.Empty);
      return Task.CompletedTask;
    }

    public Task WriteJsonAsync(object value)
    {
      _context.Response.ContentType = "application/json; charset=utf-8";
      _context.Response.Write(JsonSerializer.Serialize(value));
      return Task.CompletedTask;
    }

    public Task WriteStreamAsync(Stream stream, string contentType, string? downloadFileName = null)
    {
      if (stream == null) throw new ArgumentNullException(nameof(stream));

      _context.Response.ContentType = contentType;
      if (!string.IsNullOrEmpty(downloadFileName))
        _context.Response.AddHeader("Content-Disposition", $"attachment; filename=\"{downloadFileName}\"");

      stream.CopyTo(_context.Response.OutputStream);
      return Task.CompletedTask;
    }

    public Task RedirectAsync(string location, bool permanent = false)
    {
      if (string.IsNullOrEmpty(location))
        throw new ArgumentException("必须提供 Location。", nameof(location));

      _context.Response.StatusCode = permanent ? 308 : 302;
      _context.Response.Redirect(location, endResponse: false);
      return Task.CompletedTask;
    }

    private string GetRelativePath()
    {
      var path = _context.Request.Path ?? "/";
      var module = _context.Items["SystemWebUiModulePathPrefix"] as string;
      if (!string.IsNullOrEmpty(module) && path.StartsWith(module, StringComparison.OrdinalIgnoreCase))
        return path.Substring(module.Length).TrimStart('/');

      return path.TrimStart('/');
    }

    private static SameSiteMode MapSameSite(UiCookieSameSite sameSite)
    {
      switch (sameSite)
      {
        case UiCookieSameSite.Lax: return SameSiteMode.Lax;
        case UiCookieSameSite.Strict: return SameSiteMode.Strict;
        case UiCookieSameSite.None: return SameSiteMode.None;
        default: return (SameSiteMode)(-1);
      }
    }
  }
}
