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
    
    internal class rb_reg_s_alloc : MethodBody0 //status: done
    {
        internal static rb_reg_s_alloc singleton = new rb_reg_s_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Regexp((Class)recv);
        }
    }

    
    internal class rb_reg_s_quote : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_reg_s_quote singleton = new rb_reg_s_quote();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            String str;//, kcode;
            //int kcode_saved = reg_kcode;

            if (Class.rb_scan_args(caller, argv, 1, 1, false) == 2)
            {
                //rb_set_kcode(StringValuePtr(kcode));
                //curr_kcode = reg_kcode;
                //reg_kcode = kcode_saved;
            }
            str = String.RStringValue(argv[0], caller);
            str = Regexp.rb_reg_quote(str);
            //kcode_reset_option();

            return str;
        }
    }


    
    internal class rb_reg_s_union : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_reg_s_union singleton = new rb_reg_s_union();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            int argc = argv.Count;
            if (argc == 0)
            {
                return new Regexp("(?!)", 0);
            }
            else if (argc == 1)
            {
                Regexp v = Object.CheckConvert<Regexp>(argv[0], "to_regexp", caller);
                if (v != null)
                {
                    return v;
                }
                else
                {
                    object quote = rb_reg_s_quote.singleton.Call1(last_class, recv, caller, null, argv[0]);

                    object re = rb_reg_s_alloc.singleton.Call0(last_class, recv, caller, null);
                    return rb_reg_initialize_m.singleton.Call(last_class, re, caller, null, new Array(new object[] { quote }));
                }
            }
            else
            {
                System.Text.StringBuilder source = new System.Text.StringBuilder();
                for (int i = 0; i < argv.Count; i++)
                {
                    if (0 < i)
                        source.Append("|");
                    Regexp v = Object.CheckConvert<Regexp>(argv[i], "to_regexp", caller);
                    if (v != null)
                        source.Append(rb_reg_to_s.singleton.Call0(last_class, v, caller, null));
                    else
                        source.Append(rb_reg_s_quote.singleton.Call(last_class, null, caller, null, new Array(new object[] { argv[i] })));

                }
                return new Regexp(source.ToString(), 0);
            }
        }  
    }


    
    internal class rb_reg_s_last_match : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_reg_s_last_match singleton = new rb_reg_s_last_match();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            if (Class.rb_scan_args(caller, argv, 0, 1, false) == 1)
            {
                return Regexp.rb_reg_nth_match(Numeric.rb_num2long(argv[0], caller), Regexp.rb_backref_get(caller));
            }
            return Regexp.match_glob.getter("", caller);
        }
    }

    
    internal class rb_reg_match : MethodBody1 // author: cjs, status: done
    {
        internal static rb_reg_match singleton = new rb_reg_match();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 == null)
            {

                Regexp.rb_backref_set(null, caller);
                return null;
            }

            Regexp re = (Regexp)recv;
            String str = String.RStringValue(param0, caller);

            int start = re.rb_reg_search(str, 0, false, caller);
            if (start < 0)
            {
                return null;
            }
            return start;
        }
    }


    
    internal class rb_reg_match2 : MethodBody0 // author: cjs, status: done
    {
        internal static rb_reg_match2 singleton = new rb_reg_match2();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Regexp re = (Regexp)recv;
            int start;
            object line = Eval.rb_lastline_get(caller);

            if (!(line is String))
            {
                Regexp.rb_backref_set(null, caller);
                return null;
            }

            start = re.rb_reg_search((String)line, 0, false, caller);
            if (start < 0)
            {
                return null;
            }
            return start;
        }
    }


    
    internal class rb_reg_equal : MethodBody1 //status: done
    { 
        internal static rb_reg_equal singleton = new rb_reg_equal();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (recv == param0)
                return true;
            else if (!(param0 is Regexp))
                return false;

            Regexp a = (Regexp)recv;
            Regexp b = (Regexp)param0;

            return a.value.ToString() == b.value.ToString()
                && a.value.Options == b.value.Options;
        }
    }


    
    internal class rb_reg_hash : MethodBody0 //status: done
    {
        internal static rb_reg_hash singleton = new rb_reg_hash();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Regexp)recv).value.GetHashCode();
        }
    }


    
    internal class rb_reg_inspect : MethodBody0 //status: done
    {
        internal static rb_reg_inspect singleton = new rb_reg_inspect();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Regexp re = (Regexp)recv;

            return Regexp.rb_reg_desc(caller, re);
        }
    }


    
    internal class rb_reg_source : MethodBody0 //status: done
    {
        internal static rb_reg_source singleton = new rb_reg_source();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String(((Regexp)recv).value.ToString());
        }
    }


    
    internal class rb_reg_casefold_p : MethodBody0 //status: done
    {
        internal static rb_reg_casefold_p singleton = new rb_reg_casefold_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return System.Text.RegularExpressions.RegexOptions.None != (((Regexp)recv).value.Options | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
    }


    
    internal class rb_reg_kcode_m : MethodBody0 //status: done
    {
        internal static rb_reg_kcode_m singleton = new rb_reg_kcode_m();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Regexp re = (Regexp)recv;

            if ((re._options & (int)Regexp.KCODE.FIXED) > 0)
            {
                switch ((Regexp.KCODE)(re._options & (int)Regexp.KCODE.MASK))
                {
                    case Regexp.KCODE.NONE:
                        return new String("none");
                    case Regexp.KCODE.EUC:
                        return new String("euc");
                    case Regexp.KCODE.SJIS:
                        return new String("sjis");
                    case Regexp.KCODE.UTF8:
                        return new String("utf8");
                    default:
                        Exception.rb_bug("unknown kcode - should not happen", caller);
                        break;
                }
            }
            
            return null;
        }
    }


    
    internal class rb_reg_eqq : MethodBody1 //status: done
    {
        internal static rb_reg_eqq singleton = new rb_reg_eqq();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object str)
        {
            if (!(str is String))
                str = String.rb_check_string_type(str, caller);

            if (str == null)
            {
                Regexp.rb_backref_set(null, caller);
                return false;
            }

            return ((Regexp)recv).rb_reg_search((String)str, 0, false, caller) >= 0;
        }
    }


    
    internal class rb_reg_match_m : MethodBody1 // author: cjs, status: done
    {
        internal static rb_reg_match_m singleton = new rb_reg_match_m();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            object result = rb_reg_match.singleton.Call1(last_class, recv, caller, null, param0);

            if (result == null)
                return null;
            result = Regexp.rb_backref_get(caller);

            return result;
        }
    }


    
    internal class rb_reg_options_m : MethodBody0 // author: cjs, status: done
    {
        internal static rb_reg_options_m singleton = new rb_reg_options_m();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Regexp re = (Regexp)recv;

            if (re == null)
                return null;

            return re._options;
        }
    }


    
    internal class rb_reg_to_s : MethodBody0 //author: cjs, status: done
    { 
        internal static rb_reg_to_s singleton = new rb_reg_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Regexp re = (Regexp)recv;

            Regexp.rb_reg_check(caller, re);

            bool m = false, i = false, x = false;

            if ((re._options & (int)Regexp.RE_OPTION.IGNORECASE) == (int)Regexp.RE_OPTION.IGNORECASE)
                i = true;

            if ((re._options & (int)Regexp.RE_OPTION.EXTENDED) == (int)Regexp.RE_OPTION.EXTENDED)
                x = true;

            if ((re._options & (int)Regexp.RE_OPTION.MULTILINE) == (int)Regexp.RE_OPTION.MULTILINE)
                m = true;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("(?");

            if (m)
                sb.Append("m");
            if (i)
                sb.Append("i");
            if (x)
                sb.Append("x");

            if (!(m && i && x))
            {
                sb.Append("-");
                if (!m)
                    sb.Append("m");
                if (!i)
                    sb.Append("i");
                if (!x)
                    sb.Append("x");
            }

            sb.Append(":");
            sb.Append(re.value.ToString());
            sb.Append(")");

            String result = new String(sb.ToString());
            result.Tainted = re.Tainted;
            return result;
        }
    }


    
    internal class rb_reg_initialize_m : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_reg_initialize_m singleton = new rb_reg_initialize_m();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            Regexp self = (Regexp)recv;
            int argc = argv.Count;
            string s;
            int flags = 0;

            TypeError.rb_check_frozen(caller, self);
            if (argc == 0 || argc > 3)
            {
                throw new ArgumentError("wrong number of arguments").raise(caller);
            }
            if (argv[0] is Regexp)
            {
                if (argc > 1)
                {
                    Errors.rb_warn(string.Format(CultureInfo.InvariantCulture, "flags{0} ignored", (argc == 3) ? " and encoding" : ""));
                }
                Regexp.rb_reg_check(caller, (Regexp)argv[0]);
                flags = ((Regexp)argv[0])._options;
                s = ((Regexp)argv[0]).pattern;
            }
            else
            {
                if (argc >= 2)
                {
                    if (argv[1] is int)
                        flags = (int)argv[1];
                    else if (argv[1] is Regexp.RE_OPTION)
                        flags = (int)argv[1];
                    else if (Eval.Test(argv[1]))
                        flags = (int)Regexp.RE_OPTION.IGNORECASE;
                }
                if (argc == 3 && argv[2] != null)
                {
                    string kcode = String.StringValue(argv[2], caller);

                    flags &= ~0x70;
                    flags |= (int)Regexp.KCODE.FIXED;
                    switch (kcode[0])
                    {
                        case 'n':
                        case 'N':
                            flags |= (int)Regexp.KCODE.NONE;
                            break;
                        case 'e':
                        case 'E':
                            flags |= (int)Regexp.KCODE.EUC;
                            break;
                        case 's':
                        case 'S':
                            flags |= (int)Regexp.KCODE.SJIS;
                            break;
                        case 'u':
                        case 'U':
                            flags |= (int)Regexp.KCODE.UTF8;
                            break;
                        default:
                            break;
                    }
                }
                s = ((String)argv[0]).value;
            }

            self.Init(s, flags);

            return self;
        }
    }


    
    internal class rb_reg_init_copy : MethodBody1 // author: cjs, status: done
    {
        internal static rb_reg_init_copy singleton = new rb_reg_init_copy();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object copy)
        {
            Regexp re = (Regexp)recv;

            if (copy == re)
                return copy;
            TypeError.rb_check_frozen(caller, copy);

            if (!(copy is Regexp))
            {
                throw new TypeError("wrong argument type").raise(caller);
            }
            Regexp.rb_reg_check(caller, re);

            Regexp cpy = (Regexp)copy;
            cpy.Init(re.pattern, re._options);

            return cpy;
        }
    }
}
