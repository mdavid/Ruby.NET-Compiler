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


    internal class CONCAT : Node    // Double Quoted String
    {
        private Node head;

        internal CONCAT(Node A, Node B, YYLTYPE location)
            : base(location)
        {
            if (A == null || A is VALUE && ((string)((VALUE)A).value) == "")
            {
                this.head = B;
                return;
            }

            Node headA, headB;
            if (A is CONCAT)
                headA = ((CONCAT)A).head;
            else
                headA = A;

            if (B is CONCAT)
                headB = ((CONCAT)B).head;
            else
                headB = B;

            this.head = Parser.append(headA, headB);
        }

        internal override void GenCode0(CodeGenContext context)
        {
            // String.Concat(String.Concat(arg1, arg2), args, ...);

            head.GenCode0(context);

            if (head.nd_next != null)
            {
                int first = context.StoreInTemp("head", Runtime.StringRef, head.location);

                for (Node n = head.nd_next; n != null; n = n.nd_next)
                {
                    n.GenCode0(context);
                    int second = context.StoreInTemp("tail", Runtime.StringRef, n.location);

                    context.ldloc(first);
                    context.ldloc(second);
                    context.callvirt(Runtime.String.Concat);
                    context.stloc(first);

                    context.ReleaseLocal(second, true);
                }

                context.ldloc(first);

                context.ReleaseLocal(first, true);
            }
        }
    }



    internal class XSTRING : Node
    {
        private Node contents;

        internal XSTRING(Node contents, YYLTYPE location)
            : base(location)
        {
            this.contents = contents;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            new METHOD_CALL("`", contents, location).GenCode(context);
        }
    }



    internal class EVAL_CODE : Node    // Expression to be evaluated in string literal
    {
        // "...#{code}..."

        private Node code;        // optional

        internal EVAL_CODE(Node code, YYLTYPE location)
            : base(location)
        {
            this.code = code;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            // String.ObjectToString(code, caller);

            if (code != null)
                code.GenCode(context);
            else
                context.ldnull();

            context.ldloc(0);

            context.call(Runtime.String.ObjectAsString);
        }
    }



    internal class SYMBOL : Node
    {
        // :id

        public object id;

        internal SYMBOL(Node id, YYLTYPE location)
            : base(location)
        {
            this.id = id;
        }

        internal SYMBOL(string id, YYLTYPE location)
            : base(location)
        {
            this.id = id;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            if (id is string)
                context.ldstr((string)id);
            else if (id is VALUE && ((VALUE)id).value is string)
                context.ldstr((string)((VALUE)id).value);
            else
            {
                ((Node)id).GenCode(context);
                context.ldfld(Runtime.String.value);
            }
                
            context.newobj(Runtime.Symbol.ctor);
        }
    }
}