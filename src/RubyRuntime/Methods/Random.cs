/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System;

namespace Ruby.Methods
{
    
    internal class rb_f_srand : MethodBody // author: cjs, status: done
    {
        internal static rb_f_srand singleton = new rb_f_srand();

        private static int n = 0;

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            int old = Numeric.seed;

            Numeric.seed = (int)System.DateTime.Now.Ticks ^ System.Diagnostics.Process.GetCurrentProcess().Id ^ n++;
            Numeric.random = new System.Random(Numeric.seed);
            
            return old;
        }

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            int old = Numeric.seed;

            Numeric.seed = Numeric.rb_num2long(p1, caller);
            Numeric.random = new System.Random(Numeric.seed);

            return old;
        }
    }

    
    internal class rb_f_rand : MethodBody // author: cjs, status: done
    {
        internal static rb_f_rand singleton = new rb_f_rand();

        private Bignum rb_big_rand(Bignum max, double rand)
        {
            Bignum b = new Bignum(max.value * rand);

            return new Bignum(b.value % max.value);
        }

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (Numeric.random == null)
                rb_f_srand.singleton.Call0(last_class, recv, caller, null);

            return new Float(Numeric.random.NextDouble());
        }

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (Numeric.random == null)
                rb_f_srand.singleton.Call0(last_class, recv, caller, null);
                        
            int max;
            if (p1 == null)
                max = 0;
            else if (p1 is int)
                max = (int)p1;
            else if (p1 is Bignum)
                return rb_big_rand((Bignum)p1, Numeric.random.NextDouble());
            else if (p1 is Float)
            {
                if (((Float)p1).value <= int.MaxValue && ((Float)p1).value >= int.MinValue)
                    max = (int)((Float)p1).value;
                else
                    return rb_big_rand(new Bignum(((Float)p1).value), Numeric.random.NextDouble());
            }
            else
            {
                p1 = Integer.rb_Integer(p1, caller);
                if (p1 is Bignum)
                    return rb_big_rand((Bignum)p1, Numeric.random.NextDouble());
                max = (int)System.Math.Abs(Numeric.rb_num2long(p1, caller));
            }

            if (max == 0)
                return new Float(Numeric.random.NextDouble());
            else if (max < 0)
                max = -max;

            return (int)(max * Numeric.random.NextDouble());
        }
    }
}
