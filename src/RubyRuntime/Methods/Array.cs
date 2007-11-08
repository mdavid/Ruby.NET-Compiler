/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using Ruby.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Ruby.Methods
{
    
    internal class rb_ary_s_create : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_s_create singleton = new rb_ary_s_create();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Array ary;

            if (rest.Count < 0)
                throw new ArgumentError("negative number of arguments").raise(caller);

            if (rest.Count > 0)
                ary = new Array((Class)recv, rest);
            else
                ary = new Array((Class)recv);
            
            return ary;
        }
    }


    
    internal class ary_alloc : MethodBody0 // status: done
    {
        internal static ary_alloc singleton = new ary_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Array((Class)recv);
        }
    }


    
    internal class rb_ary_to_a : MethodBody0 // status: done
    {
        internal static rb_ary_to_a singleton = new rb_ary_to_a();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = (Array)recv;
            if (ary.my_class == Ruby.Runtime.Init.rb_cArray)
                return ary;
            else
                return new Array(ary.value);
        }
    }


    
    internal class rb_ary_to_ary_m : MethodBody0 // status: done
    {
        internal static rb_ary_to_ary_m singleton = new rb_ary_to_ary_m();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return recv;
        }
    }


    
    internal class rb_ary_frozen_p : MethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_frozen_p singleton = new rb_ary_frozen_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = (Array)recv;
            return ary.Frozen || ary.ARY_TMPLOCK;
        }
    }


    
    internal class rb_ary_replace : MethodBody1 // status: done
    {
        internal static rb_ary_replace singleton = new rb_ary_replace();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            Array ary = (Array) recv;
            Array.rb_ary_modify(caller, ary);
            ary.value.Clear();
            ary.value.AddRange(Array.ArrayValue(param0, caller));
            return ary;
        }
    }


    
    internal class rb_ary_clear : MethodBody0 // status: done
    {
        internal static rb_ary_clear singleton = new rb_ary_clear();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = (Array)recv;

            Array.rb_ary_modify(caller, ary);

            ary.value.Clear();
            return recv;
        }
    }


    
    internal class rb_ary_plus : MethodBody1 // status: done
    {
        internal static rb_ary_plus singleton = new rb_ary_plus();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            Array ary = new Array(((Array)recv).value);
            ary.value.AddRange(Array.ArrayValue(param0, caller));
            return ary;
        }
    }


    
    internal class rb_ary_shift : MethodBody0 // status: done
    {
        internal static rb_ary_shift singleton = new rb_ary_shift();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array.rb_ary_modify_check(caller, (Array)recv);

            ArrayList ary = ((Array)recv).value;

            if (ary.Count == 0)
            {
                return null;
            }
            else
            {
                object result = ary[0];
                ary.RemoveAt(0);
                return result;
            }
        }
    }


    
    internal class rb_ary_pop : MethodBody0 // status: done
    {
        internal static rb_ary_pop singleton = new rb_ary_pop();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array.rb_ary_modify_check(caller, (Array)recv);

            ArrayList ary = ((Array)recv).value;

            if (ary.Count == 0)
            {
                return null;
            }
            else
            {
                object result = ary[ary.Count - 1];
                ary.RemoveAt(ary.Count - 1);
                return result;
            }
        }
    }


    
    internal class rb_ary_first : VarArgMethodBody0 // status: done
    {
        internal static rb_ary_first singleton = new rb_ary_first();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            ArrayList ary = ((Array)recv).value;

            if (rest.Count == 0)
            {
                if (ary.Count == 0)
                    return null;
                else
                    return ary[0];
            }
            else
            {
                Class.rb_scan_args(caller, rest, 0, 1, false);

                int length = Numeric.rb_num2long(rest[0], caller);

                if (length < 0)
                    throw new ArgumentError("negative array size (or size too big)").raise(caller);

                if (length > ary.Count)
                    length = ary.Count;

                return Array.CreateUsing(ary.GetRange(0, length));
            }
        }
    }


    
    internal class rb_ary_last : VarArgMethodBody0 // status: done
    {
        internal static rb_ary_last singleton = new rb_ary_last();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            ArrayList ary = ((Array)recv).value;

            if (rest.Count == 0)
            {
                if (ary.Count == 0)
                    return null;
                else
                    return ary[ary.Count - 1];
            }
            else
            {
                Class.rb_scan_args(caller, rest, 0, 1, false);

                int length = Numeric.rb_num2long(rest[0], caller);

                if (length < 0)
                    throw new ArgumentError("negative array size (or size too big)").raise(caller);

                if (length > ary.Count)
                    length = ary.Count;

                return Array.CreateUsing(ary.GetRange(ary.Count - length, length));
            }
        }

    }


    
    internal class rb_ary_equal : MethodBody1 // status: done
    {
        internal static rb_ary_equal singleton = new rb_ary_equal();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (recv == param0)
            {
                return true;
            }
            if (param0 is Array)
            {
                ArrayList ary = ((Array)recv).value;
                ArrayList ary2 = ((Array)param0).value;

                if (ary.Count != ary2.Count)
                    return false;

                for (int i = 0; i < ary.Count; i++)
                {
                    if (!Object.Equal(ary[i], ary2[i], caller))
                        return false;
                }

                return true;
            }
            else
            {
                if (Eval.RespondTo(param0, "to_ary"))
                    return Object.Equal(param0, recv, caller);
                return false;
            }
        }
    }


    
    internal class rb_ary_eql : MethodBody1 // status: done
    {
        internal static rb_ary_eql singleton = new rb_ary_eql();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (recv == param0)
            {
                return true;
            }
            if (param0 is Array)
            {
                ArrayList ary = ((Array)recv).value;
                ArrayList ary2 = ((Array)param0).value;

                if (ary.Count != ary2.Count)
                    return false;

                for (int i = 0; i < ary.Count; i++)
                {
                    if (!Object.Equal(ary[i], ary2[i], caller))
                        return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }


    
    internal class rb_ary_empty_p : MethodBody0 // status: done
    {
        internal static rb_ary_empty_p singleton = new rb_ary_empty_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Array)recv).value.Count == 0;
        }
    }


    
    internal class rb_ary_length : MethodBody0 // status: done
    {
        internal static rb_ary_length singleton = new rb_ary_length();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Array)recv).value.Count;
        }
    }


    
    internal class rb_ary_unshift_m : MethodBody // status: done
    {
        internal static rb_ary_unshift_m singleton = new rb_ary_unshift_m();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList optional)
        {
            Array.rb_ary_modify(caller, (Array)recv);
            ((Array)recv).value.InsertRange(0, optional);
            return recv;
        }
    }


    
    internal class rb_ary_insert : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_insert singleton = new rb_ary_insert();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array args)
        {
            Array ary = (Array)recv;

            if (args.Count < 1)
                throw new ArgumentError("wrong number of arguments (at least 1)").raise(caller);

            int pos = Numeric.rb_num2long(args[0], caller);
            if (pos == -1)
            {
                pos = ary.Count;
            }
            else if (pos < 0)
            {
                pos++;
            }

            if (args.Count == 1)
                return ary;

            ary.rb_ary_splice(pos, 0, new Array(args.value.GetRange(1, args.value.Count - 1)), caller);
            return ary;
        }
    }


    
    internal class rb_ary_aref : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_aref singleton = new rb_ary_aref();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Array ary = (Array)recv;

            int beg, len;

            if (rest.Count == 2)
            {
                if (rest[0] is Symbol)
                {
                    throw new TypeError("Symbol as array index").raise(caller);
                }

                beg = Numeric.rb_num2long(rest[0], caller);
                len = Numeric.rb_num2long(rest[1], caller);
                if (beg < 0)
                {
                    beg += ary.Count;
                }

                return ary.rb_ary_subseq(beg, len);
            }

            if (rest.Count != 1)
                Class.rb_scan_args(caller, rest, 1, 1, false);

            if (rest[0] is int)
            {
                return ary.rb_ary_entry((int)rest[0]);
            }

            if (rest[0] is Symbol)
            {
                throw new TypeError("Symbol as array index").raise(caller);
            }
                                   
            object result = Range.MapToLength(rest[0], ary.Count, false, false, out beg, out len, caller);
            if (result is bool && ((bool)result) == false)
            {
                //drop through
            }
            else if (result == null)
            {
                return null;
            }
            else
            {
                return ary.rb_ary_subseq(beg, len);
            }                               

            return ary.rb_ary_entry(Numeric.rb_num2long(rest[0], caller));
        }
    }

    
    internal class rb_ary_slice_bang : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_slice_bang singleton = new rb_ary_slice_bang();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Array ary = (Array)recv;

            int beg, len;
            if (Class.rb_scan_args(caller, rest, 1, 1, false) == 2)
            {
                beg = Numeric.rb_num2long(rest[0], caller);
                len = Numeric.rb_num2long(rest[1], caller);
            }
            else
            {
                if (!(rest[0] is int) && (bool)Range.MapToLength(rest[0], ary.Count, false, false, out beg, out len, caller))
                {
                }
                else
                    return rb_ary_delete_at_m.singleton.Call1(last_class, recv, caller, null, Numeric.rb_num2long(rest[0], caller));
            }

            if (beg < 0)
                beg = ary.Count + beg;

            object result = ary.rb_ary_subseq(beg, len);
            ary.rb_ary_splice(beg, len, null, caller);
            return result;
        }
    }


    
    internal class rb_ary_reverse_m : MethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_reverse_m singleton = new rb_ary_reverse_m();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = new Array(((Array)recv).value);
            rb_ary_reverse_bang.singleton.Call0(last_class, ary, caller, block);
            
            return ary;
        }
    }


    
    internal class rb_ary_reverse_bang : MethodBody0 // status: done
    {
        internal static rb_ary_reverse_bang singleton = new rb_ary_reverse_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = (Array)recv;

            Array.rb_ary_modify(caller, ary);

            ary.value.Reverse();
            return recv;
        }
    }

    
    internal class rb_ary_sort : MethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_sort singleton = new rb_ary_sort();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = (Array)recv;
            ary = new Array(ary.my_class, ary.value);
            return rb_ary_sort_bang.singleton.Call0(last_class, ary, caller, block);
        }
    }

    
    internal class rb_ary_sort_bang : MethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_sort_bang singleton = new rb_ary_sort_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = (Array)recv;
            Array.rb_ary_modify(caller, ary);
            if (ary.Count > 1)
            {
                ary.ARY_TMPLOCK = true;
                try
                {
                    ary.value.Sort(new Array.rb_ary_sort_comparer(caller, block));
                }
                catch (System.InvalidOperationException ex)
                {
                    if (ex.InnerException is RubyException)
                        throw ex.InnerException;

                    System.Console.WriteLine(ex.ToString());
                    throw ex;
                }
                finally
                {
                    ary.ARY_TMPLOCK = false;
                }
            }
            return recv;
        }
    }

    
    internal class rb_ary_at : MethodBody1 // status: done
    {
        internal static rb_ary_at singleton = new rb_ary_at();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            ArrayList ary = ((Array)recv).value;
            int index = Array.FixIndex(ary, Numeric.rb_num2long(param0, caller));

            if (index < 0 || index >= ary.Count)
                return null;

            return ary[index];
        }
    }


    
    internal class rb_ary_compact : MethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_compact singleton = new rb_ary_compact();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = new Array(((Basic)recv).my_class, ((Array)recv).value);
            rb_ary_compact_bang.singleton.Call0(last_class, ary, caller, block);

            return ary;
        }
    }


    
    internal class rb_ary_compact_bang : MethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_compact_bang singleton = new rb_ary_compact_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = (Array)recv;
            Array.rb_ary_modify(caller, ary);

            bool compacted = false;
            for (int i = 0; i < ary.Count; i++)
                if (ary[i] == null)
                {
                    ary.value.RemoveAt(i--);
                    compacted = true;
                }

            if (!compacted)
                return null;

            return ary;
        }
    }


    
    internal class rb_ary_flatten : MethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_flatten singleton = new rb_ary_flatten();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = new Array((Array)recv);
            rb_ary_flatten_bang.singleton.Call0(last_class, ary, caller, block);
            return ary;
        }
    }


    
    internal class rb_ary_flatten_bang : MethodBody0 // status: done
    {
        internal static rb_ary_flatten_bang singleton = new rb_ary_flatten_bang();

        internal static bool FlattenHelper(ArrayList container, ArrayList memo, Array ary2, Frame caller)
        {
            bool mod = false;
            if (memo.Contains(ary2))
                throw new ArgumentError("tried to flatten recursive array").raise(caller);
            memo.Add(ary2);

            foreach (object o in ary2)
            {
                if (o is Array)
                {
                    FlattenHelper(container, memo, (Array)o, caller);
                    mod = true;
                }
                else
                {
                    container.Add(o);
                }
            }

            return mod;
        }

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array result = (Array)recv;
            ArrayList ary = new ArrayList();

            Array.rb_ary_modify(caller, result);
            if (!FlattenHelper(ary, new ArrayList(), result, caller))
                return null;

            result.value = ary;
            return result;
        }
    }


    
    internal class rb_ary_nitems : MethodBody0 // status: done
    {
        internal static rb_ary_nitems singleton = new rb_ary_nitems();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            int i = 0;

            foreach (object o in ((Array)recv).value)
                if (o != null)
                    i++;

            return i;
        }
    }


    
    internal class rb_ary_push : MethodBody1 // status: done
    {
        internal static rb_ary_push singleton = new rb_ary_push();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            Array ary = (Array)recv;

            Array.rb_ary_modify(caller, ary); 
            
            ary.value.Add(param0);
            return recv;
        }
    }


    
    internal class rb_ary_push_m : MethodBody // status: done
    {
        internal static rb_ary_push_m singleton = new rb_ary_push_m();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            Array ary = (Array)recv;

            Array.rb_ary_modify(caller, ary);

            ary.value.AddRange(args);
            return recv;
        }
    }


    
    internal class rb_ary_concat : MethodBody1 // status: done
    {
        internal static rb_ary_concat singleton = new rb_ary_concat();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            Array x = (Array)recv;
            Array y = Array.to_ary(param0, caller);
            if (y.Count > 0)
                x.rb_ary_splice(x.Count, 0, param0, caller);

            return recv;
        }
    }


    
    internal class rb_ary_delete_at_m : MethodBody1 // status: done
    {
        internal static rb_ary_delete_at_m singleton = new rb_ary_delete_at_m();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            ArrayList ary = ((Array)recv).value;
            int index = Array.FixIndex(ary, Numeric.rb_num2long(param0, caller));

            if (index < 0 || index >= ary.Count)
                return null;

            Array.rb_ary_modify_check(caller, (Array)recv);

            object result = ary[index];
            ary.RemoveAt(index);
            return result;
        }
    }

    
    internal class rb_ary_delete_if : MethodBody0 // status: done
    {
        internal static rb_ary_delete_if singleton = new rb_ary_delete_if();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            rb_ary_reject_bang.singleton.Call0(last_class, recv, caller, block);
            return recv;
        }
    }


    
    internal class rb_ary_assoc : MethodBody1 // status: done
    {
        internal static rb_ary_assoc singleton = new rb_ary_assoc();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            foreach (object o in ((Array)recv).value)
            {
                if (o is Array)
                {
                    Array child = (Array)o;

                    if (child.value.Count > 0 && Object.Equal(child[0], param0, caller))
                        return child;
                }
            }

            return null;
        }
    }


    
    internal class rb_ary_rassoc : MethodBody1 // status: done
    {
        internal static rb_ary_rassoc singleton = new rb_ary_rassoc();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            foreach (object o in ((Array)recv).value)
            {
                if (o is Array)
                {
                    Array child = (Array)o;

                    if (child.value.Count > 1 && Object.Equal(child[1], param0, caller))
                        return child;
                }
            }

            return null;
        }
    }

    
    internal class rb_ary_times : MethodBody1 // status: done
    {
        internal static rb_ary_times singleton = new rb_ary_times();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            String temp = String.rb_check_string_type(param0, caller);
            if (temp != null)
            {
                return rb_ary_join_m.singleton.Call1(last_class, recv, caller, null, temp);
            }

            int len = Numeric.rb_num2long(param0, caller);
            if (len < 0)
                throw new ArgumentError("negative argument").raise(caller);

            ArrayList ary = ((Array)recv).value;
            ArrayList result = new ArrayList(ary.Count * len);

            for (int i = 0; i < len; i++)
                result.AddRange(ary);

            Array ary2 = Array.CreateUsing(result);
            Object.obj_infect(ary2, ary);
            return ary2;
        }
    }

    
    internal class rb_ary_uniq : MethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_uniq singleton = new rb_ary_uniq();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = (Array)recv;
            ary = new Array(ary.my_class, ary.value);
            rb_ary_uniq_bang.singleton.Call0(last_class, ary, caller, block);
            return ary;
        }
    }


    
    internal class rb_ary_uniq_bang : MethodBody0 // status: done
    {
        internal static rb_ary_uniq_bang singleton = new rb_ary_uniq_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ArrayList ary = ((Array)recv).value;
            int count = ary.Count;
            Dictionary passed = new Dictionary();

            for (int i = 0; i < ary.Count; i++)
            {
                object o = ary[i];
                Dictionary.Key key = new Dictionary.Key(o);
                if (passed.ContainsKey(key))
                {
                    ary.RemoveAt(i);
                    i--;
                }
                else
                {
                    passed.Add(key, null);
                }
            }

            if (count == ary.Count)
                return null;
            else
                return recv;
        }
    }


    
    internal class rb_ary_each : MethodBody0 // status: done
    {
        internal static rb_ary_each singleton = new rb_ary_each();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ArrayList ary = ((Array)recv).value;

            try
            {
                for (int i = 0; i < ary.Count; i++)
                {
                    ArgList args = new ArgList(null, ary[i]);
                    args.single_arg = true;
                    Proc.rb_yield(block, caller, args);
                }
            }
            catch (BreakException)
            {
            }

            return recv;
        }
    }


    
    internal class rb_ary_each_index : MethodBody0 // status: done
    {
        internal static rb_ary_each_index singleton = new rb_ary_each_index();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ArrayList ary = ((Array)recv).value;

            try
            {
                for (int i = 0; i < ary.Count; i++)
                    Proc.rb_yield(block, caller, i);
            }
            catch (BreakException)
            {
            }

            return recv;
        }
    }


    
    internal class rb_ary_reverse_each : MethodBody0 // status: done
    {
        internal static rb_ary_reverse_each singleton = new rb_ary_reverse_each();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ArrayList ary = ((Array)recv).value;

            try
            {
                for (int i = ary.Count - 1; i >= 0; i--)
                {
                    Proc.rb_yield(block, caller, ary[i]);

                    // ruby suppresses scary bounds errors
                    if (i > ary.Count)
                        i = ary.Count;
                }
            }
            catch (BreakException)
            {
            }

            return recv;
        }
    }


    
    internal class rb_ary_reject : MethodBody0 // status: done
    {
        internal static rb_ary_reject singleton = new rb_ary_reject();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = new Array(((Array)recv).value);
            rb_ary_reject_bang.singleton.Call0(last_class, ary, caller, block);
            return ary;
        }
    }


    
    internal class rb_ary_reject_bang : MethodBody0 // status: done
    {
        internal static rb_ary_reject_bang singleton = new rb_ary_reject_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array.rb_ary_modify(caller, (Array)recv);
            ArrayList ary = ((Array)recv).value;

            int count = ary.Count;

            for (int i = 0; i < ary.Count; i++)
            {
                object result = Proc.rb_yield(block, caller, ary[i]);
                if (Eval.Test(result))
                {
                    ary.RemoveAt(i);
                    i--;
                }
            }

            if (count == ary.Count)
                return null;
            else
                return recv;
        }
    }


    
    internal class rb_ary_select : MethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_select singleton = new rb_ary_select();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (block == null)
                throw new LocalJumpError("no block given").raise(caller);

            ArrayList ary = ((Array)recv).value;
            ArrayList rary = new ArrayList(ary.Count);

            for (int i = 0; i < ary.Count; i++)
            {
                object result = Proc.rb_yield(block, caller, ary[i]);
                if (Eval.Test(result))
                {
                    rary.Add(ary[i]);
                }
            }

            return Array.CreateUsing(rary);
        }
    }


    
    internal class rb_ary_collect : MethodBody0 // status: done
    {
        internal static rb_ary_collect singleton = new rb_ary_collect();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ArrayList ary = ((Array)recv).value;

            if (block == null)
                return new Array(ary);

            ArrayList result = new ArrayList(ary.Count);

            for (int i = 0; i < ary.Count; i++)
            {
                result.Add(Proc.rb_yield(block, caller, ary[i]));
            }

            return Array.CreateUsing(result);
        }
    }


    
    internal class rb_ary_collect_bang : MethodBody0 // status: done
    {
        internal static rb_ary_collect_bang singleton = new rb_ary_collect_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = (Array)recv;

            Array.rb_ary_modify(caller, ary);
            try
            {
                for (int i = 0; i < ary.Count; i++)
                {
                    ary[i] = Proc.rb_yield(block, caller, ary[i]);
                }
            }
            catch (BreakException)
            {
            }

            return recv;
        }
    }


    
    internal class rb_ary_fetch : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_fetch singleton = new rb_ary_fetch();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class.rb_scan_args(caller, rest, 1, 1, false);

            if (rest.Count == 1)
            {
                ArrayList ary = ((Array)recv).value;
                int index = Array.FixIndex(ary, Numeric.rb_num2long(rest[0], caller));
                if (index < 0 || index >= ary.Count)
                {
                    if (block != null)
                        return Proc.rb_yield(block, caller, rest[0]);
                    else
                    {
                        throw new IndexError(string.Format(CultureInfo.InvariantCulture, "index {0} out of array", Numeric.rb_num2long(rest[0], caller))).raise(caller);
                    }

                }
                else
                {
                    return ary[index];
                }
            }
            else
            {
                ArrayList ary = ((Array)recv).value;
                int index = Array.FixIndex(ary, Numeric.rb_num2long(rest[0], caller));
                if (index < 0 || index >= ary.Count)
                {
                    if (block != null)
                    {
                        if (rest.Count == 1)
                            Errors.rb_warn("block supersedes default value argument");
                        return Proc.rb_yield(block, caller, rest[0]);
                    }
                    else
                        return rest[1];
                }
                else
                {
                    return ary[index];
                }
            }
        }
    }


    
    internal class rb_ary_index : MethodBody1 // status: done
    {
        internal static rb_ary_index singleton = new rb_ary_index();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            ArrayList ary = ((Array)recv).value;
            for (int i = 0; i < ary.Count; i++)
            {
                if (Object.Equal(ary[i], param0, caller))
                    return i;
            }

            return null;
        }
    }


    
    internal class rb_ary_rindex : MethodBody1 // status: done
    {
        internal static rb_ary_rindex singleton = new rb_ary_rindex();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            ArrayList ary = ((Array)recv).value;

            for (int i = ary.Count - 1; i >= 0; i--)
            {
                if (Object.Equal(ary[i], param0, caller))
                    return i;

                // ruby suppresses scary bounds errors
                if (i > ary.Count)
                    i = ary.Count;
            }

            return null;
        }
    }


    
    internal class rb_ary_diff : MethodBody1 // status: done
    {
        internal static rb_ary_diff singleton = new rb_ary_diff();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            ArrayList ary = ((Array)recv).value;
            ArrayList ary2 = Array.ArrayValue(param0, caller);
            ArrayList result = new ArrayList(ary.Count);
            Dictionary set = new Dictionary();

            for (int i = 0; i < ary2.Count; i++)
            {
                Dictionary.Key key = new Dictionary.Key(ary2[i]);
                if (!set.ContainsKey(key))
                    set.Add(key, null);
            }

            for (int i = 0; i < ary.Count; i++)
            {
                Dictionary.Key key = new Dictionary.Key(ary[i]);
                if (!set.ContainsKey(key))
                {
                    result.Add(ary[i]);
                }
            }

            return Array.CreateUsing(result);
        }
    }


    
    internal class rb_ary_and : MethodBody1 // status: done
    {
        internal static rb_ary_and singleton = new rb_ary_and();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            ArrayList ary = ((Array)recv).value;
            ArrayList ary2 = Array.ArrayValue(param0, caller);
            ArrayList result = new ArrayList(ary.Count);
            Dictionary set = new Dictionary();

            for (int i = 0; i < ary2.Count; i++)
            {
                Dictionary.Key key = new Dictionary.Key(ary2[i]);
                if (!set.ContainsKey(key))
                    set.Add(key, null);
            }

            for (int i = 0; i < ary.Count; i++)
            {
                Dictionary.Key key = new Dictionary.Key(ary[i]);
                if (set.ContainsKey(key))
                {
                    result.Add(ary[i]);
                }
            }

            return Array.CreateUsing(result);
        }
    }


    
    internal class rb_ary_or : MethodBody1 // status: done
    {
        internal static rb_ary_or singleton = new rb_ary_or();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            ArrayList ary = ((Array)recv).value;
            ArrayList ary2 = Array.ArrayValue(param0, caller);
            ArrayList result = new ArrayList(ary.Count);
            Dictionary set = new Dictionary();

            for (int i = 0; i < ary.Count; i++)
            {
                Dictionary.Key key = new Dictionary.Key(ary[i]);
                if (!set.ContainsKey(key))
                {
                    set.Add(key, null);
                    result.Add(ary[i]);
                }
            }

            for (int i = 0; i < ary2.Count; i++)
            {
                Dictionary.Key key = new Dictionary.Key(ary2[i]);
                if (!set.ContainsKey(key))
                {
                    set.Add(key, null);
                    result.Add(ary2[i]);
                }
            }

            return Array.CreateUsing(result);
        }
    }


    
    internal class rb_ary_fill : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_fill singleton = new rb_ary_fill();

        private void fill(Array ary, object item, Proc block, int beg, int end, Frame caller)
        {
            Array.rb_ary_modify(caller, ary);

            while (ary.Count < end)
                ary.Add(null);

            for (int i = beg; i < end; i++)
                if (block != null)
                    ary[i] = Proc.rb_yield(block, caller, i);
                else
                    ary[i] = item;
        }

        private int begin(object o, int max, Frame caller)
        {
            int beg = o == null ? 0 : Numeric.rb_num2long(o, caller);
            if (beg < 0)
            {
                beg = max + beg;
                if (beg < 0)
                    beg = 0;
            }
            return beg;
        }

        private void range(object o, int max, out int beg, out int len, Frame caller)
        {
            object result = Range.MapToLength(o, max, false, false, out beg, out len, caller);
            if (result == null || (result is bool && (bool)result == true))
                return;

            beg = begin(o, max, caller);
            len = max - beg;
        }

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Array ary = (Array)recv;

            if (block != null)
                fill(ary, null, block, 0, ary.Count, caller);
            else
                Class.rb_scan_args(caller, new Array(), 1, 2, false); // generate correct error

            return ary;
        }
        
        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Array ary = (Array)recv;

            int beg = 0, len = ary.Count;
            if (block != null)
            {
                range(p1, ary.Count, out beg, out len, caller);
                fill(ary, null, block, beg, beg + len, caller);
            }
            else
                fill(ary, p1, null, beg, beg + len, caller);

            return ary;
        }

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            Array ary = (Array)recv;

            int beg, len;
            if (block != null)
            {
                beg = begin(p1, ary.Count, caller);
                len = p2 == null ? ary.Count - beg : Numeric.rb_num2long(p2, caller);
                fill(ary, null, block, beg, beg + len, caller);
            }
            else
            {
                range(p2, ary.Count, out beg, out len, caller);
                fill(ary, p1, null, beg, beg + len, caller);
            }

            return ary;
        }

        public override object Call3(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, object p3)
        {
            Array ary = (Array)recv;

            if (block != null)
            {
                Class.rb_scan_args(caller, new Array(), 0, 2, false); // generate correct error
            }
            else
            {
                int beg, len;
                beg = begin(p2, ary.Count, caller);
                len = p3 == null ? ary.Count - beg : Numeric.rb_num2long(p3, caller);

                fill(ary, p1, null, beg, beg + len, caller);
            }

            return ary;
        }
    }


    
    internal class rb_ary_includes : MethodBody1 // status: done
    {
        internal static rb_ary_includes singleton = new rb_ary_includes();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            ArrayList ary = ((Array)recv).value;

            for (int i = 0; i < ary.Count; i++)
                if (Object.Equal(ary[i], param0, caller))
                    return true;

            return false;
        }
    }


    
    internal class rb_ary_delete : MethodBody1 // status: done
    {
        internal static rb_ary_delete singleton = new rb_ary_delete();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            ArrayList ary = ((Array)recv).value;
            int count = ary.Count;

            for (int i = 0; i < ary.Count; i++)
                if (Object.Equal(ary[i], param0, caller))
                    ary.RemoveAt(i--);

            if (ary.Count == count)
            {
                if (block != null)
                    return Proc.rb_yield(block, caller);
                else
                    return null;
            }

            Array.rb_ary_modify_check(caller, (Array)recv);
            
            return param0;
        }
    }


    
    internal class rb_ary_hash : MethodBody0 // status: done
    {
        internal static rb_ary_hash singleton = new rb_ary_hash();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ArrayList ary = ((Array)recv).value;
            int hash = ary.Count;

            for (int i = 0; i < ary.Count; i++)
            {
                hash = (hash << 1) | (hash < 0 ? 1 : 0);
                // rb_hash
                object n = Eval.CallPrivate(ary[i], caller, "hash", null);
                hash ^= Numeric.rb_num2long(n, caller);
            }

            return hash;
        }
    }


    
    internal class rb_ary_transpose : MethodBody0 // status: done
    {
        internal static rb_ary_transpose singleton = new rb_ary_transpose();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ArrayList ary = ((Array)recv).value;
            List<Array> result = new List<Array>();
            int len = -1;

            for (int i = 0; i < ary.Count; i++)
            {
                ArrayList tmp = Array.ArrayValue(ary[i], caller);
                if (len < 0)
                {
                    len = tmp.Count;
                    for (int j = 0; j < len; j++)
                    {
                        result.Add(new Array());
                    }
                }
                else if (len != tmp.Count)
                {
                    throw new IndexError(string.Format(CultureInfo.InvariantCulture, "element size differ {0} should be {1}", tmp.Count, len)).raise(caller);
                }

                for (int j = 0; j < len; j++)
                {
                    result[j].Add(tmp[j]);
                }
            }

            return new Array(result);
        }
    }


    
    internal class rb_ary_cmp : MethodBody1 // status: done
    {
        internal static rb_ary_cmp singleton = new rb_ary_cmp();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            ArrayList ary = ((Array)recv).value;
            ArrayList ary2 = Array.ArrayValue(param0, caller);
            int len = ary.Count;
            if (ary2.Count < len)
                len = ary2.Count;

            for (int i = 0; i < len; i++)
            {
                object v = Eval.CallPrivate(ary[i], caller, "<=>", null, ary2[i]);
                if (!(v is int && (int)v == 0))
                    return v;
            }

            len = ary.Count - ary2.Count;

            if (len == 0)
                return 0;
            else if (len > 0)
                return 1;
            return -1;
        }
    }


    
    internal class rb_ary_initialize : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_initialize singleton = new rb_ary_initialize();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class.rb_scan_args(caller, rest, 0, 2, false);

            if (rest.Count == 0)
            {
                if (block != null)
                    Errors.rb_warning("given block not used");

                return recv;
            }

            ArrayList ary = ((Array)recv).value;

            if (rest.Count == 1 && rest[0] is Array)
            {
                rb_ary_replace.singleton.Call1(last_class, recv, caller, null, rest[0]);
                return recv;
            }

            int size = Numeric.rb_num2long(rest[0], caller);
            if (size < 0)
                throw new ArgumentError("negative array size").raise(caller);
            //if (len > 0 && len * (long)sizeof(VALUE) <= len)
            //    rb_raise(rb_eArgError, "array size too big");
            Array.rb_ary_modify_check(caller, (Array)recv);

            if (block != null)
            {
                if (rest.Count == 2)
                    Errors.rb_warn("block supersedes default value argument");
                for (int i = 0; i < size; i++)
                    ary.Add(Proc.rb_yield(block, caller, i));
            }
            else
            {
                object init_value = rest.Count == 2 ? rest[1] : null;

                for (int i = 0; i < size; i++)
                    ary.Add(init_value);
            }

            return recv;
        }
    }


    
    internal class rb_ary_inspect : MethodBody0 // status: done
    {
        internal static rb_ary_inspect singleton = new rb_ary_inspect();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ArrayList ary = ((Array)recv).value;

            if (ary.Count == 0)
                return new String("[]");
            else if (Array.IsInspecting(recv))
                return new String("[...]");
            else
            {
                try
                {
                    Array.StartInspect(recv);
                    bool tainted = ((Array)recv).Tainted;
                    StringBuilder sb = new StringBuilder();
                    sb.Append('[');

                    for (int i = 0; i < ary.Count; i++)
                    {
                        if (sb.Length > 1)
                            sb.Append(", ");

                        String s = Object.Inspect(ary[i], caller);
                        tainted |= s.Tainted;
                        sb.Append(s.value);
                    }

                    sb.Append(']');

                    String result = new String(sb.ToString());
                    result.Tainted = tainted;
                    return result;
                }
                finally
                {
                    Array.EndInspect(recv);
                }
            }
        }
    }


    
    internal class rb_ary_to_s : MethodBody0 // status: done
    {
        internal static rb_ary_to_s singleton = new rb_ary_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ArrayList ary = ((Array)recv).value;

            if (ary.Count == 0)
                return new String(string.Empty);

            return rb_ary_join_m.singleton.Call1(last_class, recv, caller, null, File.rb_output_fs.value);
        }
    }


    
    internal class rb_ary_join_m : VarArgMethodBody0 // status: done
    {
        internal static rb_ary_join_m singleton = new rb_ary_join_m();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return rb_ary_join_m.singleton.Call1(last_class, recv, caller, null, null);
        }

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            ArrayList ary = ((Array)recv).value;

            if (ary.Count == 0) // Special case: [].join(arbitrary_object) == ''
                return new String();

            string sep;

            object sep_obj = p1 ?? File.rb_output_fs.value;
            if (sep_obj == null)
                sep = string.Empty;
            else
                sep = String.StringValue((String)sep_obj, caller);

            // TODO: record & propagate taint

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < ary.Count; i++)
            {
                if (sb.Length > 0)
                    sb.Append(sep);

                if (ary[i] is String)
                {
                    sb.Append(((String)ary[i]).value);
                }
                else if (ary[i] is Array)
                {
                    if (Array.IsInspecting(ary[i]))
                    {
                        sb.Append("[...]");
                    }
                    else
                    {
                        object childAry = ary[i];

                        try
                        {
                            Array.StartInspect(childAry);

                            object str = rb_ary_join_m.singleton.Call1(last_class, childAry, caller, null, p1);
                            sb.Append(((String)str).value);
                        }
                        finally
                        {
                            Array.EndInspect(childAry);
                        }
                    }
                }
                else
                {
                    sb.Append(String.ObjectAsString(ary[i], caller));
                }
            }

            return new String(sb.ToString());
        }
    }


    
    internal class rb_ary_zip : MethodBody // status: done
    {
        internal static rb_ary_zip singleton = new rb_ary_zip();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            ArrayList ary = ((Array)recv).value;
            ArrayList[] arrays = new ArrayList[args.Length];

            for (int i = 0; i < args.Length; i++)
                arrays[i] = Array.ArrayValue(args[i], caller);

            if (args.block != null)
            {
                for (int i = 0; i < ary.Count; i++)
                {
                    ArrayList tmp = new ArrayList(arrays.Length);
                    tmp.Add(ary[i]);
                    for (int j = 0; j < arrays.Length; j++)
                    {
                        if (i < arrays[j].Count)
                            tmp.Add(arrays[j][i]);
                        else
                            tmp.Add(null);
                    }

                    Proc.rb_yield(args.block, caller, Array.CreateUsing(tmp));
                }

                return null;
            }

            ArrayList result = new ArrayList(ary.Count);

            for (int i = 0; i < ary.Count; i++)
            {
                ArrayList tmp = new ArrayList(arrays.Length);
                tmp.Add(ary[i]);
                for (int j = 0; j < arrays.Length; j++)
                {
                    if (i < arrays[j].Count)
                        tmp.Add(arrays[j][i]);
                    else
                        tmp.Add(null);
                }
                result.Add(Array.CreateUsing(tmp));
            }

            return Array.CreateUsing(result);
        }
    }


    
    internal class rb_ary_indexes : MethodBody // status: partial, comment: rb_frame_last_func
    {
        internal static rb_ary_indexes singleton = new rb_ary_indexes();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            Errors.rb_warn(string.Format(CultureInfo.InvariantCulture, "Array#{0} is deprecated; use Array#values_at", "indexes")); // rb_frame_last_func

            Array result = new Array();

            for (int i = 0; i < args.Length; i++)
                rb_ary_push.singleton.Call1(last_class, result, caller, null, rb_ary_aref.singleton.Call1(last_class, recv, caller, null, args[i]));

            return result;
        }
    }


    
    internal class rb_ary_values_at : MethodBody // status: done
    {
        internal static rb_ary_values_at singleton = new rb_ary_values_at();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            ArrayList ary = ((Array)recv).value;
            ArrayList result = new ArrayList();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is int)
                {
                    int index = Array.FixIndex(ary, (int)args[i]);

                    if (index < 0 || index >= ary.Count)
                    {
                        result.Add(null);
                    }
                    else
                    {
                        result.Add(ary[index]);
                    }
                }
                else if (args[i] is Range)
                {
                    if (ary.Count != 0)
                    {
                        int begin, length;
                        object mapResult = Range.MapToLength(args[i],ary.Count, false, false, out begin, out length, caller);
                        if (mapResult == null)
                        {
                            continue;
                        }
                        else if ((bool)mapResult == true)
                        {
                            for (int j = begin; j < begin + length; j++)
                            {
                                if (j < ary.Count)
                                {
                                    result.Add(ary[j]);
                                }
                                else
                                {
                                    result.Add(null);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    int index = Array.FixIndex(ary, Numeric.rb_num2long(args[i], caller));

                    if (index < 0 || index >= ary.Count)
                        result.Add(null);
                    else
                        result.Add(ary[index]);
                }
            }

            return new Array(result);
        }
    }


    
    internal class rb_ary_aset : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_ary_aset singleton = new rb_ary_aset();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Array ary = (Array)recv;

            int offset, beg, len;

            if (rest.Count == 3)
            {
                if (rest[0] is Symbol)
                {
                    throw new TypeError("Symbol as array index").raise(caller);
                }
                if (rest[1] is Symbol)
                {
                    throw new TypeError("Symbol as subarray length").raise(caller);
                }
                ary.rb_ary_splice(Numeric.rb_num2long(rest[0], caller), Numeric.rb_num2long(rest[1], caller), rest[2], caller);
                return rest[2];
            }
            if (rest.Count != 2)
            {
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "wrong number of arguments ({0} for 2)", rest.Count)).raise(caller);
            }
            if (rest[0] is int)
            {
                offset = (int)rest[0];
            }
            else
            {
                if (rest[0] is Symbol)
                {
                    throw new TypeError("Symbol as array index").raise(caller);
                }
                
                object result = Range.MapToLength(rest[0], ary.Count, false, false, out beg, out len, caller);
                if (result == null || (result is bool && (bool)result == true))
                {
                    ary.rb_ary_splice(beg, len, rest[1], caller);
                    return rest[1];
                }


                offset = Numeric.rb_num2long(rest[0], caller);
            }

            ary.rb_ary_store(offset, rest[1], caller);
            return rest[1];
        }
    }
}
