using System.IO;
using System.Threading.Tasks;
using System.Web;
using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboard.SystemWeb
{
  internal sealed class SystemWebFormFile : IUiFormFile
  {
    private readonly HttpPostedFile _file;

    public SystemWebFormFile(HttpPostedFile file)
    {
      _file = file;
    }

    public string FileName => _file.FileName;
    public string? ContentType => _file.ContentType;
    public Task<Stream> OpenReadStreamAsync() => Task.FromResult<Stream>(_file.InputStream);
  }
}
