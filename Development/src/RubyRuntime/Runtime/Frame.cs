/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


using System.Collections.Generic;
using Ruby;


namespace Ruby.Runtime
{
    // Ruby.Frame - represents an Activation Frame created by a Ruby method call
    // Sub-classes of this class are created containing local variables for each method

    [UsedByRubyCompiler]
    public abstract class Frame
    {
        internal Frame caller;

        [UsedByRubyCompiler]
        public Proc block_arg;

        [UsedByRubyCompiler]
        public Block current_block;

        [UsedByRubyCompiler]
        public int line;
        internal Dictionary<string, object> dynamic_vars = null;
        internal Match tilde;
        internal String uscore;
        internal CallStatus call_status = CallStatus.None;

        internal Access scope_vmode = Access.Public;

        [UsedByRubyCompiler]
        public Frame(Frame caller)
        {
            this.caller = caller;
            // default visibility is private at toplevel
            if (caller == null)
                scope_vmode = Access.Private;
        }

        protected abstract string file();

        public abstract string methodName();

        public abstract Class[] nesting();

        private Frame OuterFrame()
        {
            if (current_block != null)
            {
                List<Frame> outer = current_block.OuterFrames();
                return outer[outer.Count - 1];
            }
            else
                return this;
        }

        [UsedByRubyCompiler]
        public Match Tilde
        {
            get { return OuterFrame().tilde;  }
            set { OuterFrame().tilde = value; }
        }

        internal String Uscore
        {
            get { return OuterFrame().uscore; }
            set { OuterFrame().uscore = value; }
        }

        internal string callPoint()
        {
            string location = baseName(file()) + ":" + line.ToString();
            if (methodName() != "")
                location += ":in `" + methodName() + "'";
            return location;
        }


        internal static string baseName(string fileName)
        {
            int pos = fileName.LastIndexOf('\\');
            if (pos >= 0)
                return fileName.Substring(pos+1);
            else
                return fileName;
        }

        internal List<Frame> OuterFrames()
        {
            if (current_block != null)
                return current_block.OuterFrames();
            else
                return new List<Frame>();
        }

        [UsedByRubyCompiler]
        public void SetDynamic(string vid, object value)
        {
            dynamic_vars[vid] = value;
        }

        [UsedByRubyCompiler]
        public object GetDynamic(string vid)
        {
            return dynamic_vars[vid];
        }
    }


    [UsedByRubyCompiler]
    public class ControlException : System.Exception
    {
    }



    [UsedByRubyCompiler]
    public class BreakException : ControlException
    {
        [UsedByRubyCompiler]
        public object return_value;

        [UsedByRubyCompiler]
        public Frame scope;

        [UsedByRubyCompiler]
        public BreakException(object returnValue, Frame scope)
        {
            this.return_value = returnValue;
            this.scope = scope;
        }
    }


    [UsedByRubyCompiler]
    public class ReturnException : ControlException
    {
        [UsedByRubyCompiler]
        public object return_value;

        [UsedByRubyCompiler]
        public Frame scope;

        [UsedByRubyCompiler]
        public ReturnException(object returnValue, Frame scope)
        {
            this.return_value = returnValue;
            this.scope = scope;
        }
    }


    [UsedByRubyCompiler]
    public class RetryException : ControlException
    {
        [UsedByRubyCompiler]
        public Frame scope;

        [UsedByRubyCompiler]
        public RetryException(Frame scope)
        {
            this.scope = scope;
        }
    }
}
