using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Save
{
    public interface ISaveStream
    {
        SerializeMode Mode { get; }
        void SerializeValue(string name, ref bool value);
        void SerializeValue(string name, ref sbyte value);
        void SerializeValue(string name, ref byte value);
        void SerializeValue(string name, ref short value);
        void SerializeValue(string name, ref ushort value);
        void SerializeValue(string name, ref int value);
        void SerializeValue(string name, ref uint value);
        void SerializeValue(string name, ref long value);
        void SerializeValue(string name, ref ulong value);
        void SerializeValue(string name, ref float value);
        void SerializeValue(string name, ref string value);
        void SerializeValue(string name, ref Guid value);
        void SerializeValue(string name, ref byte[] value);
        void SerializeBlock<TType>(string name, ref TType value)
            where TType : ISaveBlock, new();
        void SerializeBlocks<TType>(string name, string type, ref List<TType> value)
            where TType : ISaveBlock, new();
        void SerializeObject<TType>(string name, string type, ref TType value)
            where TType : IFileObject, new();
    }
}
