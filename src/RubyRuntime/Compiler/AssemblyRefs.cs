/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/

using Ruby.Runtime;
using PERWAPI;
using System.Collections.Generic;


namespace Ruby.Compiler
{
    // PERWAPI references to runtime Assemblies, Classes and Methods used by the compiler

    internal class Runtime
    {
        internal static AssemblyRef System = AssemblyRef.MakeAssemblyRef("mscorlib");

        internal static ClassRef SystemStringRef        = AddClass(System, "System", "String");
        internal static ClassRef SystemObjectRef        = AddClass(System, "System", "Object");
        internal static ClassRef SystemTypeRef          = AddClass(System, "System", "Type");
        internal static ClassRef SystemExceptionRef     = AddClass(System, "System", "Exception");
        internal static ClassRef SystemConsoleRef       = AddClass(System, "System", "Console");
        internal static ClassRef SystemIOTextWriterRef  = AddClass(System, "System.IO", "TextWriter");
        internal static ClassRef ParamArrayAttributeRef = AddClass(System, "System", "ParamArrayAttribute");
        internal static ClassRef RuntimeTypeHandleRef   = AddValueClass(System, "System", "RuntimeTypeHandle");

        internal static AssemblyRef Assembly = AssemblyRef.MakeAssemblyRef("Ruby.NET.Runtime", 0, 8, 2, 0,  new byte[] {0x83, 0x45, 0x95, 0xCA, 0x80, 0x23, 0xC3, 0x18} );

        internal static ClassRef ArgListRef             = AddClass(Assembly, "Ruby.Runtime", "ArgList");
        internal static ClassRef ProgramRef             = AddClass(Assembly, "Ruby.Runtime", "Program");
        internal static ClassRef ExceptionRef           = AddClass(Assembly, "Ruby", "Exception");
        internal static ClassRef ArgumentErrorRef       = AddClass(Assembly, "Ruby", "ArgumentError");
        internal static ClassRef BreakExceptionRef      = AddClass(Assembly, "Ruby.Runtime", "BreakException");
        internal static ClassRef RetryExceptionRef      = AddClass(Assembly, "Ruby.Runtime", "RetryException");
        internal static ClassRef ReturnExceptionRef     = AddClass(Assembly, "Ruby.Runtime", "ReturnException");
        internal static ClassRef SystemExitRef          = AddClass(Assembly, "Ruby.Runtime", "SystemExit");

        internal static ClassRef RubyMethodRef          = AddClass(Assembly, "Ruby.Runtime", "RubyMethod");
        internal static ClassRef EvalRef                = AddClass(Assembly, "Ruby.Runtime", "Eval");
        internal static ClassRef FloatRef               = AddClass(Assembly, "Ruby", "Float");
        internal static ClassRef FrameRef               = AddClass(Assembly, "Ruby.Runtime", "Frame");
        internal static ClassRef HashRef                = AddClass(Assembly, "Ruby", "Hash");
        internal static ClassRef InitRef                = AddClass(Assembly, "Ruby.Runtime", "Init");
        internal static ClassRef MatchRef               = AddClass(Assembly, "Ruby", "Match");
        internal static ClassRef MethodBodyRef          = AddClass(Assembly, "Ruby.Runtime", "MethodBody");
        internal static ClassRef ControlExceptionRef    = AddClass(Assembly, "Ruby.Runtime", "ControlException");
        internal static ClassRef CLRExceptionRef        = AddClass(Assembly, "Ruby", "CLRException");
        internal static ClassRef RubyAttributeRef       = AddClass(Assembly, "Ruby.Runtime", "RubyAttribute");
        internal static ClassRef ArrayRef               = AddClass(Assembly, "Ruby", "Array");
        internal static ClassRef BignumRef              = AddClass(Assembly, "Ruby", "Bignum");
        internal static ClassRef ClassRef               = AddClass(Assembly, "Ruby", "Class");
        internal static ClassRef ObjectRef              = AddClass(Assembly, "Ruby", "Object");
        internal static ClassRef ProcRef                = AddClass(Assembly, "Ruby", "Proc");
        internal static ClassRef RangeRef               = AddClass(Assembly, "Ruby", "Range");
        internal static ClassRef RegexpRef              = AddClass(Assembly, "Ruby", "Regexp");
        internal static ClassRef StringRef              = AddClass(Assembly, "Ruby", "String");
        internal static ClassRef SymbolRef              = AddClass(Assembly, "Ruby", "Symbol");
        internal static ClassRef VariablesRef           = AddClass(Assembly, "Ruby.Runtime", "Variables");
        internal static ClassRef BlockRef               = AddClass(Assembly, "Ruby.Runtime", "Block");
        internal static ClassRef RubyExceptionRef       = AddClass(Assembly, "Ruby.Runtime", "RubyException");
        internal static ClassRef IEvalRef               = AddClass(Assembly, "Ruby.Runtime", "IEval");
        internal static ClassRef errinfo_globalRef      = AddClass(Assembly, "Ruby.Runtime", "errinfo_global");
        internal static ClassRef global_variableRef     = AddClass(Assembly, "Ruby.Runtime", "global_variable");
        internal static ClassRef OptionsRef             = AddClass(Assembly, "Ruby.Runtime", "Options");
        internal static ClassRef IORef                  = AddClass(Assembly, "Ruby", "IO");
        internal static ClassRef InteropMethodAttrRef   = AddClass(Assembly, "Ruby.Interop", "InteropMethodAttribute");
        internal static ClassRef FrameAttributeRef      = AddClass(Assembly, "Ruby.Runtime", "FrameAttribute");


        internal static ClassRef MethodBodyNRef(int n)
        {
            ClassRef methodBody = Assembly.GetClass("Ruby.Runtime", "MethodBody" + n);
            if (methodBody != null)
                return methodBody;
            else
                return AddClass(Assembly, "Ruby.Runtime", "MethodBody" + n);
        }

        internal static MethodRef MethodBodyCtor(ClassRef klass)
        {
            MethodRef ctor = klass.GetMethod(".ctor");
            if (ctor != null)
                return ctor;
            else
            {
                ctor = klass.AddMethod(".ctor", PrimitiveType.Void, new Type[0]);
                ctor.AddCallConv(CallConv.Instance);
                return ctor;
            }
        }


        internal static ClassRef AddClass(AssemblyRef assembly, string ns, string name)
        {
            ClassRef klass = assembly.GetClass(ns, name);

            if (klass != null)
                return klass;
            else
                return assembly.AddClass(ns, name);
        }

        internal static ClassRef AddValueClass(AssemblyRef assembly, string ns, string name)
        {
            ClassRef klass = assembly.GetClass(ns, name);

            if (klass != null)
                return klass;
            else
                return assembly.AddValueClass(ns, name);
        }


        internal static MethodRef AddInstanceMethod(ClassRef classRef, string name, PERWAPI.Type retType, PERWAPI.Type[] args)
        {
            MethodRef method = classRef.GetMethod(name, args);

            if (method == null || method.GetCallConv() != CallConv.Instance)
            {
                method = classRef.AddMethod(name, retType, args);
                method.AddCallConv(CallConv.Instance);
            }
            return method;
        }

        internal static MethodRef AddStaticMethod(ClassRef classRef, string name, PERWAPI.Type retType, PERWAPI.Type[] args)
        {
            MethodRef method = classRef.GetMethod(name, args);

            if (method == null)
                method = classRef.AddMethod(name, retType, args);
            
            return method;
        }

        internal static FieldRef AddField(ClassRef classRef, string name, PERWAPI.Type type)
        {
            FieldRef field = classRef.GetField(name);

            if (field == null)
                field = classRef.AddField(name, type);

            return field;
        }

        internal class RubyAttribute
        {
            internal static MethodRef ctor = AddInstanceMethod(RubyAttributeRef, ".ctor", PrimitiveType.Void, new Type[0]);
        }

        internal class InteropMethodAttribute
        {
            internal static MethodRef ctor = AddInstanceMethod(InteropMethodAttrRef, ".ctor", PrimitiveType.Void, new Type[] { PrimitiveType.String });
        }

        internal class FrameAttribute
        {
            internal static MethodRef ctor = AddInstanceMethod(FrameAttributeRef, ".ctor", PrimitiveType.Void, new Type[] { PrimitiveType.String, PrimitiveType.String });
        }
        
        internal class ArgList
        {
            internal static MethodRef ctor = AddInstanceMethod(ArgListRef, ".ctor", PrimitiveType.Void, new Type[0]);
            internal static MethodRef Add = AddInstanceMethod(ArgListRef, "Add", PrimitiveType.Void, new Type[] { PrimitiveType.Object });
            internal static MethodRef AddArray = AddInstanceMethod(ArgListRef, "AddArray", PrimitiveType.Void, new Type[] { PrimitiveType.Object, FrameRef });
            internal static MethodRef CheckSingleRHS = AddInstanceMethod(ArgListRef, "CheckSingleRHS", ArgListRef, new Type[0]);
            internal static MethodRef get_Length = AddInstanceMethod(ArgListRef, "get_Length", PrimitiveType.Int32, new Type[0]);
            internal static MethodRef GetNext = AddInstanceMethod(ArgListRef, "GetNext", PrimitiveType.Object, new Type[0]);
            internal static MethodRef GetRest = AddInstanceMethod(ArgListRef, "GetRest", ArrayRef, new Type[0]);
            internal static MethodRef RunOut = AddInstanceMethod(ArgListRef, "RunOut", PrimitiveType.Boolean, new Type[0]);
            internal static MethodRef ToRubyArray = AddInstanceMethod(ArgListRef, "ToRubyArray", ArrayRef, new Type[0]);
            internal static MethodRef ToRubyObject = AddInstanceMethod(ArgListRef, "ToRubyObject", PrimitiveType.Object, new Type[0]);

            internal static FieldRef block = AddField(ArgListRef, "block", ProcRef);
            internal static FieldRef single_arg = AddField(ArgListRef, "single_arg", PrimitiveType.Boolean);
        }
        
        internal class IO
        {
            internal static MethodRef rb_gets = AddStaticMethod(IORef, "rb_gets", PrimitiveType.Object, new Type[] { FrameRef });
        }
        
        internal class Exception
        {
            internal static MethodRef raise = AddInstanceMethod(ExceptionRef, "raise", RubyExceptionRef, new Type[] { FrameRef });
        }
        
        internal class ArgumentError
        {
            internal static MethodRef ctor = AddInstanceMethod(ArgumentErrorRef, ".ctor", PrimitiveType.Void, new Type[] { PrimitiveType.String });

        }
        
        internal class BreakException
        {
            internal static MethodRef ctor = AddInstanceMethod(BreakExceptionRef, ".ctor", PrimitiveType.Void, new Type[] { PrimitiveType.Object, FrameRef });

            internal static FieldRef scope = AddField(BreakExceptionRef, "scope", FrameRef);
            internal static FieldRef return_value = AddField(BreakExceptionRef, "return_value", PrimitiveType.Object);
        }
        
        internal class RetryException
        {
            internal static MethodRef ctor = AddInstanceMethod(RetryExceptionRef, ".ctor", PrimitiveType.Void, new Type[] { FrameRef });

            internal static FieldRef scope = AddField(RetryExceptionRef, "scope", FrameRef);
        }
        
        internal class ReturnException
        {
            internal static MethodRef ctor = AddInstanceMethod(ReturnExceptionRef, ".ctor", PrimitiveType.Void, new Type[] { PrimitiveType.Object, FrameRef });

            internal static FieldRef scope = AddField(ReturnExceptionRef, "scope", FrameRef);
            internal static FieldRef return_value = AddField(ReturnExceptionRef, "return_value", PrimitiveType.Object);
        }
        
        internal class CLRException
        {
            internal static MethodRef ctor = AddInstanceMethod(CLRExceptionRef, ".ctor", PrimitiveType.Void, new Type[] { FrameRef, SystemExceptionRef });
        }
        
        internal class Array
        {
            internal static MethodRef ctor = AddInstanceMethod(ArrayRef, ".ctor", PrimitiveType.Void, new Type[0]);
            internal static MethodRef includes = AddInstanceMethod(ArrayRef, "includes", PrimitiveType.Boolean, new Type[] { PrimitiveType.Object, FrameRef });
            internal static MethodRef Store = AddStaticMethod(ArrayRef, "Store", ArrayRef, new Type[] { PrimitiveType.Object });
        }
        
        internal class Bignum
        {
            internal static MethodRef ctor = AddInstanceMethod(BignumRef, ".ctor", PrimitiveType.Void, new Type[] { PrimitiveType.Int32, PrimitiveType.String, PrimitiveType.Int32 });
        }

        internal class Class
        {
            internal static MethodRef define_alloc_func = AddInstanceMethod(ClassRef, "define_alloc_func", PrimitiveType.Void, new Type[] { MethodBodyRef }); // BBTAG: attempted fix for class library allocator functions being overwritten
            //internal static MethodRef define_alloc_func = AddInstanceMethod(ClassRef, "define_alloc_func_if_undefined", PrimitiveType.Void, new Type[] { MethodBodyRef }); // BBTAG: attempted fix for class library allocator functions being overwritten
            internal static MethodRef CLASS_OF = AddStaticMethod(ClassRef, "CLASS_OF", ClassRef, new Type[] { PrimitiveType.Object });
            internal static MethodRef define_method = AddInstanceMethod(ClassRef, "define_method", PrimitiveType.Void, new Type[] { PrimitiveType.String, MethodBodyRef, PrimitiveType.Int32, FrameRef });
            internal static MethodRef rb_define_class = AddStaticMethod(ClassRef, "rb_define_class", ClassRef, new Type[] { PrimitiveType.Object, PrimitiveType.String, PrimitiveType.Object, FrameRef });
            internal static MethodRef rb_define_module = AddStaticMethod(ClassRef, "rb_define_module", ClassRef, new Type[] { PrimitiveType.Object, PrimitiveType.String, FrameRef });
            internal static MethodRef singleton_class = AddStaticMethod(ClassRef, "singleton_class", ClassRef, new Type[] { FrameRef, PrimitiveType.Object });
            internal static MethodRef undef_method = AddInstanceMethod(ClassRef, "undef_method", PrimitiveType.Void, new Type[] { PrimitiveType.String });
        }
        
       
        internal class Eval
        {
            internal static MethodRef alias = AddStaticMethod(EvalRef, "alias", PrimitiveType.Object, new Type[] { ClassRef, PrimitiveType.String, PrimitiveType.String, FrameRef });
            internal static MethodRef block_pass = AddStaticMethod(EvalRef, "block_pass", ProcRef, new Type[] { PrimitiveType.Object, FrameRef, });
            internal static MethodRef CallPrivateA = AddStaticMethod(EvalRef, "CallPrivateA", PrimitiveType.Object, new Type[] { PrimitiveType.Object, FrameRef, PrimitiveType.String, ArgListRef, });
            internal static MethodRef CallPublicA = AddStaticMethod(EvalRef, "CallPublicA", PrimitiveType.Object, new Type[] { PrimitiveType.Object, FrameRef, PrimitiveType.String, ArgListRef, });
            internal static MethodRef CallSuperA = AddStaticMethod(EvalRef, "CallSuperA", PrimitiveType.Object, new Type[] { ClassRef, FrameRef, PrimitiveType.Object, PrimitiveType.String, ArgListRef, });
            internal static MethodRef FindSuperMethod = AddStaticMethod(EvalRef, "FindSuperMethod", RubyMethodRef, new Type[] { ClassRef, FrameRef, PrimitiveType.String });
            internal static MethodRef FindPrivateMethod = AddStaticMethod(EvalRef, "FindPrivateMethod", RubyMethodRef, new Type[] { PrimitiveType.Object, FrameRef, PrimitiveType.String });
            internal static MethodRef FindPublicMethod = AddStaticMethod(EvalRef, "FindPublicMethod", RubyMethodRef, new Type[] { PrimitiveType.Object, FrameRef, PrimitiveType.String });
            internal static MethodRef get_const = AddStaticMethod(EvalRef, "get_const", PrimitiveType.Object, new Type[] { PrimitiveType.Object, PrimitiveType.String, FrameRef });
//          internal static MethodRef ivar_defined = AddStaticMethod(EvalRef, "ivar_defined", PrimitiveType.Object, new Type[] { PrimitiveType.String });
            internal static MethodRef ivar_defined = AddStaticMethod(EvalRef, "ivar_defined", PrimitiveType.Object, new Type[] { PrimitiveType.Object, PrimitiveType.String });
            internal static MethodRef ivar_get = AddStaticMethod(EvalRef, "ivar_get", PrimitiveType.Object, new Type[] { PrimitiveType.Object, PrimitiveType.String });
            internal static MethodRef ivar_set = AddStaticMethod(EvalRef, "ivar_set", PrimitiveType.Object, new Type[] { FrameRef, PrimitiveType.Object, PrimitiveType.String, PrimitiveType.Object });
            internal static MethodRef Return = AddStaticMethod(EvalRef, "Return", PrimitiveType.Object, new Type[] { PrimitiveType.Object, FrameRef, });
            internal static MethodRef set_const = AddStaticMethod(EvalRef, "set_const", PrimitiveType.Object, new Type[] { FrameRef, PrimitiveType.Object, PrimitiveType.String, PrimitiveType.Object });
            internal static MethodRef const_defined = AddStaticMethod(EvalRef, "const_defined", PrimitiveType.Object, new Type[] { PrimitiveType.Object, PrimitiveType.String, FrameRef });
            internal static MethodRef Test = AddStaticMethod(EvalRef, "Test", PrimitiveType.Boolean, new Type[] { PrimitiveType.Object });
            internal static MethodRef Calln = AddStaticMethod(EvalRef, "Call", PrimitiveType.Object, new Type[] { PrimitiveType.Object, PrimitiveType.String, new PERWAPI.ZeroBasedArray(PrimitiveType.Object) });

            internal static FieldRef ruby_errinfo = AddField(EvalRef, "ruby_errinfo", errinfo_globalRef);


            internal static MethodRef Call(string protection, int n)
            {
                MethodRef method = EvalRef.GetMethod("Call" + protection + n);

                if (method != null)
                    return method;
                else
                {
                    List<Type> args = new List<Type>();
                    args.Add(PrimitiveType.Object);
                    args.Add(FrameRef);
                    args.Add(PrimitiveType.String);
                    args.Add(ProcRef);
                    for (int i=0; i<n; i++)
                        args.Add(PrimitiveType.Object);

                    return AddStaticMethod(EvalRef, "Call" + protection + n, PrimitiveType.Object, args.ToArray());
                }
            }

            internal static MethodRef Call(int n)
            {
                MethodRef method = EvalRef.GetMethod("Call" + n);

                if (method != null)
                    return method;
                else
                {
                    List<Type> args = new List<Type>();
                    args.Add(PrimitiveType.Object);
                    args.Add(PrimitiveType.String);
                    for (int i = 0; i < n; i++)
                        args.Add(PrimitiveType.Object);

                    return AddStaticMethod(EvalRef, "Call" + n, PrimitiveType.Object, args.ToArray());
                }
            }
        }
        
        internal class Float
        {
            internal static MethodRef ctor = AddInstanceMethod(FloatRef, ".ctor", PrimitiveType.Void, new Type[] { PrimitiveType.Float64 });
        }
        
        internal class Frame
        {
            internal static MethodRef ctor = AddInstanceMethod(FrameRef, ".ctor", PrimitiveType.Void, new Type[] { FrameRef });
            internal static MethodRef GetDynamic = AddInstanceMethod(FrameRef, "GetDynamic", PrimitiveType.Object, new Type[] { PrimitiveType.String });
            internal static MethodRef SetDynamic = AddInstanceMethod(FrameRef, "SetDynamic", PrimitiveType.Void, new Type[] { PrimitiveType.String, PrimitiveType.Object });
            internal static MethodRef get_Tilde = AddInstanceMethod(FrameRef, "get_Tilde", MatchRef, new Type[0]);
            internal static MethodRef set_Tilde = AddInstanceMethod(FrameRef, "set_Tilde", PrimitiveType.Void, new Type[] { MatchRef });

            internal static FieldRef line = AddField(FrameRef, "line", PrimitiveType.Int32);
            internal static FieldRef block_arg = AddField(FrameRef, "block_arg", ProcRef);
            internal static FieldRef current_block = AddField(FrameRef, "current_block", BlockRef);
        }
        
        internal class Hash
        {
            internal static MethodRef ctor = AddInstanceMethod(HashRef, ".ctor", PrimitiveType.Void, new Type[0]);
            internal static MethodRef Add = AddInstanceMethod(HashRef, "Add", PrimitiveType.Void, new Type[] { PrimitiveType.Object, PrimitiveType.Object });
        }
        
        internal class Init
        {
            internal static FieldRef rb_cObject = AddField(InitRef, "rb_cObject", ClassRef);
        }
        
        internal class Match
        {
            internal static MethodRef defined_nth = AddInstanceMethod(MatchRef, "defined_nth", PrimitiveType.Boolean, new Type[] { PrimitiveType.Int32 });
            internal static MethodRef get_nth = AddInstanceMethod(MatchRef, "get_nth", PrimitiveType.Object, new Type[] { PrimitiveType.Int32 });
            internal static MethodRef last_match = AddInstanceMethod(MatchRef, "last_match", PrimitiveType.Object, new Type[] { FrameRef });
            internal static MethodRef match_last = AddInstanceMethod(MatchRef, "match_last", PrimitiveType.Object, new Type[] { FrameRef });
            internal static MethodRef match_post = AddInstanceMethod(MatchRef, "match_post", PrimitiveType.Object, new Type[] { FrameRef });
            internal static MethodRef match_pre = AddInstanceMethod(MatchRef, "match_pre", PrimitiveType.Object, new Type[] { FrameRef });
        }
        
        internal class Block
        {
            internal static MethodRef ctor = AddInstanceMethod(BlockRef, ".ctor", PrimitiveType.Void, new Type[] { FrameRef });

            internal static FieldRef defining_scope = AddField(BlockRef, "defining_scope", FrameRef);

        }
        
        
        internal class Object
        {
            internal static MethodRef ctor = AddInstanceMethod(ObjectRef, ".ctor", PrimitiveType.Void, new Type[] { ClassRef });

            internal static FieldRef ruby_top_self = AddField(ObjectRef, "ruby_top_self", PrimitiveType.Object);
        }
        
        internal class Proc
        {
            internal static MethodRef ctor = AddInstanceMethod(ProcRef, ".ctor", PrimitiveType.Void, new Type[] { PrimitiveType.Object, ProcRef, MethodBodyRef, PrimitiveType.Int32 });
            internal static MethodRef yield = AddInstanceMethod(ProcRef, "yield", PrimitiveType.Object, new Type[] { FrameRef, ArgListRef });
        }
        
        internal class Range
        {
            internal static MethodRef ctor = AddInstanceMethod(RangeRef, ".ctor", PrimitiveType.Void, new Type[] { PrimitiveType.Object, PrimitiveType.Object, PrimitiveType.Boolean });
        }
        
        internal class Regexp
        {
            internal static MethodRef ctor = AddInstanceMethod(RegexpRef, ".ctor", PrimitiveType.Void, new Type[] { PrimitiveType.String, PrimitiveType.Int32 });
        }
        
        internal class String
        {
            internal static MethodRef ctor = AddInstanceMethod(StringRef, ".ctor", PrimitiveType.Void, new Type[] { PrimitiveType.String });
            internal static MethodRef Concat = AddInstanceMethod(StringRef, "Concat", StringRef, new Type[] { StringRef });
            internal static MethodRef ObjectAsString = AddStaticMethod(StringRef, "ObjectAsString", StringRef, new Type[] { PrimitiveType.Object, FrameRef });
            internal static new MethodRef ToString = AddInstanceMethod(StringRef, "ToString", PrimitiveType.String, new Type[0]);

            internal static FieldRef value = AddField(StringRef, "value", PrimitiveType.String);
        }
        
        internal class Symbol
        {
            internal static MethodRef ctor = AddInstanceMethod(SymbolRef, ".ctor", PrimitiveType.Void, new Type[] { PrimitiveType.String });
        }
        
        internal class Program
        {
            internal static MethodRef End = AddStaticMethod(ProgramRef, "End", PrimitiveType.Object, new Type[] { ProcRef });
            internal static MethodRef ruby_stop = AddStaticMethod(ProgramRef, "ruby_stop", PrimitiveType.Void, new Type[0]);
            internal static MethodRef AddProgram = AddStaticMethod(ProgramRef, "AddProgram", PrimitiveType.Void, new Type[] { PrimitiveType.String, SystemTypeRef });
        }
        
        internal class Variables
        {
            internal static MethodRef alias_variable = AddStaticMethod(VariablesRef, "alias_variable", PrimitiveType.Object, new Type[] { PrimitiveType.String, PrimitiveType.String });
            internal static MethodRef cvar_defined = AddStaticMethod(VariablesRef, "cvar_defined", PrimitiveType.Object, new Type[] { ClassRef, PrimitiveType.String });
            internal static MethodRef cvar_get = AddStaticMethod(VariablesRef, "cvar_get", PrimitiveType.Object, new Type[] { FrameRef, ClassRef, PrimitiveType.String });
            internal static MethodRef cvar_set = AddStaticMethod(VariablesRef, "cvar_set", PrimitiveType.Object, new Type[] { FrameRef, ClassRef, PrimitiveType.String, PrimitiveType.Object });
            internal static MethodRef gvar_defined = AddStaticMethod(VariablesRef, "gvar_defined", PrimitiveType.Object, new Type[] { PrimitiveType.String });
            internal static MethodRef gvar_get = AddStaticMethod(VariablesRef, "gvar_get", PrimitiveType.Object, new Type[] { PrimitiveType.String, FrameRef });
            internal static MethodRef gvar_set = AddStaticMethod(VariablesRef, "gvar_set", PrimitiveType.Object, new Type[] { PrimitiveType.String, PrimitiveType.Object, FrameRef });
        }
        
        internal class global_variable
        {
            internal static FieldRef value = AddField(global_variableRef, "value", PrimitiveType.Object);
        }

        internal class RubyException
        {
            internal static FieldRef parent = AddField(RubyExceptionRef, "parent", ExceptionRef);
        }
        
        internal class Options
        {
            internal static MethodRef SetRuntimeOption = AddStaticMethod(OptionsRef, "SetRuntimeOption", PrimitiveType.Void, new Type[] { PrimitiveType.String, PrimitiveType.Object });
            internal static MethodRef SetArgs = AddStaticMethod(OptionsRef, "SetArgs", PrimitiveType.Void, new Type[] { new ZeroBasedArray(PrimitiveType.String) });
        }


        internal class ParamArrayAttribute
        {
            internal static MethodRef ctor = AddInstanceMethod(ParamArrayAttributeRef, ".ctor", PrimitiveType.Void, new Type[0]);
        }
        
        internal class SystemString
        {
            internal static MethodRef Format = AddStaticMethod(SystemStringRef, "Format", PrimitiveType.String, new Type[] { PrimitiveType.String, PrimitiveType.Object, PrimitiveType.Object });
        }
        
        internal class SystemException
        {
            internal static MethodRef get_Message = AddInstanceMethod(SystemExceptionRef, "get_Message", PrimitiveType.String, new Type[0]);
            internal static MethodRef get_StackTrace = AddInstanceMethod(SystemExceptionRef, "get_StackTrace", PrimitiveType.String, new Type[0]);
            internal static new MethodRef ToString = AddInstanceMethod(SystemExceptionRef, "ToString", PrimitiveType.String, new Type[0]);
        }
        
        internal class SystemConsole
        {
            internal static MethodRef WriteLine = AddStaticMethod(SystemConsoleRef, "WriteLine", PrimitiveType.Void, new Type[] { PrimitiveType.String, PrimitiveType.Object });
            internal static MethodRef get_Error = AddStaticMethod(SystemConsoleRef, "get_Error", SystemIOTextWriterRef, new Type[0]);
        }
        
        internal class SystemIOTextWriter
        {
            internal static MethodRef WriteLine = AddInstanceMethod(SystemIOTextWriterRef, "WriteLine", PrimitiveType.Void, new Type[] { PrimitiveType.String, PrimitiveType.Object });

        }
        
        internal class SystemObject
        {
            internal static MethodRef ctor = AddInstanceMethod(SystemObjectRef, ".ctor", PrimitiveType.Void, new Type[0]);
        }

        internal class SystemType
        {
            internal static MethodRef GetTypeFromHandle = AddStaticMethod(SystemTypeRef, "GetTypeFromHandle", SystemTypeRef, new Type[] { RuntimeTypeHandleRef });
        }
    }
}
