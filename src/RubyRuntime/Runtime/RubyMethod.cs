/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using Ruby.Runtime;
using System.Collections.Generic;
using System.Diagnostics;


namespace Ruby.Runtime
{
    // Ruby.Method - Information about Ruby method's stored in the class methods tables.
    [UsedByRubyCompiler]
    public class RubyMethod
    {
        internal MethodBody body;
        internal int arity;
        internal Access access;
        internal Class definingClass;

        internal RubyMethod(MethodBody body, int arity, Access access, Class definingClass)
        {
            this.body = body;
            this.arity = arity;
            this.access = access;
            this.definingClass = definingClass;
        }
    }

    
    internal class MethodAlias: RubyMethod
    {
        internal MethodAlias(RubyMethod orig): base(orig.body, orig.arity, orig.access, orig.definingClass)
        {
        }
    }

    // Ruby.Runtime.Block - base class of all classes that implement Ruby Blocks
    // derived classes contain references to outer Frames

    [UsedByRubyCompiler]
    public abstract class Block : MethodBody
    {
        [UsedByRubyCompiler]
        public Frame defining_scope;

        [UsedByRubyCompiler]
        public Block(Frame defining_scope) 
        {
            this.defining_scope = defining_scope;
        }

        internal List<Frame> OuterFrames()
        {
            List<Frame> frames = new List<Frame>();

            for (int i=0; true; i++)
            {
                string fieldName = "locals" + i;
                System.Reflection.FieldInfo outerFrameField = this.GetType().GetField(fieldName);
                if (outerFrameField == null)
                    break;
                frames.Add((Frame)outerFrameField.GetValue(this));
            }
            
            return frames;
        }

        public abstract object Calln(Class last_class, Class ruby_class, object recv, Frame caller, ArgList args);


        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            return Calln(last_class, null, recv, caller, args);
        }
    }

    // Ruby.RubyMethod - base class of all classes that implement Ruby methods
    // (equivalent to the function pointers used in the Ruby interpreter)

    [UsedByRubyCompiler]
    public abstract class MethodBody
    {
        public virtual object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Calln(last_class, recv, caller, new ArgList(block));
        }

        public virtual object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return Calln(last_class, recv, caller, new ArgList(block, p1));
        }

        public virtual object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            return Calln(last_class, recv, caller, new ArgList(block, p1, p2));
        }

        public virtual object Call3(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, object p3)
        {
            return Calln(last_class, recv, caller, new ArgList(block, p1, p2, p3));
        }

        public virtual object Call4(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, object p3, object p4)
        {
            return Calln(last_class, recv, caller, new ArgList(block, p1, p2, p3, p4));
        }

        public virtual object Call5(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, object p3, object p4, object p5)
        {
            return Calln(last_class, recv, caller, new ArgList(block, p1, p2, p3, p4, p5));
        }

        public virtual object Call6(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, object p3, object p4, object p5, object p6)
        {
            return Calln(last_class, recv, caller, new ArgList(block, p1, p2, p3, p4, p5, p6));
        }

        public virtual object Call7(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, object p3, object p4, object p5, object p6, object p7)
        {
            return Calln(last_class, recv, caller, new ArgList(block, p1, p2, p3, p4, p5, p6, p7));
        }

        public virtual object Call8(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8)
        {
            return Calln(last_class, recv, caller, new ArgList(block, p1, p2, p3, p4, p5, p6, p7, p8));
        }

        public virtual object Call9(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8, object p9)
        {
            return Calln(last_class, recv, caller, new ArgList(block, p1, p2, p3, p4, p5, p6, p7, p8, p9));
        }

        public virtual object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            switch (args.Length)
            {
                case 0: return Call0(last_class, recv, caller, args.block);
                case 1: return Call1(last_class, recv, caller, args.block, args[0]);
                case 2: return Call2(last_class, recv, caller, args.block, args[0], args[1]);
                case 3: return Call3(last_class, recv, caller, args.block, args[0], args[1], args[2]);
                case 4: return Call4(last_class, recv, caller, args.block, args[0], args[1], args[2], args[3]);
                case 5: return Call5(last_class, recv, caller, args.block, args[0], args[1], args[2], args[3], args[4]);
                case 6: return Call6(last_class, recv, caller, args.block, args[0], args[1], args[2], args[3], args[4], args[5]);
                case 7: return Call7(last_class, recv, caller, args.block, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
                case 8: return Call8(last_class, recv, caller, args.block, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
                case 9: return Call9(last_class, recv, caller, args.block, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
                default:
                    throw new System.Exception("wrong number of arguments");
            }
        }
    }

    [UsedByRubyCompiler]
    public abstract class MethodBody0 : MethodBody
    {
        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 0)
                return Call0(last_class, recv, caller, args.block);
            else
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 0)").raise(caller);
        }
    }

    [UsedByRubyCompiler]
    public abstract class MethodBody1 : MethodBody
    {
        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 1)
                return Call1(last_class, recv, caller, args.block, args[0]);
            else
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 1)").raise(caller);
        }
    }

    [UsedByRubyCompiler]
    public abstract class MethodBody2 : MethodBody
    {
        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 2)
                return Call2(last_class, recv, caller, args.block, args[0], args[1]);
            else
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 2)").raise(caller);
        }
    }

    [UsedByRubyCompiler]
    public abstract class MethodBody3 : MethodBody
    {
        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 3)
                return Call3(last_class, recv, caller, args.block, args[0], args[1], args[2]);
            else
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 3)").raise(caller);
        }
    }

    [UsedByRubyCompiler]
    public abstract class MethodBody4 : MethodBody
    {
        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 4)
                return Call4(last_class, recv, caller, args.block, args[0], args[1], args[2], args[3]);
            else
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 4)").raise(caller);
        }
    }

    [UsedByRubyCompiler]
    public abstract class MethodBody5 : MethodBody
    {
        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 5)
                return Call5(last_class, recv, caller, args.block, args[0], args[1], args[2], args[3], args[4]);
            else
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 5)").raise(caller);
        }
    }

    [UsedByRubyCompiler]
    public abstract class MethodBody6 : MethodBody
    {
        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 6)
                return Call6(last_class, recv, caller, args.block, args[0], args[1], args[2], args[3], args[4], args[5]);
            else
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 6)").raise(caller);
        }
    }

    [UsedByRubyCompiler]
    public abstract class MethodBody7 : MethodBody
    {
        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 7)
                return Call7(last_class, recv, caller, args.block, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
            else
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 7)").raise(caller);
        }
    }

    [UsedByRubyCompiler]
    public abstract class MethodBody8 : MethodBody
    {
        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 8)
                return Call8(last_class, recv, caller, args.block, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
            else
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 8)").raise(caller);
        }
    }

    [UsedByRubyCompiler]
    public abstract class MethodBody9 : MethodBody
    {
        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 9)
                return Call9(last_class, recv, caller, args.block, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
            else
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 9)").raise(caller);
        }
    }

    
    internal abstract class VarArgMethodBody : MethodBody
    {
        protected Array Remaining(ArgList args, int count)
        {
            object[] rest = new object[args.Length - count];
            for (int i = 0; i < args.Length - count; i++)
                rest[i] = args[i + count];
            return new Array(rest);
        }
    }

    
    internal abstract class VarArgMethodBody0 : VarArgMethodBody
    {
        public abstract object Call(Class last_class, object recv, Frame caller, Proc block, Array rest);

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            return Call(last_class, recv, caller, args.block, Remaining(args, 0));
        }
    }

    
    internal abstract class VarArgMethodBody1 : VarArgMethodBody
    {
        public abstract object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest);

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length < 1)
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 1)").raise(caller);
            return Call(last_class, recv, caller, args.block, args[0], Remaining(args, 1));
        }
    }

    
    internal abstract class VarArgMethodBody2 : VarArgMethodBody
    {
        public abstract object Call(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, Array rest);

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length < 2)
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 2)").raise(caller);
            return Call(last_class, recv, caller, args.block, args[0], args[1], Remaining(args, 2));
        }
    }

    
    internal abstract class VarArgMethodBody3 : VarArgMethodBody
    {
        public abstract object Call(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, object p3, Array rest);

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length < 3)
                throw new ArgumentError("wrong number of arguments (" + args.Length + " for 3)").raise(caller);
            return Call(last_class, recv, caller, args.block, args[0], args[1], args[2], Remaining(args, 3));
        }
    }

    [UsedByRubyCompiler]
    public interface IEval
    {
        object Invoke(Class last_class, object recv, Frame caller, Frame frame);
    }


    
    internal class CallSuperMethodBody : MethodBody // author: Brian, status: done
    {
        private string methodId;
        private Class klass;

        internal CallSuperMethodBody(Class klass, string methodId)
        {
            this.klass = klass;
            this.methodId = methodId;
        }

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            return Eval.CallSuperA(klass, caller, recv, methodId, args);
        }
    }

    
    internal class DMethodBody : MethodBody // author: Brian, status: done
    {
        internal Method orig;

        internal DMethodBody(Method orig)
        {
            this.orig = orig;
        }

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            Method bound = (Method)(Methods.umethod_bind.singleton.Call1(last_class, orig, caller, args.block, recv));
            return bound.body.body.Calln(last_class, recv, caller, args);
        }
    }

    
    internal class AttrReaderMethodBody : MethodBody // author: Brian, status: done
    {
        internal string name;

        internal AttrReaderMethodBody(string name)
        {
            this.name = name;
        }

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Eval.ivar_get(recv, name);
        }
    }

    
    internal class AttrWriterMethodBody : MethodBody // author: Brian, status: done
    {
        internal string name;

        internal AttrWriterMethodBody(string name)
        {
            this.name = name;
        }

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return Eval.ivar_set(caller, recv, name, p1);
        }
    }
}