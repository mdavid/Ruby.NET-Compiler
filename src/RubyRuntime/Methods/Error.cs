/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using Ruby.Runtime;
using System;

namespace Ruby.Methods
{
    
    internal class rb_warn_m : MethodBody1 // author: cjs, status: done
    {
        internal static rb_warn_m singleton = new rb_warn_m();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (Options.ruby_verbose.value != null)
            {
                IO.rb_io_write(IO.rb_stderr.value, p1, caller);
                IO.rb_io_write(IO.rb_stderr.value, IO.rb_default_rs.value, caller);
            }
            return null;
        }
    }
}
