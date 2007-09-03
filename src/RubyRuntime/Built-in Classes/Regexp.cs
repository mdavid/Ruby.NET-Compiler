/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using System;
using System.Globalization;

namespace Ruby
{


    [UsedByRubyCompiler]
    public partial class Regexp : Basic
    {
        internal enum RE_OPTION { IGNORECASE = 1, EXTENDED = 2, MULTILINE = 4, SINGLELINE = 8 }
        internal enum KCODE { NONE = 0, FIXED = 16, EUC = 32, SJIS = 48, UTF8 = 64, MASK = EUC + SJIS + UTF8 }

        internal System.Text.RegularExpressions.Regex value = null;

        // --------------------------------------------------------------------------

        internal static global_variable match_glob = new match_global();
        internal static global_variable last_match_glob = new last_match_global();
        internal static global_variable prematch = new prematch_global();
        internal static global_variable postmatch = new postmatch_global();
        internal static global_variable last_paren_match = new last_paren_match_global();
        internal static global_variable ignorecase = new ignorecase_global();
        internal static global_variable kcode_glob = new kcode_global();


        // --------------------------------------------------------------------------

        internal const int MBCTYPE_ASCII = 0;


        // -------------------------------------------------------------------------------

        internal Regexp()
            : base(Ruby.Runtime.Init.rb_cRegexp) //status: done
        {
        }

        public Regexp(Class klass)
            : base(klass) //status: done
        {
        }

        [UsedByRubyCompiler]
        public Regexp(string pattern, int options)
            : this()
        {
            Init(pattern, options);
        }

        // -------------------------------------------------------------------------------

        internal string pattern;
        internal int _options;

        internal void Init(string pattern, int options)
        {
            this.pattern = pattern;
            this._options = options;

            if (string.IsNullOrEmpty(pattern))
                pattern = ".";

            this.value = new System.Text.RegularExpressions.Regex(pattern, IntToOptions(options));
        }

        internal override object Inner()
        {
            return value;
        }

        // -------------------------------------------------------------------------------

        internal static System.Text.RegularExpressions.RegexOptions IntToOptions(int options) //status: done
        {
            // .NET and ruby mappings for SINGLE- and MULTI-LINE are different

            System.Text.RegularExpressions.RegexOptions regexOptions = System.Text.RegularExpressions.RegexOptions.Multiline;

            /* match will be done case insensetively */
            if ((options & (int)RE_OPTION.IGNORECASE) != 0)
                regexOptions |= System.Text.RegularExpressions.RegexOptions.IgnoreCase;

            /* ^ and $ ignore newline */
            if ((options & (int)RE_OPTION.SINGLELINE) != 0)
                regexOptions &= ~System.Text.RegularExpressions.RegexOptions.Multiline;

            /* newline will be included for . */
            if ((options & (int)RE_OPTION.MULTILINE) != 0)
                regexOptions |= System.Text.RegularExpressions.RegexOptions.Singleline;

            /* perl-style extended pattern available */
            if ((options & (int)RE_OPTION.EXTENDED) != 0)
                regexOptions |= System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace;

            // this implementation never has a kanji code
            //if ((options & (int)RE_OPTION.intEST) != 0)
            //    regexOptions |= System.Text.RegularExpressions.RegexOptions.None;

            return regexOptions;
        }

        internal static int OptionsToInt(System.Text.RegularExpressions.RegexOptions options)
        {
            int flags = 0;
            if (InOptions(options, RE_OPTION.IGNORECASE))
                flags |= (int)RE_OPTION.IGNORECASE;
            if (InOptions(options, RE_OPTION.EXTENDED))
                flags |= (int)RE_OPTION.EXTENDED;
            if (InOptions(options, RE_OPTION.MULTILINE))
                flags |= (int)RE_OPTION.MULTILINE;
            if (InOptions(options, RE_OPTION.SINGLELINE))
                flags |= (int)RE_OPTION.SINGLELINE;
            return flags;
        }

        private static bool InOptions(System.Text.RegularExpressions.RegexOptions opt, RE_OPTION ruby_opt)
        {
            switch (ruby_opt)
            {
                case RE_OPTION.IGNORECASE:
                    return (opt & System.Text.RegularExpressions.RegexOptions.IgnoreCase) > 0;
                case RE_OPTION.EXTENDED:
                    return (opt & System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace) > 0;
                case RE_OPTION.MULTILINE:
                    return (opt & System.Text.RegularExpressions.RegexOptions.Singleline) > 0;
                case RE_OPTION.SINGLELINE:
                    return (~opt & System.Text.RegularExpressions.RegexOptions.Multiline) > 0;
                default:
                    return false;
            }
        }


        // -------------------------------------------------------------------------------

        //internal static System.Reflection.FieldInfo GetLocalVar(Frame caller, string fieldName, out Frame frame)
        //{
        //    System.Reflection.FieldInfo tilde = caller.GetType().GetField(fieldName);
        //    if (tilde != null)
        //    {
        //        frame = caller;
        //        return tilde;
        //    }

        //    if (caller.current_block == null)
        //    {
        //        frame = null;
        //        return null;
        //    }

        //    int depth = 0;
        //    while (true)
        //    {
        //        System.Reflection.FieldInfo localN = caller.current_block.GetType().GetField("locals" + (depth++));
        //        tilde = localN.FieldType.GetField(fieldName);
        //        if (tilde != null)
        //        {
        //            frame = (Frame)localN.GetValue(caller.current_block);
        //            return tilde;
        //        }
        //    }
        //}

        internal static Match rb_backref_get(Frame caller) // status: done
        {
            return caller.Tilde;
        }

        internal static void rb_backref_set(Match val, Frame caller) // status: done
        {
            caller.Tilde = val;
        }

        internal static void rb_reg_check(Frame caller, Regexp re)
        {
            if (re == null || re.value == null)
            {
                throw new TypeError("uninitialized Regexp").raise(caller);
            }
        }

        internal static String rb_reg_desc(Frame caller, Regexp re)
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder("/");

            str.Append(re.pattern);
            str.Append("/");
            if (re != null && re.value != null)
            {
                rb_reg_check(caller, re);
                if (InOptions(re.value.Options, RE_OPTION.MULTILINE))
                    str.Append("m");
                if (InOptions(re.value.Options, RE_OPTION.IGNORECASE))
                    str.Append("i");
                if (InOptions(re.value.Options, RE_OPTION.EXTENDED))
                    str.Append("x");
            }
            String result = new String(str.ToString());
            result.Tainted |= re.Tainted;
            return result;
        }

        internal static String rb_reg_quote(String rstr)
        {
            string str = rstr.value;
            int s = 0;

            for (; s < str.Length; s++)
            {
                char c = str[s];
                if (c > 0xff)
                {
                    int n = c / 16;

                    while ((n-- > 0) && (s < str.Length))
                        s++;
                    s--;
                    continue;
                }
                switch (c)
                {
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                    case '(':
                    case ')':
                    case '|':
                    case '-':
                    case '*':
                    case '.':
                    case '\\':
                    case '?':
                    case '+':
                    case '^':
                    case '$':
                    case ' ':
                    case '#':
                    case '\t':
                    case '\f':
                    case '\n':
                    case '\r':
                        goto meta_found;
                }
            }
            return rstr;

        meta_found:
            System.Text.StringBuilder t = new System.Text.StringBuilder(str.Substring(0, s));

            for (; s < str.Length; s++)
            {
                char c = str[s];
                if (c > 0xff)
                {
                    int n = c / 16;

                    while ((n-- > 0) && (s < str.Length))
                        t.Append(str[s++]);
                    s--;
                    continue;
                }
                switch (c)
                {
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                    case '(':
                    case ')':
                    case '|':
                    case '-':
                    case '*':
                    case '.':
                    case '\\':
                    case '?':
                    case '+':
                    case '^':
                    case '$':
                    case '#':
                        t.Append('\\');
                        break;
                    case ' ':
                        t.Append('\\');
                        t.Append(' ');
                        continue;
                    case '\t':
                        t.Append('\\');
                        t.Append('t');
                        continue;
                    case '\n':
                        t.Append('\\');
                        t.Append('n');
                        continue;
                    case '\r':
                        t.Append('\\');
                        t.Append('r');
                        continue;
                    case '\f':
                        t.Append('\\');
                        t.Append('f');
                        continue;
                }
                t.Append(c);
            }
            String tmp = new String(t.ToString());
            Object.obj_infect(tmp, str);
            return tmp;
        }

        internal static void rb_reg_raise(Frame caller, string err, Regexp re)
        {
            String desc = rb_reg_desc(caller, re);

            throw new RegExpError(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", err, desc.value)).raise(caller);
        }

        internal int rb_reg_search(String str, int pos, bool reverse, Frame caller)
        {
            Regexp re = this;
            int result;
            Match match;
            System.Text.RegularExpressions.Match regs;

            if (pos > str.value.Length || pos < 0)
            {
                rb_backref_set(null, caller);
                return -1;
            }

            rb_reg_check(caller, re);

            string str_for_match = str.value;
            if (str_for_match.EndsWith("\n"))
                str_for_match = str_for_match.Remove(str_for_match.Length - 1);

            if (reverse)
            {
                if (!re.value.RightToLeft)
                    re.value = new System.Text.RegularExpressions.Regex(re.pattern, re.value.Options | System.Text.RegularExpressions.RegexOptions.RightToLeft);
                regs = re.value.Match(str_for_match, 0, pos + 1);
            }
            else
            {
                if (re.value.RightToLeft)
                    re.value = new System.Text.RegularExpressions.Regex(re.pattern, re.value.Options ^ System.Text.RegularExpressions.RegexOptions.RightToLeft);
                regs = re.value.Match(str_for_match, pos, str_for_match.Length - pos);
            }

            if (regs.Success)
                if (re.pattern.Equals(""))
                    result = regs.Index + regs.Length;
                else
                    result = regs.Index;
            else
                result = -1;

            if (result < 0)
            {
                rb_backref_set(null, caller);
                return result;
            }

            match = rb_backref_get(caller);
            if (match == null) // || FL_TEST(match, MATCH_BUSY))
            {
                match = new Match(Ruby.Runtime.Init.rb_cMatch);
            }
            else
            {
                if (Eval.rb_safe_level() >= 3)
                    match.Tainted = true;
                else
                    match.Tainted = false;
            }

            match.value = regs;
            match.matched = str.value;
            rb_backref_set(match, caller);

            Object.obj_infect(match, re.Tainted);
            Object.obj_infect(match, str.Tainted);

            return result;
        }

        internal static String rb_reg_regsub(String repl, String src, Match match)
        {
            String str = new String(src.value);

            // Can't use .NET regular expression replacement as semantics are different to Ruby
            int replsub;
            if ((replsub = repl.value.IndexOf('\\')) >= 0)
            {
                System.Text.StringBuilder replstr = new System.Text.StringBuilder(repl.value.Substring(0, replsub));
                for (; replsub < repl.value.Length; replsub++)
                {
                    if (repl.value[replsub] == '\\' && repl.value.Length > replsub)
                        if (char.IsDigit(repl.value[replsub + 1]))
                        {
                            int i = 2;
                            for (; replsub + i < repl.value.Length; i++)
                                if (!char.IsDigit(repl.value[replsub + i]))
                                    break;
                            int matchnum = int.Parse(repl.value.Substring(replsub + 1, i - 1), CultureInfo.InvariantCulture);
                            if (matchnum < match.value.Groups.Count)
                                replstr.Append(match.value.Groups[matchnum]);
                            replsub += i - 1;
                        }
                        else if (repl.value[replsub + 1] == '&')
                        {
                            replstr.Append(src.value);
                            replsub++;
                        }
                        else
                            replstr.Append(repl.value[replsub]);
                    else
                        replstr.Append(repl.value[replsub]);
                }

                str.value = str.value.Remove(match.value.Index, match.value.Length);
                str.value = str.value.Insert(match.value.Index, replstr.ToString());
            }
            else
            {
                str.value = str.value.Remove(match.value.Index, match.value.Length);
                str.value = str.value.Insert(match.value.Index, repl.value);
            }

            return str;
        }

        internal static bool rb_reg_nth_defined(int nth, Match match)
        {
            if (match == null)
                return false;
            if (nth >= match.value.Groups.Count)
            {
                return false;
            }
            if (nth < 0)
            {
                nth += match.value.Groups.Count;
                if (nth <= 0)
                    return false;
            }
            return (match.value.Groups[nth].Index != -1);
        }

        internal static String rb_reg_nth_match(int nth, Match match)
        {
            String str;
            int start;

            if (match == null)
                return null;
            if (nth >= match.value.Groups.Count)
            {
                return null;
            }
            if (nth < 0)
            {
                nth += match.value.Groups.Count;
                if (nth <= 0) return null;
            }
            start = match.value.Groups[nth].Index;
            if (start == -1 || !match.value.Groups[nth].Success)
                return null;
            str = new String(match.value.Groups[nth].Value);
            str.Tainted |= match.Tainted;
            return str;
        }

        internal static String rb_reg_last_match(Match match)
        {
            return rb_reg_nth_match(0, match);
        }

        internal static int rb_reg_options(Frame caller, Regexp re) //author: war, status: done
        {
            int options;

            rb_reg_check(caller, re);
            options = OptionsToInt(re.value.Options) & ((int)RE_OPTION.IGNORECASE | (int)RE_OPTION.MULTILINE | (int)RE_OPTION.EXTENDED);

            return options;
        }

        internal static String rb_reg_match_last(Match match)
        {
            int i;

            if (match == null)
                return null;
            if (match.value.Groups[0].Index == -1)
                return null;

            for (i = match.value.Groups.Count - 1; match.value.Groups[i].Index == -1 && i > 0; i--)
                ;
            if (i == 0)
                return null;
            return rb_reg_nth_match(i, match);
        }

        internal static void re_mbcinit(int mbctype) // status: done
        {
            // not used
        }

        internal static void rb_set_kcode(string val) // status: done
        {
            // not used
        }


        private class match_global : global_variable // status: done
        {
            internal override object getter(string id, Frame caller)
            {
                // match_getter
                Match match = Regexp.rb_backref_get(caller);

                if (match == null)
                    return null;
                //rb_match_busy(match);
                return match;
            }

            internal override void setter(string id, object value, Frame caller)
            {
                // match_setter
                if (value != null)
                {
                    Object.CheckType<Match>(caller, value);
                }
                Regexp.rb_backref_set((Match)value, caller);
            }
        }


        private class last_match_global : readonly_global // status: done
        {
            internal override object getter(string id, Frame caller)
            {
                // last_match_getter
                return Regexp.rb_reg_last_match(Regexp.rb_backref_get(caller));
            }
        }


        private class prematch_global : global_variable // status: done
        {
            internal override object getter(string id, Frame caller)
            {
                // prematch_getter
                return Ruby.Methods.rb_reg_match_pre.singleton.Call0(null, Regexp.rb_backref_get(caller), null, null);
            }
        }


        private class postmatch_global : global_variable // status: done
        {
            internal override object getter(string id, Frame caller)
            {
                // postmatch_getter
                return Ruby.Methods.rb_reg_match_post.singleton.Call0(null, Regexp.rb_backref_get(caller), null, null);
            }
        }


        private class last_paren_match_global : global_variable // status: done
        {
            internal override object getter(string id, Frame caller)
            {
                // last_paren_match_getter
                return Regexp.rb_reg_match_last(Regexp.rb_backref_get(caller));
            }
        }


        private class ignorecase_global : global_variable // status: done
        {
            internal override object getter(string id, Frame caller)
            {
                //ignorecase_getter
                return (bool)Regexp.ignorecase.value;
            }

            internal override void setter(string id, object val, Frame caller)
            {
                //ignorecase_setter
                Errors.rb_warn(string.Format(CultureInfo.InvariantCulture, "modifying {0} is deprecated", id));
                Regexp.ignorecase.value = Eval.Test(val);
            }
        }

        private class kcode_global : global_variable // status: unimplemented
        {
            internal override object getter(string id, Frame caller)
            {
                //kcode_getter ???
                return new String("NONE");
            }
            internal override void setter(string id, object val, Frame caller)
            {
                //kcode_setter ???
            }
        }
    }




    public class RegExpError : StandardError //status: done
    {
        public RegExpError(string message) : this(message, Ruby.Runtime.Init.rb_eRegexpError) { }

        public RegExpError(string message, Class klass) : base(message, klass) { }

        public RegExpError(Class klass) : base(klass) { }
    }
}

