using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats
{
    public interface IFileStream
    {
        SerializeMode Mode { get; }
        long Position { get; set; }
        long Length { get; }

        void SerializeValue(ref bool value);
        void SerializeValue(ref sbyte value);
        void SerializeValue(ref byte value);
        void SerializeValue(ref short value);
        void SerializeValue(ref ushort value);
        void SerializeValue(ref int value);
        void SerializeValue(ref uint value);
        void SerializeValue(ref float value);
        void SerializeValue(ref string value);
        void SerializeValue(ref Guid value);
        void SerializeValue(ref byte[] value, int length);
        void SerializeValue(ref byte[] value, uint length);
        void SerializeBuffer(ref byte[] value);
        void SerializeName(ref string value);
        void SerializeTagList(ref List<string> value);
        void SerializeObject<TType>(ref TType value)
            where TType : IFileObject, new();
        void SerializePointer(ref IFileObject value);
        void SerializePointer(ref List<IFileObject> value, bool encoded);
    }
}
