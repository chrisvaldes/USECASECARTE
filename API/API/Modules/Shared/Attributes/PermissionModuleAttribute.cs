using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PermissionModuleAttribute : Attribute
    {
        public string Name { get; }

        public PermissionModuleAttribute(string name)
        {
            Name = name;
        }
    }
}
