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

namespace Gibbed.RED.FileFormats.Resource
{
    internal class ResourceReader : IFileStream, IDisposable
    {
        private readonly MemoryStream _Stream;
        private Info _Info;
        private bool _Disposed;

        public ResourceReader(
            Info info,
            byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            this._Info = info;
            this._Stream = new MemoryStream((byte[])data.Clone());
        }

        ~ResourceReader()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._Disposed == false)
            {
                if (disposing == true)
                {
                    this._Stream.Dispose();
                }

                this._Disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region IFileStream
        SerializeMode IFileStream.Mode
        {
            get { return SerializeMode.Reading; }
        }

        public long Position
        {
            get { return this._Stream.Position; }
            set { this._Stream.Position = value; }
        }

        public long Length
        {
            get { return this._Stream.Length; }
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeValue(ref bool value)
            // ReSharper restore RedundantAssignment
        {
            value = this._Stream.ReadValueB8();
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeValue(ref sbyte value)
            // ReSharper restore RedundantAssignment
        {
            value = this._Stream.ReadValueS8();
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeValue(ref byte value)
            // ReSharper restore RedundantAssignment
        {
            value = this._Stream.ReadValueU8();
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeValue(ref short value)
            // ReSharper restore RedundantAssignment
        {
            value = this._Stream.ReadValueS16();
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeValue(ref ushort value)
            // ReSharper restore RedundantAssignment
        {
            value = this._Stream.ReadValueU16();
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeValue(ref int value)
            // ReSharper restore RedundantAssignment
        {
            value = this._Stream.ReadValueS32();
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeValue(ref uint value)
            // ReSharper restore RedundantAssignment
        {
            value = this._Stream.ReadValueU32();
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeValue(ref float value)
            // ReSharper restore RedundantAssignment
        {
            value = this._Stream.ReadValueF32();
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeValue(ref string value)
            // ReSharper restore RedundantAssignment
        {
            value = this._Stream.ReadEncodedString();
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeValue(ref Guid value)
            // ReSharper restore RedundantAssignment
        {
            value = this._Stream.ReadValueGuid();
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeValue(ref byte[] value, int length)
            // ReSharper restore RedundantAssignment
        {
            value = new byte[length];
            if (this._Stream.Read(value, 0, value.Length) != value.Length)
            {
                throw new FormatException();
            }
        }

        void IFileStream.SerializeValue(ref byte[] value, uint length)
        {
            ((IFileStream)this).SerializeValue(ref value, (int)length);
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeBuffer(ref byte[] value)
            // ReSharper restore RedundantAssignment
        {
            var length = this._Stream.ReadValueEncodedS32();
            var buffer = new byte[length];
            this._Stream.Read(buffer, 0, buffer.Length);
            value = buffer;
        }

        void IFileStream.SerializeName(ref string value)
        {
            var index = this._Stream.ReadValueS16();

            if (index == 0)
            {
                value = null;
                return;
            }

            if (index > this._Info.Names.Length)
            {
                throw new FormatException();
            }

            value = this._Info.Names[index - 1];
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeTagList(ref List<string> value)
            // ReSharper restore RedundantAssignment
        {
            var count = this._Stream.ReadValueEncodedS32();

            var list = new List<string>();
            for (int i = 0; i < count; i++)
            {
                string item = null;
                ((IFileStream)this).SerializeName(ref item);
                list.Add(item);
            }
            value = list;
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeObject<TType>(ref TType value)
            // ReSharper restore RedundantAssignment
        {
            var instance = new TType();
            instance.Serialize(this);
            value = instance;
        }

        void IFileStream.SerializePointer(ref IFileObject value)
        {
            var index = this._Stream.ReadValueS32();

            if (index > 0)
            {
                index--;

                if (index >= this._Info.Objects.Length)
                {
                    throw new FormatException();
                }

                var obj = this._Info.Objects[index];
                value = obj.Data;
            }
            else if (index == 0)
            {
                value = null;
            }
            else /*if (value < 0)*/
            {
                index = -index;
                index--;

                if (index >= this._Info.Links.Length)
                {
                    throw new FormatException();
                }

                var link = new Link()
                {
                    Info = this._Info.Links[index],
                };

                value = link;
            }
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializePointer(ref List<IFileObject> value, bool encoded)
            // ReSharper restore RedundantAssignment
        {
            var count = encoded == false
                            ? this._Stream.ReadValueS32()
                            : this._Stream.ReadValueEncodedS32();
            var list = new List<IFileObject>();
            for (int i = 0; i < count; i++)
            {
                IFileObject item = null;
                ((IFileStream)this).SerializePointer(ref item);
                list.Add(item);
            }
            value = list;
        }
        #endregion
    }
}
