﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class ObjectEventIndexGenerator : ObjectMemberSectionGenerator<EventInfo>
    {
        public override string GetTitle()
        {
            return "Events";
        }
    }
}
