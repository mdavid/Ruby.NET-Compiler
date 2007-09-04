/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using System.Collections;
using System.Globalization;

namespace Ruby
{
    [UsedByRubyCompiler]
    public partial class String : Basic
    {
        [UsedByRubyCompiler]
        public string value;


        //-----------------------------------------------------------------

        internal static global_variable rb_fs = new global_variable(); // default separator pattern used by split


        //-----------------------------------------------------------------

        public String(Class klass)
            : base(klass) //status: done
        {
            this.value = "";
        }

        internal String()
            : base(Ruby.Runtime.Init.rb_cString) //status: done
        {
            this.value = "";
        }

        [UsedByRubyCompiler]
        public String(string value)
            : base(Ruby.Runtime.Init.rb_cString) //status: done
        {
            this.value = value;
        }


        //-----------------------------------------------------------------


        internal override object Inner()
        {
            return value;
        }

        [UsedByRubyCompiler]
        public override string ToString()
        {
            return value;
        }


        [UsedByRubyCompiler]
        public String Concat(String str)
        {
            // Used internally by compiler
            // Fixme: can we be more efficient?
            return new String(this.value + str);
        }

        internal static int Compare(Frame caller, String x, String y)
        {
            string s1 = String.StringValue(x, caller);
            string s2 = String.StringValue(y, caller);

            for (int i = 0; i < s1.Length && i < s2.Length; i++)
            {
                if (s1[i] > s2[i])
                    return 1;
                else if (s1[i] < s2[i])
                    return -1;
            }

            if (s1.Length == s2.Length)
                return 0;
            else if (s1.Length > s2.Length)
                return 1;
            else
                return -1;
        }

        internal static object Cmp(Frame caller, object x, object y)
        {
            if (x is String && y is String)
            {
                return Compare(caller, (String)x, (String)y);
            }
            else
            {
                if (!Eval.RespondTo(y, "to_str"))
                    return false;
                else if (!Eval.RespondTo(y, "<=>"))
                    return null;
                else
                {
                    object tmp = Eval.CallPrivate(y, caller, "<=>", null, x);

                    if (tmp == null)
                        return null;
                    else if (tmp is int)
                        return -(int)tmp;
                    else
                        return -Numeric.rb_num2long(tmp, caller);
                }
            }
        }

        //-----------------------------------------------------------------

        internal class Comparer : IComparer // author: cjs
        {
            private Frame caller;

            internal Comparer(Frame caller)
            {
                this.caller = caller;
            }

            #region IComparer Members

            public int Compare(object x, object y)
            {
                object result = String.Cmp(caller, x, y);

                if (result is int)
                    return (int)result;
                else
                    return -1;
            }

            #endregion
        }

        internal class CaseInsensitiveComparer : IComparer
        {
            private Frame caller;

            internal CaseInsensitiveComparer(Frame caller)
            {
                this.caller = caller;
            }

            #region IComparer Members

            public int Compare(object x, object y)
            {
                if (x is String && y is String)
                {
                    x = new String(((String)x).value.ToUpperInvariant());
                    y = new String(((String)y).value.ToUpperInvariant());
                }

                object result = String.Cmp(caller, x, y);

                if (result is int)
                    return (int)result;
                else
                    return -1;
            }

            #endregion
        }

        //-----------------------------------------------------------------

        internal static string StringValue(object obj, Frame caller)
        {
            //WARNING - No Tainted Info retained in this code. 
            if (obj is String)
            {
                return ((String)obj).value;
            }
            else if (obj is string)
            {
                return (string)obj;
            }
            else
            {
                return Object.Convert<String>(obj, "to_str", caller).value;
            }
        }

        internal static String RStringValue(object obj, Frame caller)
        {
            if (obj is String)
            {
                return (String)obj;
            }
            else if (obj is string)
            {
                return new String((string)obj);
            }
            else
            {
                return Object.Convert<String>(obj, "to_str", caller);
            }
        }

        internal static string SafeStringValue(object obj, Frame caller)
        {
            return RSafeStringValue(obj, caller).value;
        }

        internal static String RSafeStringValue(object obj, Frame caller)
        {
            String x = RStringValue(obj, caller);
            Eval.rb_check_safe_obj(caller, x);
            return x;
        }

        internal static bool TryStringValue(object obj, out string value, Frame caller)
        {
            if (obj is String)
            {
                value = ((String)obj).value;
                return true;
            }
            else if (obj is string)
            {
                value = (string)obj;
                return true;
            }
            else
            {
                value = Object.CheckConvert<String>(obj, "to_str", caller).value;
                return value != null;
            }
        }

        // rb_obj_as_string
        [UsedByRubyCompiler]
        public static String ObjectAsString(object obj, Frame caller)
        {
            if (obj is String)
            {
                return (String)obj;
            }
            else if (obj is string)
            {
                return new String((string)obj);
            }
            else
            {
                object str = Eval.CallPrivate(obj, caller, "to_s", null);

                if (!(str is String))
                {
                    return Object.rb_any_to_s(obj);
                }
                else
                {
                    return (String)str;
                }
            }
        }

        internal static Regexp get_pat(object pat, bool quote, Frame caller)
        {
            if (pat is Regexp)
                return (Regexp)pat;
            else if (pat is String)
            { }
            else
            {
                String val = rb_check_string_type(pat, caller);
                if (val == null)
                {
                    Object.CheckType<Regexp>(caller, pat);
                }
                pat = val;
            }

            if (quote)
            {
                pat = Regexp.rb_reg_quote((String)pat);
            }

            return new Regexp(((String)pat).value, 0);
        }


        internal static string DecimalToBase(int iDec, int numberBase, int zeroPadding)
        {
            if (numberBase > 16 || numberBase < 2) throw new System.ArgumentOutOfRangeException("numberBase", numberBase, "numberBase - Range: 2..16");
            if (zeroPadding < 0) throw new System.ArgumentOutOfRangeException("zeroPadding", zeroPadding, "zeroPadding should be positive");

            char[] cHexa = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            bool negative;

            if (iDec < 0)
            {
                negative = true;
                iDec *= -1;
            }
            else
            {
                negative = false;
            }

            int MaxBit = 32;
            int max = MaxBit;
            int[] result = new int[MaxBit];

            for (; iDec > 0; iDec /= numberBase)
            {
                int rem = iDec % numberBase;
                result[--max] = rem;
            }

            if (iDec > 0) throw new System.ArgumentOutOfRangeException("iDec", iDec, "iDec - Too large");

            for (int i = max; i < MaxBit; i++)
            {
                str.Append(cHexa[result[i]]);
            }
            int zeroPaddingRequired = zeroPadding - (MaxBit - max);
            if (zeroPaddingRequired > 0) str.Insert(0, new string('0', zeroPaddingRequired));
            if (negative) str.Insert(0, '-');
            return str.ToString();
        }

        internal String rb_str_dup(Frame caller)
        {
            return (String)Methods.rb_str_replace.singleton.Call1(this.my_class, new String(), caller, null, this);
        }

        internal static object rb_str_to_inum(object str, Frame caller, int numBase, bool badCheck)
        {
            string s;
            s = StringValue(str, caller);
            return Bignum.rb_str2num(s, caller, numBase, badCheck);
        }

        internal class Tr
        {
            bool gen;
            int now, max;
            string pString;
            int p, pEnd;

            internal Tr(string s)
            {
                pString = s;
                p = 0; now = 0; max = 0;
                pEnd = s.Length;
                gen = false;
            }

            internal bool HasMore()
            {
                return p != pEnd || gen == true;
            }

            internal void MoveNext()
            {
                p++;
            }

            internal int Now
            {
                get
                {
                    return now;
                }
            }

            //tr_next
            internal int Next()
            {
                for (; ; )
                {
                    if (!gen)
                    {
                        if (p == pEnd) //if EOS return -1, should use HasMore though
                        {
                            return -1;
                        }
                        //if not at last char and char is '\\' then skip over it
                        if (p < pEnd - 1 && pString[p] == '\\')
                        {
                            p++;
                        }
                        now = pString[p++];
                        //if there is a '-' then need to return the chars in the range (e.g. a-z)
                        if (p < pEnd - 1 && pString[p] == '-')
                        {
                            p++;
                            if (p < pEnd)
                            {
                                //ignore descending ranges
                                if (now > pString[p])
                                {
                                    p++;
                                    continue;
                                }
                                gen = true;
                                max = pString[p++];
                            }
                        }
                        return now;
                    }
                    else if (++now < max)
                    {
                        return now;
                    }
                    else
                    {
                        gen = false;
                        return max;
                    }
                }
            }
        }

        // tr_setup_table
        internal static void SetupTable(string str, char[] table, bool init)
        {

            char[] buf = new char[256];
            Tr tr = new Tr(str);
            int i, c;
            int cflag = 0;

            if (str.Length > 1 && str[0] == '^')
            {
                cflag = 1;
                tr.MoveNext();
            }

            if (init)
            {
                for (i = 0; i < 256; i++)
                {
                    table[i] = (char)1;
                }
            }

            for (i = 0; i < 256; i++)
            {
                buf[i] = (char)cflag;
            }

            while (tr.HasMore())
            {
                c = tr.Next();
                if (cflag == 1)
                {
                    buf[c] = (char)0;
                }
                else
                {
                    buf[c] = (char)1;
                }
            }
            for (i = 0; i < 256; i++)
            {
                char test = (char)(table[i] & buf[i]);
                table[i] = (char)(table[i] & buf[i]);
            }
        }

        internal static object tr_trans(object str, Frame caller, Proc block, object src, object repl, bool sflag)
        {

            Tr trSrc, trRepl;
            bool cflag = false;
            int[] trans = new int[256];
            int i, c;
            bool modify = false;
            int s, send;
            System.Text.StringBuilder t = new System.Text.StringBuilder();

            string sString = StringValue(str, caller);
            string srcString = StringValue(src, caller);
            string replString = StringValue(repl, caller);

            if (((String)str).value == null || ((String)str).value.Length == 0)
            {
                return null;
            }
            trSrc = new Tr(srcString);
            if (srcString.Length >= 2 && srcString[0] == '^')
            {
                cflag = true;
                trSrc.MoveNext();
            }
            if (replString.Length == 0)
            {
                return Ruby.Methods.rb_str_delete_bang.singleton.Call(null, str, caller, block, new Array(src));
            }
            trRepl = new Tr(replString);

            if (cflag)
            {
                for (i = 0; i < 256; i++)
                {
                    trans[i] = 1;
                }
                while (trSrc.HasMore())
                {
                    c = trSrc.Next();
                    trans[c] = -1;
                }
                while (trRepl.HasMore())
                {
                    /* retrieve last replacer */
                    c = trRepl.Next();
                }
                for (i = 0; i < 256; i++)
                {
                    if (trans[i] >= 0)
                    {
                        trans[i] = trRepl.Now;
                    }
                }
            }
            else
            {
                int r;

                for (i = 0; i < 256; i++)
                {
                    trans[i] = -1;
                }
                while (trSrc.HasMore())
                {
                    c = trSrc.Next();
                    r = trRepl.Next();
                    if (r == -1)
                    {
                        r = trRepl.Now;
                    }
                    trans[c] = r;
                }
            }

            s = 0;
            send = s + sString.Length;
            if (sflag)
            {
                int c0, last = -1;
                while (s < send)
                {
                    c0 = sString[s++];
                    if ((c = trans[c0]) >= 0)
                    {
                        if (last == c) continue;
                        last = c;
                        t.Append((char)c);
                        modify = true;
                    }
                    else
                    {
                        last = -1;
                        t.Append((char)c0);
                    }
                }
            }
            else
            {
                while (s < send)
                {
                    if ((c = trans[(int)sString[s]]) >= 0)
                    {
                        t.Append((char)c);
                        modify = true;
                    }
                    else
                    {
                        t.Append((char)sString[s]);
                    }
                    s++;
                }
            }

            if (modify)
            {
                ((String)str).value = t.ToString();
                return str;
            }
            else
            {
                return null;
            }
        }

        //TODO: check what happens if length is longer than the string?
        internal static String infected_str_new(int startIndex, int length, string sourceString)
        {
            String s = new String(sourceString.Substring(startIndex, length));

            //TODO:
            //OBJ_INFECT(s, str);
            return s;
        }

        //-----------------------------------------------------------------

        // This utility searches an array list and checks if any 
        // of the elements is (i) a String, and (ii) is the same
        // string as str. Beware-1! It is assumed that str is interned.
        // Beware-2! This does linear search on the underlying array.
        internal static bool ListContains(ArrayList list, string str)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i] is String && ((String)list[i]).value == str)
                    return true;
            return false;
        }

        //-----------------------------------------------------------------

        internal static void rb_invalid_str(Frame caller, string str, string type)
        {
            object s = Ruby.Methods.rb_str_inspect.singleton.Call0(null, new String(str), null, null);

            throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "invalid value for {0}: {1}", type, ((String)(s)).value)).raise(caller);
        }

        //TODO: this does not completely replicate the 'strtod' behaviour. 
        internal static double strtod(string pString, int p, out int end)
        {
            //[sign][integral-digits][.[fractional-digits]][e[sign]exponential-digits]
            System.Text.StringBuilder doubleText = new System.Text.StringBuilder();

            end = p;
            if (end < pString.Length && (pString[end] == '+' || pString[end] == '-'))
            {
                doubleText.Append(pString[end]);
                end++;
            }
            while (end < pString.Length && char.IsDigit(pString[end]))
            {
                doubleText.Append(pString[end]);
                end++;
            }
            if (end < pString.Length && pString[end] == '.')
            {
                doubleText.Append(pString[end]);
                end++;
                while (end < pString.Length && char.IsDigit(pString[end]))
                {
                    doubleText.Append(pString[end]);
                    end++;
                }
            }
            if (end < pString.Length && (pString[end] == 'e' || pString[end] == 'E'))
            {
                doubleText.Append(pString[end]);
                end++;
                if (end < pString.Length && (pString[end] == '+' || pString[end] == '-'))
                {
                    doubleText.Append(pString[end]);
                    end++;
                }
                if (end < pString.Length && char.IsDigit(pString[end]))
                {
                    while (end < pString.Length && char.IsDigit(pString[end]))
                    {
                        doubleText.Append(pString[end]);
                        end++;
                    }
                }
                else
                {
                    //must have an exponent after the 'e[sign]' so roll back if there is none
                    if (doubleText[doubleText.Length] == 'e' || doubleText[doubleText.Length] == 'E')
                    {
                        //remove one char
                        doubleText.Remove(doubleText.Length - 1, 1);
                        end--;
                    }
                    else
                    {
                        //remove two chars 
                        doubleText.Remove(doubleText.Length - 2, 1);
                        end -= 2;
                    }
                }
            }

            try
            {
                return double.Parse(doubleText.ToString(), CultureInfo.InvariantCulture);
            }
            catch (System.Exception)
            {
                end = p;
                return 0;
            }
        }


        internal static double rb_cstr_to_dbl(Frame caller, string pString, bool badcheck) //needs testing
        {
            int p = 0;
            int end = 0;
            double d = 0;

            if (pString.Length == 0)
            {
                return 0.0;
            }
            p = 0;
            if (badcheck)
            {
                while (char.IsWhiteSpace(pString[p])) p++;//TEST: IsWhiteSpace() must be equivalent to isspace()
            }
            else
            {
                while (char.IsWhiteSpace(pString[p]) || pString[p] == '_') p++;//TEST: IsWhiteSpace() must be equivalent to isspace()
            }
            try
            {
                d = strtod(pString, p, out end);
            }
            catch (System.OverflowException)
            {
                Errors.rb_warn(string.Format(CultureInfo.InvariantCulture, "Float {0} out of range", pString.Substring(p, end - p)));
            }

            if (p == end)
            {
                if (badcheck)
                {
                    goto bad;
                }
                return d;
            }
            if (end < pString.Length)
            {
                System.Text.StringBuilder buf = new System.Text.StringBuilder();
                int n = 0;

                while (p < end)
                {
                    buf.Append(pString[p++]);
                    n++;
                }
                while (p < pString.Length)
                {
                    if (pString[p] == '_')
                    {
                        /* remove underscores between digits */

                        if (badcheck)
                        {
                            if (n == buf.Length || !char.IsDigit(buf[n - 1]))
                            {
                                ++p;
                            }
                            if (!char.IsDigit(pString[p]))
                            {
                                goto bad;
                            }
                        }
                        else
                        {
                            while (pString[++p] == '_') ;
                            continue;
                        }
                    }
                    buf.Append(pString[p++]);
                    n++;
                }
                p = 0;
                pString = buf.ToString();
                try
                {
                    d = strtod(pString, p, out end);
                }
                catch (System.OverflowException)
                {
                    Errors.rb_warn(string.Format(CultureInfo.InvariantCulture, "Float {0} out of range", pString.Substring(p, end - p)));
                }
                if (badcheck)
                {
                    if (p == end) goto bad;
                    while (end < pString.Length && end < pString.Length && char.IsWhiteSpace(pString[end])) end++;
                    if (end < pString.Length)
                    {
                        goto bad;
                    }
                }
            }
            return d;

        bad:
            rb_invalid_str(caller, pString, "Float()");

            return 0;//stubb to appease compiler
        }

        internal static double rb_str_to_dbl(Frame caller, String str, bool badcheck)
        {
            if (badcheck && string.Empty.Equals(str.value))
            {
                new ArgumentError("string for Float contains null byte").raise(caller);
            }

            return rb_cstr_to_dbl(caller, str.value, badcheck);
        }

        internal static String rb_check_string_type(object ostr, Frame caller)
        {
            String str = Object.CheckConvert<String>(ostr, "to_str", caller);
            if (str != null && str.value == null)
            {
                //FL_SET(str, ELTS_SHARED);
                str.value = string.Empty;
            }
            return str;
        }

        private static int rb_str_index(String str, String sub, int offset)
        {
            int pos;

            if (offset < 0)
            {
                offset += str.value.Length;
                if (offset < 0)
                    return -1;
            }
            if (str.value.Length - offset < str.value.Length)
                return -1;
            if (str.value.Length == 0)
                return offset;
            pos = str.value.IndexOf(sub.value, (int)offset);
            if (pos < 0)
                return pos;
            return pos + offset;
        }

        internal static String rb_str_subpat(String str, Regexp re, int nth, Frame caller)
        {
            if (re.rb_reg_search(str, 0, false, caller) >= 0)
            {
                return Regexp.rb_reg_nth_match(nth, Regexp.rb_backref_get(caller));
            }
            return null;
        }



        internal static object rb_str_aref(Frame caller, String str, object indx)
        {

            if (indx is int)
            {
                goto num_index;
            }
            else if (indx is Regexp)
            {
                return rb_str_subpat(str, (Regexp)indx, 0, caller);
            }
            else if (indx is String)
            {
                if (rb_str_index(str, (String)indx, 0) != -1)
                    return indx;
                return null;
            }
            else // check if indx is Range
            {
                int beg, len;
                object result = Range.MapToLength(indx, str.value.Length, false, true, out beg, out len, caller);
                if (result is bool && ((bool)result) == false)
                {
                    //drop through the num2long conversion
                }
                else if (result == null)
                {
                    return null;
                }
                else
                {
                    return String.rb_str_substr(str, beg, len);
                }
            }
            indx = Numeric.rb_num2long(indx, caller);

        num_index:
            int idx = Numeric.rb_num2long(indx, caller);
            if (idx < 0)
                idx = str.value.Length + idx;
            if (idx < 0 || str.value.Length <= idx)
                return null;
            return (int)str.value[idx];
        }

        internal static void rb_str_splice(Frame caller, String str, int beg, int len, object _val)
        {
            if (len < 0)
                throw new IndexError(string.Format(CultureInfo.InvariantCulture, "negative length {0}", len)).raise(caller);

            String val = (String)_val;
            //rb_str_modify(str);

            if (str.value.Length < beg)
            {
                throw new IndexError(string.Format(CultureInfo.InvariantCulture, "index {0} out of string", beg)).raise(caller);
            }
            if (beg < 0)
            {
                if (-beg > str.value.Length)
                {
                    throw new IndexError(string.Format(CultureInfo.InvariantCulture, "index {0} out of string", beg)).raise(caller);
                }
                beg += str.value.Length;
            }
            if (str.value.Length < beg + len)
            {
                len = str.value.Length - beg;
            }

            str.value = str.value.Remove(beg, len).Insert(beg, val.value);
            str.Tainted |= val.Tainted;
        }

        private void rb_str_update(Frame caller, String str, int beg, int len, object val)
        {
            rb_str_splice(caller, str, beg, len, val);
        }

        internal static void rb_str_subpat_set(String str, Regexp re, int nth, object val, Frame caller)
        {
            Match match;
            int start, len;

            if (re.rb_reg_search(str, 0, false, caller) < 0)
            {
                throw new IndexError("regexp not matched").raise(caller);
            }
            match = Regexp.rb_backref_get(caller);
            if (nth >= match.value.Groups.Count)
            {
                throw new IndexError(string.Format(CultureInfo.InvariantCulture, "index {0} out of regexp", nth)).raise(caller);
            }
            if (nth < 0)
            {
                if (-nth >= match.value.Groups.Count)
                {
                    throw new IndexError(string.Format(CultureInfo.InvariantCulture, "index {0} out of regexp", nth)).raise(caller);
                }
                nth += match.value.Groups.Count;
            }

            start = match.value.Groups[nth].Index;
            if (start == -1)
            {
                throw new IndexError(string.Format(CultureInfo.InvariantCulture, "regexp group {0} not matched", nth)).raise(caller);
            }
            len = match.value.Groups[nth].Length;
            rb_str_splice(caller, str, start, len, val);
        }

        //TODO: this was added to the source tree 22/1/06, 
        //some methods, particularly those that make use of Range.MapToLength
        //may need to use this method as opposed to the CLR substr method. 
        internal static object rb_str_substr(object str, int beg, int len)
        {
            object str2;

            if (len < 0) return null;
            if (beg > ((String)str).value.Length) return null;
            if (beg < 0)
            {
                beg += ((String)str).value.Length;
                if (beg < 0) return null;
            }
            if (beg + len > ((String)str).value.Length)
            {
                len = ((String)str).value.Length - beg;
            }
            if (len < 0)
            {
                len = 0;
            }
            if (len == 0) return new String();

            if (false)
            {
                //STR_ASSOC seems to be related to the P and p directives of pack
                //this code is a bit of a mystery at the moment; although it seems
                //unimportant on the .net playform
                //    if (len > sizeof(struct RString)/2 &&
                //        beg + len == RSTRING(str)->len && !FL_TEST(str, STR_ASSOC)) {
                //            str2 = rb_str_new3(rb_str_new4(str));
                //            RSTRING(str2)->ptr += RSTRING(str2)->len - len;
                //            RSTRING(str2)->len = len;
                //    }
            }
            else
            {
                str2 = new String(((String)str).value.Substring(beg, len));
            }
            Object.obj_infect(str2, str);

            return str2;
        }

        internal static object rb_str_aset(Frame caller, String str, object indx, object val)
        {
            bool fallthrough = false;
            int idx, beg;

        num_index:
            if (indx is int || fallthrough)
            {
                if (!fallthrough)
                    idx = (int)indx;
                else
                    idx = Numeric.rb_num2long(indx, caller);

                if (str.value.Length <= idx)
                {
                    throw new IndexError(string.Format(CultureInfo.InvariantCulture, "index {0} out of string", idx)).raise(caller);
                }
                if (idx < 0)
                {
                    if (-idx > str.value.Length)
                        throw new IndexError(string.Format(CultureInfo.InvariantCulture, "index {0} out of string", idx)).raise(caller);
                    idx += str.value.Length;
                }
                if (val is int)
                {
                    //rb_str_modify(str);
                    System.Text.StringBuilder sb = new System.Text.StringBuilder(str.value);
                    if (str.value.Length <= idx)
                    {
                        sb.Length = idx + 1;
                    }
                    sb[idx] = (char)(Numeric.rb_num2long(val, caller));
                    str.value = sb.ToString();
                }
                else
                {
                    rb_str_splice(caller, str, idx, 1, val);
                }
                return val;

            }
            else if (indx is Regexp)
            {
                rb_str_subpat_set(str, (Regexp)indx, 0, val, caller);
                return val;
            }
            else if (indx is String)
            {
                beg = rb_str_index(str, (String)indx, 0);
                if (beg < 0)
                {
                    throw new IndexError("string not matched").raise(caller);
                }
                rb_str_splice(caller, str, beg, ((String)indx).value.Length, val);
                return val;
            }
            else if (indx is Range)
            {
                int len;
                //object range_result = Range.rb_range_beg_len(caller, (Range)indx, out beg, out len, str.value.Length, 2);

                //if (range_result.Equals(true))
                object result = Range.MapToLength(indx, str.value.Length, true, true, out beg, out len, caller);
                if (result == null || (result is bool && ((bool)result) == true))
                {
                    rb_str_splice(caller, str, beg, len, val);
                    return val;
                }

                fallthrough = true;
                goto num_index;
            }
            else
            {
                fallthrough = true;
                goto num_index;
            }
        }

        internal static void str_frozen_check(Frame caller, String s)
        {
            if (s.Frozen)
                throw new RuntimeError("string frozen").raise(caller);
        }

        internal static String str_gsub(String str, Frame caller, Proc block, Array argv, bool bang)
        {
            int argc = argv.Count;

            String val;
            System.Text.StringBuilder dest = new System.Text.StringBuilder();
            Regexp pat;
            String repl = null;
            Match match = null;
            System.Text.RegularExpressions.Match regs;
            int beg;
            int offset;
            bool iter = false;
            bool tainted = false;

            if (argc == 1 && block != null)
            {
                iter = true;
            }
            else if (argc == 2)
            {
                repl = String.RStringValue(argv[1], caller);
                tainted = repl.Tainted;
            }
            else
            {
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "wrong number of arguments ({0} for 2)", argc)).raise(caller);
            }

            pat = get_pat(argv[0], true, caller);
            offset = 0;
            beg = pat.rb_reg_search(str, 0, false, caller);
            if (beg < 0)
            {
                if (bang)
                    return null; /* no match, no substitution */
                else
                    return str.rb_str_dup(caller);
            }

            //rb_str_locktmp(dest);
            while (beg >= 0)
            {
                match = Regexp.rb_backref_get(caller);
                regs = match.value;
                if (iter)
                {
                    //rb_match_busy(match);
                    val = ObjectAsString(Proc.rb_yield(block, caller, Regexp.rb_reg_nth_match(0, match)), caller);
                    //str_mod_check(str, sp, slen);
                    if (bang)
                        str_frozen_check(caller, str);
                    // This should not apply to the .NET implementation, because we are not using char pointers
                    //if (val.value == dest.ToString())
                    //{  /* paranoid chack [ruby-dev:24827] */
                    //    throw new RuntimeError("block should not cheat").raise(caller);
                    //}
                    Regexp.rb_backref_set(match, caller);

                    dest.Append(val.value);
                }
                else
                {
                    val = Regexp.rb_reg_regsub(repl, str, match);

                    dest.Append(str.value.Substring(offset, beg - offset)); /* copy pre-match substr */
                    dest.Append(val.value.Substring(beg, val.value.Length - str.value.Length + 1));
                }

                if (val.Tainted)
                    tainted = true;

                offset = match.value.Index + match.value.Length;

                if (match.value.Length == 0)
                {
                    if (str.value.Length <= match.value.Index)
                        break;
                    offset = match.value.Index + 1;
                }

                if (offset > str.value.Length)
                    break;

                beg = pat.rb_reg_search(str, offset, false, caller);
            }

            if (str.value.Length > offset)
                dest.Append(str.value.Substring(offset));

            Regexp.rb_backref_set(match, caller);

            //rb_str_unlocktmp(dest);

            if (bang)
                str.value = dest.ToString();
            else
                str = new String(dest.ToString());

            if (tainted)
                str.Tainted = true;

            return str;
        }

        internal static String uscore_get(Frame caller)
        {
            String line;

            line = Eval.rb_lastline_get(caller);
            //if (!(line is String))
            //    throw new TypeError(string.Format("$_ value need to be String ({0} given)", line == null ? "nil" : Class.rb_obj_classname(line))).raise(caller);

            return line;
        }

        //-----------------------------------------------------------------

        // <ctype.h> functions and macros - char ranges may not be perfectly correct

        internal static bool ISASCII(byte c) { return isascii(c); }
        internal static bool ISPRINT(byte c) { return (ISASCII(c) && isprint(c)); }
        internal static bool ISSPACE(byte c) { return (ISASCII(c) && isspace(c)); }
        internal static bool ISUPPER(byte c) { return (ISASCII(c) && isupper(c)); }
        internal static bool ISLOWER(byte c) { return (ISASCII(c) && islower(c)); }
        internal static bool ISALNUM(byte c) { return (ISASCII(c) && isalnum(c)); }
        internal static bool ISALPHA(byte c) { return (ISASCII(c) && isalpha(c)); }
        internal static bool ISDIGIT(byte c) { return (ISASCII(c) && isdigit(c)); }
        internal static bool ISXDIGIT(byte c) { return (ISASCII(c) && isxdigit(c)); }

        internal static bool isascii(byte c)
        {
            return ((c >= 0x00 && c <= 0x7f) ? true : false);
        }

        internal static bool isspace(byte c)
        {
            return (((c >= 0x09 && c <= 0x0d) || c == 0x20) ? true : false);
        }

        internal static bool isdigit(byte c)
        {
            return ((c >= 0x00 && c <= 0x09) ? true : false);
        }


        internal static bool isupper(byte c)
        {
            return ((c >= 'A' && c <= 'Z') ? true : false);
        }

        internal static bool isalpha(byte c)
        {
            return (((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) ? true : false);
        }


        internal static bool isprint(byte c)
        {
            return ((c >= 0x20 && c <= 0x7e) ? true : false);

        }

        internal static bool isalnum(byte c)
        {
            return ((isalpha(c) || isdigit(c)) ? true : false);
        }

        internal static bool iscntrl(byte c)
        {
            return (((c >= 0x00 && c <= 0x1f) || c == 0x7f) ? true : false);
        }

        internal static bool islower(byte c)
        {
            return ((c >= 'a' && c <= 'z') ? true : false);
        }

        internal static bool ispunct(byte c)
        {
            return (!(isalnum(c) || isspace(c)) ? true : false);
        }

        internal static bool isxdigit(byte c)
        {
            return (((c >= 0 && c <= 9) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')) ? true : false);
        }
    }
}
