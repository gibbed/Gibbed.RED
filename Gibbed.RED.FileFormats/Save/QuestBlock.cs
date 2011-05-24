using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Save
{
    public class QuestBlock : ISaveBlock
    {
        private Guid _GUID;
        private List<string> _InputNames;
        private int _ActivationState;

        public void Serialize(ISaveStream stream)
        {
            if (stream.Mode == SerializeMode.Reading)
            {
                stream.SerializeValue("GUID", ref this._GUID);

                uint inputNamesCount = 0;
                stream.SerializeValue("inputNamesCount", ref inputNamesCount);
                this._InputNames = new List<string>();
                for (uint i = 0; i < inputNamesCount; i++)
                {
                    string inputName = null;
                    stream.SerializeValue("inputName", ref inputName);
                    this._InputNames.Add(inputName);
                }

                stream.SerializeValue("activationState", ref this._ActivationState);

                // it appears there is data loaded here
                // depending on the actual quest data :(
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
