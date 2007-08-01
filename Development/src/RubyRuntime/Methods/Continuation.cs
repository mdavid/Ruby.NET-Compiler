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
    
    internal class rb_cont_call : VarArgMethodBody1 //status: not supported
    {
        internal static rb_cont_call singleton = new rb_cont_call();
        
        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest)
        {
            throw new NotImplementedError("rb_cont_call").raise(caller);
        }
    }

    

    internal class rb_callcc : MethodBody0 //status: not supported 
    {
        internal static rb_callcc singleton = new rb_callcc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw new NotImplementedError("rb_callcc").raise(caller);

        }
    }
}
