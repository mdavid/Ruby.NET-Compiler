/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;

namespace Ruby.Methods
{
    
    internal class false_to_s : MethodBody0 //status: done
    {
        internal static false_to_s singleton = new false_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String("false");
        }
    }


    
    internal class false_and : MethodBody1 //status: done
    {
        internal static false_and singleton = new false_and();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return false;
        }
    }


    
    internal class false_or : MethodBody1 //status: done
    {
        internal static false_or singleton = new false_or();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is bool)
                return (bool)param0;

            return param0 != null;
        }
    }


    
    internal class false_xor : MethodBody1 //status: done
    {
        internal static false_xor singleton = new false_xor();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is bool)
                return (bool)param0;

            return param0 != null;
        }
    }
}
