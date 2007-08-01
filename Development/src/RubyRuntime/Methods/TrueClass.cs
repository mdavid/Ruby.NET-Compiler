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
    
    internal class true_to_s : MethodBody0 //status: done
    {
        internal static true_to_s singleton = new true_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String("true");
        }
    }


    
    internal class true_and : MethodBody1 //status: done
    {
        internal static true_and singleton = new true_and();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is bool)
                return (bool)param0;

            return param0 != null;
        }
    }


    
    internal class true_or : MethodBody1 //status: done
    {
        internal static true_or singleton = new true_or();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return true;
        }
    }


    
    internal class true_xor : MethodBody1 //status: done
    {
        internal static true_xor singleton = new true_xor();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is bool)
                return !(bool)param0;
            
            return param0 == null;
        }
    }
}
