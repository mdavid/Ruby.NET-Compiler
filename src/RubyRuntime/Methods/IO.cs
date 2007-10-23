/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using Ruby.Runtime;
using System.IO;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Ruby.Methods
{
    
    internal class io_alloc : MethodBody0 // status: done
    {
        internal static io_alloc singleton = new io_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new IO((Class)recv);
        }
    }

    
    internal class rb_f_syscall : VarArgMethodBody0 // status: done
    {
        internal static rb_f_syscall singleton = new rb_f_syscall();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            throw NotImplementedError.rb_notimplement(caller, "syscall").raise(caller);
        }
    }

    
    internal class rb_f_open : MethodBody // author: cjs, status: done
    {
        internal static rb_f_open singleton = new rb_f_open();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length >= 1)
            {
                string path = String.StringValue(args[0], caller);
                if (path.StartsWith("|"))
                {
                    String newpath = new String(path.Remove(0, 1));
                    args[0] = newpath;
                    return rb_io_s_popen.singleton.Calln(last_class, Ruby.Runtime.Init.rb_cIO, caller, args);
                }
            }
            return rb_io_s_open.singleton.Calln(last_class, Ruby.Runtime.Init.rb_cFile, caller, args);
        }
    }

    
    internal class rb_f_printf : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_printf singleton = new rb_f_printf();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array args)
        {
            if (args.Count == 0)
                return null;

            IO outstream;

            if (args[0] is String)
                outstream = (IO)IO.rb_stdout.value;
            else
            {
                outstream = (IO)args[0];
                args.value.RemoveAt(0);
            }

            IO.rb_io_write(outstream, rb_f_sprintf.singleton.Call(last_class, null, caller, null, args), caller);

            return null;
        }
    }

    
    internal class rb_f_print : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_print singleton = new rb_f_print();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            rb_io_print.singleton.Call(last_class, IO.rb_stdout.value, caller, block, rest);
            return null;
        }
    }

    
    internal class rb_f_putc : MethodBody1 // author: cjs, status: done
    {
        internal static rb_f_putc singleton = new rb_f_putc();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            rb_io_putc.singleton.Call1(last_class, IO.rb_stdout.value, caller, block, p1);
            return null;
        }
    }

    
    internal class rb_f_puts : MethodBody // status: done
    {
        internal static rb_f_puts singleton = new rb_f_puts();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            rb_io_puts.singleton.Calln(last_class, IO.rb_stdout.value, caller, args);
            return null;
        }
    }

    
    internal class rb_f_gets : MethodBody // author: cjs, status: done
    {
        internal static rb_f_gets singleton = new rb_f_gets();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (!IO.next_argv(caller)) return null;

            String line;
            if (IO.current_file != null)
                line = (String)Eval.CallPrivate(IO.current_file, caller, "gets", null, args.ToArray());
            else
                line = IO.argf_getline(args.ToArray(), caller);
            Eval.rb_lastline_set(caller, line);
            return line;
        }
    }

    
    internal class rb_f_readline : MethodBody // author: cjs, status: done
    {
        internal static rb_f_readline singleton = new rb_f_readline();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (!IO.next_argv(caller))
                throw EOFError.rb_eof_error().raise(caller);
            IO.ARGF_FORWARD(caller);
            String line = (String)rb_f_gets.singleton.Calln(last_class, recv, caller, args);

            if (line == null)
                throw EOFError.rb_eof_error().raise(caller);

            return line;
        }
    }

    
    internal class rb_f_getc : MethodBody0 // author: cjs, status: done
    {
        internal static rb_f_getc singleton = new rb_f_getc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Errors.rb_warn("getc is obsolete; use STDIN.getc instead");

            if (!(IO.rb_stdin.value is File))
                Eval.CallPrivate(IO.rb_stdin.value, caller, "gets", null, new object[] { });
            else
                rb_io_getc.singleton.Calln(last_class, IO.rb_stdin.value, caller, new ArgList());
            return null;
        }
    }

    
    internal class rb_f_select : VarArgMethodBody1 // author: cjs, status: partial, comment: threading
    {
        internal static rb_f_select singleton = new rb_f_select();

        private const int FD_SETSIZE = 64;
        
        private class fd_set
        {
            internal int fd_count = 0;
            internal int[] fd_array = new int[FD_SETSIZE];
        }

        private void FD_CLR(int fd, fd_set set)
        {
            for (int i = 0; i < set.fd_count; i++)
                if (set.fd_array[i] == fd)
                {
                    for (; i < set.fd_count - 1; i++)
                        set.fd_array[i] = set.fd_array[i + 1];
                    set.fd_array[set.fd_count - 1] = 0;
                    set.fd_count = 0;

                    return;
                }
        }

        private void FD_SET(int fd, fd_set set)
        {
            set.fd_array[set.fd_count] = fd;
            set.fd_count++;
        }

        private void FD_ZERO(fd_set set)
        {
            set.fd_count = 0;
            set.fd_array = new int[FD_SETSIZE];
        }

        private bool FD_ISSET(int fd, fd_set set)
        {
            for (int i = 0; i < set.fd_count; i++)
                if (set.fd_array[i] == fd)
                    return true;
            return false;
        }

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest)
        {
            object read, write = null, except = null, timeout = null;
            Array res, list;
            fd_set rset = new fd_set(), wset = new fd_set(), eset = new fd_set(), pset = new fd_set();
            object rp, wp, ep;
            System.TimeSpan tp;
            IO fptr;
            int i;
            int max = 0;
            int interrupt_flag = 0;
            int pending = 0;

            Class.rb_scan_args(caller, rest, 1, 3, false);
            read = (Array)p1;
            if (rest.Count > 0)
                write = rest[0];
            if (rest.Count > 1)
                except = rest[1];
            if (rest.Count > 2)
                timeout = rest[2];

            if (timeout == null)
                tp = System.TimeSpan.Zero;
            else
                tp = Time.rb_time_interval(timeout, caller);

            rset.fd_array = new int[64];

            FD_ZERO(pset);
            if (read != null)
            {
                Object.CheckType<Array>(caller, read);
                rp = rset;
                FD_ZERO((fd_set)rp);
                for (i = 0; i < ((Array)read).Count; i++)
                {
                    fptr = IO.GetOpenFile(caller, IO.rb_io_get_io(((Array)read)[i], caller));
                    FD_SET(IO.fileno(fptr.f), (fd_set)rp);
                    if (IO.READ_DATA_PENDING(fptr.f))
                    { /* check for buffered data */
                        pending++;
                        FD_SET(IO.fileno(fptr.f), pset);
                    }
                    if (max < IO.fileno(fptr.f))
                        max = IO.fileno(fptr.f);
                }
                if (pending > 0)
                {        /* no blocking if there's buffered data */
                    tp = System.TimeSpan.Zero;
                }
            }
            else
                rp = null;

            if (write != null)
            {
                Object.CheckType<Array>(caller, write);
                wp = wset;
                FD_ZERO((fd_set)wp);
                for (i = 0; i < ((Array)write).Count; i++)
                {
                    fptr = IO.GetOpenFile(caller, IO.rb_io_get_io(((Array)write)[i], caller));
                    FD_SET(IO.fileno(fptr.f), (fd_set)wp);
                    if (max < IO.fileno(fptr.f))
                        max = IO.fileno(fptr.f);
                    if (fptr.f2 != null)
                    {
                        FD_SET(IO.fileno(fptr.f2), (fd_set)wp);
                        if (max < IO.fileno(fptr.f2))
                            max = IO.fileno(fptr.f2);
                    }
                }
            }
            else
                wp = null;

            if (except != null)
            {
                Object.CheckType<Array>(caller, except);
                ep = eset;
                FD_ZERO((fd_set)ep);
                for (i = 0; i < ((Array)except).Count; i++)
                {
                    fptr = IO.GetOpenFile(caller, IO.rb_io_get_io(((Array)except)[i], caller));
                    FD_SET(IO.fileno(fptr.f), (fd_set)ep);
                    if (max < IO.fileno(fptr.f))
                        max = IO.fileno(fptr.f);
                    if (fptr.f2 != null)
                    {
                        FD_SET(IO.fileno(fptr.f2), (fd_set)ep);
                        if (max < IO.fileno(fptr.f2))
                            max = IO.fileno(fptr.f2);
                    }
                }
            }
            else
                ep = null;

            max++;

            //n = rb_thread_select(max, rp, wp, ep, tp);
            //if (n < 0)
            //{
            //    rb_sys_fail(0);
            //}
            if (pending == 0) //&& n == 0)
                return null; /* returns nil on timeout */

            res = new Array();
            res.Tainted = true;
            Array rparr = new Array(), wparr = new Array(), eparr = new Array();
            if (rp == null)
                rparr.Tainted = true;
            if (wp == null)
                wparr.Tainted = true;
            if (ep == null)
                eparr.Tainted = true;
            res.Add(rparr);
            res.Add(wparr);
            res.Add(eparr);

            if (interrupt_flag == 0)
            {
                if (rp != null)
                {
                    list = rparr;
                    for (i = 0; i < ((Array)read).Count; i++)
                    {
                        fptr = IO.GetOpenFile(caller, IO.rb_io_get_io(((Array)read)[i], caller));
                        if (FD_ISSET(IO.fileno(fptr.f), (fd_set)rp)
                            || FD_ISSET(IO.fileno(fptr.f), pset))
                        {
                            list.Add(((Array)read)[i]);
                        }
                    }
                }

                if (wp != null)
                {
                    list = wparr;
                    for (i = 0; i < ((Array)write).Count; i++)
                    {
                        fptr = IO.GetOpenFile(caller, IO.rb_io_get_io(((Array)write)[i], caller));
                        if (FD_ISSET(IO.fileno(fptr.f), (fd_set)wp))
                        {
                            list.Add(((Array)write)[i]);
                        }
                        else if (fptr.f2 != null && FD_ISSET(IO.fileno(fptr.f2), (fd_set)wp))
                        {
                            list.Add(((Array)write)[i]);
                        }
                    }
                }

                if (ep != null)
                {
                    list = eparr;
                    for (i = 0; i < ((Array)except).Count; i++)
                    {
                        fptr = IO.GetOpenFile(caller, IO.rb_io_get_io(((Array)except)[i], caller));
                        if (FD_ISSET(IO.fileno(fptr.f), (fd_set)ep))
                        {
                            list.Add(((Array)except)[i]);
                        }
                        else if (fptr.f2 != null && FD_ISSET(IO.fileno(fptr.f2), (fd_set)ep))
                        {
                            list.Add(((Array)except)[i]);
                        }
                    }
                }
            }

            return res;            /* returns an empty array on interrupt */
        }
    }

    
    internal class rb_f_readlines : MethodBody // author: cjs, status: done
    {
        internal static rb_f_readlines singleton = new rb_f_readlines();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            IO.NEXT_ARGF_FORWARD(caller);
            Array ary = new Array();

            String line;
            while ((line = IO.argf_getline(args.ToArray(), caller)) != null)
                //rb_ary_push.singleton.Call1(last_class, ary, caller, null, line);
                ary.Add(line);

            return ary;
        }
    }

    
    internal class rb_f_backquote : MethodBody1 // status: partial, comment: ?
    {
        internal static rb_f_backquote singleton = new rb_f_backquote();

        private void ParseCommand(string command, out string filename, out string args)
        {
            command = command.Trim();

            int start = command.IndexOfAny(new char[] { ' ', '\t' });
            if (start > 0)
            {
                filename = command.Substring(0, start);
                args = command.Substring(start + 1);
            }
            else
            {
                filename = command;
                args = "";
            }
        }

        private bool isInternalCmd(string command)
        {
            switch (command.ToUpperInvariant())
            {
                case "ASSOC":
                case "AT":
                case "ATTRIB":
                case "BREAK":
                case "CACLS":
                case "CALL":
                case "CD":
                case "CHCP":
                case "CHDIR":
                case "CHKDSK":
                case "CHKNTFS":
                case "CLS":
                case "CMD":
                case "COLOR":
                case "COMP":
                case "COMPACT":
                case "CONVERT":
                case "COPY":
                case "DATE":
                case "DEL":
                case "DIR":
                case "DISKCOMP":
                case "DISKCOPY":
                case "DOSKEY":
                case "ECHO":
                case "ENDLOCAL":
                case "ERASE":
                case "EXIT":
                case "FC":
                case "FIND":
                case "FINDSTR":
                case "FOR":
                case "FORMAT":
                case "FTYPE":
                case "GOTO":
                case "GRAFTABL":
                case "HELP":
                case "IF":
                case "LABEL":
                case "MD":
                case "MKDIR":
                case "MODE":
                case "MORE":
                case "MOVE":
                case "PATH":
                case "PAUSE":
                case "POPD":
                case "PRINT":
                case "PROMPT":
                case "PUSHD":
                case "RD":
                case "RECOVER":
                case "REM":
                case "REN":
                case "RENAME":
                case "REPLACE":
                case "RMDIR":
                case "SET":
                case "SETLOCAL":
                case "SHIFT":
                case "SORT":
                case "START":
                case "SUBST":
                case "TIME":
                case "TITLE":
                case "TREE":
                case "TYPE":
                case "VER":
                case "VERIFY":
                case "VOL":
                case "XCOPY":
                    return true;
                default:
                    return false;
            }
        }

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object comArgs)
        {
            string command = ((String)comArgs).value;
            string filename, args;
            ParseCommand(command, out filename, out args);

            System.Diagnostics.Process process = new System.Diagnostics.Process();

            if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
            {
                process.StartInfo.FileName = filename;
                process.StartInfo.Arguments = args;
            }
            else
            {
                string shell = System.Environment.GetEnvironmentVariable("COMSPEC");
                if (shell != null && isInternalCmd(filename))
                {
                    process.StartInfo.FileName = shell;
                    process.StartInfo.Arguments = "/C " + command;
                }
                else
                {
                    filename = filename.Replace('/', '\\');

                    process.StartInfo.FileName = filename;
                    process.StartInfo.Arguments = args;
                }
            }

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            try
            {
                process.Start();
            }
            catch (System.Exception ex)
            {
                throw SystemCallError.rb_sys_fail(command, ex, caller).raise(caller);
            }

            StreamReader sOut = process.StandardOutput;
            string result = sOut.ReadToEnd();
            sOut.Close();
            result = result.Replace("\r\n", "\n");

            StreamReader eOut = process.StandardError;
            string error = eOut.ReadToEnd();
            eOut.Close();
            error = error.Replace("\r\n", "\n");

            if (error != "")
                System.Console.Error.WriteLine(error);

            process.Close();

            return new String(result);
        }
    }

    
    internal class rb_f_p : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_p singleton = new rb_f_p();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            for (int i = 0; i < rest.Count; i++)
            {
                IO.rb_io_write(IO.rb_stdout.value, Object.Inspect(rest[i], caller), caller);
                IO.rb_io_write(IO.rb_stdout.value, IO.rb_default_rs, caller);
            }
            rb_io_flush.singleton.Call0(last_class, IO.rb_stdout.value, caller, null);
            return null;
        }
    }

    
    internal class rb_io_s_new : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_io_s_new singleton = new rb_io_s_new();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array args)
        {
            if (block != null)
                Errors.rb_warn(string.Format(CultureInfo.InvariantCulture, "{0}::new() does not take block; use {0}::open() instead", ((Class)recv)._name));

            return rb_class_new_instance.singleton.Call(last_class, recv, caller, null, args);
        }
    }

    
    internal class rb_io_s_open : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_io_s_open singleton = new rb_io_s_open();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            IO io = (IO)rb_class_new_instance.singleton.Call(last_class, recv, caller, null, rest);

            if (block != null)
            {
                object result = null;
                try
                {
                    result = Proc.rb_yield(block, caller, new object[] { io });
                }
                finally
                {
                    IO.io_close(io, caller);
                }

                return result;
            }

            return io;
        }
    }

    
    internal class rb_io_s_sysopen : VarArgMethodBody1 // author: cjs, status: done
    {
        internal static rb_io_s_sysopen singleton = new rb_io_s_sysopen();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest)
        {
            int numargs = Class.rb_scan_args(caller, rest, 0, 2, false);
            
            string fname = String.StringValue(p1, caller);

            int flags;
            if (numargs > 0)
            {
                if (rest[0] == null)
                    flags = File.O_RDONLY;
                else if (rest[0] is int)
                    flags = (int)rest[0];
                else
                    flags = IO.rb_io_mode_modenum(caller, String.StringValue(rest[0], caller));
            }
            else
                flags = File.O_RDONLY;

            int fmode;
            if (numargs > 1)
                fmode = (int)rest[1];
            else
                fmode = 0666;

            return IO.rb_sysopen(fname, flags, fmode);
        }
    }

    
    internal class rb_io_s_for_fd : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_io_s_for_fd singleton = new rb_io_s_for_fd();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            return rb_class_new_instance.singleton.Call(last_class, recv, caller, null, rest);
        }
    }

    
    internal class rb_io_s_popen : MethodBody // author: cjs, status: partial, comment: pipes
    {
        internal static rb_io_s_popen singleton = new rb_io_s_popen();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            string mode, pname;
            object port;

            if (Class.rb_scan_args(caller, args.ToRubyArray(), 1, 1, false) == 1)
                mode = "r";
            else
            {
                if (args[1] is int)
                    mode = IO.rb_io_modenum_mode(caller, (int)args[1]);
                else
                    mode = IO.rb_io_flags_mode(caller, IO.rb_io_mode_flags(caller, String.StringValue(args[1], caller)));
            }
            pname = String.StringValue(args[0], caller);

            port = IO.pipe_open(null, pname, mode, caller);
            if (port == null)
            {
                /* child */
                if (args.block != null)
                {
                    Proc.rb_yield(args.block, caller, new object[] { null });
                    ((System.IO.Stream)IO.rb_stdout.value).Flush();
                    ((System.IO.Stream)IO.rb_stderr.value).Flush();

                    try
                    {
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                    }
                    catch { }
                }
                return null;
            }

            ((Basic)port).my_class = ((Basic)recv).my_class;
            if (args.block != null)
            {
                object result;
                try
                {
                    result = Proc.rb_yield(args.block, caller, new object[] { port });
                }
                finally
                {
                    IO.io_close(port, caller);
                }

                return result;
            }
            return port;
        }
    }




    internal class rb_io_s_foreach : MethodBody // author: cjs, status: done
    {
        internal static rb_io_s_foreach singleton = new rb_io_s_foreach();

        private class foreach_arg
        {
            internal object sep;
            internal object io;
        }

        private static object io_s_foreach(Frame caller, Proc block, foreach_arg arg)
        {
            String str;

            while ((str = IO.rb_io_getline(arg.sep, arg.io, caller)) != null)
            {
                Proc.rb_yield(block, caller, str);
            }

            return null;
        }

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            string fname;
            foreach_arg arg = new foreach_arg();

            if (Class.rb_scan_args(caller, args.ToRubyArray(), 1, 1, false) == 2)
                arg.sep = String.StringValue(args[1], caller);
            fname = String.StringValue(args[0], caller);

            if (args.Length == 1)
            {
                arg.sep = String.StringValue(IO.rb_default_rs.value, caller);
            }
            
            arg.io = IO.rb_io_open(fname, "r", caller);
            if (arg.io == null)
                return null;

            object result = null;
            try
            {
                result = io_s_foreach(caller, args.block, arg);
            }
            finally
            {
                IO.rb_io_close(arg.io);
            }

            return result;
        }
    }

    
    internal class rb_io_s_readlines : MethodBody // author: cjs, status: done
    {
        internal static rb_io_s_readlines singleton = new rb_io_s_readlines();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            string fname;

            int argc = Class.rb_scan_args(caller, args.ToRubyArray(), 1, 1, false);
            fname = String.StringValue(args[0], caller);

            IO io = IO.rb_io_open(fname, "r", caller);
            if (io == null)
                return null;

            object result = null;
            try
            {
                if (argc == 2)
                    result = rb_io_readlines.singleton.Call1(last_class, io, caller, args.block, args[1]);
                else
                    result = rb_io_readlines.singleton.Call0(last_class, io, caller, args.block);
            }
            finally
            {
                IO.rb_io_close(io);
            }

            return result;
        }
    }

    
    internal class rb_io_s_read : MethodBody // author: cjs, status: done
    {
        internal static rb_io_s_read singleton = new rb_io_s_read();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            string fname;
            int offset = -1;
            object sep = null;

            Class.rb_scan_args(caller, args.ToRubyArray(), 1, 2, false);

            if (args.Length > 1)
                sep = args[1];
            if (args.Length > 2)
                offset = Numeric.rb_num2long(args[2], caller);
            fname = String.StringValue(args[0], caller);

            IO io = IO.rb_io_open(fname, "r", caller);
            if (io == null)
                return null;

            if (offset > -1)
            {
                IO.rb_io_seek(caller, io, offset, IO.SEEK_SET);
            }

            object result = null;
            try
            {
                result = io_read.singleton.Call(last_class, io, caller, args.block, new Array(new object[] { null, sep }));
            }
            finally
            {
                IO.rb_io_close(io);
            }

            return result;
        }
    }

    
    internal class rb_io_s_pipe : MethodBody0 // author: cjs, status: unimplemented, comment: pipes
    {
        internal static rb_io_s_pipe singleton = new rb_io_s_pipe();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw new NotImplementedError("rb_io_s_pipe").raise(caller);

            //int[] pipes = new int[2];

            //object r, w;
            //object[] args = new object[2];

            //if (IO._pipe(pipes, 1024, IO.O_BINARY) == -1)
            //    throw SystemCallError.rb_sys_fail(null, null, caller).raise(caller);

            //args[0] = pipes[0];
            //args[1] = IO.O_RDONLY;

            //r = rb_class_new_instance.singleton.Call(last_class, Ruby.Runtime.Init.rb_cIO, caller, null, new Array(args));

            //args[0] = pipes[1];
            //args[1] = IO.O_WRONLY;
            //w = rb_class_new_instance.singleton.Call(last_class, Ruby.Runtime.Init.rb_cIO, caller, null, new Array(args));
            ////rb_io_synchronized(RFILE(w)->fptr);

            //return new Array(r, w);
        }
    }

    
    internal class rb_io_stat : MethodBody0 // author: cjs, status: done
    {
        internal static rb_io_stat singleton = new rb_io_stat();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO f = IO.GetOpenFile(caller, recv);

            return new FileStat(f._path);
        }
    }


    
    internal class rb_io_initialize : VarArgMethodBody1 // author: cjs, status: done
    {
        internal static rb_io_initialize singleton = new rb_io_initialize();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object file, Array rest)
        {
            IO io = ((IO)recv);

            Eval.rb_secure(4, caller);

            int flags;
            if (rest.Count > 0)
            {
                if (rest[0] is int)
                    flags = Numeric.rb_num2long(rest[0], caller);
                else
                    flags = IO.rb_io_mode_modenum(caller, String.StringValue(rest[0], caller));
            }
            else
            {
                flags = IO.O_RDONLY;
            }

            FileMode file_mode = IO.modenumToFileMode(flags);
            FileAccess access = IO.modenumToFileAccess(flags);
            int ruby_mode = IO.rb_io_modenum_flags(flags);

            if (file is int)
            {
                io.Init(IO.rb_fdopen(caller, (int)file, IO.rb_io_modenum_mode(caller, flags)), ruby_mode);
            }
            else
            {
                string name = String.StringValue(file, caller);

                io.Init(name, file_mode, access, ruby_mode);
            }
            
            return io;
        }
    }

    
    internal class rb_io_init_copy : MethodBody1 // author: cjs, status: done
    {
        internal static rb_io_init_copy singleton = new rb_io_init_copy();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            IO fptr, orig;
            Microsoft.Win32.SafeHandles.SafeFileHandle fd;

            IO io = IO.rb_io_get_io(recv, caller);
            IO dest = IO.rb_io_get_io(p1, caller);

            if (Object.Equal(dest, io, caller))
                return dest;

            orig = IO.GetOpenFile(caller, io);
            fptr = IO.MakeOpenFile(dest);

            if (orig.f2 != null)
            {
                IO.io_fflush(orig.f2, orig);
                orig.f.Seek(0, SeekOrigin.Begin);
            }
            else if ((orig.mode & IO.FMODE_WRITABLE) > 0)
            {
                IO.io_fflush(orig.f, orig);
            }
            else
            {
                orig.f.Seek(0, SeekOrigin.Begin);
            }

            /* copy OpenFile structure */
            fptr.mode = orig.mode;
            fptr._pid = orig._pid;
            fptr.oflineno = orig.oflineno;
            if (orig._path != null)
                fptr._path = orig._path;
            fptr.finalize = orig.finalize;

            fd = ((System.IO.FileStream)orig.f).SafeFileHandle;
            fptr.f = new System.IO.FileStream(fd, IO.modenumToFileAccess(fptr.mode));
            fptr.f.Seek(orig.f.Position, SeekOrigin.Begin);
            if (orig.f2 != null)
            {
                if (!((System.IO.FileStream)orig.f).SafeFileHandle.Equals(((System.IO.FileStream)orig.f2).SafeFileHandle))
                {
                    fd = ((System.IO.FileStream)orig.f).SafeFileHandle;
                }
                fptr.f2 = new System.IO.FileStream(fd, FileAccess.Write);
                fptr.f2.Seek(orig.f2.Position, SeekOrigin.Begin);
            }
            
            if ((fptr.mode & IO.FMODE_BINMODE) > 0)
            {
                IO.rb_io_binmode(dest, caller);
            }

            return dest;
        }
    }

    
    internal class rb_io_reopen : MethodBody // author: cjs, status: done
    {
        internal static rb_io_reopen singleton = new rb_io_reopen();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            IO file = (IO)recv;

            string fname, nmode = null;
            string mode;
            IO fptr;

            Eval.rb_secure(4, caller);
            if (Class.rb_scan_args(caller, args.ToRubyArray(), 1, 1, false) == 1)
            {
                object tmp = IO.rb_io_check_io(args[0], caller);
                if (tmp != null)
                {
                    return IO.io_reopen(recv, tmp, caller);
                }
            }

            fname = String.StringValue(args[0], caller);
            IO.rb_io_taint_check(caller, recv);
            fptr = file;

            if (nmode != null)
            {
                fptr.mode = IO.rb_io_mode_flags(caller, nmode);
            }

            if (fptr._path != null)
            {
                fptr._path = null;
            }

            fptr._path = fname;
            mode = IO.rb_io_flags_mode(caller, fptr.mode);
            if (fptr.f == null)
            {
                fptr.f = new FileStream(fptr._path, IO.modenumToFileMode(fptr.mode), IO.modenumToFileAccess(fptr.mode));
                if (fptr.f2 != null)
                {
                    fptr.f2.Close();
                }
                return recv;
            }

            try
            {
                fptr.f.Close();
                fptr.f = new FileStream(fptr._path, IO.modenumToFileMode(fptr.mode), IO.modenumToFileAccess(fptr.mode));
            }
            catch (System.IO.IOException ioe)
            {
                throw SystemCallError.rb_sys_fail(fptr._path, ioe, caller).raise(caller);
            }

            if (fptr.f2 != null)
            {
                try
                {
                    fptr.f2.Close();
                    fptr.f2 = new FileStream(fptr._path, IO.modenumToFileMode(fptr.mode), FileAccess.Write);
                }
                catch (System.IO.IOException ioe)
                {
                    throw SystemCallError.rb_sys_fail(fptr._path, ioe, caller).raise(caller);
                }
            }

            return recv;
        }
    }

    
    internal class rb_io_print : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_io_print singleton = new rb_io_print();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            if (rest.Count == 0)
            {
                rest.Add(Eval.rb_lastline.value);
            }

            for (int i = 0; i < rest.Count; i++)
            {
                    if (IO.rb_output_fs.value != null && i > 0)
                        IO.rb_io_write(recv, IO.rb_output_fs.value, caller);

                    if (rest[i] == null)
                        IO.rb_io_write(recv, new String("nil"), caller);
                    else
                        IO.rb_io_write(recv, rest[i], caller);
            }

            if (IO.rb_output_rs.value != null)
                IO.rb_io_write(recv, IO.rb_output_rs.value, caller);
            
            return null;
        }
    }

    
    internal class rb_io_putc : MethodBody1 // author: cjs, status: done
    {
        internal static rb_io_putc singleton = new rb_io_putc();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object ch)
        {
            char c;
            if (ch is String && ((String)ch).value.Length >= 1)
                c = ((String)ch).value[0];
            else
                c = System.Convert.ToChar(ch, CultureInfo.InvariantCulture);

            IO.rb_io_write(recv, new String(c.ToString()), caller);

            return ch;
        }
    }

    
    internal class rb_io_puts : MethodBody // author: cjs, status: done
    {
        internal static rb_io_puts singleton = new rb_io_puts();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 0)
            {
                IO.rb_io_write(recv, IO.rb_default_rs.value, caller);
                return null;
            }

            foreach (object arg in args)
            {
                String line;

                if (arg == null)
                    line = new String("nil");
                else
                {
                    Array ary;
                    if (Array.TryToArray(arg, out ary, caller))
                    {
                        try
                        {
                            Array.StartInspect(ary);
                            IO.io_puts_ary(ary, (IO)recv, caller);
                        }
                        finally
                        {
                            Array.EndInspect(ary);
                        }
                        continue;
                    }
                    else 
                        line = String.ObjectAsString(arg, caller);
                }

                IO.rb_io_write(recv, line, caller);

                if (line.value.Length == 0 || !line.value.EndsWith("\n"))
                    IO.rb_io_write(recv, IO.rb_default_rs, caller);
            }

            return null;
        }
    }

    
    internal class rb_io_printf : MethodBody // author: cjs, status: done
    {
        internal static rb_io_printf singleton = new rb_io_printf();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            IO.rb_io_write(recv, rb_f_sprintf.singleton.Call(last_class, null, caller, null, args.GetRest()), caller);
            return null;
        }
    }

    
    internal class rb_io_each_line : MethodBody // author: cjs, status: done
    {
        internal static rb_io_each_line singleton = new rb_io_each_line();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            object str;
            object rs;

            if (args.Length == 0)
            {
                rs = IO.rb_rs.value;
            }
            else
            {
                Class.rb_scan_args(caller, args.ToRubyArray(), 1, 0, false);
                rs = args[0];
            }

            while ((str = IO.rb_io_getline(rs, recv, caller)) != null)
            {
                Proc.rb_yield(args.block, caller, new object[] { str });
            }

            return recv;
        }
    }

    
    internal class rb_io_each_byte : MethodBody0 // author: cjs, status: partial, comment: threads
    {
        internal static rb_io_each_byte singleton = new rb_io_each_byte();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO fptr = IO.GetOpenFile(caller, recv);

            Stream f;
            int c;
            try
            {
                for (; ; )
                {
                    IO.rb_io_check_readable(caller, fptr);
                    f = fptr.f;
                    IO.READ_CHECK(caller, f, fptr);
                    //TRAP_BEG;
                    try
                    {
                        if ((c = fptr.Read(caller)) == -1)
                            break;
                    }
                    catch (System.IO.IOException ioe)
                    {
                        if (!IO.rb_io_wait_readable(f, ioe))
                            throw SystemCallError.rb_sys_fail(fptr._path, ioe, caller).raise(caller);
                        continue;
                    }
                    //TRAP_END;
                    Proc.rb_yield(block, caller, new object[] { c & 0xff });
                }
            }
            catch (System.IO.IOException ioe)
            {
                throw SystemCallError.rb_sys_fail(fptr._path, ioe, caller).raise(caller);
            }
            
            return recv;
        }
    }

    
    internal class rb_io_syswrite : MethodBody1 // author: cjs, status: partial, comment: threads
    {
        internal static rb_io_syswrite singleton = new rb_io_syswrite();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            IO fptr;
            Stream f;
            long n;

            Eval.rb_secure(4, caller);
            String str;
            if (!(p1 is String))
                str = String.ObjectAsString(p1, caller);
            else
                str = (String)p1;

            fptr = IO.GetOpenFile(caller, recv);
            IO.rb_io_check_writable(caller, fptr);
            f = IO.GetWriteFile(fptr);

            if ((fptr.mode & IO.FMODE_WBUF) > 0)
            {
                Errors.rb_warn("syswrite for buffered IO");
            }
            //if (!IO.rb_thread_fd_writable(fileno(f)))
            {
                IO.rb_io_check_closed(caller, fptr);
            }

            try
            {
                new System.IO.StreamWriter(f).Write(str.value);
                n = str.value.Length;
            }
            catch (System.Exception e)
            {
                throw SystemCallError.rb_sys_fail(fptr._path, e, caller).raise(caller);
            }

            return n;
        }
    }

    
    internal class rb_io_sysread : VarArgMethodBody0 // author: cjs, status: partial, comment: threads
    {
        internal static rb_io_sysread singleton = new rb_io_sysread();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            String str = null;
            IO fptr;
            int n, ilen;

            if (Class.rb_scan_args(caller, argv, 1, 1, false) == 2)
                str = String.RStringValue(argv[1], caller);
            ilen = Numeric.rb_num2long(argv[0], caller);

            if (str == null)
            {
                str = new String(new string('\0', ilen));
            }
            else
            {
                //rb_str_modify(str);
                str.value += new string('\0', str.value.Length - ilen);
            }
            if (ilen == 0)
                return str;

            fptr = IO.GetOpenFile(caller, recv);
            IO.rb_io_check_readable(caller, fptr);

            //rb_str_locktmp(str);

            n = IO.fileno((System.IO.FileStream)fptr.f);
            //rb_thread_wait_fd(fileno(fptr->f));
            IO.rb_io_check_closed(caller, fptr);
            if (str.value.Length != ilen)
            {
                throw new RuntimeError("buffer string modified").raise(caller);
            }
            //TRAP_BEG;
            char[] cbuf = new char[ilen];
            n = new System.IO.StreamReader(fptr.f).Read(cbuf, 0, ilen);
            //TRAP_END;

            //rb_str_unlocktmp(str);
            if (n == -1)
            {
                throw SystemCallError.rb_sys_fail(fptr._path, new IOException(), caller).raise(caller);
            }

            if (n == 0 && ilen > 0)
            {
                throw EOFError.rb_eof_error().raise(caller);
            }

            str.value = new string(cbuf);
            str.Tainted = true;

            return str;
        }
    }

    
    internal class rb_io_fileno : MethodBody0 // author: cjs, status: done
    {
        internal static rb_io_fileno singleton = new rb_io_fileno();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO fptr = IO.GetOpenFile(caller, recv);
            return IO.fileno(fptr.f);
        }
    }

    
    internal class rb_io_to_io : MethodBody0 // status: done
    {
        internal static rb_io_to_io singleton = new rb_io_to_io();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return recv;
        }
    }

    
    internal class rb_io_fsync : MethodBody0 // author: cjs, status: done
    {
        internal static rb_io_fsync singleton = new rb_io_fsync();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw NotImplementedError.rb_notimplement(caller, "fsync").raise(caller);
        }
    }

    
    internal class rb_io_sync : MethodBody0 // status: done
    {
        internal static rb_io_sync singleton = new rb_io_sync();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((IO)recv).AutoFlush(caller);
        }
    }

    
    internal class rb_io_set_sync : MethodBody1 // status: done
    {
        internal static rb_io_set_sync singleton = new rb_io_set_sync();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            ((IO)recv).set_AutoFlush(caller, p1 != null);
            return recv;
        }
    }

    
    internal class rb_io_lineno : MethodBody0 // status: done
    {
        internal static rb_io_lineno singleton = new rb_io_lineno();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((IO)recv).oflineno;
        }
    }

    
    internal class rb_io_set_lineno : MethodBody1 // author: cjs, status: done
    {
        internal static rb_io_set_lineno singleton = new rb_io_set_lineno();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            IO io = (IO)recv;

            IO.rb_io_check_readable(caller, io);

            return ((IO)recv).oflineno = Numeric.rb_num2long(p1, caller);
        }
    }

    
    internal class rb_io_readlines : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_io_readlines singleton = new rb_io_readlines();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            String rs = null;

            int argc = argv.Count;
            if (argc == 0)
            {
                rs = (String)IO.rb_rs.value;
            }
            else
            {
                Class.rb_scan_args(caller, argv, 1, 0, false);
                if (argv[0] != null)
                    rs = String.RStringValue(argv[0], caller);
            }

            Array ary = new Array();
            String line;
            while ((line = IO.rb_io_getline(rs, recv, caller)) != null)
            {
                ary.Add(line);
            }
            
            return ary;
        }
    }

    
    internal class io_read : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static io_read singleton = new io_read();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            IO io = (IO)recv;

            object length = null;
            String str = null;
            int numargs = Class.rb_scan_args(caller, rest, 0, 2, false);
            if (numargs > 0)
                length = rest[0];
            if (numargs > 1 && rest[1] != null)
                str = String.RStringValue(rest[1], caller);

            IO fptr;
            if (length == null)
            {
                fptr = IO.GetOpenFile(caller, io);
                IO.rb_io_check_readable(caller, fptr);
                if (str == null)
                    str = new String();
                str.value = io.ReadToEnd(caller);
                str.Tainted = true;
                return str;
            }

            int len = 0;
            if (numargs > 0)
                len = Numeric.rb_num2long(rest[0], caller);
            if (len < 0)
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "negative length {0} given", length)).raise(caller);

            if (str == null)
            {
                str = new String();
                str.Tainted = true;
            }
            else
            {
                //rb_str_modify(str);
                if (str.value.Length > len)
                    str.value = str.value.Substring(0, len);
            }

            fptr = IO.GetOpenFile(caller, io);
            IO.rb_io_check_readable(caller, fptr);
            if (io.EndOfStream(caller))
                return null;
            if (len == 0)
                return str;

            //rb_str_locktmp(str);
            IO.READ_CHECK(caller, fptr.f, fptr);
            string buf = io.ReadBuffer(caller, len);
            //rb_str_unlocktmp(str);

            if (buf.Length == 0)
            {
                if (fptr.f == null)
                    return null;
                if (io.EndOfStream(caller))
                {
                    str.value = string.Empty;
                    return null;
                }
                if (len > 0)
                    SystemCallError.rb_sys_fail(fptr._path, new EndOfStreamException(), caller);
            }

            str.value = buf;
            str.Tainted = true;

            return str;
        }
    }

    
    internal class io_write : MethodBody1 // author: cjs, status: done
    {
        internal static io_write singleton = new io_write();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Eval.rb_secure(4, caller);

            String str;
            if (p1 is String)
                str = (String)p1;
            else
                str = String.ObjectAsString(p1, caller);

            if (!(recv is IO))
                return Eval.CallPrivate(recv, caller, "write", null, new object[] { p1 });

            if (str.value.Length == 0)
                return 0;

            IO io = (IO)recv;
            try
            {
                io.Write(caller, str.value);
            }
            catch (System.IO.IOException e)
            {
                throw SystemCallError.rb_sys_fail(io._path, e, caller).raise(caller);
            }

            return str.value.Length;
        }
    }

    
    internal class rb_io_gets_m : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_io_gets_m singleton = new rb_io_gets_m();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            string rs;
            String str;

            if (rest.Count == 0)
                rs = String.StringValue(IO.rb_rs.value, caller);
            else
            {
                Class.rb_scan_args(caller, rest, 1, 0, false);
                if (rest[0] == null)
                    rs = null;
                else
                    rs = String.StringValue(rest[0], caller);
            }

            str = IO.rb_io_getline(rs, recv, caller);

            Eval.rb_lastline_set(caller, str);

            return str;
        }
    }

    
    internal class rb_io_readline : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_io_readline singleton = new rb_io_readline();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            object line = rb_io_gets_m.singleton.Call(last_class, recv, caller, null, rest);

            if (line == null)
                throw EOFError.rb_eof_error().raise(caller);

            return line;
        }
    }

    
    internal class rb_io_getc : MethodBody0 // author: cjs, status: done
    {
        internal static rb_io_getc singleton = new rb_io_getc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO io = ((IO)recv);

            IO.rb_io_check_readable(caller, io);

            IO.READ_CHECK(caller, io.f, io);

            if (io.EndOfStream(caller))
                return null;
            else
            {
                int c;

                //TRAP_BEG;
                c = io.Read(caller);
                //TRAP_END;

                return c & 0xff;
            }
        }
    }

    
    internal class rb_io_readchar : MethodBody0 // status: done
    {
        internal static rb_io_readchar singleton = new rb_io_readchar();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((IO)recv).Read(caller);
        }
    }

    
    internal class rb_io_ungetc : MethodBody1 // author: cjs, status: done
    {
        internal static rb_io_ungetc singleton = new rb_io_ungetc();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            int c = Numeric.rb_num2long(p1, caller);

            IO fptr = IO.GetOpenFile(caller, recv);
            if (!((fptr.mode & IO.FMODE_RBUF) > 0))
                throw new IOError("unread stream").raise(caller);
            IO.rb_io_check_readable(caller, fptr);

            if (fptr.Ungetc(c, caller) == IO.EOF && c != IO.EOF)
            {
                throw new IOError("ungetc failed").raise(caller);
            }

            return null;
        }
    }

    
    internal class rb_io_addstr : MethodBody // author: cjs, status: done
    {
        internal static rb_io_addstr singleton = new rb_io_addstr();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return IO.rb_io_write(recv, p1, caller);
        }
    }

    
    internal class rb_io_flush : MethodBody0 // status: done
    {
        internal static rb_io_flush singleton = new rb_io_flush();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ((IO)recv).Flush(caller);
            return recv;
        }
    }

    
    internal class rb_io_tell : MethodBody0 // author: cjs, status: done
    {
        internal static rb_io_tell singleton = new rb_io_tell();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO fptr = IO.GetOpenFile(caller, recv);

            try
            {
                long pos = IO.io_tell(fptr);
                return Bignum.NormaliseUsing(IronMath.integer.make(pos));
            }
            catch (System.Exception e)
            {
                throw SystemCallError.rb_sys_fail(fptr._path, e, caller).raise(caller);
            }
        }
    }

    
    internal class rb_io_seek_m : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_io_seek_m singleton = new rb_io_seek_m();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            IO io = (IO)recv;

            int whence = IO.SEEK_SET;
            if (Class.rb_scan_args(caller, argv, 1, 1, false) == 2)
            {
                whence = Numeric.rb_num2long(argv[1], caller);
            }
            int offset = Numeric.rb_num2long(argv[0], caller);

            IO.rb_io_seek(caller, io, offset, whence);
            return 0;
        }
    }

    
    internal class rb_io_rewind : MethodBody0 // author: cjs, status: done
    {
        internal static rb_io_rewind singleton = new rb_io_rewind();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO io = (IO)recv;

            IO fptr = IO.GetOpenFile(caller, io);
            try
            {
                IO.io_seek(fptr, 0, 0);
            }
            catch (System.Exception e)
            {
                throw SystemCallError.rb_sys_fail(fptr._path, e, caller).raise(caller);
            }

            if (io == IO.current_file)
            {
                IO.gets_lineno -= fptr.oflineno;
            }
            fptr.oflineno = 0;

            return 0;
        }
    }

    
    internal class rb_io_set_pos : MethodBody1 // status: done
    {
        internal static rb_io_set_pos singleton = new rb_io_set_pos();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return (int)(((IO)recv).set_Position(caller, Numeric.rb_num2long(p1, caller)));
        }
    }

    
    internal class rb_io_eof : MethodBody0 // status: done
    {
        internal static rb_io_eof singleton = new rb_io_eof();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((IO)recv).EndOfStream(caller);
        }
    }

    
    internal class rb_io_close_m : MethodBody0 // author: cjs, status: done
    {
        internal static rb_io_close_m singleton = new rb_io_close_m();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO io = (IO)recv;
            if (Eval.rb_safe_level() >= 4 && !io.Tainted)
                throw new SecurityError("Insecure: can't close").raise(caller);

            IO.rb_io_check_closed(caller, io);
            IO.rb_io_close(io);
            return null;
        }
    }

    
    internal class rb_io_closed : MethodBody0 // author: cjs, status: done
    {
        internal static rb_io_closed singleton = new rb_io_closed();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return IO.rb_io_closed(caller, recv);
        }
    }

    
    internal class rb_io_close_read : MethodBody0 // status: done
    {
        internal static rb_io_close_read singleton = new rb_io_close_read();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO io = (IO)recv;
            if (Eval.rb_safe_level() >= 4 && !io.Tainted)
                throw new SecurityError("Insecure: can't close").raise(caller);
            io.CloseRead();
            return recv;
        }
    }

    
    internal class rb_io_close_write : MethodBody0 // status: done
    {
        internal static rb_io_close_write singleton = new rb_io_close_write();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO io = (IO)recv;
            if (Eval.rb_safe_level() >= 4 && !io.Tainted)
                throw new SecurityError("Insecure: can't close").raise(caller);
            io.CloseWrite();
            return recv;
        }
    }

    
    internal class rb_io_isatty : MethodBody1 // status: done
    {
        internal static rb_io_isatty singleton = new rb_io_isatty();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return !(((IO)recv).f is FileStream);
        }
    }

    
    internal class rb_io_binmode : MethodBody0 // author: cjs, status: done
    {
        internal static rb_io_binmode singleton = new rb_io_binmode();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return IO.rb_io_binmode(recv, caller);
        }
    }

    
    internal class rb_io_sysseek : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_io_sysseek singleton = new rb_io_sysseek();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            IO io = (IO)recv;

            int whence = IO.SEEK_SET;
            IO fptr;
            long pos;

            if (Class.rb_scan_args(caller, argv, 1, 1, false) == 2)
            {
                whence = Numeric.rb_num2long(argv[1], caller);
            }
            pos = Numeric.rb_num2long(argv[0], caller);
            fptr = IO.GetOpenFile(caller, io);
            if ((fptr.mode & IO.FMODE_WRITABLE) > 0 && (fptr.mode & IO.FMODE_WBUF) > 0)
            {
                Errors.rb_warn("sysseek for buffered IO");
            }
            try
            {
                SeekOrigin so;
                if (whence == IO.SEEK_END)
                    so = SeekOrigin.End;
                else if (whence == IO.SEEK_CUR)
                    so = SeekOrigin.Current;
                else //if (whence == IO.SEEK_SET) // default to begin
                    so = SeekOrigin.Begin;

                pos = fptr.f.Seek(pos, so);
            }
            catch (System.IO.IOException ioe)
            {
                throw SystemCallError.rb_sys_fail(fptr._path, ioe, caller).raise(caller);
            }
            if (pos == -1)
                throw SystemCallError.rb_sys_fail(fptr._path, new IOException(), caller).raise(caller);

            Bignum bpos = new Bignum(pos);

            return Bignum.NormaliseUsing(bpos.value);
        }
    }



    
    internal class rb_io_ioctl : VarArgMethodBody1 // author: cjs, status: partial, comment: threading, ioctl
    {
        internal static rb_io_ioctl singleton = new rb_io_ioctl();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest)
        {
            Class.rb_scan_args(caller, rest, 1, 1, false);
            
            object arg = rest.Count > 0 ? rest[0] : null;
            
            return IO.rb_io_ctl(recv, p1, arg, true, caller);
        }
    }

    
    internal class rb_io_fcntl : MethodBody // author: cjs, status: done
    {
        internal static rb_io_fcntl singleton = new rb_io_fcntl();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            throw NotImplementedError.rb_notimplement(caller, "fcntl").raise(caller);
        }
    }

    
    internal class rb_io_pid : MethodBody0 // status: done
    {
        internal static rb_io_pid singleton = new rb_io_pid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            int pid = ((IO)recv)._pid;
            return (pid == 0) ? null : (object)pid;
        }
    }

    
    internal class rb_io_inspect : MethodBody0 // author: cjs, status: done
    {
        internal static rb_io_inspect singleton = new rb_io_inspect();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            string buf, cname, st = "";

            IO fptr = ((IO)IO.rb_io_taint_check(caller, recv));

            if (fptr == null || fptr._path == null)
                return Object.rb_any_to_s(recv);

            cname = ((Class)recv)._name;
            
            if (fptr.f == null && fptr.f2 == null)
            {
                st = " (closed)";
            }

            buf = string.Format(CultureInfo.InvariantCulture, "#<{0}:{1}{2}>", cname, fptr._path, st);

            return new String(buf);
        }
    }
}
