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
    
    internal class exc_alloc : MethodBody0 // author: cjs, status: done
    {
        internal static exc_alloc singleton = new exc_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Exception((Class)recv);
        }
    }

    
    internal class exc_exception : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static exc_exception singleton = new exc_exception();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            if ((rest.Count == 0) || ((rest.Count == 0) && (rest.value[0].Equals(recv))))
                return recv;

            return Eval.CallPrivate(rest.value[0], caller, "new", block, rest);
        }
    }

    
    internal class exc_initialize : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static exc_initialize singleton = new exc_initialize();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            if (rest.Count == 0)
                return recv;

            Exception exc = (Exception)recv;
            exc.instance_variable_set("mesg", rest.value[0]);

            return exc;
        }
    }

    
    internal class exc_to_s : MethodBody0 // author: cjs, update:war, status: done
    {
        internal static exc_to_s singleton = new exc_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            //VALUE mesg = rb_attr_get(exc, rb_intern("mesg"));

            //if (NIL_P(mesg)) return rb_class_name(CLASS_OF(exc));
            //if (OBJ_TAINTED(exc)) OBJ_TAINT(mesg);
            //return mesg;

            Exception exc = (Exception)recv;

            object mesg = exc.instance_variable_get("mesg");

            if (mesg == null || (mesg is String && ((String)mesg).value == null)) return new String(exc.my_class._name);
            if(exc.Tainted) ((Object)mesg).Tainted = true;
            
            return mesg;
        }
    }

    
    internal class exc_to_str : MethodBody0 // author: cjs, status: done
    {
        internal static exc_to_str singleton = new exc_to_str();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Eval.CallPrivate(recv, caller, "to_s", null, new object[] { });
        }
    }
    

    
    internal class exc_inspect : MethodBody0 // author: cjs, status: done
    {
        internal static exc_inspect singleton = new exc_inspect();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Exception exc = (Exception)recv;

            String mesg = String.ObjectAsString(exc, caller);
            if (mesg == null || mesg.value == "")
                return new String(exc.my_class._name);
            else
                return new String("#<" + exc.my_class._name + ": " + mesg + ">");
        }
    }

    
    internal class exc_backtrace : MethodBody0 // author: cjs, status: done
    {
        internal static exc_backtrace singleton = new exc_backtrace();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Exception exception = (Exception) recv;

            if (exception.instance_vars.ContainsKey("bt"))
                return exception.instance_vars["bt"];
            else
                return null;
        }
    }

    
    internal class exc_set_backtrace : MethodBody1 // author: cjs, status: done
    {
        internal static exc_set_backtrace singleton = new exc_set_backtrace();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object bt)
        {
            Exception exception = (Exception)recv;

            string err = "backtrace must be Array of String";

            if (bt != null)
            {
                if (bt is String)
                    bt = new Array(new object[] { bt });
                else if (bt is Array)
                {
                    Array ary = (Array)bt;
                    for (int i = 0; i < ary.Count; i++)
                        if (!(ary[i] is String))
                            throw new TypeError(err).raise(caller);
                }
                else
                    throw new TypeError(err).raise(caller);
            }

            return exception.instance_variable_set("bt", bt);
        }
    }


    
    internal class exit_alloc : MethodBody0 // author: cjs, status: done
    {
        internal static exit_alloc singleton = new exit_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new SystemExit(null, (Class)recv);
        }
    }

    
    internal class exit_initialize : VarArgMethodBody0 // author: wak, status: done
    {
        internal static exit_initialize singleton = new exit_initialize();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array args)
        {
            SystemExit exc = (SystemExit)recv;

            if (args.Count > 0 && args[0] is int)
                exc._status = (int)args[0];
            else
                exc._status = 0;

            if (args.Count > 1)
                exc_initialize.singleton.Call1(last_class, recv, caller, block, args[1]);

            return exc;
        }
    }

    
    internal class exit_status : MethodBody0 // author: cjs, status: done
    {
        internal static exit_status singleton = new exit_status();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((SystemExit)recv)._status;
        }
    }

    
    internal class exit_success_p : MethodBody0 // author: cjs, status: done
    {
        internal static exit_success_p singleton = new exit_success_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((SystemExit)recv)._status == 0; // 0 = exit success
        }
    }


    
    internal class name_err_alloc : MethodBody0 // author: cjs, status: done
    {
        internal static name_err_alloc singleton = new name_err_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new NameError((Class)recv);
        }
    }

    
    internal class name_err_initialize : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static name_err_initialize singleton = new name_err_initialize();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            NameError name_err = (NameError)recv;
            if (rest.Count > 0)
                exc_initialize.singleton.Call1(last_class, recv, caller, block, rest[0]);
            if (rest.Count > 1)
                name_err.instance_variable_set("name", rest[1]);

            return recv;
        }
    }

    
    internal class name_err_name : MethodBody0 // author: cjs, status: done
    {
        internal static name_err_name singleton = new name_err_name();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((NameError)recv).instance_variable_get("name");
        }
    }

    
    internal class name_err_to_s : MethodBody0 // author: cjs, status: done
    {
        internal static name_err_to_s singleton = new name_err_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return exc_to_s.singleton.Call0(last_class, recv, caller, block);
        }
    }


    
    internal class name_err_mesg_new : MethodBody3 // status: done
    {
        internal static name_err_mesg_new singleton = new name_err_mesg_new();

        public override object Call3(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, object p3)
        {
            Data d = new Data();

            d.instance_variable_set("0", p1); //mesg
            d.instance_variable_set("1", p2); //recv
            d.instance_variable_set("2", p3); //method

            return d;
        }
    }

    
    internal class name_err_mesg_load : MethodBody1 // author: cjs, status: done
    {
        internal static name_err_mesg_load singleton = new name_err_mesg_load();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return p1;
        }
    }

    
    internal class name_err_mesg_to_str : MethodBody0 // author: cjs, status: done
    {
        internal static name_err_mesg_load singleton = new name_err_mesg_load();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Data data = (Data)recv;

            object mesg = data.instance_vars["0"];
            object obj = null;
            if (mesg == null)
                return null;
            else
            {
                string desc = null;
                String d = null;

                obj = data.instance_vars["0"];
                if (obj == null)
                    desc = "nil";
                else if (obj.Equals(true))
                    desc = "true";
                else if (obj.Equals(false))
                    desc = "false";
                else
                {
                    try
                    {
                        d = Object.Inspect(obj, caller);
                    }
                    catch (RubyException)
                    { }

                    if (d == null)
                    {
                        d = (String)rb_any_to_s.singleton.Call0(last_class, obj, caller, null);
                    }
                    desc = d.value;
                }

                if (desc != null && desc.Length > 0 && desc[0] != '#')
                    d = new String(string.Format("{0}:{1}", desc, ((Object)obj).my_class._name));

                Array args = new Array();
                args.Add(mesg);
                args.Add(data.instance_vars["2"]);
                args.Add(d);
                mesg = rb_f_sprintf.singleton.Call(last_class, recv, caller, null, args);
            }
            
            if (((Object)obj).Tainted)
                ((Object)mesg).Tainted = true;
            return mesg;
        }
    }


    
    internal class nometh_err_alloc : MethodBody0 // author: cjs, status: done
    {
        internal static nometh_err_alloc singleton = new nometh_err_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new NoMethodError(null, (Class)recv);
        }
    }

    
    internal class nometh_err_initialize : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static nometh_err_initialize singleton = new nometh_err_initialize();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            if (rest.Count == 0)
                return recv;

            NoMethodError exc = (NoMethodError)recv;
            name_err_initialize.singleton.Call(last_class, recv, caller, block, rest);
            if (rest.Count > 2)
                exc.instance_variable_set("args", rest[2]);

            return exc;
        }
    }

    
    internal class nometh_err_args : MethodBody0 // author: cjs, status: done
    {
        internal static nometh_err_args singleton = new nometh_err_args();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((NoMethodError)recv).instance_variable_get("args");
        }
    }


    
    internal class syserr_alloc : MethodBody0 // author: cjs, status: done
    {
        internal static syserr_alloc singleton = new syserr_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new SystemCallError((Class)recv);
        }
    }

    
    internal class syserr_initialize : VarArgMethodBody0 // author: cjs/war, status: done
    {
        internal static syserr_initialize singleton = new syserr_initialize();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            string err = null;
            object mesg = null, error = null;

            Class klass = (Class)Ruby.Methods.rb_obj_class.singleton.Call0(last_class, recv, caller, block);

            if (klass == Ruby.Runtime.Init.rb_eSystemCallError)
            {
                if (Class.rb_scan_args(caller, rest, 1, 1, false) == 1)
                {
                    mesg = rest[0];
                    if (mesg is int)
                    {
                        error = mesg;
                        mesg = null;
                    }
                }
                else
                {
                    //rest.Length == 2
                    mesg = rest[0];
                    error = rest[1];
                }
                if (error != null && Errno.syserr_tbl.TryGetValue(Numeric.rb_num2long(error, caller), out klass))
                {
                    if (!(recv is Object))
                    {
                        throw new TypeError("invalid instance type").raise(caller);
                    }
                    ((Basic)recv).my_class = klass;
                }
            }
            else
            {
                if (Class.rb_scan_args(caller, rest, 0, 1, false) == 1)
                {
                    mesg = rest[0];
                    error = ((Class)klass).const_get("Errno", caller);
                }
            }
            if(error != null) {
                //TODO - there is more to this, c ruby code. 
                //can return "Unknown Error" or "Unknown error"
                //err = strerror(NUM2LONG(error));
                err = "Unknown error";
            }
            else{
                err = "unknown error";
            }
            if (mesg != null)
            {
                object str = mesg;
                str = String.RStringValue(str, caller);
                ((String)mesg).value = string.Format("{0} - {1}", err, ((String)str).value);
            }
            else
            {
                mesg = new String(err);
            }
            exc_initialize.singleton.Call1(last_class, recv, caller, block, mesg);
            ((Object)recv).instance_variable_set("errno", error);
            return recv;
                    
        }
    }

    
    internal class syserr_errno : MethodBody0 // author: cjs, status: done
    {
        internal static syserr_errno singleton = new syserr_errno();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((SystemCallError)recv).errno;
        }
    }

    
    internal class syserr_eqq : MethodBody1 // author: cjs, status: done
    {
        internal static syserr_eqq singleton = new syserr_eqq();

        public override object Call1(Class last_class, object self, Frame caller, Proc block, object exc)
        {
            if (!Object.rb_obj_is_kind_of(exc, Ruby.Runtime.Init.rb_eSystemCallError))
                return false;
            if (self.GetType() == typeof(SystemCallError))
                return true;
            // else self is a derived type

            return ((SystemCallError)exc).errno == ((SystemCallError)self).errno;
        }
    }
}
