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

    internal class str_alloc : MethodBody0 //status: done
    {
        internal static str_alloc singleton = new str_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String((Class)recv);
        }
    }


    internal class rb_str_slice_bang : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_str_slice_bang singleton = new rb_str_slice_bang();

        public override object Call(Class last_class, object str, Frame caller, Proc block, Array argv)
        {
            int argc = argv.Count;
            object result;

            if (argc < 1 || 2 < argc)
            {
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "wrong number of arguments ({0} for 1)", argc)).raise(caller);
            }

            Array buf = new Array();
            int i;
            for (i = 0; i < argc; i++)
            {
                buf.Add(argv[i]);
            }
            result = rb_str_aref_m.singleton.Call(last_class, str, caller, null, buf);
            
            if (result != null)
            {
                buf.Add(new String());
                rb_str_aset_m.singleton.Call(last_class, str, caller, null, buf);
            }

            return result;
        }            
    }

    internal class rb_str_plus : MethodBody1 //status: done
    {
        internal static rb_str_plus singleton = new rb_str_plus();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {                        
            return new String(((String)recv).value + String.StringValue(param0, caller));
        }
    }

    internal class rb_str_init : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_str_init singleton = new rb_str_init();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 1)
            {
                return rb_str_replace.singleton.Call1(last_class, recv, caller, null, rest[0]);
            }
            else
                return recv;
        }
    }


    internal class rb_str_replace : MethodBody1 //author: cjs, status: done 
    {
        internal static rb_str_replace singleton = new rb_str_replace();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (recv == p1)
                return recv;

            String str = (String)recv;
            String str2 = String.RStringValue(p1, caller);

            str.value = str2.value;

            str.Tainted = str2.Tainted;
            return str;
        }
    }

    internal class rb_str_format : MethodBody1 //author: war, status: done 
    {
        internal static rb_str_format singleton = new rb_str_format();

        //needs retesting
        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object arg)
        {
             if (arg is Array)
            {
                ArgList argv = new ArgList(block, new object[] { recv });
                argv.AddRange((Array)arg);
                return rb_f_sprintf.singleton.Calln(last_class, null, caller, argv);
            }

            return rb_f_sprintf.singleton.Call2(last_class, null, caller, block, recv, arg);
        }
    }


    internal class rb_str_aset_m : VarArgMethodBody0 // author: cjs, status: done 
    {
        internal static rb_str_aset_m singleton = new rb_str_aset_m();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            String str = (String)recv;
            int argc = argv.Count;
            if (argc == 3)
            {
                if (argv[0] is Regexp)
                {
                    String.rb_str_subpat_set(str, (Regexp)argv[0], Numeric.rb_num2long(argv[1], caller), argv[2], caller);
                }
                else
                {
                    String.rb_str_splice(caller, str, Numeric.rb_num2long(argv[0], caller), Numeric.rb_num2long(argv[1], caller), argv[2]);
                }
                return argv[2];
            }
            if (argc != 2)
            {
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "wrong number of arguments ({0} for 2)", argc)).raise(caller);
            }
            return String.rb_str_aset(caller, str, argv[0], argv[1]);
        }
    }


    internal class rb_str_upto_m : MethodBody1 // author: cjs, status: done
    {
        internal static rb_str_upto_m singleton = new rb_str_upto_m();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return Call1(last_class, recv, caller, block, p1, false);
        }

        public static object Call1(Class last_class, object recv, Frame caller, Proc block, object p1, bool excl)
        {
            String beg = (String)recv;
            String end = String.RStringValue(p1, caller);

            object current, after_end;
            int n;

            n = beg.value.CompareTo(end.value);
            if (n > 0 || excl && n == 0)
                return beg;
            after_end = (String)Eval.CallPrivate0(end, caller, "succ", null);
            current = beg;
            while (!(bool)rb_str_equal.singleton.Call1(last_class, current, caller, null, after_end))
            {
                Proc.rb_yield(block, caller, new object[] { current });
                if (!excl && (bool)rb_str_equal.singleton.Call1(last_class, current, caller, null, end)) break;
                current = Eval.CallPrivate0(current, caller, "succ", null);
                if (current is String) 
                    current = Eval.CallPrivate0(current, caller, "to_str", null);
                if (!excl && (bool)rb_str_equal.singleton.Call1(last_class, current, caller, null, end)) break;
                if (((String)current).value.Length > ((String)end).value.Length)
                    break;
            }

            return beg;
        }
    }

    internal class rb_str_index_m : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_str_index_m singleton = new rb_str_index_m();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            String str = (String)recv;
            object sub;
            int pos;

            if (Class.rb_scan_args(caller, argv, 1, 1, false) == 2)
            {
                pos = Numeric.rb_num2long(argv[1], caller);
            }
            else
            {
                pos = 0;
            }
            sub = argv[0];

            if (pos < 0)
            {
                pos += str.value.Length;
                if (pos < 0)
                {
                    if (sub is Regexp)
                    {
                        Regexp.rb_backref_set(null, caller);
                    }
                    return null;
                }
            }

            if (sub is Regexp)
            {
                pos = ((Regexp)sub).rb_reg_search(str, pos, false, caller);
            }
            else
            {
                if (sub is int)
                {
                    int c = (int)sub;
                    long len = str.value.Length;
                    string p = str.value;

                    for (; pos < len; pos++)
                    {
                        if (p[pos] == c)
                            return pos;
                    }
                    return null;
                }
                else if (sub is String)
                {
                    // do nothing
                }
                else
                {
                    object tmp = String.rb_check_string_type(sub, caller);
                    if (tmp == null)
                    {

                        string errMsg = Class.CLASS_OF(sub)._name;
                        throw new TypeError(string.Format(CultureInfo.InvariantCulture, "type mismatch: {0} given", errMsg)).raise(caller);                    
                    }
                    sub = tmp;
                }
                pos = str.value.IndexOf(((String)sub).value, pos);
            }

            if (pos == -1)
                return null;

            return pos;
        }
    }


    internal class rb_str_rindex_m : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_str_rindex_m singleton = new rb_str_rindex_m();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            String str = (String)recv;

            object sub;
            int pos;

            Class.rb_scan_args(caller, argv, 1,1, false);
            sub = argv[0];
            if (argv.Count == 2)
            {
                pos = Numeric.rb_num2long(argv[1], caller);
                if (pos < 0)
                {
                    pos += str.value.Length;
                    if (pos < 0)
                    {
                        if (sub is Regexp)
                        {
                            Regexp.rb_backref_set(null, caller);
                        }
                        return null;
                    }
                }
                if (pos > str.value.Length)
                    pos = str.value.Length;
            }
            else
            {
                pos = str.value.Length;
            }

            if (sub is Regexp)
            {
                pos = ((Regexp)sub).rb_reg_search(str, pos, true, caller);
                if (pos >= 0)
                    return pos;
            }
            else if (sub is String)
            {
                pos = str.value.LastIndexOf(((String)sub).value, pos);
                if (pos >= 0)
                    return pos;
            }
            else if (sub is int)
            {
                int c = (int)sub;
                int p = pos;
                int pbeg = 0;

                if (pos == str.value.Length)
                {
                    if (pos == 0)
                        return null;
                    --p;
                }
                while (pbeg <= p)
                {
                    if (str.value[p] == c)
                        return p;
                    p--;
                }
                return null;
            }
            else
            {
                throw new TypeError(string.Format(CultureInfo.InvariantCulture, "type mismatch: {0} given", Class.rb_obj_classname(sub))).raise(caller);
            }

            return null;
        }
    }

    
    internal class rb_str_to_i : VarArgMethodBody0 //author: war, status: done
    {
        internal static rb_str_to_i singleton = new rb_str_to_i();

        public override object Call(Class last_class, object str, Frame caller, Proc block, Array rest)
        {
            int numBase;
            Class.rb_scan_args(caller, rest, 0, 1, false);
            if(rest.Count == 0) 
            {
                numBase = 10;
            }
            else
            {
                numBase = Numeric.rb_num2long(rest[0], caller);
            }

            if(numBase < 0){
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "illegal radix {0}", numBase)).raise(caller);
            }

            return String.rb_str_to_inum(str, caller, numBase, false);
        }
    }

    internal class rb_str_to_f : MethodBody0 //author: war, status: done
    {
        internal static rb_str_to_f singleton = new rb_str_to_f();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Float(String.rb_cstr_to_dbl(caller, ((String)recv).value, false));
        }
    }

    internal class rb_str_dump : MethodBody0 //author: war, status: done
    {
        internal static rb_str_dump singleton = new rb_str_dump();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            string recvString = ((String)recv).value;
            System.Text.StringBuilder dumpString = new System.Text.StringBuilder();

            dumpString.Append('\"');
            int p = 0;
            while(p < recvString.Length)
            {
                char c = recvString[p];
                p++;
                
                if (c == '"' || c == '\\')
                {
                    dumpString.Append('\\');
                    dumpString.Append(c);
                }
                else if (c == '#')
                {
                    if(p < recvString.Length && (recvString[p] == '$' || recvString[p] == '@' || recvString[p] == '{'))
                    {
                        dumpString.Append('\\');
                    }
                    dumpString.Append('#');
                }
                else if (c == '\n')
                {
                    dumpString.Append('\\');
                    dumpString.Append('n');
                }
                else if (c == '\r')
                {
                    dumpString.Append('\\');
                    dumpString.Append('r');
                }
                else if (c == '\t')
                {
                    dumpString.Append('\\');
                    dumpString.Append('\t');
                }
                else if (c == '\f')
                {
                    dumpString.Append('\\');
                    dumpString.Append('\f');
                }
                else if(c == (char)07){ // '\007'
                    dumpString.Append('\\');
                    dumpString.Append('a');
                }
                else if (c == (char)27) // '\033'
                {
                    dumpString.Append('\\');
                    dumpString.Append('e');
                }
                else if (!char.IsControl(c)) //TEST: This must be equivalent to (isascii(c) && isprint(c))
                {
                    dumpString.Append(c);
                }
                else
                {
                    dumpString.Append('\\');                        
                    dumpString.Append(String.DecimalToBase((int)c, 8, 3));
                }
            }
            dumpString.Append('\"');


            return new String(dumpString.ToString());
        }
    }

    internal class rb_str_hex : MethodBody0 //author: war, status: done
    {
        internal static rb_str_hex singleton = new rb_str_hex();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return String.rb_str_to_inum(recv, caller, 16, false);
        }
    }


    internal class rb_str_oct : MethodBody0 //author: war, status: done
    {
        internal static rb_str_oct singleton = new rb_str_oct();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return String.rb_str_to_inum(recv, caller, -8, false);
        }
    }


    internal class rb_str_split_m : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_str_split_m singleton = new rb_str_split_m();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            String str = (String)recv;

            Class.rb_scan_args(caller, rest, 0, 2, false);

            object limit = null;
            int lim = 0;
            int i = 0;
            if (rest.Count == 2)
            {
                limit = rest[1];
                lim = Numeric.rb_num2long(limit, caller);
                if (lim <= 0)
                    limit = null;
                else if (lim == 1)
                {
                    if (str.value.Length == 0)
                        return new Array();
                    return new Array(str);
                }
                i = 1;
            }

            bool awk_split = false;
            object spat = null;
            if (rest.Count > 0)
                spat = rest[0];
            if (spat == null)
            {
                if (String.rb_fs != null && String.rb_fs.value != null)
                    spat = String.rb_fs.value;
                else
                    awk_split = true;
            }
            if (!awk_split)
            {
                if (spat is String && ((String)spat).value.Length == 1)
                {
                    if (((String)spat).value[0] == ' ')
                        awk_split = true;
                    else
                        spat = new Regexp(Regexp.rb_reg_quote((String)spat).value, 0);
                }
                else
                    spat = String.get_pat(spat, true, caller);
            }

            Array result = new Array();
            int beg = 0;
            if (awk_split)
            {
                string[] strings;
                if (limit != null)
                    strings = str.value.Split(new char[] { ' ' }, lim, System.StringSplitOptions.RemoveEmptyEntries);
                else
                    strings = str.value.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in strings)
                {
                    String s1 = new String(s);
                    Object.obj_infect(s1, str);
                    result.Add(s1);

                    if (limit != null)
                        i++;
                }

                beg = str.value.Length;
            }
            else
            {
                Regexp pat = (Regexp)spat;

                int start = beg;
                int end;
                bool last_null = false;

                while ((end = pat.rb_reg_search(str, start, false, caller)) >= 0)
                {
                    Match regs = Regexp.rb_backref_get(caller);
                    if (start == end && regs.value.Length == 0)
                    {
                        if (str.value.Length == 0)
                        {
                            result.Add(new String());
                            break;
                        }
                        else if (last_null)
                        {
                            String s = new String(str.value.Substring(beg, 1));
                            Object.obj_infect(s, str);
                            result.Add(s);
                            beg = start;
                        }
                        else
                        {
                            start += 1;
                            last_null = true;
                            continue;
                        }
                    }
                    else if (start == end && last_null && pat.value.ToString().Equals(regs.value.Value))
                    {
                        beg = start = regs.value.Index + regs.value.Length;
                    }
                    else
                    {
                        String s = new String(str.value.Substring(beg, end - beg));
                        Object.obj_infect(s, str);
                        result.Add(s);
                        beg = start = regs.value.Index + regs.value.Length;
                    }
                    last_null = false;

                    for (int idx = 1; idx < regs.value.Groups.Count; idx++)
                    {
                        String tmp;
                        if (regs.value.Groups[idx].Length == 0)
                            tmp = new String();
                        else
                            tmp = new String(regs.value.Groups[idx].Value);
                        Object.obj_infect(tmp, str);
                        result.Add(tmp);
                    }

                    if (limit != null && lim <= ++i)
                        break;
                }
            }

            if (str.value.Length > 0 && (limit != null || str.value.Length > beg || lim < 0))
            {
                String tmp;
                if (str.value.Length == beg)
                    tmp = new String();
                else
                    tmp = new String(str.value.Substring(beg));
                Object.obj_infect(tmp, str);
                result.Add(tmp);
            }

            if (limit == null && lim == 0)
                while (result.Count > 0 && ((String)result.value[result.Count - 1]).value.Length == 0)
                    result.value.RemoveAt(result.value.Count - 1);

            return result;
        }
    }


    internal class rb_str_crypt : MethodBody1 // author: war, status: done
    {
        internal static rb_str_crypt singleton = new rb_str_crypt();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            String str = (String)recv;
            String salt = String.RStringValue(p1, caller);

            string s;

            if (salt.value.Length < 2)
                throw new ArgumentError("salt too short(need >=2 bytes)").raise(caller);

            if (str.value != null)
                s = str.value;
            else
                s = "";

            String result = new String(Crypt.crypt(salt.value, s));
            result.Tainted |= str.Tainted;
            result.Tainted |= salt.Tainted;
            return result;
        }
    }

    internal class rb_str_intern : MethodBody0 // author: cjs, status: done
    {
        internal static rb_str_intern singleton = new rb_str_intern();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            String str = (String)recv;
            if (str.value == null || str.value.Length == 0)
            {
                throw new ArgumentError("interning empty string").raise(caller);
            }

            return new Symbol(str.value);
        }
    }

    internal class rb_str_include : MethodBody1 //author: war, status: done
    {
        internal static rb_str_include singleton = new rb_str_include();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object arg)
        {
            string argString;

            if (arg is int)
            {
                if (((String)recv).value.Contains(((int)arg).ToString(CultureInfo.InvariantCulture)))
                {
                    return true;
                }
                return false;
            }

            argString = String.StringValue(arg, caller);
            return ((String)recv).value.Contains(argString);               
        }
    }

    internal class rb_str_scan : MethodBody1 // author: cjs, status: done
    {
        internal static rb_str_scan singleton = new rb_str_scan();

        private object scan_once(String str, Regexp pat, ref int start, Frame caller)
        {
            Match match;
            if (pat.rb_reg_search(str, start, false, caller) >= 0)
            {
                match = Regexp.rb_backref_get(caller);

                if (match.value.Length == 0)
                    /*
                     * Always consume at least one character of the input string
                     */
                    start = match.value.Index + 1;
                else
                    start = match.value.Index + match.value.Length;

                if (match.value.Groups.Count == 1)
                    return Regexp.rb_reg_nth_match(0, match);
                else
                {
                    Array result = new Array();
                    for (int i = 1; i < match.value.Groups.Count; i++)
                        result = result.Add(Regexp.rb_reg_nth_match(i, match));
                    return result;
                }
            }
            else
                return null;
        }

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            String str = (String)recv;
            Regexp pat = String.get_pat(p1, true, caller);

            object result;
            int start = 0;
            Match match = null;

            if (block == null)
            {
                Array ary = new Array();

                while ((result = scan_once(str, pat, ref start, caller)) != null)
                {
                    match = Regexp.rb_backref_get(caller);
                    ary = ary.Add(result);
                }
                Regexp.rb_backref_set(match, caller);
                return ary;
            }

            while ((result = scan_once(str, pat, ref start, caller)) != null)
            {
                match = Regexp.rb_backref_get(caller);
                match.busy = true;
                Proc.rb_yield(block, caller, new object[] { result });
                Regexp.rb_backref_set(match, caller);    /* restore $~ value */
            }
            Regexp.rb_backref_set(match, caller);

            return str;
        }
    }

    internal class rb_str_sub : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_str_sub singleton = new rb_str_sub();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            String str = ((String)recv).rb_str_dup(caller);
            rb_str_sub_bang.singleton.Call(last_class, str, caller, block, rest);
            return str;
        }
    }

    internal class rb_str_gsub : VarArgMethodBody0 //author: cjs, status: done
    {
        internal static rb_str_gsub singleton = new rb_str_gsub();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return String.str_gsub((String)recv, caller, block, rest, false);
        }
    }

    internal class rb_str_sub_bang : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_str_sub_bang singleton = new rb_str_sub_bang();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            String str = (String)recv;
            Regexp pat;
            Match match;
            String repl = null;
            bool iter = false;
            bool tainted = false;

            if (rest.Count == 1 && block != null)
            {
                iter = true;
            }
            else if (rest.Count == 2)
            {
                repl = String.RStringValue(rest[1], caller);
                if (repl.Tainted)
                    tainted = true;
            }
            else
            {
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "wrong number of arguments ({0} for 2)", rest.Count)).raise(caller);
            }

            //pat = (Regexp)rest[0];
            pat = String.get_pat(rest[0], true, caller);

            if (pat.rb_reg_search(str, 0, false, caller) >= 0)
            {
                match = Regexp.rb_backref_get(caller);
                int regs = match.value.Groups.Count;
                if (iter)
                {
                    match.busy = true;
                    repl = String.ObjectAsString(Proc.rb_yield(block, caller, new object[] { Regexp.rb_reg_nth_match(0, match) }), caller);
                    String.str_frozen_check(caller, str);
                    Regexp.rb_backref_set(match, caller);
                }

                str.value = Regexp.rb_reg_regsub(repl, str, match).value;

                if (tainted)
                    str.Tainted = true;

                return str;
            }
            return null;
        }
    }

    internal class rb_str_gsub_bang : VarArgMethodBody0 //author: cjs, status: done
    {
        internal static rb_str_gsub_bang singleton = new rb_str_gsub_bang();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return String.str_gsub((String)recv, caller, block, rest, true);
        }
    }

    internal class rb_str_chomp_bang : VarArgMethodBody0 //author: cjs, status: done
    {
        internal static rb_str_chomp_bang singleton = new rb_str_chomp_bang();

        private static String smart_chomp(String str, int len)
        {
            if (str.value[len - 1] == '\n')
            {
                str.value = str.value.Remove(--len);
                if (str.value.Length > 0 &&
                    str.value[str.value.Length - 1] == '\r')
                {
                    str.value = str.value.Remove(--len);
                }
            }
            else if (str.value[len - 1] == '\r')
            {
                str.value = str.value.Remove(--len);
            }
            else
            {
                return null;
            }
            return str;
        }

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            String str = (String)recv;

            String rs;
            char newline;
            string p;
            int len, rslen;

            if (Class.rb_scan_args(caller, argv, 0, 1, false) == 0)
            {
                len = str.value.Length;
                if (len == 0)
                    return null;
                p = str.value;
                rs = (String)IO.rb_rs.value;
                if (rs.value == IO.rb_default_rs.value)
                {
                    return smart_chomp(str, len);
                }
            }
            if (argv[0] == null)
                return null;
            rs = String.RStringValue(argv[0], caller);
            len = str.value.Length;
            if (len == 0)
                return null;
            p = str.value;
            rslen = rs.value.Length;
            if (rslen == 0)
            {
                while (len > 0 && p[len - 1] == '\n')
                {
                    len--;
                    if (len > 0 && p[len - 1] == '\r')
                        len--;
                }
                if (len < str.value.Length)
                {
                    str.value.Remove(len - 1);
                    return str;
                }
                return null;
            }
            if (rslen > len)
                return null;
            newline = rs.value[rslen - 1];
            if (rslen == 1 && newline == '\n')
                return smart_chomp(str, len);

            if (p[len - 1] == newline &&
            (rslen <= 1 || rs.value.CompareTo(p.Substring(len - rslen, rslen)) == 0))
            {
                str.value = str.value.Remove(str.value.Length - rslen);
                return str;
            }
            return null;
        }
    }

    internal class rb_str_tr : MethodBody2 //author: war, status: done
    {
        internal static rb_str_tr singleton = new rb_str_tr();

        public override object Call2(Class last_class, object str, Frame caller, Proc block, object src, object repl)
        {
            string newValue = ((String)str).value;
            str = new String(newValue);
            String.tr_trans(str, caller, block, src, repl, false);
            return str;
        }
    }


    internal class rb_str_tr_s : MethodBody2 //author: war, status: done
    {
        internal static rb_str_tr_s singleton = new rb_str_tr_s();

        public override object Call2(Class last_class, object str, Frame caller, Proc block, object src, object repl)
        {
            string newValue = ((String)str).value;
            str = new String(newValue);
            String.tr_trans(str, caller, block, src, repl, true);
            return str;
        }
    }

    internal class rb_str_delete : VarArgMethodBody0 //author: war, status: done
    {
        internal static rb_str_delete singleton = new rb_str_delete();

        public override object Call(Class last_class, object str, Frame caller, Proc block, Array rest)
        {
            string newValue = ((String)str).value;
            str = new String(newValue);
            rb_str_delete_bang.singleton.Call(last_class, str, caller, block, rest);
            return str;
        }
    }

    internal class rb_str_delete_bang : VarArgMethodBody0 //author: war, status: done
    {
        internal static rb_str_delete_bang singleton = new rb_str_delete_bang();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            int s, send;
            string sString = ((String)recv).value;
            System.Text.StringBuilder t = new System.Text.StringBuilder();
            char[] squeez = new char[256];
            bool modify = false;
            bool init = true;
            int i;

            if (rest.Count < 1)
            {
                throw new ArgumentError("wrong number of aguments").raise(caller);
            }
            for (i = 0; i < rest.Count; i++)
            {
                string sValue;

                sValue = String.StringValue(rest[i], caller);
                String.SetupTable(sValue, squeez, init);
                init = false;
            }
            s = 0;
            if (sString == null || sString.Length == 0)
            {
                return null;
            }
            send = s + sString.Length;
            while (s < send)
            {
                if (squeez[(int)sString[s]] > 0)
                {
                    modify = true;
                }
                else
                {
                    t.Append(sString[s]);
                }
                s++;
            }

            string newString = t.ToString();

            if (modify)
            {
                ((String)recv).value = newString;
                return recv;
            }
            else
            {
                return null;
            }
        }
    }

    internal class rb_str_squeeze : VarArgMethodBody0 //author: war, status: done
    {
        internal static rb_str_squeeze singleton = new rb_str_squeeze();

        public override object Call(Class last_class, object str, Frame caller, Proc block, Array rest)
        {
            string newValue = ((String)str).value;
            str = new String(newValue);
            rb_str_squeeze_bang.singleton.Call(last_class, str, caller, block, rest);
            return str;
        }
    }


    internal class rb_str_count : MethodBody //author: war, status: done
    {
        internal static rb_str_count singleton = new rb_str_count();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            char[] table = new char[256];
            int s, send;
            string sString;
            bool init = true;
            int i;

            if (args.Length < 1)
            {
                throw new ArgumentError("wrong number of arguments").raise(caller);
            }

            for (i = 0; i < args.Length; i++)
            {
                string sValue;

                sValue = String.StringValue(args[i], caller);
                String.SetupTable(sValue, table, init);
                init = false;
            }

            s = 0;
            sString = ((String)recv).value;
            if (sString == null || sString.Length == 0)
            {
                return 0;
            }
            send = s + sString.Length;
            i = 0;
            while (s < send)
            {
                if (table[(int)sString[s++]] > 0)
                {
                    i++;
                }
            }

            return i;
        }
    }


    internal class rb_str_tr_bang : MethodBody2 //author: war, status: done
    {
        internal static rb_str_tr_bang singleton = new rb_str_tr_bang();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object src, object repl)
        {
            return String.tr_trans(recv, caller, block, src, repl, false);
        }
    }


    internal class rb_str_tr_s_bang : MethodBody2 //author: war, status: done
    {
        internal static rb_str_tr_s_bang singleton = new rb_str_tr_s_bang();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object src, object repl)
        {
            return String.tr_trans(recv, caller, block, src, repl, true);
        }
    }


    internal class rb_str_squeeze_bang : VarArgMethodBody0 //author: war, status: done
    {
        internal static rb_str_squeeze_bang singleton = new rb_str_squeeze_bang();

        public override object Call(Class last_class, object str, Frame caller, Proc block, Array rest)
        {
            char[] squeez = new char[256];
            int s, send;
            System.Text.StringBuilder t = new System.Text.StringBuilder();
            string sString;
            int c, save;
            bool modify = false;
            bool init = true;
            int i;

            if (rest.Count == 0)//squeeze any character
            {
                for (i = 0; i < 256; i++)
                {
                    squeez[i] = (char)1;
                }
            }
            else //only squeeze specified characters in the 'squeez' table
            {
                for (i = 0; i < rest.Count; i++)
                {
                    string sValue;

                    sValue = String.StringValue(rest[i], caller);
                    String.SetupTable(sValue, squeez, init);
                    init = false;
                }
            }

            s = 0;
            sString = ((String)str).value;
            if (sString == null || sString.Length == 0)
            {
                return null;
            }
            send = s + sString.Length;
            save = -1;
            while (s < send)
            {
                c = sString[s++];
                if (c != save || squeez[c] == 0)
                {
                    t.Append((char)c);
                    save = c;
                }
            }
            string newString = t.ToString();
            if (newString.Length != sString.Length)
            {
                modify = true;
            }

            if (modify)
            {
                ((String)str).value = newString;
                return str;
            }
            else
            {
                return null;
            }
        }
    }

    internal class rb_str_each_line : VarArgMethodBody0 //author: war, status: done
    {
        internal static rb_str_each_line singleton = new rb_str_each_line();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            object rs = null;
            string rsString;
            int rslen;
            char newline;

            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 0)
            {
                rs = IO.rb_rs.value;
            }
            else
            {
                rs = rest[0];
            }
            if (rs == null) //no record separator 
            {
                Proc.rb_yield(block, caller, recv);
                return recv;
            }

            rsString = String.RStringValue(rs, caller).value;
            rslen = rsString.Length;
            if (rslen == 0)
            {
                newline = '\n';
            }
            else
            {
                newline = rsString[rslen - 1];
            }

            string pStr = ((String)recv).value;
            int s = 0;
            int p = 0;
            string line;
            for (p = 0 + rslen; p < pStr.Length; p++)
            {
                if (rslen == 0 && pStr[p] == '\n')
                {
                    if (++p < pStr.Length && pStr[p] != '\n') continue;
                    while (++p < pStr.Length && pStr[p] == '\n') { }
                }
                if (p > 0 && pStr[p-1] == newline && rslen <= 1 ||
                    string.Compare(rsString, 0, pStr, p - rslen, rslen) == 0)
                {
                    line = pStr.Substring(s, p - s);
                    Proc.rb_yield(block, caller, new String(line));
                    s = p;
                }
            }

            if (!(s > pStr.Length))
            {
                if (p > pStr.Length) p = pStr.Length;
                line = pStr.Substring(s, p - s);
                Proc.rb_yield(block, caller, new String(line));
            }
            
            return recv;               
        }
    }


    internal class rb_str_each_byte : MethodBody0 //author: war, status: done
    {
        internal static rb_str_each_byte singleton = new rb_str_each_byte();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            foreach(char c in ((String)recv).value){
                if (block == null)
                {
                    throw new LocalJumpError("no block given").raise(caller);
                }
                Proc.rb_yield(block, caller, (int)c);
            }

            return recv;
        }
    }

    internal class rb_str_sum : VarArgMethodBody0 //author: war, status: done
    {
        internal static rb_str_sum singleton = new rb_str_sum();

        //Note: The formula in the documentation is incorrect. This uses the same algorithm as 1.8.2
        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            int bits;
            string str = (string)((String)recv).value;

            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 0)
            {
                bits = 16;
            }
            else
            {
                bits = Numeric.rb_num2long(rest.value[0], caller);
            }

            object sum = 0;
            foreach (char ch in str.ToCharArray())
            {
                sum = Eval.CallPrivate(sum, caller, "+", null, (int)ch);
            }

            if (bits != 0)
            {
                object mod = Eval.CallPrivate(2, caller, "**", null, bits);
                mod = Eval.CallPrivate(mod, caller, "-", null, 1);
                sum = Eval.CallPrivate(sum, caller, "&", null, mod);
            }
            return sum;
        }
    }

    internal class rb_str_times : MethodBody1 //status: done
    {
        internal static rb_str_times singleton = new rb_str_times();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            string value = ((String)recv).value;
            int times = Numeric.rb_num2long(param0,caller);
            System.Text.StringBuilder sb = new System.Text.StringBuilder(value.Length * times);

            for (int i = 0; i < times; i++)
                sb.Append(value);

            return new String(sb.ToString());
        }
    }

    internal class rb_str_aref_m : VarArgMethodBody0 //author: cjs, status: done
    {
        internal static rb_str_aref_m singleton = new rb_str_aref_m();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            //static VALUE
            //rb_str_aref_m(argc, argv, str)
            //int argc;
            //VALUE *argv;
            //VALUE str;
            //{
            //    if (argc == 2) {
            //        if (TYPE(argv[0]) == T_REGEXP) {
            //            return rb_str_subpat(str, argv[0], NUM2INT(argv[1]));
            //        }
            //        return rb_str_substr(str, NUM2LONG(argv[0]), NUM2LONG(argv[1]));
            //    }
            //    if (argc != 1) {
            //        rb_raise(rb_eArgError, "wrong number of arguments (%d for 1)", argc);
            //    }
            //    return rb_str_aref(str, argv[0]);
            //}
            
            String str = (String)recv;
            int argc = argv.Count;
            if (argc == 2)
            {
                if (argv[0] is Regexp)
                {
                    return String.rb_str_subpat(str, (Regexp)argv[0], Numeric.rb_num2long(argv[1], caller), caller);
                }
                int beg = Numeric.rb_num2long(argv[0], caller);
                int len = Numeric.rb_num2long(argv[1], caller);
                if (beg < 0)
                    beg = str.value.Length + beg;
                len = System.Math.Min(len, str.value.Length - beg);
                return new String(str.value.Substring(beg, len));
            }
            if (argc != 1)
            {
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "wrong number of arguments ({0} for 1)", argc)).raise(caller);
            }
            return String.rb_str_aref(caller, str, argv[0]);
        }
    }


    internal class rb_str_insert : MethodBody2 //author: war, status: done
    {
        internal static rb_str_insert singleton = new rb_str_insert();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object param0, object param1)
        {
            int pos = Numeric.rb_num2long(param0, caller);
            string str = ((String)recv).value;
            string str2 = String.StringValue(param1, caller);

            if (pos == -1)
                pos = str2.Length;
            else if (pos < 0)
                pos = str.Length + pos;

            if (pos < 0 || pos > str.Length)
                throw new IndexError(string.Format(CultureInfo.InvariantCulture, "index {0} out of string", pos)).raise(caller);

            System.Text.StringBuilder sb = new System.Text.StringBuilder(str, str.Length + str2.Length);
            sb.Insert(pos, str2);

            return sb.ToString();
        }
    }


    internal class rb_str_length : MethodBody0 //status: done
    {
        internal static rb_str_length singleton = new rb_str_length();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((String)recv).value.Length;
        }
    }


    internal class rb_str_empty : MethodBody0 //status: done
    {
        internal static rb_str_empty singleton = new rb_str_empty();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((String)recv).value.Length == 0;
        }
    }


    internal class rb_str_chop : MethodBody0 //status: done
    {
        internal static rb_str_chop singleton = new rb_str_chop();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            string str = ((String)recv).value;

            if (str.Length >= 2 && 0 == string.Compare(str, str.Length - 2, "\r\n", 0, 2))
            {
                return new String(str.Substring(0, str.Length - 2));
            }

            return new String(str.Substring(0, str.Length - 1));
        }
    }


    internal class rb_str_chop_bang : MethodBody0 // author:cjs, status: done
    {
        internal static rb_str_chop_bang singleton = new rb_str_chop_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            String str = (String)recv;

            if (!string.IsNullOrEmpty(str.value))
            {
                //rb_str_modify(str);
                if (str.value.EndsWith("\r\n"))
                    str.value = str.value.Remove(str.value.Length - 2);
                else
                    str.value = str.value.Remove(str.value.Length - 1);

                return str;
            }

            return null;
        }
    }


    internal class rb_str_upcase : MethodBody0 //status: done
    {
        internal static rb_str_upcase singleton = new rb_str_upcase();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String(((String)recv).value.ToUpperInvariant());
        }
    }


    internal class rb_str_upcase_bang : MethodBody0 //status: done
    {
        internal static rb_str_upcase_bang singleton = new rb_str_upcase_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ((String)recv).value = ((String)recv).value.ToUpperInvariant();
            return recv;
        }
    }


    internal class rb_str_downcase : MethodBody0 //status: done
    {
        internal static rb_str_downcase singleton = new rb_str_downcase();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String(((String)recv).value.ToLowerInvariant());
        }
    }


    internal class rb_str_downcase_bang : MethodBody0 //status: done
    {
        internal static rb_str_downcase_bang singleton = new rb_str_downcase_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ((String)recv).value = ((String)recv).value.ToLowerInvariant();
            return recv;
        }
    }


    internal class rb_str_to_s : MethodBody0 //status: done
    {
        internal static rb_str_to_s singleton = new rb_str_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return recv;
        }
    }


    internal class rb_str_inspect : MethodBody0 // author: cjs/war, status: done
    {
        internal static rb_str_inspect singleton = new rb_str_inspect();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            string value = ((String)recv).value;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.Append('"');
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '"' || c == '\\' || (c == '#' && (i + 1 < value.Length) && (value[i+1] == '$' || value[i+1] == '@' || value[i+1] == '{')))
                {
                    sb.Append('\\');
                    sb.Append(c);
                }
                else if (c < 256 && String.ISPRINT((byte)c)) // ISPRINT()
                {
                    sb.Append(c);
                }
                else
                    switch (c)
                    {
                        case '\n':
                            sb.Append(@"\n");
                            break;
                        case '\r':
                            sb.Append(@"\r");
                            break;
                        case '\t':
                            sb.Append(@"\t");
                            break;
                        case '\f':
                            sb.Append(@"\f");
                            break;
                        case '\v':
                            sb.Append(@"\v");
                            break;
                        case '\a':
                            sb.Append(@"\a");
                            break;
                        case (char)33:
                            sb.Append(@"e");
                            break;
                        default:
                            sb.Append("\\"+String.DecimalToBase(c & 0xFF, 8, 3));
                            break;
                    }
            }
            sb.Append('"');
            
            String result = new String(sb.ToString());
            Object.obj_infect(result, recv);
            return result;
        }
    }


    internal class rb_str_reverse : MethodBody0 //status: done
    {
        internal static rb_str_reverse singleton = new rb_str_reverse();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            char[] chars = ((String)recv).value.ToCharArray();
            System.Array.Reverse(chars);
            return new String(new string(chars));
        }
    }


    internal class rb_str_reverse_bang : MethodBody0 //status: done
    {
        internal static rb_str_reverse_bang singleton = new rb_str_reverse_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            String str = (String)recv;
            char[] chars = str.value.ToCharArray();
            System.Array.Reverse(chars);
            str.value = new string(chars);
            return str;
        }
    }


    internal class rb_str_chomp : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_str_chomp singleton = new rb_str_chomp();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            String str = ((String)recv).rb_str_dup(caller);
            rb_str_chomp_bang.singleton.Call(last_class, str, caller, block, rest);
            return str;
        }
    }


    internal class rb_str_center : VarArgMethodBody1 //status: done
    {
        internal static rb_str_center singleton = new rb_str_center();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object length, Array rest)
        {
            if (rest.Count == 0)
            {
                string str = ((String)recv).value;
                int width = Numeric.rb_num2long(length, caller);
                if (width > str.Length)
                {
                    int left = (width - str.Length) / 2;
                    int right = left + (width - str.Length) % 2;
                    char[] c = new char[width];

                    for (int i = 0; i < left; i++)
                        c[i] = ' ';

                    for (int i = 0; i < str.Length; i++)
                        c[i + left] = str[i];

                    for (int i = 0; i < right; i++)
                        c[i + left + str.Length] = ' ';

                    return new String(new string(c));
                }
                else
                {
                    return recv;
                }
            }
            else if (rest.Count == 1)
            {
                object padding = rest.value[0];
                string str = ((String)recv).value;
                int width = Numeric.rb_num2long(length, caller);
                string pad = String.StringValue(padding, caller);

                if (width > str.Length)
                {
                    int left = (width - str.Length) / 2;
                    int right = left + (width - str.Length) % 2;
                    char[] c = new char[width];

                    for (int i = 0; i < left; i++)
                        c[i] = pad[i % pad.Length];

                    str.CopyTo(0, c, left, str.Length);

                    for (int i = 0; i < right; i++)
                        c[i + left + str.Length] = pad[i % pad.Length];

                    return new String(new string(c));
                }
                else
                {
                    return recv;
                }
            }
            else
            {
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "wrong number of arguments ({0} for 2)", rest.Count)).raise(caller);
            }
        }
    }


    internal class rb_str_ljust : VarArgMethodBody1 //status: done
    {
        internal static rb_str_ljust singleton = new rb_str_ljust();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest)
        {
            if (rest.Count == 0)
            {
                string str = ((String)recv).value;
                int width = Numeric.rb_num2long(p1, caller);
                if (width > str.Length)
                    return new String(str.PadRight(width));

                return recv;
            }
            else if (rest.Count == 1)
            {
                string str = ((String)recv).value;
                int width = Numeric.rb_num2long(p1, caller);
                object p2 = rest.value[0];
                string pad = String.StringValue(p2, caller);

                if (width > str.Length)
                {
                    char[] c = new char[width];
                    str.CopyTo(0, c, 0, str.Length);
                    for (int i = 0; i < width - str.Length; i++)
                    {
                        c[str.Length + i] = pad[i % pad.Length];
                    }

                    return new String(new string(c));
                }

                return recv;
            }
            else
            {
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "wrong number of arguments ({0} for 2)", rest.Count)).raise(caller);
            }
        }
    }


    internal class rb_str_rjust : VarArgMethodBody1 //status: done
    {
        internal static rb_str_rjust singleton = new rb_str_rjust();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest)
        {
            if (rest.Count == 0)
            {
                string str = ((String)recv).value;
                int width = Numeric.rb_num2long(p1, caller);
                if (width > str.Length)
                    return new String(str.PadLeft(width));

                return recv;
            }
            else if (rest.Count == 1)
            {
                string str = ((String)recv).value;
                int width = Numeric.rb_num2long(p1, caller);
                object p2 = rest.value[0];
                string pad = String.StringValue(p2, caller);

                if (width > str.Length)
                {
                    char[] c = new char[width];
                    str.CopyTo(0, c, width - str.Length, str.Length);

                    for (int i = 0; i < width - str.Length; i++)
                    {
                        c[i] = pad[i % pad.Length];
                    }

                    return new String(new string(c));
                }

                return recv;
            }
            else
            {
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "wrong number of arguments ({0} for 2)", rest.Count)).raise(caller);
            }
        }
    }


    internal class rb_str_concat : MethodBody1 //status: done
    {
        internal static rb_str_concat singleton = new rb_str_concat();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                int i = (int)param0;

                if (i >= 0 && i <= char.MaxValue)
                {
                    ((String)recv).value += (char)i;
                    return recv;
                }
            }

            ((String)recv).value += String.StringValue(param0, caller);

            return recv;
        }
    }

    internal class rb_str_capitalize : MethodBody0 //author: war, status: done
    {
        internal static rb_str_capitalize singleton = new rb_str_capitalize();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            String str = ((String)recv).rb_str_dup(caller);
            Ruby.Methods.rb_str_capitalize_bang.singleton.Call0(last_class, str, caller, block);            
            return str;
        }
    }


    internal class rb_str_capitalize_bang : MethodBody0 //author: war, status: done
    {
        internal static rb_str_capitalize_bang singleton = new rb_str_capitalize_bang();

        //TODO: move these constants to where they belong
        internal static bool ismbchar(char c)
        {
            return mbctab_ascii[c] > 0;
        }

        internal static byte mbclen(char c)
        {
            return (byte)(mbctab_ascii[c] + 1);
        }

        internal static byte[] mbctab_ascii = {
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            };


        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            int s, send;
            bool modify = false;

            
            //rb_str_modify(str);
            if (((String)recv).value == null || ((String)recv).value.Length == 0) return null;
            System.Text.StringBuilder str = new System.Text.StringBuilder(((String)recv).value);
            s = 0;
            send = str.Length;
            if (char.IsLower(str[s]))
            {
                str[s] = char.ToUpperInvariant(str[s]);
                modify = true;
            }
            while (++s < send)
            {
                if (ismbchar(str[s]))
                {
                    str[s] = (char)(str[s] + mbclen(str[s]) - 1);
                }
                else if(char.IsUpper(str[s])){
                    str[s] = char.ToLowerInvariant(str[s]);
                    modify = true;
                }
            }

            if (modify)
            {
                ((String)recv).value = str.ToString();
                return recv;
            }
            return null;
        }
    }


    internal class rb_str_swapcase : MethodBody0 //status: done
    {
        internal static rb_str_swapcase singleton = new rb_str_swapcase();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            char[] c = ((String)recv).value.ToCharArray();
            for (int i = 0; i < c.Length; i++)
                c[i] = (char.IsUpper(c[i])) ? char.ToLowerInvariant(c[i]) : char.ToUpperInvariant(c[i]);

            return new String(new string(c));
        }
    }


    internal class rb_str_swapcase_bang : MethodBody0 //status: done
    {
        internal static rb_str_swapcase_bang singleton = new rb_str_swapcase_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            String str = (String)recv;
            char[] c = str.value.ToCharArray();
            for (int i = 0; i < c.Length; i++)
                c[i] = (char.IsUpper(c[i])) ? char.ToLowerInvariant(c[i]) : char.ToUpperInvariant(c[i]);

            str.value = new string(c);
            return str;
        }
    }


    internal class rb_str_strip : MethodBody0 //author: war, status: done
    {
        internal static rb_str_strip singleton = new rb_str_strip();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            String str = ((String)recv).rb_str_dup(caller);
            rb_str_strip_bang.singleton.Call0(last_class, str, caller, block);
            return str;
        }
    }


    internal class rb_str_strip_bang : MethodBody0 //author: war, status: done
    {
        internal static rb_str_strip_bang singleton = new rb_str_strip_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            object l = rb_str_lstrip_bang.singleton.Call0(last_class, recv, caller, block);
            object r = rb_str_rstrip_bang.singleton.Call0(last_class, recv, caller, block);

            if (l == null && r == null)
            {
                return null; 
            }
            return recv;
        }
    }
        
    internal class rb_str_lstrip : MethodBody0 //author: war, status: done
    {
        internal static rb_str_lstrip singleton = new rb_str_lstrip();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            String str = ((String)recv).rb_str_dup(caller);
            rb_str_lstrip_bang.singleton.Call0(last_class, str, caller, block);
            return str;
        }
    }

    internal class rb_str_lstrip_bang : MethodBody0 //author: war, status: done
    {
        internal static rb_str_lstrip_bang singleton = new rb_str_lstrip_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            String str = (String)recv; 
            if (str.value == null || str.value.Length == 0 || (str.value.Length > 0 && !System.Char.IsWhiteSpace(str.value[0])))
            {
                return null;
            }
            else
            {
                str.value = str.value.TrimStart();
                return str;
            }
        }
    }

    internal class rb_str_rstrip : MethodBody0 //author: war, status: done
    {
        internal static rb_str_rstrip singleton = new rb_str_rstrip();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            String str = ((String)recv).rb_str_dup(caller);
            rb_str_rstrip_bang.singleton.Call0(last_class, str, caller, block);
            return str;
        }
    }

    internal class rb_str_rstrip_bang : MethodBody0 //author: war, status: done
    {
        internal static rb_str_rstrip_bang singleton = new rb_str_rstrip_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {

            String str = (String)recv;
            if (str.value == null || str.value.Length == 0 || (str.value.Length > 0 && !System.Char.IsWhiteSpace(str.value[str.value.Length - 1])))
            {
                return null;
            }
            else
            {
                str.value = str.value.TrimEnd();
                return str;
            }
        }
    }

    internal class rb_str_cmp_m : MethodBody1 // author:cjs, status: done
    {
        internal static rb_str_cmp_m singleton = new rb_str_cmp_m();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return String.Cmp(caller, recv, param0);
        }
    }


    internal class rb_str_casecmp : MethodBody1 //status: done
    {
        internal static rb_str_casecmp singleton = new rb_str_casecmp();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
//            static VALUE
//rb_str_casecmp(str1, str2)
//    VALUE str1, str2;
//{
//    long len;
//    int retval;

//    StringValue(str2);
//    len = lesser(RSTRING(str1)->len, RSTRING(str2)->len);
//    retval = rb_memcicmp(RSTRING(str1)->ptr, RSTRING(str2)->ptr, len);
//    if (retval == 0) {
//    if (RSTRING(str1)->len == RSTRING(str2)->len) return INT2FIX(0);
//    if (RSTRING(str1)->len > RSTRING(str2)->len) return INT2FIX(1);
//    return INT2FIX(-1);
//    }
//    if (retval == 0) return INT2FIX(0);
//    if (retval > 0) return INT2FIX(1);
//    return INT2FIX(-1);
//}


            string val = String.StringValue(param0, caller);
            return System.String.Compare(((String)recv).value, val, System.StringComparison.InvariantCultureIgnoreCase);
        }
    }


    internal class rb_str_equal : MethodBody1 //status: done
    {
        internal static rb_str_equal singleton = new rb_str_equal();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (object.ReferenceEquals(recv, param0))
            {
                return true;
            }
            else if (!(param0 is String))
            {
                if (!Eval.RespondTo(param0, "to_str"))
                    return false;
                else
                    return Object.Equal(param0, recv,caller);
            }
            else
            {
                if (((String)recv).value.Length == ((String)param0).value.Length)
                    return 0 == (int)rb_str_cmp_m.singleton.Call1(last_class, recv, caller, null, param0);
                else
                    return false;
            }
        }
    }


    internal class rb_str_eql : MethodBody1 //status: done
    {
        internal static rb_str_eql singleton = new rb_str_eql();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is String)
            {
                return ((String)recv).value == ((String)param0).value;
            }

            return false;
        }
    }


    internal class rb_str_hash_m : MethodBody0 //status: done
    {
        internal static rb_str_hash_m singleton = new rb_str_hash_m();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((String)recv).value.GetHashCode();
        }
    }

/*
 *  call-seq:
 *     str =~ obj   => fixnum or nil
 *  
 *  Match---If <i>obj</i> is a <code>Regexp</code>, use it as a pattern to match
 *  against <i>str</i>. If <i>obj</i> is a <code>String</code>, look for it in
 *  <i>str</i> (similar to <code>String#index</code>). Returns the position the
 *  match starts, or <code>nil</code> if there is no match. Otherwise, invokes
 *  <i>obj.=~</i>, passing <i>str</i> as an argument. The default
 *  <code>=~</code> in <code>Object</code> returns <code>false</code>.
 *     
 *     "cat o' 9 tails" =~ '\d'   #=> nil
 *     "cat o' 9 tails" =~ /\d/   #=> 7
 *     "cat o' 9 tails" =~ 9      #=> false
 */

    internal class rb_str_match : MethodBody1 //status: done
    {
        internal static rb_str_match singleton = new rb_str_match();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is String)
            {
                throw new TypeError("type mismatch: String given").raise(caller);
            }
            else if (param0 is Regexp)
            {
                return rb_reg_match.singleton.Call1(last_class, param0, caller, null, recv);
            }
            else
            {
                return Eval.CallPrivate(param0, caller, "=~", null, recv);
            }
        }
    }


    internal class rb_str_match_m : MethodBody1 //status: done
    {
        internal static rb_str_match_m singleton = new rb_str_match_m();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return Eval.CallPrivate(String.get_pat(param0, false, caller), caller, "match", null, recv);
        }
    }


    internal class rb_str_succ : MethodBody0 //status: done
    {
        internal static rb_str_succ singleton = new rb_str_succ();

        internal static string Helper(string str)
        {
            System.Text.StringBuilder c = new System.Text.StringBuilder();

            int i = str.Length - 1;
            for (; i >= 0; i--)
                if (char.IsLetterOrDigit(str, i))
                    break;

            if (i == -1)
            {
                int j = str.Length - 1;
                for (; j >= 0; j--)
                {
                    char chr = str[j];
                    if (chr == char.MaxValue)
                    {
                        c.Insert(0, (char)0);
                    }
                    else
                    {
                        c.Insert(0, (char)(chr + 1));
                        break;
                    }
                }

                if (j >= 0)
                    c.Insert(0, str.Substring(0, j));
            }
            else
            {
                c.Append(str, i + 1, str.Length - i - 1);

                char? carry = null;
                for (; i >= 0; i--)
                {
                    char chr = str[i];
                    if (char.IsLetterOrDigit(chr))
                    {
                        if (chr == 'Z')
                        {
                            c.Insert(0, 'A');
                            carry = 'A';
                        }
                        else if (chr == 'z')
                        {
                            c.Insert(0, 'a');
                            carry = 'a';
                        }
                        else if (chr == '9')
                        {
                            c.Insert(0, '0');
                            carry = '1';
                        }
                        else
                        {
                            c.Insert(0, (char)(chr + 1));
                            carry = null;
                            break;
                        }
                    }
                    else if (carry != null)
                    {
                        c.Insert(0, carry.Value);
                        carry = null;
                    }
                }

                if (carry != null)
                    c.Insert(0, carry.Value);

                if (i >= 0)
                    c.Insert(0, str.Substring(0, i));
            }

            return c.ToString();
        }

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String(rb_str_succ.Helper(((String)recv).value));
        }
    }


    internal class rb_str_succ_bang : MethodBody0 //status: done
    {
        internal static rb_str_succ_bang singleton = new rb_str_succ_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            String str = (String)recv;
            str.value = rb_str_succ.Helper(str.value);
            return str;
        }
    }


    internal class rb_f_sub : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_sub singleton = new rb_f_sub();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            String str = String.uscore_get(caller);

            if (rb_str_sub_bang.singleton.Call(last_class, str, caller, block, rest) == null)
                return str;

            Eval.rb_lastline_set(caller, str);
            return str;
        }
    }

    internal class rb_f_gsub : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_gsub singleton = new rb_f_gsub();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            String str = String.uscore_get(caller);

            if (rb_str_gsub_bang.singleton.Call(last_class, str, caller, block, rest) == null)
                return str;

            Eval.rb_lastline_set(caller, str);
            return str;
        }
    }


    internal class rb_f_sub_bang : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_sub_bang singleton = new rb_f_sub_bang();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return rb_str_sub_bang.singleton.Call(last_class, String.uscore_get(caller), caller, block, rest);
        }
    }


    internal class rb_f_gsub_bang : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_gsub_bang singleton = new rb_f_gsub_bang();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return rb_str_gsub_bang.singleton.Call(last_class, String.uscore_get(caller), caller, block, rest);
        }
    }


    internal class rb_f_chop : MethodBody0 // author: cjs, status: done
    {
        internal static rb_f_chop singleton = new rb_f_chop();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            String str = String.uscore_get(caller);

            if (str.value.Length > 0)
            {
                str = str.rb_str_dup(caller);
                rb_str_chop_bang.singleton.Call0(last_class, str, caller, block);
                Eval.rb_lastline_set(caller, str);
            }
            return str;
        }
    }


    internal class rb_f_chop_bang : MethodBody0 // author: cjs, status: done
    {
        internal static rb_f_chop_bang singleton = new rb_f_chop_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return rb_str_chop_bang.singleton.Call0(last_class, String.uscore_get(caller), caller, block);
        }
    }


    internal class rb_f_chomp : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_chomp singleton = new rb_f_chomp();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            String str = String.uscore_get(caller);
            String dup = str.rb_str_dup(caller);

            if (rb_str_chomp_bang.singleton.Call(last_class, dup, caller, block, rest) == null)
                return str;
            Eval.rb_lastline_set(caller, dup);
            return dup;
        }
    }


    internal class rb_f_chomp_bang : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_chomp_bang singleton = new rb_f_chomp_bang();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return rb_str_chomp_bang.singleton.Call(last_class, String.uscore_get(caller), caller, block, rest);
        }
    }


    internal class rb_f_split : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_split singleton = new rb_f_split();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return rb_str_split_m.singleton.Call(last_class, String.uscore_get(caller), caller, block, rest);
        }
    }


    internal class rb_f_scan : MethodBody1 // author: cjs, status: done
    {
        internal static rb_f_scan singleton = new rb_f_scan();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return rb_str_scan.singleton.Call1(last_class, String.uscore_get(caller), caller, block, p1);
        }

    }
}
