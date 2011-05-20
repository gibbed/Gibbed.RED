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
using System.Linq;
using System.Text;
using Gibbed.Helpers;

namespace Gibbed.RED.FileFormats
{
    public class CompiledScriptsFile
    {
        public void Deserialize(Stream input)
        {
            input.Seek(-4, SeekOrigin.End);
            
            // read strings
            var stringTableOffset = input.ReadValueU32();
            input.Seek(stringTableOffset, SeekOrigin.Begin);
            var stringCount = input.ReadValueU32();
            var strings = new Script.RawString[stringCount];
            for (int i = 0; i < strings.Length; i++)
            {
                var stringEntry = new Script.RawString();
                stringEntry.Value = ReadEncodedString(input);
                stringEntry.IsName = input.ReadValueB8();
                strings[i] = stringEntry;
            }

            input.Seek(0, SeekOrigin.Begin);
            
            // now the script data
            var unk1 = ReadEncodedString(input);
            var timeStamp = DateTime.FromFileTimeUtc(input.ReadValueS64());
            var unk3 = ReadEncodedString(input);

            var typeDefCount = input.ReadValueU32();
            var funcDefCount = input.ReadValueU32();

            var rawTypeDefs = new Script.RawTypeDefinition[typeDefCount];
            for (uint i = 0; i < typeDefCount; i++)
            {
                var rawTypeDef = new Script.RawTypeDefinition();
                rawTypeDef.Name = strings[ReadEncodedInt32(input)].Value;
                rawTypeDef.Type = (Script.NativeType)ReadEncodedInt32(input);
                rawTypeDef.UnknownCount = ReadEncodedInt32(input);
                rawTypeDef.PropertyCount = ReadEncodedInt32(input);
                rawTypeDef.Flags = (Script.RawTypeDefinitionFlags)ReadEncodedInt32(input);
                rawTypeDefs[i] = rawTypeDef;
            }

            var rawFuncDefs = new Script.RawFunctionDefinition[funcDefCount];
            for (uint i = 0; i < funcDefCount; i++)
            {
                var rawFuncDef = new Script.RawFunctionDefinition();
                rawFuncDef.Name = strings[ReadEncodedInt32(input)].Value;
                rawFuncDef.DefinedOnId = ReadEncodedInt32(input);
                rawFuncDef.Flags = ReadEncodedInt32(input);
                rawFuncDefs[i] = rawFuncDef;
            }

            // parse enums
            for (int i = 0; i < rawTypeDefs.Length; i++)
            {
                var rawTypeDef = rawTypeDefs[i];

                if (rawTypeDef.Type != Script.NativeType.Enum ||
                    (rawTypeDef.Flags & Script.RawTypeDefinitionFlags.Unknown0) == 0)
                {
                    continue;
                }

                var type = (Script.NativeType)ReadEncodedInt32(input);
                if (rawTypeDef.Type != type)
                {
                    throw new FormatException();
                }

                var id = ReadEncodedInt32(input);
                if (id != i)
                {
                    throw new FormatException();
                }

                var unk2_2 = ReadEncodedInt32(input);

                var constantCount = ReadEncodedInt32(input);
                for (int j = 0; j < constantCount; j++)
                {
                    var constantName = strings[ReadEncodedInt32(input)].Value;
                    var constantValue = ReadEncodedInt32(input);
                }
            }

            // parse classes
            for (int i = 0; i < rawTypeDefs.Length; i++)
            {
                var rawTypeDef = rawTypeDefs[i];

                if (rawTypeDef.Type != Script.NativeType.Class ||
                    (rawTypeDef.Flags & Script.RawTypeDefinitionFlags.Unknown0) == 0)
                {
                    continue;
                }

                var type = (Script.NativeType)ReadEncodedInt32(input);
                if (rawTypeDef.Type != type)
                {
                    throw new FormatException();
                }

                var id = ReadEncodedInt32(input);
                if (id != i)
                {
                    throw new FormatException();
                }

                var isExtending = ReadEncodedInt32(input);
                if (isExtending != 0)
                {
                    var extendedTypeId = ReadEncodedInt32(input);
                }

                var unk2_4 = ReadEncodedInt32(input);
                for (int j = 0; j < unk2_4; j++)
                {
                    var unk2_9 = ReadEncodedInt32(input);
                    var unk2_10 = ReadEncodedInt32(input);
                }

                for (int j = 0; j < rawTypeDef.UnknownCount; j++)
                {
                    // string index?
                    var unk2_8 = strings[ReadEncodedInt32(input)];
                }

                for (int j = 0; j < rawTypeDef.PropertyCount; j++)
                {
                    var propTypeId = ReadEncodedInt32(input);
                    var propName = strings[ReadEncodedInt32(input)];
                    var propFlags = ReadEncodedInt32(input);
                    // 1 = editable
                    // 2 = const
                    // 32 = ?
                    // 32768 = saved
                }
            }

            // parse class defaults
            for (int i = 0; i < rawTypeDefs.Length; i++)
            {
                var rawTypeDef = rawTypeDefs[i];

                if (rawTypeDef.Type != Script.NativeType.Class ||
                    (rawTypeDef.Flags & Script.RawTypeDefinitionFlags.Unknown0) == 0)
                {
                    continue;
                }

                var id = ReadEncodedInt32(input);
                if (id != i)
                {
                    throw new FormatException();
                }

                var defaultCount = ReadEncodedInt32(input);
                for (int j = 0; j < defaultCount; j++)
                {
                    var propName = strings[ReadEncodedInt32(input)]; // string index?
                    var dataType = ReadEncodedInt32(input);

                    if (dataType == 0 || dataType == 1)
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

            // parse functions (awww yeah)
            for (int i = 0; i < rawFuncDefs.Length; i++)
            {
                var rawFuncDef = rawFuncDefs[i];

                if ((rawFuncDef.Flags & 1) == 0)
                {
                    continue;
                }

                var id = ReadEncodedInt32(input);
                if (id != i)
                {
                    throw new FormatException();
                }

                var rawFlags = ReadEncodedInt32(input);
                var flags = (Script.FunctionFlags)rawFlags;

                var hasReturnValue = input.ReadValueB8();
                if (hasReturnValue == true)
                {
                    var returnValueTypeId = ReadEncodedInt32(input);
                }

                var argumentCount = ReadEncodedInt32(input);
                for (int j = 0; j < argumentCount; j++)
                {
                    var argumentTypeId = ReadEncodedInt32(input);
                    var argumentName = strings[ReadEncodedInt32(input)];
                    var argumentFlags = ReadEncodedInt32(input);
                }

                var localCount = ReadEncodedInt32(input);
                for (int j = 0; j < localCount; j++)
                {
                    var localTypeId = ReadEncodedInt32(input);
                    var localName = strings[ReadEncodedInt32(input)];
                }

                if ((flags & Script.FunctionFlags.Import) == 0)
                {
                    var unencodedByteCodeLength = ReadEncodedInt32(input);
                    int read;
                    for (read = 0; read < unencodedByteCodeLength; )
                    {
                        var op = input.ReadValueU8();
                        var opcode = (Script.Opcode)op;
                        read++;

                        switch (opcode)
                        {
                            case Script.Opcode.U12:
                            {
                                var op_0 = ReadEncodedInt32(input); read += 4;
                                var op_1 = input.ReadValueU8(); read++;
                                break;
                            }

                            case Script.Opcode.U05:
                            {
                                var op_0 = input.ReadValueS16(); read += 2;
                                break;
                            }

                            case Script.Opcode.U04:
                            {
                                var op_0 = ReadEncodedInt32(input); read += 4;
                                break;
                            }

                            case Script.Opcode.U06:
                            {
                                var op_0 = input.ReadValueF32(); read += 4;
                                break;
                            }

                            case Script.Opcode.U07:
                            {
                                var op_0 = ReadEncodedInt32(input); read += 16;
                                break;
                            }

                            case Script.Opcode.U28:
                            {
                                var op_0 = input.ReadValueU16(); read += 2;
                                var op_1 = input.ReadValueU16(); read += 2;
                                var op_2 = ReadEncodedInt32(input); read += 4;
                                break;
                            }

                            case Script.Opcode.U33:
                            case Script.Opcode.U20:
                            {
                                var op_0 = input.ReadValueU16(); read += 2;
                                var op_1 = input.ReadValueU16(); read += 2;
                                break;
                            }

                            case Script.Opcode.U13:
                            case Script.Opcode.U23:
                            case Script.Opcode.U22:
                            case Script.Opcode.U24:
                            {
                                var op_0 = input.ReadValueU16(); read += 2;
                                break;
                            }

                            case Script.Opcode.U15:
                            case Script.Opcode.U16:
                            case Script.Opcode.U17:
                            case Script.Opcode.U32:
                            {
                                var op_0 = ReadEncodedInt32(input);
                                var op_1 = ReadEncodedInt32(input);
                                read += 4;
                                break;
                            }

                            case Script.Opcode.U19:
                            {
                                var op_0 = ReadEncodedInt32(input); read += 4;
                                var op_1 = input.ReadValueU16(); read += 2;
                                break;
                            }

                            case Script.Opcode.U26:
                            {
                                var op_0 = input.ReadValueU8(); read++;
                                var op_1 = ReadEncodedInt32(input); read += 4;
                                break;
                            }

                            case Script.Opcode.U34:
                            case Script.Opcode.U86:
                            case Script.Opcode.U08:
                            case Script.Opcode.U52:
                            case Script.Opcode.U44:
                            case Script.Opcode.U60:
                            case Script.Opcode.U36:
                            case Script.Opcode.U43:
                            case Script.Opcode.U87:
                            case Script.Opcode.U51:
                            case Script.Opcode.U56:
                            case Script.Opcode.U35:
                            case Script.Opcode.U58:
                            case Script.Opcode.U85:
                            case Script.Opcode.U50:
                            case Script.Opcode.U45:
                            case Script.Opcode.U54:
                            case Script.Opcode.U57:
                            case Script.Opcode.U47:
                            case Script.Opcode.U59:
                            case Script.Opcode.U55:
                            case Script.Opcode.U41:
                            {
                                var op_0 = ReadEncodedInt32(input); read += 4;
                                break;
                            }

                            case Script.Opcode.U27:
                            {
                                var op_0 = input.ReadValueU16(); read += 2;
                                var op_1 = input.ReadValueU16(); read += 2;
                                var op_2 = ReadEncodedInt32(input);
                                if (op_2 == -1)
                                {
                                    var op_3 = ReadEncodedInt32(input);
                                }
                                read += 4;
                                break;
                            }

                            case Script.Opcode.U29:
                            case Script.Opcode.U40:
                            {
                                var op_0 = input.ReadValueU16(); read += 2;
                                var op_1 = ReadEncodedInt32(input); read += 4;
                                break;
                            }

                            case Script.Opcode.U00:
                            case Script.Opcode.U30:
                            case Script.Opcode.U03:
                            case Script.Opcode.U02:
                            case Script.Opcode.U11:
                            case Script.Opcode.U10:
                            case Script.Opcode.U31:
                            case Script.Opcode.U89:
                            case Script.Opcode.U91:
                            case Script.Opcode.U78:
                            case Script.Opcode.U90:
                            case Script.Opcode.U71:
                            case Script.Opcode.U38:
                            case Script.Opcode.U01:
                            case Script.Opcode.U88:
                            case Script.Opcode.U83:
                            case Script.Opcode.U72:
                            case Script.Opcode.U76:
                            case Script.Opcode.U70:
                            case Script.Opcode.U84:
                            case Script.Opcode.U21:
                            case Script.Opcode.U64:
                            case Script.Opcode.U92:
                            case Script.Opcode.U75:
                            case Script.Opcode.U77:
                            case Script.Opcode.U39:
                            case Script.Opcode.U69:
                            case Script.Opcode.U66:
                            case Script.Opcode.U73:
                            case Script.Opcode.U67:
                            case Script.Opcode.U79:
                            case Script.Opcode.U42:
                            case Script.Opcode.U81:
                            {
                                break;
                            }

                            default:
                            {
                                throw new NotImplementedException("unhandled " + opcode.ToString());
                            }
                        }
                    }

                    if (read != unencodedByteCodeLength)
                    {
                        throw new InvalidOperationException();
                    }
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
