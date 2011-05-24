using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats.Resource
{
    internal struct Info
    {
        public string[] Names;
        public LinkInfo[] Links;
        public ObjectInfo[] Objects;
        public string[] Dependencies;
    }
}
