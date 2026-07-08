using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Microsoft.Owin;

namespace AspNetCoreDashboard.Owin.Hosting
{
  internal sealed class OwinUiContext : IUiContext
  {
    private readonly IOwinContext _context;

    public OwinUiContext(IOwinContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IServiceProvider Services => null;
    public string Method => _context.Request.Method;
    public string Path => _context.Request.Path.Value ?? string.Empty;
    public Match RouteMatch { get; set; }
    public CancellationToken RequestAborted => _context.Request.CallCancelled;
    public ClaimsPrincipal User => _context.Request.User as ClaimsPrincipal;
    public string LocalIpAddress => _context.Request.LocalIpAddress;
    public string RemoteIpAddress => _context.Request.RemoteIpAddress;

    public int StatusCode
    {
      get => _context.Response.StatusCode;
      set => _context.Response.StatusCode = value;
    }

    public Task<string> GetQueryAsync(string name)
    {
      return Task.FromResult(_context.Request.Query.Get(name));
    }

    public async Task<string> GetFormValueAsync(string name)
    {
      var form = await _context.Request.ReadFormAsync();
      return form.Get(name);
    }

    public async Task<IUiFormFile> GetFormFileAsync(string name)
    {
      var contentType = _context.Request.ContentType ?? GetRequestHeader("Content-Type");
      if (string.IsNullOrEmpty(contentType) ||
          contentType.IndexOf("multipart/form-data", StringComparison.OrdinalIgnoreCase) < 0)
      {
        return null;
      }

      var body = await ReadBodyAsBytesAsync();
      return OwinMultipartFormReader.TryGetFile(contentType, body, name);
    }

    private async Task<byte[]> ReadBodyAsBytesAsync()
    {
      const string bufferKey = "AspNetCoreDashboard.RequestBodyBuffer";
      if (_context.Environment.TryGetValue(bufferKey, out var buffered) && buffered is byte[] bytes)
        return bytes;

      using (var memory = new MemoryStream())
      {
        await _context.Request.Body.CopyToAsync(memory);
        return memory.ToArray();
      }
    }

    public async Task<string> ReadBodyAsStringAsync()
    {
      using (var reader = new StreamReader(_context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
      {
        return await reader.ReadToEndAsync();
      }
    }

    public Task<Stream> OpenRequestBodyAsync()
    {
      return Task.FromResult<Stream>(_context.Request.Body);
    }

    public async Task<T> ReadJsonAsync<T>()
    {
      var json = await ReadBodyAsStringAsync();
      if (string.IsNullOrWhiteSpace(json))
        return default;

      return JsonSerializer.Deserialize<T>(json);
    }

    public string GetRequestHeader(string name)
    {
      if (string.IsNullOrEmpty(name))
        return null;

      var header = _context.Request.Headers.FirstOrDefault(h =>
          string.Equals(h.Key, name, StringComparison.OrdinalIgnoreCase));
      return header.Value != null && header.Value.Length > 0 ? header.Value[0] : null;
    }

    public string GetRequestCookie(string name)
    {
      if (string.IsNullOrEmpty(name))
        return null;

      var cookieHeader = GetRequestHeader("Cookie");
      if (string.IsNullOrEmpty(cookieHeader))
        return null;

      foreach (var part in cookieHeader.Split(';'))
      {
        var segment = part.Trim();
        var separator = segment.IndexOf('=');
        if (separator <= 0)
          continue;

        var cookieName = segment.Substring(0, separator).Trim();
        if (!string.Equals(cookieName, name, StringComparison.OrdinalIgnoreCase))
          continue;

        return segment.Substring(separator + 1).Trim();
      }

      return null;
    }

    public void SetResponseHeader(string name, string value)
    {
      if (string.IsNullOrEmpty(name))
        return;

      _context.Response.Headers[name] = value;
    }

    public void SetCookie(
        string name,
        string value,
        DateTimeOffset? expires = null,
        string path = null,
        bool httpOnly = false,
        bool secure = false,
        UiCookieSameSite sameSite = UiCookieSameSite.Unspecified)
    {
      var parts = new System.Collections.Generic.List<string>
            {
                $"{name}={value ?? string.Empty}",
                $"Path={path ?? "/"}"
            };

      if (expires.HasValue)
        parts.Add($"Expires={expires.Value.UtcDateTime:R}");

      if (httpOnly)
        parts.Add("HttpOnly");

      if (secure)
        parts.Add("Secure");

      if (sameSite != UiCookieSameSite.Unspecified)
        parts.Add("SameSite=" + sameSite.ToString());

      _context.Response.Headers.AppendValues("Set-Cookie", string.Join("; ", parts));
    }

    public Task WriteAsync(string content, string contentType = "text/plain")
    {
      _context.Response.ContentType = contentType;
      return _context.Response.WriteAsync(content ?? string.Empty);
    }

    public Task WriteJsonAsync(object value)
    {
      _context.Response.ContentType = "application/json; charset=utf-8";
      return _context.Response.WriteAsync(JsonSerializer.Serialize(value));
    }

    public async Task WriteStreamAsync(Stream stream, string contentType, string downloadFileName = null)
    {
      if (stream == null) throw new ArgumentNullException(nameof(stream));

      _context.Response.ContentType = contentType;
      if (!string.IsNullOrEmpty(downloadFileName))
        _context.Response.Headers["Content-Disposition"] = $"attachment; filename=\"{downloadFileName}\"";

      await stream.CopyToAsync(_context.Response.Body);
    }

    public Task RedirectAsync(string location, bool permanent = false)
    {
      if (string.IsNullOrEmpty(location))
        throw new ArgumentException("必须提供 Location。", nameof(location));

      _context.Response.StatusCode = permanent ? 308 : 302;
      _context.Response.Headers["Location"] = location;
      return Task.CompletedTask;
    }

    internal IOwinContext GetOwinContext() => _context;
  }

  internal static class OwinAssemblyExtensions
  {
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> Cache
        = new System.Collections.Concurrent.ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static Stream GetManifestResourceStreamIgnoreCase(this Assembly assembly, string name)
    {
      var key = assembly.FullName + "|" + name;
      var resourceName = Cache.GetOrAdd(key, _ =>
          assembly.GetManifestResourceNames().FirstOrDefault(f => f.Equals(name, StringComparison.OrdinalIgnoreCase)));
      return resourceName == null ? null : assembly.GetManifestResourceStream(resourceName);
    }
  }
}
