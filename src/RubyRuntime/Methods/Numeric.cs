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
    
    internal class NumericAllocator : MethodBody0 // status: done
    {
        static internal NumericAllocator singleton = new NumericAllocator();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Numeric();
        }
    }


    
    internal class num_init_copy : MethodBody2 // author: cjs, status: done
    {
        static internal num_init_copy singleton = new num_init_copy();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object param0, object param1)
        {
            /* Numerics are immutable values, which should not be copied */
            throw new TypeError(string.Format("can't copy %s", ((Class)recv)._name)).raise(caller);
        }
    }


    
    internal class num_uplus : MethodBody0 // status: done
    {
        static internal num_uplus singleton = new num_uplus();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return recv;
        }
    }


    
    internal class num_uminus : MethodBody0 // status: done
    {
        static internal num_uminus singleton = new num_uminus();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            object zero = 0;
            Numeric.do_coerce(ref zero, ref recv, true, caller);
            return Eval.CallPrivate(zero, caller, "-", null, recv);
        }
    }

    
    internal class num_coerce : MethodBody1 // status: done
    {
        static internal num_coerce singleton = new num_coerce();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (Class.CLASS_OF(recv) == Class.CLASS_OF(param0))
                return new Array(param0, recv);

            //return new Array(Float.rb_Float(param0, caller), Float.rb_Float(recv, caller));
            Float fval = Float.rb_Float(recv, caller);
            Float coerced = Float.rb_Float(param0, caller);
            return new Array(coerced, fval);
        }
    }


    
    internal class num_cmp : MethodBody1 // author: cjs, status: done
    {
        static internal num_cmp singleton = new num_cmp();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (object.Equals(recv, param0))
                return 0;
            else
                return null;
        }
    }


    
    internal class num_eql : MethodBody1 // author: cjs, status: done
    {
        static internal num_eql singleton = new num_eql();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (Class.CLASS_OF(recv) != Class.CLASS_OF(param0))
                return false;

            return Object.Equals(recv, param0);
        }
    }


    
    internal class num_quo : MethodBody1 // status: done
    {
        static internal num_quo singleton = new num_quo();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return Eval.CallPrivate(recv, caller, "/", null, param0);
        }
    }


    
    internal class num_div : MethodBody1 // status: done
    {
        static internal num_div singleton = new num_div();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return Integer.rb_Integer(Eval.CallPrivate(recv, caller, "/", null, param0), caller);
        }
    }


    
    internal class num_divmod : MethodBody1 // status: done
    {
        static internal num_divmod singleton = new num_divmod();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return new Array(num_div.singleton.Call1(last_class, recv, caller, null, param0), Eval.CallPrivate(recv, caller, "%", null, param0));
        }
    }


    
    internal class num_modulo : MethodBody1 // status: done
    {
        static internal num_modulo singleton = new num_modulo();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return Eval.CallPrivate(recv, caller, "%", null, param0);
        }
    }


    
    internal class num_remainder : MethodBody1 // status: done
    {
        static internal num_remainder singleton = new num_remainder();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            object remainder = Eval.CallPrivate(recv, caller, "%", null, param0);

            if ((!Object.Equal(remainder, 0, caller)) &&
                (((bool)Eval.CallPrivate(recv, caller, "<", null, 0) &&
                (bool)Eval.CallPrivate(param0, caller, ">", null, 0)) ||
                ((bool)Eval.CallPrivate(recv, caller, ">", null, 0) &&
                (bool)(Eval.CallPrivate(param0, caller, "<", null, 0)))))
            {
                return Eval.CallPrivate(remainder, caller, "-", null, param0);
            }
            else
            {
                return remainder;
            }
        }
    }


    
    internal class num_abs : MethodBody0 // status: done
    {
        static internal num_abs singleton = new num_abs();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if ((bool)Eval.CallPrivate(recv, caller, "<", null, 0))
                return Eval.CallPrivate(recv, caller, "-@", null);

            return recv;
        }
    }


    
    internal class num_to_int : MethodBody0 // status: done
    {
        static internal num_to_int singleton = new num_to_int();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Eval.CallPrivate(recv, caller, "to_i", null);
        }
    }


    
    internal class num_int_p : MethodBody0 // status: done
    {
        static internal num_int_p singleton = new num_int_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }


    
    internal class num_zero_p : MethodBody0 // status: done
    {
        static internal num_zero_p singleton = new num_zero_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (object.Equals(recv, 0))
                return true;
            return false;
        }
    }


    
    internal class num_nonzero_p : MethodBody0 // status: done
    {
        static internal num_nonzero_p singleton = new num_nonzero_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if ((bool)Eval.CallPrivate(recv, caller, "zero?", null))
                return null;
            else
                return recv;
        }
    }


    
    internal class num_floor : MethodBody0 // status: done
    {
        static internal num_floor singleton = new num_floor();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return flo_floor.singleton.Call0(last_class, Float.rb_Float(recv, caller), caller, null);
        }
    }


    
    internal class num_ceil : MethodBody0 // status: done
    {
        static internal num_ceil singleton = new num_ceil();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return flo_ceil.singleton.Call0(last_class, Float.rb_Float(recv, caller), caller, null);
        }
    }


    
    internal class num_round : MethodBody0 // status: done
    {
        static internal num_round singleton = new num_round();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return flo_round.singleton.Call0(last_class, Float.rb_Float(recv, caller), caller, null);
        }
    }


    
    internal class num_truncate : MethodBody0 // status: done
    {
        static internal num_truncate singleton = new num_truncate();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return flo_truncate.singleton.Call0(last_class, Float.rb_Float(recv, caller), caller, null);
        }
    }

    
    internal class num_step : VarArgMethodBody0 // author: cjs, status: done
    {
        static internal num_step singleton = new num_step();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            object to;
            object step;
            if (rest.Count == 1)
            {
                to = rest[0];
                step = 1;
            }
            else if (rest.Count == 2)
            {
                to = rest[0];
                step = rest[1];
            }
            else
            {
                throw new ArgumentError("wrong number of arguments").raise(caller);
            }

            if (Object.Equal(step, 0, caller))
                throw new ArgumentError("step cannot be 0").raise(caller);

            if (recv is System.Int32 && to is System.Int32 && step is System.Int32)
            {
                int i = (int)recv;
                int end = (int)to;
                int diff = (int)step;

                if (diff > 0)
                {
                    while (i <= end)
                    {
                        Proc.rb_yield(block, caller, i);
                        i += diff;
                    }
                }
                else
                {
                    while (i >= end)
                    {
                        Proc.rb_yield(block, caller, i);
                        i += diff;
                    }
                }

            }
            else if (recv is Float && to is Float && step is Float)
            {
                double beg = ((Float)recv).value;
                double end = ((Float)to).value;
                double unit = ((Float)step).value;
                double n = (end - beg) / unit;
                double err = (System.Math.Abs(beg) + System.Math.Abs(end) + System.Math.Abs(end - beg)) / System.Math.Abs(unit) * double.Epsilon;
                int i = 0;

                if (err > 0.5) err = 0.5;
                n = System.Math.Floor(n + err) + 1;
                for (i = 0; i < n; i++)
                {
                    Proc.rb_yield(block, caller, new object[] { new Float(i * unit + beg) });
                }
            }
            else
            {
                object i = recv;
                string cmp = ((bool)Eval.CallPrivate(step, caller, ">", null, 0)) ? ">" : "<";
                for (; ; )
                {
                    if ((bool)Eval.CallPrivate(i, caller, cmp, null, to))
                        break;
                    Proc.rb_yield(block, caller, i);
                    i = Eval.CallPrivate(i, caller, "+", null, step);
                }
            }

            return recv;
        }

    }


    
    internal class num_sadded : MethodBody1 // author: cjs, status: done
    {
        static internal num_sadded singleton = new num_sadded();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            throw new TypeError(string.Format("can't define singleton method {0} for {1}", param0.ToString(), this.GetType().Name)).raise(caller);
        }
    }
}
