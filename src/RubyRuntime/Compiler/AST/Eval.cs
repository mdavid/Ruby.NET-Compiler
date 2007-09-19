/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/

using Ruby.Runtime;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Text;
using PERWAPI;


namespace Ruby.Compiler.AST
{

    internal class EVAL : BLOCK
    {
        public ClassDef evalClass;
        private Frame frame;
        ClassRef block_type;

        internal EVAL(Scope current, Frame frame, YYLTYPE location): base(current, location)
        {
            this.frame = frame;
            if (frame.current_block != null)
                block_type = CodeGenContext.FindClass(frame.current_block.GetType());
        }

        internal override void Init(YYLTYPE location, params object[] input)
        {
            this.location = location;
            body = (Node)input[0];
        }



        internal override VAR create_local_here(string id, YYLTYPE location)
        {
            if (frame.dynamic_vars == null)
                frame.dynamic_vars = new Dictionary<string, object>();

            if (!frame.dynamic_vars.ContainsKey(id))
                frame.dynamic_vars.Add(id, null);

            return new DynamicLocalVar(id, this, location);
        }


        internal override bool defined_dynamically(string id)
        {
            return frame.dynamic_vars != null && frame.dynamic_vars.ContainsKey(id);
        }


        internal PERWAPI.PEFile GenerateCode(Field CurrentRubyClass)
        {
            CodeGenContext context = new CodeGenContext();
            context.CurrentRubyClass = CurrentRubyClass;

            string name = "Eval_" + System.Guid.NewGuid().ToString("N");

            context.CreateAssembly("./", name + ".dll", name);

            GenerateClassForMethod(context);
                      
            return context.Assembly;
        }


        protected new PERWAPI.MethodDef GenerateClassForMethod(CodeGenContext context)
        {
            // public class Eval: IEval {
            evalClass = context.CreateGlobalClass("_Internal", "Eval", Runtime.SystemObjectRef);
            evalClass.AddImplementedInterface(Runtime.IEvalRef);

            if (context.CurrentRubyClass == null)
            {
                context.CurrentRubyClass = CodeGenContext.AddField(evalClass, PERWAPI.FieldAttr.PublicStatic, "myRubyClass", Runtime.ClassRef);

                CodeGenContext cctor = context.CreateStaticConstructor(evalClass);

                cctor.ldsfld(Runtime.Init.rb_cObject);
                cctor.stsfld(context.CurrentRubyClass);
                cctor.ret();
                cctor.Close();
            }

            MethodDef constructor = GenConstructor(evalClass, context);

            GenInvokeMethod(evalClass, context);

            return constructor;
            // }
        }


        private MethodDef GenConstructor(ClassDef Class, CodeGenContext context)
        {
            // internal Eval(MyBlock block) { ... }
            CodeGenContext constructor;
            if (block_type != null)
                constructor = context.CreateConstructor(Class, new Param[] { new Param(ParamAttr.Default, "frame", block_type) });
            else
                constructor = context.CreateConstructor(Class);

            constructor.ldarg(0); // this
            constructor.call(Runtime.SystemObject.ctor);

            if (block_type != null)
            {
                for (int depth = 0; true; depth++)
                {
                    FieldInfo field = frame.current_block.GetType().GetField("locals" + depth);

                    if (field == null)
                        break;

                    Field oldField = CodeGenContext.FindField(field);

                    FieldDef newField = CodeGenContext.AddField(Class, FieldAttr.Public, field.Name, CodeGenContext.FindClass(field.FieldType));

                    frameFields.Add(newField);

                    // this.myField = frame.myField
                    constructor.ldarg(0);
                    constructor.ldarg("frame");
                    constructor.ldfld(oldField);
                    constructor.stfld(newField);
                }
            }

            constructor.ret();

            constructor.Close();

            return constructor.Method;
        }


        private void GenInvokeMethod(ClassDef Class, CodeGenContext context)
        {
            // internal object Invoke(Class last_class, object recv, Frame caller, Frame frame);
            CodeGenContext Invoke = context.CreateMethod(Class, MethAttr.PublicVirtual, "Invoke", PrimitiveType.Object,
                    new Param(ParamAttr.Default, "last_class", Runtime.ClassRef),
                    new Param(ParamAttr.Default, "recv", PrimitiveType.Object),
                    new Param(ParamAttr.Default, "caller", Runtime.FrameRef),
                    new Param(ParamAttr.Default, "frame", Runtime.FrameRef));


            Invoke.ldarg("frame");
            Invoke.cast(FrameClass);
            int frame = Invoke.StoreInTemp("frame", FrameClass, location);

            AddScopeBody(Invoke);

            Invoke.ReleaseLocal(frame, true);

            Invoke.Close();
        }



        internal object ExecuteInit(PERWAPI.PEFile Assembly, Ruby.Class klass, object recv, Frame caller, Frame frame)
        {
            System.Reflection.Assembly loadedAssembly = CodeGenContext.Load(Assembly);
            System.Type EvalType = loadedAssembly.GetType("_Internal.Eval");
            ConstructorInfo constructor = EvalType.GetConstructors()[0];

            if (klass != null)
            {
                FieldInfo currentClassField = EvalType.GetField("myRubyClass");
                currentClassField.SetValue(null, klass);
            }
            
            IEval evalObj;
            try
            {
                if (frame.current_block != null)
                    evalObj = (IEval)constructor.Invoke(new object[] { frame.current_block });
                else
                    evalObj = (IEval)constructor.Invoke(new object[0]);
            }
            catch (System.Reflection.TargetInvocationException exception)
            {
                throw exception.InnerException;
            }

            return evalObj.Invoke(klass, recv, caller, frame);
        }
    }
}