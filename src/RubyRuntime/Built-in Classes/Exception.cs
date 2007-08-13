/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Diagnostics;
using System.Globalization;

namespace Ruby
{
    [UsedByRubyCompiler]
    public partial class Exception : Object
    {
        internal RubyException rubyException;

        // -----------------------------------------------------------------------

        internal Exception()
            : this(null, Ruby.Runtime.Init.rb_eException)
        {
        }

        internal Exception(string message)
            : this(message, Ruby.Runtime.Init.rb_eException)
        {
        }

        internal Exception(Class klass)
            : this(klass._name, klass)
        {
        }

        public Exception(string message, Class klass)
            : base(klass)
        {
            rubyException = new RubyException(this);
            instance_variable_set("mesg", new String(message));
        }

        // -----------------------------------------------------------------------


        internal override object Inner()
        {
            return rubyException;
        }

        [UsedByRubyCompiler]
        public RubyException raise(Frame caller)
        {
            Array backtrace = new Array();

            Frame frame = caller;
            while (frame != null)
            {
                backtrace.Add(new String(frame.callPoint()));
                frame = frame.caller;
            }

            this.instance_variable_set("bt", backtrace);

            return rubyException;
        }

        internal static void rb_bug(string mesg, Frame caller)
        {
            System.IO.TextWriter stderr = System.Console.Error;

            if (caller != null)
                stderr.Write(caller.callPoint());
            stderr.Write("[BUG] ");
            stderr.Write(mesg);
            stderr.Write(string.Format(CultureInfo.InvariantCulture, "\nruby {0} ({1}) [{2}]\n\n", new object[] { Version.ruby_version, Version.ruby_release_date, Version.ruby_platform }));

            try
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            catch
            {
            }
        }
    }



    [UsedByRubyCompiler]
    public partial class SystemExit : Exception
    {
        internal int _status;

        internal SystemExit(string message) : base(message, Ruby.Runtime.Init.rb_eSystemExit) { }

        internal SystemExit(string message, Class klass)
            : base(message, klass)
        {
            this._status = 0; //default to 0 (success)
        }
    }



    public partial class fatal : Exception //status: done
    {
        internal fatal(string message) : base(message, Ruby.Runtime.Init.rb_eFatal) { }

        protected fatal(string message, Class klass) : base(message, klass) { }
    }



    public partial class SignalException : Exception //status: done
    {
        internal SignalException(string message) : base(message, Ruby.Runtime.Init.rb_eSignal) { }

        protected SignalException(string message, Class klass) : base(message, klass) { }
    }



    public partial class Interrupt : Exception //status: done
    {
        internal Interrupt(Frame caller, string message) : base(message, Ruby.Runtime.Init.rb_eInterrupt) { }

        internal Interrupt(Frame caller, string message, Class klass) : base(message, klass) { }
    }



    public class StandardError : Exception //status: done
    {
        internal StandardError(string message) : base(message, Ruby.Runtime.Init.rb_eStandardError) { }

        protected StandardError(Class klass) : base(klass) { }

        protected StandardError(string message, Class klass) : base(message, klass) { }
    }



    internal class TypeError : StandardError //status: done
    {
        internal TypeError(string message) : base(message, Ruby.Runtime.Init.rb_eTypeError) { }

        protected TypeError(string message, Class klass) : base(message, klass) { }

        internal static TypeError rb_error_frozen(Frame caller, string what)
        {
            return new TypeError(string.Format(CultureInfo.InvariantCulture, "can't modify frozen {0}", what));
        }

        internal static void rb_check_frozen(Frame caller, object obj)
        {
            if (((Basic)obj).Frozen)
                throw TypeError.rb_error_frozen(caller, ((Basic)obj).my_class._name).raise(caller);
        }
    }


    [UsedByRubyCompiler]
    public class ArgumentError : StandardError //status: done
    {
        [UsedByRubyCompiler]
        public ArgumentError(string message) : base(message, Ruby.Runtime.Init.rb_eArgError) { }

        protected ArgumentError(string message, Class klass) : base(message, klass) { }
    }



    internal class IndexError : StandardError //status: done
    {
        internal IndexError(string message) : this(message, Ruby.Runtime.Init.rb_eIndexError) { }

        protected IndexError(string message, Class klass) : base(message, klass) { }
    }



    internal class RangeError : StandardError //status: done
    {
        internal RangeError(string message) : this(message, Ruby.Runtime.Init.rb_eRangeError) { }

        protected RangeError(string message, Class klass) : base(message, klass) { }
    }



    internal class NameError : StandardError //status: done
    {
        internal NameError() : this(Ruby.Runtime.Init.rb_eNameError) { }

        internal NameError(string message) : this(message, Ruby.Runtime.Init.rb_eNameError) { }

        internal NameError(string name, string message) : this(name, message, Ruby.Runtime.Init.rb_eNameError) { }

        internal NameError(string name, string message, Class klass)
            : base(message, klass)
        {
            Methods.name_err_initialize.singleton.Call2(null, this, null, null, message, name);
        }

        internal NameError(string message, Class klass) : base(message, klass) { }

        internal NameError(Class klass) : base(klass) { }

    }



    internal class NoMethodError : NameError
    {
        internal NoMethodError() : base(Ruby.Runtime.Init.rb_eNoMethodError) { }

        internal NoMethodError(string message) : base(message, Ruby.Runtime.Init.rb_eNoMethodError) { }

        internal NoMethodError(string message, Class klass) : base(message, klass) { }
    }



    internal class ScriptError : Exception //status: done
    {
        internal ScriptError(string message) : base(message, Ruby.Runtime.Init.rb_eScriptError) { }

        protected ScriptError(string message, Class klass) : base(message, klass) { }
    }



    internal class SyntaxError : ScriptError //status: done
    {
        internal SyntaxError(string message) : this(message, Ruby.Runtime.Init.rb_eSyntaxError) { }

        protected SyntaxError(string message, Class klass) : base(message, klass) { }
    }



    internal class LoadError : ScriptError //status: done
    {
        internal LoadError(string message) : base(message, Ruby.Runtime.Init.rb_eLoadError) { }

        protected LoadError(string message, Class klass) : base(message, klass) { }
    }



    internal class NotImplementedError : ScriptError //status: done
    {
        internal NotImplementedError(string message) : this(message, Ruby.Runtime.Init.rb_eNotImpError) { }

        protected NotImplementedError(string message, Class klass) : base(message, klass) { }

        internal static NotImplementedError rb_notimplement(Frame caller, string method)
        {
            return new NotImplementedError(string.Format(CultureInfo.InvariantCulture, "The {0}() function is unimplemented on this machine", method));
        }
    }


    [UsedByRubyCompiler]
    public class CLRException : StandardError
    {
        internal System.Exception inner;

        [UsedByRubyCompiler]
        public CLRException(Frame caller, System.Exception e)
            : base(e.Message, Ruby.Runtime.Init.rb_eCLRException)
        {
            //this.raise(caller);
            this.inner = e;

            Array backtrace = new Array();

            foreach (StackFrame frame0 in new System.Diagnostics.StackTrace(inner, true).GetFrames())
            {
                System.Reflection.MethodBase method = frame0.GetMethod();
                if (method.GetCustomAttributes(typeof(System.Diagnostics.DebuggerNonUserCodeAttribute), false).Length == 0)
                {
                    string file = frame0.GetFileName();
                    if (file != null) file = Frame.baseName(file);

                    string methodName = method.DeclaringType.Name + "." + method.Name;

                    string trace;
                    if (file != null)
                        trace = System.String.Format(CultureInfo.InvariantCulture, "{0}:{1} in `{2}'", file, frame0.GetFileLineNumber(), methodName);
                    else
                        trace = System.String.Format(CultureInfo.InvariantCulture, "??? in `{0}'", methodName);

                    backtrace.Add(new String(trace));
                }
            }

            Frame frame1 = caller;
            while (frame1 != null)
            {
                backtrace.Add(new String(frame1.callPoint()));
                frame1 = frame1.caller;
            }

            this.instance_variable_set("bt", backtrace);
        }
    }


    internal class RuntimeError : StandardError //status: done
    {
        internal RuntimeError(string message) : base(message, Ruby.Runtime.Init.rb_eRuntimeError) { }

        protected RuntimeError(string message, Class klass) : base(message, klass) { }
    }



    internal class SecurityError : StandardError //status: done 
    {
        internal SecurityError(string message) : base(message, Ruby.Runtime.Init.rb_eSecurityError) { }

        protected SecurityError(string message, Class klass) : base(message, klass) { }
    }


    internal class ThreadError : StandardError //status: done 
    {
        internal ThreadError(string message) : base(message, Ruby.Runtime.Init.rb_eThreadError) { }

        protected ThreadError(string message, Class klass) : base(message, klass) { }
    }


    internal class NoMemoryError : Exception //status: done
    {
        internal NoMemoryError(string message) : base(message, Ruby.Runtime.Init.rb_eNoMemError) { }

        protected NoMemoryError(string message, Class klass) : base(message, klass) { }
    }



    internal class SystemCallError : StandardError
    {
        internal int errno;

        internal SystemCallError(int errno)
            : base(Ruby.Runtime.Init.rb_eSystemCallError)
        {
            this.errno = errno;
        }

        internal SystemCallError(string message) : base(message, Ruby.Runtime.Init.rb_eSystemCallError) { }

        internal SystemCallError(string message, Class klass)
            : base(message, klass)
        {
            this.errno = 0;
        }

        internal SystemCallError(Class klass)
            : base(null, klass)
        {
            this.errno = 0;
        }

        internal SystemCallError(string message, Class klass, int errno)
            : base(message, klass)
        {
            this.errno = errno;
        }

        private static int HResultToPosixErrorCode(int HRESULT)
        {
            return HRESULT;
        }

        internal static SystemCallError rb_sys_fail(string mesg, Frame caller)
        {
            if (Errno.errno == 0)
                Exception.rb_bug(string.Format(CultureInfo.InvariantCulture, "rb_sys_fail({0}) - errno == 0", (mesg != null ? mesg : "")), caller);

            SystemCallError error;
            if (Errno.errno == Errno.EPERM)
            {
                error = new SystemCallError("Operation not permitted", (Class)Ruby.Runtime.Init.rb_mErrno.instance_variable_get("EPERM"), Errno.EPERM);
            }
            else if (Errno.errno == Errno.EPERM)
            {
                error = new SystemCallError(string.Format(CultureInfo.InvariantCulture, "No such file or directory - {0}", mesg), (Class)Ruby.Runtime.Init.rb_mErrno.instance_variable_get("ENOENT"), Errno.ENOENT);
            }
            else if (Errno.errno == Errno.EPERM)
            {
                error = new SystemCallError(string.Format(CultureInfo.InvariantCulture, "Permission denied - {0}", mesg), (Class)Ruby.Runtime.Init.rb_mErrno.instance_variable_get("EACCES"), Errno.EACCES);
            }
            else
            {
                error = new SystemCallError(mesg);
                error.errno = Errno.errno;
            }

            return error;
        }

        internal static SystemCallError rb_sys_fail(string mesg, System.Exception e, Frame caller)
        {
            SystemCallError error = null;
            if (e is System.IO.FileNotFoundException || e is System.IO.DirectoryNotFoundException || e is System.IO.DriveNotFoundException)
            {
                error = new SystemCallError(string.Format(CultureInfo.InvariantCulture, "No such file or directory - {0}", mesg), (Class)Ruby.Runtime.Init.rb_mErrno.instance_variable_get("ENOENT"), Errno.ENOENT);
            }
            else if (e is System.IO.IOException)
            {
                if (e.Message.Contains("ready"))
                    error = new SystemCallError(string.Format(CultureInfo.InvariantCulture, "No such device or address - {0}", mesg), (Class)Ruby.Runtime.Init.rb_mErrno.instance_variable_get("ENXIO"), Errno.ENXIO);
            }
            else if (e is System.UnauthorizedAccessException)
            {
                error = new SystemCallError(string.Format(CultureInfo.InvariantCulture, "Permission denied - {0}", mesg), (Class)Ruby.Runtime.Init.rb_mErrno.instance_variable_get("EACCES"), Errno.EACCES);
            }

            if (error == null)
            {
                int HRESULT = System.Runtime.InteropServices.Marshal.GetHRForException(e);
                int errno = HResultToPosixErrorCode(HRESULT);
                error = new SystemCallError(mesg);
                error.errno = errno;
            }

            if (e == null || (error.errno) == 0)
                Exception.rb_bug(string.Format(CultureInfo.InvariantCulture, "rb_sys_fail({0}) - errno == 0", (mesg != null ? mesg : "")), caller);

            return error;
        }
    }
}

