using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Save
{
    public class QuestData : ISaveBlock
    {
        public List<QuestBlock> _Blocks;

        public void Serialize(ISaveStream stream)
        {
            if (stream.Mode == SerializeMode.Reading)
            {
                uint numBlocks = 0;
                stream.SerializeValue("numBlocks", ref numBlocks);
                this._Blocks = new List<QuestBlock>();
                for (uint i = 0; i < numBlocks; i++)
                {
                    QuestBlock block = null;
                    stream.SerializeBlock("questBlock", ref block);
                    this._Blocks.Add(block);
                }

                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
