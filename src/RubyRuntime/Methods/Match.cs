/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Globalization;

namespace Ruby.Methods
{
    
    internal class match_alloc : MethodBody0 // author: cjs, status: done
    {
        internal static match_alloc singleton = new match_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Match((Class)recv);
        }
    }


    
    internal class match_to_a : MethodBody0 // author: cjs, status: done
    {
        internal static match_to_a singleton = new match_to_a();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Match.match_array((Match)recv, 0);
        }
    }

     
    internal class match_captures : MethodBody0 //author: cjs, status: done
    {
        internal static match_captures singleton = new match_captures();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Match.match_array((Match)recv, 1);
        }
    }


    
    internal class match_size : MethodBody0 //status: done
    {
        internal static match_size singleton = new match_size();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Match)recv).value.Length;
        }
    }


    
    internal class rb_reg_match_post : MethodBody0 //status: done
    {
        internal static rb_reg_match_post singleton = new rb_reg_match_post();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Match md = (Match) recv;
            String str = new String(md.matched.Substring(md.value.Index + md.value.Length));
            if (md.Tainted) str.Tainted = true;
            return str;
        }
    }


    
    internal class rb_reg_match_pre : MethodBody0 //status: done
    {
        internal static rb_reg_match_pre singleton = new rb_reg_match_pre();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Match md = (Match)recv;
            String str = new String(md.matched.Substring(0, md.value.Index));
            if (md.Tainted) str.Tainted = true;
            return str;
        }
    }


    
    internal class match_string : MethodBody0 //status: done
    {
        internal static match_string singleton = new match_string();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String(((Match)recv).matched);
        }
    }


    
    internal class match_to_s : MethodBody0 //status: done
    {
        internal static match_to_s singleton = new match_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String(((Match)recv).value.Value);
        }
    }


    
    internal class match_begin : MethodBody1 //status: done
    {
        internal static match_begin singleton = new match_begin();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return ((Match)recv).value.Captures[(int)param0].Index;
        }
    }


    
    internal class match_end : MethodBody1 //status: done
    {
        internal static match_end singleton = new match_end();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            System.Text.RegularExpressions.Capture capture
                = ((Match)recv).value.Captures[(int)param0];

            return capture.Index + capture.Length;
        }
    }


    
    internal class match_offset : MethodBody1 //status: done
    {
        internal static match_offset singleton = new match_offset();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            System.Text.RegularExpressions.Capture capture
                = ((Match)recv).value.Captures[(int)param0];

            return new Array(capture.Index, capture.Index + capture.Length);
        }
    }

    
    internal class match_values_at : MethodBody //status: done
    {
        internal static match_values_at singleton = new match_values_at();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            System.Text.RegularExpressions.CaptureCollection captures
                = ((Match)recv).value.Captures;

            Array result = new Array();
            foreach (int index in args)
            {
                int i = (index < 0) ? captures.Count - index : index;
                result.value.Add(new String(captures[i].Value));
            }

            return result;
        }
    }


    
    internal class match_aref : MethodBody //status: done
    {
        internal static match_aref singleton = new match_aref();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            object idx, rest = null;

            Array argv = args.ToRubyArray();

            if (Class.rb_scan_args(caller, argv, 1, 1, false) == 2)
                rest = args[1];
            idx = args[0];

            if (rest != null || !(idx is int) || ((int)idx) < 0)
            {
                return rb_ary_aref.singleton.Call(last_class, match_to_a.singleton.Call0(last_class, recv, caller, null), caller, null, argv);
            }

            return Regexp.rb_reg_nth_match((int)idx, (Match)recv);
        }
    }


    
    internal class match_select : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static match_select singleton = new match_select();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            int argc = argv.Count;
            if (argc > 0)
            {
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "wrong number of arguments ({0} for 0)", argc)).raise(caller);
            }
            else
            {
                Match match = (Match)recv;
                Array result = new Array();
                bool taint = match.Tainted;

                System.Text.RegularExpressions.Match m = match.value;

                while (m.Success)
                {
                    String str = new String(m.Value);
                    if (taint)
                        str.Tainted = true;
                    if (Eval.Test(Proc.rb_yield(block, caller, new object[] { str })))
                    {
                        result.Add(str);
                    }
                    m = m.NextMatch();
                }
                return result;
            }
        }
    }


    
    internal class match_init_copy : MethodBody1 // author: cjs, status: done
    {
        internal static match_init_copy singleton = new match_init_copy();

        public override object Call1(Class last_class, object orig, Frame caller, Proc block, object obj)
        {
            if (obj == orig)
                return obj;

            if (!(obj is Match))
            {
                throw new TypeError("wrong argument class").raise(caller);
            }
            ((Match)obj).matched = ((Match)orig).matched;
            ((Match)obj).value = ((Match)orig).value;

            return obj;
        }
    }
}
