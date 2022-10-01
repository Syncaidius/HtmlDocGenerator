﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlDocGenerator
{
    public class DocObject
    {
        public DocObject(string name, DocObjectType type)
        {
            Name = name;
            Type = type;
        }

        public DocObject AddMember(string name, DocObjectType type)
        {
            DocObject member = new DocObject(name, type);

            Members.Add(name, member);
            return member;
        }

        public override string ToString()
        {
            return $"{Name} - {Type} - Members: {Members.Count}";
        }

        public string Name { get; set; }

        public Dictionary<string, DocObject> Members { get; } = new Dictionary<string, DocObject>();

        public DocObjectType Type { get; set; }
    }

    public enum DocObjectType
    {
        None = 0,
         
        /// <summary>
        /// A valid type, it's unknown whether it is a class, struct or enum.
        /// </summary>
        UnspecifiedType = 1,

        Class = 2,

        Struct = 3,

        Enum = 4,

        Event = 5,

        Field = 6,

        Property = 7,

        Method = 8,
    }
}
