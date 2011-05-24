using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Save
{
    public class Quest : ISaveBlock
    {
        private string _FileName;
        public QuestData _Data;

        public void Serialize(ISaveStream stream)
        {
            stream.SerializeValue("fileName", ref this._FileName);
            stream.SerializeBlock("questThread", ref this._Data);
            throw new NotImplementedException();
        }
    }
}
