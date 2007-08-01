/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using System.Collections;
using System.Text;

namespace Ruby.Runtime
{
    // Ruby.ArgList used to pass a list of arguments to a Ruby method or block call

    [UsedByRubyCompiler]
    public class ArgList : ICollection
    {
        private int offset = 0;

        [UsedByRubyCompiler]
        public bool single_arg = false;
        // We need to distinguish between foo(42) and foo(*[42])
        // Both produce a list of length 1, but they have different semantics
        // when passed to a block.

        internal ArrayList list;

        [UsedByRubyCompiler]
        public Proc block = null;


        [UsedByRubyCompiler]
        public ArgList()
        {
            this.list = new ArrayList();
        }

        internal ArgList(Proc block, params object[] array)
        {
            this.block = block;
            this.list = new ArrayList(array);
        }

        internal object this[int i]
        {
            get { System.Diagnostics.Debug.Assert(i < list.Count);  return list[i]; }
            set { list[i] = value; }
        }

        [UsedByRubyCompiler]
        public int Length
        {
            get { return list.Count; }
        }

        [UsedByRubyCompiler]
        public void Add(object element)
        {
            list.Add(element);
        }

        internal void AddRange(ICollection elements)
        {
            list.AddRange(elements);
        }

        [UsedByRubyCompiler]
        public void AddArray(object obj, Frame caller)
        {
            Array array;
            if (Array.TryToArray(obj, out array, caller))
                list.AddRange(array.value);
            else
                list.Add(obj);
        }

        [UsedByRubyCompiler]
        public bool RunOut()
        {
            return offset >= list.Count;
        }

        [UsedByRubyCompiler]
        public object GetNext()
        {
            if (offset < list.Count)
                return list[offset++];
            else
                return null;
        }

        [UsedByRubyCompiler]
        public Array GetRest()
        {
            Array array = new Array();
            while (offset < list.Count)
                array.Add(list[offset++]);
            return array;
        }

        [UsedByRubyCompiler]
        public ArgList CheckSingleRHS()
        {
            if (single_arg && list[0] is Array)
                list = new ArrayList((Array)list[0]);
            return this;
        }

        [UsedByRubyCompiler]
        public object ToRubyObject()
        {
            switch (list.Count)
            {
                case 0:
                    return null;
                case 1:
                    return list[0];
                default:
                    return new Array(list);
            }
        }

        [UsedByRubyCompiler]
        public Array ToRubyArray()
        {
            return new Array(list);
        }

        internal object[] ToArray()
        {
            return list.ToArray();
        }

        #region ICollection

        void ICollection.CopyTo(System.Array array, int index)
        {
            list.CopyTo(array, index);
        }

        int ICollection.Count
        {
            get { return list.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return list.IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return list.SyncRoot; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
        #endregion
    }
}
