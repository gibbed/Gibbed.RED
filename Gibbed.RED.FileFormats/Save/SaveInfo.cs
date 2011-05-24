using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Save
{
    public class SaveInfo : ISaveBlock
    {
        private byte[] _MagicNumber;
        private byte[] _SpecialKey;
        private string _Description;

        public void Serialize(ISaveStream stream)
        {
            stream.SerializeValue("magic_number", ref this._MagicNumber);
            stream.SerializeValue("special_key", ref this._SpecialKey);
            stream.SerializeValue("description", ref this._Description);
        }
    }
}
