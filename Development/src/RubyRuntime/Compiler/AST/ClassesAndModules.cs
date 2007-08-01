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

    internal abstract class CLASS_OR_MODULE : Scope
    {
        protected CONST name;
        internal FieldDef singletonField;
        internal string internal_name;

        internal CLASS_OR_MODULE(Scope parent, YYLTYPE location)
            : base(parent, location)
        {
        }

        internal abstract void DefineClass(CodeGenContext context, PERWAPI.FieldDef singleton);

        internal virtual FieldDef DefineAllocator(CodeGenContext context, MethodDef ctor)
        {
            return null;
        }

        internal override void Init(YYLTYPE location, params object[] inputs)
        {
            this.location = location;
            this.name = (CONST)inputs[0];
            this.body = (Node)inputs[1];
        }


        private ISimple PushScope(CodeGenContext context, out bool created)
        {
            if (name != null && name.scope != null)
                return context.PreCompute(name.scope, "scope", out created);
            else
            {
                created = false;
                if (context.CurrentRubyClass != null)
                    return new StaticField(context.CurrentRubyClass, name.location);
                else
                    return new StaticField(Runtime.Init.rb_cObject, name.location);
            }
        }

        protected abstract ISimple PushSuper(CodeGenContext context, out bool created);

        static internal Stack<ClassDef> interopClasses = new Stack<ClassDef>();

        internal static ClassDef CurrentInteropClass()
        {
            if (interopClasses.Count > 0)
                return interopClasses.Peek();
            else
                return null;
        }




        internal override void GenCode0(CodeGenContext context)
        {
            bool scope_created, super_created;

            CodeGenContext newContext = new CodeGenContext(context);

            // scope and super must be evaluated in surrounding scope and passed to Init method
            ISimple scope = PushScope(context, out scope_created);
            ISimple super = PushSuper(context, out super_created);

            string basename = ID.ToDotNetName(name.vid);

            //public class MyClass: Object {

            ClassDef interopClass = newContext.CreateNestedClass(CurrentInteropClass(), basename, Runtime.ObjectRef);
            interopClasses.Push(interopClass);
            
            //    public static Class classname;
            int seqNo = 0;
            internal_name = basename;
            while (FileClass().GetField(internal_name) != null)
                internal_name = basename + (seqNo++);

            // Define singleton in file class so that it gets initialized by .cctor
            singletonField = FileClass().AddField(PERWAPI.FieldAttr.PublicStatic, internal_name, Runtime.ClassRef);
            newContext.CurrentRubyClass = singletonField;

            //    public MyClass() : base(singleton) { };
            if (interopClass.GetMethod(".ctor", new Type[0]) == null)
            {
                CodeGenContext class_constructor0 = newContext.CreateConstructor(interopClass);
                class_constructor0.ldarg(0);
                class_constructor0.ldsfld(singletonField);
                class_constructor0.call(Runtime.Object.ctor);
                class_constructor0.ret();
                class_constructor0.Close();
            }

            //    public MyClass(Class klass) : base(klass) { };
            MethodDef ctor = interopClass.GetMethod(".ctor", new Type[] { Runtime.ClassRef } );
            CodeGenContext class_constructor;
            if (ctor == null)
            {
                class_constructor = newContext.CreateConstructor(interopClass, new Param(ParamAttr.Default, "klass", Runtime.ClassRef));
                class_constructor.ldarg(0);
                class_constructor.ldarg("klass");
                class_constructor.call(Runtime.Object.ctor);
                class_constructor.ret();
                class_constructor.Close();
            }
            else
            {
                class_constructor = new CodeGenContext();
                class_constructor.Method = ctor;
            }


            //    internal static void Init_fullname(object scope, object super, object recv, Frame caller) {
            CodeGenContext Init = newContext.CreateMethod(FileClass(), MethAttr.PublicStatic, "Init_" + internal_name, PrimitiveType.Object,
                    new Param(ParamAttr.Default, "scope", PrimitiveType.Object),
                    new Param(ParamAttr.Default, "super", PrimitiveType.Object),
                    new Param(ParamAttr.Default, "recv", PrimitiveType.Object),
                    new Param(ParamAttr.Default, "caller", Runtime.FrameRef));


            Init.startMethod(this.location);

            AddScopeLocals(Init);

            // singleton = scope.define_???(...)
            DefineClass(Init, singletonField);

            // recv = singleton;
            Init.ldsfld(singletonField);
            Init.starg("recv");

            // Fixme: should be conditional
            // singleton.define_allocator(allocator);
            FieldDef allocator = DefineAllocator(newContext, class_constructor.Method);
            if (allocator != null)
            {
                Init.ldsfld(singletonField);
                Init.ldsfld(allocator);
                Init.call(Runtime.Class.define_alloc_func);
            }

            AddScopeBody(Init);

            Init.ReleaseLocal(0, true);

            Init.Close();

            interopClasses.Pop();

            // --------------------- Return to old Context ----------------------------


            context.newLine(location);
            // Init(scope, super, recv, caller);
            scope.GenSimple(context);
            super.GenSimple(context);
            context.ldarg("recv");
            context.ldloc(0);
            context.call(Init.Method);

            context.ReleaseLocal(super, super_created);
            context.ReleaseLocal(scope, scope_created);
        }
    }



    internal class CLASS : CLASS_OR_MODULE        // Class Definition
    {
        //    class name < super_class 
        //        body
        //    end

        internal CLASS(Scope parent, YYLTYPE location): base(parent, location)
        {
        }

        private Node super_class;    // optional


        internal override void Init(YYLTYPE location, params object[] inputs)
        {
            this.location = location;
            this.name = (CONST)inputs[0];
            this.super_class = (Node)inputs[1];
            this.body = (Node)inputs[2];
        }

        protected override ISimple PushSuper(CodeGenContext context, out bool created)
        {
            if (super_class != null)
                return context.PreCompute(super_class, "super", out created);
            else
            {
                created = false;
                return new NIL(location);
            }
        }

        internal override void DefineClass(CodeGenContext context, PERWAPI.FieldDef singleton)
        {
            // Class.define_class(scope, name.vid, super, caller);

            context.ldarg("scope");
            context.ldstr(name.vid.ToString());
            context.ldarg("super");
            context.ldloc(0);
            context.call(Runtime.Class.rb_define_class);
            context.stsfld(singleton);
        }

        internal override FieldDef DefineAllocator(CodeGenContext context, MethodDef ctor)
        {
            // Conservative - don't create allocator if we're not sure what the base class is
            if (super_class != null)
                return null;

            ClassRef baseClass = Runtime.MethodBodyNRef(0);
            ClassDef allocator = context.CreateGlobalClass("_Internal", "Method_" + internal_name + "_Allocator", baseClass);

            //     internal static Call0(Class last_class, object recv, ...) { body }
            CodeGenContext Call = context.CreateMethod(allocator, MethAttr.PublicVirtual, "Call0", PrimitiveType.Object, new Param[] { 
                    new Param(ParamAttr.Default, "last_class", Runtime.ClassRef),
                    new Param(ParamAttr.Default, "recv", PrimitiveType.Object),
                    new Param(ParamAttr.Default, "caller", Runtime.FrameRef),
                    new Param(ParamAttr.Default, "block", Runtime.ProcRef)});

           
            // return new MyClass((Class)recv));
            Call.ldarg("recv");
            Call.cast(Runtime.ClassRef);
            Call.newobj(ctor);
            Call.ret();
            Call.Close();

            //     internal MyAllocator() {}
            CodeGenContext constructor = context.CreateConstructor(allocator);
            constructor.ldarg(0);
            constructor.call(Runtime.MethodBodyCtor(baseClass));
            constructor.ret();
            constructor.Close();

            //     internal static MyAllocator singleton;
            FieldDef singleton = CodeGenContext.AddField(allocator, FieldAttr.PublicStatic, "myRubyMethod", allocator);

            //     static MyAllocator() {
            //         singleton = new MyAllocator();
            //     }
            CodeGenContext staticConstructor = context.CreateStaticConstructor(allocator);
            staticConstructor.newobj(constructor.Method);
            staticConstructor.stsfld(singleton);
            staticConstructor.ret();
            staticConstructor.Close();

            return singleton;
        }     
    }



    internal class SCLASS : CLASS_OR_MODULE        // Singleton Class definition
    {
        //    class << singleton 
        //        body
        //    end

        internal SCLASS(Scope parent, YYLTYPE location): base(parent, location)
        {
        }

        private Node singleton;

        internal override void Init(YYLTYPE location, params object[] inputs)
        {
            this.singleton = (Node)inputs[0];
            this.body = (Node)inputs[1];
            this.name = new CONST(parent_scope, ID.intern("singletonClass"), singleton.location);
            this.location = location;
        }

        protected override ISimple PushSuper(CodeGenContext context, out bool created)
        {
            return context.PreCompute(singleton, "super", out created); // pass singleton as super
        }

        internal override void DefineClass(CodeGenContext context, PERWAPI.FieldDef singleton)
        {
            // Class.singleton_class(caller, super)
            context.ldloc(0);
            context.ldarg("super");
            context.call(Runtime.Class.singleton_class);
            context.stsfld(singleton);
        }
    }



    internal class MODULE : CLASS_OR_MODULE        // Module definition
    {
        //    module name 
        //        body 
        //    end

        internal MODULE(Scope parent, YYLTYPE location): base(parent, location)
        {
        }

        protected override ISimple PushSuper(CodeGenContext context, out bool created)
        {
            created = false;
            return new NIL(location);
        }

        internal override void DefineClass(CodeGenContext context, PERWAPI.FieldDef singleton)
        {
            // Class.define_module(scope, name.vid, caller);
            context.ldarg("scope");
            context.ldstr(name.vid.ToString());
            context.ldloc(0);
            context.call(Runtime.Class.rb_define_module);
            context.stsfld(singleton);
        }
    }



    internal class StaticField : Node, ISimple
    {
        private Field field;

        internal StaticField(Field field, YYLTYPE location)
            : base(location)
        {
            this.field = field;
        }

        public void GenSimple(CodeGenContext context)
        {
            context.ldsfld(field);
        }
    }
}