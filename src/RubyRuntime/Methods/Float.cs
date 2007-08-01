/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;

namespace Ruby.Methods
{
    
    internal class rb_flo_induced_from : MethodBody1 // author: cjs, status: done
    {
        internal static rb_flo_induced_from singleton = new rb_flo_induced_from();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (p1 is Float)
            {
                return p1;
            }
            else if (p1 is int || p1 is Bignum)
            {
                return Eval.CallPrivate(p1, caller, "to_f", null);
            }
            else
            {
                throw new TypeError(string.Format("failed to convert {0} into Float", Class.rb_obj_classname(p1))).raise(caller);
            }
        }
    }


    
    internal class flo_coerce : MethodBody1 // status: done
    {
        internal static flo_coerce singleton = new flo_coerce();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return new Array(Float.rb_Float(param0, caller), recv);
        }
    }


    
    internal class flo_uminus : MethodBody0 // status: done
    {
        internal static flo_uminus singleton = new flo_uminus();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Float(-((Float)recv).value);
        }
    }


    
    internal class flo_plus : MethodBody1 // author: cjs, status: done
    {
        internal static flo_plus singleton = new flo_plus();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return new Float(((Float)recv).value + (double)(int)param0);
            }
            else if (param0 is Float)
            {
                return new Float(((Float)recv).value + ((Float)param0).value);
            }
            else if (param0 is Bignum)
            {
                return new Float(((Float)recv).value + (double)((Bignum)param0).value);
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "+", caller);
            }
        }
    }


    
    internal class flo_minus : MethodBody1 // author: cjs, status: done
    {
        internal static flo_minus singleton = new flo_minus();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return new Float(((Float)recv).value - (double)(int)param0);
            }
            else if (param0 is Float)
            {
                return new Float(((Float)recv).value - ((Float)param0).value);
            }
            else if (param0 is Bignum)
            {
                return new Float(((Float)recv).value - (double)((Bignum)param0).value);
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "-", caller);
            }
        }
    }


    
    internal class flo_mul : MethodBody1 // author: cjs, status: done
    {
        internal static flo_mul singleton = new flo_mul();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return new Float(((Float)recv).value * (double)(int)param0);
            }
            else if (param0 is Float)
            {
                return new Float(((Float)recv).value * ((Float)param0).value);
            }
            else if (param0 is Bignum)
            {
                return new Float(((Float)recv).value * (double)((Bignum)param0).value);
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "*", caller);
            }
        }
    }


    
    internal class flo_div : MethodBody1 // author: cjs, status: done
    {
        internal static flo_div singleton = new flo_div();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return new Float(((Float)recv).value / (double)(int)param0);
            }
            else if (param0 is Float)
            {
                return new Float(((Float)recv).value / ((Float)param0).value);
            }
            else if (param0 is Bignum)
            {
                return new Float(((Float)recv).value / (double)((Bignum)param0).value);
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "/", caller);
            }
        }
    }


    
    internal class flo_mod : MethodBody1 // author: cjs, status: done
    {
        internal static flo_mod singleton = new flo_mod();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double y;

            if (param0 is int)
            {
                y = (double)(int)param0;
            }
            else if (param0 is Float)
            {
                y = ((Float)param0).value;
            }
            else if (param0 is Bignum)
            {
                y = (double)((Bignum)param0).value;
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "%", caller);
            }

            return new Float(((Float)recv).value % y);
        }
    }


    
    internal class flo_divmod : MethodBody1 // author: cjs, status: done
    {
        internal static flo_divmod singleton = new flo_divmod();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            double x = ((Float)recv).value;
            double y;

            if (param0 is int)
            {
                y = (double)(int)param0;
            }
            else if (param0 is Float)
            {
                y = ((Float)param0).value;
            }
            else if (param0 is Bignum)
            {
                y = (double)((Bignum)param0).value;
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "divmod", caller);
            }

            if (y == 0.0)
            {
                return new Array(new Float(double.NaN), new Float(double.NaN));
            }

            return new Array(new Float((int)(x / y)), new Float(x % y));
        }
    }


    
    internal class flo_pow : MethodBody1 // author: cjs, status: done
    {
        internal static flo_pow singleton = new flo_pow();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return new Float(System.Math.Pow(((Float)recv).value, (double)(int)param0));
            }
            else if (param0 is Float)
            {
                return new Float(System.Math.Pow(((Float)recv).value, ((Float)param0).value));
            }
            else if (param0 is Bignum)
            {
                return new Float(System.Math.Pow(((Float)recv).value, (double)((Bignum)param0).value));
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "**", caller);
            }
        }
    }


    
    internal class flo_eq : MethodBody1 // author: cjs, status: done
    {
        internal static flo_eq singleton = new flo_eq();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return ((Float)recv).value == (double)(int)param0;
            }
            else if (param0 is Float)
            {
                return ((Float)recv).value == ((Float)param0).value;
            }
            else if (param0 is Bignum)
            {
                return ((Float)recv).value == (double)((Bignum)param0).value;
            }
            else
            {
                if (recv == param0)
                    return true;
                else
                    return Eval.CallPrivate(param0, caller, "==", null, recv);
            }
        }
    }


    
    internal class flo_gt : MethodBody1 // author: cjs, status: done
    {
        internal static flo_gt singleton = new flo_gt();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return ((Float)recv).value > (double)(int)param0;
            }
            else if (param0 is Float)
            {
                return ((Float)recv).value > ((Float)param0).value;
            }
            else if (param0 is Bignum)
            {
                return ((Float)recv).value > (double)((Bignum)param0).value;
            }
            else
            {
                return Numeric.rb_num_coerce_relop(recv, param0, ">", caller);
            }
        }
    }


    
    internal class flo_ge : MethodBody1 // author: cjs, status: done
    {
        internal static flo_ge singleton = new flo_ge();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return ((Float)recv).value >= (double)(int)param0;
            }
            else if (param0 is Float)
            {
                return ((Float)recv).value >= ((Float)param0).value;
            }
            else if (param0 is Bignum)
            {
                return ((Float)recv).value >= (double)((Bignum)param0).value;
            }
            else
            {
                return Numeric.rb_num_coerce_relop(recv, param0, ">=", caller);
            }
        }
    }


    
    internal class flo_lt : MethodBody1 // author: cjs, status: done
    {
        internal static flo_lt singleton = new flo_lt();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return ((Float)recv).value < (double)(int)param0;
            }
            else if (param0 is Float)
            {
                bool result = ((Float)recv).value < ((Float)param0).value;
                return result;
            }
            else if (param0 is Bignum)
            {
                return ((Float)recv).value < (double)((Bignum)param0).value;
            }
            else
            {
                return Numeric.rb_num_coerce_relop(recv, param0, "<", caller);
            }
        }
    }


    
    internal class flo_le : MethodBody1 // author: cjs, status: done
    {
        internal static flo_le singleton = new flo_le();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                bool r =  ((Float)recv).value <= (double)(int)param0;
                return r;
            }
            else if (param0 is Float)
            {
                bool r = ((Float)recv).value <= ((Float)param0).value;
                return r;
            }
            else if (param0 is Bignum)
            {
                return ((Float)recv).value <= (double)((Bignum)param0).value;
            }
            else
            {
                return Numeric.rb_num_coerce_relop(recv, param0, "<=", caller);
            }
        }
    }


    
    internal class flo_eql : MethodBody1 // status: done
    {
        internal static flo_eql singleton = new flo_eql();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is Float)
            {
                return ((Float)recv).value == ((Float)param0).value;
            }

            return false;
        }
    }


    
    internal class flo_to_f : MethodBody0 // status: done
    {
        internal static flo_to_f singleton = new flo_to_f();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return recv;
        }
    }


    
    internal class flo_abs : MethodBody0 // status: done
    {
        internal static flo_abs singleton = new flo_abs();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Float(System.Math.Abs(((Float)recv).value));
        }
    }


    
    internal class flo_zero_p : MethodBody0 // status: done
    {
        internal static flo_zero_p singleton = new flo_zero_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Float)recv).value == 0.0;
        }
    }


    
    internal class flo_floor : MethodBody0 // author: cjs, status: done
    {
        internal static flo_floor singleton = new flo_floor();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            double val = System.Math.Floor(((Float)recv).value);

            if (!Numeric.FIXABLE(val))
                return new Bignum(val);

            return (int)val;
        }
    }


    
    internal class flo_ceil : MethodBody0 // author: cjs, status: done
    {
        internal static flo_ceil singleton = new flo_ceil();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            double val = System.Math.Ceiling(((Float)recv).value);

            if (!Numeric.FIXABLE(val))
                return new Bignum(val);

            return (int)val;
        }
    }


    
    internal class flo_round : MethodBody0 // author: cjs, status: done
    {
        internal static flo_round singleton = new flo_round();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            double val = System.Math.Round(((Float)recv).value, System.MidpointRounding.AwayFromZero);

            if (!Numeric.FIXABLE(val))
                return new Bignum(val);

            return (int)val;
        }
    }


    
    internal class flo_truncate : MethodBody0 // author: cjs, status: done
    {
        internal static flo_truncate singleton = new flo_truncate();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            double val = System.Math.Truncate(((Float)recv).value);

            if (!Numeric.FIXABLE(val))
                return new Bignum(val);

            return (int)val;
        }
    }


    
    internal class flo_is_nan_p : MethodBody0 // status: done
    {
        internal static flo_is_nan_p singleton = new flo_is_nan_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return double.IsNaN(((Float)recv).value);
        }
    }


    
    internal class flo_is_infinite_p : MethodBody0 // status: done
    {
        internal static flo_is_infinite_p singleton = new flo_is_infinite_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            double value = ((Float)recv).value;

            if (double.IsInfinity(value))
                return value < 0 ? -1 : 1;

            return null;
        }
    }


    
    internal class flo_is_finite_p : MethodBody0 // status: done
    {
        internal static flo_is_finite_p singleton = new flo_is_finite_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return !double.IsInfinity(((Float)recv).value);
        }
    }

    
    internal class flo_cmp : MethodBody1 // status: done
    {
        internal static flo_cmp singleton = new flo_cmp();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return Float.rb_dbl_cmp(((Float)recv).value, (double)(int)param0);
            }
            else if (param0 is Bignum)
            {
                return Float.rb_dbl_cmp(((Float)recv).value, (double)((Bignum)param0).value);
            }
            else if (param0 is Float)
            {
                return Float.rb_dbl_cmp(((Float)recv).value, ((Float)param0).value);
            }
            else
            {
                return Numeric.rb_num_coerce_cmp(recv, param0, "<=>", caller);
            }
        }
    }

    
    internal class flo_hash : MethodBody0 // status: done
    {
        internal static flo_hash singleton = new flo_hash();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Float)recv).value.GetHashCode();
        }
    }

    
    internal class flo_to_s : MethodBody0 // author: cjs, status: done
    {
        internal static flo_to_s singleton = new flo_to_s();
       
        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            double val = ((Float)recv).value;

            string number;
            if (val != 0.0 && 
                !double.IsInfinity(val) &&
                !double.IsNaN(val) &&
                ((val >= 99999999999999.9454 || val < -99999999999999.9454) || 
                 (val < 0.00009999999999999994 && val > -0.00009999999999999994))
                )
            {
                //in this range use exponential notation
                number = val.ToString("e14"); 
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
            }
            else
            {
                number = val.ToString("0.0#################");
            }

            return new String(number);
        }
    }
}
