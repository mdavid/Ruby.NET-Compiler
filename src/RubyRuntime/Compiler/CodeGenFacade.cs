/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/


using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Text;
using System.Diagnostics;
using PERWAPI;
using Ruby.Runtime;
using System.Globalization;

namespace Ruby.Compiler {
    // Facade to PERWAPI

    internal class CodeGenContext {
        internal PEFile Assembly;

        internal MethodDef Method;
        internal List<Local> CLRLocals;
        internal Labels labels;
        internal Field CurrentRubyClass;

        // BBTAG: try static equivalent of ruby_frame->orig_func
        internal MethodDef orig_func;

        // BBTAG: also need the complete formals for orig_func
        internal AST.FORMALS orig_func_formals;

        internal static ClassSkeleton objectSkeleton = new ClassSkeleton("Object", Runtime.ObjectRef);
        internal ClassSkeleton currentSkeleton;
        internal List<ClassSkeletonPostPass> postPassList = new List<ClassSkeletonPostPass>();

        internal List<PERWAPI.ReferenceScope> peFiles = new List<PERWAPI.ReferenceScope>();

        internal CodeGenContext() {
            currentSkeleton = objectSkeleton;
        }

        internal CodeGenContext(CodeGenContext context) {
            this.Assembly = context.Assembly;
            this.Method = context.Method;
            this.CLRLocals = context.CLRLocals;
            this.labels = context.labels;
            this.CurrentRubyClass = context.CurrentRubyClass;

            this.orig_func = context.orig_func;                 // BBTAG
            this.orig_func_formals = context.orig_func_formals; // BBTAG
            this.currentSkeleton = context.currentSkeleton;     // BBTAG
            this.postPassList = context.postPassList;           // BBTAG
            this.peFiles = context.peFiles;                     // BBTAG
        }


        private CILInstructions buffer {
            get { return this.Method.GetCodeBuffer(); }
        }

        internal void startMethod(YYLTYPE location) {
            buffer.DefaultSourceFile = SourceFile.GetSourceFile(location.file, System.Guid.Empty, System.Guid.Empty, System.Guid.Empty);
            newStartPoint(location);
        }

        internal void newLine(YYLTYPE location) {
            if (buffer.DefaultSourceFile != null && location != null)
                buffer.Line((uint)location.first_line, (uint)location.first_column, (uint)location.last_line, (uint)location.last_column);
        }


        internal void newStartPoint(YYLTYPE location) {
            if (buffer.DefaultSourceFile != null && location != null)
                buffer.Line((uint)location.first_line, (uint)location.first_column, (uint)location.first_column);
        }

        internal void newEndPoint(YYLTYPE location) {
            if (buffer.DefaultSourceFile != null && location != null)
                buffer.Line((uint)location.last_line, (uint)location.last_column, (uint)location.last_column);
        }

        internal void newobj(PERWAPI.Method method) {
            buffer.MethInst(MethodOp.newobj, method);
        }

        internal void newarr(PERWAPI.Type type) {
            buffer.TypeInst(TypeOp.newarr, type);
        }

        internal void callvirt(PERWAPI.Method method) {
            buffer.MethInst(MethodOp.callvirt, method);
        }

        internal void call(PERWAPI.Method method) {
            buffer.MethInst(MethodOp.call, method);
        }

        internal void dup() {
            buffer.Inst(Op.dup);
        }

        internal void pop() {
            buffer.Inst(Op.pop);
        }

        internal void stelem_ref() {
            buffer.Inst(Op.stelem_ref);
        }

        internal void ldloc(int loc) {
            buffer.LoadLocal(loc);
        }

        internal void stloc(int loc) {
            buffer.StoreLocal(loc);
        }

        internal void ret() {
            buffer.Inst(Op.ret);
        }

        internal void ldnull() {
            buffer.Inst(Op.ldnull);
        }

        internal void ldtoken(Type aType) {
            buffer.TypeInst(TypeOp.ldtoken, aType);
        }

        internal void ldc_i4(int i) {
            buffer.PushInt(i);
        }

        internal void ldc_r8(double d) {
            buffer.ldc_r8(d);
        }

        internal void ldstr(string str) {
            Debug.Assert(str != null);
            buffer.ldstr(str);
        }

        internal void ldarg(string argName) {
            ldarg(FindArg(argName));
        }

        internal void ldarg(int argNum) {
            Debug.Assert(argNum >= 0);
            buffer.LoadArg(argNum);
        }

        internal void starg(string argName) {
            buffer.StoreArg(FindArg(argName));
        }

        internal void cast(PERWAPI.Type t) {
            buffer.TypeInst(TypeOp.castclass, t);
        }

        internal void isinst(PERWAPI.Type t) {
            buffer.TypeInst(TypeOp.isinst, t);
        }

        internal void ldfld(Field field) {
            buffer.FieldInst(FieldOp.ldfld, field);
        }

        internal void ldsfld(Field field) {
            buffer.FieldInst(FieldOp.ldsfld, field);
        }

        internal void stfld(Field field) {
            buffer.FieldInst(FieldOp.stfld, field);
        }

        internal void stsfld(Field field) {
            buffer.FieldInst(FieldOp.stsfld, field);
        }

        internal void box(PERWAPI.Type t) {
            buffer.TypeInst(TypeOp.box, t);
        }

        internal void br(CILLabel label) {
            buffer.Branch(BranchOp.br, label);
        }

        internal void leave(CILLabel label) {
            buffer.Branch(BranchOp.leave, label);
        }

        internal void brfalse(CILLabel label) {
            buffer.Branch(BranchOp.brfalse, label);
        }

        internal void brtrue(CILLabel label) {
            buffer.Branch(BranchOp.brtrue, label);
        }

        internal void bne(CILLabel label) {
            buffer.Branch(BranchOp.bne_un, label);
        }

        internal void bge(CILLabel label) {
            buffer.Branch(BranchOp.bge, label);
        }

        internal void endfinally() {
            buffer.Inst(Op.endfinally);
        }

        internal void rethrow() {
            buffer.Inst(Op.rethrow);
        }

        internal void throwOp() {
            buffer.Inst(Op.throwOp);
        }


        internal void PushTrue() {
            buffer.PushTrue();
        }

        internal void PushFalse() {
            buffer.PushFalse();
        }

        // -----------------------------------------------------------------

        internal void CodeLabel(CILLabel label) {
            buffer.CodeLabel(label);
        }

        internal CILLabel NewLabel() {
            return buffer.NewLabel();
        }

        internal Stack<Clause> blocks = new Stack<Clause>();

        internal void Goto(CILLabel label) {
            Clause top = Clause.None;
            if (blocks.Count > 0)
                top = blocks.Peek();

            if (top == Clause.Try || top == Clause.Catch)
                leave(label);
            else if (top == Clause.Finally)
                endfinally();
            else
                br(label);
        }

        internal void StartBlock(Clause blockType) {
            blocks.Push(blockType);
            buffer.StartBlock();
        }

        internal TryBlock EndTryBlock() {
            System.Diagnostics.Debug.Assert(blocks.Peek() == Clause.Try);
            blocks.Pop();
            return buffer.EndTryBlock();
        }

        internal void EndCatchBlock(PERWAPI.Class type, TryBlock tryBlock) {
            System.Diagnostics.Debug.Assert(blocks.Peek() == Clause.Catch);
            blocks.Pop();
            buffer.EndCatchBlock(type, tryBlock);
        }

        internal void EndFinallyBlock(TryBlock tryBlock) {
            System.Diagnostics.Debug.Assert(blocks.Peek() == Clause.Finally);
            blocks.Pop();
            buffer.EndFinallyBlock(tryBlock);
        }


        // -----------------------------------------------------------------


        internal void CreateAssembly(string directory, string fileName, string assemblyName, bool GUI) {
            Assembly = new PEFile(fileName, assemblyName);
            Assembly.SetSubSystem(GUI ? SubSystem.Windows_GUI : SubSystem.Windows_CUI);
            Assembly.SetNetVersion(NetVersion.Version2);
            Assembly.GetThisAssembly().AddCustomAttribute(Runtime.RubyAttribute.ctor, new byte[0]);
            Assembly.SetOutputDirectory(directory);
        }

        internal ClassDef CreateNestedClass(ClassDef parent, string name, PERWAPI.Class superType) {
            if (parent == null) {
                if (Assembly.GetClass(name) != null)
                    return Assembly.GetClass(name);
                return Assembly.AddClass(TypeAttr.Public | TypeAttr.BeforeFieldInit, null, name, superType);
            } else {
                if (parent.GetNestedClass(name) != null)
                    return parent.GetNestedClass(name);
                return parent.AddNestedClass(TypeAttr.NestedPublic | TypeAttr.BeforeFieldInit, name, superType);
            }
        }

        internal ClassDef CreateGlobalClass(string nsName, string name, PERWAPI.Class superType) {
            string fullname = name;
            int seq = 1;
            // find a name that hasn't been used
            while (Assembly.GetClass(nsName, fullname) != null)
                fullname = name + (seq++).ToString(CultureInfo.InvariantCulture);

            return Assembly.AddClass(TypeAttr.Public | TypeAttr.BeforeFieldInit, nsName, fullname, superType);
        }

        static int indent = 0;

        internal void Indent() {
            for (int i = 0; i < indent; i++)
                System.Console.Write("\t");
        }

        internal CodeGenContext CreateMethod(ClassDef ParentClass, MethAttr attr, string name, PERWAPI.Type return_type, params Param[] parameters) {
            CodeGenContext newContext = new CodeGenContext(this);

            newContext.Method = ParentClass.AddMethod(attr, ImplAttr.IL, name, return_type, parameters);

            if ((attr & MethAttr.Static) == 0)
                newContext.Method.AddCallConv(CallConv.Instance);

            newContext.CLRLocals = new List<Local>();

            newContext.Method.CreateCodeBuffer();

            newContext.buffer.OpenScope();

            return newContext;
        }

        internal CodeGenContext CreateModuleMethod(string name, PERWAPI.Type return_type, params Param[] parameters) {
            CodeGenContext newContext = new CodeGenContext(this);

            newContext.Method = Assembly.AddMethod(MethAttr.PublicStatic, ImplAttr.IL, name, return_type, parameters);

            newContext.CLRLocals = new List<Local>();

            newContext.Method.CreateCodeBuffer();

            newContext.buffer.OpenScope();

            return newContext;
        }


        internal CodeGenContext CreateConstructor(ClassDef ParentClass, params Param[] parameters) {
            return CreateMethod(ParentClass, MethAttr.HideBySig | MethAttr.SpecialRTSpecialName | MethAttr.Public, ".ctor", PrimitiveType.Void, parameters);
        }

        internal CodeGenContext CreateStaticConstructor(ClassDef ParentClass) {
            return CreateMethod(ParentClass, MethAttr.HideBySig | MethAttr.SpecialRTSpecialName | MethAttr.Private | MethAttr.Static, ".cctor", PrimitiveType.Void);
        }

        internal static FieldDef AddField(ClassDef ParentClass, FieldAttr attr, string fieldName, PERWAPI.Type fieldType) {
            return ParentClass.AddField(attr, fieldName, fieldType);
        }

        // -----------------------------------------------------------------

        internal ClassDef CurrentClass() {
            return (ClassDef)Method.GetParent();
        }

        internal string CurrentMethodName() {
            CustomAttribute methodAttribute = CurrentClass().GetCustomAttributes()[0];
            return System.Text.UnicodeEncoding.UTF8.GetString(methodAttribute.byteVal);
        }

        // BBTAG: corresponds to ruby_frame->orig_func
        internal string OrigFuncName() {
            CustomAttribute methodAttribute = ((ClassDef)orig_func.GetParent()).GetCustomAttributes()[0];
            return System.Text.UnicodeEncoding.UTF8.GetString(methodAttribute.byteVal);
        }

        // -----------------------------------------------------------------

        private Dictionary<PERWAPI.Type, Stack<int>> unused_locals = new Dictionary<PERWAPI.Type, Stack<int>>();
        internal List<int> locals_inuse = new List<int>();


        internal void Close() {
            //indent--;
            //Indent();
            //System.Console.WriteLine("Close {0}", this.Method.QualifiedName());

            Method.SetMaxStack(100);

            Method.AddLocals(CLRLocals.ToArray(), true);

            foreach (Local local in CLRLocals) {
                //System.Console.WriteLine("\t{0} {1} {2}", local.Name, local.type.TypeName(), local.GetIndex());
                buffer.BindLocal(local);
            }

            buffer.CloseScope();

            if (locals_inuse.Count > 0) {
                //foreach (int local in locals_inuse)
                //    System.Console.WriteLine("({0}, {1})", CLRLocals[local].name, CLRLocals[local].type);
                throw new System.Exception("unreleased locals");
            }
        }

        internal int CreateLocal(string name, PERWAPI.Type type) {
            int local;
            if (unused_locals.ContainsKey(type) && unused_locals[type].Count > 0) {
                local = unused_locals[type].Pop();
            } else {
                Local loc = new Local(name, type);
                CLRLocals.Add(loc);
                local = CLRLocals.Count - 1;
            }

            locals_inuse.Add(local);
            return local;
        }

        internal void ReleaseLocal(AST.ISimple temp, bool created) {
            if (temp is AST.LOCAL)
                ReleaseLocal(((AST.LOCAL)temp).local, created);
        }

        internal void ReleaseLocal(int local, bool created) {
            if (created) {
                Debug.Assert(locals_inuse.Contains(local));
                locals_inuse.Remove(local);

                PERWAPI.Type type = CLRLocals[local].type;
                if (!unused_locals.ContainsKey(type))
                    unused_locals[type] = new Stack<int>();


                Debug.Assert(!unused_locals[type].Contains(local));
                unused_locals[type].Push(local);
            }
        }

        internal int StoreInTemp(string name, PERWAPI.Type type, YYLTYPE location) {
            return StoreInLocal(name, type, location).local;
        }

        internal AST.LOCAL StoreInLocal(string name, PERWAPI.Type type, YYLTYPE location) {
            int local = CreateLocal(name, type);
            stloc(local);
            return new AST.LOCAL(local, location);
        }

        internal AST.ISimple PreCompute(AST.Node node, string name, out bool created) {
            return PreCompute(node, name, PrimitiveType.Object, out created);
        }

        internal AST.ISimple PreCompute(AST.Node node, string name, PERWAPI.Type type, out bool created) {
            if (node is AST.ISimple) {
                created = false;
                return (AST.ISimple)node;
            } else {
                created = true;
                node.GenCode(this);
                return StoreInLocal(name, type, node.location);
            }
        }

        internal AST.ISimple PreCompute0(AST.Node node, string name, out bool created) {
            if (node is AST.ISimple) {
                created = false;
                return (AST.ISimple)node;
            } else {
                created = true;
                node.GenCode0(this);
                return StoreInLocal(name, PrimitiveType.Object, node.location);
            }
        }


        internal bool HasArg(ClassRef argType) {
            foreach (Type t in Method.GetParTypes())
                if (argType == t)
                    return true;
            return false;
        }

        internal void ruby_cbase(AST.Scope current) {
            LoadCurrentClass();
        }

        internal void ruby_class(AST.Scope current) {
            ClassDef parent = CurrentClass();
            if (parent.NameSpace() == "_Internal" && parent.Name().StartsWith("Block")) {
                // if (ruby_class == null)
                ldarg("ruby_class");
                CILLabel elseLabel = NewLabel();
                brtrue(elseLabel);
                //    CurrentClass
                LoadCurrentClass();
                CILLabel endLabel = NewLabel();
                br(endLabel);
                CodeLabel(elseLabel);
                // else
                //    ruby_class
                ldarg("ruby_class");
                CodeLabel(endLabel);
            } else
                LoadCurrentClass();
        }

        internal void LoadCurrentClass() {
            if (CurrentRubyClass != null)
                ldsfld(CurrentRubyClass);
            else
                ldsfld(Runtime.Init.rb_cObject);
        }

        internal void LastClass(AST.Scope parent_scope, bool frame) {
            AST.Scope scope_cnt = parent_scope;
            AST.DEFS singletonMethod = null;

            while (!(scope_cnt is AST.CLASS_OR_MODULE) && (scope_cnt != null)) {
                if (scope_cnt is AST.DEFS)
                    singletonMethod = (AST.DEFS)scope_cnt;
                scope_cnt = scope_cnt.parent_scope;
            }

            if (scope_cnt == null) {
                ldsfld(Runtime.Init.rb_cObject);
                return;
            }

            AST.CLASS_OR_MODULE parentClass = (AST.CLASS_OR_MODULE)scope_cnt;

            if (singletonMethod != null) {
                if (singletonMethod.receiver is AST.SELF) {
                    ldsfld(parentClass.singletonField);
                    call(Runtime.Class.CLASS_OF);
                } else {
                    if (frame) {
                        if (singletonMethod.receiver is AST.IVAR) {
                            ldsfld(parentClass.singletonField);
                            ldstr(((AST.IVAR)singletonMethod.receiver).vid);
                            call(Runtime.Eval.ivar_get);
                        } else {
                            singletonMethod.receiver.GenCode(this);
                            call(Runtime.Class.CLASS_OF);
                        }
                    } else {
                        singletonMethod.receiver.GenCode(this);
                        call(Runtime.Class.CLASS_OF);
                    }
                }
            } else
                ldsfld(parentClass.singletonField);
        }


        internal static PERWAPI.FieldRef FindParentClassField(System.Type type) {
            FrameAttribute frame = (FrameAttribute)type.GetCustomAttributes(typeof(FrameAttribute), false)[0];
            if (frame.classname == "")
                return null;
            System.Reflection.Module module = type.Assembly.GetModules(false)[0];
            System.Type sourcefile = module.GetType("_Internal.SourceFile_" + frame.sourcefile);

            FieldInfo field = sourcefile.GetField(frame.classname);
            return FindField(field);
        }


        internal bool Reachable() {
            buffer.EndInstCounter();
            CILInstruction prev = buffer.GetPrevInstruction();
            return !((prev is Instr &&
                    (((Instr)prev).GetOp() == Op.ret ||
                    ((Instr)prev).GetOp() == Op.endfinally ||
                    ((Instr)prev).GetOp() == Op.throwOp)) ||
                    (prev is BranchInstr &&
                    ((int)((BranchInstr)prev).GetOp() == (int)BranchOp.br ||
                     (int)((BranchInstr)prev).GetOp() == (int)BranchOp.br_s ||
                     (int)((BranchInstr)prev).GetOp() == (int)BranchOp.leave_s ||
                     (int)((BranchInstr)prev).GetOp() == (int)BranchOp.leave)));
        }


        // -------------------------------------------------------------------------


        private int FindArg(string name) {
            int seq;
            if ((Method.GetMethAttributes() & MethAttr.Static) != 0)
                seq = 0;
            else
                seq = 1;

            foreach (Param p in Method.GetParams())
                if (p.GetName() == name)
                    return seq;
                else
                    seq++;

            return -1;
        }

        internal static System.Reflection.Assembly ResolveAssembly(object sender, System.ResolveEventArgs args) {
            foreach (System.Reflection.Assembly assembly in loaded) {
                if (assembly.FullName == args.Name)
                    return assembly;
            }
            return null;
        }

        private static AssemblyRef FindAssembly(System.Type type) {
            if (cached.ContainsKey(type.Assembly))
                return cached[type.Assembly];
            else {
                AssemblyName name = type.Assembly.GetName();
                return cached[type.Assembly] = AssemblyRef.MakeAssemblyRef(name.Name, (ushort)name.Version.Major, (ushort)name.Version.Minor, (ushort)name.Version.Build, (ushort)name.Version.Revision, name.GetPublicKeyToken());
            }
        }


        internal static ClassRef FindClass(System.Type type) {
            AssemblyRef assembly = FindAssembly(type);

            ClassRef result;
            if (type.IsNested) {
                ClassRef parent = FindClass(type.DeclaringType);
                result = parent.GetNestedClass(type.Name);
                if (result == null)
                    result = parent.AddNestedClass(type.Name);
            } else {
                result = assembly.GetClass(type.Namespace, type.Name);
                if (result == null)
                    result = assembly.AddClass(type.Namespace, type.Name);
            }

            return result;
        }

        internal static FieldRef FindField(FieldInfo field) {
            ClassRef parent = FindClass(field.DeclaringType);
            FieldRef fieldRef = parent.GetField(field.Name);
            if (fieldRef == null)
                fieldRef = parent.AddField(field.Name, FindClass(field.FieldType));

            return fieldRef;
        }


        internal static Field FindField(PERWAPI.Class klass, string fieldName) {
            if (klass is ClassDef)
                return ((ClassDef)klass).GetField(fieldName);
            else {
                FieldRef f = ((ClassRef)klass).GetField(fieldName);
                if (f == null)
                    f = ((ClassRef)klass).AddField(fieldName, PERWAPI.PrimitiveType.Object);
                return f;
            }
        }

        private static Dictionary<System.Reflection.Assembly, PERWAPI.AssemblyRef> cached = new Dictionary<System.Reflection.Assembly, PERWAPI.AssemblyRef>();

        // --------------------------------------------------------------------------

        private static List<System.Reflection.Assembly> loaded = new List<System.Reflection.Assembly>();


        internal static System.Reflection.Assembly Load(PEFile assembly) {
            MemoryStream binaryStream = new MemoryStream();
            assembly.SetOutputStream(binaryStream);
            assembly.MakeDebuggable(false, false);
            assembly.WritePEFile(false);
            byte[] assemblyBytes = binaryStream.ToArray();

            System.Reflection.Assembly loadedAssembly = System.Reflection.Assembly.Load(assemblyBytes);
            loaded.Add(loadedAssembly);
            return loadedAssembly;
        }
    }


    internal enum Clause { None, Try, Catch, Finally };

    internal class Labels {
        internal CILLabel Redo;
        internal CILLabel Retry;
        internal CILLabel Break;
        internal CILLabel Next;
        internal CILLabel Return;
    }
}
