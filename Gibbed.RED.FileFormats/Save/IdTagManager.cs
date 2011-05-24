using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Save
{
    public class IdTagManager : ISaveBlock
    {
        private ulong _TagIndex;

        public void Serialize(ISaveStream stream)
        {
            stream.SerializeValue("tagIndex", ref this._TagIndex);
        }
    }
}
