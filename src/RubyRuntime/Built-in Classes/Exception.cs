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

        public Exception(Class klass)
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

        public SystemExit(string message) : base(message, Ruby.Runtime.Init.rb_eSystemExit) { }

        public SystemExit(Class klass): base(klass)
        {
            this._status = 0;
        }

        public SystemExit(string message, Class klass)
            : base(message, klass)
        {
            this._status = 0; //default to 0 (success)
        }
    }



    public partial class fatal : Exception //status: done
    {
        public fatal(string message) : base(message, Ruby.Runtime.Init.rb_eFatal) { }

        public fatal(string message, Class klass) : base(message, klass) { }

        public fatal(Class klass) : base(klass) { }
    }



    public partial class SignalException : Exception //status: done
    {
        public SignalException(string message) : base(message, Ruby.Runtime.Init.rb_eSignal) { }

        public SignalException(string message, Class klass) : base(message, klass) { }

        public SignalException(Class klass) : base(klass) { }
    }



    public partial class Interrupt : Exception //status: done
    {
        public Interrupt(Frame caller, string message) : base(message, Ruby.Runtime.Init.rb_eInterrupt) { }

        public Interrupt(Frame caller, string message, Class klass) : base(message, klass) { }

        public Interrupt(Class klass) : base(klass) { }
    }



    public class StandardError : Exception //status: done
    {
        public StandardError(string message) : base(message, Ruby.Runtime.Init.rb_eStandardError) { }

        public StandardError(Class klass) : base(klass) { }

        public StandardError(string message, Class klass) : base(message, klass) { }
    }



    public class TypeError : StandardError //status: done
    {
        public TypeError(string message) : base(message, Ruby.Runtime.Init.rb_eTypeError) { }

        public TypeError(Class klass) : base(klass) { }

        public TypeError(string message, Class klass) : base(message, klass) { }

        public static TypeError rb_error_frozen(Frame caller, string what)
        {
            return new TypeError(string.Format(CultureInfo.InvariantCulture, "can't modify frozen {0}", what));
        }

        public static void rb_check_frozen(Frame caller, object obj)
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

        public ArgumentError(Class klass) : base(klass) { }

        public ArgumentError(string message, Class klass) : base(message, klass) { }
    }



    public class IndexError : StandardError //status: done
    {
        public IndexError(string message) : this(message, Ruby.Runtime.Init.rb_eIndexError) { }

        public IndexError(Class klass) : base(klass) { }

        public IndexError(string message, Class klass) : base(message, klass) { }
    }



    public class RangeError : StandardError //status: done
    {
        public RangeError(string message) : this(message, Ruby.Runtime.Init.rb_eRangeError) { }

        public RangeError(Class klass) : base(klass) { }

        public RangeError(string message, Class klass) : base(message, klass) { }
    }



    public class NameError : StandardError //status: done
    {
        public NameError(string message) : this(message, Ruby.Runtime.Init.rb_eNameError) { }

        public NameError(string name, string message) : this(name, message, Ruby.Runtime.Init.rb_eNameError) { }

        public NameError(string name, string message, Class klass)
            : base(message, klass)
        {
            Methods.name_err_initialize.singleton.Call2(null, this, null, null, message, name);
        }

        public NameError(string message, Class klass) : base(message, klass) { }

        public NameError(Class klass) : base(klass) { }

    }



    public class NoMethodError : NameError
    {
        public NoMethodError(string message) : base(message, Ruby.Runtime.Init.rb_eNoMethodError) { }

        public NoMethodError(Class klass) : base(klass) { }

        public NoMethodError(string message, Class klass) : base(message, klass) { }
    }



    public class ScriptError : Exception //status: done
    {
        public ScriptError(string message) : base(message, Ruby.Runtime.Init.rb_eScriptError) { }

        public ScriptError(Class klass) : base(klass) { }

        public ScriptError(string message, Class klass) : base(message, klass) { }
    }



    public class SyntaxError : ScriptError //status: done
    {
        public SyntaxError(string message) : this(message, Ruby.Runtime.Init.rb_eSyntaxError) { }

        public SyntaxError(Class klass) : base(klass) { }

        public SyntaxError(string message, Class klass) : base(message, klass) { }
    }



    public class LoadError : ScriptError //status: done
    {
        public LoadError(string message) : base(message, Ruby.Runtime.Init.rb_eLoadError) { }

        public LoadError(Class klass) : base(klass) { }

        public LoadError(string message, Class klass) : base(message, klass) { }
    }



    public class NotImplementedError : ScriptError //status: done
    {
        public NotImplementedError(string message) : this(message, Ruby.Runtime.Init.rb_eNotImpError) { }

        public NotImplementedError(Class klass) : base(klass) { }

        public NotImplementedError(string message, Class klass) : base(message, klass) { }

        public static NotImplementedError rb_notimplement(Frame caller, string method)
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


    public class RuntimeError : StandardError //status: done
    {
        public RuntimeError(string message) : base(message, Ruby.Runtime.Init.rb_eRuntimeError) { }

        public RuntimeError(string message, Class klass) : base(message, klass) { }

        public RuntimeError(Class klass) : base(klass) { }
    }



    public class SecurityError : StandardError //status: done 
    {
        public SecurityError(string message) : base(message, Ruby.Runtime.Init.rb_eSecurityError) { }

        public SecurityError(string message, Class klass) : base(message, klass) { }

        public SecurityError(Class klass) : base(klass) { }
    }


    public class ThreadError : StandardError //status: done 
    {
        public ThreadError(string message) : base(message, Ruby.Runtime.Init.rb_eThreadError) { }

        public ThreadError(string message, Class klass) : base(message, klass) { }

        public ThreadError(Class klass) : base(klass) { }
    }


    public class NoMemoryError : Exception //status: done
    {
        public NoMemoryError(string message) : base(message, Ruby.Runtime.Init.rb_eNoMemError) { }

        public NoMemoryError(string message, Class klass) : base(message, klass) { }

        public NoMemoryError(Class klass) : base(klass) { }
    }



    public class SystemCallError : StandardError
    {
        internal int errno;

        public SystemCallError(int errno)
            : base(Ruby.Runtime.Init.rb_eSystemCallError)
        {
            this.errno = errno;
        }

        public SystemCallError(string message) : base(message, Ruby.Runtime.Init.rb_eSystemCallError) { }

        public SystemCallError(string message, Class klass)
            : base(message, klass)
        {
            this.errno = 0;
        }

        public SystemCallError(Class klass)
            : base(null, klass)
        {
            this.errno = 0;
        }

        public SystemCallError(string message, Class klass, int errno)
            : base(message, klass)
        {
            this.errno = errno;
        }

        public static int HResultToPosixErrorCode(int HRESULT)
        {
            return HRESULT;
        }

        public static SystemCallError rb_sys_fail(string mesg, Frame caller)
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

        public static SystemCallError rb_sys_fail(string mesg, System.Exception e, Frame caller)
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

