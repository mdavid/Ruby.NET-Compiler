/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using Ruby.Runtime;
using System.Globalization;

namespace Ruby.Methods
{
    
    internal class rb_big_uminus : MethodBody0 // status: done
    {
        internal static rb_big_uminus singleton = new rb_big_uminus();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Bignum.NormaliseUsing(-((Bignum)recv).value);
        }
    }

    
    internal class rb_big_plus : MethodBody1 // status: done
    {
        internal static rb_big_plus singleton = new rb_big_plus();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return Bignum.NormaliseUsing(((Bignum)recv).value + (int)param0);
            }
            else if (param0 is Bignum)
            {
                return Bignum.NormaliseUsing(((Bignum)recv).value + ((Bignum)param0).value);
            }
            else if (param0 is Float)
            {
                return new Float((double)((Bignum)recv).value + ((Float)param0).value);
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "+", caller);
            }
        }
    }

    
    internal class rb_big_minus : MethodBody1 // status: done
    {
        internal static rb_big_minus singleton = new rb_big_minus();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return Bignum.NormaliseUsing(((Bignum)recv).value - (int)param0);
            }
            else if (param0 is Bignum)
            {
                return Bignum.NormaliseUsing(((Bignum)recv).value - ((Bignum)param0).value);
            }
            else if (param0 is Float)
            {
                return new Float((double)((Bignum)recv).value - ((Float)param0).value);
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "-", caller);
            }
        }
    }

    
    internal class rb_big_mul : MethodBody1 // status: done
    {
        internal static rb_big_mul singleton = new rb_big_mul();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (recv is int)
                recv = new Bignum((double)(int)recv);

            if (param0 is int)
            {
                return Bignum.NormaliseUsing(((Bignum)recv).value * (int)param0);
            }
            else if (param0 is Bignum)
            {
                return Bignum.NormaliseUsing(((Bignum)recv).value * ((Bignum)param0).value);
            }
            else if (param0 is Float)
            {
                return new Float((double)((Bignum)recv).value * ((Float)param0).value);
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "*", caller);
            }
        }
    }

    
    internal class rb_big_div : MethodBody1 // status: done
    {
        internal static rb_big_div singleton = new rb_big_div();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (object.Equals(param0, 0))
                throw new ZeroDivisionError("divided by 0").raise(caller);

            if (param0 is int)
            {
                return Bignum.NormaliseUsing(((Bignum)recv).value / (int)param0);
            }
            else if (param0 is Bignum)
            {
                return Bignum.NormaliseUsing(((Bignum)recv).value / ((Bignum)param0).value);
            }
            else if (param0 is Float)
            {
                return new Float((double)((Bignum)recv).value / ((Float)param0).value);
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "/", caller);
            }
        }
    }

    
    internal class rb_big_modulo : MethodBody1 // status: done
    {
        internal static rb_big_modulo singleton = new rb_big_modulo();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return Bignum.NormaliseUsing(((Bignum)recv).value % (int)param0);
            }
            else if (param0 is Bignum)
            {
                return Bignum.NormaliseUsing(((Bignum)recv).value % ((Bignum)param0).value);
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "%", caller);
            }
        }
    }

    
    internal class rb_big_remainder : MethodBody1 // status: done
    {
        internal static rb_big_remainder singleton = new rb_big_remainder();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                IronMath.integer mod;
                IronMath.integer.divrem(((Bignum)recv).value,  (int)param0, out mod); 
                return Bignum.NormaliseUsing(mod);
            }
            else if (param0 is Bignum)
            {
                IronMath.integer mod;
                IronMath.integer.divrem(((Bignum)recv).value, ((Bignum)param0).value, out mod);
                return Bignum.NormaliseUsing(mod);
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "remainder", caller);
            }
        }
    }

    
    internal class rb_big_divmod : MethodBody1 // status: done
    {
        internal static rb_big_divmod singleton = new rb_big_divmod();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                IronMath.integer remainder;
                IronMath.integer division = IronMath.integer.divmod(((Bignum)recv).value, (int)param0, out remainder);
                return new Array(Bignum.NormaliseUsing(division), Bignum.NormaliseUsing(remainder));
            }
            else if (param0 is Bignum)
            {
                IronMath.integer remainder;
                IronMath.integer division = IronMath.integer.divmod(((Bignum)recv).value, ((Bignum)param0).value, out remainder);
                return new Array(Bignum.NormaliseUsing(division), Bignum.NormaliseUsing(remainder));
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "divmod", caller);
            }
        }
    }

    
    internal class rb_big_quo : MethodBody1 // status: done
    {
        internal static rb_big_quo singleton = new rb_big_quo();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return new Float((double)((Bignum)recv).value / (double)(int)param0);
            }
            else if (param0 is Bignum)
            {
                return new Float((double)((Bignum)recv).value / (double)((Bignum)param0).value);
            }
            else if (param0 is Float)
            {
                return new Float((double)((Bignum)recv).value / ((Float)param0).value);
            }
            else
            {
                return Numeric.rb_num_coerce_bin(recv, param0, "quo", caller);
            }
        }
    }

    
    internal class rb_big_pow : MethodBody1 // author:cjs, status: done
    {
        internal static rb_big_pow singleton = new rb_big_pow();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object y)
        {
            object x = recv;

            if (y is int)
            {
                return new Bignum(((Bignum)x).value.pow((int)y));
            }
            else if (y is Bignum)
            {
                Errors.rb_warn("in a**b, b may be too big");
                return new Float(System.Math.Pow(((Bignum)x).value.ToFloat64(), ((Bignum)y).value.ToFloat64()));
            }
            else if (y is Float)
            {
                return new Float(System.Math.Pow(((Bignum)x).value.ToFloat64(), ((Float)y).value));
            }
            else
            {
                return Numeric.rb_num_coerce_bin(x, y, "pow", caller);
            }
        }
    }

    
    internal class rb_big_and : MethodBody1 // status: done
    {
        internal static rb_big_and singleton = new rb_big_and();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            IronMath.integer y;
            object intValue = Integer.rb_to_int(param0, caller);

            if (intValue is int)
                y = (int)intValue;
            else
                y = ((Bignum)intValue).value;

            return Bignum.NormaliseUsing(((Bignum)recv).value & y);
        }
    }

    
    internal class rb_big_or : MethodBody1 // status: done
    {
        internal static rb_big_or singleton = new rb_big_or();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            IronMath.integer y;
            object intValue = Integer.rb_to_int(param0, caller);

            if (intValue is int)
                y = (int)intValue;
            else
                y = ((Bignum)intValue).value;

            return Bignum.NormaliseUsing(((Bignum)recv).value | y);
        }
    }

    
    internal class rb_big_xor : MethodBody1 // status: done
    {
        internal static rb_big_xor singleton = new rb_big_xor();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            IronMath.integer y;
            object intValue = Integer.rb_to_int(param0, caller);

            if (intValue is int)
                y = (int)intValue;
            else
                y = ((Bignum)intValue).value;

            return Bignum.NormaliseUsing(((Bignum)recv).value ^ y);
        }
    }

    
    internal class rb_big_neg : MethodBody0 // status: done
    {
        internal static rb_big_neg singleton = new rb_big_neg();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Bignum.NormaliseUsing(~((Bignum)recv).value);
        }
    }

    
    internal class rb_big_lshift : MethodBody1 // status: done
    {
        internal static rb_big_lshift singleton = new rb_big_lshift();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            int shift = Numeric.rb_num2long(param0, caller);
            return Bignum.NormaliseUsing(((Bignum)recv).value << shift);
        }
    }

    
    internal class rb_big_rshift : MethodBody1 // status: done
    {
        internal static rb_big_rshift singleton = new rb_big_rshift();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            int shift = Numeric.rb_num2long(param0, caller);
            return Bignum.NormaliseUsing(((Bignum)recv).value >> shift);
        }
    }

    
    internal class rb_big_to_f : MethodBody0 // status: done
    {
        internal static rb_big_to_f singleton = new rb_big_to_f();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Float((double)((Bignum)recv).value);
        }
    }

    
    internal class rb_big_abs : MethodBody0 // status: done
    {
        internal static rb_big_abs singleton = new rb_big_abs();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Bignum.NormaliseUsing(((Bignum)recv).value.abs());
        }
    }

    
    internal class rb_big_size : MethodBody0 // status: done
    {
        internal static rb_big_size singleton = new rb_big_size();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Bignum)recv).value.length;
        }
    }

    
    internal class rb_big_hash : MethodBody0 // status: done
    {
        internal static rb_big_hash singleton = new rb_big_hash();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return recv.GetHashCode();
        }
    }

    
    internal class rb_big_aref : MethodBody1 // status: done
    {
        internal static rb_big_aref singleton = new rb_big_aref();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            IronMath.integer val = ((Bignum)recv).value;
            int i;

            if (param0 is Bignum)
            {
                return (val < 0) ? 1 : 0;
            }

            i = Numeric.rb_num2long(param0, caller);

            if (val.length * sizeof(int) * 8 - 1 < i)
                return (val < 0) ? 1 : 0;

            return (val & (IronMath.integer.ONE << i)) > 0 ? 1 : 0;
        }
    }

    
    internal class rb_big_cmp : MethodBody1 // status: done
    {
        internal static rb_big_cmp singleton = new rb_big_cmp();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return ((Bignum)recv).value.CompareTo((IronMath.integer)(int)param0);
            }
            else if (param0 is Bignum)
            {
                return ((Bignum)recv).value.CompareTo(((Bignum)param0).value);
            }
            else if (param0 is Float)
            {
                return Float.rb_dbl_cmp((double)((Bignum)recv).value, ((Float)param0).value);
            }
            else
            {
                return Numeric.rb_num_coerce_cmp(recv, param0, "<=>", caller);
            }
        }
    }

    
    internal class rb_big_eq : MethodBody1 // status: done
    {
        internal static rb_big_eq singleton = new rb_big_eq();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return ((Bignum)recv).value == (int)param0;
            }
            else if (param0 is Bignum)
            {
                return ((Bignum)recv).value == ((Bignum)param0).value;
            }
            else if (param0 is Float)
            {
                return (double)((Bignum)recv).value == ((Float)param0).value;
            }
            else
            {
                return Object.Equal(param0, recv, caller);
            }
        }
    }

    
    internal class rb_big_eql : MethodBody1 // status: done
    {
        internal static rb_big_eql singleton = new rb_big_eql();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is Bignum)
            {
                return ((Bignum)recv).value == ((Bignum)param0).value;
            }

            return false;
        }
    }

    
    internal class rb_big_coerce : MethodBody1 // status: done
    {
        internal static rb_big_coerce singleton = new rb_big_coerce();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return new Array(new Bignum((int)param0), recv);
            }
            else
            {
                throw new TypeError(string.Format(CultureInfo.InvariantCulture, "Can't coerce {0} to Bignum", Class.CLASS_OF(param0)._name)).raise(caller);
            }
        }
    }

    
    internal class rb_big_to_s : VarArgMethodBody0 // status: done
    {
        internal static rb_big_to_s singleton = new rb_big_to_s();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            if (rest.Count == 0)
                return new String(((Bignum)recv).value.ToString());

            uint radix = (uint)Numeric.rb_num2long(rest.value[0], caller);

            try
            {
                return new String(((Bignum)recv).value.ToString(radix));
            }
            catch (System.ArgumentOutOfRangeException)
            {
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "illegal radix {0}", radix)).raise(caller);
            }
        }
    }
}
