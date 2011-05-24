using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Gibbed.Helpers;
using System.Text;

namespace Gibbed.RED.FileFormats.Resource
{
    internal class ResourceReader : IFileStream, IDisposable
    {
        private MemoryStream Stream;
        private Info Info;
        private bool _Disposed = false;

        public ResourceReader(
            Info info,
            byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            this.Info = info;
            this.Stream = new MemoryStream((byte[])data.Clone());
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
                    this.Stream.Dispose();
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
            get
            {
                return this.Stream.Position;
            }
            set
            {
                this.Stream.Position = value;
            }
        }

        public long Length
        {
            get
            {
                return this.Stream.Length;
            }
        }

        void IFileStream.SerializeValue(ref bool value)
        {
            value = this.Stream.ReadValueB8();
        }

        void IFileStream.SerializeValue(ref sbyte value)
        {
            value = this.Stream.ReadValueS8();
        }

        void IFileStream.SerializeValue(ref byte value)
        {
            value = this.Stream.ReadValueU8();
        }

        void IFileStream.SerializeValue(ref short value)
        {
            value = this.Stream.ReadValueS16();
        }

        void IFileStream.SerializeValue(ref ushort value)
        {
            value = this.Stream.ReadValueU16();
        }

        void IFileStream.SerializeValue(ref int value)
        {
            value = this.Stream.ReadValueS32();
        }

        void IFileStream.SerializeValue(ref uint value)
        {
            value = this.Stream.ReadValueU32();
        }

        void IFileStream.SerializeValue(ref float value)
        {
            value = this.Stream.ReadValueF32();
        }

        void IFileStream.SerializeValue(ref string value)
        {
            value = this.Stream.ReadStringEncodedUnicode();
        }

        void IFileStream.SerializeValue(ref byte[] value, int length)
        {
            value = new byte[length];
            if (this.Stream.Read(value, 0, value.Length) != value.Length)
            {
                throw new FormatException();
            }
        }

        void IFileStream.SerializeValue(ref byte[] value, uint length)
        {
            ((IFileStream)this).SerializeValue(ref value, (int)length);
        }

        void IFileStream.SerializeName(ref string value)
        {
            var index = this.Stream.ReadValueS16();

            if (index == 0)
            {
                value = null;
                return;
            }
            else if (index > this.Info.Names.Length)
            {
                throw new FormatException();
            }

            value = this.Info.Names[index - 1];
        }

        void IFileStream.SerializePointer(ref IFileObject value)
        {
            var index = this.Stream.ReadValueS32();

            if (index > 0)
            {
                index--;

                if (index >= this.Info.Objects.Length)
                {
                    throw new FormatException();
                }

                var obj = this.Info.Objects[index];
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

                if (index >= this.Info.Links.Length)
                {
                    throw new FormatException();
                }

                var link = new Link()
                {
                    Info = this.Info.Links[index],
                };

                value = link;
            }
        }
        #endregion
    }
}
