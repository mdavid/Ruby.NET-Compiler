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

    public partial class Binding : Object, IContext
    {
        internal Frame frame;
        internal object self;

        internal Binding(Frame frame, object self)
        {
            this.frame = frame;
            this.self = self;
        }

        public Frame Frame()
        {
            return frame;
        }

        public object Self()
        {
            return self;
        }

        internal object Clone()
        {
            return Ruby.Methods.proc_clone.singleton.Call0(null, this, null, null);
        }
    }


    internal interface IContext
    {
        Frame Frame();
        object Self();
    }
}

