/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using Ruby.Runtime;
using System.Collections.Generic;
using System.Globalization;

namespace Ruby.Methods
{
    
    internal class rb_class_alloc : MethodBody0 // author: Brian, status:done
    {
        internal static rb_class_alloc singleton = new rb_class_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Class("", null, Class.Type.Class, (Class)recv);
        }
    }

    
    internal class rb_module_alloc : MethodBody0 //status:done
    {
        internal static rb_module_alloc singleton = new rb_module_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Class("rb_module_alloc", Ruby.Runtime.Init.rb_cObject, Class.Type.Class, (Class)recv);
        }
    }


    
    internal class rb_class_superclass : MethodBody0//status:done
    {
        internal static rb_class_superclass singleton = new rb_class_superclass();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Class)recv).super_real();
        }
    }


    
    internal class rb_class_new_instance : VarArgMethodBody0 // author: Brian, status: done
    {
        internal static rb_class_new_instance singleton = new rb_class_new_instance();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            object obj = Eval.CallPrivate(recv, caller, "allocator", block);
            Eval.CallPrivate(obj, caller, "initialize", block, rest.value.ToArray());
            return obj;
        }
    }

    
    internal class rb_mod_nesting : MethodBody0 //author: Brian, status: done
    {
        internal static rb_mod_nesting singleton = new rb_mod_nesting();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class[] nesting = caller.nesting();
            if (nesting == null)
                return new Array();
            else
                return new Array(nesting);
        }
    }


    internal class rb_mod_s_constants : MethodBody0//author:Brian, status:done
    {
        internal static rb_mod_s_constants singleton = new rb_mod_s_constants();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            // this functionality seems to be getting done in another method
            // could not find a testing sequence that made this fail. Although
            // Eval.getConst only calls it under certain circumstances. 
            return Init.rb_cObject.get_constants();
        }
    }

    
    internal class rb_mod_append_features : MethodBody1 // author: Brian, status: done
    {
        internal static rb_mod_append_features singleton = new rb_mod_append_features();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Class module = (Class)recv;

            Class.is_class_or_module(caller, p1, null);
            Class include = (Class)p1;
            include.include_module(caller, module);

            return module;
        }
    }

    
    internal class rb_mod_extend_object : MethodBody1 //author: Brian, status: done
    {
        internal static rb_mod_extend_object singleton = new rb_mod_extend_object();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Class.singleton_class(caller, p1).include_module(caller, (Class)recv);
            return p1;
        }
    }

    
    internal class rb_mod_include : VarArgMethodBody0//author: Brian, status:done
    {
        internal static rb_mod_include singleton = new rb_mod_include();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            foreach (object m in rest)
            {
                Class.check_type(caller, m, Class.Type.Module);
                Eval.CallPrivate((Class)m, caller, "append_features", block, recv);
                Eval.CallPrivate((Class)m, caller, "included", block, recv);
            }

            return recv;
        }
    }

    
    internal class rb_mod_public : VarArgMethodBody0//author: Brian, status: done
    {
        internal static rb_mod_public singleton = new rb_mod_public();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return Calln(last_class, recv, caller, new ArgList(block, rest.value.ToArray()));
        }

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            ((Class)recv).visibility_helper(caller, Access.Public, args);
            return recv;
        }
    }

    
    internal class rb_mod_protected : VarArgMethodBody0 //author: Brian, status: done
    {
        internal static rb_mod_protected singleton = new rb_mod_protected();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return Calln(last_class, recv, caller, new ArgList(block, rest.value.ToArray()));
        }

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            ((Class)recv).visibility_helper(caller, Access.Protected, args);
            return recv;
        }
    }

    
    internal class rb_mod_private : VarArgMethodBody0//author: Brian, status: done
    {
        internal static rb_mod_private singleton = new rb_mod_private();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return Calln(last_class, recv, caller, new ArgList(block, rest.value.ToArray()));
        }

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            ((Class)recv).visibility_helper(caller, Access.Private, args);
            return recv;
        }
    }

    
    internal class rb_mod_modfunc : VarArgMethodBody0//author: Brian, status:done
    {
        internal static rb_mod_modfunc singleton = new rb_mod_modfunc();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class module = (Class)recv;

            if (module._type != Class.Type.Module)
                throw new TypeError("module_function can only be called on modules").raise(caller);

            if (rest.Count == 0)
            {
                //module.scope_vmode = Access.ModuleFunction;
                caller.scope_vmode = Access.ModuleFunction;
                return module;
            }

            module.secure_visibility(caller);
            module.set_method_visibility(caller, Access.Private, rest.value.ToArray());

            foreach (object s in rest)
            {
                string name = Symbol.rb_to_id(caller, s);
                Class m = module;
                RubyMethod method = null;

                while (true)
                {
                    if (!m.get_method(name, out method))
                        if (!Ruby.Runtime.Init.rb_cObject.get_method(name, out method))
                            throw new System.Exception("BUG: undefined method '" + name + "', can't happen");

                    if (!(method.body is CallSuperMethodBody))
                        break;

                    m = m.super;
                    if (m == null) break;
                }

                Class.singleton_class(caller, module).add_method(name, method.body, method.arity, Access.Public, caller);
            }

            return module;
        }
        
    }

    
    internal class rb_mod_remove_method : VarArgMethodBody0//author: Brian, status: done
    {
        internal static rb_mod_remove_method singleton = new rb_mod_remove_method();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class klass0 = (Class)recv;

            foreach (object s in rest)
                klass0.remove_method(Symbol.rb_to_id(caller, s), caller);

            return klass0;
        }
    }


    
    internal class rb_mod_undef_method : VarArgMethodBody0//author: Brian, status: done
    {
        internal static rb_mod_undef_method singleton = new rb_mod_undef_method();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class klass0 = (Class)recv;

            foreach (object s in rest)
                klass0.undef(Symbol.rb_to_id(caller, s), caller);

            return klass0;
        }
    }

    
    internal class rb_mod_alias_method : MethodBody2//author: Brian, status: done
    {
        internal static rb_mod_alias_method singleton = new rb_mod_alias_method();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            ((Class)recv).alias(p1.ToString(), p2.ToString(), caller);
            return recv;
        }
    }

    
    internal class rb_mod_define_method : VarArgMethodBody1 //author: Brian, status: done
    {
        internal static rb_mod_define_method singleton = new rb_mod_define_method();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest)
        {
            string id = Symbol.rb_to_id(caller, p1);
            object body;
            Class klass0 = (Class)recv;

            if (rest.Count == 0)
            {
                if (block == null)
                    throw new ArgumentError("tried to create Proc object without a block").raise(caller);

                body = new Proc(recv, block, block.body, block._arity, ProcKind.Lambda);
            }
            else if (rest.Count == 1)
            {
                body = rest[0];

                if (!(body is Method || body is Proc))
                    throw new TypeError("wrong argument type " + Class.CLASS_OF(body).classname() + " (expected Proc/Method)").raise(caller);
            }
            else
            {
                throw new ArgumentError("wrong number of arguments (" + (rest.Count + 1) + " for 1)").raise(caller);
            }

            MethodBody mbody = null;
            int arity = 0;

            if (body is Method)
            {
                Method tmp = (Method)body;
                mbody = new DMethodBody(tmp);
                arity = tmp.body.arity;
            }
            else if (body is Proc)
            {
                Proc tmp = (Proc)body;
                body = proc_clone.singleton.Call0(last_class, tmp, caller, block);
                mbody = tmp.body;
                arity = tmp._arity;
                body = tmp;
            }

            klass0.add_method(id, mbody, arity, caller);

            return body;
        }
    }

    
    internal class rb_mod_attr : MethodBody//author: Brian, status: done
    {
        internal static rb_mod_attr singleton = new rb_mod_attr();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            Class.rb_scan_args(caller, args.ToRubyArray(), 1, 1, false);

            Class klass0 = (Class)recv;
            bool write = false;

            if (args.Length == 2)
                write = Eval.Test(args[1]);

            klass0.attr(Symbol.rb_to_id(caller, args[0]), true, write, caller);
            return null;
        } 
    }

    
    internal class rb_mod_attr_reader : VarArgMethodBody0//author: Brian, status: done
    {
        internal static rb_mod_attr_reader singleton = new rb_mod_attr_reader();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class klass0 = (Class)recv;

            foreach (object obj in rest)
                klass0.attr(Symbol.rb_to_id(caller, obj), true, false, caller);

            return null;
        }
    }


    
    internal class rb_mod_attr_writer : VarArgMethodBody0//author: Brian, status: done
    {
        internal static rb_mod_attr_writer singleton = new rb_mod_attr_writer();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class klass0 = (Class)recv;

            foreach (object obj in rest)
                klass0.attr(Symbol.rb_to_id(caller, obj), false, true, caller);

            return null;
        }
    }

    
    internal class rb_mod_attr_accessor : VarArgMethodBody0//author: Brian, status: done
    {
        internal static rb_mod_attr_accessor singleton = new rb_mod_attr_accessor();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class klass0 = (Class)recv;

            foreach (object obj in rest)
                klass0.attr(Symbol.rb_to_id(caller, obj), true, true, caller);

            return null;
        }
    }


    
    internal class rb_mod_method_defined : MethodBody1//author: Brian, status: done
    {
        internal static rb_mod_method_defined singleton = new rb_mod_method_defined();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object mid)
        {
            return Class.rb_method_boundp((Class)recv, Symbol.rb_to_id(caller, mid), true);
        }
    }

    
    internal class rb_mod_public_method_defined : MethodBody1//author: Brian, status: done
    {
        internal static rb_mod_public_method_defined singleton = new rb_mod_public_method_defined();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Class klass0 = (Class)recv;
            RubyMethod method = null;

            if (klass0.get_method(Symbol.rb_to_id(caller, p1), out method))
            {
                if (method.access == Access.Public)
                    return true;
            }

            return false;
        }
    }

    
    internal class rb_mod_private_method_defined : MethodBody1//author: Brian, status: done
    {
        internal static rb_mod_private_method_defined singleton = new rb_mod_private_method_defined();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Class klass0 = (Class)recv;
            RubyMethod method = null;

            if (klass0.get_method(Symbol.rb_to_id(caller, p1), out method))
            {
                if (method.access == Access.Private)
                    return true;
            }

            return false;
        }
    }


    
    internal class rb_mod_protected_method_defined : MethodBody1//author: Brian, status: done
    {
        internal static rb_mod_protected_method_defined singleton = new rb_mod_protected_method_defined();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Class klass0 = (Class)recv;
            RubyMethod method = null;

            if (klass0.get_method(Symbol.rb_to_id(caller, p1), out method))
            {
                if (method.access == Access.Protected)
                    return true;
            }

            return false;
        }
    }

    
    internal class rb_mod_public_method : VarArgMethodBody0//author: Brian, status: done
    {
        internal static rb_mod_public_method singleton = new rb_mod_public_method();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class klass0 = ((Object)recv).my_class;
            klass0.set_method_visibility(caller, Access.Public, rest.value.ToArray());
            return recv;
        }
    }

    
    internal class rb_mod_private_method : VarArgMethodBody0//author: Brian, status: done
    {
        internal static rb_mod_private_method singleton = new rb_mod_private_method();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class klass0 = ((Object)recv).my_class;
            klass0.set_method_visibility(caller, Access.Private, rest.value.ToArray());
            return recv;
        }
    }

    
    internal class rb_mod_module_eval : VarArgMethodBody0//author: Brian, status: done
    {
        internal static rb_mod_module_eval singleton = new rb_mod_module_eval();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return Eval.specific_eval(last_class, recv, (Class)recv, caller, block, rest);
        }
    }

    
    internal class rb_mod_autoload : MethodBody2//author: Brian, status: done
    {
        internal static rb_mod_autoload singleton = new rb_mod_autoload();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object file)
        {
            string id = Symbol.rb_to_id(caller, p1);

            Eval.rb_check_safe_str(caller, file);
            ((Class)recv).autoload(caller, id, ((String)file).value);
            return null;
        }
    }


    
    internal class rb_mod_autoload_p : MethodBody1//author: Brian, status: done
    {
        internal static rb_mod_autoload_p singleton = new rb_mod_autoload_p();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string id = Symbol.rb_to_id(caller, p1);
            return new String(((Class)recv).autoload_tbl[id]);
        }
    }

    
    internal class rb_mod_method : MethodBody1//author: Brian, status: done
    {
        internal static rb_mod_method singleton = new rb_mod_method();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return ((Class)recv).mnew(caller, null, Symbol.rb_to_id(caller, p1), true);
        }
    }


    
    internal class rb_mod_freeze : MethodBody0//status:done
    {
        internal static rb_mod_freeze singleton = new rb_mod_freeze();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            rb_mod_to_s.singleton.Call0(last_class, recv, caller, block);
            return rb_obj_freeze.singleton.Call0(last_class, recv, caller, block);
        }
    }


    
    internal class rb_mod_eqq : MethodBody1//status:done
    {
        internal static rb_mod_eqq singleton = new rb_mod_eqq();

        public override object Call1(Class last_class, object mod, Frame caller, Proc block, object obj)
        {
            return rb_obj_is_kind_of.singleton.Call1(last_class, obj, caller, block, mod);
        }
    }


    
    internal class rb_mod_cmp : MethodBody1//status:done
    {
        internal static rb_mod_cmp singleton = new rb_mod_cmp();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (recv == p1)
                return 0;
            if (!(p1 is Class))
                return null;

            object cmp = rb_class_inherited_p.singleton.Call1(last_class, recv, caller, null, p1);
            
            if (cmp == null)
                return null;
            if (Eval.Test(cmp))
                return -1;
            else
                return 1;
        }
    }


    
    internal class rb_mod_lt : MethodBody1//author:Brian, status:done
    {
        internal static rb_mod_lt singleton = new rb_mod_lt();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (recv == p1)
                return false;
            return rb_class_inherited_p.singleton.Call1(last_class, recv, caller, null, p1);
        }
    }


    
    internal class rb_class_inherited_p : MethodBody1//status:done
    {
        internal static rb_class_inherited_p singleton = new rb_class_inherited_p();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (recv == p1)
                return true;

            Class.is_class_or_module(caller, p1, "compared with non class/module");

            Class a = (Class)recv;
            Class b = (Class)p1;

            if (a._type == Class.Type.Singleton)
            {
                if (a.is_kind_of(b))
                    return true;

                a = a.my_class;
            }

            if (a.is_kind_of(b))
                return true;

            if (b.is_kind_of(a))
                return false;

            return null;
        }
    }

    
    internal class rb_mod_gt : MethodBody1//author:Brian, status:done
    {
        internal static rb_mod_gt singleton = new rb_mod_gt();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (recv == p1)
                return false;

            return rb_mod_ge.singleton.Call1(last_class, recv, caller, null, p1);
        }
    }

    
    internal class rb_mod_ge : MethodBody1//author:Brian, status:done
    {
        internal static rb_mod_ge singleton = new rb_mod_ge();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Class.is_class_or_module(caller, p1, "compared with non class/module");
            return rb_class_inherited_p.singleton.Call1(last_class, p1, caller, null, recv);
        }
    }

    
    internal class rb_mod_init_copy : MethodBody1//author: Brian, status: done
    {
        internal static rb_mod_init_copy singleton = new rb_mod_init_copy();

        public override object Call1(Class last_class, object clone, Frame caller, Proc block, object orig)
        {
            rb_obj_init_copy.singleton.Call1(last_class, clone, caller, block, orig);
            if (Class.CLASS_OF(clone)._type != Class.Type.Singleton)
                ((Object)clone).my_class = Class.rb_singleton_class_clone(orig);
            
            Class cloneCls = (Class)clone;
            Class origCls = (Class)orig;

            cloneCls.super = origCls.super;
            cloneCls.instance_vars = new Dictionary<string, object>(origCls.instance_vars);
            cloneCls.classpath = null;
            cloneCls._name = null;
            
            foreach (KeyValuePair<string,RubyMethod> pair in origCls._methods)
                cloneCls.add_method(pair.Key, pair.Value.body, pair.Value.arity, caller);

            return clone;
        }
    }

    
    internal class rb_mod_to_s : MethodBody0//author: kjg/brian, status: done
    {
        internal static rb_mod_to_s singleton = new rb_mod_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class cls = (Class)recv;
            if (cls._type == Class.Type.Singleton) // BBTAG: for anonymous classes
                return string.Format(CultureInfo.InvariantCulture, "#<Class:{0}>", (cls.attached is Class ?
                        Object.Inspect(cls.attached, caller) :
                        Object.rb_any_to_s(cls.attached)));
            else
                return new String(Class.rb_class_name((Class)recv));
        }
    }



    
    internal class rb_mod_included_modules : MethodBody0//author: Brian, status: done
    {
        internal static rb_mod_included_modules singleton = new rb_mod_included_modules();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class klass0 = (Class)recv;
            Array array = new Array();

            for (Class p = klass0.super; p != null; p = p.super)
            {
                if (p._type == Class.Type.IClass)
                    array.Add(p.my_class);
            }

            return array;
        }
    }

    
    internal class rb_mod_include_p : MethodBody1//author: Brian, status: done
    {
        internal static rb_mod_include_p singleton = new rb_mod_include_p();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Class mod = (Class)recv;

            if (!(p1 is Class) || !(((Class)p1)._type == Class.Type.Module))
                throw new TypeError("wrong argument type " + p1 + " (Module expected)").raise(caller);

            for (Class p = mod.super; p != null; p = p.super)
            {
                if (p._type == Class.Type.IClass)
                    if (p.my_class == p1)
                        return true;
            }

            return false;
        }
    }

    
    internal class rb_mod_name : MethodBody0//author: Brian, status: done
    {
        internal static rb_mod_name singleton = new rb_mod_name();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String(((Class)recv).classname());
        }
    }


    
    internal class rb_mod_ancestors : MethodBody0//author: Brian, status: done
    {
        internal static rb_mod_ancestors singleton = new rb_mod_ancestors();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class mod = (Class)recv;
            Array array = new Array();

            for (Class p = mod; p != null; p = p.super)
            {
                if (p._type == Class.Type.Singleton)
                    continue;

                if (p._type == Class.Type.IClass)
                    array.Add(p.my_class);
                else
                    array.Add(p);
            }

            return array;
        }
    }


    
    internal class rb_module_s_alloc : MethodBody0 //status:done
    {
        internal static rb_module_s_alloc singleton = new rb_module_s_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Class(null, null, Class.Type.Module, (Class)recv);
        }
    }

    
    internal class rb_class_s_alloc : MethodBody0 //status:done
    {
        internal static rb_class_s_alloc singleton = new rb_class_s_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Class(null, null, Class.Type.Class, (Class)recv);
        }
    }

     
    internal class rb_mod_initialize : MethodBody0 //author: Brian, status: done
    {
        internal static rb_mod_initialize singleton = new rb_mod_initialize();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class klass0 = (Class)recv;

            if (block != null)
                rb_mod_module_eval.singleton.Call(last_class, klass0, caller, block, new Array(klass0));

            return klass0;
        }
    }

    
    internal class rb_class_instance_methods : VarArgMethodBody0  //author: kjg, status: done
    {
        internal static rb_class_instance_methods singleton = new rb_class_instance_methods();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 0)
                rest.Add(true);    // Contrary to the comment in CRuby, the default is true!
            // Class.CLASS_OF(recv).get_all_specified_methods(list, Access.notPrivate, rest[0]);
            ((Class)recv).get_all_specified_methods(list, Access.notPrivate, rest[0]);
            return Array.CreateUsing(list);
        }
    }


    
    internal class rb_class_public_instance_methods : VarArgMethodBody0//author: kjg, status: done
    {
        internal static rb_class_public_instance_methods singleton = new rb_class_public_instance_methods();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 0)
                rest.Add(true);
            ((Class)recv).get_all_specified_methods(list, Access.Public, rest[0]);
            return Array.CreateUsing(list);
        }
    }


    
    internal class rb_class_protected_instance_methods : VarArgMethodBody0//author: kjg, status: done
    {
        internal static rb_class_protected_instance_methods singleton = new rb_class_protected_instance_methods();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 0)
                rest.Add(true);
            Class.CLASS_OF(recv).get_all_specified_methods(list, Access.Protected, rest[0]);
            return Array.CreateUsing(list);
        }
    }


    
    internal class rb_class_private_instance_methods : VarArgMethodBody0//author: kjg, status: done
    {
        internal static rb_class_private_instance_methods singleton = new rb_class_private_instance_methods();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 0)
                rest.Add(true);
            Class.CLASS_OF(recv).get_all_specified_methods(list, Access.Private, rest[0]);
            return Array.CreateUsing(list);
        }
    }


    
    internal class rb_mod_constants : MethodBody0//author:Brian, status:done
    {
        internal static rb_mod_constants singleton = new rb_mod_constants();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Class)recv).get_constants();
        }
    }


    
    internal class rb_mod_const_get : MethodBody1//author: Brian, status: done
    {
        internal static rb_mod_const_get singleton = new rb_mod_const_get();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string id = Symbol.rb_to_id(caller, p1);
            if (!Symbol.is_const_id(id))
                throw new NameError("wrong constant name " + id).raise(caller);

            return ((Class)recv).const_get(id, caller);
        }
    }


    
    internal class rb_mod_const_set : MethodBody2//author: Brian, status:done
    {
        internal static rb_mod_const_set singleton = new rb_mod_const_set();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            string id = Symbol.rb_to_id(caller, p1);
            if (!Symbol.is_const_id(id))
                throw new NameError("wrong constant name " + id).raise(caller);

            return ((Class)recv).const_set(id, p2);
        }
    }


    
    internal class rb_mod_const_defined : MethodBody1//author: Brian, status: done
    {
        internal static rb_mod_const_defined singleton = new rb_mod_const_defined();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string id = Symbol.rb_to_id(caller, p1);
            if (!Symbol.is_const_id(id))
                throw new NameError("wrong constant name " + id).raise(caller);

            return ((Class)recv).const_defined(id, false);
        }
    }


    
    internal class rb_mod_remove_const : MethodBody1//author:Brian, status:done
    {
        internal static rb_mod_remove_const singleton = new rb_mod_remove_const();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Class klass0 = (Class)recv;
            string id = Symbol.rb_to_id(caller, p1);
            if (!Symbol.is_const_id(id))
                throw new NameError("wrong constant name " + id).raise(caller);

            if (!klass0.Tainted && Eval.rb_safe_level() >= 4)
                throw new SecurityError("Insecure: can't remove constant").raise(caller);
            if (klass0.Frozen)
                throw TypeError.rb_error_frozen(caller, "class/module").raise(caller);

            if (!klass0.const_defined(id, false))
                throw new NameError(System.String.Format(CultureInfo.InvariantCulture, "constant {0}::{1} not defined", klass0.classname(), id)).raise(caller);

            return klass0.remove_const(id);
        }
    }


    
    internal class rb_mod_const_missing : MethodBody1//author:Brian, status:done
    {
        internal static rb_mod_const_missing singleton = new rb_mod_const_missing();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            // CRuby pops the frame for 'const_missing' to get the right stack trace - not applicable in .NET?
            // ruby_frame = ruby_frame->prev; 
            ((Class)recv).uninitialized_const(caller, Symbol.rb_to_id(caller, p1));
            return null; // unreachable
        }
    }



    
    internal class rb_mod_class_variables : MethodBody0//author: kjg/brian, status: done
    {
        internal static rb_mod_class_variables singleton = new rb_mod_class_variables();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            for (Class cls = (Class)recv; cls != null; cls = cls.super)
            {
                if (cls.instance_vars != null)
                {
                    foreach (System.Collections.Generic.KeyValuePair<string, object> var in cls.instance_vars)
                        if (Symbol.is_class_id(var.Key) && var.Value != null && !String.ListContains(list, var.Key))
                            list.Add(new String(var.Key));
                }
            }
            return Array.CreateUsing(list);
        }

    }

    
    internal class rb_mod_remove_cvar : MethodBody1 //author: Brian, status:done
    {
        internal static rb_mod_remove_cvar singleton = new rb_mod_remove_cvar();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string id = Symbol.rb_to_id(caller, p1);

            if (!Symbol.is_class_id(id))
                throw new NameError("wrong class variable name " + id).raise(caller);

            if (!((Basic)recv).Tainted && (Eval.rb_safe_level() >= 4))
                throw new SecurityError("Insecure: can't remove class variable").raise(caller);

            if (((Basic)recv).Frozen)
                throw TypeError.rb_error_frozen(caller, "class/module").raise(caller);

            Class klass0 = (Class)recv;

            if (klass0.instance_variable_get(id) != null)
            {
                object val = klass0.remove_cvar(id);
                return val;
            }

            throw new NameError("class variable " + id + " not defined for " + klass0.classname()).raise(caller);
        }
    }

    
    internal class rb_obj_alloc : MethodBody0 // author: Brian, status: done
    {
        internal static rb_obj_alloc singleton = new rb_obj_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class klass0 = (Class)recv;
            object obj = null;

            if (klass0.super == null)
                throw new TypeError("Can't instantiate uninitialized class").raise(caller);

            if (klass0._type == Class.Type.Singleton)
                throw new TypeError("Can't create instance of virtual class").raise(caller);

            obj = Eval.CallPrivate(klass0, caller, "allocator", block, new object[] { });

            if (Class.CLASS_OF(obj) != klass0.class_real())
                throw new TypeError("Wrong instance allocation").raise(caller);

            return obj;
        }
    }

    
    internal class rb_class_initialize : MethodBody // author: Brian, status: done
    {
        internal static rb_class_initialize singleton = new rb_class_initialize();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Class klass0 = (Class)recv;
            Class super = (Class)p1;

            if (klass0.super != null)
                throw new TypeError("Already initialized class").raise(caller);

            if (super == null)
                super = Ruby.Runtime.Init.rb_cObject;
            else
                super.check_inheritable(caller);

            klass0.super = super;
            Class.rb_make_metaclass(klass0, super.my_class);
            rb_mod_initialize.singleton.Call0(last_class, klass0, caller, block);
            klass0.class_inherited((Class)super, caller);

            return klass0;
        }

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Call1(last_class, recv, caller, block, null);
        }

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 0)
                return Call0(last_class, recv, caller, args.block);
            else if (args.Length == 1)
                return Call1(last_class, recv, caller, args.block, args[0]);
            else
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 1)").raise(caller);
        }
    }


    
    internal class rb_class_init_copy : MethodBody1 //author: Brian, status: done
    {
        internal static rb_class_init_copy singleton = new rb_class_init_copy();

        public override object Call1(Class last_class, object clone, Frame caller, Proc block, object orig)
        {
            if (((Class)clone).super != null)
                throw new TypeError("already initialized class").raise(caller);

            return rb_mod_init_copy.singleton.Call1(last_class, clone, caller, block, orig);
        }
    }
}
