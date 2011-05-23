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
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.RED.FileFormats.Resource.Serializers
{
    public class CNameSerializer : IPropertySerializer
    {
        public void Serialize(IResourceFile resource, MemoryStream output, object value)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(IResourceFile resource, MemoryStream input)
        {
            if (input.Length < 2)
            {
                throw new FormatException("not enough data for CName property");
            }

            var index = input.ReadValueU16();
            var value = resource.ReadString(index);
            return value;
        }
    }
}
