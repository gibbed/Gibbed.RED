using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Resource
{
    public class Dummy : IFileObject
    {
        public byte[] Data;

        public void Serialize(IFileStream stream)
        {
            if (stream.Mode == SerializeMode.Reading)
            {
                stream.SerializeValue(ref this.Data, (int)stream.Length);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
