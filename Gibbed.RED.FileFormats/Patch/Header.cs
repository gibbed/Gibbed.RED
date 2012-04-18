/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

namespace Gibbed.RED.FileFormats.Patch
{
    public class Header
    {
        public uint Magic;
        public ushort DateYear;
        public ushort DateMonth;
        public ushort DateDay;
        public ushort DateUnk0;
        public ushort DateHour;
        public ushort DateMinutes;
        public ushort DateSeconds;
        public ushort DateUnk1;
        public string Host;
        public string Creator;
        public uint FileCount;
        public uint Flags;
        public uint FileTableOffset;

        private static void HashU16(ref ulong hash, ushort value)
        {
            for (int i = 0; i < 2; i++)
            {
                hash ^= (byte)(value & 0xFF);
                hash *= 0x00000100000001B3UL;
                value >>= 8;
            }
        }

        private static void HashU32(ref ulong hash, uint value)
        {
            for (int i = 0; i < 4; i++)
            {
                hash ^= (byte)(value & 0xFF);
                hash *= 0x00000100000001B3UL;
                value >>= 8;
            }
        }

        private static void HashString(ref ulong hash, string value)
        {
            int i = 0;

            if (string.IsNullOrEmpty(value) == false)
            {
                for (; i < value.Length && i < 64; i++)
                {
                    hash ^= (byte)value[i];
                    hash *= 0x00000100000001B3UL;
                }
            }

            for (; i < 64; i++)
            {
                hash ^= 0;
                hash *= 0x00000100000001B3UL;
            }
        }

        public ulong Hash
        {
            get
            {
                var hash = 0UL;
                HashU32(ref hash, this.Magic);
                HashU16(ref hash, this.DateYear);
                HashU16(ref hash, this.DateMonth);
                HashU16(ref hash, this.DateDay);
                HashU16(ref hash, this.DateUnk0);
                HashU16(ref hash, this.DateHour);
                HashU16(ref hash, this.DateMinutes);
                HashU16(ref hash, this.DateSeconds);
                HashU16(ref hash, this.DateUnk1);
                HashString(ref hash, this.Host);
                HashString(ref hash, this.Creator);
                HashU32(ref hash, this.FileCount);
                HashU32(ref hash, this.Flags);
                HashU32(ref hash, this.FileTableOffset);
                return hash;
            }
        }
    }
}
