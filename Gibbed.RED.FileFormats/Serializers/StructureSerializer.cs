using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Serializers
{
    public class StructureSerializer<TType> : IPropertySerializer
        where TType : Game.TTypedClass, new()
    {
        public void Serialize(IFileStream stream, object value)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(IFileStream stream)
        {
            var obj = new TType();
            PropertySerializer.Serialize(obj, stream);
            return obj;
        }
    }
}
