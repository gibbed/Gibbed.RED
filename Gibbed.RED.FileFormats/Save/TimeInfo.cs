using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Save
{
    public class TimeInfo : ISaveBlock
    {
        public byte[] NightTime
        {
            get { return this._NightTime; }
            set { this._NightTime = value; }
        }

        private byte[] _NightTime;

        public void Serialize(ISaveStream stream)
        {
            stream.SerializeValue("night_time", ref this._NightTime);
        }
    }
}
