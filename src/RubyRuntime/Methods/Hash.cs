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
    using KeyValuePair = System.Collections.Generic.KeyValuePair<Dictionary.Key, object>;
    using System.Globalization;


    
    internal class rb_hash_s_create : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_hash_s_create singleton = new rb_hash_s_create();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Hash hash;

            if ((rest.Count == 1) && (rest[0] is Hash))
            {
                hash = new Hash((Class)recv);
                hash.value = ((Hash)rest[0]).value;
                return hash;
            }

            if (rest.Count % 2 != 0)
                throw new ArgumentError("odd number of arguments for Hash").raise(caller);

            hash = new Hash((Class)recv);
            for (int i = 0; i < rest.Count; i += 2)
                rb_hash_aset.singleton.Call2(last_class, hash, caller, null, rest[i], rest[i + 1]);

            return hash;
        }
    }


    
    internal class rb_hash_initialize : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_hash_initialize singleton = new rb_hash_initialize();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array args)
        {
            Hash hash = (Hash)recv;

            Hash.rb_hash_modify(caller, hash);

            if (block != null)
            {
                if (args.Count > 0)
                    throw new ArgumentError("wrong number of arguments").raise(caller);
                hash.defaultProc = block;
            }
            else
            {
                if (Class.rb_scan_args(caller, args, 0, 1, false) > 0)
                    hash.defaultValue = args[0];
            }

            return hash;
        }
    }


    
    internal class hash_alloc : MethodBody0 //status: done
    {
        internal static hash_alloc singleton = new hash_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Hash((Class)recv);
        }
    }


    
    internal class rb_hash_clear : MethodBody0 //status: done
    {
        internal static rb_hash_clear singleton = new rb_hash_clear();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Hash hash = ((Hash)recv);

            Hash.rb_hash_modify(caller, hash);

            hash.value.Clear();

            return recv;
        }
    }

    
    internal class rb_hash_invert : MethodBody0 //status: done
    {
        internal static rb_hash_invert singleton = new rb_hash_invert();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Hash hash = (Hash)recv;
            Dictionary newDict = new Dictionary();

            foreach (KeyValuePair pair in hash.value)
                newDict[new Dictionary.Key(pair.Value)] = pair.Key.key;
            
            return new Hash(newDict);
        }
    }


    
    internal class rb_hash_has_key : MethodBody1 //status: done
    {
        internal static rb_hash_has_key singleton = new rb_hash_has_key();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return ((Hash)recv).value.ContainsKey(new Dictionary.Key(param0));
        }
    }


    
    internal class rb_hash_has_value : MethodBody1 //status: done
    {
        internal static rb_hash_has_value singleton = new rb_hash_has_value();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            Hash hash = (Hash)recv;
            Dictionary dict = hash.value;

            foreach (object value in dict.Values)
            {
                bool equal = Object.Equal(param0, value, caller);
                if (!object.ReferenceEquals(hash.value, dict))
                    throw new RuntimeError("rehash occurred during iteration").raise(caller);
                if (equal)
                    return true;
            }

            return false;
        }
    }


    
    internal class rb_hash_to_a : MethodBody0 //status: done
    {
        internal static rb_hash_to_a singleton = new rb_hash_to_a();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Dictionary dictionary = ((Hash)recv).value;
            System.Collections.ArrayList ary = new System.Collections.ArrayList(dictionary.Count);

            foreach (Dictionary.Key key in dictionary.Keys)
                ary.Add(new Array(key.key, dictionary[key]));

            return Array.CreateUsing(ary);
        }
    }


    
    internal class rb_hash_to_hash : MethodBody0 //status: done
    {
        internal static rb_hash_to_hash singleton = new rb_hash_to_hash();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return recv;
        }
    }


    
    internal class rb_hash_values : MethodBody0 //status: done
    {
        internal static rb_hash_values singleton = new rb_hash_values();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array newarray = new Array();

            foreach (object value in ((Hash)recv).value.Values)
                newarray.Add(value);

            return newarray;
        }
    }


    
    internal class rb_hash_shift : MethodBody0 //status: done
    {
        internal static rb_hash_shift singleton = new rb_hash_shift();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Hash hash = (Hash)recv;

            Hash.rb_hash_modify(caller, hash);

            Dictionary dict = hash.value;

            // NB. we just want the first pair if it exists
            foreach (KeyValuePair pair in dict)
            {
                dict.Remove(pair.Key);
                return new Array(pair.Key.key, pair.Value);
            }

            if (hash.defaultProc != null)
                return Proc.rb_yield(hash.defaultProc, caller, hash, null);
            else
                return hash.defaultValue;
        }
    }


    
    internal class rb_hash_delete : MethodBody1 //status: done
    {
        internal static rb_hash_delete singleton = new rb_hash_delete();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            Hash hash = (Hash)recv;

            Hash.rb_hash_modify(caller, hash);

            Dictionary.Key key = new Dictionary.Key(param0);
            Dictionary dict = hash.value;
            object value;

            if (dict.TryGetValue(key, out value))
            {
                dict.Remove(key);
                return value;
            }
            else if (block != null)
            {
                return Proc.rb_yield(block, caller, param0);
            }
            else
            {
                return null;
            }
        }
    }


    
    internal class rb_hash_delete_if : MethodBody0 //status: done
    {
        internal static rb_hash_delete_if singleton = new rb_hash_delete_if();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Hash hash = ((Hash)recv);

            Hash.rb_hash_modify(caller, hash);

            Dictionary dict = hash.value;
            System.Collections.Generic.List<KeyValuePair> list = new System.Collections.Generic.List<KeyValuePair>(dict.Count);
            list.AddRange(dict);

            foreach (KeyValuePair pair in list)
                if (Eval.Test(Proc.rb_yield(block, caller, pair.Key.key, pair.Value)))
                    dict.Remove(pair.Key);

            return recv;
        }
    }

    
    internal class rb_hash_select : MethodBody0 //status: done
    {
        internal static rb_hash_select singleton = new rb_hash_select();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Hash hash = (Hash)recv;
            Dictionary dict = hash.value;

            System.Collections.ArrayList result = new System.Collections.ArrayList();

            foreach (KeyValuePair pair in dict)
            {
                if (Eval.Test(Proc.rb_yield(block, caller, pair.Key.key, pair.Value)))
                    result.Add(new Array(pair.Key.key, pair.Value));
                if (!object.ReferenceEquals(hash.value, dict))
                    throw new RuntimeError("rehash occurred during iteration").raise(caller);
            }

            return Array.CreateUsing(result);
        }
    }


    
    internal class rb_hash_reject : MethodBody0 //status: done
    {
        internal static rb_hash_reject singleton = new rb_hash_reject();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Dictionary dict = new Dictionary();

            foreach (KeyValuePair pair in ((Hash)recv).value)
                if (!Eval.Test(Proc.rb_yield(block, caller, pair.Key.key, pair.Value)))
                    dict.Add(pair.Key, pair.Value);

            return Hash.CreateUsing(dict);
        }
    }


    
    internal class rb_hash_reject_bang : MethodBody0 //status: done
    {
        internal static rb_hash_reject_bang singleton = new rb_hash_reject_bang();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Dictionary dict = ((Hash)recv).value;
            int n = dict.Count;
            rb_hash_delete_if.singleton.Call0(last_class, recv, caller, block);

            if (n == dict.Count)
                return null;
            else
                return recv;
        }
    }


    
    internal class rb_hash_replace : MethodBody1 //status: done
    {
        internal static rb_hash_replace singleton = new rb_hash_replace();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            Hash hash = (Hash)recv;
            Dictionary dict = hash.value;
            Hash hash2 = Hash.to_hash(param0, caller);
            Dictionary dict2 = hash2.value;

            dict.Clear();

            foreach (KeyValuePair pair in dict2)
                dict[pair.Key] = pair.Value;
            hash.defaultProc = hash2.defaultProc;
            hash.defaultValue = hash2.defaultValue;

            return recv;
        }
    }


    
    internal class rb_hash_keys : MethodBody0 //status: done
    {
        internal static rb_hash_keys singleton = new rb_hash_keys();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array newarray = new Array();

            foreach (Dictionary.Key key in ((Hash)recv).value.Keys)
                newarray.Add(key.key);

            return newarray;
        }
    }



    internal class rb_hash_each : MethodBody0 // author: js, status: done
    {
        internal static rb_hash_each singleton = new rb_hash_each();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Hash hash = (Hash)recv;
            Dictionary dict = hash.value;

            // Don't lock the original dictionary.
            Dictionary copy = new Dictionary(dict);

            foreach (KeyValuePair pair in copy)
            {
                Proc.rb_yield(block, caller, new Array(pair.Key.key, pair.Value));
                if (!object.ReferenceEquals(hash.value, dict))
                    throw new RuntimeError("rehash occurred during iteration").raise(caller);
            }

            return recv;
        }
    }



    internal class rb_hash_each_pair : MethodBody0 // author: js, status: done
    {
        internal static rb_hash_each_pair singleton = new rb_hash_each_pair();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Hash hash = (Hash)recv;
            Dictionary dict = hash.value;

            // Don't lock the original dictionary.
            Dictionary copy = new Dictionary(dict);

            foreach (KeyValuePair pair in copy)
            {
                Proc.rb_yield(block, caller, pair.Key.key, pair.Value);
                if (!object.ReferenceEquals(hash.value, dict))
                    throw new RuntimeError("rehash occurred during iteration").raise(caller);
            }

            return recv;
        }
    }



    internal class rb_hash_each_key : MethodBody0 // author: js, status: done
    {
        internal static rb_hash_each_key singleton = new rb_hash_each_key();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Hash hash = (Hash)recv;
            Dictionary dict = hash.value;

            // Don't lock the original dictionary.
            Dictionary.Key[] keys = new Dictionary.Key[dict.Count];
            dict.Keys.CopyTo(keys, 0);

            foreach (Dictionary.Key key in keys)
            {
                Proc.rb_yield(block, caller, key.key);
                if (!object.ReferenceEquals(hash.value, dict))
                    throw new RuntimeError("rehash occurred during iteration").raise(caller);
            }

            return recv;
        }
    }



    internal class rb_hash_each_value : MethodBody0 // author: js, status: done
    {
        internal static rb_hash_each_value singleton = new rb_hash_each_value();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Hash hash = (Hash)recv;
            Dictionary dict = hash.value;

            // Don't lock the original dictionary.
            object[] values = new object[dict.Count];
            dict.Values.CopyTo(values, 0);

            foreach (object value in values)
            {
                Proc.rb_yield(block, caller, value);
                if (!object.ReferenceEquals(hash.value, dict))
                    throw new RuntimeError("rehash occurred during iteration").raise(caller);
            }

            return recv;
        }
    }



    internal class rb_hash_sort : MethodBody0 // author: cjs, status: done
    {
        internal static rb_hash_sort singleton = new rb_hash_sort();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array array = (Array)rb_hash_to_a.singleton.Call0(last_class, recv, caller, null);

            rb_ary_sort_bang.singleton.Call0(last_class, array, caller, block);

            return array;
        }
    }


    
    internal class rb_hash_size : MethodBody0 //status: done
    {
        internal static rb_hash_size singleton = new rb_hash_size();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Hash)recv).value.Count;
        }
    }


    
    internal class rb_hash_empty_p : MethodBody0 //status: done
    {
        internal static rb_hash_empty_p singleton = new rb_hash_empty_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Hash)recv).value.Count == 0;
        }
    }


    
    internal class rb_hash_aset : MethodBody2 //status: done
    {
        internal static rb_hash_aset singleton = new rb_hash_aset();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object param0, object val)
        {
            Hash hash = ((Hash)recv);

            Hash.rb_hash_modify(caller, hash);

            Dictionary.Key key = new Dictionary.Key(param0);

            hash.value[key] = val;

            ((Hash)recv).value[new Dictionary.Key(param0)] = val;
            return val;
        }
    }

    
    internal class rb_hash_default : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_hash_default singleton = new rb_hash_default();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            object key = null;

            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 1)
                key = rest[0];

            Hash hash = (Hash)recv;
            if (hash.defaultProc != null)
            {
                return Proc.rb_yield(hash.defaultProc, caller, hash, key);
            }
            else
                return hash.defaultValue;
        }
    }

    
    internal class rb_hash_set_default : MethodBody1 // author: cjs, status: done
    {
        internal static rb_hash_set_default singleton = new rb_hash_set_default();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Hash hash = (Hash)recv;

            Hash.rb_hash_modify(caller, hash);

            hash.defaultValue = p1;
            hash.defaultProc = null;
            return hash.defaultValue;
        }
    }

    
    internal class rb_hash_default_proc : MethodBody0 // author: cjs, status: done
    {
        internal static rb_hash_default_proc singleton = new rb_hash_default_proc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Hash)recv).defaultProc;
        }
    }

    
    internal class rb_hash_index : MethodBody1 //status: done
    {
        internal static rb_hash_index singleton = new rb_hash_index();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            Hash hash = (Hash)recv;
            Dictionary dict = hash.value;

            foreach (KeyValuePair pair in dict)
            {
                bool equal = Object.Equal(pair.Value, param0, caller);
                if (!object.ReferenceEquals(hash.value, dict))
                    throw new RuntimeError("rehash occurred during iteration").raise(caller);
                if (equal)
                    return pair.Key.key;
            }

            return null;
        }
    }

    
    internal class rb_hash_values_at : MethodBody //status: done
    {
        internal static rb_hash_values_at singleton = new rb_hash_values_at();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            System.Collections.ArrayList result = new System.Collections.ArrayList(args.Length);

            for (int i = 0; i < args.Length; i++)
                result.Add(rb_hash_aref.singleton.Call1(last_class, recv, caller, null, args[i]));

            return Array.CreateUsing(result);
        }
    }

    
    internal class rb_hash_indexes : MethodBody //status: done
    {
        internal static rb_hash_indexes singleton = new rb_hash_indexes();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            Errors.rb_warn(string.Format(CultureInfo.InvariantCulture, "Hash#{0} is deprecated; use Hash#values_at", "indexes")); //rb_frame_last_func

            System.Collections.ArrayList result = new System.Collections.ArrayList(args.Length);

            for (int i = 0; i < args.Length; i++)
                result.Add(rb_hash_aref.singleton.Call1(last_class, recv, caller, null, args[i]));

            return Array.CreateUsing(result);
        }
    }


    
    internal class rb_hash_update : MethodBody1 //status: done
    {
        internal static rb_hash_update singleton = new rb_hash_update();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            Dictionary dict = ((Hash)recv).value;
            Dictionary dict2 = Hash.to_hash(param0, caller).value;

            if (block != null)
            {
                foreach (KeyValuePair pair in dict2)
                {
                    object value;
                    if (dict.TryGetValue(pair.Key, out value))
                        value = Proc.rb_yield(block, caller, pair.Key.key, value, pair.Value);
                    else
                        value = pair.Value;
                    dict[pair.Key] = value;
                }
            }
            else
            {
                foreach (KeyValuePair pair in dict2)
                    dict[pair.Key] = pair.Value;
            }

            return recv;
        }
    }

    
    internal class rb_hash_rehash : MethodBody0 //status: done
    {
        internal static rb_hash_rehash singleton = new rb_hash_rehash();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Hash hash = (Hash)recv;

            Hash.rb_hash_modify(caller, hash);

            Dictionary newDict = new Dictionary();

            foreach (KeyValuePair pair in hash.value)
                newDict.Add(pair.Key, pair.Value);

            hash.value = newDict;

            return hash;
        }
    }

    
    internal class rb_hash_to_s : MethodBody0 //status: done
    {
        internal static rb_hash_to_s singleton = new rb_hash_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if ((bool)Ruby.Methods.rb_obj_inspect.rb_inspecting_p(last_class, recv, caller, block))
                return new String("{...}");
            return rb_ary_to_s.singleton.Call0(last_class, rb_hash_to_a.singleton.Call0(last_class, recv, caller, null), caller, null);
        }
    }

    
    internal class rb_hash_inspect : MethodBody0 //status: done
    {
        internal static rb_hash_inspect singleton = new rb_hash_inspect();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Hash hash = (Hash)recv;
            if (hash.value.Count == 0)
                return new String("{}");
            else if (Hash.IsInspecting(recv))
                return new String("{...}");
            else
            {
                try
                {
                    bool taint = hash.Tainted;
                    Hash.StartInspect(recv);
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.Append('{');

                    foreach (KeyValuePair pair in hash.value)
                    {
                        if (sb.Length > 1)
                            sb.Append(", ");

                        String key = Object.Inspect(pair.Key.key, caller);
                        taint |= key.Tainted;
                        sb.Append(key.value);
                        sb.Append("=>");
                        String val = Object.Inspect(pair.Value, caller);
                        taint |= val.Tainted;
                        sb.Append(val.value);
                    }

                    sb.Append('}');

                    String result = new String(sb.ToString());
                    result.Tainted = taint;
                    return result;
                }
                finally
                {
                    Hash.EndInspect(recv);
                }
            }
        }
    }

    
    internal class rb_hash_merge : MethodBody1 //status: done
    {
        internal static rb_hash_merge singleton = new rb_hash_merge();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            return rb_hash_update.singleton.Call1(last_class, rb_obj_dup.singleton.Call0(last_class, recv, caller, null), caller, null, param0);
        }
    }

    
    internal class rb_hash_aref : MethodBody1 //status: done
    {
        internal static rb_hash_aref singleton = new rb_hash_aref();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            object value;
            if (((Hash)recv).value.TryGetValue(new Dictionary.Key(param0), out value))
                 return value;
            else
                return Eval.CallPrivate(recv, caller, "default", null, param0);
        }
    }

    
    internal class rb_hash_fetch : VarArgMethodBody0 // author: js, status: done
    {
        internal static rb_hash_fetch singleton = new rb_hash_fetch();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class.rb_scan_args(caller, rest, 1, 1, false);
            object param0 = rest[0];

            object value;
            if (((Hash)recv).value.TryGetValue(new Dictionary.Key(param0), out value))
            {
                return value;
            }
            else if (block == null)
            {
                if (rest.Count > 1)
                    return rest[1];
                else
                    throw new IndexError("key not found").raise(caller);
            }
            else
            {
                if (rest.Count > 1)
                    Errors.rb_warn("block supersedes default value argument");
                return Proc.rb_yield(block, caller, param0);
            }
        }
    }

    
    internal class rb_hash_equal : MethodBody1 //status: done
    {
        internal static rb_hash_equal singleton = new rb_hash_equal();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (object.ReferenceEquals(recv, param0))
            {
                return true;
            }
            else if (!(param0 is Hash))
            {
                if (!Eval.RespondTo(param0, "to_hash"))
                    return false;
                else
                    return Object.Equal(param0, recv, caller);
            }
            else
            {
                Hash hash1 = (Hash)recv;
                Dictionary dict1 = hash1.value;
                Dictionary dict2 = ((Hash)param0).value;

                if (dict1.Count != dict2.Count)
                    return false;

                foreach (KeyValuePair pair in dict1)
                {
                    object value;
                    bool equal = dict2.TryGetValue(pair.Key, out value) && Object.Equal(pair.Value, value, caller);
                    if (!object.ReferenceEquals(hash1.value, dict1))
                        throw new RuntimeError("rehash occurred during iteration").raise(caller);
                    if (!equal)
                        return false;
                }

                return true;
            }
        }
    }
}
