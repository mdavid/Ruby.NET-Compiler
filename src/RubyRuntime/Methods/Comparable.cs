/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using Ruby.Runtime;

namespace Ruby.Methods
{
    
    internal class cmp_equal : MethodBody1 //status: done
    {
        internal static cmp_equal singleton = new cmp_equal();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            try
            {
                return Comparable.Compare(recv, param0, caller) == 0;
            }
            catch
            {
                return null;
            }
        }
    }

    
    internal class cmp_gt : MethodBody1 //status: done
    {
        internal static cmp_gt singleton = new cmp_gt();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return Comparable.Compare(recv, param0, caller) > 0;
        }
    }

    
    internal class cmp_ge : MethodBody1 //status: done
    {
        internal static cmp_ge singleton = new cmp_ge();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return Comparable.Compare(recv, param0, caller) >= 0;
        }
    }

    
    internal class cmp_lt : MethodBody1 //status: done
    {
        internal static cmp_lt singleton = new cmp_lt();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return Comparable.Compare(recv, param0, caller) < 0;
        }
    }

    
    internal class cmp_le : MethodBody1 //status: done
    {
        internal static cmp_le singleton = new cmp_le();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return Comparable.Compare(recv, param0, caller) <= 0;
        }
    }

    
    internal class cmp_between : MethodBody2 //status: done
    {
        internal static cmp_between singleton = new cmp_between();

        public override object Call2(Class last_class, object x, Frame caller, Proc block, object min, object max)
        {
            return ((!(bool)cmp_lt.singleton.Call1(last_class, x, caller, null, min))
                && (!(bool)cmp_gt.singleton.Call1(last_class, x, caller, null, max)));
        }
    }
}
