/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


using Ruby;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Ruby.Runtime
{
    // Ruby.global_variable - runtime representation of Ruby global variables

    [UsedByRubyCompiler]
    public class global_variable
    {
        [UsedByRubyCompiler]
        public object value;

        internal List<Proc> trace;
        bool block_trace = false;

        internal virtual object getter(string id, Frame caller)
        {
            return value; 
        }

        internal virtual void setter(string id, object val, Frame caller)
        { 
            value = val;

            if (trace != null && !block_trace)
            {
                block_trace = true;
                try
                {
                    foreach (Proc proc in trace)
                        proc.body.Calln(null, null, caller, new ArgList(null, val));
                }
                finally
                {
                    block_trace = false;
                }
            }
        }
    }

    
    internal class readonly_global : global_variable
    {
        internal override void setter(string id, object val, Frame caller)
        {
            throw new NameError(id, id + " is a read-only variable").raise(caller);
        }
    }

    
    internal class str_global : global_variable
    {
        internal override void setter(string id, object val, Frame caller)
        {
            if ((val != null) && !(val is String))
                throw new TypeError(string.Format(CultureInfo.InvariantCulture, "value of {0} must be String", id)).raise(caller);

            this.value = val;
        }
    }

    internal class alias_global : global_variable
    {
        internal string aliased;

        public alias_global(string aliased)
        {
            this.aliased = aliased;
        }

        internal override object getter(string id, Frame caller)
        {
            Match tilde;
            switch (aliased)
            {
                case "$&":
                    tilde = caller.Tilde;
                    if (tilde != null)
                        return tilde.last_match(caller);
                    else
                        return null;
                case "$`":
                    tilde = caller.Tilde;
                    if (tilde != null)
                        return tilde.match_pre(caller);
                    else
                        return null;
                case "$'":
                    tilde = caller.Tilde;
                    if (tilde != null)
                        return tilde.match_post(caller);
                    else
                        return null;
                case "$+":
                    tilde = caller.Tilde;
                    if (tilde != null)
                        return tilde.match_last(caller);
                    else
                        return null;
                default:
                    break;
            }
            return Variables.gvar_get(aliased, caller);
        }

        internal override void setter(string id, object val, Frame caller)
        {
            if (aliased != "$&" && aliased != "$`" && aliased != "$'" && aliased != "$+")
                Variables.gvar_set(aliased, val, caller);
            else
            {
                throw new NameError(id + " is a read-only variable").raise(caller);
            }
        }
    }
}
