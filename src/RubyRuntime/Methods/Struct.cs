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
    
    internal class rb_struct_s_def : VarArgMethodBody0 //author: Brian, status: done
    {
        internal static rb_struct_s_def singleton = new rb_struct_s_def();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            string name = null;
            string[] members = new string[0];

            /**
            if (rest.Count > 0)
            {
                if (!(rest[0] is Symbol))
                {
                    name = Symbol.rb_to_id(rest[0]);
                    if (rest.Count > 1)
                        rest = new Array(rest.value.GetRange(1, rest.Count - 1));
                }
            }
            */

            // BBTAG: the following is a temporary work-around as the compiler currently does not
            // differentiate between Symbols and Strings: if the first identifier is a valid const 
            // name, then we assume it is the name of the struct

            if (rest.Count > 0)
            {
                string id = Symbol.rb_to_id(caller, rest[0]);
                if (Symbol.is_const_id(id))
                {
                    name = id;
                    if (rest.Count > 1)
                        rest = new Array(rest.value.GetRange(1, rest.Count - 1));
                }
            }

            if (rest.Count > 0)
            {
                members = new string[rest.Count];

                for (int i = 0; i < rest.Count; i++)
                    members[i] = Symbol.rb_to_id(caller, rest[i]);
            }

            Class st = Struct.make_struct(name, new Array(members), Ruby.Runtime.Init.rb_cStruct, caller);

            if (block != null)
                rb_mod_module_eval.singleton.Call(last_class, st, caller, block, new Array());

            return st;
        }
    }

    
    internal class rb_struct_initialize : VarArgMethodBody0 //author: Brian, status: done
    {
        internal static rb_struct_initialize singleton = new rb_struct_initialize();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Object self = (Object)recv;
            Class structClass = Class.CLASS_OF(recv).class_real();
            if (!(structClass.instance_variable_get("__members__") is Array))
                throw new TypeError("members field of struct is not an Array").raise(caller);

            Array members = (Array)(structClass.instance_variable_get("__members__"));

            if (rest.Count > members.Count)
                throw new ArgumentError("struct size differs").raise(caller);

            for (int i = 0; i < members.Count; i++)
            {
                if (i < rest.Count)
                    self.instance_variable_set(Symbol.rb_to_id(caller, members[i]), rest[i]);
                else
                    self.instance_variable_set(Symbol.rb_to_id(caller, members[i]), null);
            }

            return null;
        }
    }

    
    internal class rb_struct_init_copy : MethodBody1 // author: Brian, status: done
    {
        internal static rb_struct_init_copy singleton = new rb_struct_init_copy();

        public override object Call1(Class last_class, object copy, Frame caller, Proc block, object s)
        {
            if (copy == s)
                return copy;

            TypeError.rb_check_frozen(caller, copy);
            if (!((bool)(rb_obj_is_instance_of.singleton.Call1(last_class, s, caller, block, Class.CLASS_OF(copy).class_real()))))
            {
                throw new TypeError("wrong argument class").raise(caller);
            }

            Object copyStruct = (Object)copy;
            Object origStruct = (Object)s;
            Class structClass = origStruct.my_class;

            Array members = (Array)(structClass.instance_variable_get("__members__"));
            foreach (string id in members)
            {
                copyStruct.instance_variable_set(id, origStruct.instance_variable_get(id));
            }

            return copyStruct;
        }
    }

     
    internal class rb_struct_equal : MethodBody1 //author: Brian, status: done
    {
        internal static rb_struct_equal singleton = new rb_struct_equal();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (recv == p1) return true;
            if (!(p1 is Struct)) return false;
            if (Class.CLASS_OF(recv).class_real() != Class.CLASS_OF(p1).class_real()) return false;
            Class structClass = Class.CLASS_OF(recv).class_real();
            if (!(structClass.instance_variable_get("__members__") is Array))
                throw new TypeError("members field of struct is not an Array").raise(caller);

            Array members = (Array)(structClass.instance_variable_get("__members__"));

            foreach (object m in members)
            {
                string id = Symbol.rb_to_id(caller, m);
                if (!((bool)(Eval.CallPublic(((Object)recv).instance_variable_get(id), caller, "==", block, ((Object)p1).instance_variable_get(id)))))
                    return false;
            }

            return true;
        }
    }

    
    internal class rb_struct_eql : MethodBody1 //author: Brian, status: done
    {
        internal static rb_struct_eql singleton = new rb_struct_eql();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (recv == p1) return true;
            if (!(p1 is Struct)) return false;
            if (Class.CLASS_OF(recv).class_real() != Class.CLASS_OF(p1).class_real()) return false;
            Class structClass = Class.CLASS_OF(recv).class_real();
            if (!(structClass.instance_variable_get("__members__") is Array))
                throw new TypeError("members field of struct is not an Array").raise(caller);

            Array members = (Array)(structClass.instance_variable_get("__members__"));

            foreach (object m in members)
            {
                string id = Symbol.rb_to_id(caller, m);
                if (!((bool)(Eval.CallPublic(((Object)recv).instance_variable_get(id), caller, "eql?", block, ((Object)p1).instance_variable_get(id)))))
                    return false;
            }

            return true;
        }
    }

    
    internal class rb_struct_hash : MethodBody0 //author: Brian, status: done
    {
        internal static rb_struct_hash singleton = new rb_struct_hash();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class klass0 = Class.CLASS_OF(recv).class_real();
            object members = klass0.instance_variable_get("__members__");

            if (!(members is Array))
                throw new TypeError("members field of struct is not an Array").raise(caller);

            Array values = new Array();

            if (((Array)members).Count > 0)
            {
                foreach (object m in (Array)members)
                {
                    string id = Symbol.rb_to_id(caller, m);
                    values.Add(id);
                    values.Add(((Object)recv).instance_variable_get(id));
                }
            }

            return rb_ary_hash.singleton.Call0(last_class, values, caller, block);
        }
    }


    
    internal class rb_struct_inspect : MethodBody0 //author: Brian, status: done
    {
        internal static rb_struct_inspect singleton = new rb_struct_inspect();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class klass0 = Class.CLASS_OF(recv).class_real();
            string str = "#<struct " + klass0.classname();
            object members = klass0.instance_variable_get("__members__");

            if (!(members is Array))
                throw new TypeError("members field of struct is not an Array").raise(caller);

            if (((Array)members).Count > 0)
            {
                str += " ";
                bool flag = false;
                foreach (object m in (Array)members)
                {
                    string id = Symbol.rb_to_id(caller, m);
                    if (flag)
                        str += ", ";
                    str += id + "=" + Eval.CallPublic(((Object)recv).instance_variable_get(id), caller, "inspect", block);
                    flag = true;
                }
            }

            str += ">";
            return new String(str);
        }
    }


    
    internal class rb_struct_to_a : MethodBody0 //author: Brian, status: done
    {
        internal static rb_struct_to_a singleton = new rb_struct_to_a();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class klass0 = Class.CLASS_OF(recv).class_real();
            object members = klass0.instance_variable_get("__members__");

            if (!(members is Array))
                throw new TypeError("members field of struct is not an Array").raise(caller);

            Array res = new Array();
            if (((Array)members).Count > 0)
            {
                foreach (object m in (Array)members)
                {
                    string id = Symbol.rb_to_id(caller, m);
                    res.Add(((Object)recv).instance_variable_get(id));
                }
            }

            return res;
        }
    }


    
    internal class rb_struct_size : MethodBody0 //author: Brian, status: done
    {
        internal static rb_struct_size singleton = new rb_struct_size();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Class.CLASS_OF(recv).class_real().instance_variable_get("__size__");
        }
    }


    
    internal class rb_struct_each : MethodBody0 //author: Brian, status: done
    {
        internal static rb_struct_each singleton = new rb_struct_each();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class klass0 = Class.CLASS_OF(recv).class_real();
            object members = klass0.instance_variable_get("__members__");

            if (!(members is Array))
                throw new TypeError("members field of struct is not an Array").raise(caller);

            if (((Array)members).Count > 0)
            {
                foreach (object m in (Array)members)
                {
                    string id = Symbol.rb_to_id(caller, m);
                    object val = ((Object)recv).instance_variable_get(id);
                    if (block != null)
                        Proc.rb_yield(block, caller, val);
                    else
                        throw new LocalJumpError("no block given").raise(caller);
                }
            }

            return recv;
        }
    }

    
    internal class rb_struct_each_pair : MethodBody0 //author: Brian, status: done
    {
        internal static rb_struct_each_pair singleton = new rb_struct_each_pair();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class klass0 = Class.CLASS_OF(recv).class_real();
            object members = klass0.instance_variable_get("__members__");

            if (!(members is Array))
                throw new TypeError("members field of struct is not an Array").raise(caller);

            if (((Array)members).Count > 0)
            {
                foreach (object m in (Array)members)
                {
                    string id = Symbol.rb_to_id(caller, m);
                    object val = ((Object)recv).instance_variable_get(id);
                    if (block != null)
                        Proc.rb_yield(block, caller, new Symbol(id), val);
                    else
                        throw new LocalJumpError("no block given").raise(caller);
                }
            }

            return recv;
        }
    }

    
    internal class rb_struct_aref_id : MethodBody1 //author: Brian, status: done
    {
        internal static rb_struct_aref_id singleton = new rb_struct_aref_id();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Object self = (Object)recv;
            string id = Symbol.rb_to_id(caller, p1);

            Array members = (Array)(rb_struct_s_members.singleton.Call0(last_class, Class.CLASS_OF(recv).class_real(), caller, block));
            for (int i = 0; i < members.Count; i++)
            {
                if (Symbol.rb_to_id(caller, members[i]).Equals(id))
                    return self.instance_variable_get(id);
            }

            throw new NameError("no member '" + id + "' in struct").raise(caller);
        }
    }

    
    internal class rb_struct_aref : MethodBody1 //author: Brian, status: done
    {
        internal static rb_struct_aref singleton = new rb_struct_aref();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if ((p1 is String) || (p1 is Symbol))
                return rb_struct_aref_id.singleton.Call1(last_class, recv, caller, block, p1);

            Array members = (Array)(rb_struct_s_members.singleton.Call0(last_class, Class.CLASS_OF(recv).class_real(), caller, block));
            int i = Object.Convert<int>(p1, "to_i", caller);
            if (i < 0)
                i = members.Count + i;
            if (i < 0)
                throw new IndexError("offset " + i + " too small for struct(size:" + members.Count + ")").raise(caller);
            if (members.Count <= i)
                throw new IndexError("offset " + i + " too large for struct(size:" + members.Count + ")").raise(caller);

            return ((Object)recv).instance_variable_get(Symbol.rb_to_id(caller, members[i]));
        }
    }

    
    internal class rb_struct_aset : MethodBody2 //author: Brian, status: done
    {
        internal static rb_struct_aset singleton = new rb_struct_aset();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            if (p1 is String || p1 is Symbol)
            {
                return Struct.rb_struct_aset_id(caller, recv, Symbol.rb_to_id(caller, p1), p2);
            }

            Array members = (Array)(rb_struct_s_members.singleton.Call0(last_class, Class.CLASS_OF(recv).class_real(), caller, block));

            int i = (int)(Integer.rb_to_int(p1, caller));

            if (i < 0)
                i = members.Count + i;

            if (i < 0)
                throw new IndexError("offset " + i + " too small for struct(size:" + members.Count + ")").raise(caller);

            if (members.Count <= i)
                throw new IndexError("offset " + i + " too large for struct(size:" + members.Count + ")").raise(caller);

            Struct.rb_struct_modify(caller, recv);
            ((Object)recv).instance_variable_set(Symbol.rb_to_id(caller, members[i]), p2);
            return p2;
        }
    }

    // BBTAG: the Pickaxe documentation for this method is wrong - it doesn't accept any args
    
    internal class rb_struct_select : MethodBody0 //author: Brian, status: done
    {
        internal static rb_struct_select singleton = new rb_struct_select();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class klass0 = Class.CLASS_OF(recv).class_real();
            object members = klass0.instance_variable_get("__members__");

            if (!(members is Array))
                throw new TypeError("members field of struct is not an Array").raise(caller);

            Array res = new Array();

            if (((Array)members).Count > 0)
            {
                foreach (object m in (Array)members)
                {
                    string id = Symbol.rb_to_id(caller, m);
                    object val = ((Object)recv).instance_variable_get(id);

                    if (block != null)
                    {
                        if ((bool)(Proc.rb_yield(block, caller, val)))
                            res.Add(val);
                    }
                    else
                        throw new LocalJumpError("no block given").raise(caller);
                }
            }

            return res;
        }
    }


    
    internal class rb_struct_values_at : VarArgMethodBody0 //author: Brian, status: done
    {
        internal static rb_struct_values_at singleton = new rb_struct_values_at();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class klass0 = Class.CLASS_OF(recv).class_real();
            object members = klass0.instance_variable_get("__members__");

            if (!(members is Array))
                throw new TypeError("members field of struct is not an Array").raise(caller);

            Array values = new Array();

            if (((Array)members).Count > 0)
            {
                foreach (object m in (Array)members)
                {
                    string id = Symbol.rb_to_id(caller, m);
                    values.Add(((Object)recv).instance_variable_get(id));
                }
            }

            ArgList args = new ArgList();
            args.AddRange(rest.value);
            return rb_ary_values_at.singleton.Calln(last_class, values, caller, args);
        }
    }


    
    internal class rb_struct_members_m : MethodBody0 //author: Brian, status: done
    {
        internal static rb_struct_members_m singleton = new rb_struct_members_m();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class klass0 = Class.CLASS_OF(recv).class_real();
            Array members = (Array)rb_struct_s_members.singleton.Call0(last_class, klass0, caller, block);
            Array res = new Array();

            foreach (object m in members)
                res.Add(new String(Symbol.rb_to_id(caller, m)));

            return res;
        }
    }

    
    internal class struct_alloc : MethodBody0 //author: Brian, status: done
    {
        internal static struct_alloc singleton = new struct_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Struct((Class)recv);
        }
    }

    
    internal class rb_struct_s_members : MethodBody0 //author: Brian, status: done
    {
        internal static rb_struct_s_members singleton = new rb_struct_s_members();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            object members = ((Object)recv).instance_variable_get("__members__");

            if (members == null || !(members is Array))
                throw new Exception("BUG: uninitialized struct").raise(caller);

            return members;
        }
    }
}
