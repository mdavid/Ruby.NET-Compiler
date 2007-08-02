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
    
    internal class rb_f_binding : MethodBody0 //status: done
    {
        internal static rb_f_binding singleton = new rb_f_binding();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Binding(caller, recv);
        }
    }
}
