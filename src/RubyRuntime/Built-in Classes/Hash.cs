/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using System.Collections;

namespace Ruby
{
    [UsedByRubyCompiler]
    public partial class Hash : Basic
    {
        internal Dictionary value = new Dictionary();
        internal object defaultValue = null;
        internal Proc defaultProc = null;


        //-----------------------------------------------------------------

        internal static object envtbl;


        //-----------------------------------------------------------------

        [UsedByRubyCompiler]
        public Hash()
            : base(Ruby.Runtime.Init.rb_cHash)
        {
        }

        internal Hash(Class klass): base(klass) 
        {
        }

        internal Hash(Dictionary value): this()
        {
            this.value = value; // Fixme: should we be cloning here???
        }


        //-----------------------------------------------------------------

        internal override object Inner()
        {
            return value;
        }

        [UsedByRubyCompiler]
        public void Add(object key, object data)
        {
            value[new Dictionary.Key(key)] = data;
        }


        //-----------------------------------------------------------------

        internal static void rb_hash_modify(Frame caller, Hash hash)
        {
            if (hash.value == null)
                throw new TypeError("uninitialized Hash").raise(caller);
            if (hash.Frozen)
                throw TypeError.rb_error_frozen(caller, "hash").raise(caller);
            if (!hash.Tainted && Eval.rb_safe_level() >= 4)
                throw new SecurityError("Insecure: can't modify hash").raise(caller);
        }

        internal static Hash CreateUsing(Dictionary value)
        {
            return new Hash(value);
        }

        internal static Dictionary HashValue(object obj, Frame caller)
        {
            if (obj is Hash)
                return ((Hash)obj).value;

            return Object.Convert<Hash>(obj, "to_hash", caller).value;
        }

        internal static void StartInspect(object obj)
        {
            System.LocalDataStoreSlot slot = System.Threading.Thread.GetNamedDataSlot("Hash.Inspecting");
            ArrayList inspecting = (ArrayList)System.Threading.Thread.GetData(slot);

            if (inspecting == null)
                System.Threading.Thread.SetData(slot, inspecting = new ArrayList());

            inspecting.Add(obj);
        }

        internal static void EndInspect(object obj)
        {
            System.LocalDataStoreSlot slot = System.Threading.Thread.GetNamedDataSlot("Hash.Inspecting");
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
            System.LocalDataStoreSlot slot = System.Threading.Thread.GetNamedDataSlot("Hash.Inspecting");
            ArrayList inspecting = (ArrayList)System.Threading.Thread.GetData(slot);

            if (inspecting != null)
            {
                for (int i = 0; i < inspecting.Count; i++)
                    if (Object.ReferenceEquals(obj, inspecting[i]))
                        return true;
            }

            return false;
        }
    }

    
    internal class Dictionary : System.Collections.Generic.Dictionary<Dictionary.Key, object>
    {
        
        internal struct Key
        {
            internal object key;

            internal Key(object key)
            {
                this.key = key;
            }

            public override int GetHashCode()
            {
                if (key == null || key is Object || key is Basic)
                    return (int)Eval.CallPrivate(key, null, "hash", null);
                else
                    return key.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is Key)
                {
                    object otherKey = ((Key)obj).key;

                    if (key == null || key is Object || key is Basic)
                        return (bool)Eval.CallPrivate(key, null, "eql?", null, otherKey);
                    else
                        return key.Equals(otherKey);
                }
                else
                    return false;
            }
        }

        public Dictionary()
            : base()
        {
        }

        public Dictionary(Dictionary dictionary)
            : base(dictionary)
        {
        }
    }
}
