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
    internal class method_eq : MethodBody1 //author: Brian, status: done
    {
        internal static method_eq singleton = new method_eq();

        public override object Call1(Class last_class, object self, Frame caller, Proc block, object param0)
        {
            Method method = (Method)self;
            if (!(param0 is Method))
                return false;
            Method other = (Method)param0;

            if (Class.CLASS_OF(method) != Class.CLASS_OF(other))
                return false;

            if (method.oklass != other.oklass || method.rklass != other.rklass ||
                method.recv != other.recv || method.body != other.body)
                return false;

            return true;
        }
    }


    
    internal class method_call : MethodBody //author: Brian, status: done
    {
        internal static method_call singleton = new method_call();

        public override object Calln(Class last_class, object self, Frame caller, ArgList args)
        {
            int safe = -1;
            Method method = (Method)self;
            object result = null;

            if (method.recv == null)
                throw new TypeError("you cannot call unbound method; bind first").raise(caller);

            if (method.Tainted)
            {
                safe = Eval.ruby_safe_level;
                if (Eval.ruby_safe_level < 4)
                    Eval.ruby_safe_level = 4;
            }

            result = method.body.body.Calln(last_class, method.recv, caller, args);

            if (safe >= 0)
                Eval.ruby_safe_level = safe;

            return result;
        }
    }


    
    internal class method_arity : MethodBody0 //status: done
    {
        internal static method_arity singleton = new method_arity();

        public override object Call0(Class last_class, object self, Frame caller, Proc block)
        {
            return ((Method)self).body.arity;
        }
    }


    
    internal class method_clone : MethodBody0 //author: Brian, status: done
    {
        internal static method_clone singleton = new method_clone();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Method self = (Method)recv;
            Method clone = new Method(self.recv, self.id, self.oid, self.rklass, self.oklass, self.body, (self.my_class == Ruby.Runtime.Init.rb_cUnboundMethod));
            Object.clone_setup(clone, self);

            return clone;
        }
    }


    
    internal class method_inspect : MethodBody0 //author: Brian, status: done
    {
        internal static method_inspect singleton = new method_inspect();

        public override object Call0(Class last_class, object self, Frame caller, Proc block)
        {
            Method method = (Method)self;
            string str = "#<" + Class.CLASS_OF(method).classname() + ": ";
            string sharp = "#";

            if (method.oklass._type == Class.Type.Singleton)
            {
                object v = method.oklass.attached;

                if (method.recv == null)
                {
                    str += Eval.CallPublic(method.oklass, caller, "inspect", null);
                }
                else if (method.recv == v)
                {
                    str += Eval.CallPublic(v, caller, "inspect", null);
                    sharp = ".";
                }
                else
                {
                    str += Eval.CallPublic(method.recv, caller, "inspect", null);
                    str += "(" + Eval.CallPublic(v, caller, "inspect", null) + ")";
                    sharp = ".";
                }
            }
            else
            {
                str += method.rklass.classname();
                if (method.rklass != method.oklass)
                {
                    str += "(" + method.oklass.classname() + ")";
                }
            }

            str += sharp + method.oid + ">";

            return new String(str);
        }
    }


    
    internal class method_proc : MethodBody0 //status: done
    {
        internal static method_proc singleton = new method_proc();

        public override object Call0(Class last_class, object self, Frame caller, Proc block)
        {
            Method method = (Method)self;
            return new Proc(method.recv, block, method.body.body, method.body.arity, ProcKind.Method);
        }
    }


    
    internal class method_unbind : MethodBody0 //author: Brian, status: done
    {
        internal static method_unbind singleton = new method_unbind();

        public override object Call0(Class last_class, object self, Frame caller, Proc block)
        {
            Method orig = (Method)self;
            Method method = new Method(null, orig.id, orig.oid, orig.rklass, orig.oklass, orig.body, true);
            method.Tainted |= orig.Tainted;
            return method;
        }
    }

    
    internal class umethod_bind : MethodBody1//author: Brian, status: done
    {
        internal static umethod_bind singleton = new umethod_bind();

        public override object Call1(Class last_class, object self, Frame caller, Proc block, object recv)
        {
            Method method = (Method)self;
            if (method.rklass != Class.CLASS_OF(recv))
            {
                if (method.rklass._type == Class.Type.Singleton)
                    throw new TypeError("singleton method called for a different object").raise(caller);

                if (!Object.rb_obj_is_kind_of(recv, method.rklass))
                {
                    throw new TypeError("bind argument must be an instance of " + method.rklass.classname()).raise(caller);
                }                
            }

            //Method boundMethod = new Method(recv, method.id, method.oid, Class.CLASS_OF(recv), method.body);
            Method boundMethod = new Method(recv, method.id, method.oid, Class.CLASS_OF(recv), method.oklass, method.body, false);
            return boundMethod;
        }
    }
}
