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
    
    internal class rb_int_induced_from : MethodBody1 // author: cjs, status: done
    {
        internal static rb_int_induced_from singleton = new rb_int_induced_from();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (p1 is int || p1 is Bignum)
            {
                return p1;
            }
            else if (p1 is Float)
            {
                return Eval.CallPrivate(p1, caller, "to_i", null);
            }
            else
            {
                throw new TypeError(string.Format("failed to convert {0} into Integer", Class.rb_obj_classname(p1))).raise(caller);
            }
        }
    }


    
    internal class int_int_p : MethodBody0 // status: done
    {
        internal static int_int_p singleton = new int_int_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return true;
        }
    }


    
    internal class int_upto : MethodBody1 // status: done
    {
        internal static int_upto singleton = new int_upto();

        public override object Call1(Class last_class, object from, Frame caller, Proc block, object to)
        {
            if (from is int && to is int)
            {
                int end = (int)to;

                if (block == null)
                    throw new LocalJumpError("no block given").raise(caller);

                for (int i = (int)from; i <= end; i++)
                    Proc.rb_yield(block, caller, i);
            }
            else
            {
                object i = from;
                while (!(bool)Eval.CallPrivate(i, caller, ">", null, to))
                {
                    if (block == null)
                        throw new LocalJumpError("no block given").raise(caller);

                    Proc.rb_yield(block, caller, i);
                    i = Eval.CallPrivate(i, caller, "+", null, 1);
                }
            }

            return from;
        }
    }


    
    internal class int_downto : MethodBody1 // status: done
    {
        internal static int_downto singleton = new int_downto();

        public override object Call1(Class last_class, object from, Frame caller, Proc block, object to)
        {
            if (from is int && to is int)
            {
                int end = (int)to;

                if (block == null)
                    throw new LocalJumpError("no block given").raise(caller);

                for (int i = (int)from; i >= end; i--)
                    Proc.rb_yield(block, caller, i);
            }
            else
            {
                object i = from;
                
                while (!(bool)Eval.CallPrivate(i, caller, "<", null, to))
                {
                    if (block == null)
                        throw new LocalJumpError("no block given").raise(caller);

                    Proc.rb_yield(block, caller, i);
                    i = Eval.CallPrivate(i, caller, "-", null, 1);
                }
            }

            return from;
        }
    }


    
    internal class int_dotimes : MethodBody0 // status: done
    {
        internal static int_dotimes singleton = new int_dotimes();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (recv is int)
            {
                int end = (int)recv;

                if (block == null)
                    throw new LocalJumpError("no block given").raise(caller);

                for (int i = 0; i < end; i++)
                {
                    Proc.rb_yield(block, caller, i);
                }
            }
            else
            {
                for (int i = 0; (bool)Eval.CallPrivate(i, caller, "<", null, recv); i++)
                {
                    if (block == null)
                        throw new LocalJumpError("no block given").raise(caller);

                    Proc.rb_yield(block, caller, i);
                }
            }

            return recv;
        }
    }


    
    internal class int_succ : MethodBody0 // status: done
    {
        internal static int_succ singleton = new int_succ();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (recv is int)
            {
                try
                {
                    return checked((int)recv + 1);
                }
                catch (System.OverflowException)
                {
                    return Bignum.NormaliseUsing(IronMath.integer.make((int)recv) + 1);
                }
            }
            else
            {
                return Eval.CallPrivate(recv, caller, "+", null, 1);
            }
        }
    }

    
    internal class int_chr : MethodBody0 // status: done
    {
        internal static int_chr singleton = new int_chr();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            int i = Numeric.rb_num2long(recv, caller);

            if (i < 0 || 0xFF < i)
                throw new RangeError(string.Format("{0} out of char range", i)).raise(caller);

            return new String(((char)i).ToString());
        }
    }


    
    internal class int_to_i : MethodBody0 // status: done
    {
        internal static int_to_i singleton = new int_to_i();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return recv;
        }
    }
}
