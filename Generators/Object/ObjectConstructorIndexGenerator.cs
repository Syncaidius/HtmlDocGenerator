//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace HtmlDocGenerator
//{
//    public class ObjectConstructorIndexGenerator : ObjectMethodIndexGenerator
//    {
//        public override string GetTitle()
//        {
//            return "Constructors";
//        }

//        protected override bool IsValidMethod(MethodBase member)
//        {
//            return member.IsConstructor;
//        }

//        protected override string GetMethodName(DocObject obj, MethodBase member)
//        {
//            return obj.Name;
//        }

//        public override MemberTypes MemberType { get; } = MemberTypes.Constructor;
//    }
//}
