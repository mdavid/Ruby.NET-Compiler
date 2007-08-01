/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Collections;

namespace Ruby
{
    [UsedByRubyCompiler]
    public partial class Array : Basic, ICollection
    {
        internal ArrayList value;

        // -----------------------------------------------------------------------------

        [UsedByRubyCompiler]
        public Array()
            : base(Ruby.Runtime.Init.rb_cArray)
        {
            value = new ArrayList();
        }

        internal Array(ICollection collection): this()
        {
            value = new ArrayList(collection);
        }

        internal Array(params object[] collection)
            : this((ICollection)collection)
        {
        }

        internal Array(Class klass)
            : base(klass)
        {
            value = new ArrayList();
        }

        internal Array(Class klass, ICollection collection)
            : base(klass)
        {
            value = new ArrayList(collection);
        }

        internal Array(Class klass, params object[] collection)
            : this(klass, (ICollection)collection)
        {
        }

        // -----------------------------------------------------------------------------


        internal object this[int index]
        {
            get
            {
                return this.value[index];
            }

            set
            {
                this.value[index] = value;
            }
        }

        internal override object Inner()
        {
            return value;
        }

        internal bool ARY_TMPLOCK = false;

        #region ICollection Members
        public void CopyTo(System.Array array, int index)
        {
            this.value.CopyTo(array, index);
        }

        public int Count
        {
            get { return this.value.Count; }
        }

        public bool IsSynchronized
        {
            get { return this.value.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return this.value.SyncRoot; }
        }
        #endregion

        #region IEnumerable Members
        public IEnumerator GetEnumerator()
        {
            return this.value.GetEnumerator();
        }
        #endregion


        // used in Case ... When statements 
        internal class rb_ary_sort_comparer : IComparer
        {
            private Frame caller;
            private Proc comparer;

            internal rb_ary_sort_comparer(Frame caller, Proc block)
            {
                this.caller = caller;
                this.comparer = block;
            }

            internal rb_ary_sort_comparer(Frame caller) : this(caller, null) { }

            int IComparer.Compare(object x, object y)
            {
                if (comparer != null)
                {
                    object result = Proc.rb_yield(comparer,caller, x, y);
                    return (int)result;
                }
                else
                    return Comparable.Compare(x, y, caller);
            }
        }

        [UsedByRubyCompiler]
        public bool includes(object target, Frame caller)
        {
            foreach (object element in value)
                if (Eval.Test(Eval.CallPrivate(element, caller, "===", null, target)))
                    return true;
            return false;
        }

        internal Array Add(object element)
        {
            value.Add(element);
            return this;
        }

        // STATIC HELPERS -----------------------------------------------------------------

        static internal bool TryToArray(object obj, out Array array, Frame caller)
        {
            Class type = Class.CLASS_OF(obj);
            RubyMethod to_ary;
            if (type.get_method("to_ary", out to_ary))
            {
                object result = to_ary.body.Call0(null, obj, caller, null);
                if (result != null && result is Array)
                {
                    array = (Array)result;
                    return true;
                }
            }

            array = null;
            return false;
        }

        [UsedByRubyCompiler]
        public static Array Store(object obj)
        {
            return new Array(obj);
        }


        internal static Array rb_Array(object o, Frame caller)
        {
            if (o is Array)
            {
                return (Array)o;
            }
            else if (o is ArrayList)
            {
                return new Array((ArrayList)o);
            }
            else
            {
                object conv = Object.CheckConvert<Array>(o, "to_ary", caller); // rb_check_array_type
                //object conv = Object.Convert<Array>(o, "to_a", false, caller);

                if (conv == null)
                {
                    // Only allow Kernel.to_a
                    Class origin;
                    if (Eval.FindPrivateMethod(Class.CLASS_OF(conv), caller, "to_a", out origin) != null && origin.is_kind_of(Init.rb_mKernel))
                    {
                        object conv2 = Eval.Call0(conv, "to_a");
                        if (conv2 is Array)
                            return (Array)conv2;
                        else
                            throw new TypeError("`to_a' did not return Array").raise(caller);
                    }
                    else
                        return new Array(new object[] { o });
                }

                return (Array)conv;
            }
            
        }

        internal static void rb_ary_modify_check(Frame caller, Array ary)
        {
            if (ary.Frozen)
                throw TypeError.rb_error_frozen(caller, "array").raise(caller);
            if (ary.ARY_TMPLOCK)
                throw new RuntimeError("can't modify array during iteration").raise(caller);
            if (!ary.Tainted && Eval.rb_safe_level() >= 4)
                throw new SecurityError("Insecure: can't modify array").raise(caller);
        }

        internal static void rb_ary_modify(Frame caller, Array ary)
        {
            rb_ary_modify_check(caller, ary);
            //if (FL_TEST(ary, ELTS_SHARED))
            //{
            //    FL_UNSET(ary, ELTS_SHARED);
            //}
        }


        internal static Array CreateUsing(ArrayList value)
        {
            Array result = new Array();
            result.value = value;
            return result;
        }

        internal static ArrayList ArrayValue(object ary, Frame caller)
        {
            if (ary is Array)
                return ((Array)ary).value;

            return Object.Convert<Array>(ary, "to_ary", caller).value;
        }

        internal static bool TryArrayValue(object obj, out ArrayList value, Frame caller)
        {
            if (obj is Array)
            {
                value = ((Array)obj).value;
                return true;
            }
            else if (Eval.RespondTo(obj, "to_ary"))
            {
                value = Object.Convert<Array>(obj, "to_ary", caller).value;
                return true;
            }

            value = null;
            return false;
        }

        internal static int FixIndex(ArrayList list, int index)
        {
            return (index < 0) ? list.Count + index : index;
        }

        internal static int FixIndexInsert(ArrayList list, int index)
        {
            return (index < 0) ? list.Count + index : index;
        }

        internal static void StartInspect(object obj)
        {
            System.LocalDataStoreSlot slot = System.Threading.Thread.GetNamedDataSlot("Array.Inspecting");
            ArrayList inspecting = (ArrayList) System.Threading.Thread.GetData(slot);

            if (inspecting == null)
                System.Threading.Thread.SetData(slot, inspecting = new ArrayList());

            inspecting.Add(obj);
        }

        internal static void EndInspect(object obj)
        {
            System.LocalDataStoreSlot slot = System.Threading.Thread.GetNamedDataSlot("Array.Inspecting");
            ArrayList inspecting = (ArrayList)System.Threading.Thread.GetData(slot);

            if (inspecting == null)
            {
                return;
            }
            else
            {
                inspecting.RemoveAt(inspecting.Count - 1);
            }
        }

        internal static bool IsInspecting(object obj)
        {
            System.LocalDataStoreSlot slot = System.Threading.Thread.GetNamedDataSlot("Array.Inspecting");
            ArrayList inspecting = (ArrayList)System.Threading.Thread.GetData(slot);

            if (inspecting != null)
            {
                for (int i = 0; i < inspecting.Count; i++)
                    if (Object.ReferenceEquals(obj, inspecting[i]))
                        return true;
            }

            return false;
        }

        internal static Array to_ary(object ary, Frame caller)
        {
            return Object.Convert<Array>(ary, "to_ary", caller);
        }

        // -------- Instance Methods ----------------------------------------------------

        internal void rb_ary_splice(int beg, int len, object rpl, Frame caller)
        {
            long rlen;

            if (len < 0)
                throw new IndexError(string.Format("negative length ({0})", len)).raise(caller);

            if (beg < 0)
            {
                beg += this.Count;
                if (beg < 0)
                {
                    beg -= this.Count;
                    throw new IndexError(string.Format("index {0} out of array", beg)).raise(caller);
                }
            }
            if (beg + len > this.Count)
            {
                len = this.Count - beg;
            }

            if (rpl == null)
            {
                rlen = 0;
            }
            else
            {
                if (rpl is Array)
                    rlen = ((Array)rpl).Count;
                else
                    rlen = 1;
            }
            Array.rb_ary_modify(caller, this);

            if (beg >= this.Count)
            {
                while (beg > this.Count)
                {
                    this.value.Add(null);
                }
                if (rpl is Array)
                    this.value.InsertRange(this.value.Count, (Array)rpl);
                else
                    this.value.Insert(beg, rpl);
            }
            else
            {
                long alen;

                if (beg + len > this.Count)
                {
                    len = this.Count - beg;
                }

                alen = this.Count + rlen - len;

                this.value.RemoveRange(beg, len);

                if (rlen > 0)
                {
                    if (rpl is Array)
                    {
                        this.value.InsertRange(beg, (Array)rpl);
                    }
                    else
                        this.value.Insert(beg, rpl);
                }
            }
        }

        internal Array rb_ary_subseq(int beg, int len)
        {
            if (beg > this.Count)
                return null;
            if (beg < 0 || len < 0)
                return null;

            if (beg + len > this.Count)
            {
                len = this.Count - beg;
                if (len < 0)
                    len = 0;
            }

            if (len == 0) 
                return new Array();

            return new Array(this.value.GetRange(beg, len));
        }

        internal object rb_ary_entry(int offset)
        {
            if (offset < 0)
            {
                offset += this.Count;
            }

            if (this.Count == 0)
                return null;
            if (offset < 0 || this.Count <= offset)
            {
                return null;
            }

            return this[offset];
        }

        internal void rb_ary_store(int idx, object val, Frame caller)
        {
            if (idx < 0)
            {
                idx += this.Count;
                if (idx < 0)
                {
                    throw new IndexError(string.Format("index {0} out of array", idx - this.Count)).raise(caller);
                }
            }

            Array.rb_ary_modify(caller, this);
            
            while (idx >= this.Count)
                this.value.Add(null);

            this[idx] = val;
        }
    }
}
