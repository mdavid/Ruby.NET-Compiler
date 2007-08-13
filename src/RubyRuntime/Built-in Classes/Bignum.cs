/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using System.Globalization;

namespace Ruby
{
    [UsedByRubyCompiler]
    public partial class Bignum : Integer
    {
        internal IronMath.integer value;

        // -----------------------------------------------------------------------------

        private Bignum() : base(Ruby.Runtime.Init.rb_cBignum) {    }

        protected Bignum(Class klass) : base(klass) { }

        internal Bignum(double value) : this()
        {
            // IronMath's make method for doubles is very naive
            // build IronMath integer by parsing the double

            string number = value.ToString("e14", CultureInfo.InvariantCulture);
            int eIndex = number.IndexOf('e');
            if (number[eIndex - 1] == '0')
            {
                int count = 0;
                for (int i = eIndex - 1; i > number.IndexOf('.') + 1; i--)
                {
                    if (number[i] == '0')
                        count++;
                    else
                        break;
                }
                number = number.Remove(eIndex - count, count);
            }
            number = number.Remove(number.IndexOf('.'), 1);

            int exp = int.Parse(number.Substring(number.IndexOf('e') + 1), CultureInfo.InvariantCulture);
            exp -= number.IndexOf('e') - 1;
            if (value < 0)
                exp++;

            int digits = int.Parse(number.Remove(number.IndexOf('e')), CultureInfo.InvariantCulture);

            IronMath.integer x = IronMath.integer.make(digits);
            IronMath.integer y = IronMath.integer.make(10).pow(exp);

            this.value = IronMath.integer.multiply(x, y);
        }

        internal Bignum(int value) : this()
        {
            this.value = IronMath.integer.make(value);
        }

        internal Bignum(long value): this()
        {
            this.value = IronMath.integer.make(value);
        }

        internal Bignum(IronMath.integer value): this()
        {
            this.value = value;
        }

        [UsedByRubyCompiler]
        public Bignum(int sign, string number, int bas)
            : this()
        {
            value = IronMath.integer.ZERO;

            for (int i = 0; i < number.Length ; i++)
            {
                uint digit = (uint)(char.IsDigit(number[i]) ? number[i] - '0' : number[i] - 'A' + 10);
                value = value * bas + digit;

            }

            value *= sign;
        }


        // -----------------------------------------------------------------------------

        internal override object Inner()
        {
            return value;
        }

        public override string ToString()
        {
            return value.ToString();
        }


        // -----------------------------------------------------------------------------


        internal static object NormaliseUsing(IronMath.integer value)
        {
            if (Numeric.FIXNUM_MIN <= value && value <= Numeric.FIXNUM_MAX)
                return value.ToInt32();
            else 
                return new Bignum(value);
        }

        internal static object Normalise(object x)
        {
            if (x is int && Numeric.FIXNUM_MIN <= (int)x && (int)x <= Numeric.FIXNUM_MAX)
                return x;
            else
                return NormaliseUsing(((Bignum)x).value);
        }

        internal static int rb_big2long(Bignum b, Frame caller)
        {
            if (b.value > int.MaxValue || b.value < int.MinValue)
            {
                throw new RangeError("bignum too big to convert into `long'").raise(caller);
            }

            return b.value.ToInt32();
        }

        internal static uint rb_big2ulong(Bignum b, Frame caller)
        {
            if (b.value > uint.MaxValue || b.value < uint.MinValue)
            {
                throw new RangeError("bignum too big to convert into `unsigned long'").raise(caller);
            }

            return b.value.ToUInt32();
        }

        internal static uint rb_big2ulong_pack(Bignum b, Frame caller)
        {
            if (b.value < 0)
            {
                Bignum positiveBignum = new Bignum(b.value * -1);
                return (uint)((rb_big2ulong(positiveBignum, caller) * -1) & 0xFFFFFFFF);
            }
            else
            {
                return rb_big2ulong(b, caller);
            }
        }

        internal static ulong rb_big2uquad(Bignum b, Frame caller)
        {
            if (b.value > ulong.MaxValue || b.value < ulong.MinValue)
            {
                throw new RangeError("bignum too big to convert into `quad int'").raise(caller);
            }

            return b.value.ToUInt64();
        }

        internal static ulong rb_big2uquad_pack(Bignum b, Frame caller)
        {
            if (b.value < 0)
            {
                Bignum positiveBignum = new Bignum(b.value * -1);
                return ((ulong)((long)rb_big2uquad(positiveBignum, caller) * -1) & 0xFFFFFFFFFFFFFFFF);
            }
            else
            {
                return rb_big2uquad(b, caller);
            }
        }

        internal static object rb_uint2inum(uint n, Frame caller)
        {
            if (n < int.MaxValue)
            {
                return (int)n;
            }
            else
            {
                return new Bignum(n);
            }
        }
        
        internal static object rb_str2num(string str, Frame caller, int numBase, bool badCheck)
        {
            bool positive = true;
            //int len = 1;
            char nonDigit = (char)0;
            byte charValue = 0;
            bool isNonDigit = false;
            object num = 0;
            int pos = 0;
            string s = str;

            if (str == null || str.Length == 0)
            {
                if (badCheck) goto bad;
                return 0;
            }
            if (badCheck)
            {
                str = str.TrimStart(' ');
            }
            else
            {
                str = str.TrimStart(' ', '_');
            }
            if (str[0] == '+')
            {
                str = str.Remove(0, 1);
            }
            else if (str[0] == '-')
            {
                str = str.Remove(0, 1);
                positive = false;
            }
            if (str[0] == '+' || str[0] == '-')
            {
                if (badCheck) goto bad;
                return 0;
            }
            if (numBase <= 0)
            {
                if (str[0] == '0')
                {
                    switch (str[1])
                    {
                        case 'x':
                            numBase = 16;
                            break;
                        case 'X':
                            numBase = 16;
                            break;
                        case 'b':
                            numBase = 2;
                            break;
                        case 'B':
                            numBase = 2;
                            break;
                        case 'o':
                            numBase = 8;
                            break;
                        case 'O':
                            numBase = 8;
                            break;
                        case 'd':
                            numBase = 10;
                            break;
                        case 'D':
                            numBase = 10;
                            break;
                        default:
                            numBase = 8;
                            break;
                    }
                }
                else if (numBase < -1)
                {
                    numBase = -numBase;
                }
                else
                {
                    numBase = 10;
                }
            }
            //Note: if len is not used, this switch can be
            //shortened.
            switch (numBase)
            {
                case 2:
                    //len = 1;
                    if (str[0] == '0' && (str[1] == 'b' || str[1] == 'B'))
                    {
                        str = str.Remove(0, 2);
                    }
                    break;

                case 3:
                    //len = 2;
                    break;

                case 8:
                    if (str[0] == '0' && (str[1] == 'o' || str[1] == 'O'))
                    {
                        str = str.Remove(0, 2);
                    }
                    //len = 3;
                    break;
                case 4:
                    //len = 3;
                    break;
                case 5:
                    //len = 3;
                    break;
                case 6:
                    //len = 3;
                    break;
                case 7:
                    //len = 3;
                    break;

                case 10:
                    if (str[0] == '0' && str.Length > 1 && (str[1] == 'd' || str[1] == 'D'))
                    {
                        str = str.Remove(0, 2);
                    }
                    //len = 4;
                    break;
                case 9:
                    //len = 4;
                    break;
                case 11:
                    //len = 4;
                    break;
                case 12:
                    //len = 4;
                    break;
                case 13:
                    //len = 4;
                    break;
                case 14:
                    //len = 4;
                    break;
                case 15:
                    //len = 4;
                    break;
                case 16:
                    //len = 4;
                    if (str[0] == '0' && str.Length > 1 && (str[1] == 'x' || str[1] == 'X'))
                    {
                        str = str.Remove(0, 2);
                    }
                    break;
                default:
                    if (numBase < 2 || 36 < numBase)
                    {
                        throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "illegal radix {0}", numBase)).raise(caller);
                    } 
                    if (numBase <= 32)
                    {
                        //len = 5;
                    }
                    else
                    {
                        //len = 6;
                    }
                    break;
            }


            if (badCheck && str[0] == '_')
            {
                goto bad;
            }
            //Not very efficient, should make use of 'len' to perform the 
            //calculation more quickly if result will fit into uint. 
            foreach (char c in str)
            {
                pos++;

                if (c == '_')
                {
                    if (badCheck)
                    {
                        if (isNonDigit) goto bad;
                        nonDigit = c;
                        isNonDigit = true;
                    }
                    continue;
                }
                else if (char.IsDigit(c))
                {
                    charValue = (byte)((byte)c - (byte)'0');
                }
                else if (char.IsLetter(c))
                {
                    if (char.IsLower(c))
                    {
                        charValue = (byte)((byte)c - ((byte)'a' - (byte)10));
                    }
                    else if (char.IsUpper(c))
                    {
                        charValue = (byte)((byte)c - ((byte)'A' - (byte)10));
                    }
                }
                else
                {
                    break;
                }
                if (charValue >= numBase) break;

                num = Eval.CallPrivate(num, caller, "*", null, numBase);
                num = Eval.CallPrivate(num, caller, "+", null, (int)charValue);
            }
            if (!positive)
            {
                num = Eval.CallPrivate(num, caller, "*", null, -1);
            }
            if (badCheck)
            {
                //TODO: Some checking to do here. 
            }
        bad:
            //TODO:
            //rb_invalid_str(s, "Integer");
            return num;
        }
    }
}
