/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/


using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using PERWAPI;


namespace Ruby.Compiler.AST
{

    internal abstract class CALL : Node
    {
        internal ListGen args;
        internal bool IsPublic;

        internal CALL(Node args, YYLTYPE location)
            : base(location)
        {
            if (args == null)
                this.args = null;
            else if (args is ListGen)
                this.args = (ListGen)args;
            else
                this.args = new ARGS(args, null, null, null, location);
        }

        internal Node block
        {
            set { ((ARGS)args).block = value; }
        }

        internal override void Defined(CodeGenContext context)
        {
            if (args != null)
                new AND(new ProxyNode(MethodDefined, location), new ProxyNode(((Node)args).Defined, location), location).GenCode(context);
            else
                MethodDefined(context);
        }

        internal virtual void MethodDefined(CodeGenContext context)
        {
        }

        internal void GenCall(CodeGenContext context)
        {
            int result = context.CreateLocal("result", PrimitiveType.Object);

            PERWAPI.CILLabel endLabel = context.NewLabel();
            PERWAPI.CILLabel retryLabel = context.NewLabel();

            context.CodeLabel(retryLabel);

            context.StartBlock(Clause.Try);
            {
                // object result = Call(...)
                GenCall0(context);
                context.stloc(result);

                context.Goto(endLabel);
            }
            PERWAPI.TryBlock tryBlock = context.EndTryBlock();
            context.StartBlock(Clause.Catch);
            {
                CatchBreakException(context, result, endLabel);
            }
            context.EndCatchBlock(Runtime.BreakExceptionRef, tryBlock);
            context.StartBlock(Clause.Catch);
            {
                CatchRetryException(context, retryLabel);
            }
            context.EndCatchBlock(Runtime.RetryExceptionRef, tryBlock);

            context.CodeLabel(endLabel);
            context.ldloc(result);

            context.ReleaseLocal(result, true);
        }


        private void CatchBreakException(CodeGenContext context, int result, PERWAPI.CILLabel endLabel)
        {
            // catch (Exception exception)
            int exception = context.StoreInTemp("exception", Runtime.BreakExceptionRef, location);
            PERWAPI.CILLabel reThrowLabel = context.NewLabel();

            // if (exception.scope != current_frame) goto reThrowLabel; 
            context.ldloc(0);
            context.ldloc(exception);
            context.ldfld(Runtime.BreakException.scope);
            context.bne( reThrowLabel);

            // result = exception.return_value;
            context.ldloc(exception);
            context.ldfld(Runtime.BreakException.return_value);
            context.stloc(result);

            // goto endLabel;
            context.Goto(endLabel);

            // reThrowLabel:
            context.CodeLabel(reThrowLabel);

            // throw exception;
            context.ldloc(exception);
            context.throwOp();

            context.ReleaseLocal(exception, true);
        }


        private void CatchRetryException(CodeGenContext context, PERWAPI.CILLabel retryLabel)
        {
            // catch (Exception exception)
            int exception = context.StoreInTemp("exception", Runtime.RetryExceptionRef, location);
            PERWAPI.CILLabel reThrowLabel = context.NewLabel();

            // if (exception.scope != current_frame) goto reThrowLabel; 
            context.ldloc(0);
            context.ldloc(exception);
            context.ldfld(Runtime.RetryException.scope);
            context.bne( reThrowLabel);

            // goto retryLabel
            context.Goto(retryLabel);

            // reThrowLabel:
            context.CodeLabel(reThrowLabel);

            // throw exception;
            context.ldloc(exception);
            context.throwOp();

            context.ReleaseLocal(exception, true);
        }

        internal abstract void GenCall0(CodeGenContext context);
    }



    internal class METHOD_CALL : CALL
    {
        // receiver.method_id(args)

        protected string method_id;
        private Node receiver;


        internal static Node Create(Node receiver, string method_id, Node args, YYLTYPE location)
        {
            if (method_id == ID.intern(Tokens.tOROP))
                return new OR(receiver, args, location);

            if (method_id == ID.intern(Tokens.tANDOP))

                return new AND(receiver, args, location);

            return new METHOD_CALL(receiver, method_id, args, location);
        }

        internal METHOD_CALL(string method_id, Node args, YYLTYPE location)
            : base(args, location)
        {
            System.Diagnostics.Debug.Assert(args != null);
            this.method_id = method_id;
            this.receiver = new SELF(location);
            this.IsPublic = false;
        }

        internal METHOD_CALL(string method_id, Node args, Node block, YYLTYPE location)
            : this(method_id, args, location)
        {
            this.block = block;
        }

        internal METHOD_CALL(Node receiver, string method_id, Node args, YYLTYPE location)
            : base(args, location)
        {
            this.method_id = method_id;
            this.receiver = receiver;
            this.IsPublic = true;
        }

        internal METHOD_CALL(Node receiver, string method_id, Node args, Node block, YYLTYPE location)
            : this(receiver, method_id, args, location)
        {
            this.block = block;
        }


        private ISimple self; 
        private ISimple arguments;
        private List<ISimple> fixed_arguments;
        private List<bool> fixed_created;

        internal override void GenCode0(CodeGenContext context)
        {
            if (Scanner.is_identchar(method_id[0]))
                context.newLine(location);
            
            SetLine(context);
            
            bool self_created, arguments_created = false;

            // object self = receiver;
            self = context.PreCompute(receiver, "receiver", out self_created);

            if (args.ShortAndSimple())
                fixed_arguments = args.GenFixedArgs(context, out fixed_created);
            else
                arguments = args.GenArgList(context, out arguments_created);


            GenCall(context);

            context.ReleaseLocal(self, self_created);

            if (args.ShortAndSimple())
                for (int i = 0; i < fixed_created.Count; i++)
                    context.ReleaseLocal(fixed_arguments[i], fixed_created[i]);
            else
                context.ReleaseLocal(arguments, arguments_created);
        }


        internal override void GenCall0(CodeGenContext context)
        {
            // Ruby.Eval.Call???(self, frame, method_id, arguments);
            self.GenSimple(context);
            context.ldloc(0);
            context.ldstr(method_id.ToString());
            if (args.ShortAndSimple())
            {
                foreach (ISimple arg in fixed_arguments)
                    arg.GenSimple(context);

                if (IsPublic)
                    context.call(Runtime.Eval.Call("Public", fixed_arguments.Count-1));
                else
                    context.call(Runtime.Eval.Call("Private", fixed_arguments.Count-1));
            }
            else
            {
                arguments.GenSimple(context);
                if (IsPublic)
                    context.call(Runtime.Eval.CallPublicA);
                else
                    context.call(Runtime.Eval.CallPrivateA);
            }
        }


        internal override string DefinedName()
        {
            return "method";
        }


        internal override void MethodDefined(CodeGenContext context)
        {
            // Eval.Find???Method(receiver, thisFrame, method_id)
            receiver.GenCode(context);
            context.ldloc(0);
            context.ldstr(method_id.ToString());
            if (IsPublic)
                context.call(Runtime.Eval.FindPublicMethod);
            else
                context.call(Runtime.Eval.FindPrivateMethod);
        }
    }



    internal class SUPER : CALL
    {
        // super(args)

        private Scope parent_scope;

        internal SUPER(Scope parent_scope, Node args, YYLTYPE location)
            : base(args, location)
        {
            this.parent_scope = parent_scope;
        }

        private ISimple arguments;

        
        /*
        private ListGen ParentArgs(CodeGenContext context)
        {
            Param[] parameters = context.Method.GetParams();       // BBTAG
            //Param[] parameters = context.orig_func.GetParams();
            
            if (parameters[3].GetName() == "args")
                return new PARAM("args", this.location);
            
            Node args = null;
            for (int i=4; i<parameters.Length; i++)
                args = Parser.append(args, new PARAM(parameters[i].GetName(), this.location));

            return new ARGS(args, null, null, new PARAM("block", this.location), this.location);
        }
        */

        private string ParentMethodName(CodeGenContext context)
        {
            if (parent_scope is BLOCK)
            {
                string methodName = null;
                Scope scope_cnt = parent_scope;
                while (!(scope_cnt is DEFN || scope_cnt is DEFS) && (scope_cnt != null))
                    scope_cnt = scope_cnt.parent_scope;

                if (scope_cnt is DEFN)
                    methodName = ((DEFN)scope_cnt).method_id;
                else
                    return "";          

                return methodName;
            }
            else
                return context.CurrentMethodName();
        }
        
        private ListGen ParentArgs(CodeGenContext context)
        {
            FORMALS formals = null;
            Node args = null;

            int depth = 0;
            if (parent_scope is BLOCK)
            {
                Scope scope_cnt = parent_scope;
                while (!(scope_cnt is DEFN || scope_cnt is DEFS) && (scope_cnt != null))
                {
                    scope_cnt = scope_cnt.parent_scope;
                    depth++;
                }

                if (scope_cnt is DEFN)
                    formals = ((DEFN)scope_cnt).formals;
            }
            else if (parent_scope is DEFN)
                formals = ((DEFN)parent_scope).formals;

            if (formals != null)
            {
                for (StaticLocalVar formal = formals.normal; formal != null; formal = (StaticLocalVar)formal.nd_next)
                {
                    if (parent_scope is BLOCK)
                        args = Parser.append(args, new StaticOuterVar(formal.vid, (BLOCK)parent_scope, depth, this.location));
                    else
                        args = Parser.append(args, new StaticLocalVar(parent_scope, formal.vid, this.location));
                }
            }

            if (parent_scope is BLOCK || parent_scope is SOURCEFILE)
                return new ARGS(args, null, null, null, this.location);              // FIXME: what about if there is a block arg to a super call within a block?
            else
                return new ARGS(args, null, null, new PARAM("block", this.location), this.location);
        }
         

        internal override void GenCode0(CodeGenContext context)
        {
            context.newLine(location);
            SetLine(context);

            if (args == null)
                args = ParentArgs(context);

            // object arguments = args;
            bool created;
            arguments = args.GenArgList(context, out created);


            GenCall(context);

            context.ReleaseLocal(arguments, created);
        }


        internal override void GenCall0(CodeGenContext context)
        {
            //Ruby.Eval.CallSuperA(last_class, caller, self, methodId, args);
            context.last_class(parent_scope);  
            //context.ruby_cbase(parent_scope);      // BBTAG
            context.ldloc(0);
            new SELF(location).GenCode(context);
            //context.ldstr(context.CurrentMethodName());      // BBTAG
            context.ldstr(ParentMethodName(context));
            //context.ldstr(context.OrigFuncName());
            arguments.GenSimple(context);
            context.call(Runtime.Eval.CallSuperA);
        }


        internal override string DefinedName()
        {
            return "super";
        }


        internal override void MethodDefined(CodeGenContext context)
        {
            // Eval.FindSuperMethod(last_class, thisFrame, currentMethod)
            context.last_class(parent_scope);             
            //context.ruby_cbase(parent_scope);
            context.ldloc(0);
            //context.ldstr(context.CurrentMethodName());   // BBTAG
            context.ldstr(ParentMethodName(context));
            //context.ldstr(context.OrigFuncName());
            context.call(Runtime.Eval.FindSuperMethod);
        }
    }



    internal class YIELD : CALL
    {
        // yield(args)

        internal YIELD(Node args, YYLTYPE location)
            : base(args, location)
        {
            System.Diagnostics.Debug.Assert(args != null);
        }

        private ISimple arguments;

        internal override void GenCode0(CodeGenContext context)
        {
            context.newLine(location);
            SetLine(context);

            // Ruby.ArgList arguments = args;
            bool created;
            arguments = args.GenArgList(context, out created);
            GenCall(context);
            context.ReleaseLocal(arguments, created);
        }

        internal override void GenCall0(CodeGenContext context)
        {
            // block.yield(caller, arguments)            
            LoadBlock(context);
            context.ldloc(0); // caller
            arguments.GenSimple(context);
            context.callvirt(Runtime.Proc.yield);
        }


        internal override string DefinedName()
        {
            return "yield";
        }


        internal override void Defined(CodeGenContext context)
        {
            LoadBlock(context);
        }
    }
}
