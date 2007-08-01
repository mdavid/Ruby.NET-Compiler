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
    
    internal class prec_induced_from : MethodBody1 // author: cjs, status: done
    {
        internal static prec_induced_from singleton = new prec_induced_from();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            throw new TypeError(string.Format("undefined conversion from {0} into {1}", p1, recv)).raise(caller);
        }
    }

    
    internal class prec_included : MethodBody1 // author: cjs, status: done
    {
        internal static prec_included singleton = new prec_included();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Object.CheckType<Class>(caller, p1);

            Class.rb_define_singleton_method(p1, "induced_from", prec_induced_from.singleton, 1, caller);
            return recv;
        }
    }

    
    internal class prec_prec : MethodBody1 // author: cjs, status: done
    {
        internal static prec_prec singleton = new prec_prec();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return Eval.CallPrivate(recv, caller, "induced_from", block, new object[] { p1 });
        }
    }

    
    internal class prec_prec_i : MethodBody0 // author: cjs, status: done
    {
        internal static prec_prec_i singleton = new prec_prec_i();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return prec_prec.singleton.Call1(last_class, recv, caller, block, Ruby.Runtime.Init.rb_cInteger);
        }
    }

    
    internal class prec_prec_f : MethodBody1 // author: cjs, status: done
    {
        internal static prec_prec_f singleton = new prec_prec_f();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return prec_prec.singleton.Call1(last_class, recv, caller, block, Ruby.Runtime.Init.rb_cFloat);
        }
    }
}
