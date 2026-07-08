using System.IO;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Microsoft.Owin;

namespace AspNetCoreDashboard.Owin.Hosting
{
  /// <summary>将 OWIN 多部分表单字段适配为 <see cref="IUiFormFile"/>。</summary>
  internal sealed class OwinFormFile : IUiFormFile
  {
    private readonly string _fileName;
    private readonly string _contentType;
    private readonly byte[] _bytes;

    public OwinFormFile(string fileName, string contentType, byte[] bytes)
    {
      _fileName = fileName;
      _contentType = contentType;
      _bytes = bytes;
    }

    public string FileName => _fileName;
    public string ContentType => _contentType;
    public Task<Stream> OpenReadStreamAsync() => Task.FromResult<Stream>(new MemoryStream(_bytes, writable: false));
  }
}
