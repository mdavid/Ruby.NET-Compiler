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

    internal class COND : Node
    {
        //    if cond
        //        body
        //    else
        //        else 
        //    end

        protected Node cond;
        protected Node body;        // optional
        protected Node _else;        // optional

        internal COND(Node cond, Node body, Node _else, YYLTYPE location)
            : base(location)
        {
            Debug.Assert(cond != null);
            this.cond = cond;
            this.body = body;
            this._else = _else;
        }


        internal override void GenCode0(CodeGenContext context)
        {
            PERWAPI.CILLabel elseLabel = context.NewLabel();
            PERWAPI.CILLabel endLabel = context.NewLabel();

            // if (Eval.Test(cond))
            cond.GenCode(context);
            context.call(Runtime.Eval.Test);
            context.brfalse(elseLabel);
            body.GenCode(context);
            context.br(endLabel);
            context.CodeLabel(elseLabel);
            _else.GenCode(context);
            context.CodeLabel(endLabel);
        }
    }




    internal class IF : COND
    {
        internal IF(Node cond, Node body, Node _else, YYLTYPE location): base(cond, body, _else, location)
        {
        }

        internal override void GenCode0(CodeGenContext context)
        {
            PERWAPI.CILLabel elseLabel = context.NewLabel();
            PERWAPI.CILLabel endLabel = context.NewLabel();

            // if (Eval.Test(cond))
            context.newLine(cond.location);
            cond.GenCode(context);
            context.call(Runtime.Eval.Test);
            context.brfalse(elseLabel);

            if (body != null)
            {
                context.newStartPoint(body.location);
                body.GenCode(context);
            }
            else
                context.ldnull();

            if (context.Reachable())
                context.br(endLabel);

            context.CodeLabel(elseLabel);


            if (_else != null)
            {
                context.newStartPoint(_else.location);
                _else.GenCode(context);
            }
            else
                context.ldnull();

            context.CodeLabel(endLabel);
            context.newEndPoint(location);
        }
    }



    internal class PreTestLoop : Node
    {
        //    while cond
        //        body
        //    end

        private Node cond;
        private Node body;        // optional
        private Scope parent_scope;
        private bool whileTrue;

        internal PreTestLoop(Scope parent_scope, Node cond, bool whileTrue, Node body, YYLTYPE location)
            : base(location)
        {
            this.parent_scope = parent_scope;
            this.cond = cond;
            this.whileTrue = whileTrue;
            this.body = body;
            Debug.Assert(parent_scope != null);

        }

        internal PreTestLoop(Node cond, Node body, Scope scope, bool whileTrue, YYLTYPE location)
            : base(location)
        {
            this.cond = cond;
            this.whileTrue = whileTrue;
            this.body = body;
            this.parent_scope = scope;
            Debug.Assert(scope != null);
        }

        internal override void GenCode0(CodeGenContext context)
        {
            Labels original = context.labels;

            // ---------------- Create new label context for loop ----------------------
            context.labels = new Labels();
            context.labels.Next = context.NewLabel();
            context.labels.Break = context.NewLabel();
            context.labels.Redo = context.NewLabel();
            context.labels.Retry = context.NewLabel();
            context.labels.Return = original.Return;

            context.CodeLabel(context.labels.Retry);

            context.newLine(cond.location);

            context.ldnull();
            context.stloc(parent_scope.returnTemp);

            // if (Eval.Test(cond))
            cond.GenCode(context);
            context.call(Runtime.Eval.Test);
            if (whileTrue)
                context.brfalse(context.labels.Break);
            else
                context.brtrue(context.labels.Break);

            context.CodeLabel(context.labels.Redo);

            if (body != null)
            {
                body.GenCode(context);
                context.pop();
            }

            context.CodeLabel(context.labels.Next);

            context.br(context.labels.Retry);

            context.CodeLabel(context.labels.Break);

            context.newEndPoint(location);

            context.ldloc(parent_scope.returnTemp);

            // --------------------- Restore Label context -------------------------
            context.labels = original;
        }
    }



    internal class PostTestLoop : Node
    {
        //    until cond
        //        body
        //    end

        private Node cond;
        private Node body;        // optional
        private Scope parent_scope;
        private bool whileTrue;

        internal PostTestLoop(Scope parent_scope, Node cond, bool whileTrue, Node body, YYLTYPE location)
            : base(location)
        {
            this.parent_scope = parent_scope;
            this.cond = cond;
            this.whileTrue = whileTrue;
            this.body = body;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            Labels original = context.labels;

            // ---------------- Create new label context for loop ----------------------
            context.labels = new Labels();
            context.labels.Next = context.NewLabel();
            context.labels.Break = context.NewLabel();
            context.labels.Redo = context.NewLabel();
            context.labels.Retry = context.NewLabel();
            context.labels.Return = original.Return;

            context.CodeLabel(context.labels.Retry);

            context.newStartPoint(location);

            context.ldnull();
            context.stloc(parent_scope.returnTemp);

            context.CodeLabel(context.labels.Redo);

            if (body != null)
            {
                body.GenCode(context);
                context.pop();
            }

            context.CodeLabel(context.labels.Next);

            // if (Eval.Test(cond))
            context.newLine(cond.location);
            cond.GenCode(context);
            context.call(Runtime.Eval.Test);
            if (whileTrue)
                context.brtrue(context.labels.Retry);
            else
                context.brfalse(context.labels.Retry);

            context.CodeLabel(context.labels.Break);

            context.newEndPoint(location);

            context.ldloc(parent_scope.returnTemp);

            // --------------------- Restore Label context -------------------------
            context.labels = original;
        }
    }



    internal class GetWhile : Node
    {
        private Node body;
        private Scope parent_scope;

        internal GetWhile(Node body, Scope parent_scope, YYLTYPE location)
            : base(location)
        {
            this.body = body;
            this.parent_scope = parent_scope;
        }

        internal override void GenCode0(CodeGenContext context)
        {
              new PreTestLoop(new ProxyNode(call_rb_gets, location), body, parent_scope, true, location).GenCode0(context);
        }

        private void call_rb_gets(CodeGenContext context)
        {
            context.ldloc(0);
            context.call(Runtime.IO.rb_gets);
        }
    }



    internal class FOR : Node
    {
        //    for var in list; 
        //        body
        //    end

        private Node list;
        private Node body;        // optional

        internal FOR(Node list, Node body, YYLTYPE location)
            : base(location)
        {
            this.list = list;
            this.body = body;
        }

        internal override void GenCode0(CodeGenContext context)
        {        
            new METHOD_CALL(list, ID.intern("each"), new ARGS(location), body, location).GenCode(context);
        }
    }



    internal class CASE : Node
    {
        //    case target 
        //        body
        //    end

        private Node target;    // optional
        private Node body;        // optional

        internal CASE(Node target, Node body, YYLTYPE location)
            : base(location)
        {
            this.target = target;
            this.body = body;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            PERWAPI.CILLabel endLabel = context.NewLabel();
            Node clause;

            if (target != null)
            {
                context.newLine(target.location);
                target.GenCode(context);
                LOCAL t = context.StoreInLocal("target", PrimitiveType.Object, location);
                
                for (clause = body; clause != null && clause is WHEN; clause = clause.nd_next)
                    ((WHEN)clause).GenCode(context, t, endLabel);

                context.ReleaseLocal(t.local, true);
            }
            else
            {
                for (clause = body; clause != null && clause is WHEN; clause = clause.nd_next)
                {
                    context.newLine(clause.location);
                    ((WHEN)clause).GenCode(context, endLabel);
                }
            }

            if (clause != null) /* assume else clause */
                clause.GenCode(context);
            else
                context.ldnull();

            context.CodeLabel(endLabel);
            context.newEndPoint(location);
        }
    }



    internal class WHEN : Node    // When clause within Case 
    {
        //    when comparison
        //        body
        //    next

        private ARGS comparison;
        private Node body;    // optional

        internal WHEN(Node comparison, Node body, Node next, YYLTYPE location)
            : base(location)
        {
            Debug.Assert(comparison != null);
            this.comparison = (ARGS)comparison;
            this.body = body;
            this.nd_next = next;
        }

        internal void GenCode(CodeGenContext context, LOCAL target, PERWAPI.CILLabel endLabel)
        {
            PERWAPI.CILLabel elseLabel = context.NewLabel();

            context.newLine(comparison.location);
            // if (comparison.ToRubyArray().includes(target, caller))
            bool created;
            ISimple list = comparison.GenArgList(context, out created);
            list.GenSimple(context);
            context.ReleaseLocal(list, created);
            context.callvirt(Runtime.ArgList.ToRubyArray);
            target.GenCode(context);
            context.ldloc(0);
            context.callvirt(Runtime.Array.includes);
            context.brfalse(elseLabel);

            if (body != null)
            {
                context.newStartPoint(body.location);
                body.GenCode(context);
            }
            else
                context.ldnull();
                
            context.br(endLabel);

            context.CodeLabel(elseLabel);
        }


        internal void GenCode(CodeGenContext context, PERWAPI.CILLabel endLabel)
        {
            PERWAPI.CILLabel elseLabel = context.NewLabel();

            context.newLine(comparison.location);
            // if (comparision.ToRubyArray().includes(true, caller))
            bool created;
            ISimple list = comparison.GenArgList(context, out created);
            list.GenSimple(context);
            context.ReleaseLocal(list, created);
            context.callvirt(Runtime.ArgList.ToRubyArray);
            new TRUE(comparison.location).GenCode(context);
            context.ldloc(0);
            context.callvirt(Runtime.Array.includes);
            context.brfalse(elseLabel);

            if (body != null)
            {
                context.newStartPoint(body.location);
                body.GenCode(context);
            }
            else
                context.ldnull();

            context.br(endLabel);
            context.CodeLabel(elseLabel);
        }
    }



    internal class LOCAL : Node, ISimple   // For Internal Code Generation Use Only.
    {
        internal int local;

        internal LOCAL(int local, YYLTYPE location)
            : base(location)
        {
            this.local = local;
        }

        public void GenSimple(CodeGenContext context)
        {
            context.ldloc(local);
        }
    }
}