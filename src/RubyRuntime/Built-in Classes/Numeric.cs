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

    public partial class Numeric : Basic
    {
        internal const int FLT_RADIX = 2;
        internal const int FLT_ROUNDS = 1;

        internal const int DBL_MANT_DIG = 53;
        internal const int DBL_DIG = 15;
        internal const int DBL_MIN_EXP = -1024;
        internal const int DBL_MAX_EXP = 1024;
        internal const int DBL_MIN_10_EXP = -307;
        internal const int DBL_MAX_10_EXP = 308;

        internal const double DBL_MIN = 2.2250738585072014e-308;
        internal const double DBL_MAX = 1.7976931348623158e+308;
        internal const double DBL_EPSILON = 2.2204460492503131e-016;

        internal const int FIXNUM_MIN = -1073741824;
        internal const int FIXNUM_MAX = 1073741823;

        internal static int seed = 0;
        internal static System.Random random = null;

        // ------------------------------------------------------------------------------

        internal Numeric(): base(Ruby.Runtime.Init.rb_cNumeric)
        {
        }

        internal Numeric(Class klass): base(klass)
        {
        }


        // ------------------------------------------------------------------------------

        internal static bool do_coerce(ref object x, ref object y, bool raise_error, Frame caller)
        {
            try
            {
                Array result = (Array)Eval.CallPrivate(y, caller, "coerce", null, x);
                x = result[0];
                y = result[1];
                return true;
            }
            catch
            {
                if (raise_error)
                    throw new TypeError(string.Format(CultureInfo.InvariantCulture, "{0} can't be coerced into {1}", Class.rb_obj_classname(y), Class.rb_obj_classname(x))).raise(caller); 

                return false;
            }
        }

        internal static object rb_num_coerce_relop(object self, object y, string func, Frame caller) // status: done
        {
            object ret = null;

            if (!do_coerce(ref self, ref y, false, caller)
                || null == (ret = Eval.CallPrivate(self, caller, func, null, y)))
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "comparison of {0} with {1} failed", self, y)).raise(caller);

            return ret;
        }

        internal static object rb_num_coerce_bin(object self, object y, string func, Frame caller)
        {
            Numeric.do_coerce(ref self, ref y, true, caller);
            return Eval.CallPrivate(self, caller, func, null, y);
        }

        internal static object rb_num_coerce_cmp(object self, object y, string func, Frame caller)
        {
            if (do_coerce(ref self, ref y, false, caller))
                return Eval.CallPrivate(self, caller, func, null, y);
            else
                return null;
        }

        // originally in object.c
        internal static double rb_num2dbl(object o, Frame caller)
        {
            if (o == null)
            {
                throw new TypeError("no implicit conversion to float from nil").raise(caller);
            }
            else if (o is String)
            {
                throw new TypeError("no implicit conversion to float from string").raise(caller);
            }
            else if (o is Float)
            {
                return ((Float)o).value;
            }
            else
            {
                return Float.rb_Float(o, caller).value;
            }
        }


        internal static int rb_num2long(object o, Frame caller) // status: done
        {
            if (o == null)
            {
                throw new TypeError("no implicit conversion from nil to integer").raise(caller);
            }
            else if (o is int)
            {
                return (int)o;
            }
            else if (o is Float)
            {
                try
                {
                    return checked((int)((Float)o).value);
                }
                catch (System.OverflowException)
                {
                    throw new RangeError(string.Format(CultureInfo.InvariantCulture, "float {0} out of range of integer", (double)o)).raise(caller);
                }
            }
            else if (o is Bignum)
            {
                return Bignum.rb_big2long((Bignum)o, caller);
            }
            else
            {
                object result = Integer.rb_to_int(o, caller);
                if (result is int)
                    return (int)result;
                else
                    // fortunately users can't subclass Integer - otherwise this recursion could be infinite
                    return rb_num2long(result, caller);
            }
        }

        internal static bool FIXABLE(double d)
        {
            return FIXNUM_MIN <= d && d <= FIXNUM_MAX;
        }

        internal static bool FIXABLE(int i)
        {
            return FIXNUM_MIN <= i && i <= FIXNUM_MAX;
        }
    }


    
    internal class FloatDomainError : RangeError // status: done
    {
        internal FloatDomainError(string message) : this(message, Ruby.Runtime.Init.rb_eFloatDomainError) { }

        internal FloatDomainError(string message, Class klass) : base(message, klass) { }
    }



    
    internal class ZeroDivisionError : StandardError // status: done
    {
        internal ZeroDivisionError(string message) : this(message, Ruby.Runtime.Init.rb_eZeroDivError) { }

        protected ZeroDivisionError(string message, Class klass) : base(message, klass) { }

        internal static ZeroDivisionError rb_num_zerodiv(Frame caller)
        {
            return new ZeroDivisionError("divided by 0");
        }
    }
}


