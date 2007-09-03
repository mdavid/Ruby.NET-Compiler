/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using System.Collections.Generic;
using Ruby.Methods;
using Ruby.Runtime;
using System.Globalization;

namespace Ruby.Runtime
{
    internal enum Receiver 
    { 
        Explicit = 0,   // explicit receiver specified
        Self     = 1,   // implicit receiver (function call)
        Virtual  = 2,   // local variable or method call
        Super    = 3    // super class method call
    }

    internal enum CallStatus { Private, Protected, VCall, Super, None };

    // Ruby.Eval contains the implementation of Ruby method calling
    
    
    [UsedByRubyCompiler]
    public class Eval
    {
        // ------------------------------------------------------------------------------
        [UsedByRubyCompiler]
        public static errinfo_global ruby_errinfo = new errinfo_global();   // $!

        internal static readonly_global rb_load_path = new readonly_global(); // $:
        internal static readonly_global rb_features = new readonly_global();  // $"
        internal static errat_global errat = new errat_global();              // $@
        internal static safe_global safe = new safe_global();                 // $SAFE
        internal static lastline_global rb_lastline = new lastline_global();  // $_
        internal static ThreadGroup thgroup_default;                          // ThreadGroup.Default
        internal static Thread curr_thread, main_thread;     
                                                           
                                                           

        private static bool rubyRunning = false;

        internal static int ruby_safe_level = 0;           // BBTAG: also need a safe level in current thread; WARTAG: done
        internal const int SAFE_LEVEL_MAX = 4;             // BBTAG: see CRuby: what is PROC_TMASK?

        // -----------------------------------------------------------------------------

        internal static bool RubyRunning
        {
            get { return Eval.rubyRunning; }
            set { Eval.rubyRunning = value; }
        }

        [UsedByRubyCompiler]
        public static object Return(object value, Frame caller)
        {
            Array array;
            if (Array.TryToArray(value, out array, caller))
            {
                int length = array.Count;

                if (length == 0)
                    return null;
                if (length == 1)
                    return array[0];
            }
           
            return value;
        }


        #region Interop

        private static Frame dummyFrame = new DummyFrame();

        public static object Call0(object recv, string methodId)
        {
            return CallPublic0(recv, dummyFrame, methodId, null);
        }

        public static object Call1(object recv, string methodId, object arg1)
        {
            return CallPublic1(recv, dummyFrame, methodId, null, arg1);
        }

        public static object Call2(object recv, string methodId, object arg1, object arg2)
        {
            return CallPublic2(recv, dummyFrame, methodId, null, arg1, arg2);
        }

        public static object Call3(object recv, string methodId, object arg1, object arg2, object arg3)
        {
            return CallPublic3(recv, dummyFrame, methodId, null, arg1, arg2, arg3);
        }

        public static object Call4(object recv, string methodId, object arg1, object arg2, object arg3, object arg4)
        {
            return CallPublic4(recv, dummyFrame, methodId, null, arg1, arg2, arg3, arg4);
        }

        public static object Call5(object recv, string methodId, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            return CallPublic5(recv, dummyFrame, methodId, null, arg1, arg2, arg3, arg4, arg5);
        }

        public static object Call6(object recv, string methodId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
        {
            return CallPublic6(recv, dummyFrame, methodId, null, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public static object Call7(object recv, string methodId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
        {
            return CallPublic7(recv, dummyFrame, methodId, null, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public static object Call8(object recv, string methodId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
        {
            return CallPublic8(recv, dummyFrame, methodId, null, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public static object Call9(object recv, string methodId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
        {
            return CallPublic9(recv, dummyFrame, methodId, null, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }


        public static object Call(object recv, string methodId, params object[] args)
        {
            return CallPublic(recv, dummyFrame, methodId, null, args);
        }
        #endregion


        #region FixedArgCases
        [UsedByRubyCompiler]
        public static object CallPublic0(object recv, Frame caller, string methodId, Proc block)
        {
            Class origin;
            RubyMethod method = FindPublicMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call0(origin, recv, caller, block);
            else
                return method_missing(recv, caller, methodId, new ArgList(block));
        }

        [UsedByRubyCompiler]
        public static object CallPublic1(object recv, Frame caller, string methodId, Proc block, object arg1)
        {
            Class origin;
            RubyMethod method = FindPublicMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call1(origin, recv, caller, block, arg1);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1));
        }

        [UsedByRubyCompiler]
        public static object CallPublic2(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2)
        {
            Class origin;
            RubyMethod method = FindPublicMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call2(origin, recv, caller, block, arg1, arg2);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1));
        }

        [UsedByRubyCompiler]
        public static object CallPublic3(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3)
        {
            Class origin;
            RubyMethod method = FindPublicMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call3(origin, recv, caller, block, arg1, arg2, arg3);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1));
        }

        [UsedByRubyCompiler]
        public static object CallPublic4(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3, object arg4)
        {
            Class origin;
            RubyMethod method = FindPublicMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call4(origin, recv, caller, block, arg1, arg2, arg3, arg4);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1, arg2, arg3, arg4));
        }

        [UsedByRubyCompiler]
        public static object CallPublic5(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            Class origin;
            RubyMethod method = FindPublicMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call5(origin, recv, caller, block, arg1, arg2, arg3, arg4, arg5);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1, arg2, arg3, arg4, arg5));
        }

        [UsedByRubyCompiler]
        public static object CallPublic6(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
        {
            Class origin;
            RubyMethod method = FindPublicMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call6(origin, recv, caller, block, arg1, arg2, arg3, arg4, arg5, arg6);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1, arg2, arg3, arg4, arg5, arg6));
        }

        [UsedByRubyCompiler]
        public static object CallPublic7(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
        {
            Class origin;
            RubyMethod method = FindPublicMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call7(origin, recv, caller, block, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1, arg2, arg3, arg4, arg5, arg6, arg7));
        }

        [UsedByRubyCompiler]
        public static object CallPublic8(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
        {
            Class origin;
            RubyMethod method = FindPublicMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call8(origin, recv, caller, block, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
        }

        [UsedByRubyCompiler]
        public static object CallPublic9(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
        {
            Class origin;
            RubyMethod method = FindPublicMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call9(origin, recv, caller, block, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9));
        }
        #endregion

        [UsedByRubyCompiler]
        public static object CallPublic(object recv, Frame caller, string methodId, Proc block, params object[] args)
        {
            return CallPublicA(recv, caller, methodId, new ArgList(block, args)); 
        }


        [UsedByRubyCompiler]
        public static object CallPublicA(object recv, Frame caller, string methodId, ArgList args)
        {
            Class origin;
            RubyMethod method = FindPublicMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Calln(origin, recv, caller, args);
            else
                return method_missing(recv, caller, methodId, args);
        }


        #region FixedArgCases
        
        [UsedByRubyCompiler]
        public static object CallPrivate0(object recv, Frame caller, string methodId, Proc block)
        {
            Class origin;
            RubyMethod method = FindPrivateMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call0(origin, recv, caller, block);
            else
                return method_missing(recv, caller, methodId, new ArgList(block));
        }

        [UsedByRubyCompiler]
        public static object CallPrivate1(object recv, Frame caller, string methodId, Proc block, object arg1)
        {
            Class origin;
            RubyMethod method = FindPrivateMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call1(origin, recv, caller, block, arg1);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1));
        }

        [UsedByRubyCompiler]
        public static object CallPrivate2(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2)
        {
            Class origin;
            RubyMethod method = FindPrivateMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call2(origin, recv, caller, block, arg1, arg2);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1));
        }

        [UsedByRubyCompiler]
        public static object CallPrivate3(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3)
        {
            Class origin;
            RubyMethod method = FindPrivateMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call3(origin, recv, caller, block, arg1, arg2, arg3);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1));
        }

        [UsedByRubyCompiler]
        public static object CallPrivate4(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3, object arg4)
        {
            Class origin;
            RubyMethod method = FindPrivateMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call4(origin, recv, caller, block, arg1, arg2, arg3, arg4);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1, arg2, arg3, arg4));
        }

        [UsedByRubyCompiler]
        public static object CallPrivate5(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            Class origin;
            RubyMethod method = FindPrivateMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call5(origin, recv, caller, block, arg1, arg2, arg3, arg4, arg5);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1, arg2, arg3, arg4, arg5));
        }

        [UsedByRubyCompiler]
        public static object CallPrivate6(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
        {
            Class origin;
            RubyMethod method = FindPrivateMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call6(origin, recv, caller, block, arg1, arg2, arg3, arg4, arg5, arg6);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1, arg2, arg3, arg4, arg5, arg6));
        }

        [UsedByRubyCompiler]
        public static object CallPrivate7(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
        {
            Class origin;
            RubyMethod method = FindPrivateMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call7(origin, recv, caller, block, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1, arg2, arg3, arg4, arg5, arg6, arg7));
        }

        [UsedByRubyCompiler]
        public static object CallPrivate8(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
        {
            Class origin;
            RubyMethod method = FindPrivateMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call8(origin, recv, caller, block, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
        }

        [UsedByRubyCompiler]
        public static object CallPrivate9(object recv, Frame caller, string methodId, Proc block, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
        {
            Class origin;
            RubyMethod method = FindPrivateMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Call9(origin, recv, caller, block, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            else
                return method_missing(recv, caller, methodId, new ArgList(block, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9));
        }
        #endregion

        [UsedByRubyCompiler]
        public static object CallPrivate(object recv, Frame caller, string methodId, Proc block, params object[] args)
        {
            return CallPrivateA(recv, caller, methodId, new ArgList(block, args));
        }

        [UsedByRubyCompiler]
        public static object CallPrivateA(object recv, Frame caller, string methodId, ArgList args)
        {
            Class origin;
            RubyMethod method = FindPrivateMethod(recv, caller, methodId, out origin);
            if (method != null)
                return method.body.Calln(origin, recv, caller, args);
            else
                return method_missing(recv, caller, methodId, args);
        }


        [UsedByRubyCompiler]
        public static object CallSuperA(Class klass, Frame caller, object recv, string methodId, ArgList args)
        {
            Class origin;
            RubyMethod method = FindSuperMethod(klass, caller, methodId, out origin);

            if (method != null)
                return method.body.Calln(origin, recv, caller, args);
            else
            {
                caller.call_status = CallStatus.Super;
                return method_missing(recv, caller, methodId, args);
            }
        }


        internal static RubyMethod FindSuperMethod(Class klass, Frame caller, string methodId, out Class origin)
        {
            return FindMethodForClass(klass.super, Receiver.Super, caller, methodId, out origin);
        }

        internal static RubyMethod FindPrivateMethod(object recv, Frame caller, string methodId, out Class origin)
        {
            return FindMethodForClass(Class.CLASS_OF(recv), Receiver.Self, caller, methodId, out origin);
        }


        internal static RubyMethod FindPublicMethod(object recv, Frame caller, string methodId, out Class origin)
        {
            return FindMethodForClass(Class.CLASS_OF(recv), Receiver.Explicit, caller, methodId, out origin);
        }



        [UsedByRubyCompiler]
        public static RubyMethod FindSuperMethod(Class klass, Frame caller, string methodId)
        {
            Class origin;
            return FindMethodForClass(klass.super, Receiver.Super, caller, methodId, out origin);
        }

        [UsedByRubyCompiler]
        public static RubyMethod FindPrivateMethod(object recv, Frame caller, string methodId)
        {
            Class origin;
            return FindMethodForClass(Class.CLASS_OF(recv), Receiver.Self, caller, methodId, out origin);
        }

        [UsedByRubyCompiler]
        public static RubyMethod FindPublicMethod(object recv, Frame caller, string methodId)
        {
            Class origin;
            return FindMethodForClass(Class.CLASS_OF(recv), Receiver.Explicit, caller, methodId, out origin);
        }




        internal static RubyMethod FindMethodForClass(Class klass, Receiver receiverStyle, Frame caller, string methodId, out Class origin)
        {
            RubyMethod method;

            if (klass == null)
            {
                origin = null;
                return null;
            }

            if (klass.get_method(methodId, out method, out origin) && method != null)
            {
                if (method is MethodAlias)
                    origin = method.definingClass;

                if (method.access == Access.Private && receiverStyle == Receiver.Explicit)
                {
                    if (caller != null)
                        caller.call_status = CallStatus.Private;
                    return null;
                }

                if (method.access == Access.Protected && receiverStyle == Receiver.Explicit)
                {
                    Class outerScope = Ruby.Runtime.Init.rb_cObject;

                    if (caller != null)
                    {
                        Class[] nesting = caller.nesting();
                        if (nesting != null && nesting.Length > 0)
                            outerScope = nesting[0];
                    }

                    if (!outerScope.is_kind_of(method.definingClass))
                    {
                        if (caller != null)
                            caller.call_status = CallStatus.Protected;
                        return null;
                    }
                }

                return method;
            }
            else
            {
                if (caller != null)
                    caller.call_status = CallStatus.VCall;
                return null;
            }
        }

        internal static object method_missing(object recv, Frame caller, string methodId, ArgList args)
        {
            ArgList newargs = new ArgList(args.block);
            newargs.Add(methodId);
            newargs.AddRange(args);

            Class origin;
            RubyMethod method = FindPrivateMethod(recv, caller, "method_missing", out origin);
            if (method != null)
                return method.body.Calln(origin, recv, caller, newargs);
            else
                throw new NoMethodError("method_missing method missing").raise(caller);
        }

        [UsedByRubyCompiler]
        public static object ivar_get(object obj, string id)
        {
            if (obj is Object)
                return ((Object)obj).instance_variable_get(id);
            else
                // Fixme: only if EXIVAR or special ...

                // Lookaside
                return Object.generic_ivar_get(obj, id);
        }

        [UsedByRubyCompiler]
        public static object ivar_set(Frame caller, object obj, string id, object value)
        {
            if (obj is Object)
            {
                Object o = (Object)obj;
                if (!o.Tainted && Eval.rb_safe_level() >= 4)
                    throw new SecurityError("Insecure: can't modify instance variable").raise(caller);
                if (o.Frozen)
                    throw TypeError.rb_error_frozen(caller, "object").raise(caller);

                o.instance_variable_set(id, value);
            }
            else
            {
                // Lookaside
                Object.generic_ivar_set(obj, id, value);
            }
            return value;
        }

        [UsedByRubyCompiler]
        public static object ivar_defined(object obj, string id)
        {
            if (obj is Object)
                return ((Object)obj).instance_vars.ContainsKey(id);
            else
            {
                // Lookaside
                return Object.generic_ivar_defined(obj, id);
            }
        }

        // BBTAG: helper method
        internal static object const_get_defined(Class current, string id, Frame caller, bool get)
        {
            if (current.const_defined(id, false))
            {
                if (get)
                    return current.const_get(id, caller);
                else
                    return true;
            }

            foreach (Class klass in caller.nesting())
            {
                if (klass.const_defined(id, false))
                {
                    if (get)
                        return klass.const_get(id, caller);
                    else
                        return true;
                }
            }

            if (get)
                return current.const_get(id, caller);
            else
                return current.const_defined(id, true);
        }

        [UsedByRubyCompiler]
        public static object const_defined(object current, string id, Frame caller)
        {
            return const_get_defined((Class)current, id, caller, false);
        }

        [UsedByRubyCompiler]
        public static object get_const(object current, string id, Frame caller)
        {
            return const_get_defined((Class)current, id, caller, true);
        }

        [UsedByRubyCompiler]
        public static object set_const(Frame caller, object scope, string id, object value)
        {
            Class klass = (Class)scope;
            if (!klass.Tainted && rb_safe_level() >= 4)
                throw new SecurityError("Insecure: can't set constant").raise(caller);
            if (klass.Frozen)
            {
                if (klass._type == Class.Type.Module)
                    throw TypeError.rb_error_frozen(caller, "module").raise(caller);
                else
                    throw TypeError.rb_error_frozen(caller, "class").raise(caller);
            }
            return ((Class)scope).const_set(id, value);
        }

        [UsedByRubyCompiler]
        public static Proc block_pass(object arg, Frame caller)
        {
            if (arg is Proc)
                return (Proc)arg;

            if (arg == null)
                return null;

            object result = Eval.CallPrivate(arg, caller, "to_proc", null);
            
            if (result is Proc)
                return (Proc)result;
            else
                throw new System.Exception("wrong argument type (expected Proc)");
        }


        internal static int object_id(object obj)
        {
            if (obj == null)
                return 4;
            
            if (obj is int)
                return ((int)obj) * 2 + 1;
            
            if (obj is bool)
                if ((bool)obj)
                    return 2;
                else
                    return 0;

            //if (obj is Symbol)
            //    return ((Symbol)obj).id;

            return obj.GetHashCode();
        }


        [UsedByRubyCompiler]
        public static object alias(Class klass, string name, string def, Frame caller)
        {
            klass.define_alias(name, def, caller);
            return null;
        }

        // ruby_cbase: gets the class attached to the current 'node' (i.e. the current lexical context)
        internal static Class ruby_cbase(Frame caller)
        {
            Class[] nesting = caller.nesting();

            if (nesting == null || nesting.Length == 0)
                return Ruby.Runtime.Init.rb_cObject;

            return nesting[0];
        }

        [UsedByRubyCompiler]
        public static bool Test(object test)
        {
            return !(test == null || (test is bool && ((bool)test) == false));
        }

        internal static bool RespondTo(object obj, string method)
        {
            Class klass;
            // Fixme!!!
            return Eval.FindPrivateMethod(obj, null, method, out klass) != null;

            //return (bool)(rb_obj_respond_to.singleton.Call1(null, obj, null, null, true));
        }

        // ------------------------------------------------------------------------------


        internal static object eval(object self, String src, IContext scope, string file, int line, Frame caller)
        {
            //System.Console.WriteLine("eval({0})", src.value);
            Frame frame = scope.Frame();
            //frame.caller = caller;      // BBTAG

            Compiler.AST.EVAL tree = (Compiler.AST.EVAL)Compiler.Parser.ParseString(caller, frame, file, src, line);
            PERWAPI.FieldRef surroundingClass = Compiler.CodeGenContext.FindParentClassField(frame.GetType());
            PERWAPI.PEFile assembly = tree.GenerateCode(surroundingClass);

            //assembly.WritePEFile(false);
            return tree.ExecuteInit(assembly, null, scope.Self(), caller, frame);
            //throw new System.Exception("testing");
        }

        internal static object eval_under(Class klass, object self, String src, IContext scope, string file, int line, Frame caller)
        {
            //System.Console.WriteLine("eval({0})", src.value);
            Frame frame = scope.Frame();
            //frame.caller = caller;      // BBTAG

            Compiler.AST.EVAL tree = (Compiler.AST.EVAL)Compiler.Parser.ParseString(caller, frame, file, src, line);
            PERWAPI.PEFile assembly = tree.GenerateCode(null);

            //Compiler.CodeGenContext.WriteToFile(assembly);
            return tree.ExecuteInit(assembly, klass, scope.Self(), caller, frame);
            //throw new System.Exception("testing");
        }

        internal static object specific_eval(Class last_class, object self, Class ruby_class, Frame caller, Proc block, Array args) //author: Brian, status: partial
        {
            if (block != null)
            {
                ArgList argList = new ArgList();
                argList.AddArray(args, caller);
                return block.yield_under(caller, argList, ruby_class, self);
            }
            else
            {
                string file = "eval";
                int line = 1;

                object scope = null;
                object vfile = null;
                object vline = null;
                String src = null;

                if (args.Count > 0)
                {
                    src = (String)(args[0]);
                    if (args.Count > 1)
                    {
                        vfile = args[1];
                        file = ((String)vfile).value;
                        if (args.Count > 2)
                        {
                            vline = args[2];
                            line = (int)vline;
                        }
                    }
                }

                scope = new Binding(caller, self);

                if (!(scope is IContext))
                    throw new TypeError(string.Format(CultureInfo.InvariantCulture, "wrong argument type {0} (expected Proc/Binding)", scope.GetType())).raise(caller);

                object ret = Eval.eval_under(ruby_class, self, (String)src, (IContext)scope, file, line, caller);

                return ret;
            }
        }

        // ------------------------------------------------------------------------------


        internal static MethodBody rb_method_node(Class klass, string id) // status: unimplemented
        {
            
            throw new System.NotImplementedException();
        }

        internal static int rb_safe_level() // status: done
        {
            return Eval.ruby_safe_level;
        }

        internal static void rb_secure(int level, Frame caller) // status: done
        {
            if (level <= rb_safe_level())
            {
                if ((caller != null) && (caller.methodName().Length > 0))
                {
                    throw new SecurityError("Insecure operation `" + caller.methodName() + " at level " + ruby_safe_level).raise(caller);
                }
                else
                {
                    throw new SecurityError("Insecure operation at level " + ruby_safe_level).raise(caller);
                }
            }
        }

        internal static void rb_check_safe_obj(Frame caller, object obj) // status: done
        {
            if (rb_safe_level() > 0 && ((Object)obj).Tainted)
                throw new SecurityError("Insecure operation: -r").raise(caller);

            rb_secure(4, null);
        }

        internal static void rb_check_safe_str(Frame caller, object str) // status: done
        {
            rb_check_safe_obj(caller, str);
            if (!(str is String))
                throw new TypeError("wrong argument type " + str + " (expected String)").raise(caller);
        }

        internal static void terminate_process(Frame caller, int status, string message) // author: cjs, status: done
        {
            throw new SystemExit(message).raise(caller);
        }

        internal static void rb_exit(Frame caller, int status) // author: cjs, comment: prot_tag
        {
            //FIXME: prot_tag
            //if (prot_tag)
            terminate_process(caller, status, "exit");

            //ruby_finalize();

            System.Environment.Exit(status);
        }

        internal static String rb_lastline_get(Frame caller) // author: cjs, status: done
        {
            return caller.Uscore;
        }

        internal static void rb_lastline_set(Frame caller, String line) // author: cjs, status: done
        {
            caller.Uscore = line;
        }










        private class DummyFrame : Frame
        {
            public DummyFrame()
                : base(null)
            {
            }

            protected override string file()
            {
                return "";
            }

            public override string methodName()
            {
                return "";
            }

            public override Class[] nesting()
            {
                return new Class[0];
            }
        }
    }




    internal class errat_global : global_variable
    {
        internal override object getter(string id, Frame caller) // status: done
        {
            System.Console.WriteLine("get global {0}, currently {1}", id, Eval.ruby_errinfo.value);
            if (Eval.ruby_errinfo.value == null)
                return null;

            return Methods.exc_backtrace.singleton.Call0(null, Eval.ruby_errinfo.value, caller, caller.block_arg);
        }

        internal override void setter(string id, object value, Frame caller) // status: done
        {
            if (Eval.ruby_errinfo.value == null)
                throw new ArgumentError("$! not set").raise(caller);

            Methods.exc_set_backtrace.singleton.Call1(null, Eval.ruby_errinfo.value, caller, caller.block_arg, value);
        }
    }

    [UsedByRubyCompiler]
    public class errinfo_global : global_variable
    {
        internal override void setter(string id, object value, Frame caller) // status: done
        {
            if (value != null && !(value is Exception))
                throw new TypeError("assigning non-exception to $!").raise(caller);

            this.value = value;
        }
    }


    internal class safe_global : global_variable
    {
        internal override object getter(string id, Frame caller) // author: Brian, status: done
        {
            // safe_getter
            return Eval.ruby_safe_level;
        }

        internal override void setter(string id, object value, Frame caller) // author: Brian, status: done
        {
            // safe_setter
            int level = Object.Convert<int>(value, "to_int", caller);

            if (level < Eval.ruby_safe_level)
            {
                throw new SecurityError("tried to downgrade safe level from " + Eval.ruby_safe_level + " to " + level).raise(caller);
            }

            if (level > Eval.SAFE_LEVEL_MAX) level = Eval.SAFE_LEVEL_MAX;

            Eval.ruby_safe_level = level;
            Eval.curr_thread.safe = level;
        }
    }


    internal class lastline_global : global_variable // author:cjs, status: done
    {
        internal override object getter(string id, Frame caller)
        {
            return Eval.rb_lastline_get(caller);
        }
        internal override void setter(string id, object value, Frame caller)
        {
            Eval.rb_lastline_set(caller, String.RStringValue(value, caller));
        }
    }
}



namespace Ruby
{    
    internal class LocalJumpError : StandardError
    {
        public LocalJumpError(string message) : this(message, Ruby.Runtime.Init.rb_eLocalJumpError) { }

        public LocalJumpError(string message, Class klass) : base(message, klass) { }

        public LocalJumpError(Class klass) : base(klass) { }
    }


    
    internal class SystemStackError : StandardError
    {
        public SystemStackError(string message) : this(message, Ruby.Runtime.Init.rb_eSysStackError) { }

        public SystemStackError(string message, Class klass) : base(message, klass) { }

        public SystemStackError(Class klass) : base(klass) { }
    }
}