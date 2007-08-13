/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Globalization;


namespace Ruby.Methods
{
    
    internal class os_each_obj : VarArgMethodBody0 // status: done
    {
        internal static os_each_obj singleton = new os_each_obj();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            if (rest.Count > 0)
                if (rest[0] is Class)
                    return each_object((Class)rest[0], caller, block);
                else
                    throw new TypeError("class or module required").raise(caller);
            else
                return each_object(null, caller, block);
        }

        internal int each_object(Class klass, Frame caller, Proc block)
        {
            int n = 0;

            System.GC.Collect();
            ObjectSpace.objects.RemoveAll(delegate(System.WeakReference r) { return !r.IsAlive; });

            System.Collections.Generic.List<System.WeakReference> copy = new System.Collections.Generic.List<System.WeakReference>(ObjectSpace.objects);
            copy.Reverse();
            foreach (System.WeakReference r in copy)
                if (klass == null || Class.CLASS_OF(r.Target).is_kind_of(klass))
                {
                    if (r.Target is Class && ((Class)r.Target)._type == Class.Type.IClass || ((Class)r.Target)._type == Class.Type.Singleton)
                        continue;

                    Proc.rb_yield(block, caller, r.Target);
                    n++;
                }

            return n;
        }
    }

    
    internal class add_final : MethodBody0 // author: Brian, status: done
    {
        internal static add_final singleton = new add_final();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object finalizer)
        {
            Errors.rb_warn("ObjectSpace::add_finalizer is deprecated; use define_finalizer");

            if (!Eval.RespondTo(finalizer, "call"))
            {
                throw new ArgumentError("wrong type argument " + Class.rb_obj_classname(finalizer) + " (should be callable)").raise(caller);
            }

            ObjectSpace._finalizers.Add(finalizer);
            return finalizer;
        }
    }

    
    internal class rm_final : MethodBody0 //author: Brian, status: done
    {
        internal static rm_final singleton = new rm_final();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object finalizer)
        {
            Errors.rb_warn("ObjectSpace::remove_finalizer is deprecated; use undefine_finalizer");
            ObjectSpace._finalizers.Remove(finalizer);
            return finalizer;
        }
    }

    
    internal class finals : MethodBody0 //author: Brian, status: done
    {
        internal static finals singleton = new finals();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Errors.rb_warn("ObjectSpace::finalizers is deprecated");
            return new Array(ObjectSpace._finalizers);
        }
    }

    
    internal class call_final : MethodBody0 //author: Brian, status: done
    {
        internal static call_final singleton = new call_final();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object obj)
        {
            Errors.rb_warn("ObjectSpace::call_finalizer is deprecated; use define_finalizer");
            if (obj is Basic)
                ((Basic)obj).finalize_flag = true;
            return obj;
        }
    }

    
    internal class define_final : VarArgMethodBody0 //author: Brian, status: done
    {
        internal static define_final singleton = new define_final();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            object finalizer = null;

            if (rest.Count == 1)
            {
                if (block == null)
                {
                    throw new ArgumentError("no block given").raise(caller);
                }
                finalizer = block;
            }
            else if (rest.Count == 2)
            {
                finalizer = rest[1];
                if (!Eval.RespondTo(finalizer, "call"))
                {
                    throw new ArgumentError("wrong type argument " + finalizer.ToString() + " (should be callable)").raise(caller);
                }
            }
            else
            {
                throw new ArgumentError("wrong number of arguments (" + rest.Count + " for 2)").raise(caller);
            }

            if (((Basic)rest[0]).finalizers == null)
                ((Basic)rest[0]).finalizers = new System.Collections.ArrayList();
            ((Basic)rest[0]).finalizers.Add(finalizer);

            Array entry = new Array(Eval.rb_safe_level(), finalizer);

            return entry;
        }
    }

    
    internal class undefine_final : MethodBody1 // author: Brian, status: done
    {
        internal static undefine_final singleton = new undefine_final();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            ((Basic)p1).finalizers.Clear();
            return p1;
        }
    }

    
    internal class id2ref : MethodBody1 // author: cjs, status: done
    {
        internal static id2ref singleton = new id2ref();

        public override object  Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Eval.rb_secure(4, caller);

            int ptr = Numeric.rb_num2long(p1, caller);

            if (ptr == 0)
                return false;
            if (ptr == 2)
                return true;
            if (ptr == 4)
                return null;

            object result;
            try
            {
                result = ObjectSpace.id_lookup[ptr];
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new RangeError(string.Format(CultureInfo.InvariantCulture, "{0} is not id value", ptr.ToString("x", CultureInfo.InvariantCulture))).raise(caller);
            }

            if (((result == null) || (result is bool) || (result is int)) || ((result is Basic) && ((Basic)result).my_class != null))
                return result;

            throw new RangeError(string.Format(CultureInfo.InvariantCulture, "{0} is recycled object", ptr.ToString("x", CultureInfo.InvariantCulture))).raise(caller);
        }
    }
}
