/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using System;
using System.Collections.Generic;

namespace Ruby
{
    [UsedByRubyCompiler]
    public partial class Object : Basic
    {
        internal Dictionary<string, object> instance_vars = new Dictionary<string, object>();

        // ---------------------------------------------------------------------------

        [UsedByRubyCompiler]
        public static object ruby_top_self; // main object

        internal static Dictionary<object, Dictionary<string, object>> generic_iv_tbl = new Dictionary<object, Dictionary<string, object>>();

        private static object nil_ivar_singleton = new object();        // singleton marker to allow adding ivars to nil

        // ---------------------------------------------------------------------------

        internal Object(): base(Ruby.Runtime.Init.rb_cObject) { }

        [UsedByRubyCompiler]
        public Object(Class klass) : base(klass) { }

        // ---------------------------------------------------------------------------

        internal bool instance_variable_defined(string symbol)
        {
            return instance_vars.ContainsKey(symbol);
        }

        internal object instance_variable_get(string symbol)
        {
            if (instance_vars.ContainsKey(symbol))
                return instance_vars[symbol];
            else
                return null;
        }

        internal object instance_variable_set(string symbol, object value)
        {
            instance_vars[symbol] = value;
            return value;
        }

        internal object instance_variable_remove(string symbol)
        {
            if (instance_vars.ContainsKey(symbol))
            {
                object val = instance_vars[symbol];
                instance_vars.Remove(symbol);
                return val;
            }

            return null;
        }


        //-----------------------------------------------------------------


        internal static object rb_obj_freeze(Frame caller, object obj)
        {
            if (obj is Basic)
            {
                Basic o = (Basic)obj;
                if (!o.Frozen)
                {
                    if (Eval.rb_safe_level() >= 4 && !o.Tainted)
                        throw new SecurityError("Insecure: can't freeze object").raise(caller);
                    o.Frozen = true;
                }
            }

            return obj;
        }

        internal static String rb_any_to_s(object recv) //author: kjg, status: done
        {
            // CRuby uses the address of the object as a unique identifier.
            // On the CLR, we cannot do this in verifiable code. However,
            // obj.GetHashCode() returns a unique int32 for every object.
            //
            string syStr = string.Format("#<{0}:0x{1}>",
                Class.CLASS_OF(recv).ToString(),
                System.Convert.ToString(recv.GetHashCode(), 16)); // convert to HEX
            String rbStr = new String(syStr);
            // This code anticipates the full implementation of tainting
            if (recv is Basic && ((Basic)recv).Tainted)
                rbStr.Tainted = true;
            return rbStr;
        }

        internal static bool rb_obj_is_kind_of(object obj, Class klass)//author: Brian
        {
            return Class.CLASS_OF(obj).is_kind_of(klass);
        }

        internal static bool Equal(object o1, object o2, Frame caller) //status: done
        {
            if (Object.ReferenceEquals(o1, o2))
                return true;

            return Eval.Test(Eval.CallPrivate(o1, caller, "==", null, o2));
        }

        // rb_inspect
        internal static String Inspect(object obj, Frame caller) //status: done
        {
            return String.ObjectAsString(Eval.CallPrivate(obj, caller, "inspect", null), caller);
        }

        // rb_check_type
        // BBTAG: john - this does the same thing as Class.check_type but also works on types other
        // than classes: perhaps we might merge these?
        internal static void CheckType<T>(Frame caller, object value, Class.Type classType)
        {
            if (value is T)
            {
                if (value is Class)
                {
                    Class.Type type = ((Class)value)._type;
                    if (type != classType)
                        throw new TypeError("wrong argument type " + type + " (" + classType + " expected)").raise(caller);
                }
            }
            else
            {
                throw new TypeError(string.Format("wrong argument type {0} ({1} expected)", Class.rb_obj_classname(value), typeof(T).Name)).raise(caller);  
            }
        }

        internal static void CheckType<T>(Frame caller, object value)
        {
            if (!(value is T))
                throw new TypeError(string.Format("wrong argument type {0} ({1} expected)", Class.rb_obj_classname(value), typeof(T).Name)).raise(caller);
        }

        // convert_type
        internal static object Convert<T>(object value, string method, bool raise, Frame caller) //status: done
        {
            if (!Eval.RespondTo(value, method))
            {
                if (raise)
                {
                    throw new TypeError(string.Format("cannot convert {0} into {1}",
                         value == null ? "nil" :
                         (value is bool && (bool)value == true) ? "true" :
                         (value is bool && (bool)value == false) ? "false" :
                         Class.CLASS_OF(value).ToString(), typeof(T).Name)).raise(caller);
                }
                else
                {
                    return null;
                }
            }

            return Eval.CallPrivate(value, caller, method, null);
        }

        // rb_convert_type
        internal static T Convert<T>(object value, string method, Frame caller) //status: done
        {
            if (value is T)
                return (T)value;
            else
            {
                object result = Convert<T>(value, method, true, caller);
                if (!(result is T))
                {
                    throw new TypeError(string.Format("{0}#{1} should return {2}",
                        Class.CLASS_OF(value), method, typeof(T).Name)).raise(caller);
                }
                else
                {
                    return (T)result;
                }
            }
        }

        // rb_check_convert_type
        internal static T CheckConvert<T>(object value, string method, Frame caller) //status: done
        {
            if (value is T && !(value is Data))
                return (T)value;
            else
            {
                object result = Convert<T>(value, method, false, caller);
                if (result == null)
                {
                    return default(T);
                }
                else if (!(result is T))
                {
                    throw new TypeError(string.Format("{0}#{1} should return {2}",
                        Class.CLASS_OF(value), method, typeof(T).Name)).raise(caller);
                }
                else
                {
                    return (T)result;
                }
            }
        }

        internal static bool rb_special_const_p(object obj)
        {
            string classname;
            return rb_special_const_p(obj, out classname);
        }

        internal static bool rb_special_const_p(object obj, out string classname) // author: Brian, status: done
        {
            if (obj is bool)
            {
                if ((bool)obj)
                    classname = "TrueClass";
                else
                    classname = "FalseClass";

                return true;
            }

            if (obj is int)
            {
                classname = "Fixnum";
                return true;
            }

            if (obj is string || obj is Symbol)
            {
                classname = "Symbol";
                return true;
            }

            if (obj == null)
            {
                classname = "NilClass";
                return true;
            }

            classname = "";
            return false;
        }

        private static bool Immediate(object obj)
        {
            return (obj == null || obj is int || obj is bool);
        }

        // BBTAG: corresponds to FL_TEST(obj, FL_EXIVAR) macro
        internal static bool exivar_p (object obj)
        {
            if (Immediate(obj) || obj is Object)
                return false;

            if (generic_iv_tbl.ContainsKey(obj) && generic_iv_tbl[obj].Count > 0)
                return true;

            return false;
        }

        internal static object generic_ivar_get(object obj, string id)
        {
            if (obj == null)
                obj = nil_ivar_singleton;

            if (generic_iv_tbl.ContainsKey(obj))
                return generic_iv_tbl[obj][id];

            return null;
        }

        internal static void generic_ivar_set(object obj, string id, object val)
        {
            if (obj == null)
                obj = nil_ivar_singleton;

            if (!generic_iv_tbl.ContainsKey(obj))
                generic_iv_tbl[obj] = new Dictionary<string, object>();

            generic_iv_tbl[obj][id] = val;
        }

        internal static bool generic_ivar_defined(object obj, string id)
        {
            if (obj == null)
                obj = nil_ivar_singleton;
            if (generic_iv_tbl.ContainsKey(obj))
                return generic_iv_tbl[obj].ContainsKey(id);

            return false;
        }

        internal static bool generic_ivar_remove(object obj, string id, out object val)
        {
            if (obj == null)
                obj = nil_ivar_singleton;

            if (generic_iv_tbl.ContainsKey(obj))
            {
                if (generic_iv_tbl[obj].ContainsKey(id))
                {
                    val = generic_iv_tbl[obj][id];
                    generic_iv_tbl[obj].Remove(id);
                    return true;
                }
            }

            val = null;
            return false;
        }

        internal static Dictionary<string, object> get_generic_ivars(object obj)
        {
            if (!exivar_p(obj)) return null;

            if (obj == null)
                obj = nil_ivar_singleton;

            if (generic_iv_tbl.ContainsKey(obj))
                return generic_iv_tbl[obj];
            else
                return null;
        }

        internal static void rb_copy_generic_ivar(object clone, object obj)
        {
            if (generic_iv_tbl.ContainsKey(obj))
                generic_iv_tbl[clone] = new Dictionary<string, object>(generic_iv_tbl[obj]);
        }

        // OBJ_INFECT macro
        internal static void obj_infect(object x, object s)
        {
            if (!rb_special_const_p(x) && !rb_special_const_p(s))
            {
                if (x is Basic && s is Basic)
                {
                    Basic p1 = (Basic)x;
                    Basic p2 = (Basic)s;

                    if (p2.Tainted)
                        p1.Tainted = true;
                }
            }
        }

        // OBJSETUP macro
        internal static void obj_setup(object obj, Class klass, object t)
        {
            if (obj is Basic)
            {
                Basic clone = (Basic)obj;
                // FIXME: need to copy all flags from t
                if (t is Basic)
                {
                    Basic other = (Basic)t;
                    clone.Frozen = other.Frozen;
                }
                clone.my_class = klass;
                if (Eval.rb_safe_level() >= 3)
                    clone.Tainted = true;
            }
        }

        // CLONESETUP macro
        internal static void clone_setup(object clone, object obj)
        {
            obj_setup(clone, Class.rb_singleton_class_clone(obj), obj);
            Class.rb_singleton_class_attached(((Object)clone).my_class, clone);
            if (exivar_p(obj))
                rb_copy_generic_ivar(clone, obj);
        }

    }
}
