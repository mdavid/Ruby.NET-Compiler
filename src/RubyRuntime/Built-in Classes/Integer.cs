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

    public abstract partial class Integer : Numeric
    {
        public Integer(Class klass)
            : base(klass)
        {
        }

        // ----------------------------------------------------------------------------


        internal static object rb_to_int(object val, Frame caller) // status: done
        {
            return rb_to_integer(val, "to_int", caller);
        }


        private static object rb_to_integer(object val, string method, Frame caller) // status: done
        {
            object result = Object.Convert<Integer>(val, method, true, caller);
            if (!(result is int || result is Integer))
            {
                throw new TypeError(string.Format(CultureInfo.InvariantCulture, "{0}#{1} should return Integer",
                    Class.CLASS_OF(val), method)).raise(caller);
            }
            else
            {
                return result;
            }
        }

        internal static object rb_Integer(object val, Frame caller) // status: done
        {
            if (val is Float)
            {
                double fval = ((Float)val).value;

                if (double.IsInfinity(fval))
                    throw new FloatDomainError("Infinity").raise(caller);

                if (double.IsNaN(fval))
                    throw new FloatDomainError("NaN").raise(caller);

                if (fval < Numeric.FIXNUM_MIN || fval > Numeric.FIXNUM_MAX)
                    return new Bignum(fval);
            }
            else if (val is int)
            {
                return Bignum.Normalise(val);
            }
            else if (val is Integer)
            {
                return val;
            }
            else if (val is String)
            {
                return Bignum.rb_str2num(((String)val).value, caller, 0, true);
            }

            object tmp = Object.Convert<Integer>(val, "to_int", false, caller);
            if (tmp == null)
            {
                return Integer.rb_to_integer(val, "to_i", caller);
            }
            else
            {
                return tmp;
            }
        }
    }
}

