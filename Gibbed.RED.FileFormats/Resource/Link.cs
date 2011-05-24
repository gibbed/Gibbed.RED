using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Resource
{
    public class Link : IFileObject
    {
        public LinkInfo Info;

        public void Serialize(IFileStream stream)
        {
            throw new NotSupportedException();
        }
    }
}
