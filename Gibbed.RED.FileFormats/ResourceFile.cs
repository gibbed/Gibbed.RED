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
using Gibbed.Helpers;

namespace Gibbed.RED.FileFormats
{
    public class ResourceFile : Resource.IResourceFile
    {
        public uint Version;

        public List<string> Strings
            = new List<string>();
        public List<Resource.Dependency> Dependencies
            = new List<Resource.Dependency>();
        public List<Resource.Object> Objects
            = new List<Resource.Object>();
        public List<string> Unknown3s
            = new List<string>();

        public string ReadString(int id)
        {
            if (id < 1 || id > this.Strings.Count)
            {
                throw new IndexOutOfRangeException();
            }

            return this.Strings[id - 1];
        }

        public int WriteString(string value)
        {
            int index = this.Strings.IndexOf(value);
            if (index >= 0)
            {
                return 1 + index;
            }

            this.Strings.Add(value);
            return this.Strings.Count;
        }

        public void Deserialize(Stream input)
        {
            if (input.ReadValueU32() != 0x57325243) // 'W2RC' -> Witcher 2 Resource?
            {
                throw new FormatException();
            }

            this.Version = input.ReadValueU32();
            if (this.Version < 83 || this.Version > 115)
            {
                throw new FormatException();
            }

            var flags = input.ReadValueU32();

            uint stringDataOffset = input.ReadValueU32();
            uint stringCount = input.ReadValueU32();
            uint objectDataOffset = input.ReadValueU32();
            uint objectCount = input.ReadValueU32();
            uint dependencyDataOffset = input.ReadValueU32();
            uint dependencyCount = input.ReadValueU32();
            uint unk3Offset = 0;
            uint unk3Count = 0;

            if (this.Version >= 46)
            {
                unk3Offset = input.ReadValueU32();
                unk3Count = input.ReadValueU32();
            }

            this.Strings.Clear();
            if (stringCount > 0)
            {
                input.Seek(stringDataOffset, SeekOrigin.Begin);
                for (uint i = 0; i < stringCount; i++)
                {
                    this.Strings.Add(input.ReadStringEncodedUnicode());
                }
            }

            this.Dependencies.Clear();
            if (dependencyCount > 0)
            {
                input.Seek(dependencyDataOffset, SeekOrigin.Begin);

                for (uint i = 0; i < dependencyCount; i++)
                {
                    var dependency = new Resource.Dependency();
                    dependency.Unknown0 = input.ReadStringEncoded();
                    dependency.Unknown1 = input.ReadValueU16();
                    dependency.Unknown2 = input.ReadValueU16();
                    this.Dependencies.Add(dependency);
                }
            }

            this.Unknown3s.Clear();
            if (this.Version >= 46 && unk3Count > 1)
            {
                input.Seek(unk3Offset, SeekOrigin.Begin);

                for (uint i = 1; i < unk3Count; i++)
                {
                    this.Unknown3s.Add(input.ReadStringEncodedUnicode());
                }
            }

            /* TODO: since objects can reference other objects
             * we need to deserialize everything at this point
             * into their classes so references can be picked
             * up properly where as it would probably desynchronize
             * if we did lazy loading.
             * 
             * Though this means we need a much better number of
             * implemented class serializers. */
            this.Objects = new List<Resource.Object>();
            if (objectCount > 0)
            {
                input.Seek(objectDataOffset, SeekOrigin.Begin);

                var objects = new List<Resource.Object>();
                for (uint i = 0; i < objectCount; i++)
                {
                    var obj = new Resource.Object();
                    obj.TypeNameIndex = input.ReadValueS16();
                    if (obj.TypeNameIndex < 1 ||
                        obj.TypeNameIndex > this.Strings.Count)
                    {
                        throw new FormatException();
                    }

                    var parentIndex = input.ReadValueS32();
                    var size = input.ReadValueU32();
                    var offset = input.ReadValueU32();
                    obj.Flags = input.ReadValueU32();
                    obj.Unknown5 = input.ReadValueU32();
                    obj.Link = this.Version < 102 ? null : input.ReadStringEncodedUnicode();

                    var position = input.Position;
                    input.Seek(offset, SeekOrigin.Begin);
                    obj.Data = input.ReadToMemoryStream(size);
                    input.Seek(position, SeekOrigin.Begin);

                    objects.Add(obj);

                    if (obj.Unknown5 != 0)
                    {
                        throw new FormatException();
                    }

                    if (parentIndex > 0)
                    {
                        var parent = objects[parentIndex - 1];
                        if (parent.Children == null)
                        {
                            parent.Children = new List<Resource.Object>();
                        }
                        parent.Children.Add(obj);
                        obj.Parent = parent;
                    }
                    else
                    {
                        obj.Parent = null;
                        this.Objects.Add(obj);
                    }
                }
            }
        }
    }
}
