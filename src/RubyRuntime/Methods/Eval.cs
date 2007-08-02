/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using Ruby.Runtime;
using System.Collections.Generic;


namespace Ruby.Methods
{
    
    internal class rb_f_loop : MethodBody0 //status: done
    {
        internal static rb_f_loop singleton = new rb_f_loop();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            while (true)
                Proc.rb_yield(block, caller);
        }
    }


    
    internal class proc_lambda : MethodBody0 //status: partial
    {
        internal static proc_lambda singleton = new proc_lambda();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            // Fixme: are these arguments correct?
            return new Proc(recv, block.block, block.body, block._arity, ProcKind.Lambda);
        }
    }

    
    internal class rb_f_block_given_p : MethodBody0 //status: done
    {
        internal static rb_f_block_given_p singleton = new rb_f_block_given_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return caller.block_arg != null;
        }
    }

    
    internal class rb_f_caller : MethodBody //status: done
    {
        internal static rb_f_caller singleton = new rb_f_caller();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 0)
                return Call1(last_class, recv, caller, args.block, 1);
            else
                return Call1(last_class, recv, caller, args.block, args[0]);
        }

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object level)
        {
            Array list = new Array();
            int skip = (int)level;

            int i = 0;
            while (caller != null)
            {
                if (i >= skip)
                    list.Add(new String(caller.callPoint()));

                caller = caller.caller;
                i++;
            }

            return list;
        }
    }


    
    internal class rb_method_missing : MethodBody //author: Brian, status: partial
    {
        internal static rb_method_missing singleton = new rb_method_missing();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            //throw new Ruby.NoMethodError(System.String.Format("undefined method '{0}' for {1}", args[0], Class.CLASS_OF(recv).class_real())).raise(caller);
            if (args.Length == 0)
                throw new ArgumentError("no id given").raise(caller);

            string id = Symbol.rb_to_id(caller, args[0]);

            string msg = null;
            string classStr = Class.CLASS_OF(recv).class_real().ToString();
            bool throwNameError = false;

            if (caller != null)
            {
                if (caller.call_status == CallStatus.Private)
                    msg = "private method `" + id + "' called for " + classStr;
                else if (caller.call_status == CallStatus.Protected)
                    msg = "protected method `" + id + "' called for " + classStr;
                else if (caller.call_status == CallStatus.VCall)
                {
                    msg = "undefined local variable or method `" + id + "' for " + classStr;
                    // BBTAG: how do we distinguish between method calls and variable access?
                    //throwNameError = true;
                }
                else if (caller.call_status == CallStatus.Super)
                {
                    msg = "super: no superclass method `" + id + "'";
                }
            }

            if (msg == null)
                msg = "undefined method `" + id + "' for " + classStr;

            if (throwNameError)
                throw new NameError(msg).raise(caller);
            else
                throw new NoMethodError(msg).raise(caller);
        }
    }

    
    internal class top_include : VarArgMethodBody0 // status: partial
    {
        internal static top_include singleton = new top_include();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array modules)
        {
            return rb_mod_include.singleton.Call(last_class, Ruby.Runtime.Init.rb_cObject, caller, block, modules);
        }
    }


    
    internal class rb_f_raise : MethodBody0 // author: cjs, status: done
    {
        internal static rb_f_raise singleton = new rb_f_raise();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Call1(last_class, recv, caller, block, Eval.ruby_errinfo.value);
        }

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Exception exc;

            if (p1 is Exception)
                exc = (Exception)p1;
            else if (p1 is String)
                exc = new RuntimeError(((String)p1).value);
            else if (Eval.RespondTo(p1, "exception"))
                exc = (Exception)Eval.CallPrivate(p1, caller, "exception", null);
            else
                throw new TypeError("exception object expected").raise(caller);

            throw exc.raise(caller);
        }

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            throw ((Exception)Eval.CallPrivate(p1, caller, "new", null, p2)).raise(caller);
        }
    }


    
    internal class rb_f_eval : VarArgMethodBody1 //status: partial 
    {
        internal static rb_f_eval singleton = new rb_f_eval();

        public override object Call(Class last_class, object self, Frame caller, Proc block, object src, Array args)
        {
            string file = "eval";
            int line = 1;

            object scope = null;
            object vfile = null;
            object vline = null;
            if (args.Count > 0)
            {
                scope = args[0];
                if (args.Count > 1)
                {
                    vfile = args[1];
                    file = ((String)vfile).value;
                    if (args.Count > 2)
                    {
                        vline = args[2];
                        line = (int)vline;
                    }
                }
            }

            if (scope == null)
                scope = new Binding(caller, self);

            if (!(scope is IContext))
                throw new TypeError(string.Format("wrong argument type {0} (expected Proc/Binding)", scope.GetType())).raise(caller);

            return Eval.eval(self, (String)src, (IContext)scope, file, line, caller);
        }
    }


    
    internal class rb_f_exit : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_exit singleton = new rb_f_exit();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Eval.rb_secure(4, caller);

            int istatus;
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 1)
            {
                if (rest[0] is bool)
                    if ((bool)rest[0])
                        istatus = Process.EXIT_SUCCESS;
                    else
                        istatus = Process.EXIT_FAILURE;
                else
                    istatus = Numeric.rb_num2long(rest[0], caller);
            }
            else
            {
                istatus = Process.EXIT_SUCCESS;
            }

            Eval.rb_exit(caller, istatus);

            return null; /* not reached */
        }
    }

    
    internal class rb_f_abort : VarArgMethodBody0 // author: cjs, status: partial, comment: error_print
    {
        internal static rb_f_abort singleton = new rb_f_abort();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Eval.rb_secure(4, caller);

            if (rest.Count == 0)
            {
                if (Eval.ruby_errinfo.value != null)
                {
                    //FIXME: error_print
                    if (Eval.ruby_errinfo.value is Exception)
                        ((Exception)Eval.ruby_errinfo.value).rubyException.Report();
                }
                Eval.rb_exit(caller, Process.EXIT_FAILURE);
            }
            else
            {
                Class.rb_scan_args(caller, rest, 0, 1, false);
                String mesg = String.RStringValue(rest[0], caller);
                rb_io_puts.singleton.Call1(last_class, IO.rb_stderr, caller, null, mesg);
                Eval.terminate_process(caller, Process.EXIT_FAILURE, mesg.value);
            }

            return null; /* not reached */
        }
    }

    
    internal class rb_f_at_exit : MethodBody0 // author: cjs, status: done
    {
        internal static rb_f_at_exit singleton = new rb_f_at_exit();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (block == null)
                throw new ArgumentError("called without a block").raise(caller);

            Program.end_procs.Push(block);

            return block;
        }
    }

    
    internal class rb_f_catch : MethodBody1 //status: partial
    {
        internal static rb_f_catch singleton = new rb_f_catch();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object symbol)
        {
            try
            {
                return Proc.rb_yield(block, caller);
            }
            catch (SymbolException e)
            {
                if (e.symbol.id_s == ((Symbol)symbol).id_s)
                    return e.args;
                else
                    throw e;
            }
        }
    }

    
    internal class rb_f_throw : VarArgMethodBody1 //status: partial
    {
        internal static rb_f_throw singleton = new rb_f_throw();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object symbol, Array rest)
        {
            throw new SymbolException((Symbol)symbol, rest);
        }
    }

    
    internal class rb_f_local_variables : MethodBody0 //author: Brian, status: partial
    {
        internal static rb_f_local_variables singleton = new rb_f_local_variables();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            // BBTAG: temporary, pending some method in Frame generated by compiler to get local variables, perhaps?
            System.Type frameType = caller.GetType();
            System.Reflection.FieldInfo[] fields = frameType.GetFields();
            Array vars = new Array();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                if (field.FieldType.Equals(new object().GetType()))
                {
                    vars.Add(new String(field.Name));
                }
            }
            
            if (caller.dynamic_vars != null)
                foreach (string id in caller.dynamic_vars.Keys)
                {
                    vars.Add(new String(id));
                }
            return vars;
        }
    }

    
    internal class rb_f_trace_var : VarArgMethodBody0 //status: partial
    {
        internal static rb_f_trace_var singleton = new rb_f_trace_var();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Symbol var = (Symbol)rest[0];
            Proc cmd = (Proc)rest[1];
            Variables.gvar_trace(var.id_s, cmd);
            return null;
        }
    }

    
    internal class rb_f_untrace_var : VarArgMethodBody0 //status: unimplemented
    {
        internal static rb_f_untrace_var singleton = new rb_f_untrace_var();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array args)
        {
            Symbol var = (Symbol)args[0];
            if (args.Count > 1)
                return Variables.gvar_untrace(var.id_s, (Proc)args[1]);
            else
                return Variables.gvar_untrace(var.id_s);
        }
    }

    
    internal class set_trace_func : MethodBody1 //status: unimplemented
    {
        internal static set_trace_func singleton = new set_trace_func();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            throw new NotImplementedError("set_trace_func").raise(caller);
        }
    }

    
    internal class rb_f_load : MethodBody1 //status: partial 
    {
        internal static rb_f_load singleton = new rb_f_load();

        //private static int indent = 0;

        private static void Indent(int tabs)
        {
            for (int i = 0; i < tabs; i++)
                System.Console.Write("  ");
        }

        private static Dictionary<string, object> compiled = new Dictionary<string, object>();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object fname)
        {
            return load(fname, caller, false);
        }

        internal static object load(object fname, Frame caller, bool try_add_ext)
        {
            //Indent(indent++);
            //System.Console.WriteLine("loading {0}", fname);

            if (Program.programs.ContainsKey(File.stripExtension(fname.ToString())))
            {
                Ruby.Compiler.AST.SOURCEFILE.LoadExisting(fname.ToString(), caller);
                ((Array)Eval.rb_features.value).Add(fname); // BBTAG: try to fix infinite recursion
                return true;
            }
         
            String path = File.rb_find_file((String)fname, try_add_ext); 

            if (path == null)
                throw new LoadError("No such file to load -- " + ((String)fname).value).raise(caller);

            ((Array)Eval.rb_features.value).Add(fname);

            if (path.value.EndsWith(".dll"))
                Compiler.AST.SOURCEFILE.Execute(path.value, caller);
            else
            {
                List<string> options;
                Compiler.AST.SOURCEFILE module = File.load_file(caller, path.value, false, out options, null);
                PERWAPI.PEFile assembly = module.GenerateCode(path.value, ".dll", null);
                Compiler.AST.SOURCEFILE.Load(assembly, caller, path.value);
            }

            //Indent(--indent);
            //System.Console.WriteLine("finished loading {0}", fname);
            return true;
        }
    }

    
    internal class rb_f_require : MethodBody1 //status: partial
    {
        internal static rb_f_require singleton = new rb_f_require();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object fname)
        {
            if (!((Array)Eval.rb_features.value).includes(fname, caller))
                rb_f_load.load(fname, caller, true);

            return true;
        }
    }

    
    internal class rb_f_autoload : MethodBody2 //author: Brian, status: done
    {
        internal static rb_f_autoload singleton = new rb_f_autoload();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            return rb_mod_autoload.singleton.Call2(last_class, Eval.ruby_cbase(caller), caller, block, p1, p2);
        }
    }

    
    internal class rb_f_autoload_p : MethodBody1 //author: Brian, status: done
    {
        internal static rb_f_autoload_p singleton = new rb_f_autoload_p();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return rb_mod_autoload_p.singleton.Call1(last_class, Eval.ruby_cbase(caller), caller, block, p1);
        }
    }

    
    internal class top_public : VarArgMethodBody0  //author: Brian, status: done
    {
        internal static top_public singleton = new top_public();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return rb_mod_public.singleton.Call(last_class, Ruby.Runtime.Init.rb_cObject, caller, block, rest);
        }
    }

   
    internal class top_private : VarArgMethodBody0 //author: Brian, status: done
    {
        internal static top_private singleton = new top_private();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return rb_mod_private.singleton.Call(last_class, Ruby.Runtime.Init.rb_cObject, caller, block, rest);
        }
    }

    
    internal class localjump_xvalue : MethodBody0 //status: done
    {
        internal static localjump_xvalue singleton = new localjump_xvalue();

        public override object Call0(Class last_class, object exc, Frame caller, Proc block)
        {
            return ((Exception)exc).instance_variable_get("@exit_value");
        }
    }

    
    internal class localjump_reason : MethodBody0 //status: done
    {
        internal static localjump_reason singleton = new localjump_reason();

        public override object Call0(Class last_class, object exc, Frame caller, Proc block)
        {
            return ((Exception)exc).instance_variable_get("@reason");
        }
    }
}
