using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CustomPermissionAttribute : Attribute
    {
        public string Code { get; }
        public string Name { get; }

        public CustomPermissionAttribute(string code, string name)
        {
            Code = code;
            Name = name;
        }
    }
}
