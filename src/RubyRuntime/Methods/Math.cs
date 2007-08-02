/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using Ruby.Runtime;

namespace Ruby.Methods
{
    
    internal class math_atan2 : MethodBody2 //status: done
    {
        internal static math_atan2 singleton = new math_atan2();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object param0, object param1)
        {
            double y = Float.rb_Float(param0, caller).value;
            double x = Float.rb_Float(param1, caller).value;
            return new Float(System.Math.Atan2(y, x));
        }
    }

    
    internal class math_cos : MethodBody1 //status: done
    {
        internal static math_cos singleton = new math_cos();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = Float.rb_Float(param0, caller).value;
            return new Float(System.Math.Cos(x));
        }
    }

    
    internal class math_sin : MethodBody1 //status: done
    {
        internal static math_sin singleton = new math_sin();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = Float.rb_Float(param0, caller).value;
            return new Float(System.Math.Sin(x));
        }
    }

    
    internal class math_tan : MethodBody1 //status: done
    {
        internal static math_tan singleton = new math_tan();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = Float.rb_Float(param0, caller).value;
            return new Float(System.Math.Tan(x));
        }
    }

    
    internal class math_acos : MethodBody1 //status: done
    {
        internal static math_acos singleton = new math_acos();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = Float.rb_Float(param0, caller).value;
            return new Float(System.Math.Acos(x));
        }
    }

    
    internal class math_asin : MethodBody1 //status: done
    {
        internal static math_asin singleton = new math_asin();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = Float.rb_Float(param0, caller).value;
            return new Float(System.Math.Asin(x));
        }
    }

    
    internal class math_atan : MethodBody1 //status: done
    {
        internal static math_atan singleton = new math_atan();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = Float.rb_Float(param0, caller).value;
            return new Float(System.Math.Atan(x));
        }
    }

    
    internal class math_cosh : MethodBody1 //status: done
    {
        internal static math_cosh singleton = new math_cosh();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = Float.rb_Float(param0, caller).value;
            return new Float(System.Math.Cosh(x));
        }
    }

    
    internal class math_sinh : MethodBody1 //status: done
    {
        internal static math_sinh singleton = new math_sinh();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = Float.rb_Float(param0, caller).value;
            return new Float(System.Math.Sinh(x));
        }
    }

    
    internal class math_tanh : MethodBody1 //status: done
    {
        internal static math_tanh singleton = new math_tanh();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = Float.rb_Float(param0, caller).value;
            return new Float(System.Math.Tanh(x));
        }
    }

    
    internal class math_acosh : MethodBody1 // author: cjs, status: done
    {
        internal static math_acosh singleton = new math_acosh();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double d = Numeric.rb_num2dbl(param0, caller);
            //2*log(sqrt((x+1)/2) + sqrt((x-1)/2)) 
            return new Float(2 * System.Math.Log(System.Math.Sqrt((d + 1) / 2) + System.Math.Sqrt((d - 1) / 2)));
        }
    }

    
    internal class math_asinh : MethodBody1 // author: cjs, status: done
    {
        internal static math_asinh singleton = new math_asinh();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double d = Numeric.rb_num2dbl(param0, caller);
            //sign(x) * log(|x| + sqrt(x*x+1))
            return new Float(d / System.Math.Abs(d) * System.Math.Log(System.Math.Abs(d) + System.Math.Sqrt(d * d + 1)));
        }
    }

    
    internal class math_atanh : MethodBody1 // author: cjs, status: done
    {
        internal static math_atanh singleton = new math_atanh();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double d = Numeric.rb_num2dbl(param0, caller);

            if (System.Math.Abs(d) > 1)
            {
                Errno.errno = Errno.EDOM;
                throw SystemCallError.rb_sys_fail("Domain error - atanh", caller).raise(caller);
            }

            return new Float(System.Math.Log((1 + d) / (1 - d)) / 2);
        }
    }

    
    internal class math_exp : MethodBody1 //status: done
    {
        internal static math_exp singleton = new math_exp();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = Float.rb_Float(param0, caller).value;
            return new Float(System.Math.Exp(x));
        }
    }

    
    internal class math_log : MethodBody1 //status: done
    {
        internal static math_log singleton = new math_log();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = Float.rb_Float(param0, caller).value;
            return new Float(System.Math.Log(x));
        }
    }

    
    internal class math_log10 : MethodBody1 //status: done
    {
        internal static math_log10 singleton = new math_log10();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = Float.rb_Float(param0, caller).value;
            return new Float(System.Math.Log10(x));
        }
    }

    
    internal class math_sqrt : MethodBody1 // author: cjs, status: done
    {
        internal static math_sqrt singleton = new math_sqrt();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = Float.rb_Float(param0, caller).value;
            return new Float(System.Math.Sqrt(x));
        }
    }

    
    internal class math_frexp : MethodBody1 // author: cjs, status: done
    {
        internal static math_frexp singleton = new math_frexp();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            double d = Numeric.rb_num2dbl(p1, caller);

            long bits = System.BitConverter.DoubleToInt64Bits(d);

            bool negative = (bits < 0);
            int exponent = (int)((bits >> 52) & 0x7FFL) - 1022;
            long mantissa = bits & 0xFFFFFFFFFFFFFL;

            double mant = System.BitConverter.Int64BitsToDouble(mantissa | 0x3FF0000000000000L) / 2;
            mant = negative ? -mant : mant;

            return new Array(new object[] { new Float(mant), exponent });
        }
    }

    
    internal class math_ldexp : MethodBody2 // author: cjs, status: done
    {
        internal static math_ldexp singleton = new math_ldexp();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            double mantissa = Numeric.rb_num2dbl(p1, caller);
            int exponent = Numeric.rb_num2long(Integer.rb_Integer(p2, caller), caller);
            return new Float(mantissa * System.Math.Pow(2, exponent));
        }
    }

    
    internal class math_hypot : MethodBody2 // author: cjs, status: done
    {
        internal static math_hypot singleton = new math_hypot();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            double x = Float.rb_Float(p1, caller).value;
            double y = Float.rb_Float(p2, caller).value;
            return new Float(System.Math.Sqrt(x*x+y*y));
        }
    }

    
    internal class math_erf : MethodBody1 // author: war, status: done
    {
        internal static math_erf singleton = new math_erf();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            Float f = Float.rb_Float(param0, caller);
            return new Float(Math.erf(f.value));            
        }
    }

    
    internal class math_erfc : MethodBody1 // author: war, status: done
    {
        internal static math_erfc singleton = new math_erfc();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            Float f = Float.rb_Float(param0, caller);
            return new Float(1-Math.erf(f.value));            
        }
    }
}
