using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.RED.FileFormats
{
    internal static class StreamHelpers
    {
        public static int ReadValueEncodedS32(this Stream stream)
        {
            var op = stream.ReadValueU8();

            uint value = (byte)(op & 0x3F);

            if ((op & 0x40) != 0)
            {
                int shift = 6;
                byte extra;
                do
                {
                    if (shift > 27)
                    {
                        throw new InvalidOperationException();
                    }

                    extra = stream.ReadValueU8();
                    value |= (uint)(extra & 0x7F) << shift;
                    shift += 7;
                }
                while ((extra & 0x80) != 0);
            }

            if ((op & 0x80) != 0)
            {
                return -(int)value;
            }

            return (int)value;
        }

        public static string ReadStringEncoded(this Stream stream)
        {
            var length = stream.ReadValueEncodedS32();

            if (length < 0 || length >= 0x10000)
            {
                throw new InvalidOperationException();
            }

            return stream.ReadString(length, true, Encoding.ASCII);
        }

        public static string ReadStringEncodedUnicode(this Stream stream)
        {
            var length = stream.ReadValueEncodedS32();

            if (length < 0)
            {
                length = -length;
                if (length >= 0x10000)
                {
                    throw new InvalidOperationException();
                }

                // ASCII
                return stream.ReadString(length, true, Encoding.ASCII);
            }
            else
            {
                if (length >= 0x10000)
                {
                    throw new InvalidOperationException();
                }

                // Unicode
                return stream.ReadString(length * 2, true, Encoding.Unicode);
            }
        }
    }
}
