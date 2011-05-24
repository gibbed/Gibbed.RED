using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Game
{
    public abstract class TTypedClass : IFileObject
    {
        public void Serialize(IFileStream stream)
        {
            PropertySerializer.Serialize(this, stream);
        }
    }
}
