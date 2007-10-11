/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/


using System.CodeDom;
using System.Collections;
using PERWAPI;


namespace Ruby.Compiler.AST
{

    internal class FORMALS : Node
    {
        internal StaticLocalVar normal;
        internal ASSIGNMENT optional;
        internal StaticLocalVar rest;
        internal StaticLocalVar block;

        internal FORMALS(Node normal, Node optional, Node rest, Node block, YYLTYPE location)
            : base(location)
        {
            this.normal = (StaticLocalVar)normal;
            this.optional = (ASSIGNMENT)optional;
            this.rest = (StaticLocalVar)rest;
            this.block = (StaticLocalVar)block;
        }

        internal int min_args
        {
            get
            {
                int count = 0;
                for (Node arg = normal; arg != null; arg = arg.nd_next)
                    count++;

                return count;
            }
        }

        internal int arity
        {
            get
            {
                if (optional != null || rest != null)
                    return -min_args - 1;

                return min_args;
            }
        }

        public CodeParameterDeclarationExpressionCollection ToCodeParameterDeclarationExpressionCollection()
        {
            CodeParameterDeclarationExpressionCollection collection = new CodeParameterDeclarationExpressionCollection();

            for (Node n = normal; n != null; n = n.nd_next)
                collection.Add(new CodeParameterDeclarationExpression(typeof(object), ((StaticLocalVar)n).vid));

            // Fixme: add other parameters here

            return collection;
        }

        internal bool ShortAndSimple()
        {
            if (optional != null || rest != null)
                return false;
            else
                return min_args < 10;
        }

        internal void CopySimple(CodeGenContext context, Scope scope)
        {
            if (block != null)
            {
                // locals.block = block;
                string bname = ID.ToDotNetName(block.vid);
                context.ldloc(0);
                LoadBlock(context);
                context.stfld(scope.GetFrameField(bname));
            }

            for (Node f = normal; f != null; f = f.nd_next)
            {
                string fname = ID.ToDotNetName(((VAR)f).vid);

                // local.f = f;
                context.ldloc(0);
                context.ldarg(fname);
                context.stfld(scope.GetFrameField(fname));
            }
        }

        internal void CopyToLocals(CodeGenContext context, Scope scope)
        {
            CopyNormalFormals(context, scope);
            CopyOptionalFormals(context, scope);
            CopyRestFormals(context, scope);
            CopyBlockFormal(context, scope);
        }



        private void CopyNormalFormals(CodeGenContext context, Scope scope)
        {
            PERWAPI.CILLabel OKLabel = context.NewLabel();

            if (min_args > 0)
            {
                // if (args.Length < min_args)
                context.ldarg("args");
                context.callvirt(Runtime.ArgList.get_Length);
                int length = context.StoreInTemp("length", PrimitiveType.Int32, location);
                context.ldloc(length);
                context.ldc_i4(min_args);
                context.bge(OKLabel);

                //context.Inst(Op.clt);
                //context.brfalse(OKLabel);

                // context.Branch(BranchOp.bge, OKLabel);

                // throw new ArgumentError(string.Format("wrong number of arguments ({0} for {1})", args.Length, arity).raise(caller);
                // FIXME: next line needs a String
                context.ldstr("wrong number of arguments ({0} for {1})");
                context.ldloc(length);
                context.box(PrimitiveType.Int32);
                context.ldc_i4(min_args);
                context.box(PrimitiveType.Int32);
                context.call(Runtime.SystemString.Format);
                context.newobj(Runtime.ArgumentError.ctor);
                context.ldloc(0);
                context.callvirt(Runtime.Exception.raise);
                context.throwOp();

                context.ReleaseLocal(length, true);

                // OKLabel:
                context.CodeLabel(OKLabel);
            }

            // Copy parameters to locals
            for (Node f = normal; f != null; f = f.nd_next)
            {
                string name = ID.ToDotNetName(((VAR)f).vid);

                // local.f = args.GetNext();
                context.ldloc(0);
                context.ldarg("args");
                context.callvirt(Runtime.ArgList.GetNext);
                context.stfld(scope.GetFrameField(name));
            }
        }

        private void CopyOptionalFormals(CodeGenContext context, Scope scope)
        {
            for (ASSIGNMENT opt = (ASSIGNMENT)optional; opt != null; opt = (ASSIGNMENT)opt.nd_next)
            {
                PERWAPI.CILLabel runout_label = context.NewLabel();
                PERWAPI.CILLabel end_label = context.NewLabel();

                string name = ID.ToDotNetName(((VAR)(opt.lhs)).vid);
                Node defaultValue = opt.rhs;

  

                // if (args.RunOut()) goto RunOut
                context.ldarg("args");
                context.callvirt(Runtime.ArgList.RunOut);
                context.brtrue(runout_label);

                // locals.name = args.GetNext();
                context.ldloc(0);
                context.ldarg("args");
                context.callvirt(Runtime.ArgList.GetNext);
                context.br(end_label);

                // RunOut:
                context.CodeLabel(runout_label);

                // object def = defaultValue;
                bool created;
                ISimple def = context.PreCompute(defaultValue, "default", out created);

                // locals.name = defaultValue
                context.ldloc(0);
                def.GenSimple(context);

                context.ReleaseLocal(def, created);

                context.CodeLabel(end_label);

                // locals.name = ...
                context.stfld(scope.GetFrameField(name));
            }
        }

        private void CopyRestFormals(CodeGenContext context, Scope scope)
        {
            if (rest != null)
            {
                string name = ID.ToDotNetName(rest.vid);

                // locals.name = args.GetRest();
                context.ldloc(0);
                context.ldarg("args");
                context.callvirt(Runtime.ArgList.GetRest);
                context.stfld(scope.GetFrameField(name));
            }
        }

        private void CopyBlockFormal(CodeGenContext context, Scope scope)
        {
            if (block != null)
            {
                string name = ID.ToDotNetName(block.vid);

                // locals.name = args.block;
                context.ldloc(0);
                LoadBlock(context);
                context.stfld(scope.GetFrameField(name));
            }
        }
    }
}