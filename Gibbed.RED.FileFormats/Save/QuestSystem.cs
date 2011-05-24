using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Save
{
    public class QuestSystem : ISaveBlock
    {
        private List<QuestExternalScenePlayer> _QuestExternalScenePlayers;
        private List<Quest> _Quests;

        public void Serialize(ISaveStream stream)
        {
            // supposedly only ever has two blocks but oh well...
            stream.SerializeBlocks(
                "questExternalScenePlayers",
                "CQuestExternalScenePlayer",
                ref this._QuestExternalScenePlayers);

            if (stream.Mode == SerializeMode.Reading)
            {
                uint numQuests = 0;
                stream.SerializeValue("numQuests", ref numQuests);

                this._Quests = new List<Quest>();
                for (uint i = 0; i < numQuests; i++)
                {
                    Quest quest = null;
                    stream.SerializeBlock("quest", ref quest);
                    this._Quests.Add(quest);
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
