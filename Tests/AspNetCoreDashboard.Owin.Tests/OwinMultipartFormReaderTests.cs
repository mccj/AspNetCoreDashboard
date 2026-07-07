using System.Text;
using AspNetCoreDashboard.Owin.Hosting;
using Xunit;

namespace AspNetCoreDashboard.Owin.Tests
{
  public sealed class OwinMultipartFormReaderTests
  {
    [Fact]
    public void TryGetFile_parses_multipart_body()
    {
      var body = Encoding.UTF8.GetBytes(
          "--abc\r\n" +
          "Content-Disposition: form-data; name=\"file\"; filename=\"test.txt\"\r\n" +
          "\r\n" +
          "data\r\n" +
          "--abc--\r\n");

      var file = OwinMultipartFormReader.TryGetFile("multipart/form-data; boundary=abc", body, "file");
      Assert.NotNull(file);
      Assert.Equal("test.txt", file!.FileName);
    }
  }
}
