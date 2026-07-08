using System.IO;
using System.Threading.Tasks;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>与宿主无关的上传表单文件抽象。</summary>
  public interface IUiFormFile
  {
    /// <summary>客户端提供的原始文件名。</summary>
    string FileName { get; }

    /// <summary>声明的内容类型；未知时为 null。</summary>
    string ContentType { get; }

    /// <summary>打开上传流以供读取。</summary>
    Task<Stream> OpenReadStreamAsync();
  }
}
