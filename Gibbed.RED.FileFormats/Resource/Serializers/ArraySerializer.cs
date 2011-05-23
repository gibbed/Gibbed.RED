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

namespace Gibbed.RED.FileFormats.Resource.Serializers
{
    public class ArraySerializer<TSerializer> : IPropertySerializer
        where TSerializer : IPropertySerializer, new()
    {
        private static TSerializer Serializer
            = new TSerializer();

        public void Serialize(IResourceFile resource, MemoryStream output, object value)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(IResourceFile resource, MemoryStream input)
        {
            if (input.Length < 6)
            {
                throw new FormatException("not enough data for @*IMaterial property");
            }

            var count = input.ReadValueU32();
            var elementTypeName = resource.ReadString(input.ReadValueU16());
            var unk2 = input.ReadValueS16();

            if (unk2 != -1)
            {
                throw new InvalidOperationException();
            }

            var list = new List<object>();
            for (uint i = 0; i < count; i++)
            {
                var element = Serializer.Deserialize(resource, input);
                list.Add(element);
            }
            return list;
        }
    }

    public class ArraySerializer<TElement, TSerializer> : IPropertySerializer
        where TSerializer : IPropertySerializer, new()
    {
        private static TSerializer Serializer
            = new TSerializer();

        public void Serialize(IResourceFile resource, MemoryStream output, object value)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(IResourceFile resource, MemoryStream input)
        {
            if (input.Length < 6)
            {
                throw new FormatException("not enough data for @*IMaterial property");
            }

            var count = input.ReadValueU32();
            var elementTypeName = resource.ReadString(input.ReadValueU16());
            var unk2 = input.ReadValueS16();

            if (unk2 != -1)
            {
                throw new InvalidOperationException();
            }

            var list = new List<TElement>();
            for (uint i = 0; i < count; i++)
            {
                var element = (TElement)Serializer.Deserialize(resource, input);
                list.Add(element);
            }
            return list;
        }
    }
}
