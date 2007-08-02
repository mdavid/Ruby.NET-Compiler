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

    internal abstract class Scope : Node
    {
        internal List<string> locals_list = new List<string>();
        internal ClassDef frame_def;
        internal System.Type frame_type;
        internal Node body;
        internal Scope block_parent;
        internal Scope parent_scope;
        internal Scope BEGIN;


        internal PERWAPI.Class FrameClass
        {
            get
            {
                if (frame_def != null)
                    return frame_def;
                else
                    return CodeGenContext.FindClass(frame_type);
            }
        }

        internal Field GetFrameField(string name)
        {
            if (frame_def != null)
                return frame_def.GetField(name);
            else
                return CodeGenContext.FindField(frame_type.GetField(name));
        }

        internal ClassDef FileClass()
        {
            Scope parent;
            for (parent = this; parent != null && !(parent is SOURCEFILE); parent = parent.parent_scope);
            if (parent == null)
                return null;
            else
                return ((SOURCEFILE)parent).fileClass;
        }

        internal Scope(Scope parent_scope, YYLTYPE location)
            : base(location)
        {
            this.parent_scope = parent_scope;
            this.block_parent = null;
        }

        internal Scope(Node body, YYLTYPE location)
            : base(location)
        {
            this.body = body;
        }

        internal virtual void Init(YYLTYPE location, params object[] inputs)
        {
            this.body = (Node)inputs[0];
            this.location = location;
        }


        // ------------------------------------------------------------------------------

        // returns true if id already exists here or in an outer scope
        internal bool has_local(string id)
        {
            if (defined_statically(id) || defined_dynamically(id))
                return true;
            else if (block_parent != null)
                return block_parent.has_local(id);
            else
                return false;
        }

        internal bool defined_statically(string id)
        {
            return locals_list.Contains(id);
        }

        internal virtual bool defined_dynamically(string id)
        {
            return false;
        }


        internal virtual VAR add_locally(string id, YYLTYPE location)
        {
            if (defined_statically(id))
                return new StaticLocalVar(this, id, location);
            else
                return create_local_here(id, location);
        }

        internal virtual VAR add_local(string id, YYLTYPE location)
        {
            if (defined_statically(id))
                return new StaticLocalVar(id, this, location);
            else if (defined_dynamically(id))
                return new DynamicLocalVar(id, (BLOCK)this, location);
            else if (has_local(id))
                return get_outer_local(id, (BLOCK)this, 0, location);
            else
                return create_local_here(id, location);
        }

        internal virtual VAR create_local_here(string id, YYLTYPE location)
        {
            locals_list.Add(id);
            return new StaticLocalVar(this, id, location);
        }

        internal VAR get_outer_local(string id, BLOCK block, int depth, YYLTYPE location)
        {
            if (defined_statically(id))
                return new StaticOuterVar(id, block, depth, location);
            else if (defined_dynamically(id))
                return new DynamicOuterVar(id, block, depth, location);
            else
                return block_parent.get_outer_local(id, block, depth + 1, location);
        }


        // ------------------------------------------------------------------------------

        internal string CurrentMethodName()
        {
            for (Scope scope = this; scope != null; scope = scope.parent_scope)
                if (scope is DEFN)
                    return ((DEFN)scope).method_id.ToString();

            return "";
        }

        // ------------------------------------------------------------------------------

        private static int N = 0;

        internal void AddScopeLocals(CodeGenContext context)
        {
            // ------------------ Start new Context ----------------------------

            // [InteropMethod("MyClass")]
            // private class ActivationFrame: Ruby.Frame { ... }
            frame_def = context.CreateGlobalClass("_Internal", "Frame" + (N++), Runtime.FrameRef);

            Scope parentClass;
            for (parentClass = this; parentClass != null && !(parentClass is CLASS_OR_MODULE); parentClass = parentClass.parent_scope) ;

            string className = "";
            if (parentClass != null)
                className = ((CLASS_OR_MODULE)parentClass).internal_name;

            ClassDef fileClass = FileClass();
            string src = "";
            if (fileClass != null)
                src = fileClass.Name().Substring(11);

            frame_def.AddCustomAttribute(Runtime.FrameAttribute.ctor, new Constant[] { new StringConst(src), new StringConst(className) });

            foreach (string local in locals_list)
                CodeGenContext.AddField(frame_def, PERWAPI.FieldAttr.Public, ID.ToDotNetName(local), PrimitiveType.Object);

            // internal ActivationFrame(Frame caller): base(caller) { }
            CodeGenContext frame_ctor = context.CreateConstructor(frame_def, new Param(ParamAttr.Default, "caller", Runtime.FrameRef));
            frame_ctor.ldarg(0);
            frame_ctor.ldarg("caller");
            frame_ctor.call(Runtime.Frame.ctor);
            frame_ctor.ret();
            frame_ctor.Close();

            // internal string file() {
            CodeGenContext file = context.CreateMethod(frame_def, PERWAPI.MethAttr.PublicVirtual, "file", PrimitiveType.String);

            file.startMethod(this.location);
            //    return "thisfile.rb"
            file.ldstr(this.location.file);
            file.ret();
            file.Close();

            // internal override string methodName() {
            CodeGenContext methodName = context.CreateMethod(frame_def, PERWAPI.MethAttr.PublicVirtual, "methodName", PrimitiveType.String);
            methodName.startMethod(this.location);
            //    return "CurrentMethodName"
            methodName.ldstr(CurrentMethodName());
            methodName.ret();
            methodName.Close();

            CreateNestingMethod(frame_def, context);

            // ------------------ Return to Old Context ----------------------

            // ActivationFrame frame = new ActivationFrame(caller);
            context.ldarg("caller");
            context.newobj(frame_ctor.Method);
            int frame = context.StoreInTemp("frame", FrameClass, location);
            Debug.Assert(frame == 0);

            // frame.block_arg = block;
            context.ldloc(frame);
            LoadBlock0(context);
            context.stfld(Runtime.Frame.block_arg);

            if (this is BLOCK)
            {
                // frame.current_block = this;
                context.ldloc(frame);
                context.ldarg(0);
                context.stfld(Runtime.Frame.current_block);
            }
        }


        private void CreateNestingMethod(ClassDef Class, CodeGenContext context)
        {
            List<FieldDef> list = new List<FieldDef>();
            for (Scope parent = this; parent != null; parent = parent.parent_scope)
                if (parent is CLASS_OR_MODULE)
                    list.Add(((CLASS_OR_MODULE)parent).singletonField);

            // internal override Class[] nesting() {
            CodeGenContext nesting = context.CreateMethod(Class, PERWAPI.MethAttr.PublicVirtual, "nesting", new PERWAPI.ZeroBasedArray(Runtime.ClassRef));
            nesting.startMethod(this.location);
            //     Class[] array = new Class[list.Count];
            nesting.ldc_i4(list.Count);
            nesting.newarr(Runtime.ClassRef);
            int array = nesting.CreateLocal("array", new PERWAPI.ZeroBasedArray(Runtime.ClassRef));
            nesting.stloc(array);
            
            for (int i = 0; i < list.Count; i++)
            {
                // array[i] = list[i];
                nesting.ldloc(array);
                nesting.ldc_i4(i);
                nesting.ldsfld(list[i]);
                nesting.stelem_ref();
            }

            //     return array;
            nesting.ldloc(array);
            nesting.ret();
            nesting.ReleaseLocal(array, true);
            nesting.Close();
        }

        internal int returnTemp;

        internal void AddScopeBody(CodeGenContext context)
        {
            returnTemp = context.CreateLocal("returnTemp", PrimitiveType.Object);

            context.labels = new Labels();
            context.labels.Redo = context.NewLabel();
            context.labels.Return = context.NewLabel();

            // try { ... }
            context.StartBlock();
            {
                if (BEGIN != null)
                    BEGIN.GenCode(context);

                context.CodeLabel(context.labels.Redo);

                if (body != null)
                {
                    body.GenCode(context);

                    if (context.Reachable())
                        context.stloc(returnTemp);
                }

                context.leave(context.labels.Return);
            }
            PERWAPI.TryBlock tryBlock = context.EndTryBlock();        

            CatchReturnException(context, tryBlock);

            // ReturnLabel:
            //    return returnTemp;
            context.CodeLabel(context.labels.Return);
            context.newEndPoint(location);
            if (context.Method.GetRetType() != PERWAPI.PrimitiveType.Void)
                context.ldloc(returnTemp);
            context.ret();

            context.ReleaseLocal(returnTemp, true);
        }


        internal void CatchReturnException(CodeGenContext context, PERWAPI.TryBlock tryBlock)
        {
            // catch (Ruby.ReturnException exception) { ... }
            context.StartBlock();
            {
                PERWAPI.CILLabel falseLabel = context.NewLabel();

                int exception = context.StoreInTemp("exception", Runtime.ReturnExceptionRef, location);

                // if (exception.scope == thisframe)
                context.ldloc(exception);
                context.ldfld(Runtime.ReturnException.scope);
                context.ldloc(0);
                context.bne(falseLabel);

                //     returnTemp = exception.return_value;
                context.ldloc(exception);
                context.ldfld(Runtime.ReturnException.return_value);
                context.stloc(returnTemp);
                context.leave(context.labels.Return);

                // falseLabel:
                context.CodeLabel(falseLabel);
                // throw exception
                context.rethrow();

                context.ReleaseLocal(exception, true);
            }
            context.EndCatchBlock(Runtime.ReturnExceptionRef, tryBlock);
        }
    }
}