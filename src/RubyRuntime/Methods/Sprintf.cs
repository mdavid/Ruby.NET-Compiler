/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Ruby.Methods
{
    //TODO: Test completely when 'sprintf' is implemented
    
    internal class rb_f_sprintf : VarArgMethodBody0 //author: war, status: partial
    {
        internal static rb_f_sprintf singleton = new rb_f_sprintf();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array args)
        {
            object fmt;
            string fmtString;
            int p, end;
            System.Text.StringBuilder result = new System.Text.StringBuilder();

            int width, prec, flags = Sprintf.FNONE;
            int nextArg = 1;
            int posArg = 0;
            //int tainted = 0;
            object nextValue;
            object tmp = null;
            object str;

            fmt = args[0];
            //if (OBJ_TAINTED(fmt)) tainted = 1;
            fmtString = String.StringValue(fmt, caller);
            //    fmt = rb_str_new4(fmt); - tainted work being done in this method
            p = 0; //p = RSTRING(fmt)->ptr;
            end = fmtString.Length;

            for (; p < end; p++)
            {
                int t;
                int n;

                for (t = p; t < end && fmtString[t] != '%'; t++) ; //skip over preceding chars
                result.Append(fmtString.Substring(p, t - p));
                if (t >= end)
                {
                    /* end of fmt string */
                    goto sprint_exit;
                }
                p = t + 1;  /* skip '%' */

                width = prec = -1;
                nextValue = null;

            retry:
                switch (fmtString[p])
                {
                    case ' ':
                        flags |= Sprintf.FSPACE; //leave a space at the start of positive numbers.
                        p++;
                        goto retry;

                    case '#':
                        flags |= Sprintf.FSHARP;
                        p++;
                        goto retry;

                    case '+':
                        flags |= Sprintf.FPLUS;
                        p++;
                        goto retry;

                    case '-':
                        flags |= Sprintf.FMINUS;
                        p++;
                        goto retry;

                    case '0':
                        flags |= Sprintf.FZERO;
                        p++;
                        goto retry;

                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        n = 0;
                        for (; p < end && char.IsDigit(fmtString[p]); p++)
                        {
                            n = 10 * n + (fmtString[p] - '0');
                        }
                        if (p >= end)
                        {
                            throw new ArgumentError("malformed format string - %[0-9]").raise(caller);
                        }
                        if (fmtString[p] == '$')
                        {
                            if (nextValue != null)
                            {
                                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "value given twice - {0}$", n)).raise(caller);
                            }
                            nextValue = Sprintf.GETPOSARG(caller, n, ref posArg, args);
                            p++;
                            goto retry;
                        }
                        width = n;
                        flags |= Sprintf.FWIDTH;
                        goto retry;

                    case '*':
                        if ((flags & Sprintf.FWIDTH) > 0)
                        {
                            throw new ArgumentError("width given twice").raise(caller);
                        }
                        flags |= Sprintf.FWIDTH;
                        width = Sprintf.GETASTER(ref t, ref p, out n, end, fmtString,
                            ref posArg, ref nextArg, nextValue, tmp, args, caller);
                        if (width < 0)
                        {
                            flags |= Sprintf.FMINUS;
                            width = -width;
                        }
                        p++;
                        goto retry;

                    case '.':
                        if ((flags & Sprintf.FPREC) > 0)
                        {
                            throw new ArgumentError("precision given twice").raise(caller);
                        }
                        flags |= Sprintf.FPREC;

                        prec = 0;
                        p++;
                        if (fmtString[p] == '*')
                        {
                            prec = Sprintf.GETASTER(ref t, ref p, out n, end, fmtString,
                            ref posArg, ref nextArg, nextValue, tmp, args, caller);
                            if (prec < 0) /* ignore negative precision */
                            {
                                flags &= ~Sprintf.FPREC;
                            }
                            p++;
                            goto retry;
                        }
                        for (; p < end && char.IsDigit(fmtString[p]); p++)
                        {
                            prec = 10 * prec + (fmtString[p] - '0');
                        }
                        if (p >= end)
                        {
                            throw new ArgumentError("malformed format string - %%.[0-9]").raise(caller);
                        }
                        goto retry;
                    case '\n':
                        p--;
                        goto case '%';
                    case '\0':
                        goto case '%';
                    case '%':
                        if (flags != Sprintf.FNONE)
                        {
                            throw new ArgumentError("illegal format character - %").raise(caller);
                        }
                        result.Append("%");
                        break;
                    case 'c':
                        {
                            object val = Sprintf.GETARG(caller, ref posArg, ref nextArg, nextValue, args);
                            int c;
                            if (!((flags & Sprintf.FMINUS) > 0))
                            {
                                while (--width > 0)
                                {
                                    result.Append(" ");
                                }
                            }
                            c = Numeric.rb_num2long(val, caller) & 0xff;
                            result.Append((char)(byte)c);
                            while (--width > 0)
                            {
                                result.Append(" ");
                            }
                        }
                        break;
                    case 's':
                        goto case 'p';
                    case 'p':
                        {
                            object arg = Sprintf.GETARG(caller, ref posArg, ref nextArg, nextValue, args);
                            int len;

                            if (fmtString[p] == 'p') arg = Object.Inspect(arg, caller);
                            str = String.ObjectAsString(arg, caller);
                            //if (OBJ_TAINTED(str)) tainted = 1;
                            len = ((String)str).value.Length;
                            if ((flags & Sprintf.FPREC) > 0)
                            {
                                if (prec < len)
                                {
                                    len = prec;
                                }
                            }
                            if ((flags & Sprintf.FWIDTH) > 0)
                            {
                                if (width > len)
                                {
                                    width -= len;
                                    if (!((flags & Sprintf.FMINUS) > 0))
                                    {
                                        while (width-- > 0)
                                        {
                                            result.Append(' ');
                                        }
                                    }
                                    result.Append(((String)str).value.Substring(0, len));
                                    if ((flags & Sprintf.FMINUS) > 0)
                                    {
                                        while (width-- > 0)
                                        {
                                            result.Append(' ');
                                        }
                                    }
                                    break;
                                }
                            }
                            result.Append(((String)str).value.Substring(0, len));
                        }
                        break;

                    case 'd':
                    case 'i':
                    case 'o':
                    case 'x':
                    case 'X':
                    case 'b':
                    case 'B':
                    case 'u':
                        {
                            object val = Sprintf.GETARG(caller, ref posArg, ref nextArg, nextValue, args);
                            string prefix = "";
                            int sign = 0;
                            int v = 0;
                            int bignum = 0;
                            int numBase = 0;
                            char sc = '\0';
                            string fbuf = "";
                            string nbuf = "";
                            int s = 0;
                            string sString = "";
                            int tt = 0;
                            string ttString = "";
                            object tmp2;
                            //int pos;
                            int len;

                            switch (fmtString[p])
                            {
                                case 'd':
                                    goto case 'i';
                                case 'i':
                                    sign = 1; break;
                                case 'o':
                                case 'x':
                                case 'X':
                                case 'b':
                                case 'B':
                                case 'u':
                                default:
                                    if ((flags & (Sprintf.FPLUS | Sprintf.FSPACE)) > 0) sign = 1;
                                    break;
                            }
                            if ((flags & Sprintf.FSHARP) > 0)
                            {
                                switch (fmtString[p])
                                {
                                    case 'o':
                                        prefix = "0"; break;
                                    case 'x':
                                        prefix = "0x"; break;
                                    case 'X':
                                        prefix = "0X"; break;
                                    case 'b':
                                        prefix = "0b"; break;
                                    case 'B':
                                        prefix = "0B"; break;
                                }
                                if (prefix.Length > 0)
                                {
                                    width -= prefix.Length;
                                }
                            }
                        bin_retry:
                            if (val is Float)
                            {
                                val = new Bignum(((Float)val).value);
                                if (val is int) goto bin_retry;
                                bignum = 1;
                            }
                            else if (val is String)
                            {
                                val = String.rb_str_to_inum(val, caller, 0, true); //TEST - should base be 0?
                                goto bin_retry;
                            }
                            else if (val is Bignum)
                            {
                                bignum = 1;
                            }
                            else if (val is int)
                            {
                                v = (int)val;
                            }
                            else
                            {
                                val = Integer.rb_Integer(val, caller);
                                goto bin_retry;
                            }
                            switch (fmtString[p])
                            {
                                case 'o':
                                    numBase = 8; break;
                                case 'x':
                                case 'X':
                                    numBase = 16; break;
                                case 'b':
                                case 'B':
                                    numBase = 2; break;
                                case 'u':
                                case 'd':
                                case 'i':
                                default:
                                    numBase = 10; break;
                            }
                            if (bignum == 0)
                            {
                                if (numBase == 2)
                                {
                                    val = new Bignum(v);
                                    goto bin_retry;
                                }
                                if (sign != 0)
                                {
                                    char c = fmtString[p];
                                    if (c == 'i') c = 'd'; /* %d and %i are identical */
                                    if (v < 0)
                                    {
                                        v = -v;
                                        sc = '-';
                                        width--;
                                    }
                                    else if ((flags & Sprintf.FPLUS) > 0)
                                    {
                                        sc = '+';
                                        width--;
                                    }
                                    else if ((flags & Sprintf.FSPACE) > 0)
                                    {
                                        sc = ' ';
                                        width--;
                                    }
                                    fbuf = Sprintf.sprintf("%%l%c", c);
                                    nbuf = Sprintf.sprintf(fbuf, v);
                                    s = 0;//s = nbuf;
                                    sString = nbuf;
                                    goto format_integer;
                                }
                                s = 0;//s = nbuf;
                                if (v < 0)
                                {
                                    if (numBase == 10)
                                    {
                                        Errors.rb_warning("negative number for %%u specifier");
                                    }
                                    if (!((flags & (Sprintf.FPREC | Sprintf.FZERO)) > 0))
                                    {
                                        nbuf = nbuf.Insert(s, "..");
                                        s += 2;
                                    }
                                }
                                fbuf = Sprintf.sprintf("%%l%c", fmtString[p] == 'X' ? 'x' : fmtString[p]);
                                nbuf = nbuf.Substring(0, s) + Sprintf.sprintf(fbuf, v);
                                if (v < 0)
                                {
                                    char d = '\0';
                                    Sprintf.remove_sign_bits(nbuf.Substring(s), numBase);
                                    switch (numBase)
                                    {
                                        case 16:
                                            d = 'f'; break;
                                        case 8:
                                            d = '7'; break;
                                    }
                                    if (d != '\0' && (nbuf[s] != d))
                                    {
                                        nbuf.Insert(s, d.ToString());
                                    }
                                }
                                s = 0;//s = nbuf;
                                sString = nbuf;
                                goto format_integer;
                            }
                            //bignums between here and format_integer
                            if (sign > 0)
                            {
                                tmp2 = ((Bignum)val).value.ToString((uint)numBase); //tmp2 = rb_big2str(val, numBase);
                                s = 0;
                                sString = (string)tmp2;
                                if (sString[s] == '-')
                                {
                                    s++;
                                    sc = '-';
                                    width--;
                                }
                                else if ((flags & Sprintf.FPLUS) > 0)
                                {
                                    sc = '+';
                                    width--;
                                }
                                else if ((flags & Sprintf.FSPACE) > 0)
                                {
                                    sc = ' ';
                                    width--;
                                }
                                goto format_integer;
                            }
                            //negative bignum from now on
                            if (((Bignum)val).value < (IronMath.integer.make(0)))
                            {
                                val = new Bignum(((Bignum)val).value);
                                //must be equivalent to rb_big_2comp(val); TEST
                                new Bignum(~((Bignum)val).value);
                            }
                            tmp2 = new String(((Bignum)val).value.ToString((uint)numBase)); //tmp2 = rb_big2str(val, numBase);
                            s = 0;//              s = RSTRING(tmp2)->ptr;
                            sString = ((String)tmp2).value;
                            if (sString[s] == '-')
                            {
                                if (numBase == 10)
                                {
                                    Errors.rb_warning("negative number for %%u specifier");
                                    s++;
                                }
                                else
                                {
                                    sString = Sprintf.remove_sign_bits(sString.Substring(++s), numBase);
                                    tmp2 = new String();
                                    tt = 0;
                                    ttString = ((String)tmp2).value;
                                    if (!((flags & (Sprintf.FPREC | Sprintf.FZERO)) > 0))
                                    {
                                        ttString.Insert(tt, "..");
                                        tt += 2;
                                    }
                                    switch (numBase)
                                    {
                                        case 16:
                                            if (sString[0] != 'f')
                                            {
                                                ttString += "f"; //strcpy(t++, "f");
                                                tt++;
                                            }
                                            break;
                                        case 8:
                                            if (sString[0] != 'f')
                                            {
                                                ttString += "7"; //strcpy(t++, "7");
                                                tt++;
                                            }
                                            break;
                                        case 2:
                                            if (sString[0] != 'f')
                                            {
                                                ttString += "1"; //strcpy(t++, "1");
                                                tt++;
                                            }
                                            break;
                                    }
                                    ttString += sString; //check //strcpy(t, s);
                                    bignum = 2;
                                }
                            }
                            s = 0;
                            sString = ((String)tmp2).value;
                        format_integer:
                            //pos = -1;
                            len = sString.Length;
                            if (fmtString[p] == 'X')
                            {
                                sString = sString.ToUpperInvariant();
                            }
                            if ((flags & (Sprintf.FZERO | Sprintf.FPREC)) == Sprintf.FZERO)
                            {
                                prec = width;
                                width = 0;
                            }
                            else
                            {
                                if (prec < len) prec = len;
                                width -= prec;
                            }

                            if (!((flags & Sprintf.FMINUS) > 0))
                            {
                                while (width-- > 0)
                                {
                                    result.Append(' ');
                                }
                            }
                            if (sc != '\0')
                            {
                                result.Append(sc);
                            }
                            if (prefix.Length > 0)
                            {
                                result.Append(prefix);
                            }
                            if (bignum == 0 && v < 0)
                            {
                                char c = Sprintf.sign_bits(numBase, fmtString[p]);
                                while (len < prec--)
                                {
                                    result.Append(c);
                                }
                            }
                            else
                            {
                                char c;

                                if ((bignum != 0) && ((Bignum)val).value < (IronMath.integer.make(0)))
                                {
                                    c = Sprintf.sign_bits(numBase, fmtString[p]);
                                }
                                else
                                {
                                    c = '0';
                                }
                                while (len < prec--)
                                {
                                    result.Append(c);
                                }
                            }
                            result.Append(sString.Substring(s, len));
                            while (width-- > 0)
                            {
                                result.Append(' ');
                            }
                        }
                        break;
                    case 'f':
                    case 'g':
                    case 'G':
                    case 'e':
                    case 'E':
                        {
                            object val = Sprintf.GETARG(caller, ref posArg, ref nextArg, nextValue, args);
                            double fval;
                            int i = 0;
                            int need = 6;
                            string fbuf;

                            fval = Float.rb_Float(val, caller).value;
                            if (double.IsNaN(fval) || double.IsInfinity(fval))
                            {
                                string expr;

                                if (double.IsNaN(fval))
                                {
                                    expr = "NaN";
                                }
                                else
                                {
                                    expr = "Inf";
                                }
                                need = expr.Length;
                                if ((!double.IsNaN(fval) && fval < 0.0) || ((flags & Sprintf.FPLUS) != 0))
                                {
                                    need++;
                                }
                                if (((flags & Sprintf.FWIDTH) != 0) && need < width)
                                {
                                    need = width;
                                }
                                //TODO - may need a blen count. 
                                //                  sprintf(&buf[blen], "%*s", need, "");
                                if ((flags & Sprintf.FMINUS) != 0)
                                {
                                    if (!double.IsNaN(fval) && fval < 0.0)
                                    {
                                        result.Append('-'); //buf[blen++] = '-';
                                    }
                                    else if ((flags & Sprintf.FPLUS) != 0)
                                    {
                                        result.Append('+'); //buf[blen++] = '+';
                                    }
                                    else if ((flags & Sprintf.FSPACE) != 0)
                                    {
                                        result.Append(' '); //blen++;
                                    }
                                    result.Append(expr); //strncpy(&buf[blen], expr, strlen(expr));
                                }
                                else if ((flags & Sprintf.FZERO) != 0)
                                {
                                    if (!double.IsNaN(fval) && fval < 0.0)
                                    {
                                        result.Append('-');//buf[blen++] = '-';
                                        need--;
                                    }
                                    else if ((flags & Sprintf.FPLUS) != 0)
                                    {
                                        result.Append('+');    //buf[blen++] = '+';
                                        need--;
                                    }
                                    else if ((flags & Sprintf.FSPACE) != 0)
                                    {
                                        result.Append(' '); //blen++;
                                        need--;
                                    }
                                    while (need-- - expr.Length > 0)
                                    {
                                        result.Append('0');
                                    }
                                    result.Append(expr); //strncpy(&buf[blen], expr, strlen(expr));                                    
                                }
                                else
                                {
                                    if (!double.IsNaN(fval) && fval < 0.0)
                                        //run this - may need a blen count. 
                                        //could buffer overflow - check this out
                                        result[need - expr.Length - 1] = '-';  //buf[blen + need - strlen(expr) - 1] = '-';
                                    else if ((flags & Sprintf.FPLUS) != 0)
                                        result[need - expr.Length - 1] = '-';  //buf[blen + need - strlen(expr) - 1] = '+';
                                    //TODO - may need a blen count
                                    //strncpy(&buf[blen + need - strlen(expr)], expr, strlen(expr));
                                }
                                //blen += strlen(&buf[blen]);
                                break;
                            }

                            fbuf = Sprintf.fmt_setup(fmtString[p], flags, width, prec);
                            need = 0;
                            if (fmtString[p] != 'e' && fmtString[p] != 'E')
                            {
                                i = int.MinValue;
                                object frexpResult = Ruby.Methods.math_frexp.singleton.Call1(last_class, null, caller, block, new Float(fval));
                                i = (int)((Array)frexpResult).value[1]; //get the exponent
                                if (i > 0)
                                {
                                    need = Sprintf.BIT_DIGITS(i);
                                }
                            }
                            need += ((flags & Sprintf.FPREC) > 0) ? prec : 6;
                            if (((flags & Sprintf.FWIDTH) != 0) && need < width)
                            {
                                need = width;
                            }

                            need += 20;
                            result.Append(Sprintf.sprintf(fbuf, fval));  //              sprintf(&buf[blen], fbuf, fval);  
                            //may need to include belen calcs?
                            //              blen += strlen(&buf[blen]); 
                        }
                        break;
                    default:
                        if (char.IsLetterOrDigit(fmtString[p]) || fmtString[p] == ' ')
                            throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "malformed format string - %{0}", fmtString)).raise(caller);
                        else
                            throw new ArgumentError("malformed format string").raise(caller);
                }
                flags = Sprintf.FNONE;
            }
            
        sprint_exit:
            //    /* XXX - We cannot validiate the number of arguments because
            //    *       the format string may contain `n$'-style argument selector.
            //    */

            if (Eval.Test(Options.ruby_verbose.value) && posArg >= 0 && nextArg < args.Count) {
                throw new ArgumentError("too many arguments for format string").raise(caller);
            }

            String res = new String(result.ToString());
            //TODO:
            //    if (tainted) OBJ_TAINT(result);
            return res;
        }
    }
}
