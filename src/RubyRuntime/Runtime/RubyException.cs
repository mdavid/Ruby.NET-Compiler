/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;

namespace Ruby.Runtime
{
    // Ruby.RubyException - used for raising Ruby Exceptions

    [UsedByRubyCompiler]
    public class RubyException : System.Exception
    {
        // The Ruby Exception object nested within the .NET exception object
        // (Ruby.Exception inherits from Object rather than System.Exception)
        // (If only we had multiple inheritance!)

        [UsedByRubyCompiler]
        public Exception parent; 


        public RubyException(Exception parent) : base()
        {
            this.parent = parent;
        }

        public override string Message
        {
            get { return parent.instance_variable_get("mesg").ToString(); }
        }

        internal virtual void Report()
        {
            if (!(parent is SystemExit))
            {
                System.Console.Error.WriteLine("{0} ({1})", Message, parent.GetType());
                foreach (String frame in (Array)parent.instance_variable_get("bt"))
                    System.Console.Error.WriteLine(frame);
            }
        }
    }

    // Ruby.SymbolException - used for Ruby throw and catch
    
    internal class SymbolException : ControlException
    {
        internal string tag;
        internal object arg;

        internal SymbolException(string tag, object arg)
        {
            this.tag = tag;
            this.arg = arg;
        }
    }
}
