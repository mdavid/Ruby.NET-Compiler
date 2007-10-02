/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


using System.Collections.Generic;
using System.Collections;
using System.Text;
using Ruby;

namespace Ruby.Runtime
{
    // Collection of static methods used by compiler generated code to manipulate variables.

    [UsedByRubyCompiler]
    public class Variables
    {
        internal static Dictionary<string, global_variable> global_vars = new Dictionary<string, global_variable>();

        internal static bool is_ivar_name(string key)
        {
            return (key.Length > 1 && key[0] == '@' && key[1] != '@');
        }

        internal static bool is_cvar_name(string key)
        {
            return key.StartsWith("@@");
        }

        [UsedByRubyCompiler]
        public static object gvar_set(string gvar_name, object value, Frame caller)
        {
            if (!global_vars.ContainsKey(gvar_name))
                global_vars[gvar_name] = new global_variable();

            global_vars[gvar_name].setter(gvar_name, value, caller);
            
            return value;
        }

        [UsedByRubyCompiler]
        public static object gvar_get(string gvar_name, Frame caller)
        {
            if (global_vars.ContainsKey(gvar_name))
                return global_vars[gvar_name].getter(gvar_name, caller);
            else
                return null;
        }

        [UsedByRubyCompiler]
        public static object gvar_defined(string gvar_name)
        {
            return global_vars.ContainsKey(gvar_name);
        }


        internal static void gvar_trace(string gvar_name, Proc cmd)
        {
            if (global_vars[gvar_name].trace == null)
                global_vars[gvar_name].trace = new List<Proc>();

            global_vars[gvar_name].trace.Add(cmd);
        }

        internal static object gvar_untrace(string gvar_name, Proc cmd)
        {
            global_vars[gvar_name].trace.Remove(cmd);
            return null;
        }

        internal static object gvar_untrace(string gvar_name)
        {
            Array removed = new Array(global_vars[gvar_name].trace.ToArray());
            global_vars[gvar_name].trace.Clear();
            return removed;
        }


        [UsedByRubyCompiler]
        public static object alias_variable(string name1, string name2)
        {
            throw new System.NotImplementedException("alias_variable");
        }

        internal static void cvar_override_check(string id, Class klass) // author: Brian, status: done
        {
            Class orig = klass;

            if (klass._type == Class.Type.IClass)
                klass = klass.my_class;

            klass = klass.super;
            while (klass != null)
            {
                if (klass.instance_variable_get(id) != null)
                    Methods.rb_warn_m.singleton.Call1(klass, klass, null, null, new String("class variable " + id + " of " + klass + " is overridden by " + orig));

                klass = klass.super;
            }
        }

        [UsedByRubyCompiler]
        public static object cvar_set(Frame caller, Class klass, string cvar_name, object value) //author: Brian, status: done
        {
            return cvar_set(caller, klass, cvar_name, value, true);
        }

        internal static object cvar_set(Frame caller, Class klass, string cvar_name, object value, bool warn) //author: Brian, status: done
        {
            Class tmp = klass;
            while (tmp != null)
            {
                if (tmp.attached != null)
                    tmp = (Class)(tmp.attached);

                if (tmp.instance_variable_defined(cvar_name))
                {
                    if (tmp.Frozen)
                        throw TypeError.rb_error_frozen(caller, "class/module").raise(caller);
                    if (!tmp.Tainted && Eval.rb_safe_level() >= 4)
                        throw new SecurityError("Insecure: can't modify class variable").raise(caller);
                    if (warn && Eval.Test(Options.ruby_verbose.value) && klass != tmp)
                        Methods.rb_warn_m.singleton.Call1(klass, klass, null, null, new String("already initialized class variable " + cvar_name));

                    tmp.instance_variable_set(cvar_name, value);

                    if (Eval.Test(Options.ruby_verbose.value))
                        cvar_override_check(cvar_name, tmp);

                    return value;
                }
                tmp = tmp.super;
            }

            return klass.instance_variable_set(cvar_name, value);
        }

        [UsedByRubyCompiler]
        public static object cvar_get(Frame caller, Class klass, string cvar_name) //author: Brian, status: done
        {
            Class tmp = klass;

            while (tmp != null)
            {
                if (tmp.attached != null)
                    tmp = (Class)(tmp.attached);

                object value = tmp.instance_variable_get(cvar_name);

                if ((value == null) && (tmp.instance_variable_defined(cvar_name)))
                    return value;

                if (value != null)
                {
                    if (Eval.Test(Options.ruby_verbose.value))
                        cvar_override_check(cvar_name, tmp);
                    return value;
                }
                tmp = tmp.super;
            }

            throw new NameError("uninitialized class variable " + cvar_name + " in " + klass.classname()).raise(caller);
        }

        [UsedByRubyCompiler]
        public static object cvar_defined(Class klass, string cvar_name)//author: Brian, status: done
        {
            Class tmp = klass;

            while (tmp != null)
            {
                if (tmp.attached != null)
                    tmp = (Class)(tmp.attached);

                if (tmp.instance_vars.ContainsKey(cvar_name))
                    return true;

                tmp = tmp.super;
            }

            return false;
        }

        internal static void define_global_const(string name, object value) // author: kjg
        {
            Ruby.Runtime.Init.rb_cObject.define_const(name, value);
        }


        // ------------------------------------------------------------------------------

        internal static void rb_define_global_const(string name, object val)
        {
            rb_define_const(Ruby.Runtime.Init.rb_cObject, name, val);
        }

        internal static void rb_define_const(Class klass, string name, object val)
        {
            klass.define_const(name, val);
        }

        internal static void rb_define_variable(string name, global_variable var)
        {
            global_vars[name] = var;

        }
    }
}
