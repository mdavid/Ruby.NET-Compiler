/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/

using System.CodeDom;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using PERWAPI;


namespace Ruby.Compiler.AST
{

    internal class ASSIGNMENT : Node
    {
        // lhs = rhs;

        internal LVALUE lhs;
        internal Node rhs;

        internal ASSIGNMENT(LVALUE lhs, Node rhs, YYLTYPE location)
            : base(location)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

          internal override void Defined(CodeGenContext context)
        {
            context.PushTrue();
            context.box(PrimitiveType.Boolean);
        }

        internal override string DefinedName()
        {
            return "assignment";
        }


        internal override void GenCode0(CodeGenContext context)
        {
            context.newLine(location);
            lhs.Assign(context, rhs);
        }

        public CodeStatement ToCodeStatement()
        {
            return new CodeAssignStatement(lhs.ToCodeExpression(), rhs.ToCodeExpression());
        }
    }



    internal class MLHS : LVALUE        // Grouping of args used within multiple assignment
    {
        // ( elements )

        internal LVALUE elements;    // optional

        internal MLHS(LVALUE elements, YYLTYPE location): base(location)
        {
            this.elements = elements;
        }

        internal MLHS append(Node next)
        {
            Parser.append(elements, next);
            return this;
        }

        internal override void Assign(CodeGenContext context, Node rhs)
        {
            // Gen right hand sides
            ListGen mrhs;
            if (rhs is ListGen && !(rhs is MultipleRHS))
                mrhs = (ListGen)rhs;
            else
                mrhs = new ARGS(null, null, rhs, null, location, true);

            bool created;
            ISimple list = mrhs.GenArgList(context, out created);
            list.GenSimple(context);
            context.callvirt(Runtime.ArgList.CheckSingleRHS);
            int array = context.StoreInTemp("mrhs", Runtime.ArgListRef, location);

            context.ReleaseLocal(list, created);

            // Gen assignments to left hand sides
            for (LVALUE l = elements; l != null; l = (LVALUE)l.nd_next)
            {
                l.Assign(context, new MultipleRHS(array, l.location));
                context.pop();
            }

            context.ldloc(array);
            context.callvirt(Runtime.ArgList.ToRubyArray);

            context.ReleaseLocal(array, true);
        }

    }



    internal class MultipleRHS : ListGen
    {
        internal int arglist;

        internal MultipleRHS(int arglist, YYLTYPE location): base(location)
        {
            this.arglist = arglist;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            // arglist.GetNext(array);
            context.ldloc(arglist);
            context.callvirt(Runtime.ArgList.GetNext);
        }

        internal override ISimple GenArgList(CodeGenContext context, out bool created)
        {
            created = false;
            return new LOCAL(arglist, location);
        }
    }



    internal class LHS_STAR : LVALUE
    {
        // *arrayParam

        internal LVALUE arrayParam;        // optional
        private Node rhs;

        internal LHS_STAR(LVALUE arrayParam, YYLTYPE location): base(location)
        {
            if (arrayParam == null)
                this.arrayParam = new DummyParam(location);
            else
                this.arrayParam = arrayParam;
        }


        internal override void Assign(CodeGenContext context, Node rhs)
        {
            this.rhs = rhs;

            if (rhs is ListGen)
                arrayParam.Assign(context, new ProxyNode(GetRestRHS, location));
            else
                arrayParam.Assign(context, new ProxyNode(RHSAsArray, location));
        }

        private void GetRestRHS(CodeGenContext context)
        {
            // rhs.GetRest();
            bool created;
            ISimple list = ((ListGen)rhs).GenArgList(context, out created);
            list.GenSimple(context);
            context.callvirt(Runtime.ArgList.GetRest);

            context.ReleaseLocal(list, created);
        }

        private void RHSAsArray(CodeGenContext context)
        {
            // Array.Store(rhs);
            rhs.GenCode(context);
            context.call(Runtime.Array.Store);
        }
    }



    internal class OP_ASGN : ASSIGNMENT
    {
        // lhs op= rhs

        protected string op;

        internal OP_ASGN(LVALUE lhs, string op, Node rhs, YYLTYPE location)
            : base(lhs, rhs, location)
        {
            this.op = op;
        }


        internal override void GenCode0(CodeGenContext context)
        {
            context.newLine(location);
            if (lhs is ARRAY_ACCESS) // for array access -> need to avoid recomputation of lhs index
                ((ARRAY_ACCESS)lhs).AssignOp(context, op, rhs);
            else if (lhs is CVAR && op == "||")
            {
                CILLabel alreadyDefined1 = new CILLabel();
                CILLabel alreadyDefined2 = new CILLabel();
                CILLabel alreadyDefined3 = new CILLabel();
                Node lhsDefined = new DEFINED(lhs, location);
                lhsDefined.GenCode(context);
                context.brtrue(alreadyDefined1);
                lhs.Assign(context, rhs);
                context.br(alreadyDefined2);
                context.CodeLabel(alreadyDefined1);
                lhs.GenCode(context);
                context.CodeLabel(alreadyDefined2);
            }
            else
                lhs.Assign(context, METHOD_CALL.Create(lhs, op, rhs, location));
        }
    }



    internal class OP_ASGN2 : OP_ASGN
    {
        // lhs.id op= rhs

        private Node recv;
        private string vid;

        internal OP_ASGN2(Node recv, string vid, string op, Node rhs, YYLTYPE location)
            : base(null, op, rhs, location)
        {
            this.recv = recv;
            this.vid = vid;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            context.newLine(location);
            recv.GenCode(context);
            LOCAL recvLocal = context.StoreInLocal("recv", PrimitiveType.Object, recv.location);

            // lhs.vid=(lhs.vid() op rhs)
            new METHOD_CALL(recvLocal, vid + "=", new METHOD_CALL(new METHOD_CALL(recvLocal, vid, new ARGS(null, null, null, null, recv.location, true), null, recv.location), op, rhs, location), location).GenCode(context);

            context.ReleaseLocal(recvLocal.local, true);
        }
    }
}