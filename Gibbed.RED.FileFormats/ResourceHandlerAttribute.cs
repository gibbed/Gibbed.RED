using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.RED.FileFormats
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = true, Inherited = false)]
    public class ResourceHandlerAttribute : Attribute
    {
        public string Name;

        public ResourceHandlerAttribute(string name)
        {
            this.Name = name;
        }
    }
}
