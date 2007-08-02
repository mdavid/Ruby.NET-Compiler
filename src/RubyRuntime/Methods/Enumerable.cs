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
    
    internal class enum_to_a : MethodBody0 // author: cjs, status: done
    {
        internal static enum_to_a singleton = new enum_to_a();

        private class to_a : MethodBody1
        {
            internal Array memo;

            internal to_a(Array memo)
            {
                this.memo = memo;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                memo.Add(i);

                return null;
            }
        }

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array memo = new Array();

            Eval.CallPrivate(recv, caller, "each", new Proc(null, null, new to_a(memo), 1));

            return memo;
        }
    }

    
    internal class enum_sort : MethodBody0 // author: cjs, status: done
    {
        internal static enum_sort singleton = new enum_sort();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = (Array)enum_to_a.singleton.Call0(last_class, recv, caller, null);

            rb_ary_sort_bang.singleton.Call0(last_class, ary, caller, block);

            return ary;
        }
    }

    
    internal class enum_sort_by : MethodBody0 // author: cjs, status: done
    {
        internal static enum_sort_by singleton = new enum_sort_by();


        private class sort_by_i : MethodBody1
        {
            internal Array ary;

            internal sort_by_i(Array ary)
            {
                this.ary = ary;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                ary.Add(new Array(Proc.rb_yield(block, caller, i), i));

                return null;
            }
        }


        private class sort_by_cmp : MethodBody2
        {
            internal sort_by_cmp() { }

            public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
            {
                return Comparable.Compare(((Array)p1)[0], ((Array)p2)[0], caller);
            }
        }
        
        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array tuples = new Array();

            Eval.CallPrivate(recv, caller, "each", new Proc(null, block, new sort_by_i(tuples), 1));

            rb_ary_sort_bang.singleton.Call0(last_class, tuples, caller, new Proc(null, null, new sort_by_cmp(), 1));

            Array ary = new Array();

            foreach (Array tuple in tuples)
            {
                ary.Add(tuple[1]);
            }

            return ary;
        }
    }

    
    internal class enum_grep : MethodBody1 // author: cjs, status: done
    {
        internal static enum_grep singleton = new enum_grep();

        private class Memo
        {
            internal Array ary;
            internal object pattern;
        }

        private class grep_iter_i : MethodBody1
        {
            internal Memo memo;

            internal grep_iter_i(Memo memo)
            {
                this.memo = memo;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                if (Eval.Test(Eval.CallPrivate(memo.pattern, caller, "===", block, new object[] { i })))
                    memo.ary.Add(Proc.rb_yield(block, caller, i));

                return null;
            }
        }

        private class grep_i : MethodBody1
        {
            internal Memo memo;

            internal grep_i(Memo memo)
            {
                this.memo = memo;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                if (Eval.Test(Eval.CallPrivate(memo.pattern, caller, "===", block, new object[] { i })))
                    memo.ary.Add(i);

                return null;
            }
        }

        public override object Call1(Class last_class, object list, Frame caller, Proc block, object p1)
        {
            Memo memo = new Memo();
            memo.ary = new Array();
            memo.pattern = p1;

            Eval.CallPrivate(list, caller, "each", new Proc(null, block, block != null ? (MethodBody)new grep_iter_i(memo) : (MethodBody)new grep_i(memo), 1));

            return memo.ary;
        }
    }

    
    internal class enum_find : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static enum_find singleton = new enum_find();

        private class find_i : MethodBody1
        {
            internal bool found;
            internal object value;

            internal find_i()
            {
                found = false;
                value = null;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                if (Eval.Test(Proc.rb_yield(block, caller, i)))
                {
                    value = i;
                    found = true;
                    throw new BreakException(true, caller);
                }

                return null;
            }
        }

        public override object Call(Class last_class, object list, Frame caller, Proc block, Array rest)
        {
            bool if_none = Class.rb_scan_args(caller, rest, 0, 1, false) == 1;

            find_i iterator = new find_i();

            // FIXME: Why do I need to use try..catch here when I don't for ALL, ANY
            try
            {
                Eval.CallPrivate(list, caller, "each", new Proc(null, block, iterator, 1));
            }
            catch (BreakException be)
            {
                if (!Eval.Test(be.return_value))
                    throw be;
            }

            if (iterator.found)
                return iterator.value;

            if (if_none && rest[0] != null)
            {
                Eval.CallPrivate(rest[0], caller, "call", null);
            }

            return null;
        }
    }

    
    internal class enum_find_all : MethodBody0 // author: cjs, status: done
    {
        internal static enum_find_all singleton = new enum_find_all();

        private class find_all_i : MethodBody1
        {
            internal Array ary;

            internal find_all_i(Array ary)
            {
                this.ary = ary;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                if (Eval.Test(Proc.rb_yield(block, caller, i)))
                    ary.Add(i);

                return null;
            }
        }

        public override object Call0(Class last_class, object list, Frame caller, Proc block)
        {
            Array ary = new Array();

            Eval.CallPrivate(list, caller, "each", new Proc(null, block, new find_all_i(ary), 1));

            return ary;
        }
    }

    
    internal class enum_reject : MethodBody0 // author: cjs, status: done
    {
        internal static enum_reject singleton = new enum_reject();

        private class reject_i : MethodBody1
        {
            internal Array ary;

            internal reject_i(Array ary)
            {
                this.ary = ary;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                if (!Eval.Test(Proc.rb_yield(block, caller, i)))
                {
                    ary.Add(i);
                }
                return null;
            }
        }

        public override object Call0(Class last_class, object list, Frame caller, Proc block)
        {
            Array ary = new Array();

            Eval.CallPrivate(list, caller, "each", new Proc(null, block, new reject_i(ary), 1));

            return ary;
        }
    }

    
    internal class enum_collect : MethodBody // status: done
    {
        internal static enum_collect singleton = new enum_collect();

        private class collect_i : MethodBody1
        {
            internal Array ary;

            internal collect_i(Array ary)
            {
                this.ary = ary;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                ary.Add(Proc.rb_yield(block, caller, i));
                return null;
            }

            public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
            {
                return Call1(last_class, recv, caller, args.block, args.ToRubyObject());
            }
        }

        private class collect_all : MethodBody1
        {
            internal Array ary;

            internal collect_all(Array ary)
            {
                this.ary = ary;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                ary.Add(i);
                return null;
            }

            public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
            {
                return Call1(last_class, recv, caller, args.block, args.ToRubyObject());
            }
        }

        public override object Calln(Class last_class, object list, Frame caller, ArgList args)
        {
            Array ary = new Array();
            Eval.CallPrivate(list, caller, "each", new Proc(null, args.block, args.block != null ? (MethodBody)new collect_i(ary) : (MethodBody)new collect_all(ary), 1));
            return ary;
        }
    }

    
    internal class enum_inject : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static enum_inject singleton = new enum_inject();

        private class Memo
        {
            internal object result;
            internal bool first = true;
        }


        private class inject_i : MethodBody1
        {
            internal Memo memo;

            internal inject_i(Memo memo)
            {
                this.memo = memo;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                if (memo.first)
                {
                    memo.first = false;
                    memo.result = i;
                }
                else
                    memo.result = Proc.rb_yield(block, caller, new object[] { memo.result, i });

                return null;
            }
        }

        public override object Call(Class last_class, object list, Frame caller, Proc block, Array rest)
        {
            Class.rb_scan_args(caller, rest, 0, 1, false);

            Memo memo = new Memo();

            if (rest.Count > 0)
            {
                memo.first = false;
                memo.result = rest[0];
            }
            else
            {
                memo.first = true;
                memo.result = null;
            }

            //    rb_iterate(rb_each, obj, inject_i, (VALUE)memo);
            Eval.CallPrivate(list, caller, "each", new Proc(null, block, new inject_i(memo), 1));

            return memo.result;
        }
    }

    
    internal class enum_partition : MethodBody0 // author: cjs, status: done
    {
        internal static enum_partition singleton = new enum_partition();
        

        private class Memo
        {
            internal Array true_array = new Array();
            internal Array false_array = new Array();
        }


        private class partition_i : MethodBody1
        {
            internal Memo memo;

            internal partition_i(Memo memo)
            {
                this.memo = memo;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                if (Eval.Test(Proc.rb_yield(block, caller, i)))
                    memo.true_array.Add(i);
                else
                    memo.false_array.Add(i);

                return null;
            }
        }

        public override object Call0(Class last_class, object list, Frame caller, Proc block)
        {
            Memo memo = new Memo();

            Eval.CallPrivate(list, caller, "each", new Proc(null, block, new partition_i(memo), 1));

            return new Array(new object[] { memo.true_array, memo.false_array });
        }
    }

    
    internal class enum_all : MethodBody0 // author: cjs, status: done
    {
        internal static enum_all singleton = new enum_all();


        private class all_i : MethodBody1
        {
            internal bool found;

            internal all_i()
            {
                found = false;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                if (block != null)
                    i = Proc.rb_yield(block, caller, i);

                if (!Eval.Test(i))
                {
                    found = true;
                    throw new BreakException(true, caller);
                }

                return null;
            }
        }

        public override object Call0(Class last_class, object list, Frame caller, Proc block)
        {
            all_i iterator = new all_i();

            Eval.CallPrivate(list, caller, "each", new Proc(null, block, iterator, 1));

            return !iterator.found; // are all the values not null/false
        }
    }

    
    internal class enum_any : MethodBody0 // author: cjs, status: done
    {
        internal static enum_any singleton = new enum_any();

        private class any_i : MethodBody1
        {
            internal bool found;

            internal any_i()
            {
                found = false;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                if (block != null)
                    i = Proc.rb_yield(block, caller, i);

                if (Eval.Test(i))
                {
                    found = true;
                    throw new BreakException(true, caller);
                }

                return null;
            }
        }
        
        public override object Call0(Class last_class, object list, Frame caller, Proc block)
        {
            any_i iterator = new any_i();

            Eval.CallPrivate(list, caller, "each", new Proc(null, block, iterator, 1));

            return iterator.found; // are any of the values not null/false
        }
    }

    
    internal class enum_min : MethodBody0 // author: cjs, status: done
    {
        internal static enum_min singleton = new enum_min();


        private class min_i : MethodBody1
        {
            private Memo memo;

            internal min_i(object memo)
            {
                this.memo = (Memo)memo;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                if (memo.min == null)
                {
                    memo.min = i;
                }
                else
                {
                    int result = Comparable.Compare(i, memo.min, caller);

                    if (result < 0)
                        memo.min = i;
                }

                return null;
            }
        }


        private class min_ii : MethodBody1
        {
            private Memo memo;

            internal min_ii(object memo)
            {
                this.memo = (Memo)memo;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {


                if (memo.min == null)
                {
                    memo.min = i;
                }
                else
                {
                    int result = Numeric.rb_num2long(Proc.rb_yield(block, caller, new object[] { i, memo.min }), caller);

                    if (result < 0)
                        memo.min = i;
                }

                return null;
            }
        }

        private class Memo
        {
            internal object min;

            internal Memo(object min)
            {
                this.min = min;
            }
        }

        public override object Call0(Class last_class, object list, Frame caller, Proc block)
        {
            Memo memo = new Memo(null);

            Eval.CallPrivate(list, caller, "each", new Proc(null, block, block != null ? (MethodBody)new min_ii(memo) : (MethodBody)new min_i(memo), 1));

            return memo.min;
        }
    }

    
    internal class enum_max : MethodBody0 // author: cjs/war, status: done
    {
        internal static enum_max singleton = new enum_max();

        private class max_i : MethodBody1
        {
            private Memo memo;

            internal max_i(object memo)
            {
                this.memo = (Memo)memo;
            }


            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                if (memo.max == null)
                {
                    memo.max = i;
                }
                else
                {
                    int result = Comparable.Compare(i, memo.max, caller);

                    if (result > 0)
                        memo.max = i;
                }

                return null;
            }
        }


        private class max_ii : MethodBody1
        {
            private Memo memo;

            internal max_ii(object memo)
            {
                this.memo = (Memo)memo;
            }


            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {

                if (memo.max == null)
                {
                    memo.max = i;
                }
                else
                {
                    int result = Numeric.rb_num2long(Proc.rb_yield(block, caller, new object[] { i, memo.max }), caller);

                    if (result > 0)
                        memo.max = i;
                }

                return null;
            }
        }

        private class Memo
        {
            internal object max;

            internal Memo(object max)
            {
                this.max = max;
            }
        }

        public override object Call0(Class last_class, object list, Frame caller, Proc block)
        {

            Memo memo = new Memo(null);

            Eval.CallPrivate(list, caller, "each", new Proc(null, block, block != null ? (MethodBody)new max_ii(memo) : (MethodBody)new max_i(memo), 1));

            return memo.max;
        }
    }

    
    internal class enum_member : MethodBody1 // author: cjs, status: done
    {
        internal static enum_member singleton = new enum_member();

        private class Memo
        {
            internal bool found;
            internal object member;
        }


        private class member_i : MethodBody1
        {
            internal Memo memo;

            internal member_i(Memo memo)
            {
                this.memo = memo;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                if (Eval.Equals(memo.member, i))
                {
                    memo.found = true;
                    throw new BreakException(true, caller);
                }
                return null;
            } 
        }

        public override object Call1(Class last_class, object list, Frame caller, Proc block, object p1)
        {
            Memo memo = new Memo();
            memo.found = false;
            memo.member = p1;

            Eval.CallPrivate(list, caller, "each", new Proc(null, block, new member_i(memo), 1));

            return memo.found;
        }
    }

    
    internal class enum_each_with_index : MethodBody0 // author: cjs, status: done
    {
        internal static enum_each_with_index singleton = new enum_each_with_index();


        private class each_with_index_i : MethodBody1
        {
            internal int memo;

            internal each_with_index_i(int memo)
            {
                this.memo = memo;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object i)
            {
                Proc.rb_yield(block, caller, new object[] { i, memo++ });

                return null;
            }
        }

        public override object Call0(Class last_class, object list, Frame caller, Proc block)
        {
            int memo = 0;

            Eval.CallPrivate(list, caller, "each", new Proc(list, block, new each_with_index_i(memo), 1));

            return list;
        }
    }

    
    internal class enum_zip : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static enum_zip singleton = new enum_zip();

        private class Memo
        {
            internal System.Collections.Generic.List<Array> args;
            internal int index;
        }

        private class zip_i : MethodBody1
        {
            internal Memo memo;
            internal Array result;

            internal zip_i(Memo memo)
            {
                result = new Array();
                this.memo = memo;
            }

            public override object Call1(Class last_class, object recv, Frame caller, Proc block, object val)
            {
                Array tmp;
                int i;

                tmp = new Array();
                tmp.Add(val);

                for (i = 0; i < memo.args.Count; i++)
                {
                    if (memo.args[i].Count > memo.index)
                        tmp.Add(memo.args[i][memo.index]);
                    else
                        tmp.Add(null);
                }

                if (block != null)
                {
                    block.yield(caller, tmp);
                }
                else
                {
                    result.Add(tmp);
                }

                memo.index++;

                return null;
            }
        }

        public override object Call(Class last_class, object list, Frame caller, Proc block, Array rest)
        {
            Memo memo = new Memo();
            memo.args = new System.Collections.Generic.List<Array>();
            memo.index = 0;

            for (int i = 0; i < rest.Count; i++)
            {
                memo.args.Add(Object.CheckConvert<Array>(rest[i], "to_a", caller));
            }

            zip_i iterator = new zip_i(memo);

            Eval.CallPrivate(list, caller, "each", new Proc(list, block, iterator, 1));

            return block == null ? iterator.result : null;
        }
    }
}