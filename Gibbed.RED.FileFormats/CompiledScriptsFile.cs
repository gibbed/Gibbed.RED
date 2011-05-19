/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
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
using Gibbed.Helpers;

namespace Gibbed.RED.FileFormats
{
    public class CompiledScriptsFile
    {
        private class RawStringEntry
        {
            public string Text;
            public byte Unknown;

            public override string ToString()
            {
                return this.Text;
            }
        }

        private class RawTypeDefinition
        {
            public string Name;
            public CompiledScript.ScriptType Type;
            public int UnknownCount;
            public int PropertyCount;
            public int Flags;
        }

        public class RawFunctionDefinition
        {
            public int Unknown0;
            public int Unknown1;
            public int Flags;
        }

        public void Deserialize(Stream input)
        {
            input.Seek(-4, SeekOrigin.End);
            input.Seek(input.ReadValueU32(), SeekOrigin.Begin);
            var stringCount = input.ReadValueU32();

            var strings = new List<RawStringEntry>();
            for (uint i = 0; i < stringCount; i++)
            {
                var stringEntry = new RawStringEntry();
                stringEntry.Text = ReadEncodedString(input);
                stringEntry.Unknown = input.ReadValueU8();
                strings.Add(stringEntry);
            }

            input.Seek(0, SeekOrigin.Begin);

            var unk1 = ReadEncodedString(input);
            var timeStamp = DateTime.FromFileTimeUtc(input.ReadValueS64());
            var unk3 = ReadEncodedString(input);

            var typeDefCount = input.ReadValueU32();
            var funcDefCount = input.ReadValueU32();

            var typeDefs = new List<RawTypeDefinition>();
            for (uint i = 0; i < typeDefCount; i++)
            {
                var typeDef = new RawTypeDefinition();
                typeDef.Name = strings[ReadEncodedInt32(input)].Text;
                typeDef.Type = (CompiledScript.ScriptType)ReadEncodedInt32(input);
                typeDef.UnknownCount = ReadEncodedInt32(input);
                typeDef.PropertyCount = ReadEncodedInt32(input);
                typeDef.Flags = ReadEncodedInt32(input);
                typeDefs.Add(typeDef);
            }

            var functionDefs = new List<RawFunctionDefinition>();
            for (uint i = 0; i < funcDefCount; i++)
            {
                var funcDef = new RawFunctionDefinition();
                funcDef.Unknown0 = ReadEncodedInt32(input);
                funcDef.Unknown1 = ReadEncodedInt32(input);
                funcDef.Flags = ReadEncodedInt32(input);
                functionDefs.Add(funcDef);
            }

            // parse enums
            foreach (var typeDef in typeDefs)
            {
                if (typeDef.Type != CompiledScript.ScriptType.Enum ||
                    (typeDef.Flags & 1) == 0)
                {
                    continue;
                }

                var unk2_0 = ReadEncodedInt32(input);
                var unk2_1 = ReadEncodedInt32(input);

                if (typeDef.Type == CompiledScript.ScriptType.Enum)
                {
                    var unk2_2 = ReadEncodedInt32(input);
                    var unk2_3 = ReadEncodedInt32(input);

                    for (int i = 0; i < unk2_3; i++)
                    {
                        var enumName = strings[ReadEncodedInt32(input)]; // string index
                        var enumValue = ReadEncodedInt32(input);
                    }
                }
            }

            // parse classes
            foreach (var typeDef in typeDefs)
            {
                if (typeDef.Type != CompiledScript.ScriptType.Class ||
                    (typeDef.Flags & 1) == 0)
                {
                    continue;
                }

                var unk2_0 = ReadEncodedInt32(input);
                var unk2_1 = ReadEncodedInt32(input);

                if (typeDef.Type == CompiledScript.ScriptType.Class)
                {
                    var isExtending = ReadEncodedInt32(input);
                    if (isExtending != 0)
                    {
                        var extendedTypeIndex = ReadEncodedInt32(input);
                    }

                    var unk2_4 = ReadEncodedInt32(input);
                    for (int i = 0; i < unk2_4; i++)
                    {
                        var unk2_9 = ReadEncodedInt32(input);
                        var unk2_10 = ReadEncodedInt32(input);
                    }

                    for (int i = 0; i < typeDef.UnknownCount; i++)
                    {
                        // string index?
                        var unk2_8 = strings[ReadEncodedInt32(input)];
                    }

                    for (int i = 0; i < typeDef.PropertyCount; i++)
                    {
                        var unk2_5 = ReadEncodedInt32(input);
                        var propName = strings[ReadEncodedInt32(input)];
                        var unk2_7 = ReadEncodedInt32(input);
                    }
                }
            }

            foreach (var typeDef in typeDefs)
            {
                if (typeDef.Type != CompiledScript.ScriptType.Class ||
                    (typeDef.Flags & 1) == 0)
                {
                    continue;
                }

                var parentId = ReadEncodedInt32(input);
                if (typeDefs[parentId].Type == CompiledScript.ScriptType.Class)
                {
                    var unk3_1 = ReadEncodedInt32(input);
                    for (int i = 0; i < unk3_1; i++)
                    {
                        var unk3_2_0 = strings[ReadEncodedInt32(input)]; // string index?
                        var typeType = ReadEncodedInt32(input);

                        if (typeType == 0 || typeType == 1)
                        {
                            var typeName = ReadEncodedString(input);
                            var typeDataSize = input.ReadValueU32(); // size + 4
                            var typeData = new byte[typeDataSize - 4];
                            input.Read(typeData, 0, typeData.Length);
                        }
                        else
                        {
                            throw new FormatException();
                        }
                    }
                }
            }

            foreach (var funcDef in functionDefs)
            {
                if ((funcDef.Flags & 1) == 0)
                {
                    continue;
                }

                var unk4_0 = ReadEncodedInt32(input);
                var unk4_1 = ReadEncodedInt32(input);

                var unk4_2 = input.ReadValueU8();
                if (unk4_2 != 0)
                {
                    var unk4_3 = ReadEncodedInt32(input);
                }

                var unk4_4 = ReadEncodedInt32(input);
                for (int i = 0; i < unk4_4; i++)
                {
                    var unk4_5 = ReadEncodedInt32(input);
                    var unk4_6 = ReadEncodedInt32(input);
                    var unk4_7 = ReadEncodedInt32(input);
                }

                var unk4_8 = ReadEncodedInt32(input);
                for (int i = 0; i < unk4_8; i++)
                {
                    var unk4_9 = ReadEncodedInt32(input);
                    var unk4_10 = ReadEncodedInt32(input);
                }

                var unencodedByteCodeLength = ReadEncodedInt32(input);
                if (unencodedByteCodeLength > 0)
                {
                    throw new NotImplementedException();
                }
            }
        }

        protected static int ReadEncodedInt32(Stream stream)
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

        protected static string ReadEncodedString(Stream stream)
        {
            var length = ReadEncodedInt32(stream);

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
