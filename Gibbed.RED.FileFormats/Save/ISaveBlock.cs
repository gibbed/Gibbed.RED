using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Save
{
    public interface ISaveBlock
    {
        void Serialize(ISaveStream stream);
    }
}
