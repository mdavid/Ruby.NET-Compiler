/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ruby
{
    
    internal class Sprintf
    {
        internal static int FNONE = 0;
        internal static int FSHARP = 1;
        internal static int FMINUS = 2;
        internal static int FPLUS = 4;
        internal static int FZERO = 8;
        internal static int FSPACE = 16;
        internal static int FWIDTH = 32;
        internal static int FPREC = 64;

        internal static int GETASTER(ref int t, ref int p, out int n, int end,
            string fmtString, ref int posArg, ref int nextArg,
            object nextValue, object tmp, Array argv, Frame caller)
        {
            t = p++;
            n = 0;
            for (; p < end && char.IsDigit(fmtString[p]); p++)
            {
                n = 10 * n + (fmtString[p] - '0');
            }
            if (p >= end)
            {
                throw new ArgumentError("malformed format string - %*[0-9]").raise(caller);
            }
            if (fmtString[p] == '$')
            {
                tmp = GETPOSARG(caller, n, ref posArg, argv);
            }
            else
            {
                tmp = GETARG(caller, ref posArg, ref nextArg, nextValue, argv);
                p = t;
            }
            return Numeric.rb_num2long(tmp, caller);
        }

        internal static object GETNTHARG(Frame caller, int nth, Array argv)
        {
            if (nth >= argv.Count)
            {
                throw new ArgumentError("too few arguments.").raise(caller);
            }
            else
            {
                return argv[nth];
            }

        }

        internal static object GETPOSARG(Frame caller, int n, ref int posArg, Array argv)
        {
            if (posArg > 0)
            {
                throw new ArgumentError(string.Format("numbered({0}) after unnumbered({1})", n, posArg)).raise(caller);
            }
            else
            {
                if (n < 1)
                {
                    throw new ArgumentError(string.Format("invalid index - {0}$", n)).raise(caller);
                }
                else
                {
                    posArg = -1;
                    return GETNTHARG(caller, n, argv);
                }
            }
        }

        internal static object GETARG(Frame caller, ref int posArg, ref int nextArg, object nextValue, Array argv)
        {
            if (nextValue != null)
            {
                return nextValue;
            }
            else
            {
                if (posArg < 0)
                {
                    throw new ArgumentError(string.Format("unnumbered({0}) mixed with numbered", nextArg)).raise(caller);
                }
                else
                {
                    posArg = nextArg++;
                    return GETNTHARG(caller, posArg, argv);
                }
            }
        }

        internal static string remove_sign_bits(string str, int numBase)
        {
            int s = 0;
            int t = 0;

            if (numBase == 16)
            {
                while (str[t] == 'f')
                {
                    t++;
                }
            }
            else if (numBase == 8)
            {
                if (str[t] == '3') t++;
                while (str[t] == '7')
                {
                    t++;
                }
            }
            else if (numBase == 2)
            {
                while (str[t] == '1')
                {
                    t++;
                }
            }
            if (t > s)
            {
                str = str.Substring(t);
            }
            return str;
        }

        internal static char sign_bits(int numBase, char p)
        {

            char c = '.';

            switch (numBase)
            {

                case 16:
                    if (p == 'X') c = 'F';
                    else c = 'f';
                    break;
                case 8:
                    c = '7'; break;
                case 2:
                    c = '1'; break;
            }
            return c;
        }

        internal static int BIT_DIGITS(int N)
        {
            return (int)(((N) * 146) / 485 + 1);  /* log2(10) =~ 146/485 */
        }

        internal static string fmt_setup(char c, int flags, int width, int prec)
        {
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            buf.Append('%');
            if ((flags & FSHARP) != 0) buf.Append('#');
            if ((flags & FPLUS) != 0) buf.Append('+');
            if ((flags & FMINUS) != 0) buf.Append('-');
            if ((flags & FZERO) != 0) buf.Append('0');
            if ((flags & FSPACE) != 0) buf.Append(' ');

            if ((flags & FWIDTH) != 0)
            {
                string temp = sprintf("%d", width);
                buf.Append(temp);

            }
            if ((flags & FPREC) != 0)
            {
                string temp = sprintf(".%d", prec);
                buf.Append(temp);
            }

            buf.Append(c);
            return buf.ToString();
        }

        //[DllImport("msvcrt.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
        //static extern int sprintf(StringBuilder buffer, string format, __arglist);

        //stubbed for the moment - needs to replicate C language behaviour
        internal static string sprintf(string fmt, params object[] args)
        {
            StringBuilder result = new StringBuilder();
            int c = 0;
            int argNum = 0;
            while (c < fmt.Length)
            {
                if (fmt[c] == '%')
                {
                    c++;
                    int width = 0;
                    int precision = 0;
                    char type;
                    int temp = 0;
                    char lengthModifier;

                    //width
                    while (char.IsNumber(fmt[c]))
                    {
                        temp = 10 * temp + int.Parse(fmt[c].ToString());
                        c++;
                    }
                    width = temp;
                    if (fmt[c] == '.')
                    {
                        c++;
                        //precision
                        while (char.IsNumber(fmt[c]))
                        {
                            temp = 10 * temp + int.Parse(fmt[c].ToString());
                            c++;
                        }
                        precision = temp;
                    }
                    //length modifier
                    if (fmt[c] == 'l')
                    {
                        lengthModifier = fmt[c];
                        c++;
                    }
                    type = fmt[c];
                    switch (type)
                    {
                        case 'd':
                            result.Append(((int)args[argNum]).ToString());
                            argNum++;
                            break;
                        case 'g':
                        case 'e':
                            result.Append(((double)args[argNum]).ToString("r").ToLower());
                            argNum++;
                            break;
                        case 's':
                            result.Append(args[argNum]);
                            argNum++;
                            break;
                        case 'c':
                            result.Append((char)args[argNum]);
                            argNum++;
                            break;
                        case 'x':
                        case 'X':
                            result.Append(((int)args[argNum]).ToString("x"));
                            argNum++;
                            break;
                        case '%':
                            result.Append("%");
                            break;
                        case 'f':
                            string fix = ((double)args[argNum]).ToString(string.Format("f{0}", precision));
                            if (width > 0)
                                fix = fix.Substring(0, width);
                            result.Append(fix);
                            break;
                        default:
                            throw new NotImplementedError(string.Format("{0}", fmt)).raise(null);
                    }
                    c++;
                }
                else
                {
                    result.Append(fmt[c]);
                    c++;
                }
            }

            return result.ToString();
        }
    }
}
