/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/

using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using PERWAPI;


namespace Ruby.Compiler.AST
{

    internal class NOT : Node    // Logical Not operator
    {
        // !   cond
        // not cond

        private Node cond;

        internal NOT(Node cond, YYLTYPE location)
            : base(location)
        {
            Debug.Assert(cond != null);
            this.cond = cond;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            new COND(cond, new FALSE(location), new TRUE(location), location).GenCode(context);
        }
    }



    internal class AND : Node    // Logical And operator
    {
        // lhs and rhs
        // lhs &&  rhs

        private Node lhs;
        private Node rhs;

        internal AND(Node lhs, Node rhs, YYLTYPE location)
            : base(location)
        {
            Debug.Assert(lhs != null && rhs != null);
            this.lhs = lhs;
            this.rhs = rhs;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            bool created;
            ISimple left = context.PreCompute(lhs, "lhs", out created);

            new COND((Node)left, rhs, (Node)left, location).GenCode(context);

            context.ReleaseLocal(left, created);
        }
    }



    internal class OR : Node    // Logical Or operator
    {
        // lhs or rhs
        // lhs || rhs

        private Node lhs;
        private Node rhs;

        internal OR(Node lhs, Node rhs, YYLTYPE location)
            : base(location)
        {
            Debug.Assert(lhs != null && rhs != null);
            this.lhs = lhs;
            this.rhs = rhs;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            bool created;
            ISimple left = context.PreCompute(lhs, "lhs", out created);

            new COND((Node)left, (Node)left, rhs, location).GenCode(context);

            context.ReleaseLocal(left, created);
        }
    }



    internal class MATCH : Node
    {
        // str =~ pattern

        private Node str;
        private Node pattern;

        internal MATCH(Node str, Node pattern, YYLTYPE location)
            : base(location)
        {
            Debug.Assert(str != null && pattern != null);
            this.str = str;
            this.pattern = pattern;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            new METHOD_CALL(pattern, ID.intern(Tokens.tMATCH), str, location).GenCode(context);
        }

          internal override void Defined(CodeGenContext context)
        {
            context.PushTrue();
            context.box(PrimitiveType.Boolean);
        }

        internal override string DefinedName()
        {
            return "method";
        }
    }



    internal class DOT2 : Node
    {
        // beg .. end

        protected Node beg;
        protected Node end;

        internal DOT2(Node beg, Node end, YYLTYPE location)
            : base(location)
        {
            Debug.Assert(beg != null && end != null);
            this.beg = beg;
            this.end = end;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            bool start_created, finish_created;
            ISimple start = context.PreCompute(beg, "beg", out start_created);
            ISimple finish = context.PreCompute(end, "end", out finish_created);

            start.GenSimple(context);
            finish.GenSimple(context);
            context.PushFalse();
            context.newobj(Runtime.Range.ctor);

            context.ReleaseLocal(start, start_created);
            context.ReleaseLocal(finish, finish_created);
        }
    }



    internal class DOT3 : DOT2
    {
        // beg ... end

        internal DOT3(Node beg, Node end, YYLTYPE location)
            : base(beg, end, location)
        {
        }

        internal override void GenCode0(CodeGenContext context)
        {
            bool start_created, finish_created;
            ISimple start = context.PreCompute(beg, "beg", out start_created);
            ISimple finish = context.PreCompute(end, "end", out finish_created);

            start.GenSimple(context);
            finish.GenSimple(context);
            context.PushTrue();
            context.newobj(Runtime.Range.ctor);

            context.ReleaseLocal(start, start_created);
            context.ReleaseLocal(finish, finish_created);
        }
    }



    internal class DEFINED : Node
    {
        //    defined?(arg)

        private Node arg;

        internal DEFINED(Node arg, YYLTYPE location)
            : base(location)
        {
            Debug.Assert(arg != null);
            this.arg = arg;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            arg.GenDefined(context);
        }
    }



    internal class UNDEF : Node
    {
        // undef mid

        private string mid;
        private Scope parent_scope;

        internal UNDEF(Scope parent_scope, string mid, YYLTYPE location)
            : base(location)
        {
            this.mid = mid;
            this.parent_scope = parent_scope;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            // ruby_class.undef_method(mid);
            context.newLine(location);
            context.ruby_class(parent_scope);
            context.ldstr(mid.ToString());
            context.callvirt(Runtime.Class.undef_method);
            context.ldnull();
        }
    }



    internal class ALIAS : Node
    {
        // alias lhs rhs

        private string lhs;
        private string rhs;
        private Scope parent_scope;

        internal ALIAS(Scope parent_scope, string lhs, string rhs, YYLTYPE location)
            : base(location)
        {
            this.lhs = lhs;
            this.rhs = rhs;
            this.parent_scope = parent_scope;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            // Eval.alias(ruby_class, lhs, rhs, myFrame);
            context.newLine(location);
            context.ruby_class(parent_scope);
            context.ldstr(lhs.ToString());
            context.ldstr(rhs.ToString());
            context.ldloc(0);
            context.call(Runtime.Eval.alias);
        }
    }



    internal class VALIAS : Node
    {
        // alias lhs rhs

        private string lhs;
        private string rhs;

        internal VALIAS(string lhs, string rhs, YYLTYPE location)
            : base(location)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            context.newLine(location);
            context.ldstr(lhs.ToString());
            context.ldstr(rhs.ToString());
            context.call(Runtime.Variables.alias_variable);
        }
    }
}