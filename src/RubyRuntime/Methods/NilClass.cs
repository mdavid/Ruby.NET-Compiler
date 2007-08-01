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
    
    internal class nil_inspect : MethodBody0 //status: done
    {
        internal static nil_inspect singleton = new nil_inspect();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String("nil");
        }
    }


    
    internal class nil_to_a : MethodBody0 //status: done
    {
        internal static nil_to_a singleton = new nil_to_a();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Array();
        }
    }


    
    internal class nil_to_f : MethodBody0 //status: done
    {
        internal static nil_to_f singleton = new nil_to_f();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Float(0.0);
        }
    }


    
    internal class nil_to_i : MethodBody0 //status: done
    {
        internal static nil_to_i singleton = new nil_to_i();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return 0;
        }
    }


    
    internal class nil_to_s : MethodBody0 //status: done
    {
        internal static nil_to_s singleton = new nil_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String("");
        }
    }

    
    internal class nil_p : MethodBody0 //author: cjs, status: done
    {
        internal static nil_p singleton = new nil_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return true;
        }
    }
}
