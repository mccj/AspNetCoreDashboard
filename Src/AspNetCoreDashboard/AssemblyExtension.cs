using System;
using System.Linq;

namespace AspNetCoreDashboard.Dashboard
{
   public static class AssemblyExtension
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<string, string> keyValuePairs = new System.Collections.Concurrent.ConcurrentDictionary<string, string>();
        public static System.IO.Stream GetManifestResourceStreamIgnoreCase(this System.Reflection.Assembly assembly, string name)
        {
            var key = assembly.FullName + name;
            var r = keyValuePairs.GetOrAdd(key, keyName =>
            {
                return assembly.GetManifestResourceNames().FirstOrDefault(f => f.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            });
            return assembly.GetManifestResourceStream(r);
        }
    }
    //public static class HttpRequestExtension
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="httpRequest"></param>
    //    /// <param name="encoding"></param>
    //    /// <returns></returns>
    //    public static async Task<string> GetRawBodyStringFormater(this HttpRequest httpRequest, Encoding encoding)
    //    {
    //        if (encoding == null)
    //        {
    //            encoding = Encoding.UTF8;
    //        }

    //        using (StreamReader reader = new StreamReader(httpRequest.Body, encoding))
    //        {
    //            return await reader.ReadToEndAsync();
    //        }
    //    }
    //    /// <summary>
    //    /// 二进制
    //    /// </summary>
    //    /// <param name="httpRequest"></param>
    //    /// <param name="encoding"></param>
    //    /// <returns></returns>
    //    public static async Task<byte[]> GetRawBodyBinaryFormater(this HttpRequest httpRequest, Encoding encoding)
    //    {
    //        if (encoding == null)
    //        {
    //            encoding = Encoding.UTF8;
    //        }

    //        using (var reader = new StreamReader(httpRequest.Body, encoding))
    //        {
    //            using (var ms = new MemoryStream())
    //            {
    //                await httpRequest.Body.CopyToAsync(ms);
    //                return ms.ToArray();  // returns base64 encoded string JSON result
    //            }
    //        }
    //    }
    //}
}
