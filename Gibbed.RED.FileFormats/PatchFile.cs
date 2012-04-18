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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.RED.FileFormats
{
    public class PatchFile
    {
        public uint Flags;
        public DateTime TimeStamp;
        public string Host;
        public string Creator;

        public List<Patch.Entry> Entries
            = new List<Patch.Entry>();

        public void Deserialize(Stream input)
        {
            input.Seek(-8, SeekOrigin.End);
            if (input.ReadValueU32() != 0x57325048) // W2PH
            {
                throw new FormatException();
            }

            var baseOffset = input.ReadValueU32();

            input.Seek(baseOffset, SeekOrigin.Begin);
            var headerData = input.ReadToMemoryStream(0xA8);
            // ReSharper disable UseObjectOrCollectionInitializer
            var header = new Patch.Header();
            // ReSharper restore UseObjectOrCollectionInitializer

            header.Magic = headerData.ReadValueU32();
            if (header.Magic != 0x50433257) // W2CP
            {
                throw new FormatException();
            }

            header.DateYear = headerData.ReadValueU16();
            header.DateMonth = headerData.ReadValueU16();
            header.DateDay = headerData.ReadValueU16();
            header.DateUnk0 = headerData.ReadValueU16();
            header.DateHour = headerData.ReadValueU16();
            header.DateMinutes = headerData.ReadValueU16();
            header.DateSeconds = headerData.ReadValueU16();
            header.DateUnk1 = headerData.ReadValueU16();
            header.Host = headerData.ReadString(64, true, Encoding.ASCII);
            header.Creator = headerData.ReadString(64, true, Encoding.ASCII);
            header.FileCount = headerData.ReadValueU32();
            header.Flags = headerData.ReadValueU32();
            header.FileTableOffset = headerData.ReadValueU32();

            var fileHash = headerData.ReadValueU64();
            var actualHash = header.Hash;
            if (actualHash != fileHash)
            {
                throw new FormatException();
            }

            this.TimeStamp = new DateTime(
                header.DateYear,
                header.DateMonth,
                header.DateDay,
                header.DateHour,
                header.DateMinutes,
                header.DateSeconds);
            this.Host = header.Host;
            this.Creator = header.Creator;
            this.Flags = header.Flags;

            input.Seek(baseOffset + header.FileTableOffset, SeekOrigin.Begin);

            var entryCount = input.ReadValueEncodedS32();
            var names = new string[entryCount];
            var parents = new int[entryCount];
            for (int i = 0; i < entryCount; i++)
            {
                var nameLength = input.ReadValueEncodedS32();
                if (nameLength >= 512)
                {
                    throw new FormatException();
                }
                names[i] = input.ReadString(nameLength, Encoding.ASCII);
                parents[i] = input.ReadValueEncodedS32();
            }

            input.Seek(baseOffset + 0xA8, SeekOrigin.Begin);
            this.Entries.Clear();
            for (int i = 0; i < header.FileCount; i++)
            {
                if (input.ReadValueU32() != 0x46494C45) // FILE
                {
                    throw new FormatException();
                }

                var index = input.ReadValueEncodedS32();
                if (index < 0 || index >= entryCount)
                {
                    throw new FormatException();
                }

                // ReSharper disable UseObjectOrCollectionInitializer
                var entry = new Patch.Entry();
                // ReSharper restore UseObjectOrCollectionInitializer
                entry.UnpatchedHash = input.ReadValueU64();
                entry.PatchedHash = input.ReadValueU64();
                entry.OriginalSize = (uint)input.ReadValueEncodedS32();
                entry.PatchDataSize = (uint)input.ReadValueEncodedS32();
                entry.Offset = input.Position;
                input.Seek(entry.PatchDataSize, SeekOrigin.Current);

                entry.Name = names[index];
                var parent = parents[index];
                while (parent > 0)
                {
                    entry.Name = Path.Combine(names[parent], entry.Name);
                    parent = parents[parent];
                }

                this.Entries.Add(entry);
            }
        }
    }
}
