using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Game
{
    public class GameTime : TTypedClass
    {
        [PropertyName("m_seconds")]
        [PropertySerializer(typeof(Serializers.IntSerializer))]
        public int Seconds { get; set; }
    }
}
