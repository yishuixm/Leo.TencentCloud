using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Leo.TencentCloud.Cos
{
    public static class StreamExtension
    {
        public static async Task<byte[]> ToBytesAsync(this Stream stream)
        {
            byte[] data;
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                data = memoryStream.ToArray();
            }

            return data;
        }
    }
}
