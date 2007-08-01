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

namespace Ruby.Methods
{
    
    internal class rb_f_getenv : MethodBody1 //author: cjs, status: done
    {
        internal static rb_f_getenv singleton = new rb_f_getenv();

        public override object Call1(Class klass, object recv, Frame caller, Proc block, object p1)
        {
            string name = String.StringValue(p1, caller);
            string val = System.Environment.GetEnvironmentVariable(name);

            if (val == null)
                return null;
            else if (Env.PATH_ENV.Equals(name, System.StringComparison.OrdinalIgnoreCase) && !Env.rb_env_path_tainted())
            {
                String s = new String(val);
                Object.rb_obj_freeze(caller, s);
                return s;
            }
            else
                return Env.env_str_new2(caller, val);
        }
    }


    
    internal class env_fetch : VarArgMethodBody0 //author: cjs, status: done
    {
        internal static env_fetch singleton = new env_fetch();

        public override object Call(Class klass, object recv, Frame caller, Proc block, Array rest)
        {
            int count = Class.rb_scan_args(caller, rest, 1, 1, false);

            if (block != null && count == 2)
            {
                Errors.rb_warn("block supersedes default value argument");
            }

            object if_none = null;
            if (count == 2)
                if_none = rest[1];

            string name = String.StringValue(rest[0], caller);
            string val = System.Environment.GetEnvironmentVariable(name);

            if (val == null)
            {
                if (block != null)
                    return Proc.rb_yield(block, caller, rest[0]);

                if (if_none != null)
                    return if_none;

                throw new IndexError("key not found").raise(caller);
            }
            else if (Env.PATH_ENV.Equals(name, System.StringComparison.OrdinalIgnoreCase) && !Env.rb_env_path_tainted())
                return new String(val);
            else
                return Env.env_str_new2(caller, val);
        }
    }


    
    internal class env_aset : MethodBody2 //author: cjs, status: done
    {
        internal static env_aset singleton = new env_aset();

        public override object Call2(Class klass, object recv, Frame caller, Proc block, object key, object val)
        {
            if (Eval.rb_safe_level() >= 4)
            {
                throw new SecurityError("cannot change environment variable").raise(caller);
            }

            if (val == null)
            {
                System.Environment.SetEnvironmentVariable(Symbol.rb_to_string(caller, key), null);
                return null;
            }

            string sval = String.StringValue(val, caller);
            System.Environment.SetEnvironmentVariable(String.StringValue(key, caller), sval);

            if (Env.PATH_ENV.Equals(sval, System.StringComparison.OrdinalIgnoreCase))
                if (((Object)val).Tainted)
                    Env.path_tainted = 1;
                else
                    Env.path_tainted = 0; // path_tainted_p will always return 0 on windows

            return val;
        }
    }


    
    internal class env_each : MethodBody0 //author: cjs, status: done
    {
        internal static env_each singleton = new env_each();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            if (block == null)
                throw new LocalJumpError("no block given").raise(caller);

            Env.env_each_i(caller, block, false);

            return recv;
        }
    }


    
    internal class env_each_pair : MethodBody0 //author: cjs, status: done
    {
        internal static env_each_pair singleton = new env_each_pair();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            if (block == null)
                throw new LocalJumpError("no block given").raise(caller);

            Env.env_each_i(caller, block, true);

            return recv;
        }
    }


    
    internal class env_each_key : MethodBody0 //author: cjs, status: done
    {
        internal static env_each_key singleton = new env_each_key();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            if (block == null)
                throw new LocalJumpError("no block given").raise(caller);

            Array keys = (Array)env_keys.singleton.Call0(klass, recv, caller, null);
            foreach (object key in keys)
                Proc.rb_yield(block, caller, key);

            return recv;
        }
    }


    
    internal class env_each_value : MethodBody0 //author: cjs, status: done
    {
        internal static env_each_value singleton = new env_each_value();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            if (block == null)
                throw new LocalJumpError("no block given").raise(caller);

            Array values = (Array)env_values.singleton.Call0(klass, recv, caller, null);
            foreach (object value in values)
                Proc.rb_yield(block, caller, value);

            return recv;
        }
    }


    
    internal class env_delete_m : MethodBody1 //author: cjs, status: done
    {
        internal static env_delete_m singleton = new env_delete_m();

        public override object Call1(Class klass, object recv, Frame caller, Proc block, object p1)
        {
            object val = Env.env_delete(caller, p1);

            if (val != null && block != null)
                Proc.rb_yield(block, caller, val);

            return val;
        }
    }


    
    internal class env_delete_if : MethodBody0 //author: Brian, status: done
    {
        internal static env_delete_if singleton = new env_delete_if();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            env_reject_bang.singleton.Call0(klass, recv, caller, block);
            return recv;
        }
    }


    
    internal class env_clear : MethodBody0 //author: cjs, status: done
    {
        internal static env_clear singleton = new env_clear();

        public override object Calln(Class klass, object recv, Frame caller, ArgList args)
        {
            Eval.rb_secure(4, caller);
            
            Array keys = (Array)env_keys.singleton.Call0(klass, recv, caller, null);
            foreach (object key in keys)
            {
                object val = rb_f_getenv.singleton.Call1(klass, recv, caller, null, key);
                if (val != null)
                    Env.env_delete(caller, val);
            }

            return recv;
        }
    }


    
    internal class env_reject : MethodBody //author:Brian, status: done
    {
        internal static env_reject singleton = new env_reject();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            return rb_hash_reject.singleton.Call0(klass, env_to_hash.singleton.Call0(klass, Hash.envtbl, caller, block), caller, block);
        }
    }


    
    internal class env_reject_bang : MethodBody0 //author: cjs, status: done
    {
        internal static env_reject_bang singleton = new env_reject_bang();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            int del = 0;

            Eval.rb_secure(4, caller);
            Array keys = (Array)env_keys.singleton.Call0(klass, recv, caller, null);

            foreach (object key in keys)
            {
                object value = rb_f_getenv.singleton.Call1(klass, recv, caller, null, key);
                if (value != null)
                {
                    if ((bool)Proc.rb_yield(block, caller, key, value))
                    {
                        ((Object)key).Tainted = false;
                        Env.env_delete(caller, key);
                        del++;
                    }
                }
            }

            if (del == 0)
                return null;

            return recv;
        }
    }


    
    internal class env_select : MethodBody0 //author: cjs, status: done
    {
        internal static env_select singleton = new env_select();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            if (block == null)
                throw new LocalJumpError("no block given").raise(caller);

            Array ary = new Array();

            foreach (System.Collections.DictionaryEntry pair in System.Environment.GetEnvironmentVariables())
            {
                String key = Env.env_str_new(caller, (string)pair.Key);
                String val = Env.env_str_new2(caller, (string)pair.Value);

                if ((bool)(Proc.rb_yield(block, caller, key, val)))
                    ary.Add(new Array(key, val));
            }

            return ary;
        }
    }


    
    internal class env_shift : MethodBody0 //author: Brian, status: done
    {
        internal static env_shift singleton = new env_shift();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            foreach (System.Collections.DictionaryEntry pair in System.Environment.GetEnvironmentVariables())
            {
                String key = Env.env_str_new(caller, (string)pair.Key);
                String value = Env.env_str_new2(caller, (string)pair.Value);

                Env.env_delete(caller, key);
                return new Array(key, value);
            }

            return null;
        }
    }


    
    internal class env_invert : MethodBody0 //author: Brian, status: done
    {
        internal static env_invert singleton = new env_invert();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            return rb_hash_invert.singleton.Call0(klass, env_to_hash.singleton.Call0(klass, Hash.envtbl, caller, block), caller, block);
        }
    }


    
    internal class env_replace : MethodBody1 //author: cjs, status: done
    {
        internal static env_replace singleton = new env_replace();

        public override object Call1(Class klass, object recv, Frame caller, Proc block, object p1)
        {
            if (recv == p1)
                return recv;

            Hash hash = Object.Convert<Hash>(p1, "to_hash", caller);

            Array keys = (Array)env_keys.singleton.Call0(klass, recv, caller, null);

            foreach (System.Collections.Generic.KeyValuePair<Dictionary.Key, object> pair in hash.value)
            {
                env_aset.singleton.Call2(klass, recv, caller, null, pair.Key.key, pair.Value);
                if ((bool)rb_ary_includes.singleton.Call1(klass, keys, caller, null, pair.Key.key))
                    rb_ary_delete.singleton.Call1(klass, keys, caller, null, pair.Key.key);
            }

            foreach (object key in keys)
                Env.env_delete(caller, key);

            return recv;
        }
    }


    
    internal class env_update : MethodBody1 //author: cjs, status: done
    {
        internal static env_update singleton = new env_update();

        public override object Call1(Class klass, object recv, Frame caller, Proc block, object p1)
        {
            if (recv == p1)
                return recv;

            Hash hash = Object.Convert<Hash>(p1, "to_hash", caller);

            Array keys = (Array)env_keys.singleton.Call0(klass, recv, caller, null);

            foreach (System.Collections.Generic.KeyValuePair<Dictionary.Key, object> pair in hash.value)
            {
                object val;
                if (block != null)
                    val = Proc.rb_yield(block, caller, pair.Key.key, rb_f_getenv.singleton.Call1(klass, recv, caller, null, pair.Key.key), pair.Value);
                else
                    val = pair.Value;
                env_aset.singleton.Call2(klass, recv, caller, null, pair.Key.key, val);
            }

            return recv;
        }
    }


    
    internal class env_inspect : MethodBody0 //author: cjs, status: done
    {
        internal static env_inspect singleton = new env_inspect();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder("{");

            bool first = true;
            foreach (System.Collections.DictionaryEntry pair in System.Environment.GetEnvironmentVariables())
            {
                if (first)
                    first = false;
                else
                    str.Append(", ");
                str.Append("\"");
                str.Append(pair.Key);
                str.Append("\"=>");
                str.Append(pair.Value);
            }

            str.Append("}");

            String s = new String(str.ToString());
            s.Tainted = true;

            return s;

            //return rb_hash_inspect.singleton.Call0(klass, env_to_hash.singleton.Call0(klass, recv, caller, block), caller, block);
        }
    }


    
    internal class env_none : MethodBody0 //author: Brian, status: done
    {
        internal static env_none singleton = new env_none();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            return null;
        }
    }


    
    internal class env_to_a : MethodBody0 //author: cjs, status: done
    {
        internal static env_to_a singleton = new env_to_a();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            Array ary = new Array();

            foreach (System.Collections.DictionaryEntry pair in System.Environment.GetEnvironmentVariables())
            {
                ary.Add(new Array(Env.env_str_new(caller, (string)pair.Key), Env.env_str_new2(caller, (string)pair.Value)));
            }

            return ary;
        }
    }


    
    internal class env_to_s : MethodBody1 //author: Brian, status: done
    {
        internal static env_to_s singleton = new env_to_s();

        public override object Calln(Class klass, object recv, Frame caller, ArgList args)
        {
            return new String("ENV");
        }
    }


    
    internal class env_index : MethodBody //author: cjs, status: done
    {
        internal static env_index singleton = new env_index();

        public override object Call1(Class klass, object recv, Frame caller, Proc block, object p1)
        {
            string value = String.StringValue(p1, caller);

            foreach (System.Collections.DictionaryEntry pair in System.Environment.GetEnvironmentVariables())
            {
                if (Object.Equals(pair.Value, value))
                    return Env.env_str_new(caller, (string)pair.Key);
            }

            return null;
        }

    }


    
    internal class env_indexes : VarArgMethodBody0 //author: cjs, status: done
    {
        internal static env_indexes singleton = new env_indexes();

        public override object Call(Class klass, object recv, Frame caller, Proc block, Array rest)
        {
            Errors.rb_warn(string.Format("ENV#{0} is deprecated; use ENV#values_at", "indexes")); // rb_frame_last_func

            Array ary = new Array();

            for (int i = 0; i < rest.Count; i++)
            {
                String tmp = String.rb_check_string_type(rest[i], caller);
                if (tmp == null)
                    ary.Add(null);
                else
                    ary.Add(Env.env_str_new2(caller, System.Environment.GetEnvironmentVariable(tmp.value)));
            }

            return ary;
        }
    }


    
    internal class env_size : MethodBody0 //author: Brian, status: done
    {
        internal static env_size singleton = new env_size();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            return (System.Environment.GetEnvironmentVariables().Count);
        }
    }


    
    internal class env_empty_p : MethodBody0 //author: Brian, status: done
    {
        internal static env_empty_p singleton = new env_empty_p();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            return (System.Environment.GetEnvironmentVariables().Count == 0);
        }
    }


    
    internal class env_keys : MethodBody0 //author: cjs, status: done
    {
        internal static env_keys singleton = new env_keys();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            Array keys = new Array();

            foreach (string key in System.Environment.GetEnvironmentVariables().Keys)
                keys.Add(Env.env_str_new(caller, key));

            return keys;
        }
    }


    
    internal class env_values : MethodBody //author: cjs, status: done
    {
        internal static env_values singleton = new env_values();

        public override object Calln(Class klass, object recv, Frame caller, ArgList args)
        {
            Array values = new Array();

            foreach (string val in System.Environment.GetEnvironmentVariables().Values)
                values.Add(Env.env_str_new2(caller, val));

            return values;
        }
    }


    
    internal class env_values_at : VarArgMethodBody0 //author: cjs, status: done
    {
        internal static env_values_at singleton = new env_values_at();

        public override object Call(Class klass, object recv, Frame caller, Proc block, Array rest)
        {
            Array ary = new Array();

            foreach (object key in rest)
            {
                object val = rb_f_getenv.singleton.Call1(klass, recv, caller, null, key);

                ary.Add(val);
            }

            return ary;
        }
    }


    
    internal class env_has_key : MethodBody1 //author: Brian, status: done
    {
        internal static env_has_key singleton = new env_has_key();

        public override object Call1(Class klass, object recv, Frame caller, Proc block, object p1)
        {
            return (System.Environment.GetEnvironmentVariable(Symbol.rb_to_string(caller, p1)) != null);
        }
    }


    
    internal class env_has_value : MethodBody1 //author: Brian, status: done
    {
        internal static env_has_value singleton = new env_has_value();

        public override object Call1(Class klass, object recv, Frame caller, Proc block, object p1)
        {
            if (!(p1 is String))
                return false;

            foreach (string val in System.Environment.GetEnvironmentVariables().Values)
            {
                if (val.Equals(Symbol.rb_to_string(caller, p1)))
                    return true;
            }

            return false;
        }
    }


    
    internal class env_to_hash : MethodBody0 //author: cjs, status: done
    {
        internal static env_to_hash singleton = new env_to_hash();

        public override object Call0(Class klass, object recv, Frame caller, Proc block)
        {
            Hash hash = new Hash();
            foreach (System.Collections.DictionaryEntry pair in System.Environment.GetEnvironmentVariables())
            {
                hash.Add(Env.env_str_new(caller, (string)pair.Key), Env.env_str_new2(caller, (string)pair.Value));
            }
            return hash;
        }
    }



}
