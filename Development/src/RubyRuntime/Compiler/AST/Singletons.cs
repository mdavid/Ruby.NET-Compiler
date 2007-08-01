/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/


using System;
using System.Collections.Generic;
using System.Text;
using PERWAPI;


namespace Ruby.Compiler.AST
{

    internal class SELF : Node, ISimple
    {
        // self
        internal SELF(YYLTYPE location)
            : base(location)
        {
        }

        public void GenSimple(CodeGenContext context)
        {
            context.ldarg("recv");
        }

          internal override void Defined(CodeGenContext context)
        {
            context.PushTrue();
            context.box(PrimitiveType.Boolean);
        }

        internal override string DefinedName()
        {
            return "self";
        }
    }




    internal class NIL : Node, ISimple
    {
        // nil
        internal NIL(YYLTYPE location)
            : base(location)
        {
        }

        public void GenSimple(CodeGenContext context)
        {
            context.ldnull();
        }

          internal override void Defined(CodeGenContext context)
        {
            context.PushTrue();
            context.box(PrimitiveType.Boolean);
        }

        internal override string DefinedName()
        {
            return "nil";
        }
    }




    internal class TRUE : Node, ISimple
    {
        // true
        internal TRUE(YYLTYPE location)
            : base(location)
        {
        }

        public void GenSimple(CodeGenContext context)
        {
            context.PushTrue();
            context.box(PrimitiveType.Boolean);
        }

          internal override void Defined(CodeGenContext context)
        {
            context.PushTrue();
            context.box(PrimitiveType.Boolean);
        }

        internal override string DefinedName()
        {
            return "true";
        }
    }




    internal class FALSE : Node, ISimple
    {
        // false
        internal FALSE(YYLTYPE location)
            : base(location)
        {
        }

        public void GenSimple(CodeGenContext context)
        {
            context.PushFalse();
            context.box(PrimitiveType.Boolean);
        }

          internal override void Defined(CodeGenContext context)
        {
            context.PushTrue();
            context.box(PrimitiveType.Boolean);
        }

        internal override string DefinedName()
        {
            return "false";
        }
    }
}