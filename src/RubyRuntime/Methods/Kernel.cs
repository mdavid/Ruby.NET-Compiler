/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;

using System.Collections.Generic;

namespace Ruby.Methods
{
    
    internal class rb_obj_equal : MethodBody1 //author: cjs, status: done
    {
        internal static rb_obj_equal singleton = new rb_obj_equal();

        public override object Call1(Class last_class, object obj, Frame caller, Proc block, object other)
        {
            // Special case for bool (ReferenceEquals always returns false equating boxed bools)
            if (obj is bool)
                return other is bool && (bool)obj == (bool)other;

            // Special case for symbol as there is no "==" defined
            if (obj is Symbol)
                return other is Symbol && ((Symbol)obj).id_new == ((Symbol)other).id_new;

            return object.ReferenceEquals(obj, other);
        }
    }

    
    internal class puts : MethodBody //status: done
    {
        internal static puts singleton = new puts();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            return rb_f_puts.singleton.Calln(last_class, IO.rb_stdout.value, caller, args);
        }
    }


    
    internal class rb_obj_class : MethodBody0 //status: done
    {
        internal static rb_obj_class singleton = new rb_obj_class();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Class klass0 = Class.CLASS_OF(recv);

            return klass0.class_real();

        }
    }


    
    internal class rb_obj_public_methods : VarArgMethodBody0 //status: done
    {
        internal static rb_obj_public_methods singleton = new rb_obj_public_methods();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            System.Collections.ArrayList list;
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 0)
                rest.Add(true);

            if (Marshal.RTEST(rest[0]))
            {
                list = new System.Collections.ArrayList();
                Class.CLASS_OF(recv).get_all_specified_methods(list, Access.Public, Eval.Test(rest[0]));
                return Array.CreateUsing(list);
            }
            else
            {
                list = new System.Collections.ArrayList();
                Class.CLASS_OF(recv).public_methods(list);
                return new Array(list);
            }
        }
    }


    
    internal class rb_obj_id : MethodBody0 // author: cjs, status: done
    {
        internal static rb_obj_id singleton = new rb_obj_id();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (recv is bool && (bool)recv == false)
                return 0;
            if (recv is bool && (bool)recv == true)
                return 2;
            if (recv == null)
                return 4;
            if (recv is Ruby.Symbol)
                return ObjectSpace.obj2id(((Symbol)recv).id_s);
            else
                return ObjectSpace.obj2id(recv);
        }
    }

    
    internal class rb_obj_ivar_get : MethodBody1 //status: done
    {
        internal static rb_obj_ivar_get singleton = new rb_obj_ivar_get();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object symbol)
        {
            string id = Symbol.rb_to_string(caller, symbol);
            if (!Symbol.is_instance_id(id))
            {
                throw new NameError("`" + id + "' is not allowed as an instance variable name").raise(caller);
            }
            return Eval.ivar_get(recv, id);
        }
    }

    
    internal class rb_obj_method : MethodBody1 //author: Brian, status: done
    {
        internal static rb_obj_method singleton = new rb_obj_method();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object symbol)
        {
            return Class.CLASS_OF(recv).mnew(caller, recv, Symbol.rb_to_id(caller, symbol), false);
            //            return new Method(recv, ((Symbol)symbol).id);
        }
    }

    
    internal class rb_obj_respond_to : VarArgMethodBody0 //Author: kjg, status: done
    {
        internal static rb_obj_respond_to singleton = new rb_obj_respond_to();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            bool acceptPriv = false;
            string name;
            Class.rb_scan_args(caller, rest, 1, 1, false);
            if (rest.Count == 2)
                acceptPriv = Eval.Test(rest[1]);
            name = Symbol.rb_to_id(caller, rest[0]);
            return Class.rb_method_boundp(Class.CLASS_OF(recv), name, acceptPriv);
        }
    }

    
    internal class rb_f_send : VarArgMethodBody1 //status: done
    {
        internal static rb_f_send singleton = new rb_f_send();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object vid, Array args)
        {
            return Eval.CallPrivate(recv, caller, Symbol.rb_to_id(caller, vid), block, args.value.ToArray());
        }
    }

    
    internal class rb_obj_instance_eval : VarArgMethodBody0 //author: Brian, status: done
    {
        internal static rb_obj_instance_eval singleton = new rb_obj_instance_eval();

        public override object Call(Class last_class, object self, Frame caller, Proc block, Array rest)
        {
            Class klass;

            if ((self is int) || (self is string) || (self is Symbol))
                klass = null;
            else
                klass = Class.singleton_class(caller, self);

            return Eval.specific_eval(last_class, self, klass, caller, block, rest);
        }
    }

    
    internal class rb_obj_extend : VarArgMethodBody0 //author: kjg, status: done
    {
        internal static rb_obj_extend singleton = new rb_obj_extend();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array args)
        {
            int argc = args.Count;
            Class.rb_scan_args(caller, args, 1, 0, true);
            for (int i = 0; i < argc; i++)
                Class.check_type(caller, args[i], Class.Type.Module);
            while (argc-- > 0)
            {
                Eval.CallPrivate(args[argc], caller, "extend_object", null, recv);
                Eval.CallPrivate(args[argc], caller, "extended", null, recv);
            }
            return recv;
        }
    }

    
    internal class rb_obj_display : VarArgMethodBody0 //author: Brian, status: done
    {
        internal static rb_obj_display singleton = new rb_obj_display();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            object output;
            if (rest.Count == 0)
                output = IO.rb_stdout.value;
            else
                output = rest[0];

            IO.rb_io_write(output, recv, caller);

            return null;
        }
    }

    
    internal class rb_false : MethodBody0 //status: done
    {
        internal static rb_false singleton = new rb_false();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }


    
    internal class rb_obj_pattern_match : MethodBody1 //status: done
    {
        internal static rb_obj_pattern_match singleton = new rb_obj_pattern_match();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return false;
        }
    }

    
    internal class rb_obj_id_obsolete : MethodBody0 //status: done
    {
        internal static rb_obj_id_obsolete singleton = new rb_obj_id_obsolete();

        public override object Call0(Class last_class, object obj, Frame caller, Proc block)
        {
            Errors.rb_warn("Object#id will be deprecated; use Object#object_id");
            return rb_obj_id.singleton.Call0(last_class, obj, caller, block);
        }
    }

    
    internal class rb_obj_type : MethodBody0 //status: done
    {
        internal static rb_obj_type singleton = new rb_obj_type();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Errors.rb_warn("Object#type is deprecated; use Object#class");
            return Class.CLASS_OF(recv).class_real();
        }
    }

    
    internal class rb_obj_clone : MethodBody0 //status: done
    {
        internal static rb_obj_clone singleton = new rb_obj_clone();

        public override object Call0(Class last_class, object obj, Frame caller, Proc block)
        {
            string classname;
            if (Object.rb_special_const_p(obj, out classname))
                throw new TypeError("can't clone " + classname).raise(caller);

            object clone = rb_obj_alloc.singleton.Call0(last_class, rb_obj_class.singleton.Call0(last_class, obj, caller, null), caller, null);
            ((Basic)clone).my_class = Class.rb_singleton_class_clone(obj);

            if (obj is Object)
                ((Object)clone).instance_vars = new Dictionary<string, object>(((Object)obj).instance_vars);

            ((Basic)clone).Tainted = ((Basic)obj).Tainted;

            clone = Eval.CallPrivate(clone, caller, "initialize_copy", null, obj);

            ((Basic)clone).Frozen = ((Basic)obj).Frozen;

            return clone;
        }
    }


    
    internal class rb_obj_dup : MethodBody0 //status: done
    {
        internal static rb_obj_dup singleton = new rb_obj_dup();

        public override object Call0(Class last_class, object obj, Frame caller, Proc block)
        {
            string classname;
            if (Object.rb_special_const_p(obj, out classname))
                throw new TypeError("can't dup " + classname).raise(caller);

            object clone = rb_obj_alloc.singleton.Call0(last_class, rb_obj_class.singleton.Call0(last_class, obj, caller, null), caller, null);

            ((Basic)clone).Tainted = ((Basic)obj).Tainted;

            if (obj is Object)
                ((Object)clone).instance_vars = new Dictionary<string, object>(((Object)obj).instance_vars);

            return Eval.CallPrivate(clone, caller, "initialize_copy", null, obj);
        }
    }

    
    internal class rb_obj_init_copy : MethodBody1 //status: done
    {
        internal static rb_obj_init_copy singleton = new rb_obj_init_copy();

        public override object Call1(Class last_class, object obj, Frame caller, Proc block, object orig)
        {
            if (obj == orig)
                return obj;
            TypeError.rb_check_frozen(caller, obj);
            if (obj.GetType() != orig.GetType() || rb_obj_class.singleton.Call0(last_class, obj, caller, null) != rb_obj_class.singleton.Call0(last_class, orig, caller, null))
                throw new TypeError("initialize_copy should take same class object").raise(caller);
            return obj;
        }
    }


    
    internal class rb_obj_taint : MethodBody0 //author: kjg/brian, status: done
    {
        internal static rb_obj_taint singleton = new rb_obj_taint();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Eval.rb_secure(4, caller);
            if (recv is Basic)
            {
                Basic bas = (Basic)recv;
                if (!bas.Tainted)
                {
                    if (bas.Frozen)
                        throw TypeError.rb_error_frozen(caller, "object").raise(caller);

                    bas.Tainted = true;
                }
            }
            return recv;
        }
    }

    
    internal class rb_obj_tainted : MethodBody0 //author: kjg, status: done
    {
        internal static rb_obj_tainted singleton = new rb_obj_tainted();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (recv is Basic)
                return ((Basic)recv).Tainted;
            else
                return false;
        }
    }

    
    internal class rb_obj_untaint : MethodBody0 //author: Brian, status: done
    {
        internal static rb_obj_untaint singleton = new rb_obj_untaint();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Eval.rb_secure(3, caller);
            Basic bas = (Basic)recv;
            if (bas.Tainted)
            {
                if (bas.Frozen)
                {
                    throw TypeError.rb_error_frozen(caller, "object").raise(caller);
                }
                bas.Tainted = false;
            }

            return bas;
        }
    }

    
    internal class rb_obj_freeze : MethodBody0 //author: cjs, status: done
    {
        internal static rb_obj_freeze singleton = new rb_obj_freeze();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Object.rb_obj_freeze(caller, recv);
        }
    }

    
    internal class rb_obj_frozen_p : MethodBody0 //author: kjg, status: done
    {
        internal static rb_obj_frozen_p singleton = new rb_obj_frozen_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (recv is Basic)
                return ((Basic)recv).Frozen;
            else
                return false;
        }
    }

    
    internal class rb_any_to_a : MethodBody0 //author: Brian, status: done
    {
        internal static rb_any_to_a singleton = new rb_any_to_a();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Errors.rb_warn("default `to_a' will be obsolete");
            return new Array(recv);
        }
    }

    
    internal class rb_any_to_s : MethodBody0 //author: kjg, status: done
    {
        internal static rb_any_to_s singleton = new rb_any_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Object.rb_any_to_s(recv); // delegates to Object routine
        }
    }

    //
    // Returns an array of method names.
    // Undocumented behaviour: if the optional argument evaluates to false
    // only the singleton methods are returned. True ==> default behaviour.
    //
    
    internal class rb_obj_methods : VarArgMethodBody0 //suthor: kjg, status: done
    {
        internal static rb_obj_methods singleton = new rb_obj_methods();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            System.Collections.ArrayList list;
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 0)
                rest.Add(true);
            if (!Eval.Test(rest[0]))
                return rb_obj_singleton_methods.singleton.Call(last_class, recv, caller, block, rest);
            else    // arg-length check already done => inline remaining body of rb_class_instance_methods
            {
                list = new System.Collections.ArrayList();
                Class.CLASS_OF(recv).get_all_specified_methods(list, Access.notPrivate, true);
                return Array.CreateUsing(list);
            }
        }
    }

    //
    // Returns an array of singleton method names for the receiver, including 
    // mixin methods if and only if the optional argument evaluates true.
    //
    // This method can have zero args, or one Boolean arg.  
    // If zero the effect is as if arg0 == true.
    // Emprically, args other than Ruby.False or nil means true.
    //
    
    internal class rb_obj_singleton_methods : VarArgMethodBody0 //author: kjg, status: done
    {
        internal static rb_obj_singleton_methods singleton = new rb_obj_singleton_methods();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class klass0 = Class.CLASS_OF(recv);
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            bool recur = true;
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 0)
                rest.Add(true);
            recur = Eval.Test(rest[0]);

            if (klass0 != null && klass0._type == Class.Type.Singleton)
                klass0.get_specified_methods(list, Access.notPrivate);  // force "all but private" case
            if (recur)
                for (Class cls = klass0.super;
                    cls != null && (cls._type == Class.Type.Singleton || cls._type == Class.Type.IClass);
                    cls = cls.super)
                    cls.get_specified_methods(list, Access.notPrivate);
            return Array.CreateUsing(list);
        }
    }

    
    internal class rb_obj_protected_methods : VarArgMethodBody0 //Author: kjg, status: done
    {
        internal static rb_obj_protected_methods singleton = new rb_obj_protected_methods();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            bool recur = true;
            System.Collections.ArrayList list;
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 1)
                recur = Eval.Test(rest[0]);
            list = new System.Collections.ArrayList();
            Class.CLASS_OF(recv).get_all_specified_methods(list, Access.Protected, recur);
            return Array.CreateUsing(list);
        }
    }

    
    internal class rb_obj_private_methods : VarArgMethodBody0 //Author: kjg, status: done
    {
        internal static rb_obj_private_methods singleton = new rb_obj_private_methods();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            bool recur = true;
            System.Collections.ArrayList list;
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 1)
                recur = Eval.Test(rest[0]);
            list = new System.Collections.ArrayList();
            Class.CLASS_OF(recv).get_all_specified_methods(list, Access.Private, recur);
            return Array.CreateUsing(list);
        }
    }

    
    internal class rb_obj_instance_variables : MethodBody0 // Author: kjg/brian, status: done
    {
        internal static rb_obj_instance_variables singleton = new rb_obj_instance_variables();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)

            //VALUE ary;
        //ary = rb_ary_new();
        //switch (TYPE(obj))
        //{
        //    case T_OBJECT:
        //    case T_CLASS:
        //    case T_MODULE:
        //        if (ROBJECT(obj)->iv_tbl)
        //        {
        //            st_foreach_safe(ROBJECT(obj)->iv_tbl, ivar_i, ary);
        //        }
        //        break;
        //    default:
        //        if (!generic_iv_tbl) break;
        //        if (FL_TEST(obj, FL_EXIVAR) || rb_special_const_p(obj))
        //        {
        //            st_table* tbl;
        //            if (st_lookup(generic_iv_tbl, obj, (st_data_t*)&tbl))
        //            {
        //                st_foreach_safe(tbl, ivar_i, ary);
        //            }
        //        }
        //        break;
        //}
        //return ary;
        {
            if (recv is Object)
            {
                System.Collections.ArrayList list = new System.Collections.ArrayList();
                Object obj = (Object)recv;
                if (obj != null && obj.instance_vars != null)
                {
                    foreach (System.Collections.Generic.KeyValuePair<string, object> var in obj.instance_vars)
                        if (Symbol.is_instance_id(var.Key) && var.Value != null && !String.ListContains(list, var.Key))
                            list.Add(new String(var.Key));
                }
                return Array.CreateUsing(list);
            }
            else
            {
                // Lookaside
                string classname;
                if (Object.exivar_p(recv) || Object.rb_special_const_p(recv, out classname))
                {
                    System.Collections.ArrayList list = new System.Collections.ArrayList();
                    Dictionary<string, object> instance_vars = Object.get_generic_ivars(recv);
                    if (instance_vars != null)
                    {
                        foreach (System.Collections.Generic.KeyValuePair<string, object> var in instance_vars)
                            if (Symbol.is_instance_id(var.Key) && var.Value != null && !String.ListContains(list, var.Key))
                                list.Add(new String(var.Key));
                        return Array.CreateUsing(list);
                    }
                }
            }
            return new Array();
        }
    }

    
    internal class rb_obj_ivar_set : MethodBody2 //author: Brian, status: done
    {
        internal static rb_obj_ivar_set singleton = new rb_obj_ivar_set();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            string id = Symbol.rb_to_id(caller, p1);

            if (!Symbol.is_instance_id(id))
                throw new NameError("`" + id + "' is not allowed as an instance variable name").raise(caller);

            return Eval.ivar_set(caller, recv, id, p2);
        }
    }

    
    internal class rb_obj_is_instance_of : MethodBody1 //status: done
    {
        internal static rb_obj_is_instance_of singleton = new rb_obj_is_instance_of();

        public override object Call1(Class last_class, object obj, Frame caller, Proc block, object c)
        {
            if (!(c is Class))
                throw new TypeError("class or module required").raise(caller);

            return (rb_obj_class.singleton.Call0(last_class, obj, caller, null) == c);
        }
    }

    
    internal class rb_obj_is_kind_of : MethodBody1 //author: Brian, status: done
    {
        internal static rb_obj_is_kind_of singleton = new rb_obj_is_kind_of();

        public override object Call1(Class last_class, object obj, Frame caller, Proc block, object p1)
        {
            if (!(p1 is Class))
                throw new TypeError("class or module required").raise(caller);

            return Class.CLASS_OF(obj).is_kind_of((Class)p1);
        }
    }


    
    internal class rb_obj_remove_instance_variable : MethodBody1 //author: Brian, status: done
    {
        internal static rb_obj_remove_instance_variable singleton = new rb_obj_remove_instance_variable();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string id = Symbol.rb_to_id(caller, p1);

            if (recv is Basic)
            {
                if (!((Basic)recv).Tainted && Eval.rb_safe_level() >= 4)
                    throw new SecurityError("Insecure: can't modify instance variable").raise(caller);
                if (((Basic)recv).Frozen)
                    throw TypeError.rb_error_frozen(caller, "object").raise(caller);
            }

            if (!Symbol.is_instance_id(id))
                throw new NameError("`" + id + "' is not allowed as an instance variable name").raise(caller);

            if (recv is Class || recv is Object)
            {
                object val = ((Object)recv).instance_variable_remove(id);
                if (val != null)
                    return val;
            }

            string classname;
            if (Object.exivar_p(recv) || Object.rb_special_const_p(recv, out classname))
            {
                object val = null;
                if (Object.generic_ivar_remove(recv, id, out val))
                    return val;
            }

            throw new NameError("instance variable " + id + " not defined").raise(caller);
        }
    }


    
    internal class rb_f_integer : MethodBody1 // author: cjs, status: done
    {
        internal static rb_f_integer singleton = new rb_f_integer();

        public override object Call1(Class last_class, object obj, Frame caller, Proc block, object arg)
        {
            return Integer.rb_Integer(arg, caller);
        }
    }


    
    internal class rb_f_float : MethodBody1 // author: cjs, status: done
    {
        internal static rb_f_float singleton = new rb_f_float();

        public override object Call1(Class last_class, object obj, Frame caller, Proc block, object arg)
        {
            return Float.rb_Float(arg, caller);
        }
    }


    
    internal class rb_f_string : MethodBody1 // author: cjs, status: done
    {
        internal static rb_f_string singleton = new rb_f_string();

        public override object Call1(Class last_class, object obj, Frame caller, Proc block, object arg)
        {
            return Object.Convert<String>(arg, "to_s", caller);
        }
    }


    
    internal class rb_f_array : MethodBody1 // author: cjs, status: done
    {
        internal static rb_f_array singleton = new rb_f_array();

        public override object Call1(Class last_class, object obj, Frame caller, Proc block, object arg)
        {
            return Array.rb_Array(arg, caller);
        }
    }
}
