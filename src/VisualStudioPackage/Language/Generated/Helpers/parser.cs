/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


using System;
using System.Collections.Generic;
using System.Text;
using Ruby.NET.ParserGenerator;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Ruby.NET.Parser
{

    // The non-autogenerated part of the Parser class.
    public partial class Parser
    {
        private int in_single = 0;
        private int in_def = 0;


        public new Scanner scanner
        {
            get { return (Scanner)base.scanner; }
            set { value.parser = this;  base.scanner = value; }
        }

        private Stack<Scope> scope_stack = new Stack<Scope>();

        private void enter_scope()
        {
            scope_stack.Push(new Scope());
        }

        private void enter_block()
        {
            scope_stack.Push(new Scope(CurrentScope));
        }

        private void leave_scope()
        {
            scope_stack.Pop();
        }

        public Scope CurrentScope
        {
            get
            {
                if (scope_stack.Count > 0)
                    return scope_stack.Peek();
                else
                    return null;
            }
        }

        private void assignable(string id)
        {
            if (ID.Scope(id) == ID_Scope.LOCAL)
                CurrentScope.add_local(id);
        }

        public void SetHandler(IErrorHandler handler)
        {
            scanner.Handler = handler;
        }
      
        
        internal TextSpan MkTSpan(LexLocation s) { return TextSpan(s.sLin, s.sCol, s.eLin, s.eCol); }

        internal void Match(LexLocation lh, LexLocation rh)
        {
            DefineMatch(MkTSpan(lh), MkTSpan(rh));
        }

        internal void DefineRegion(LexLocation loc)
        {
            if (loc.sLin < loc.eLin)
                DefineRegion(MkTSpan(loc));
        }
    }
}