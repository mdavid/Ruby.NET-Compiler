/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/


using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Diagnostics;
using PERWAPI;


namespace Ruby.Compiler.AST
{
    
    internal abstract class ListGen: Node
    {
        internal ListGen(YYLTYPE location)
            : base(location)
        {
        }

        internal abstract ISimple GenArgList(CodeGenContext context, out bool created);

        internal virtual bool ShortAndSimple()
        {
            return false;
        }

        internal virtual List<ISimple> GenFixedArgs(CodeGenContext context, out List<bool> created)
        {
            throw new System.Exception("Not supported");
        }

        internal override void GenCode0(CodeGenContext context)
        {
            bool created;
            ISimple list = GenArgList(context, out created);
            list.GenSimple(context);
            context.callvirt(Runtime.ArgList.ToRubyObject);

            context.ReleaseLocal(list, created);
        }
    }



    internal class ARGS : ListGen
    {
        internal Node parameters;
        internal Node hashlist;
        internal Node array;
        internal Node block;

        internal ARGS(YYLTYPE location): base(location)
        {
            this.parameters = null;
            this.hashlist = null;
            this.array = null;
            this.block = null;
        }

        internal ARGS(Node parameters, Node hashlist, Node array, Node block, YYLTYPE location): base(location)
        {
            this.parameters = parameters;
            this.hashlist = hashlist;
            this.array = array;
            this.block = block;
        }

        internal bool IsEmpty
        {
            get { return parameters == null && hashlist == null && array == null && block == null; }
        }

        internal override bool ShortAndSimple()
        {
            if (hashlist != null || array != null)
                return false;

            int length = 0;
            for (Node arg = parameters; arg != null; arg = arg.nd_next)
                length++;

            return length < 10;
        }

        internal override List<ISimple> GenFixedArgs(CodeGenContext context, out List<bool> created)
        {
            List<ISimple> fixed_args = new List<ISimple>();
            created = new List<bool>();

            if (block != null)
            {
                bool b_created;
                fixed_args.Add(context.PreCompute(block, "block", Runtime.ProcRef, out b_created));
                created.Add(b_created);
            }
            else
            {
                fixed_args.Add(new AST.NIL(location));
                created.Add(false);
            }

            for (Node arg = parameters; arg != null; arg = arg.nd_next)
            {
                //object argument = arg;
                bool argument_created;
                fixed_args.Add(context.PreCompute0(arg, "arg", out argument_created));
                created.Add(argument_created);
            }

            return fixed_args;
        }

        internal override ISimple GenArgList(CodeGenContext context, out bool created)
        {
            bool single = true;

            // ArgList arglist = new ArgList();
            context.newobj(Runtime.ArgList.ctor);
            int arglist = context.StoreInTemp("arglist", Runtime.ArgListRef, location);

            int added = 0;
            for (Node arg = parameters; arg != null; arg = arg.nd_next)
            {
                //object argument = arg;
                bool argument_created;
                ISimple argument = context.PreCompute0(arg, "arg", out argument_created);

                // arglist.Add(argument);
                context.ldloc(arglist);
                argument.GenSimple(context);
                context.callvirt(Runtime.ArgList.Add);
                added++;

                context.ReleaseLocal(argument, argument_created);
            }

            if (added != 1)
                single = false;

            if (hashlist != null)
            {
                // object hash = hashlist;
                bool hash_created;
                ISimple hash = context.PreCompute(new HASH(hashlist, hashlist.location), "hashlist", out hash_created);

                // arglist.Add(hash);
                context.ldloc(arglist);
                hash.GenSimple(context);
                context.callvirt(Runtime.ArgList.Add);
                single = false;

                context.ReleaseLocal(hash, hash_created);
            }

            if (array != null)
            {
                // object list = array;
                bool list_created;
                ISimple list = context.PreCompute(array, "array", out list_created);

                // arglist.AddArray(list, caller);
                context.ldloc(arglist);
                list.GenSimple(context);
                context.ldloc(0);
                context.callvirt(Runtime.ArgList.AddArray);
                single = false;

                context.ReleaseLocal(list, list_created);
            }

            if (block != null)
            {
                // object b = block;
                bool b_created;
                ISimple b = context.PreCompute(block, "block", Runtime.ProcRef, out b_created);

                // arglist.block = b;
                context.ldloc(arglist);
                b.GenSimple(context);
                context.stfld(Runtime.ArgList.block);

                context.ReleaseLocal(b, b_created);
            }

            if (single)
            {
                context.ldloc(arglist);
                context.PushTrue();
                context.stfld(Runtime.ArgList.single_arg);
            }

            created = true;
            return new LOCAL(arglist, location);
        }



          internal override void Defined(CodeGenContext context)
        {
            PERWAPI.CILLabel undefined_label = context.NewLabel();
            PERWAPI.CILLabel end_label = context.NewLabel();

            for (Node arg = parameters; arg != null; arg = arg.nd_next)
            {
                arg.Defined(context);
                context.brfalse(undefined_label);
            }

            if (array != null)
            {
                array.Defined(context);
                context.brfalse(undefined_label);
            }

            if (hashlist != null)
            {
                hashlist.Defined(context);
                context.brfalse(undefined_label);
            }

            if (block != null)
            {
                block.Defined(context);
                context.brfalse(undefined_label);
            }

            context.PushTrue();
            context.box(PrimitiveType.Boolean);

            if (!IsEmpty)
            {
                context.br(end_label);

                context.CodeLabel(undefined_label);
                context.PushFalse();
                context.box(PrimitiveType.Boolean);

                context.CodeLabel(end_label);
            }
        }
    }



    internal class AMPER : Node
    {
        private Node arg;

        internal AMPER(Node arg, YYLTYPE location)
            : base(location)
        {
            this.arg = arg;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            // Ruby.Eval.block_pass(arg, caller);
            arg.GenCode(context);
            context.ldloc(0);
            context.call(Runtime.Eval.block_pass);
        }
    }


    internal delegate ISimple ProxyListDelegate(CodeGenContext context, out bool created);



    internal class ProxyList : ListGen
    {
        private ProxyListDelegate proxy;

        internal ProxyList(ProxyListDelegate proxy, YYLTYPE location): base(location)
        {
            this.proxy = proxy;
        }

        internal override ISimple GenArgList(CodeGenContext context, out bool created)
        {
            return proxy(context, out created);
        }
    }
}