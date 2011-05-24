using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Save
{
    public class QuestExternalScenePlayer : ISaveBlock
    {
        public void Serialize(ISaveStream stream)
        {
            if (stream.Mode == SerializeMode.Reading)
            {
                uint tagsCount = 0;
                stream.SerializeValue("tagsCount", ref tagsCount);

                for (uint i = 0; i < tagsCount; i++)
                {
                    string tag = null;
                    stream.SerializeValue("tag", ref tag);

                    uint dialogsCount = 0;
                    stream.SerializeValue("dialogsCount", ref dialogsCount);

                    for (uint j = 0; j < dialogsCount; j++)
                    {
                        Guid guid = Guid.Empty;
                        stream.SerializeValue("guid", ref guid);
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
