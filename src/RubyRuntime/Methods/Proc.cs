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
    
    internal class proc_s_new : MethodBody0 //status: done
    {
        internal static proc_s_new singleton = new proc_s_new();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (block != null)
                return new Proc(block.self, block.block, block.body, block._arity, ProcKind.RawProc);
            else if (caller.block_arg != null)
                return Call0(last_class, recv, caller, caller.block_arg);
            else
                throw new ArgumentError("tried to create Proc object without a block").raise(caller);
        }
    }

    
    internal class proc_clone : MethodBody0 //status: done
    {
        internal static proc_clone singleton = new proc_clone();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Proc self = (Proc)recv;
            return new Proc(self.self, self.block, self.body, self._arity, self.kind);
        }
    }

    
    internal class proc_dup : MethodBody0 //status: done
    {
        internal static proc_dup singleton = new proc_dup();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Proc self = (Proc)recv;
            return new Proc(self.self, self.block, self.body, self._arity, self.kind);
        }
    }

    
    internal class proc_call : MethodBody //status: done
    {
        internal static proc_call singleton = new proc_call();


        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            Proc proc = (Proc)recv;
            switch (proc.kind)
            {
                case ProcKind.Method:
                case ProcKind.Lambda:
                    {
                        args.single_arg = false;

                        if (!proc.HasCorrectArgs(args.Length))
                            throw new ArgumentError(string.Format("wrong number of arguments ({0} for {1})", args.Length, System.Math.Abs(proc._arity))).raise(caller);

                        try
                        {
                            return Proc.rb_yield(proc, caller, args);
                        }
                        catch (ReturnException exception)
                        {
                            return exception.return_value;
                        }
                        catch (BreakException exception)
                        {
                            return exception.return_value;
                        }
                    }
                case ProcKind.RawProc:
                    {
                        try
                        {
                            return Proc.rb_yield(proc, caller, args);
                        }
                        catch (BreakException)
                        {
                            throw new LocalJumpError("break from proc-closure").raise(caller);
                        }
                    }
                default:
                case ProcKind.Block:
                    {
                        return Proc.rb_yield(proc, caller, args);
                    }
            }
        }
    }

    
    internal class proc_arity : MethodBody0 //status: done
    {
        internal static proc_arity singleton = new proc_arity();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Proc)recv)._arity;
        }
    }

    
    internal class proc_eq : MethodBody1 // author: cjs, status: partial, comment: execution info
    {
        internal static proc_eq singleton = new proc_eq();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (recv == p1) return true;
            if (!(p1 is Proc)) return false;
            //if (RDATA(other)->dmark != (RUBY_DATA_FUNC)blk_mark) return false; //What is this?
            if (recv.GetType() != p1.GetType()) return false;

            Proc data = (Proc)recv;
            Proc data2 = (Proc)p1;
            if (data.body != data2.body) return false;
            //if (data->var != data2->var) return false; // Where are var, scope and flags?
            //if (data->scope != data2->scope) return false;
            if (data.instance_vars != data2.instance_vars) return false;
            //if (data->flags != data2->flags) return false;

            return true;
        }
    }


    
    internal class proc_to_s : MethodBody0 // author: cjs, status: partial, comment: node info
    {
        internal static proc_to_s singleton = new proc_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Proc self = (Proc)recv;

            string cname = self.my_class._name;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("#<");
            sb.Append(cname);
            sb.Append(":0x");
            sb.Append(self.body.GetHashCode().ToString("X"));
            //if ((node = data->frame.node) || (node = data->body))
            //{
            //    //sb.Append("@");
            //    //sb.Append(node->nd_file);
            //    //sb.Append(":");
            //    //sb.Append(nd_line(node));
            //}
            sb.Append(">");
            String str = new String(sb.ToString());
            str.Tainted = self.Tainted;

            return str;
        }
    }


    
    internal class proc_to_self : MethodBody0 //status: done
    {
        internal static proc_to_self singleton = new proc_to_self();

        public override object Call0(Class last_class, object self, Frame caller, Proc block)
        {
            return self;
        }
    }

    
    internal class proc_binding : MethodBody0 // author: cjs, status: partial, comment: need to duplicate frame
    {
        internal static proc_binding singleton = new proc_binding();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Binding binding = new Binding(caller, recv);         
            return binding;
        }
    }
}
