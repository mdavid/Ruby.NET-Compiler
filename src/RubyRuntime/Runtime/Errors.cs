/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Ruby.Runtime
{
    internal class Errors
    {
        internal static void rb_warn(object message)
        {
            if (Options.ruby_verbose.value == null)
                return;

            Compiler.Compiler.LogWarning("warning: " + message);
        }

        internal static void rb_warning(object message)
        {
            if (!Eval.Test(Options.ruby_verbose.value))
                return;

            Compiler.Compiler.LogWarning("warning: " + message);
        }
    }
}
