/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/


using System.Collections.Generic;
using System.Text;


namespace Ruby.Compiler.AST
{

    internal class TRY_BLOCK : Node
    {
        //        body
        //    rescue 
        //        rescue
        //    else
        //        else
        //    ensure
        //        ensure

        private Node body;                // optional
        private RESCUE_CLAUSE rescue;    // optional
        private Node _else;                // optional
        private Node ensure;            // optional
        private Scope parent_scope;

        internal TRY_BLOCK(Scope parent_scope, Node body, Node rescue, Node _else, Node ensure, YYLTYPE location)
            : base(location)
        {
            this.parent_scope = parent_scope;
            this.body = body;
            this.rescue = (RESCUE_CLAUSE)rescue;
            this._else = _else;
            this.ensure = ensure;

        }


        internal override void GenCode0(CodeGenContext context)
        {
            PERWAPI.CILLabel finalLabel = context.NewLabel();

            int RescueTemp = context.CreateLocal("rescueTemp", PERWAPI.PrimitiveType.Object);
            context.ldnull();
            context.stloc(RescueTemp);

            if (ensure != null)
            {
                context.StartBlock(Clause.Try); // outer try block with finally

                context.StartBlock(Clause.Try); // inner try block with catch
            }

            GenInnerBlock(context, RescueTemp);

            if (ensure != null)
            {
                context.Goto(finalLabel);

                PERWAPI.TryBlock innerTry = context.EndTryBlock();

                context.StartBlock(Clause.Catch);
                GenRescue(context, null, 0, null);
                context.EndCatchBlock(Runtime.SystemExceptionRef, innerTry);

                PERWAPI.TryBlock outerTry = context.EndTryBlock();
                
                // Fixme: reset labels to prevent branches out of finally block.    
                context.StartBlock(Clause.Finally);
                ensure.GenCode(context);
                if (context.Reachable())
                    context.pop();
                context.endfinally();
                context.EndFinallyBlock(outerTry);

                context.CodeLabel(finalLabel);
                context.newEndPoint(location);
            }

            context.ldloc(RescueTemp);

            context.ReleaseLocal(RescueTemp, true);
        }


        private void GenInnerBlock(CodeGenContext context, int RescueTemp)
        {
            PERWAPI.CILLabel elseLabel = context.NewLabel();

            Labels catchLabels = new Labels();
            catchLabels.Break = context.labels.Break;
            catchLabels.Next = context.labels.Next;
            catchLabels.Redo = context.labels.Redo;
            catchLabels.Return = context.labels.Return;
            catchLabels.Retry = context.NewLabel();

            context.CodeLabel(catchLabels.Retry);

            context.StartBlock(Clause.Try);
            {
                if (body != null)
                    body.GenCode(context);
                else
                    context.ldnull();

                if (context.Reachable())
                {
                    context.stloc(RescueTemp);
                    context.Goto(elseLabel);
                }
            }
            PERWAPI.TryBlock innerTry = context.EndTryBlock();

            PERWAPI.CILLabel endLabel = context.NewLabel();

            if (rescue != null)
            {
                context.StartBlock(Clause.Catch);
                {
                    Labels original = context.labels;
                    context.labels = catchLabels;

                    GenRescue(context, endLabel, RescueTemp, rescue);
                    
                    context.labels = original;
                }
                context.EndCatchBlock(Runtime.SystemExceptionRef, innerTry);
            }

            context.CodeLabel(elseLabel);
            {
                if (_else != null)
                {
                    _else.GenCode(context);
                    if (context.Reachable())
                        context.stloc(RescueTemp);
                }
            }
            context.CodeLabel(endLabel);
        }


        internal void GenRescue(CodeGenContext context, PERWAPI.CILLabel endLabel, int RescueTemp, RESCUE_CLAUSE clauses)
        {
            // catch (System.Exception e) {

            int e = context.StoreInTemp("e", Runtime.SystemExceptionRef, location);

            //if (e is Ruby.ControlException)
            PERWAPI.CILLabel else1 = context.NewLabel();
            context.ldloc(e);
            context.isinst(Runtime.ControlExceptionRef);
            context.brfalse(else1);
            //    throw e;
            context.rethrow();
            context.CodeLabel(else1);

            // Ruby.Exception exception;
            int exception = context.CreateLocal("exception", Runtime.ExceptionRef);

            //if (!(e is Ruby.RubyException))
            PERWAPI.CILLabel else2 = context.NewLabel();
            PERWAPI.CILLabel end = context.NewLabel();
            context.ldloc(e);
            context.isinst(Runtime.RubyExceptionRef);
            context.brtrue(else2);
            //    exception = new Ruby.CLRException(frame, e);
            context.ldloc(0);
            context.ldloc(e);
            context.newobj(Runtime.CLRException.ctor);
            context.stloc(exception);
            context.br(end);

            //else
            context.CodeLabel(else2);
            //     exception = (Ruby.RubyException)e.parent;
            context.ldloc(e);
            context.cast(Runtime.RubyExceptionRef);
            context.ldfld(Runtime.RubyException.parent);
            context.stloc(exception);

            context.CodeLabel(end);

            //Eval.ruby_errinfo.value = exception;
            context.ldsfld(Runtime.Eval.ruby_errinfo);
            context.ldloc(exception);
            context.stfld(Runtime.global_variable.value);

            if (clauses != null)
                clauses.GenCode(context, endLabel, RescueTemp, exception);

            context.rethrow();

            context.ReleaseLocal(e, true);
            context.ReleaseLocal(exception, true);
        }
    }



    internal class RESCUE_CLAUSE : Node
    {
        //    rescue types => var 
        //        body
        //    next

        private Node types;            // optional
        private LVALUE var;            // optional
        private Node body;          // optional
        private RESCUE_CLAUSE next; // optional
        private Scope parent_scope;


        internal RESCUE_CLAUSE(Scope parent_scope, Node body, YYLTYPE location)
            : base(location)
        {
            this.parent_scope = parent_scope;
            this.body = body;
        }


        internal RESCUE_CLAUSE(Scope parent_scope, Node types, LVALUE var, Node body, Node next, YYLTYPE location)
            : base(location)
        {
            if (types == null)
                types = new CONST(parent_scope, ID.intern("StandardError"), location);

            this.parent_scope = parent_scope;
            this.types = types;
            this.var = var;
            this.body = body;
            this.next = (RESCUE_CLAUSE)next;
        }

        internal void GenCode(CodeGenContext context, PERWAPI.CILLabel endLabel, int RescueTemp, int exception)
        {
            for (RESCUE_CLAUSE clause = this; clause != null; clause = clause.next)
            {
                PERWAPI.CILLabel nextClause = context.NewLabel();
                PERWAPI.CILLabel thisClause = context.NewLabel();
 
                for (Node type = clause.types; type != null; type = type.nd_next)
                {
                    // Precompute each separately to avoid computing a list of types
                    type.GenCode0(context);
                    LOCAL tt = context.StoreInLocal("type", PERWAPI.PrimitiveType.Object, type.location);

                    new METHOD_CALL(tt, ID.intern(Tokens.tEQQ), new AST.LOCAL(exception, type.location), type.location).GenCode(context);

                    context.ReleaseLocal(tt.local, true);

                    context.call(Runtime.Eval.Test);
                    context.brtrue(thisClause);
                }

                context.br(nextClause);

                context.CodeLabel(thisClause);

                if (clause.var != null)
                {
                    clause.var.Assign(context, new AST.LOCAL(exception, clause.var.location));
                    context.pop();
                }

                if (clause.body != null)
                    clause.body.GenCode(context);
                else
                    context.ldnull();

                if (context.Reachable())
                    context.stloc(RescueTemp);

                // reset $!
                //Eval.ruby_errinfo.value = null;
                context.ldsfld(Runtime.Eval.ruby_errinfo);
                context.ldnull();
                context.stfld(Runtime.global_variable.value);

                context.Goto(endLabel);

                context.CodeLabel(nextClause);
            }
        }
    }



    internal class RESCUE_EXPR : Node
    {
        //    expr rescue rescue 

        private Node expr;
        private Node rescue;

        internal RESCUE_EXPR(Node expr, Node rescue, YYLTYPE location)
            : base(location)
        {
            this.expr = expr;
            this.rescue = rescue;
        }


        internal override void GenCode0(CodeGenContext context)
        {
            int RescueTemp = context.CreateLocal("rescueTemp", PERWAPI.PrimitiveType.Object);

            PERWAPI.CILLabel endLabel = context.NewLabel();

            context.StartBlock(Clause.Try);
            {
                expr.GenCode(context);
                if (context.Reachable())
                    context.stloc(RescueTemp);
                context.Goto(endLabel);
            }
            PERWAPI.TryBlock tryBlock = context.EndTryBlock();

            context.StartBlock(Clause.Catch);
            {
                context.pop();
                rescue.GenCode(context);
                if (context.Reachable())
                    context.stloc(RescueTemp);
                context.Goto(endLabel);
            }
            context.EndCatchBlock(Runtime.RubyExceptionRef, tryBlock);

            context.CodeLabel(endLabel);

            context.ldloc(RescueTemp);

            context.ReleaseLocal(RescueTemp, true);
        }
    }
}
