/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/


using System.Diagnostics;


namespace Ruby.Compiler.AST
{

    internal class ControlFlowNode : Node
    {
        protected Scope parent_scope;
        protected Node return_val;    // optional

        internal ControlFlowNode(Scope parent_scope, YYLTYPE location)
            : base(location)
        {
            this.parent_scope = parent_scope;
        }

        internal ControlFlowNode(Scope parent_scope, ARGS return_val, YYLTYPE location)
            : base(location)
        {
            this.parent_scope = parent_scope;

            // no return value
            if (return_val == null)
                return;
 
            if (return_val.block != null)
                throw new System.Exception("block argument should not be given");

            // just a single value => return that value
            if (return_val.hashlist == null && return_val.array == null && return_val.parameters.nd_next == null)
            {
                this.return_val = return_val.parameters;
                return;
            }

            // just hash values => return a HASH
            if (return_val.parameters == null && return_val.array == null)
            {
                this.return_val = new HASH(return_val.hashlist, return_val.location);
                return;
            }

            // return *p => if p is an ARRAY then ... else ...
            if (return_val.parameters == null && return_val.hashlist == null)
            {
                this.array = return_val.array;
                this.return_val = new ProxyNode(ReturnArray, return_val.location);
            }
            else
            {
                this.args = return_val;
                this.return_val = new ProxyNode(ReturnRubyArray, return_val.location);
            }
        }

        private Node array;
        private ARGS args;

        protected void ReturnArray(CodeGenContext context)
        {
            // Ruby.Eval.Return(array, caller);
            array.GenCode(context);
            context.ldloc(0);
            context.call(Runtime.Eval.Return);
        }

        protected void ReturnRubyArray(CodeGenContext context)
        {
            args.GenCode(context);
        }
    }



    internal class RETURN : ControlFlowNode
    {
        // return return_val;

        internal RETURN(Scope current, Node return_val, YYLTYPE location)
            : base(current, (ARGS)return_val, location)
        {
        }

        internal override void GenCode0(CodeGenContext context)
        {
            context.newLine(location);

            if (return_val != null)
                return_val.GenCode(context);
            else
                context.ldnull();

            if (this.parent_scope is BLOCK)
            {
                //throw new Ruby.ReturnException(return_value, this.defining_scope);
                context.ldarg(0);  // current Ruby.MethodBody
                context.ldfld(Runtime.Block.defining_scope);
                context.newobj(Runtime.ReturnException.ctor);
                context.throwOp();
            }
            else if (this.parent_scope is BEGIN)
            {
                //throw new Ruby.ReturnException(return_value, caller);
                context.ldarg("caller");
                context.newobj(Runtime.ReturnException.ctor);
                context.throwOp();
            }
            else
            {
                // return
                context.stloc(parent_scope.returnTemp);
                context.Goto(context.labels.Return);
            }
        }
    }



    internal class BREAK : ControlFlowNode
    {
        // break return_val;

        internal BREAK(Scope current, Node return_val, YYLTYPE location)
            : base(current, (ARGS)return_val, location)
        {
        }

        internal override void GenCode0(CodeGenContext context)
        {
            context.newLine(location);

            if (return_val != null)
                return_val.GenCode(context);
            else
                context.ldnull();

            if (context.labels != null && context.labels.Break != null)
            {
                context.stloc(parent_scope.returnTemp);
                context.Goto(context.labels.Break);
            }
            else
            {
                System.Diagnostics.Debug.Assert(this.parent_scope is BLOCK);
                //throw new Ruby.BreakException(return_value, this.defining_scope);
                context.ldarg(0);  // current Ruby.MethodBody
                context.ldfld(Runtime.Block.defining_scope);
                context.newobj(Runtime.BreakException.ctor);
                context.throwOp();
            }
        }
    }



    internal class NEXT : ControlFlowNode
    {
        // next return_val;


        internal NEXT(Scope current, Node return_val, YYLTYPE location)
            : base(current, (ARGS)return_val, location)
        {
        }

        internal override void GenCode0(CodeGenContext context)
        {
            context.newLine(location);

            if (return_val != null)
                return_val.GenCode(context);
            else
                context.ldnull();

            context.stloc(parent_scope.returnTemp);

            if (context.labels != null && context.labels.Next != null)
                context.Goto(context.labels.Next);
            else
                context.Goto(context.labels.Return);
        }
    }



    internal class REDO : ControlFlowNode
    {
        // redo;

        internal REDO(Scope current, YYLTYPE location): base(current, location)
        {
        }

        internal override void GenCode0(CodeGenContext context)
        {
            context.newLine(location);

            if (context.labels != null && context.labels.Redo != null)
                context.Goto(context.labels.Redo);
            else
                throw new System.Exception("unexpected REDO not in loop or block");
        }
    }



    internal class RETRY : ControlFlowNode
    {
        // retry

        internal RETRY(Scope current, YYLTYPE location): base(current, location)
        {
        }

        internal override void GenCode0(CodeGenContext context)
        {
            context.newLine(location);

            if (context.labels != null && context.labels.Retry != null)
                context.Goto(context.labels.Retry);                
            else
            {
                System.Diagnostics.Debug.Assert(this.parent_scope is BLOCK);
                // throw new Ruby.RetryException(block.defining_scope);
                context.ldarg(0);  // current Ruby.MethodBody
                context.ldfld(Runtime.Block.defining_scope);
                context.newobj(Runtime.RetryException.ctor);
                context.throwOp();
            }
        }
    }
}
