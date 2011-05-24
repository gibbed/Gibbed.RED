using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Save
{
    public class TimeManager : ISaveBlock
    {
        private Game.GameTime _Time;
        private bool _IsPaused;
        private float _RealTime;

        public void Serialize(ISaveStream stream)
        {
            stream.SerializeObject("time", "GameTime", ref this._Time);
            stream.SerializeValue("isPaused", ref this._IsPaused);
            stream.SerializeValue("realTime", ref this._RealTime);
        }
    }
}
