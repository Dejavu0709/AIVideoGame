#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using System.IO;
using System.Text;

using BestHTTP.PlatformSupport.Memory;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;

namespace BestHTTP.Addons.TLSSecurity.Databases.Shared.Utils
{
    public static class StreamUtil
    {
        public static void WriteLengthPrefixedString(this Stream stream, string str)
        {
            if (str != null)
            {
                var byteCount = Encoding.UTF8.GetByteCount(str);

                if (byteCount >= 1 << 16)
                    throw new InvalidParameterException($"byteCount({byteCount})");

                stream.WriteByte((byte)(byteCount >> 8));
                stream.WriteByte((byte)(byteCount));

                byte[] tmp = BufferPool.Get(byteCount, true);

                Encoding.UTF8.GetBytes(str, 0, str.Length, tmp, 0);
                stream.Write(tmp, 0, byteCount);

                BufferPool.Release(tmp);
            }
            else
            {
                stream.WriteByte(0);
                stream.WriteByte(0);
            }
        }

        public static string ReadLengthPrefixedString(this Stream stream)
        {
            int strLength = stream.ReadByte() << 8 | stream.ReadByte();
            string result = null;

            if (strLength != 0)
            {
                byte[] buffer = BufferPool.Get(strLength, true);

                stream.Read(buffer, 0, strLength);
                result = System.Text.Encoding.UTF8.GetString(buffer, 0, strLength);

                BufferPool.Release(buffer);
            }

            return result;
        }
    }
}
#endif
