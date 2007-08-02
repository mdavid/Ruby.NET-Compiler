/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using System.Collections.Generic;

namespace Ruby
{

    public partial class Struct : Object
    {
        internal Struct(Class klass)
            : base(klass)
        {
        }


        internal static object rb_struct_new(Class last_klass, Class klass, Frame caller, Proc block, params object[] members)
        {
            object[] mem = new object[members.Length];

            for (int counter = 0; counter < members.Length; counter++)
            {
                mem[counter] = members[counter];
            }

            return Methods.rb_class_new_instance.singleton.Call(last_klass, klass, caller, block, new Array(mem));
        }

        internal static Class rb_struct_define(string name, Frame caller, params string[] members)
        {
            Array ary = new Array();
            foreach (string mem in members)
                ary.Add(new Symbol(mem));

            return make_struct(name, ary, Ruby.Runtime.Init.rb_cStruct, caller);
        }

        internal static Class make_struct(string name, Array members, Class klass, Frame caller)
        {
            // Compiler Bug???: Found bug here the following code crashes this method:
            // Customer = Struct.new( :name, :address, :zip )  

            // FIXME: OBJ_FREEZE(members);            
            Class nstr = null;

            if (name == null)
            {
                nstr = new Class(null, klass, Class.Type.Class);
                Class.rb_make_metaclass(nstr, klass.my_class);
                nstr.class_inherited(klass, caller);
            }
            else
            {
                if (!Symbol.is_const_id(name))
                {
                    throw new NameError("identifier " + name + " needs to be constant").raise(caller);
                }
                if (klass.const_defined(name, false))
                {
                    // rb_warn
                    klass.remove_const(name);
                }

                nstr = Class.rb_define_class_under(klass, name, klass, caller);
            }
            nstr.instance_variable_set("__size__", members.Count);
            nstr.instance_variable_set("__members__", members);

            Class.rb_define_alloc_func(nstr, Methods.struct_alloc.singleton);
            Class.rb_define_singleton_method(nstr, "new", Methods.rb_class_new_instance.singleton, -1, caller);
            Class.rb_define_singleton_method(nstr, "[]", Methods.rb_class_new_instance.singleton, -1, caller);
            Class.rb_define_singleton_method(nstr, "members", Methods.rb_struct_s_members.singleton, 0, caller);

            foreach (object m in members)
            {
                string id = Symbol.rb_to_id(caller, m);
                if (Symbol.is_local_id(id) || Symbol.is_const_id(id))
                {
                    Class.rb_define_method(nstr, id, new AttrReaderMethodBody(id), 0, caller);
                    Class.rb_define_method(nstr, id + "=", new AttrWriterMethodBody(id), 1, caller);
                }
            }

            return nstr;
        }

        internal static object rb_struct_aset_id(Frame caller, object s, string id, object val)
        {
            Class structClass = ((Object)s).my_class;
            rb_struct_modify(caller, s);
            Array members = (Array)(structClass.instance_variable_get("__members__"));
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].Equals(id))
                {
                    ((Object)s).instance_variable_set(id, val);
                    return val;
                }
            }

            throw new NameError("no member '" + id + "' in struct").raise(caller);
        }

        internal static void rb_struct_modify(Frame caller, object s)
        {
            if (((Basic)s).Frozen)
                throw TypeError.rb_error_frozen(caller, "Struct").raise(caller);
            if (!((Basic)s).Tainted && Eval.rb_safe_level() >= 4)
                throw new SecurityError("Insecure: can't modify Struct").raise(caller);
        }
    }
}
