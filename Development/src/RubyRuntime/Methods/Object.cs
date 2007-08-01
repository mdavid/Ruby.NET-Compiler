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
    
    internal class rb_true : MethodBody0 //status: done
    {
        internal static rb_true singleton = new rb_true();

        public override object Call0(Class last_class, object self, Frame caller, Proc block)
        {
            return true;
        }
    }

    
    internal class main_to_s : MethodBody0 //status: done
    {
        internal static main_to_s singleton = new main_to_s();

        public override object Call0(Class last_class, object self, Frame caller, Proc block)
        {
            return new String("main");
        }
    }

    
    internal class rb_equal : MethodBody1 //status: done
    {
        internal static rb_equal singleton = new rb_equal();

        public override object Call1(Class last_class, object self, Frame caller, Proc block, object other)
        {
            return Object.Equal(self, other, caller);
        }
    }


    
    internal class rb_class_allocate_instance : MethodBody0 //status: done
    {
        internal static rb_class_allocate_instance singleton = new rb_class_allocate_instance();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Object((Class)recv);
        }
    }

    
    internal class rb_obj_dummy : MethodBody0 //status: done
    {
        internal static rb_obj_dummy singleton = new rb_obj_dummy();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            return null;
        }
    }
    

    internal class rb_obj_inspect : MethodBody0 // author: cjs/war, status: done
    {
        internal static rb_obj_inspect singleton = new rb_obj_inspect();

        public override object Call0(Class last_class, object obj, Frame caller, Proc block)
        {

            if (obj is Object && ((Object)obj).instance_vars.Count > 0)
            {
                String str;
                string s;
                string c = Class.rb_obj_classname(obj);

                if ((bool)rb_inspecting_p(last_class, obj, caller, block))
                {
                    s = string.Format("#<{0}:0x{1:x} ...>", c, obj.GetHashCode());
                    return new String(s);
                }

                str = new String();
                str.value = string.Format("-<{0}:0x{1:x}", c, obj.GetHashCode());
                return rb_protect_inspect(last_class, caller, block, inspect_obj, obj, str);
            }
            return Eval.CallPrivate0(obj, caller, "to_s", null);
        }

        public delegate  object Inspect_Func(Frame caller, object obj, object arg);
        public static object rb_protect_inspect(Class last_class, Frame caller, Proc block, Inspect_Func func, object obj, object arg)
        {
            object inspect_tbl = get_inspect_tbl(last_class, caller, block, true);
            object id = rb_obj_id.singleton.Call0(last_class, obj, caller, block);
            if ((bool)rb_ary_includes.singleton.Call1(last_class, inspect_tbl, caller, block, id))
            {
                return func(caller, obj, arg);
            }
            rb_ary_push.singleton.Call1(last_class, inspect_tbl, caller, block, id);

            object result;
            try
            {
                result = func(caller, obj, arg);
            }
            finally
            {
                inspect_ensure(last_class, caller, block, obj);                
            }
            return result; 
        }

        public static object inspect_ensure(Class last_class, Frame caller, Proc block, object obj)
        {
            object inspect_tbl;

            inspect_tbl = get_inspect_tbl(last_class, caller, block, false);
            if (inspect_tbl != null)
            {
                rb_ary_pop.singleton.Call0(last_class, inspect_tbl, caller, block);
            }
            return 0;
        }

        public static object inspect_obj(Frame caller, object obj, object str)
        {
            foreach (System.Collections.Generic.KeyValuePair<string, object> o in ((Object)obj).instance_vars)
            {
                object str2;
                string ivname;

                if(Class.CLASS_OF(o.Value) == null) continue;
                if(!Symbol.is_instance_id(o.Key)) continue;
                if (((String)str).value[0] == '-') /* first element */
                {
                    ((String)str).value = "#" + ((String)str).value.Substring(1);
                    ((String)str).value += " ";
                }
                else
                {
                    ((String)str).value += ", ";
                }
                ivname = o.Key;
                ((String)str).value += ivname;
                ((String)str).value += "=";
                str2 = Object.Inspect(o.Value, caller);
                ((String)str).value += str2;
                Object.obj_infect(((String)str).value, str2);
            }
            ((String)str).value += ">";
            ((String)str).value = "#" + ((String)str).value.Substring(1);
            Object.obj_infect(str, obj);

            return str;
        }       


        public static object rb_inspecting_p(Class last_class, object obj, Frame caller, Proc block)
        {
            object inspect_tbl;

            inspect_tbl = get_inspect_tbl(last_class, caller, block, false);
            if (inspect_tbl == null) return false;
            return rb_ary_includes.singleton.Call1(last_class, inspect_tbl, caller, block, rb_obj_id.singleton.Call0(last_class, obj, caller, block));
        }


        public static object get_inspect_tbl(Class last_class, Frame caller, Proc block, bool create)
        {
            Thread curr_thread = (Thread)rb_thread_current.singleton.Call0(last_class, null, caller, block);
            string inspect_key = "__inspect_key__";
            object inspect_tbl = Thread.rb_thread_local_aref(curr_thread, "__inspect_key__", caller);

            if (inspect_tbl == null)
            {
                if (create)
                {
                    //tbl_init:
                    inspect_tbl = new Array();
                    Thread.rb_thread_local_aset(curr_thread, inspect_key, inspect_tbl, caller);
                }
            }
            else if (!(inspect_tbl is Array))
            {
                Errors.rb_warn("invalid inspect_tbl value");
                if (create)
                {
                    // goto tbl_init;
                    inspect_tbl = new Array();
                    Thread.rb_thread_local_aset(curr_thread, inspect_key, inspect_tbl, caller);
                    return inspect_tbl;
                }
                Thread.rb_thread_local_aset(curr_thread, "__inspect_key__", null, caller);
                return null;
            }
            return inspect_tbl;
        }


    }
}
