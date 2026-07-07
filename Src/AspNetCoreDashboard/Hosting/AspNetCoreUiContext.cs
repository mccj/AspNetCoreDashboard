using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreDashboard.Hosting
{
  internal sealed class AspNetCoreUiContext : IUiContext
  {
    private readonly HttpContext _httpContext;

    public AspNetCoreUiContext(HttpContext httpContext)
    {
      _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
    }

    public IServiceProvider? Services => _httpContext.RequestServices;
    public string Method => _httpContext.Request.Method;
    public string Path => _httpContext.Request.Path.Value ?? string.Empty;
    public Match? RouteMatch { get; set; }
    public CancellationToken RequestAborted => _httpContext.RequestAborted;
    public ClaimsPrincipal? User => _httpContext.User;
    public string? LocalIpAddress => _httpContext.Connection.LocalIpAddress?.ToString();
    public string? RemoteIpAddress => _httpContext.Connection.RemoteIpAddress?.ToString();

    public int StatusCode
    {
      get => _httpContext.Response.StatusCode;
      set => _httpContext.Response.StatusCode = value;
    }

    public Task<string?> GetQueryAsync(string name)
    {
      return Task.FromResult<string?>(_httpContext.Request.Query[name].ToString());
    }

    public async Task<string?> GetFormValueAsync(string name)
    {
      if (!_httpContext.Request.HasFormContentType)
        return null;

      var form = await _httpContext.Request.ReadFormAsync();
      return form[name].ToString();
    }

    public async Task<IUiFormFile?> GetFormFileAsync(string name)
    {
      if (!_httpContext.Request.HasFormContentType)
        return null;

      var form = await _httpContext.Request.ReadFormAsync();
      var file = form.Files.GetFile(name);
      return file == null ? null : new AspNetCoreFormFile(file);
    }

    public async Task<string> ReadBodyAsStringAsync()
    {
      _httpContext.Request.EnableBuffering();
      _httpContext.Request.Body.Position = 0;

      using (var reader = new StreamReader(_httpContext.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
      {
        var body = await reader.ReadToEndAsync();
        _httpContext.Request.Body.Position = 0;
        return body;
      }
    }

    public Task<Stream> OpenRequestBodyAsync()
    {
      _httpContext.Request.EnableBuffering();
      _httpContext.Request.Body.Position = 0;
      return Task.FromResult<Stream>(_httpContext.Request.Body);
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
      if (string.IsNullOrEmpty(name))
        return null;

      return _httpContext.Request.Headers[name].ToString();
    }

    public string? GetRequestCookie(string name)
    {
      if (string.IsNullOrEmpty(name))
        return null;

      return _httpContext.Request.Cookies[name];
    }

    public void SetResponseHeader(string name, string value)
    {
      if (string.IsNullOrEmpty(name))
        return;

      _httpContext.Response.Headers[name] = value;
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
      var cookieOptions = new CookieOptions
      {
        HttpOnly = httpOnly,
        Secure = secure,
        Path = path ?? "/",
        SameSite = MapSameSite(sameSite)
      };

      if (expires.HasValue)
        cookieOptions.Expires = expires.Value;

      _httpContext.Response.Cookies.Append(name, value ?? string.Empty, cookieOptions);
    }

    private static SameSiteMode MapSameSite(UiCookieSameSite sameSite)
    {
      switch (sameSite)
      {
        case UiCookieSameSite.Lax: return SameSiteMode.Lax;
        case UiCookieSameSite.Strict: return SameSiteMode.Strict;
        case UiCookieSameSite.None: return SameSiteMode.None;
        default: return SameSiteMode.Unspecified;
      }
    }

    public Task WriteAsync(string content, string contentType = "text/plain")
    {
      _httpContext.Response.ContentType = contentType;
      return _httpContext.Response.WriteAsync(content ?? string.Empty);
    }

    public Task WriteJsonAsync(object value)
    {
      _httpContext.Response.ContentType = "application/json; charset=utf-8";
      var json = JsonSerializer.Serialize(value);
      return _httpContext.Response.WriteAsync(json);
    }

    public async Task WriteStreamAsync(Stream stream, string contentType, string? downloadFileName = null)
    {
      if (stream == null) throw new ArgumentNullException(nameof(stream));

      _httpContext.Response.ContentType = contentType;
      if (!string.IsNullOrEmpty(downloadFileName))
        _httpContext.Response.Headers.ContentDisposition = $"attachment; filename=\"{downloadFileName}\"";

      await stream.CopyToAsync(_httpContext.Response.Body, RequestAborted);
    }

    public Task RedirectAsync(string location, bool permanent = false)
    {
      if (string.IsNullOrEmpty(location))
        throw new ArgumentException("必须提供 Location。", nameof(location));

      _httpContext.Response.StatusCode = permanent
          ? StatusCodes.Status308PermanentRedirect
          : StatusCodes.Status302Found;
      _httpContext.Response.Headers.Location = location;
      return Task.CompletedTask;
    }

    internal HttpContext GetHttpContext() => _httpContext;
  }
}
