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
    
    internal class rb_f_global_variables : MethodBody0 //author: kjg, status: partial
    {
        internal static rb_f_global_variables singleton = new rb_f_global_variables();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            // Not all globals are in the table, so we need special 
            // code to add the missing entries to the list.  FIXME kjg
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            foreach (System.Collections.Generic.KeyValuePair<string, global_variable> var in Variables.global_vars)
                if (/*var.Value.getter(var.Key, caller) != null && */!String.ListContains(list, var.Key))
                    list.Add(new String(var.Key));
            return Array.CreateUsing(list);
        }
    }
}
