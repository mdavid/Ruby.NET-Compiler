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
    
    internal class rb_gc_start : MethodBody0 // status: done
    {
        internal static rb_gc_start singleton = new rb_gc_start();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            System.GC.Collect();
            return null;
        }
    }

    
    internal class rb_gc_enable : MethodBody0 // status: done
    {
        internal static rb_gc_enable singleton = new rb_gc_enable();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            bool old = GC.dont_gc;
            GC.dont_gc = false;
            return old;
        }
    }

    
    internal class rb_gc_disable : MethodBody0 // status: done
    {
        internal static rb_gc_disable singleton = new rb_gc_disable();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            bool old = GC.dont_gc;

            GC.dont_gc = true;
            return old;
        }
    }
}
