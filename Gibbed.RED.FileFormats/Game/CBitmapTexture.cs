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
using Gibbed.RED.FileFormats.Resource;
using Gibbed.RED.FileFormats.Resource.Serializers;

namespace Gibbed.RED.FileFormats.Game
{
    public class CBitmapTexture : CResource
    {
        [PropertyName("width")]
        [PropertyDescription("Width of the texture")]
        [PropertySerializer(typeof(UintSerializer))]
        public uint Width { get; set; }

        [PropertyName("height")]
        [PropertyDescription("Height of the texture")]
        [PropertySerializer(typeof(UintSerializer))]
        public uint Height { get; set; }

        [PropertyName("format")]
        [PropertyDescription("Source texture format type")]
        [PropertySerializer(typeof(ETextureRawFormatSerializer))]
        public ETextureRawFormat Format { get; set; }

        [PropertyName("compression")]
        [PropertyDescription("Compression method to use")]
        [PropertySerializer(typeof(ETextureCompressionSerializer))]
        public ETextureCompression Compression { get; set; }

        [PropertyName("textureGroup")]
        [PropertySerializer(typeof(CNameSerializer))]
        public string TextureGroup { get; set; }

        [PropertyName("preserveArtistData")]
        [PropertySerializer(typeof(BoolSerializer))]
        public bool PreserveArtistData { get; set; }

        [PropertyName("importFile")]
        [PropertySerializer(typeof(StringSerializer))]
        public string ImportFile { get; set; }

        public uint Unknown0 { get; set; }
        public List<Mipmap> Mipmaps { get; set; }

        public CBitmapTexture()
        {
            this.Width = 0;
            this.Height = 0;
            this.PreserveArtistData = false;
            this.ImportFile = "";
            this.Format = ETextureRawFormat.TrueColor;
            this.Compression = ETextureCompression.None;

            this.Unknown0 = 0;
            this.Mipmaps = new List<Mipmap>();
        }

        public override void Deserialize(IResourceFile resource, Stream input)
        {
            base.Deserialize(resource, input);

            this.Unknown0 = input.ReadValueU32();
            if (this.Unknown0 != 0)
            {
                throw new FormatException();
            }

            this.Mipmaps.Clear();
            var mipCount = input.ReadValueU32();
            for (uint i = 0; i < mipCount; i++)
            {
                var mip = new Mipmap();
                mip.Width = input.ReadValueU32();
                mip.Height = input.ReadValueU32();
                mip.Unknown2 = input.ReadValueU32();

                var size = input.ReadValueU32();

                mip.Data = new byte[size];
                input.Read(mip.Data, 0, mip.Data.Length);
                this.Mipmaps.Add(mip);
            }

            var unknown1 = input.ReadValueU32();
            if (unknown1 != 0)
            {
                throw new FormatException();
            }

            var unknown2 = input.ReadValueU8();
            if (unknown2 != 0)
            {
                throw new FormatException();
            }
        }

        public class Mipmap
        {
            public uint Width;
            public uint Height;
            public uint Unknown2;
            public byte[] Data;
        }
    }
}
