/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/

using System.CodeDom;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


namespace Ruby.Compiler.AST
{

    public abstract class Node
    {
        internal YYLTYPE location;
        internal Node nd_next;


        internal Node(YYLTYPE location)
        {
            this.location = location;
            this.nd_next = null;
        }


        private static void NameAssembly(string inputFileName, out string outputFileName, out string assemblyName)
        {
            if (inputFileName.EndsWith(".rb"))
                outputFileName = inputFileName.Substring(0, inputFileName.Length - 3);
            else
                outputFileName = inputFileName;

            if (outputFileName.Contains("\\"))
                assemblyName = outputFileName.Substring(outputFileName.LastIndexOf('\\') + 1);
            else
                assemblyName = outputFileName;

            outputFileName += ".exe";
        }


        internal void GenCode(CodeGenContext context)
        {
            for (Node stmt = this; stmt != null; stmt = stmt.nd_next)
            {
                stmt.GenCode0(context);
                if (stmt.nd_next != null && context.Reachable())
                    context.pop();
            }
        }


        internal virtual void GenCode0(CodeGenContext context)
        {
            if (this is ISimple)
                ((ISimple)this).GenSimple(context);
            else
                throw new NotImplementedException(GetType().ToString());
        }


        public virtual CodeExpression ToCodeExpression()
        {
            throw new System.NotImplementedException("Ruby Code DOM for " + GetType().ToString());
        }


        internal virtual string DefinedName()
        {
            return "expression";
        }

        internal virtual void Defined(CodeGenContext context)
        {
            GenCode0(context);
        }


        internal void GenDefined(CodeGenContext context)
        {
            new COND(new ProxyNode(Defined, location), new VALUE(DefinedName(), location), new NIL(location), location).GenCode(context);
        }

        internal void LoadBlock0(CodeGenContext context)
        {
            if (context.HasArg(Runtime.ArgListRef))
            {
                context.ldarg("args"); // args.block
                context.ldfld(Runtime.ArgList.block);
            }
            else if (context.HasArg(Runtime.ProcRef))
            {
                context.ldarg("block");
            }
            else
                context.ldnull();
        }

        internal void LoadBlock(CodeGenContext context)
        {
            context.ldloc(0);
            context.ldfld(Runtime.Frame.block_arg);
        }


        internal void SetLine(CodeGenContext context)
        {
            if (location != null)
            {
                // frame.line = thisline
                context.ldloc(0);
                context.ldc_i4(this.location.first_line);
                context.stfld(Runtime.Frame.line);
            }
        }
    }


    internal interface ISimple
    {
        void GenSimple(CodeGenContext context);
    }


    internal delegate void GenCodeDelegate(CodeGenContext context);



    internal class ProxyNode : Node
    {
        private GenCodeDelegate proxy;

        internal ProxyNode(GenCodeDelegate proxy, YYLTYPE location)
            : base(location)
        {
            this.proxy = proxy;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            proxy(context);
        }
    }
}



