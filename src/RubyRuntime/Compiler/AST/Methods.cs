/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/

using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using PERWAPI;


namespace Ruby.Compiler.AST
{

    internal class DEFN : Scope        // Regular Method Definition
    {
        //    def method_id(args)
        //        body
        //    end

        YYLTYPE name_location; // used by codeDOM for replacing names


        internal DEFN(Scope parent, string method_id, YYLTYPE location, YYLTYPE name_location)
            : base(parent, location)
        {
            this.method_id = method_id;
            this.name_location = name_location;
        }

        internal string method_id;
        internal FORMALS formals;    // optional


        public CodeMemberMethod ToCodeMemberMethod()
        {
            CodeMemberMethod method;

            if (method_id == "initialize")
                method = new CodeConstructor();
            else
                method = new CodeMemberMethod();

            method.Name = method_id;
            method.ReturnType = new CodeTypeReference(typeof(object));

            method.Parameters.AddRange(formals.ToCodeParameterDeclarationExpressionCollection());

            if (method_id == "InitializeComponent")
                for (Node n = body; n != null; n = n.nd_next)
                {
                    if (n is ASSIGNMENT)
                        method.Statements.Add(((ASSIGNMENT)n).ToCodeStatement());
                    else if (n is METHOD_CALL)
                        method.Statements.Add(n.ToCodeExpression());
                    else
                        throw new System.NotImplementedException(n.GetType().ToString());
                }
            else if (body != null)
                method.Statements.Add(new CodeCommentStatement("Dummy statement so that it doesn't appear empty to the designer"));

            method.UserData["original_name"] = method.Name;
            method.UserData["name_location"] = name_location;
            method.UserData["location"] = this.location;

            return method;
        }

        internal override void Init(YYLTYPE location, params object[] inputs)
        {
            this.location = location;
            this.formals = (FORMALS)inputs[0];
            this.body = (Node)inputs[1];
        }


        internal override void GenCode0(CodeGenContext context)
        {
            // ruby_class.define_method(...);
            context.newStartPoint(location);
            context.ruby_class(parent_scope);
            DefineMethod(context);
            if (CLASS_OR_MODULE.interopClasses.Count > 0)
            {
                // BBTAG: remove existing interop method
                string methodName = Translate(this.method_id);
                if (CLASS_OR_MODULE.CurrentInteropClass().GetMethod(methodName) != null)
                    CLASS_OR_MODULE.CurrentInteropClass().RemoveMethod(methodName);
                AddInteropMethod(context);
            }
        }

        private string Translate(string name)
        {
            bool illegal = !char.IsLetter(name[0]);
            foreach (char c in name)
                if (!(char.IsLetterOrDigit(c) || c == '_'))
                    illegal = true;

            if (!illegal)
                return name;

            if (name.EndsWith("?"))
                return Translate("is_" + name.Substring(0, name.Length - 1));
            if (name.EndsWith("!"))
                return Translate("do_" + name.Substring(0, name.Length - 1));
            if (name.EndsWith("="))
                return Translate("set_" + name.Substring(0, name.Length - 1));
            else
                return "dummy";
        }

        private void AddInteropMethod(CodeGenContext context)
        {
            List<Param> args = new List<Param>();
            if (formals.arity < 0)
            {
                Param param = new Param(ParamAttr.Default, "args", new PERWAPI.ZeroBasedArray(PERWAPI.PrimitiveType.Object));
                param.AddCustomAttribute(Runtime.ParamArrayAttribute.ctor, new byte[0]);
                args.Add(param);
            }
            else
                for (int i = 1; i <= this.formals.arity; i++)
                    args.Add(new Param(ParamAttr.Default, "p" + i, PrimitiveType.Object));

            // public object method_id(...) {

            try
            {
                CodeGenContext method = context.CreateMethod(CLASS_OR_MODULE.CurrentInteropClass(), MethAttr.PublicVirtual, Translate(this.method_id), PERWAPI.PrimitiveType.Object, args.ToArray());

                method.startMethod(this.location);

                //    return Eval.Calln(this, "method_id", ...);
                method.ldarg(0);
                method.ldstr(method_id);

                if (formals.arity < 0)
                {
                    method.ldarg(1);
                    method.call(Runtime.Eval.Calln);
                }
                else
                {
                    for (int i = 1; i <= args.Count; i++)
                        method.ldarg(i);
                    method.call(Runtime.Eval.Call(args.Count));
                }

                method.ret();

                method.Close();
            }
            catch (PERWAPI.DescriptorException e)
            {
                Compiler.LogWarning(e.Message);
            }
        }


        internal void DefineMethod(CodeGenContext context)
        {
            // ... .define_method("MyMethod", MyMethod.singleton, arity, caller);
            context.ldstr(method_id.ToString());
            context.ldsfld(GenerateClassForMethod(context));
            context.ldc_i4(formals.arity);
            context.ldloc(0);
            context.callvirt(Runtime.Class.define_method);
            context.ldnull();
        }


        private FieldDef GenerateClassForMethod(CodeGenContext context)
        {
            ClassRef baseClass;
            if (formals.ShortAndSimple())
                baseClass = Runtime.MethodBodyNRef(formals.min_args);
            else
                baseClass = Runtime.MethodBodyRef;

            // private class MyMethod: MethodBody? {
            ClassDef methodClass = context.CreateGlobalClass("_Internal", "Method_" + ID.ToDotNetName(method_id), baseClass);
            methodClass.AddCustomAttribute(Runtime.InteropMethodAttribute.ctor, System.Text.UnicodeEncoding.UTF8.GetBytes(method_id));

            //     internal static Calln(Class last_class, object recv, ...) { body }
            GenCallMethod(methodClass, context);

            //     internal MyMethod() {}
            CodeGenContext constructor = context.CreateConstructor(methodClass);
            constructor.ldarg(0);
            constructor.call(Runtime.MethodBodyCtor(baseClass));
            constructor.ret();
            constructor.Close();

            //     internal static MyMethod singleton;
            FieldDef singleton = CodeGenContext.AddField(methodClass, FieldAttr.PublicStatic, "myRubyMethod", methodClass);

            //     static MyMethod() {
            //         singleton = new MyMethod();
            //     }
            CodeGenContext staticConstructor = context.CreateStaticConstructor(methodClass);
            staticConstructor.newobj(constructor.Method);
            staticConstructor.stsfld(singleton);
            staticConstructor.ret();
            staticConstructor.Close();
            
            // }

            // ------------------- Return to original context -----------------------------

            return singleton;
        }


        private void GenCallMethod(ClassDef Class, CodeGenContext context)
        {
            List<Param> args = new List<Param>();
            args.Add(new Param(ParamAttr.Default, "last_class", Runtime.ClassRef));
            args.Add(new Param(ParamAttr.Default, "recv", PrimitiveType.Object));
            args.Add(new Param(ParamAttr.Default, "caller", Runtime.FrameRef));

            CodeGenContext Call;

            if (formals.ShortAndSimple())
            {
                args.Add(new Param(ParamAttr.Default, "block", Runtime.ProcRef));
                for (StaticLocalVar formal = formals.normal; formal != null; formal = (StaticLocalVar)formal.nd_next)
                    args.Add(new Param(ParamAttr.Default, formal.vid, PrimitiveType.Object));

                Call = context.CreateMethod(Class, MethAttr.PublicVirtual, "Call" + formals.min_args, PrimitiveType.Object, args.ToArray());
            }
            else
            {
                args.Add(new Param(ParamAttr.Default, "args", Runtime.ArgListRef));

                // internal static object Calln(Class last_class, object recv, Frame caller, Proc block, ArrayList args)
                Call = context.CreateMethod(Class, MethAttr.PublicVirtual, "Calln", PrimitiveType.Object, args.ToArray());
            }

            Call.orig_func = Call.Method;     // BBTAG
            Call.orig_func_formals = formals; // BBTAG
            Call.currentSkeleton = context.currentSkeleton; // BBTAG
            Call.postPassList = context.postPassList; // BBTAG
            Call.peFiles = context.peFiles;   // BBTAG

            Call.startMethod(this.location);

            AddScopeLocals(Call);
            if (formals.ShortAndSimple())
                formals.CopySimple(Call, this);
            else
                formals.CopyToLocals(Call, this);

            AddScopeBody(Call);

            Call.newEndPoint(location);

            Call.ReleaseLocal(0, true);

            Call.Close();
        }
    }



    internal class DEFS : DEFN	// Singleton Method Definition
    {
        //	def receiver.method_id(args)
        //		body
        //	end

        internal DEFS(Scope parent, string method_id, YYLTYPE location, YYLTYPE name_location): base(parent, method_id, location, name_location)
        {
        }

        private Node receiver;

        internal override void Init(YYLTYPE location, params object[] inputs)
        {
            this.location = location;
            this.receiver = (Node)inputs[0];
            this.formals = (FORMALS)inputs[1];
            this.body = (Node)inputs[2];
        }

        internal override void GenCode0(CodeGenContext context)
        {
            // singleton_class(caller, receiver).define_method(...);
            context.newStartPoint(location);
            context.ldloc(0);
            receiver.GenCode(context);
            context.call(Runtime.Class.singleton_class);
            DefineMethod(context);
        }
    }



    internal class BLOCK : Scope
    {
        //	{ |args| body }

        protected LVALUE args;	// optional
        internal List<FieldDef> frameFields = new List<FieldDef>();
        private int arity;

        internal BLOCK(Scope current, YYLTYPE location)
            : base(current, location)
        {
            this.block_parent = current;
        }

        internal override void Init(YYLTYPE location, params object[] input)
        {
            this.location = location;
            this.args = (LVALUE)input[0];
            body = (Node)input[1];

            Node list = this.args;

            if (list == null)
                arity = -1;
            else
            {
                arity = 0;

                if (list is MLHS)
                    list = ((MLHS)list).elements;

                while (list != null)
                {
                    arity++;

                    if (list is LHS_STAR)
                        arity = -arity;

                    list = (LVALUE)list.nd_next;
                }
            }
        }


        internal override void GenCode0(CodeGenContext context)
        {
            MethodDef BlockN_constructor = GenerateClassForMethod(context);

            // new Ruby.Proc(recv, block, new BlockN(locals, this.locals1, this.locals2 ...), arity);            
            context.ldarg("recv");  // recv
            LoadBlock(context);

            context.ldloc(0);            // locals
            if (block_parent is BLOCK)
                foreach (FieldDef field in ((BLOCK)block_parent).frameFields)
                {
                    // this.localsN
                    context.ldarg(0);
                    context.ldfld(field);
                }

            context.newobj(BlockN_constructor);

            context.ldc_i4(arity);

            context.newobj(Runtime.Proc.ctor);
        }


        protected MethodDef GenerateClassForMethod(CodeGenContext context)
        {
            // private class Block_N: Ruby.Block {
            ClassDef blockClass = context.CreateGlobalClass("_Internal", "Block", Runtime.BlockRef);

            System.Diagnostics.Debug.Assert(frameFields.Count == 0);

            // create locals for outer scopes
            int depth = 0;
            for (Scope scope = block_parent; scope != null; scope = scope.block_parent)
            {
                FieldDef fieldDef = CodeGenContext.AddField(blockClass, FieldAttr.Public, "locals" + (depth++), scope.FrameClass);
                frameFields.Add(fieldDef);
            }

            MethodDef constructor = GenConstructor(blockClass, context);

            //     internal static object Call(...) { body }
            GenCallMethod(blockClass, context);

            // }

           // -------------------------- Return to original context ---------------------------

            return constructor;
        }


        private MethodDef GenConstructor(ClassDef Class, CodeGenContext context)
        {
            // internal Block_N(outer locals ...): base(defining_scope) { ... }
            List<Param> parameters = new List<Param>();

            foreach (FieldDef field in frameFields)
                parameters.Add(new Param(ParamAttr.Default, field.Name(), field.GetFieldType()));

            CodeGenContext constructor = context.CreateConstructor(Class, parameters.ToArray());

            constructor.ldarg(0); // this
            constructor.ldarg(1); // defining_scope
            constructor.call(Runtime.Block.ctor);

            int N = 0;
            foreach (FieldDef field in frameFields)
            {
                // locals.field = localsN;
                constructor.ldarg(0);
                constructor.ldarg("locals" + (N++));
                constructor.stfld(field);
            }

            constructor.ret();

            constructor.Close();

            return constructor.Method;
        }


        private void GenCallMethod(ClassDef Class, CodeGenContext context)
        {
            // internal static object Calln(Class last_class, object recv, Frame caller, ArgList args)

            CodeGenContext Calln = context.CreateMethod(Class, MethAttr.PublicVirtual, "Calln", PrimitiveType.Object,
                    new Param(ParamAttr.Default, "last_class", Runtime.ClassRef),
                    new Param(ParamAttr.Default, "ruby_class", Runtime.ClassRef),
                    new Param(ParamAttr.Default, "recv", PrimitiveType.Object),
                    new Param(ParamAttr.Default, "caller", Runtime.FrameRef),
                    new Param(ParamAttr.Default, "args", Runtime.ArgListRef));

            Calln.startMethod(this.location);

            AddScopeLocals(Calln);

            if (args != null)
            {
                new ASSIGNMENT(args, new BLOCKARGS(args.location), args.location).GenCode(Calln); // Fixme: should be inside try block for ReturnException
                Calln.pop();
            }

            AddScopeBody(Calln);

            Calln.ReleaseLocal(0, true);

            Calln.Close();
        }
    }



    internal class BLOCKARGS : ListGen   // used internally
    {
        internal BLOCKARGS(YYLTYPE location)
            : base(location)
        {
        }

        internal override ISimple GenArgList(CodeGenContext context, out bool created)
        {
            created = false;
            return new PARAM("args", location);
        }
    }



    internal class FORBODY : BLOCK
    {
        internal FORBODY(Scope current, YYLTYPE location): base(current, location)
        {
        }


        internal override VAR create_local_here(string id, YYLTYPE location)
        {
            block_parent.create_local_here(id, location);
            return get_outer_local(id, this, 0, location);
        }

    }



    internal class begin : Node        // begin Block
    {
        //    begin
        //        body
        //    end

        private Node body;        // optional

        internal begin(Node body, YYLTYPE location)
            : base(location)
        {
            this.body = body;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            if (body != null)
                body.GenCode(context);
            else
                context.ldnull();
        }
    }



    internal class BEGIN : Scope        // BEGIN Block
    {
        //    BEGIN { body }

        internal BEGIN(Scope parent_scope, YYLTYPE location)
            : base(parent_scope, location)
        {
        }


        static int seq = 0;

        internal override void GenCode0(CodeGenContext context)
        {
            CodeGenContext Begin = context.CreateMethod(FileClass(), MethAttr.PublicStatic, "Begin" + (seq++), PrimitiveType.Object,
                    new Param(ParamAttr.Default, "recv", PrimitiveType.Object),
                    new Param(ParamAttr.Default, "caller", Runtime.FrameRef));

            Begin.startMethod(this.location);

            AddScopeLocals(Begin);

            AddScopeBody(Begin);

            Begin.ReleaseLocal(0, true);

            Begin.Close();


            // Begin(recv, caller);
            context.ldarg("recv");
            context.ldloc(0);
            context.call(Begin.Method);
            context.pop();
        }
    }



    internal class END : BLOCK        // END Block
    {
        //    END { body }

        internal END(Scope parent_scope, YYLTYPE location)
            : base(parent_scope, location)
        {
        }

        internal override void Init(YYLTYPE location, params object[] input)
        {
            this.location = location;
            this.args = new MLHS(null, location);
            this.body = (Node)input[0];
        }

        internal override void GenCode0(CodeGenContext context)
        {
            base.GenCode0(context);
            context.call(Runtime.Program.End);
        }
    }
}
