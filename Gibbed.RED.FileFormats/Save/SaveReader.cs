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

namespace Gibbed.RED.FileFormats.Save
{
    public class SaveReader : ISaveStream, IFileStream, IDisposable
    {
        private readonly MemoryStream _Stream;
        private bool _Disposed;

        public SaveReader(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            this._Stream = new MemoryStream((byte[])data.Clone());
        }

        ~SaveReader()
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

        private byte[] ReadValue(string name, string type)
        {
            if (this._Stream.ReadValueU32() != 0x4C415641)
            {
                throw new FormatException("not a value");
            }

            var actualName = this._Stream.ReadEncodedString();
            if (actualName != name)
            {
                throw new FormatException(string.Format("read wrong value (got '{0}', wanted '{1}'", actualName, name));
            }

            var actualType = this._Stream.ReadEncodedString();
            if (actualName != name)
            {
                throw new FormatException(string.Format("read wrong type for value {0} (got '{1}', wanted '{2}'",
                                                        name,
                                                        actualType,
                                                        type));
            }

            var size = this._Stream.ReadValueU32();
            var data = new byte[size];
            if (this._Stream.Read(data, 0, data.Length) != data.Length)
            {
                throw new FormatException();
            }

            return data;
        }

        private byte[] ReadArray(string name, string type, string elementType, out int count)
        {
            using (var input = new MemoryStream(this.ReadValue(name, type)))
            {
                count = input.ReadValueS32();
                var actualElementType = input.ReadEncodedString();
                if (actualElementType != elementType)
                {
                    throw new FormatException(
                        string.Format("read wrong element type for value {0} (got '{1}', wanted '{2}'",
                                      name,
                                      actualElementType,
                                      elementType));
                }

                if (input.ReadValueS16() != -1)
                {
                    throw new FormatException();
                }

                var data = new byte[input.Length - input.Position];
                if (input.Read(data, 0, data.Length) != data.Length)
                {
                    throw new FormatException();
                }

                return data;
            }
        }

        #region ISaveStream
        SerializeMode ISaveStream.Mode
        {
            get { return SerializeMode.Reading; }
        }

        void ISaveStream.SerializeValue(string name, ref bool value)
        {
            var data = this.ReadValue(name, "Bool");
            if (data.Length != 1)
            {
                throw new FormatException();
            }
            value = data[0] != 0;
        }

        void ISaveStream.SerializeValue(string name, ref sbyte value)
        {
            var data = this.ReadValue(name, "Int8");
            if (data.Length != 1)
            {
                throw new FormatException();
            }
            value = (sbyte)data[0];
        }

        void ISaveStream.SerializeValue(string name, ref byte value)
        {
            var data = this.ReadValue(name, "Float");
            if (data.Length != 1)
            {
                throw new FormatException();
            }
            value = data[0];
        }

        void ISaveStream.SerializeValue(string name, ref short value)
        {
            var data = this.ReadValue(name, "Int16");
            if (data.Length != 2)
            {
                throw new FormatException();
            }
            value = BitConverter.ToInt16(data, 0);
        }

        void ISaveStream.SerializeValue(string name, ref ushort value)
        {
            var data = this.ReadValue(name, "Uint16");
            if (data.Length != 2)
            {
                throw new FormatException();
            }
            value = BitConverter.ToUInt16(data, 0);
        }

        void ISaveStream.SerializeValue(string name, ref int value)
        {
            var data = this.ReadValue(name, "Int");
            if (data.Length != 4)
            {
                throw new FormatException();
            }
            value = BitConverter.ToInt32(data, 0);
        }

        void ISaveStream.SerializeValue(string name, ref uint value)
        {
            var data = this.ReadValue(name, "Uint");
            if (data.Length != 4)
            {
                throw new FormatException();
            }
            value = BitConverter.ToUInt32(data, 0);
        }

        void ISaveStream.SerializeValue(string name, ref long value)
        {
            var data = this.ReadValue(name, "Int64");
            if (data.Length != 8)
            {
                throw new FormatException();
            }
            value = BitConverter.ToInt64(data, 0);
        }

        void ISaveStream.SerializeValue(string name, ref ulong value)
        {
            var data = this.ReadValue(name, "Uint64");
            if (data.Length != 8)
            {
                throw new FormatException();
            }
            value = BitConverter.ToUInt64(data, 0);
        }

        void ISaveStream.SerializeValue(string name, ref float value)
        {
            var data = this.ReadValue(name, "Float");
            if (data.Length != 4)
            {
                throw new FormatException();
            }
            value = BitConverter.ToSingle(data, 0);
        }

        void ISaveStream.SerializeValue(string name, ref string value)
        {
            var data = this.ReadValue(name, "String");
            if (data.Length < 1)
            {
                throw new FormatException();
            }
            using (var input = new MemoryStream(data))
            {
                value = input.ReadEncodedString();
            }
        }

        void ISaveStream.SerializeValue(string name, ref Guid value)
        {
            var data = this.ReadValue(name, "CGUID");
            if (data.Length < 16)
            {
                throw new FormatException();
            }
            using (var input = new MemoryStream(data))
            {
                value = input.ReadValueGuid();
            }
        }

        void ISaveStream.SerializeValue(string name, ref byte[] value)
        {
            int count;
            var data = this.ReadArray(name, "@Uint8", "Uint8", out count);
            if (data.Length != count)
            {
                throw new InvalidOperationException();
            }
            value = data;
        }

        void ISaveStream.SerializeBlock<TType>(string name, ref TType value)
        {
            if (this._Stream.ReadValueU32() != 0x4B434C42) // BLCK
            {
                throw new FormatException();
            }

            var actualName = this._Stream.ReadEncodedString();
            if (name != actualName)
            {
                throw new FormatException();
            }

            var size = this._Stream.ReadValueU32();
            var data = new byte[size];
            if (this._Stream.Read(data, 0, data.Length) != data.Length)
            {
                throw new FormatException();
            }

            using (var reader = new SaveReader(data))
            {
                var instance = new TType();
                instance.Serialize(reader);

                if (reader._Stream.Position != reader._Stream.Length)
                {
                    throw new FormatException();
                }

                value = instance;
            }
        }

        void ISaveStream.SerializeBlocks<TType>(
            string name, string type, ref List<TType> value)
        {
            if (this._Stream.ReadValueU32() != 0x4B434C42) // BLCK
            {
                throw new FormatException();
            }

            var actualName = this._Stream.ReadEncodedString();
            if (name != actualName)
            {
                throw new FormatException();
            }

            var size = this._Stream.ReadValueU32();
            var data = new byte[size];
            if (this._Stream.Read(data, 0, data.Length) != data.Length)
            {
                throw new FormatException();
            }

            using (var reader = new SaveReader(data))
            {
                var list = new List<TType>();
                while (reader._Stream.Position < reader._Stream.Length)
                {
                    var instance = new TType();
                    ((ISaveStream)reader).SerializeBlock(type, ref instance);
                    list.Add(instance);
                }
                value = list;
            }
        }

        void ISaveStream.SerializeObject<TType>(
            // ReSharper disable RedundantAssignment
            string name, string type, ref TType value)
            // ReSharper restore RedundantAssignment
        {
            var data = this.ReadValue(name, type);

            using (var reader = new SaveReader(data))
            {
                var instance = new TType();
                instance.Serialize(reader);
                value = instance;
            }
        }
        #endregion

        #region IFileStream
        SerializeMode IFileStream.Mode
        {
            get { return SerializeMode.Reading; }
        }

        long IFileStream.Position
        {
            get { return this._Stream.Position; }
            set { this._Stream.Position = value; }
        }

        long IFileStream.Length
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

        void IFileStream.SerializeBuffer(ref byte[] value)
        {
            throw new NotSupportedException();
        }

        // ReSharper disable RedundantAssignment
        void IFileStream.SerializeName(ref string value)
            // ReSharper restore RedundantAssignment
        {
            value = this._Stream.ReadEncodedString();
        }

        void IFileStream.SerializeTagList(ref List<string> value)
        {
            throw new NotSupportedException();
        }

        void IFileStream.SerializeObject<TType>(ref TType value)
        {
            throw new NotSupportedException();
        }

        void IFileStream.SerializePointer(ref IFileObject value)
        {
            throw new NotSupportedException();
        }

        void IFileStream.SerializePointer(ref List<IFileObject> value, bool encoded)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}
