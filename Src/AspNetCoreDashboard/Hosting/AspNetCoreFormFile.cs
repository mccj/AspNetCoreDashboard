using System.IO;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreDashboard.Hosting
{
  /// <summary>将 ASP.NET Core <see cref="IFormFile"/> 适配为 <see cref="IUiFormFile"/>。</summary>
  internal sealed class AspNetCoreFormFile : IUiFormFile
  {
    private readonly IFormFile _file;

    public AspNetCoreFormFile(IFormFile file)
    {
      _file = file;
    }

    public string FileName => _file.FileName;
    public string ContentType => _file.ContentType;
    public Task<Stream> OpenReadStreamAsync() => Task.FromResult(_file.OpenReadStream());
  }
}
