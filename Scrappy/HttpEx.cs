using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Scrappy
{
    public static class HttpEx
    {
        public static async Task<string> GetContentAsString(this HttpResponseMessage response)
        {
            string content;
            if (response.Headers.Any(x => x.Key == "Content-Encoding" && x.Value.Contains("gzip")))
            {
                using (var decompressedStream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress))
                using (var streamReader = new StreamReader(decompressedStream))
                {
                    content = await streamReader.ReadToEndAsync();
                }
            }
            else
            {
                content = await response.Content.ReadAsStringAsync();
            }
            return content;
        }

        public static async Task<byte[]> GetContentAsBytes(this HttpResponseMessage response)
        {
            byte[] content;
            if (response.Headers.Any(x => x.Key == "Content-Encoding" && x.Value.Contains("gzip")))
            {
                using (var decompressedStream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress))
                using (var memoryStream = new MemoryStream())
                {
                    var buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = await decompressedStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await memoryStream.WriteAsync(buffer, 0, bytesRead);
                    }
                    content = memoryStream.ToArray();
                }
            }
            else
            {
                content = await response.Content.ReadAsByteArrayAsync();
            }
            return content;
        }
    }
}



