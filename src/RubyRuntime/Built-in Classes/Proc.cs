/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using System.Collections.Generic;


namespace Ruby
{
    internal enum ProcKind { Method, Block, RawProc, Lambda };

    [UsedByRubyCompiler]
    public partial class Proc : Object, IContext
    {
        internal object self;
        internal Proc block;
        internal MethodBody body;
        internal int _arity;
        internal ProcKind kind = ProcKind.Block;

        // ----------------------------------------------------------------------------

        public Proc(Class klass)
            : base(klass)
        {
            this.kind = ProcKind.RawProc;
        }

        [UsedByRubyCompiler]
        public Proc(object self, Proc block, MethodBody body, int arity)
            : this(self, block, body, arity, ProcKind.RawProc)
        {
        }

        internal Proc(object self, Proc block, MethodBody body, int arity, ProcKind kind)
            : base(Ruby.Runtime.Init.rb_cProc)
        {
            this.self = self;
            this.block = block;
            this.body = body;
            this.kind = kind;
            this._arity = arity;
        }

        // ----------------------------------------------------------------------------

        public Frame Frame()
        {
            return (Frame) body.GetType().GetField("locals0").GetValue(body);
        }


        public object Self()
        {
            return self;
        }

        internal object yield(Frame caller, params object[] args)
        {
            ArgList list = new ArgList(block, args);
            if (args.Length == 1)
                list.single_arg = true;

            return yield(caller, list);
            
        }

        [UsedByRubyCompiler]
        public object yield(Frame caller, ArgList args)
        {
            args.block = this.block;
            // return body.Calln(my_class, self, caller, args); // BBTAG: last_class should be nearest lexical class definition rather than Proc
            Class last_class = Init.rb_cObject;
            if (caller != null && caller.nesting().Length > 0)
                last_class = caller.nesting()[0];
            return body.Calln(last_class, self, caller, args);
        }

        internal static object rb_yield(Proc block, Frame caller, params object[] args)
        {
            //this test is performed in 'rb_yield_0' in c ruby.             
            if (block == null)
            {
                throw new LocalJumpError("no block given").raise(caller);
            }

            return block.yield(caller, args);
        }

        internal static object rb_yield(Proc block, Frame caller, ArgList args)
        {
            if (block == null)
            {
                throw new LocalJumpError("no block given").raise(caller);
            }

            return block.yield(caller, args);
        }

        internal object yield_under(Frame caller, ArgList args, Class klass, object self)
        {
            args.block = this.block;

            if (body is Block)
                return ((Block)body).Calln(klass, klass, self, caller, args);
            else
                return body.Calln(klass, self, caller, args);
        }

        internal bool HasCorrectArgs(int argCount)
        {
            if (_arity < 0)
                return argCount >= -_arity-1;
            else
                return argCount == _arity;
        }
    }
}
