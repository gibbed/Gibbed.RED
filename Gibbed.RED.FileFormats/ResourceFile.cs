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
using Gibbed.IO;

namespace Gibbed.RED.FileFormats
{
    public class ResourceFile
    {
        public uint Version;

        public List<Resource.ObjectInfo> Objects
            = new List<Resource.ObjectInfo>();

        public List<string> Dependencies
            = new List<string>();

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

            // ReSharper disable UnusedVariable
            var flags = input.ReadValueU32();
            // ReSharper restore UnusedVariable

            var nameDataOffset = input.ReadValueU32();
            var nameCount = input.ReadValueU32();
            var objectDataOffset = input.ReadValueU32();
            var objectCount = input.ReadValueU32();
            var linkDataOffset = input.ReadValueU32();
            var linkCount = input.ReadValueU32();
            uint dependencyDataOffset = 0;
            uint dependencyCount = 0;

            if (this.Version >= 46)
            {
                dependencyDataOffset = input.ReadValueU32();
                dependencyCount = input.ReadValueU32();
            }

            var info = new Resource.Info
            {
                Names = new string[nameCount],
            };

            if (nameCount > 0)
            {
                input.Seek(nameDataOffset, SeekOrigin.Begin);
                for (uint i = 0; i < nameCount; i++)
                {
                    info.Names[i] = input.ReadEncodedString();
                }
            }

            info.Links = new Resource.LinkInfo[linkCount];
            if (linkCount > 0)
            {
                input.Seek(linkDataOffset, SeekOrigin.Begin);

                for (uint i = 0; i < linkCount; i++)
                {
                    // ReSharper disable UseObjectOrCollectionInitializer
                    var link = new Resource.LinkInfo();
                    // ReSharper restore UseObjectOrCollectionInitializer
                    link.FileName = input.ReadEncodedStringW();
                    link.Unknown1 = input.ReadValueU16();
                    link.Unknown2 = input.ReadValueU16();
                    info.Links[i] = link;
                }
            }

            this.Dependencies.Clear();
            if (dependencyCount > 1)
            {
                input.Seek(dependencyDataOffset, SeekOrigin.Begin);

                for (uint i = 1; i < dependencyCount; i++)
                {
                    this.Dependencies.Add(input.ReadEncodedString());
                }
            }

            this.Objects = new List<Resource.ObjectInfo>();
            info.Objects = new Resource.ObjectInfo[objectCount];
            if (objectCount > 0)
            {
                input.Seek(objectDataOffset, SeekOrigin.Begin);

                var offsets = new Dictionary<Resource.ObjectInfo, ObjectLocation>();

                for (uint i = 0; i < objectCount; i++)
                {
                    var typeNameIndex = input.ReadValueS16();
                    if (typeNameIndex < 1 ||
                        typeNameIndex > info.Names.Length)
                    {
                        throw new FormatException();
                    }

                    var obj = new Resource.ObjectInfo
                    {
                        TypeName = info.Names[typeNameIndex - 1],
                    };

                    var parentIndex = input.ReadValueS32();

                    // ReSharper disable UseObjectOrCollectionInitializer
                    var location = new ObjectLocation();
                    // ReSharper restore UseObjectOrCollectionInitializer
                    location.Size = input.ReadValueU32();
                    location.Offset = input.ReadValueU32();

                    obj.Flags = input.ReadValueU32();
                    obj.TemplateIndex = input.ReadValueS32();
                    obj.Link = this.Version < 102 ? null : input.ReadEncodedString();

                    obj.Data = TypeCache.Supports(obj.TypeName) == false
                                   ? new Resource.Dummy()
                                   : TypeCache.Instantiate(obj.TypeName);

                    info.Objects[i] = obj;
                    offsets.Add(obj, location);

                    if (parentIndex > 0)
                    {
                        var parent = info.Objects[parentIndex - 1];
                        if (parent.Children == null)
                        {
                            parent.Children = new List<Resource.ObjectInfo>();
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

                foreach (var obj in info.Objects)
                {
                    var location = offsets[obj];

                    input.Seek(location.Offset, SeekOrigin.Begin);

                    var data = new byte[location.Size];
                    input.Read(data, 0, data.Length);

                    using (var reader = new Resource.ResourceReader(info, data))
                    {
                        obj.Data.Serialize(reader);

                        if (reader.Position != reader.Length)
                        {
                            throw new FormatException();
                        }
                    }
                }
            }
        }

        private struct ObjectLocation
        {
            public uint Offset;
            public uint Size;
        }

        #region TypeCache
        // Lame ass way to do this but, it'll work for now.
        private static class TypeCache
        {
            private static Dictionary<string, Type> _Lookup;

            private static void BuildLookup()
            {
                _Lookup = new Dictionary<string, Type>();

                foreach (var type in System.Reflection.Assembly
                    .GetAssembly(typeof(TypeCache)).GetTypes())
                {
                    if (typeof(IFileObject).IsAssignableFrom(type) == false)
                    {
                        continue;
                    }

                    foreach (ResourceHandlerAttribute attribute in
                        type.GetCustomAttributes(typeof(ResourceHandlerAttribute), false))
                    {
                        if (_Lookup.ContainsKey(attribute.Name) == true)
                        {
                            throw new InvalidOperationException();
                        }

                        _Lookup.Add(attribute.Name, type);
                    }
                }
            }

            public static bool Supports(string className)
            {
                if (_Lookup == null)
                {
                    BuildLookup();
                }

                // ReSharper disable PossibleNullReferenceException
                return _Lookup.ContainsKey(className);
                // ReSharper restore PossibleNullReferenceException
            }

            public static IFileObject Instantiate(string className)
            {
                if (_Lookup == null)
                {
                    BuildLookup();
                }
                else if (_Lookup.ContainsKey(className) == false)
                {
                    return null;
                }

                // ReSharper disable PossibleNullReferenceException
                return (IFileObject)Activator.CreateInstance(_Lookup[className]);
                // ReSharper restore PossibleNullReferenceException
            }
        }
        #endregion
    }
}
