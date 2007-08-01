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
    
    internal class range_last : MethodBody0 //status: done
    {
        internal static range_last singleton = new range_last();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Range)recv).End;
        }
    }


    
    internal class range_first : MethodBody0 //status: done
    {
        internal static range_first singleton = new range_first();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Range)recv).Begin;
        }
    }


    
    internal class range_hash : MethodBody0 //status: done
    {
        internal static range_hash singleton = new range_hash();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {

            Range range = (Range)recv;
            int hash = range.Excl ? 1 : 0;
            hash ^= range.Begin.GetHashCode() << 1;
            hash ^= range.End.GetHashCode() << 9;
            hash ^= (range.Excl ? 1 : 0) << 24;
            return hash;
        }
    }


    
    internal class range_each : MethodBody0 // author: cjs, status: done
    {
        internal static range_each singleton = new range_each();

        internal static int rb_cmpint(Frame caller, object cmp, object a, object b)
        {
            if (cmp is int)
                return (int)cmp;
            if (cmp is Bignum)
                return ((Bignum)cmp).value.Sign;
            if (Eval.Test(Eval.CallPrivate(cmp, caller, "<", null, 0)))
                return 1;
            if (Eval.Test(Eval.CallPrivate(cmp, caller, ">", null, 0)))
                return -1;
            return 0;
        }

        internal static bool r_lt(Frame caller, object a, object b)
        {
            object cmp = Eval.CallPrivate1(a, caller, "<=>", null, b);

            if (cmp == null) return false;
            if (rb_cmpint(caller, cmp, a, b) < 0) return true;
            return false;
        }

        internal static object r_le(Frame caller, object a, object b)
        {
            object cmp = Eval.CallPrivate1(a, caller, "<=>", null, b);

            if (cmp == null) return false;
            int c = rb_cmpint(caller, cmp, a, b);
            if (c == 0) 
                return 0;
            else
                return (c < 0);
        }
        
        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Range range = (Range)recv;

            if (!Eval.RespondTo(range.Begin, "succ"))
                throw new TypeError(string.Format("cannot iterate from {0}", (Class.CLASS_OF(range.Begin)._name))).raise(caller);
                
            if (range.Begin is int && range.End is int)
            {
                int end = (int)range.End;
                if (!range.Excl)
                    end++;

                if (block == null)
                    throw new LocalJumpError("no block given").raise(caller);

                for (int i = (int)range.Begin; i < end; i++)
                    Proc.rb_yield(block, caller, i);
            }
            else
            {
                object o = range.Begin;
                object c;

                if (range.Excl)
                    while (r_lt(caller, o, range.End))
                    {
                        Proc.rb_yield(block, caller, o);
                        o = Eval.CallPrivate(o, caller, "succ", null);
                    }
                else
                    while (Eval.Test(c = r_le(caller, o, range.End)))
                    {
                        Proc.rb_yield(block, caller, o);
                        if (c is int && (int)c == 0) break;
                        o = Eval.CallPrivate(o, caller, "succ", null);
                    }
            }

            return range;
        }
    }

    
    internal class range_step : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static range_step singleton = new range_step();

        private object r_le(object a, object b, Frame caller)
        {
            object cmp = Eval.CallPrivate(a, caller, "<=>", null, b);

            if (cmp == null)
                return false;

            int icmp = (int)cmp;
            if (icmp == 0)
                return 0;
            return icmp < 0;
        }
        
        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            int step;

            if (rest.Count == 0)
                step = 1;
            else if (rest.Count == 1)
                step = Numeric.rb_num2long(rest[0], caller);
            else
                throw new ArgumentError(string.Format("wrong number of arguments ({0} for 1)", rest.Count)).raise(caller);

            if (step < 0)
                throw new ArgumentError("step can't be negative").raise(caller);
            if (step == 0)
                throw new ArgumentError("step can't be 0").raise(caller);

            Range range = (Range)recv;

            if (range.Begin is int && range.End is int)
            {
                int end = (int)range.End;
                if (!range.Excl)
                    end++;

                if (block == null)
                    throw new LocalJumpError("no block given").raise(caller);

                for (int i = (int)range.Begin; i < end; i += step)
                    Proc.rb_yield(block, caller, i);
            }
            else if (Class.rb_obj_is_kind_of(range.Begin, Ruby.Runtime.Init.rb_cNumeric))
            {
                object begin = range.Begin;
                string function = range.Excl ? "<" : "<=";

                while (Eval.Test(Eval.CallPrivate1(begin, caller, function, null, range.End)))
                {
                    Proc.rb_yield(block, caller, begin);
                    begin = Eval.CallPrivate1(begin, caller, "+", null, step);
                }
            }
            else
            {
                if (!Eval.RespondTo(range.Begin, "succ"))
                    throw new TypeError(string.Format("cannot iterate from {0}", Class.rb_obj_classname(range.Begin))).raise(caller);

                object current = range.Begin;

                int i = 0;
                object cmp = null;
                while (Eval.Test(cmp = r_le(current, range.End, caller)))
                {
                    if (range.Excl && object.Equals(cmp, 0))
                        break;

                    if (i == 0)
                    {
                        if (block == null)
                            throw new LocalJumpError("no block given").raise(caller);

                        Proc.rb_yield(block, caller, current);

                        i = step - 1;
                    }
                    else
                    {
                        i--;
                    }

                    if (!range.Excl && object.Equals(cmp, 0))
                        break;

                    current = Eval.CallPrivate(current, caller, "succ", null);
                }
            }

            return recv;
        }
    }

    
    internal class range_include : MethodBody1 //status: done
    {
        internal static range_include singleton = new range_include();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            Range range = (Range)recv;

            if (Range.LE(range.Begin, param0, caller))
            {
                if (range.Excl)
                {
                    return Range.LT(param0, range.End, caller);
                }
                else
                {
                    return Range.LE(param0, range.End, caller);
                }
            }

            return false;
        }
    }


    
    internal class range_exclude_end_p : MethodBody0 //status: done
    {
        internal static range_exclude_end_p singleton = new range_exclude_end_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Range)recv).Excl;
        }
    }


    
    internal class range_eq : MethodBody1 //status: done
    {
        internal static range_eq singleton = new range_eq();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (object.ReferenceEquals(recv, param0))
                return true;

            if (!(param0 is Range))
                return false;

            Range r1 = (Range) recv;
            Range r2 = (Range) param0;

            return Object.Equal(r1.Begin, r2.Begin, caller)
                && Object.Equal(r1.End, r2.End, caller)
                && r1.Excl == r2.Excl;
        }
    }


    
    internal class range_eql : MethodBody1 //status: done
    {
        internal static range_eql singleton = new range_eql();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (object.ReferenceEquals(recv, param0))
                return true;

            if (!(param0 is Range))
                return false;

            Range r1 = (Range)recv;
            Range r2 = (Range)param0;

            return Object.Equal(r1.Begin, r2.Begin, caller)
                && Object.Equal(r1.End, r2.End, caller)
                && r1.Excl == r2.Excl;
        }
    }

    
    internal class range_alloc : MethodBody0 //author: war, status: done
    {
        internal static range_alloc singleton = new range_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Range((Class)recv);
        }       
    }
    

    internal class range_initialize : VarArgMethodBody2 // author: cjs/war, status: done
    {
        internal static range_initialize singleton = new range_initialize();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, Array rest)
        {            
            Range range = (Range)recv;

            if (range.Begin != null)
                throw new NameError("initialize", "`initialize' called twice").raise(caller);

            if (!(p1 is int) || !(p2 is int))
                try
                {
                    if (Eval.CallPrivate(p1, caller, "<=>", null, p2) == null)
                        throw new ArgumentError("bad value for range").raise(caller);
                }
                catch (RubyException ex)
                {
                    if (Class.rb_obj_is_kind_of(ex.parent, Ruby.Runtime.Init.rb_eStandardError))
                        throw new ArgumentError("bad value for range").raise(caller);
                    else
                        throw ex;
                }

            range.Begin = p1;
            range.End = p2;
            range.excl = (rest.Count > 0 && Eval.Test(rest[0]));

            return recv;
        }
    }


    
    internal class range_to_s : MethodBody0 //status: done
    {
        internal static range_to_s singleton = new range_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Range range = (Range) recv;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(String.ObjectAsString(range.Begin, caller));
            sb.Append(range.Excl ? "..." : "..");
            sb.Append(String.ObjectAsString(range.End, caller));
            return new String(sb.ToString());
        }
    }


    
    internal class range_inspect : MethodBody0 //status: done
    {
        internal static range_inspect singleton = new range_inspect();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Range range = (Range)recv;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(Object.Inspect(range.Begin, caller));
            sb.Append(range.Excl ? "..." : "..");
            sb.Append(Object.Inspect(range.End, caller));
            return new String(sb.ToString());
        }
    }
}
