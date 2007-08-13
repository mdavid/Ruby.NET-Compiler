/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Globalization;

namespace Ruby.Methods
{
    
    internal class rb_fix_induced_from : MethodBody1 // author: cjs, status: done
    {
        internal static rb_fix_induced_from singleton = new rb_fix_induced_from();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (p1 is int)
                return p1;

            int i = Numeric.rb_num2long(p1, caller);

            if (!Numeric.FIXABLE(i))
                throw new RangeError(string.Format(CultureInfo.InvariantCulture, "integer {0} out of range of fixnum", i)).raise(caller);
            
            return i;
        }
    }


    
    internal class fix_to_s : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static fix_to_s singleton = new fix_to_s();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            int i = Numeric.rb_num2long(recv, caller);
            if (rest.Count == 0)
                return new String(i.ToString(CultureInfo.InvariantCulture));

            return rb_big_to_s.singleton.Call(last_class, new Bignum(i), caller, block, rest);
        }
    }


    
    internal class fix_id2name : MethodBody0 // author: cjs, status: done
    {
        internal static fix_id2name singleton = new fix_id2name();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            string name = Symbol.rb_id2name((uint)(int)recv);
            if (name != null)
                return new String(name);
            else
                return null;
        }
    }


    
    internal class fix_to_sym : MethodBody0 // author: cjs, status: done
    {
        internal static fix_to_sym singleton = new fix_to_sym();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            string name = Symbol.rb_id2name((uint)(int)recv);
            if (name != null)
                return new Symbol(name);
            else
                return null;
        }
    }


    
    internal class fix_uminus : MethodBody0 // status: done
    {
        internal static fix_uminus singleton = new fix_uminus();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return -(int)recv;
        }
    }


    
    internal class fix_plus : MethodBody1 // status: done
    {
        internal static fix_plus singleton = new fix_plus();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                try
                {
                    return checked((int)recv + (int)param0);
                }
                catch (System.OverflowException)
                {
                    return Bignum.NormaliseUsing(IronMath.integer.make((int)recv) + (int)param0);
                }
            }

            if (param0 is Float)
            {
                return new Float((double)(int)recv + ((Float)param0).value);
            }

            return Numeric.rb_num_coerce_bin(recv, param0, "+", caller);
        }
    }


    
    internal class fix_minus : MethodBody1 // status: done
    {
        internal static fix_minus singleton = new fix_minus();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                try
                {
                    return checked((int)recv - (int)param0);
                }
                catch (System.OverflowException)
                {
                    return Bignum.NormaliseUsing(IronMath.integer.make((int)recv) - (int)param0);
                }
            }

            if (param0 is Float)
            {
                return new Float((double)(int)recv - ((Float)param0).value);
            }

            return Numeric.rb_num_coerce_bin(recv, param0, "-", caller);
        }
    }


    
    internal class fix_mul : MethodBody1 // status: done
    {
        internal static fix_mul singleton = new fix_mul();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                try
                {
                    return checked((int)recv * (int)param0);
                }
                catch (System.OverflowException)
                {
                    return Bignum.NormaliseUsing(IronMath.integer.make((int)recv) * (int)param0);
                }
            }

            if (param0 is Float)
            {
                return new Float((double)(int)recv * ((Float)param0).value);
            }

            return Numeric.rb_num_coerce_bin(recv, param0, "*", caller);
        }
    }


    
    internal class fix_div : MethodBody1 // author: cjs, status: done
    {
        internal static fix_div singleton = new fix_div();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                int x = (int)recv;
                int y = (int)param0;

                int mod;
                int div = fix_divmod.fixdivmod(caller, x, y, out mod);

                return Bignum.Normalise(div);
            }

            // FIXME: Must not hardcode "/"; the code is wrong if this method
            // is being called as "div".
            return Numeric.rb_num_coerce_bin(recv, param0, "/", caller);
        }
    }


    
    internal class fix_mod : MethodBody1 // author: cjs, status: done
    {
        internal static fix_mod singleton = new fix_mod();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                int x = (int)recv;
                int y = (int)param0;

                int mod;
                fix_divmod.fixdivmod(caller, x, y, out mod);

                return Bignum.Normalise(mod);
            }

            return Numeric.rb_num_coerce_bin(recv, param0, "%", caller);
        }
    }


    
    internal class fix_divmod : MethodBody1 // author: cjs, status: done
    {
        internal static fix_divmod singleton = new fix_divmod();

        internal static int fixdivmod(Frame caller, int x, int y, out int mod)
        {
            int div;

            if (y == 0)
                throw ZeroDivisionError.rb_num_zerodiv(caller).raise(caller);

            if (y < 0)
            {
                if (x < 0)
                    div = -x / -y;
                else
                    div = -(x / -y);
            }
            else
            {
                if (x < 0)
                    div = -(-x / y);
                else
                    div = x / y;
            }
            mod = x - div * y;
            if ((mod < 0 && y > 0) || (mod > 0 && y < 0))
            {
                mod += y;
                div -= 1;
            }
            return div;
        }

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                int x = (int) recv;
                int y = (int) param0;

                int remainder;
                //System.Math.DivRem(x, y, out remainder); does not handle sign correctly
                int quotient = fixdivmod(caller, x, y, out remainder);
                return new Array(quotient, remainder);
            }

            return Numeric.rb_num_coerce_bin(recv, param0, "divmod", caller);
        }
    }


    
    internal class fix_quo : MethodBody1 // status: done
    {
        internal static fix_quo singleton = new fix_quo();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return new Float((double)(int)recv / (double)(int)param0);
            }

            return Numeric.rb_num_coerce_bin(recv, param0, "quo", caller);
        }
    }


    
    internal class fix_pow : MethodBody1 // status: done
    {
        internal static fix_pow singleton = new fix_pow();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                int x, y;
                y = (int)param0;
                if (y == 0) return 1;
                if (y == 1) return recv;
                x = (int)recv;
                if (y > 0)
                    return rb_big_pow.singleton.Call1(last_class, new Bignum(x), caller, null, y);

                return new Float(System.Math.Pow((double)x, (double)y));
            }

            return Numeric.rb_num_coerce_bin(recv, param0, "**", caller);
        }
    }


    
    internal class fix_abs : MethodBody0 // status: done
    {
        internal static fix_abs singleton = new fix_abs();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            int value = (int)recv;

            if (value < 0)
                return value * -1;

            return value;
        }
    }


    
    internal class fix_equal : MethodBody1 // status: done
    {
        internal static fix_equal singleton = new fix_equal();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return (int)recv == (int)param0;
            }
            else
            {
                return Eval.CallPrivate(param0, caller, "==", null, recv);
            }
        }
    }


    
    internal class fix_cmp : MethodBody1 // status: done
    {
        internal static fix_cmp singleton = new fix_cmp();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                int a = (int)recv;
                int b = (int)param0;
                return (a == b) ? 0 : ((a > b) ? 1 : -1);
            }
            else
            {
                return Numeric.rb_num_coerce_cmp(recv, param0, "<=>", caller);
            }
        }
    }


    
    internal class fix_gt : MethodBody1 // status: done
    {
        internal static fix_gt singleton = new fix_gt();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return (int)recv > (int)param0;
            }
            else
            {
                return Numeric.rb_num_coerce_relop(recv, param0, ">", caller);
            }
        }
    }


    
    internal class fix_ge : MethodBody1 // status: done
    {
        internal static fix_ge singleton = new fix_ge();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return (int)recv >= (int)param0;
            }
            else
            {
                return Numeric.rb_num_coerce_relop(recv, param0, ">=", caller);
            }
        }
    }


    
    internal class fix_lt : MethodBody1 // status: done
    {
        internal static fix_lt singleton = new fix_lt();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return (int)recv < (int)param0;
            }
            else
            {
                return Numeric.rb_num_coerce_relop(recv, param0, "<", caller);
            }
        }
    }


    
    internal class fix_le : MethodBody1 // status: done
    {
        internal static fix_le singleton = new fix_le();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return (int)recv <= (int)param0;
            }
            else
            {
                return Numeric.rb_num_coerce_relop(recv, param0, "<=", caller);
            }
        }
    }


    
    internal class fix_rev : MethodBody0 // status: done
    {
        internal static fix_rev singleton = new fix_rev();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ~(int)recv;
        }
    }


    
    internal class fix_and : MethodBody1 // status: done
    {
        internal static fix_and singleton = new fix_and();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return (int) recv & (int) param0;
            }
            else if (param0 is Bignum)
            {
                return rb_big_and.singleton.Call1(last_class, param0, caller, null, recv);
            }

            return (int)recv & Numeric.rb_num2long(param0, caller);
        }
    }


    
    internal class fix_or : MethodBody1 // status: done
    {
        internal static fix_or singleton = new fix_or();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return (int)recv | (int)param0;
            }
            else if (param0 is Bignum)
            {
                return rb_big_or.singleton.Call1(last_class, param0, caller, null, recv);
            }

            return (int)recv | Numeric.rb_num2long(param0, caller);
        }
    }


    
    internal class fix_xor : MethodBody1 // status: done
    {
        internal static fix_xor singleton = new fix_xor();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is int)
            {
                return (int)recv ^ (int)param0;
            }
            else if (param0 is Bignum)
            {
                return rb_big_xor.singleton.Call1(last_class, param0, caller, null, recv);
            }

            return (int)recv ^ Numeric.rb_num2long(param0, caller);
        }
    }


    
    internal class fix_aref : MethodBody1 // status: done
    {
        internal static fix_aref singleton = new fix_aref();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            int val, i;

            val = (int)recv;

            if (param0 is Bignum)
            {
                return rb_big_aref.singleton.Call1(last_class, new Bignum(val), caller, block, param0);
            }

            i = Numeric.rb_num2long(param0, caller);
            if (i < 0)
                return 0;

            if (sizeof(int) * 8 - 1 < i)
                return (val < 0) ? 1 : 0;

            return (val & (1 << i)) > 0 ? 1 : 0;
        }
    }


    
    internal class fix_lshift : MethodBody1 // status: done
    {
        internal static fix_lshift singleton = new fix_lshift();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            int width = (param0 is int) ? (int)param0 : Numeric.rb_num2long(param0, caller);
            if (width < 0)
                return fix_rshift.singleton.Call1(last_class, recv, caller, null, -width);

            int val = (int)recv;
            const int WordSize = sizeof(int) * 8 - 1;

            if (width > WordSize || ((uint)val >> (WordSize - width)) > 0)
                return new Bignum(IronMath.integer.make((int)recv) << width);
            else
                return val << width;
        }
    }


    
    internal class fix_rshift : MethodBody1 // status: done
    {
        internal static fix_rshift singleton = new fix_rshift();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            int width = (param0 is int) ? (int)param0 : Numeric.rb_num2long(param0, caller);
            if (width < 0)
                return fix_lshift.singleton.Call1(last_class, recv, caller, null, -width);

            try
            {
                return checked((int)recv >> width);
            }
            catch (System.OverflowException)
            {
                return Bignum.NormaliseUsing(IronMath.integer.make((int)recv) >> width);
            }
        }
    }


    
    internal class fix_size : MethodBody0 // status: done
    {
        internal static fix_size singleton = new fix_size();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return sizeof(int);
        }
    }


    
    internal class fix_to_f : MethodBody0 // status: done
    {
        internal static fix_to_f singleton = new fix_to_f();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Float((double)(int)recv);
        }
    }


    
    internal class fix_zero_p : MethodBody0 // status: done
    {
        internal static fix_zero_p singleton = new fix_zero_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return (int)recv == 0;
        }
    }
}
