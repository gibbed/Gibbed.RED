using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats
{
    public interface IFileObject
    {
        void Serialize(IFileStream stream);
    }
}
