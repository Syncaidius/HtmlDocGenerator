using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public enum DocObjectType
    {
        Unknown = 0,

        ObjectType = 1,

        Class = 2,

        Struct = 3,

        Enum = 4,

        Interface = 5,

        Event = 6,

        Field = 7,

        Property = 8,

        Method = 9,

        Invalid = 10,
    }
}
