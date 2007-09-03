/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Ruby.Runtime;
using Ruby;
using System.Globalization;


namespace Ruby
{
    [UsedByRubyCompiler]
    public partial class IO : Basic
    {
        private UngetcStreamReader reader = null;
        private StreamWriter writer = null;

        // OpenFile
        internal delegate void IOFinalize(IO fptr, bool noraise);
        internal int mode;
        internal int _pid;
        internal int oflineno;
        internal string _path;
        internal IOFinalize finalize;
        internal Stream f;
        internal Stream f2;

        //ARGF
        internal static bool argf_binmode = false;

        // -----------------------------------------------------------------------------

        internal static readonly_global argf = new readonly_global();     // $<
        internal static str_global rb_output_fs = new str_global();       // $,
        internal static str_global rb_rs = new str_global();              // $/ (current value)
        internal static str_global rb_output_rs = new str_global();       // $\
        internal static lineno_global lineno_global = new lineno_global();       // (lineno_setter) $. (int)
        internal static global_variable rb_stdin = new global_variable();
        internal static stdout_global rb_stdout = new stdout_global();    // (stdout_setter)
        internal static stdout_global rb_stderr = new stdout_global();    // (stdout_setter) 
        internal static defout_global defout = new defout_global();       // (defout_setter -> rb_stdout)
        internal static deferr_global deferr = new deferr_global();       // (deferr_setter -> rb_stderr)
        internal static readonly_global filename = new readonly_global(); // String containing name of current file
        internal static op_i_global opt_i = new op_i_global();            // (opt_i_get, opt_i_set)

        internal static String rb_default_rs;                             // $/ (default value)        
        internal static IO orig_stdout, orig_stderr;                      // IO objects for original stdout and stderr 
        internal static object current_file;
        internal static int gets_lineno;
        internal static int init_p = 0, next_p = 0;
        internal static string ruby_inplace_mode;

        private static pipe_list _pipe_list = null;

        // -----------------------------------------------------------------------------

        internal const int FMODE_READABLE = 1;
        internal const int FMODE_WRITABLE = 2;
        internal const int FMODE_READWRITE = 3;
        internal const int FMODE_APPEND = 64;
        internal const int FMODE_CREATE = 128;
        internal const int FMODE_BINMODE = 4;
        internal const int FMODE_SYNC = 8;
        internal const int FMODE_WBUF = 16;
        internal const int FMODE_RBUF = 32;

        internal const int SEEK_SET = 0;
        internal const int SEEK_CUR = 1;
        internal const int SEEK_END = 2;

        private const int _O_RDONLY = 0x0000;  /* open for reading only */
        private const int _O_WRONLY = 0x0001;  /* open for writing only */
        private const int _O_RDWR = 0x0002;  /* open for reading and writing */
        private const int _O_APPEND = 0x0008;  /* writes done at eof */
        private const int _O_CREAT = 0x0100;  /* create and open file */
        private const int _O_TRUNC = 0x0200;  /* open and truncate */
        private const int _O_EXCL = 0x0400;  /* open only if file doesn't already exist */
        private const int _O_TEXT = 0x4000;  /* file mode is text (translated) */
        private const int _O_BINARY = 0x8000;  /* file mode is binary (untranslated) */

        internal const int O_RDONLY = _O_RDONLY;
        internal const int O_WRONLY = _O_WRONLY;
        internal const int O_RDWR = _O_RDWR;
        internal const int O_APPEND = _O_APPEND;
        internal const int O_CREAT = _O_CREAT;
        internal const int O_TRUNC = _O_TRUNC;
        internal const int O_EXCL = _O_EXCL;
        internal const int O_BINARY = _O_BINARY;

        internal const int EOF = -1;

        // -----------------------------------------------------------------------------


        public IO(Class klass)
            : base(klass)
        {
        }

        internal IO(Stream stream)
            : base(Ruby.Runtime.Init.rb_cIO)
        {
            if (stream.CanRead)
                reader = new UngetcStreamReader(stream);

            if (stream.CanWrite)
            {
                writer = new StreamWriter(stream);
                writer.AutoFlush = true;
            }

            this.f = stream;
        }


        // -----------------------------------------------------------------------------

        internal override object Inner()
        {
            return f;
        }

        private void InitOpenFile(int mode, int pid, int lineno, string path, IOFinalize finalize, Stream f, Stream f2)
        {
            this.mode = mode;
            this._pid = pid;
            this.oflineno = lineno;
            this._path = path;
            this.finalize = finalize;
            this.f = f;
            this.f2 = f2;
        }

        internal void Init(Stream stream, int mode)
        {
            InitOpenFile(mode, 0, 0, null, null, stream, null);

            if (stream.CanRead)
                reader = new UngetcStreamReader(stream);

            if (stream.CanWrite)
            {
                writer = new StreamWriter(stream);
                writer.AutoFlush = true;
            }
        }

        internal void Init(string name, FileMode fm, FileAccess fa, int mode)
        {
            Stream stream = new FileStream(name, fm, fa);
            InitOpenFile(mode, 0, 0, name, null, stream, null);

            if (fa.Equals(FileAccess.Read) || fa.Equals(FileAccess.ReadWrite))
            {
                reader = new UngetcStreamReader(stream);
            }

            if (fa.Equals(FileAccess.Write) || fa.Equals(FileAccess.ReadWrite))
            {
                writer = new StreamWriter(stream);
            }
        }

        private bool TextMode
        {
            get { return (mode & FMODE_BINMODE) == 0; }
        }

        internal bool AutoFlush(Frame caller)
        {
            return Writer(caller).AutoFlush;
        }

        internal void set_AutoFlush(Frame caller, bool value)
        {
            Writer(caller).AutoFlush = value;
        }

        private UngetcStreamReader Reader(Frame caller)
        {
            if (reader != null)
                return reader;

            throw new IOError("not opened for reading").raise(caller);
        }


        private StreamWriter Writer(Frame caller)
        {
            if (writer != null)
                return writer;

            throw new IOError("not opened for writing").raise(caller);
        }

        internal int Read(Frame caller)
        {
            int ch = Reader(caller).Read();

            if (TextMode && ch == '\r' && Reader(caller).Peek() == '\n')
                return Reader(caller).Read();
            else
                return ch;
        }

        internal string ReadLine(Frame caller)
        {
            return Reader(caller).ReadLine();
        }

        internal string ReadBuffer(Frame caller, int length)
        {
            char[] buffer = new char[length];
            Reader(caller).ReadBlock(buffer, 0, length);
            string str = new string(buffer);
            if (TextMode)
                str = str.Replace("\r\n", "\n");
            return str;
        }

        internal string ReadToEnd(Frame caller)
        {
            string str = Reader(caller).ReadToEnd();
            if (TextMode)
                str = str.Replace("\r\n", "\n");
            return str;
        }

        internal int Peek(Frame caller)
        {
            int ch = Reader(caller).Peek();

            if (ch != '\r')
                return ch;
            else
            {
                Reader(caller).Read();
                return Peek(caller);
            }
        }

        internal int set_Position(Frame caller, int value)
        {
            Reader(caller).BaseStream.Position = value;
            return value;
        }

        internal void Seek(Frame caller, int offset, SeekOrigin origin)
        {
            Reader(caller).BaseStream.Seek(offset, origin);
        }

        internal void CloseRead()
        {
            if (reader != null)
            {
                try
                {
                    Stream stream = reader.BaseStream;
                    reader.Close(); //can't close as it will close the basestream
                    reader = null;
                    if (writer == null)
                        stream.Close();
                }
                catch (System.ObjectDisposedException) { }
            }
        }

        internal bool EndOfStream(Frame caller)
        {
            return Reader(caller).EndOfStream;
        }

        internal void Write(Frame caller, string str)
        {
            Writer(caller).Write(str.Replace("\n", "\r\n"));
        }

        internal void Flush(Frame caller)
        {
            Writer(caller).Flush();
        }

        internal void CloseWrite()
        {
            if (writer != null)
            {
                try
                {
                    Stream stream = writer.BaseStream;
                    writer.Close(); //can't close as it will close the basestream
                    writer = null;
                    if (reader == null)
                        stream.Close();
                }
                catch (System.ObjectDisposedException) { }
            }
        }


        // -----------------------------------------------------------------------------
        // ARGF Methods follow

        internal static object ARGF_FORWARD(Frame caller)
        {
            if (!(current_file is File))
                return argf_forward(caller);
            else
                return null;
        }

        internal static object NEXT_ARGF_FORWARD(Frame caller)
        {
            if (!next_argv(caller))
                return null;
            else
                return ARGF_FORWARD(caller);
        }

        internal static void argf_close(object obj, Frame caller)
        {
            if (obj is IO)
                rb_io_close((IO)obj);
            else
                Eval.CallPrivate(obj, caller, "close", null, new object[] { });
        }

        internal static object argf_forward(Frame caller)
        {
            //fixme: how to get last_func, argc, argv ?
            //return rb_funcall3(current_file, ruby_frame->last_func, ruby_frame->argc, ruby_frame->argv);
            return null;
        }

        internal static String argf_getline(object[] args, Frame caller)
        {
            String line;

            if (!next_argv(caller))
                return null;

            while (true)
            {
                if (args.Length == 0 && rb_rs.value.Equals(rb_default_rs.value))
                    line = rb_io_gets(current_file);
                else
                {
                    string rs;
                    if (args.Length == 0)
                        rs = String.StringValue(rb_rs.value, caller);
                    else
                        rs = String.StringValue(args[0], caller);

                    line = rb_io_getline(rs, current_file, caller);
                }

                if (line == null && next_p != -1)
                {
                    argf_close(current_file, caller);
                    next_p = 1;
                    continue;
                }

                if (line != null)
                {
                    gets_lineno++;
                    lineno_global.value = gets_lineno;
                }

                return line;
            }
        }

        internal static void ruby_add_suffix(Frame caller, String str, string suffix)
        {
            const string suffix1 = ".$$$";
            const string suffix2 = ".~~~";

            if (str.value.Length > 1000)
                throw new fatal(string.Format(CultureInfo.InvariantCulture, "Cannot do inplace edit on long filename ({0} characters)", str.value.Length)).raise(caller);

            /* Style 0 */
            int slen = str.value.Length;
            str.value = str.value + suffix;
            if (valid_filename(str.value))
                return;

            /* Fooey, style 0 failed.  Fix str before continuing. */
            str.value.Remove(slen);

            StringBuilder buf = new StringBuilder(1000);

            slen = suffix.Length;
            int baselen = 0;
            int s = 0;
            while (s < str.value.Length)
            {
                buf.Append(str.value[s]);
                if (str.value[s] == '.')
                    break;

                baselen++;
                if (str.value[s] == '\\' || str.value[s] == '/')
                    baselen = 0;
                s++;
            }

            StringBuilder ext = new StringBuilder(24);
            int extlen = 0;
            while (s < str.value.Length)
            {
                ext.Append(str.value[s++]);
                extlen++;
            }

            if (extlen == 0)
            {
                ext.Append('.');
                extlen++;
            }

            bool fallback = false;
            do
            {
                if (suffix[0] == '.') /* Style 1 */
                {
                    if (ext.ToString().Equals(suffix))
                    {
                        fallback = true;
                        break;
                    }
                    buf.Append(suffix);
                }
                else if (suffix.Length == 1) /* Style 2 */
                {
                    if (extlen < 4)
                    {
                        ext.Append(suffix);
                        extlen++;
                    }
                    else if (baselen < 8)
                    {
                        buf.Append(suffix);
                    }
                    else if (ext[3] != suffix[0])
                    {
                        ext[3] = suffix[0];
                    }
                    else if (buf[7] != suffix[0])
                    {
                        buf[7] = suffix[0];
                    }
                    else
                    {
                        fallback = true;
                        break;
                    }
                    buf.Append(ext.ToString());
                }
                else
                {
                    fallback = true;
                }
            } while (false);

            if (fallback) /* Style 3:  Panic */
                buf.Append(ext.ToString().Equals(suffix1) ? suffix2 : suffix1);

            str.value = buf.ToString();
        }

        internal static bool next_argv(Frame caller)
        {
            string fn;
            IO fptr;
            bool stdout_binmode = false;

            if (rb_stdout.value is File)
            {
                fptr = IO.GetOpenFile(caller, rb_stdout.value);
                if ((fptr.mode & IO.FMODE_BINMODE) > 0)
                    stdout_binmode = true;
            }

            if (init_p == 0)
            {
                if (((Array)(Options.rb_argv.value)).Count > 0)
                    next_p = 1;
                else
                    next_p = -1;
                init_p = 1;
                gets_lineno = 0;
            }

            if (next_p == 1)
            {
                next_p = 0;

                while (true)
                {
                    if (((Array)Options.rb_argv.value).Count > 0)
                    {
                        filename.value = Ruby.Methods.rb_ary_shift.singleton.Call0(null, Options.rb_argv.value, caller, null);

                        fn = filename.value.ToString();
                        if (fn.Length == 1 && fn.Equals("-"))
                        {
                            current_file = (IO)rb_stdin.value;
                            if (ruby_inplace_mode != null)
                            {
                                Errors.rb_warn("Can't do inplace edit for stdio; skipping");
                                continue;
                            }
                        }
                        else
                        {
                            Stream fr = System.IO.File.Open(fn, FileMode.Open, FileAccess.Read);
                            String str;

                            if (ruby_inplace_mode != null)
                            {
                                if (!rb_stdout.value.Equals(orig_stdout))
                                    rb_io_close((IO)rb_stdout.value);

                                if (ruby_inplace_mode.Length > 0)
                                {
                                    str = new String(fn);
                                    ruby_add_suffix(caller, str, ruby_inplace_mode);
                                    fr.Close();
                                    System.IO.File.Delete(str.value);
                                    System.IO.File.Move(fn, str.value);
                                    fr = System.IO.File.Open(str.value, FileMode.Open, FileAccess.Read);
                                }
                                else
                                {
                                    throw new fatal("Can't do inplace edit without backup").raise(caller);
                                }

                                Stream fw = System.IO.File.Open(fn, FileMode.OpenOrCreate, FileAccess.Write);

                                rb_stdout.value = prep_stdio(fw, FMODE_WRITABLE, Ruby.Runtime.Init.rb_cFile);
                                prep_path(rb_stdout.value, fn, caller);
                                if (stdout_binmode)
                                    rb_io_binmode(rb_stdout.value, caller);
                            }

                            current_file = prep_stdio(fr, FMODE_READABLE, Ruby.Runtime.Init.rb_cFile);
                            prep_path(current_file, fn, caller);
                        }
                        if (argf_binmode)
                            rb_io_binmode(current_file, caller);
                    }
                    else
                    {
                        next_p = 1;
                        return false;
                    }
                }
            }
            else if (next_p == -1)
            {
                current_file = (IO)rb_stdin.value;
                filename.value = new String("-");
                if (ruby_inplace_mode != null)
                {
                    Errors.rb_warn("Can't do inplace edit for stdio");
                    rb_stdout.value = orig_stdout;
                }
            }

            return true;
        }

        // ---------------------------------------------
        // IO methods follow

        internal static bool READ_DATA_PENDING(Stream fp)
        {
            if (fp.CanSeek)
                return fp.Position < fp.Length - 1; // Check for end of stream
            else
                return true;
        }

        internal static void READ_CHECK(Frame caller, Stream fp, IO fptr)
        {
            if (!READ_DATA_PENDING(fp))
            {
                //rb_thread_wait_fd(fileno(fp));
                rb_io_check_closed(caller, fptr);
            }
        }

        internal static int fileno(Stream f)
        {
            if (f is FileStream)
                return ((FileStream)f).SafeFileHandle.DangerousGetHandle().ToInt32();
            else
            {
                if (f.Equals(((IO)IO.rb_stdin.value).f))
                    return 0;
                if (f.Equals(((IO)IO.rb_stdout.value).f))
                    return 1;
                if (f.Equals(((IO)IO.rb_stderr.value).f))
                    return 2;

                return -1;
            }
        }

        internal static IO flush_before_seek(IO fptr)
        {
            if ((fptr.mode & FMODE_WBUF) > 0)
            {
                io_fflush(GetWriteFile(fptr), fptr);
            }
            return fptr;
        }

        internal static object io_close(object io, Frame caller)
        {
            return Eval.CallPrivate(io, caller, "close", null, new object[] { });
        }

        internal static void io_fflush(Stream f, IO fptr)
        {
            //if (!rb_thread_fd_writable(fileno(f)))
            //{
            //    rb_io_check_closed(fptr);
            //}

            for (; ; )
            {
                f.Flush();
                if (f.Position >= f.Length)
                    break;
                //if (!rb_io_wait_writable(fileno(f)))
                //    throw SystemCallError.rb_sys_fail(fptr.path, null, null).raise(caller);
            }
            fptr.mode &= ~FMODE_WBUF;
        }

        internal static void io_puts_ary(Array ary, IO io, Frame caller)
        {
            foreach (object element in ary)
                if (Array.IsInspecting(element))
                    Ruby.Methods.rb_io_puts.singleton.Calln(null, io, caller, new ArgList(null, new object[] { new String("[...]") }));
                else
                    Ruby.Methods.rb_io_puts.singleton.Calln(null, io, caller, new ArgList(null, new object[] { element }));
        }

        internal static object io_reopen(object _io, object _nfile, Frame caller)
        {
            IO io = (IO)_io;
            IO nfile = (IO)_nfile;

            IO fptr, orig;
            string mode;
            int fd, fd2;
            long pos = 0;

            if (Eval.rb_safe_level() >= 4 && (!io.Tainted || !nfile.Tainted))
                throw new SecurityError("Insecure: can't reopen").raise(caller);
            fptr = GetOpenFile(caller, io);
            orig = GetOpenFile(caller, nfile);

            if (fptr == orig)
                return io;

            if ((orig.mode & FMODE_READABLE) > 0)
            {
                pos = io_tell(orig);
            }
            if (orig.f2 != null)
            {
                io_fflush(orig.f2, orig);
            }
            else if ((orig.mode & FMODE_WRITABLE) > 0)
            {
                io_fflush(orig.f, orig);
            }
            if ((fptr.mode & FMODE_WRITABLE) > 0)
            {
                io_fflush(GetWriteFile(fptr), fptr);
            }

            /* copy OpenFile structure */
            fptr.mode = orig.mode;
            fptr._pid = orig._pid;
            fptr.oflineno = orig.oflineno;
            if (fptr._path != null)
                fptr._path = null;
            fptr._path = orig._path;
            fptr.finalize = orig.finalize;

            mode = rb_io_mode_string(fptr);
            fd = fileno(fptr.f);
            fd2 = fileno(orig.f);
            if (fd != fd2)
            {
                if (fptr.f.Equals(System.Console.OpenStandardInput()) || fptr.f.Equals(System.Console.OpenStandardOutput()) || fptr.f.Equals(System.Console.OpenStandardError()))
                {
                    //clearerr(fptr.f);
                    /* need to keep stdio objects */
                    //if (dup2(fd2, fd) < 0)
                    //    rb_sys_fail(orig.path);
                }
                else
                {
                    Stream f2 = fptr.f2;
                    int m = fptr.mode;
                    fptr.f.Close();
                    fptr.f = f2;
                    fptr.f2 = null;
                    fptr.mode &= ((m & FMODE_READABLE) > 0) ? ~FMODE_READABLE : ~FMODE_WRITABLE;
                    //if (dup2(fd2, fd) < 0)
                    //    rb_sys_fail(orig.path);
                    if (f2 != null)
                    {
                        fptr.f = rb_fdopen(caller, fd, "r");
                        fptr.f2 = f2;
                    }
                    else
                    {
                        fptr.f = rb_fdopen(caller, fd, mode);
                    }
                    fptr.mode = m;
                }
                //rb_thread_fd_close(fd);
                if (((orig.mode & FMODE_READABLE) > 0) && pos >= 0)
                {
                    io_seek(fptr, pos, SEEK_SET);
                    io_seek(orig, pos, SEEK_SET);
                }
            }

            if (fptr.f2 != null && (fd != fileno(fptr.f2)))
            {
                fd = fileno(fptr.f2);
                if (orig.f2 != null)
                {
                    fptr.f2.Close();
                    //rb_thread_fd_close(fd);
                    fptr.f2 = null;
                }
                else if (fd != (fd2 = fileno(orig.f2)))
                {
                    fptr.f2.Close();
                    //rb_thread_fd_close(fd);
                    //if (dup2(fd2, fd) < 0)
                    //    rb_sys_fail(orig.path);
                    fptr.f2 = rb_fdopen(caller, fd, "w");
                }
            }

            if ((fptr.mode & FMODE_BINMODE) > 0)
            {
                rb_io_binmode(io, caller);
            }

            io.my_class = nfile.my_class;

            return io;
        }

        internal static long io_seek(IO fptr, long pos, int whence)
        {
            fptr = flush_before_seek(fptr);

            switch (whence)
            {
                case SEEK_CUR:
                    return fptr.f.Seek(pos, SeekOrigin.Current);
                case SEEK_END:
                    return fptr.f.Seek(pos, SeekOrigin.End);
                case SEEK_SET:
                default:
                    return fptr.f.Seek(pos, SeekOrigin.Begin);
            }
        }

        internal static long io_tell(IO fptr)
        {
            return flush_before_seek(fptr).f.Position;
        }

        internal static void prep_path(object obj, string path, Frame caller)
        {
            IO io = (IO)obj;
            if (io._path != null)
                Exception.rb_bug("illegal prep_path() call", caller);

            io._path = path;
        }

        internal static IO prep_stdio(Stream f, int mode, Class klass)
        {
            IO io = new IO(klass);

            io.Init(f, mode);

            return io;
        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr _fdopen(int _FileHandle, string _Format);

        internal static Stream rb_fdopen(Frame caller, int fd, string mode)
        {
            switch (fd)
            {
                case 0: return ((IO)IO.rb_stdin.value).f;
                case 1: return ((IO)IO.rb_stdout.value).f;
                case 2: return ((IO)IO.rb_stderr.value).f;
                default:
                    return new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(_fdopen(fd, mode), true)
                        , modenumToFileAccess(rb_io_mode_modenum(caller, mode)));
            }
        }

        internal static IO rb_file_open(Frame caller, string fname, string mode)
        {
            IO io = new IO(Ruby.Runtime.Init.rb_cIO);

            int mode1 = IO.rb_io_mode_flags(caller, mode);
            int mode2 = rb_io_mode_modenum(caller, mode);

            FileMode fm = IO.modenumToFileMode(mode2);
            FileAccess fa = IO.modenumToFileAccess(mode2);

            io.Init(fname, fm, fa, mode1);

            return io;
        }

        internal static object rb_io_binmode(object io, Frame caller)
        {
            IO fptr = GetOpenFile(caller, io);

            //if (fptr.f && setmode(fileno(fptr.f), O_BINARY) == -1)
            //    throw SystemCallError.rb_sys_fail(fptr.path, IOError, caller);
            //if (fptr.f2 && setmode(fileno(fptr.f2), O_BINARY) == -1)
            //    throw SystemCallError.rb_sys_fail(fptr.path, IOError, caller);

            fptr.mode |= FMODE_BINMODE;

            return io;
        }

        internal static void rb_io_check_closed(Frame caller, IO fptr)
        {
            rb_io_check_initialized(caller, fptr);
            if (fptr.f == null && fptr.f2 == null)
                throw new IOError("closed stream").raise(caller);
        }

        internal static void rb_io_check_initialized(Frame caller, IO fptr)
        {
            if (fptr == null)
                throw new IOError("uninitialized stream").raise(caller);
        }

        internal static IO rb_io_check_io(object io, Frame caller)
        {
            return Object.CheckConvert<IO>(io, "to_io", caller); //rb_check_convert_type
        }

        internal static void rb_io_check_readable(Frame caller, IO fptr)
        {
            rb_io_check_closed(caller, fptr);

            if ((fptr.mode & FMODE_READABLE) == 0)
                throw new IOError("not opened for reading").raise(caller);

            fptr.mode |= FMODE_RBUF;
        }

        internal static void rb_io_check_writable(Frame caller, IO fptr)
        {
            rb_io_check_closed(caller, fptr);

            if ((fptr.mode & FMODE_WRITABLE) == 0)
                throw new IOError("not opened for writing").raise(caller);

            if ((fptr.mode & FMODE_RBUF) > 0 && fptr.f.Position < fptr.f.Length && fptr.f2 != null)
            {
                io_seek(fptr, 0, SEEK_CUR);
            }

            if (fptr.f2 != null)
            {
                fptr.mode &= ~FMODE_RBUF;
            }
        }

        internal static void rb_io_close(object obj)
        {
            if (obj == null)
                return;

            IO io = (IO)obj;

            io.CloseRead();

            io.CloseWrite();

            if (io.f != null)
            {
                io.f.Close();
                io.f.Dispose();
                io.f = null;
            }
            if (io.f2 != null)
            {
                io.f2.Close();
                io.f2.Dispose();
                io.f2 = null;
            }
            io._path = null;
        }

        internal static bool rb_io_closed(Frame caller, object o)
        {
            IO fptr = (IO)o;
            rb_io_check_initialized(caller, fptr);

            return fptr.f != null || fptr.f2 != null;
        }

        private const int IOCPARM_MASK = 0x7f;

        private static int IOCPARM_LEN(int x)
        {
            return ((x) >> 16) & IOCPARM_MASK;
        }

        private static int io_cntl(Frame caller, int fd, int cmd, long narg, bool io_p)
        {
            if (!io_p)
                throw NotImplementedError.rb_notimplement(caller, "rb_io_ioctl").raise(caller);

            //TRAP_BEG;
            //int retval = ioctl(fd, cmd, narg);
            //TRAP_END;
            //return retval;

            throw new System.NotImplementedException("ioctl");
        }

        internal static int rb_io_ctl(object io, object req, object arg, bool io_p, Frame caller)
        {
            int cmd = Numeric.rb_num2long(req, caller);
            IO fptr;
            int len = 0;
            int narg = 0;

            if (arg == null)
                narg = 0;
            else if (arg is int)
                narg = (int)arg;
            else if (arg is bool)
                if ((bool)arg)
                    narg = 1;
                else
                    narg = 0;
            else
            {
                String tmp = String.rb_check_string_type(arg, caller);
                if (tmp == null)
                    narg = Numeric.rb_num2long(arg, caller);
                else
                {
                    arg = tmp;
                    len = IOCPARM_LEN(cmd); /* on BSDish systems we're safe */

                    //rb_str_modify(arg);

                    System.Text.StringBuilder argstr = new System.Text.StringBuilder(((String)arg).value);

                    if (len <= argstr.Length)
                    {
                        len = argstr.Length;
                    }
                    if (argstr.Length < len)
                    {
                        argstr.Length = len + 1;
                    }
                    argstr[len] = (char)17;    /* a little sanity check here */
                    ((String)arg).value = argstr.ToString();

                    narg = 0;
                }
            }

            fptr = IO.GetOpenFile(caller, io);
            int retval = io_cntl(caller, IO.fileno(fptr.f), cmd, narg, io_p);
            if (retval < 0)
                throw SystemCallError.rb_sys_fail(fptr._path, caller).raise(caller);
            if (arg is String && ((String)arg).value[len] == 17)
            {
                throw new ArgumentError("return value overflowed string").raise(caller);
            }

            if (fptr.f2 != null && IO.fileno(fptr.f) != IO.fileno(fptr.f))
                /* call on f2 too; ignore result */
                io_cntl(caller, IO.fileno(fptr.f2), cmd, narg, io_p);

            return retval;
        }

        internal static IO rb_io_get_io(object io, Frame caller)
        {
            return Object.Convert<IO>(io, "to_io", caller); //rb_convert_type
        }

        internal static String rb_io_getline(object _rs, object io, Frame caller)
        {
            String line = null;

            IO file = (IO)io;
            rb_io_check_readable(caller, file);

            if (_rs == null)
            {
                line = new String(file.ReadToEnd(caller));
                if (string.IsNullOrEmpty(line.value))
                    return null;
            }
            else if (_rs.Equals(rb_default_rs))
            {
                return rb_io_getline_fast(caller, file, '\n');
            }
            else
            {
                string rs = String.StringValue(_rs, caller);
                int c;
                string rsptr;
                long rslen;
                bool rspara = false;

                rslen = rs.Length;
                if (rslen == 0)
                {
                    rsptr = "\n\n";
                    rslen = rsptr.Length;
                    rspara = true;
                    swallow(caller, file, '\n');
                }
                else if (rslen == 1)
                {
                    return rb_io_getline_fast(caller, file, rs.ToCharArray()[0]);
                }
                else
                {
                    rsptr = rs;
                }

                while (true)
                {
                    c = file.Read(caller);
                    if (c == -1)
                        break;
                    else if (line.value.Length < rslen ||
                             (rspara || rscheck(caller, rs, rsptr)) || // I think its for thread safety
                             !line.value.EndsWith(rsptr))
                        line.value += new string((char)c, 1);
                    else
                        break;
                }

                if (rspara)
                    if (c != -1)
                        swallow(caller, file, '\n');
            }

            if (line != null)
            {
                file.oflineno++;
                lineno_global.value = file.oflineno;
                line.Tainted = true;
            }

            return line;
        }

        internal static String rb_io_getline_fast(Frame caller, IO file, char rs)
        {
            StringBuilder builder = new StringBuilder();

            while (!file.EndOfStream(caller))
            {
                READ_CHECK(caller, file.f, file);
                //TRAP_BEG();
                int c = file.Read(caller);
                //TRAP_END();
                if (c == IO.EOF)
                    break;
                builder.Append((char)c);
                if ((char)c == rs)
                    break;
            }

            if (builder.Length > 0)
            {
                file.oflineno++;
                lineno_global.value = file.oflineno;
                String line = new String(builder.ToString());
                line.Tainted = true;
                return line;
            }
            else
                return null;
        }

        internal static String rb_io_gets(object io)
        {
            IO file = (IO)io;
            rb_io_check_readable(null, file);

            return rb_io_getline_fast(null, file, '\n');
        }

        internal static IO rb_io_open(string fname, string mode, Frame caller)
        {
            if (fname[0] == '|')
                return pipe_open(null, fname.Substring(1), mode, caller);
            else
                return rb_file_open(caller, fname, mode);
        }

        private static string MODE_BINMODE(int flags, string nobin, string bin)
        {
            if ((flags & FMODE_BINMODE) > 0)
                return bin;
            else
                return nobin;
        }

        internal static string rb_io_flags_mode(Frame caller, int flags)
        {
            if ((flags & FMODE_APPEND) > 0)
            {
                if ((flags & FMODE_READWRITE) == FMODE_READWRITE)
                {
                    return MODE_BINMODE(flags, "a+", "ab+");
                }
                return MODE_BINMODE(flags, "a", "ab");
            }
            switch (flags & FMODE_READWRITE)
            {
                case FMODE_READABLE:
                    return MODE_BINMODE(flags, "r", "rb");
                case FMODE_WRITABLE:
                    return MODE_BINMODE(flags, "w", "wb");
                case FMODE_READWRITE:
                    if ((flags & FMODE_CREATE) > 0)
                    {
                        return MODE_BINMODE(flags, "wb+", "w+");
                    }
                    return MODE_BINMODE(flags, "rb+", "r+");
            }
            throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "illegal access mode {0}", flags)).raise(caller);
        }

        internal static int rb_io_mode_flags(Frame caller, string mode)
        {
            int flags = 0;
            int i = 0;

            switch (mode[i++])
            {
                case 'r':
                    flags |= FMODE_READABLE;
                    break;
                case 'w':
                    flags |= FMODE_WRITABLE | FMODE_CREATE;
                    break;
                case 'a':
                    flags |= FMODE_WRITABLE | FMODE_APPEND | FMODE_CREATE;
                    break;
                default:
                    throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "illegal access mode {0}", flags)).raise(caller);
            }

            while (i < mode.Length)
            {
                switch (mode[i++])
                {
                    case 'b':
                        flags |= FMODE_BINMODE;
                        break;
                    case '+':
                        flags |= FMODE_READWRITE;
                        break;
                    default:
                        throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "illegal access mode {0}", flags)).raise(caller);
                }
            }

            return flags;
        }

        private static string MODE_BINARY(int flags, string nobin, string bin)
        {
            if ((flags & O_BINARY) > 0)
                return bin;
            else
                return nobin;
        }

        internal static int rb_io_modenum_flags(int mode)
        {
            int flags = 0;

            switch (mode & (O_RDONLY | O_WRONLY | O_RDWR))
            {
                case O_RDONLY:
                    flags = FMODE_READABLE;
                    break;
                case O_WRONLY:
                    flags = FMODE_WRITABLE;
                    break;
                case O_RDWR:
                    flags = FMODE_READWRITE;
                    break;
            }

            if ((mode & O_APPEND) > 0)
            {
                flags |= FMODE_APPEND;
            }
            if ((mode & O_CREAT) > 0)
            {
                flags |= FMODE_CREATE;
            }
            if ((mode & O_BINARY) > 0)
            {
                flags |= FMODE_BINMODE;
            }

            return flags;
        }

        internal static string rb_io_modenum_mode(Frame caller, int flags)
        {
            if ((flags & O_APPEND) > 0)
            {
                if ((flags & O_RDWR) == O_RDWR)
                {
                    return MODE_BINARY(flags, "a+", "ab+");
                }
                return MODE_BINARY(flags, "a", "ab");
            }
            switch (flags & (O_RDONLY | O_WRONLY | O_RDWR))
            {
                case O_RDONLY:
                    return MODE_BINARY(flags, "r", "rb");
                case O_WRONLY:
                    return MODE_BINARY(flags, "w", "wb");
                case O_RDWR:
                    return MODE_BINARY(flags, "r+", "rb+");
            }
            throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "illegal access modenum {0}", flags)).raise(caller);
        }

        internal static int rb_io_mode_modenum(Frame caller, string mode)
        {
            int flags = 0;
            char[] m = mode.ToCharArray();

            switch (m[0])
            {
                case 'r':
                    flags |= File.O_RDONLY;
                    break;
                case 'w':
                    flags |= File.O_WRONLY | File.O_CREAT;
                    break;
                case 'a':
                    flags |= File.O_WRONLY | File.O_APPEND | File.O_CREAT;
                    break;
                default:
                    throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "illegal access mode {0}", mode)).raise(caller);
            }

            for (int i = 1; i < m.Length; i++)
            {
                switch (m[i])
                {
                    case 'b':
                        flags |= File.O_BINARY;
                        break;
                    case '+':
                        flags |= File.O_RDWR;
                        break;
                    default:
                        throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "illegal access mode {0}", mode)).raise(caller);
                }
            }

            return flags;
        }

        internal static string rb_io_mode_string(IO fptr)
        {
            switch (fptr.mode & FMODE_READWRITE)
            {
                case FMODE_READABLE:
                default:
                    return "r";
                case FMODE_WRITABLE:
                    return "w";
                case FMODE_READWRITE:
                    return "r+";
            }
        }

        internal static FileMode modenumToFileMode(int flags)
        {
            if ((flags & File.O_APPEND) > 0)
                return FileMode.Append;
            else if ((flags & File.O_CREAT) > 0)
                return FileMode.OpenOrCreate;
            else
                return FileMode.Open;
        }

        internal static FileAccess modenumToFileAccess(int flags)
        {
            if ((flags & File.O_WRONLY) > 0)
                return FileAccess.Write;
            else if ((flags & File.O_RDWR) > 0)
                return FileAccess.ReadWrite;
            else
                return FileAccess.Read;
        }

        internal static int rb_io_seek(Frame caller, IO io, int offset, int whence)
        {
            IO fptr = GetOpenFile(caller, io);
            try
            {
                IO.io_seek(fptr, offset, whence);
            }
            catch (System.Exception e)
            {
                throw SystemCallError.rb_sys_fail(fptr._path, e, caller).raise(caller);
            }

            return 0;
        }

        internal static int rb_io_write(object io, object str, Frame caller)
        {
            return (int)Eval.CallPrivate(io, caller, "write", null, new object[] { str });
        }

        [UsedByRubyCompiler]
        public static object rb_gets(Frame caller)
        {
            if (((String)(IO.rb_rs.value)).value != IO.rb_default_rs.value)
                return Methods.rb_f_gets.singleton.Call1(null, 0, caller, null, 0);

        retry:
            if (!next_argv(caller)) return null;
            String line = rb_io_gets(current_file);
            if (line == null && next_p != -1)
            {
                argf_close(current_file, caller);
                next_p = 1;
                goto retry;
            }
            Eval.rb_lastline_set(caller, line);
            if (line != null)
            {
                gets_lineno++;
                lineno_global.value = gets_lineno;
            }

            return line;
        }

        internal static int rb_sysopen(string filename, int flags, int mode)
        {
            // Win32 ignores mode

            FileStream f = System.IO.File.Open(filename, modenumToFileMode(flags), modenumToFileAccess(flags));

            return f.SafeFileHandle.DangerousGetHandle().ToInt32(); ;
        }

        internal static bool rscheck(Frame caller, string rs, string other)
        {
            if (!rs.Equals(other))
                throw new RuntimeError("rs modified").raise(caller);

            return true;
        }

        internal static bool swallow(Frame caller, IO io, int term)
        {
            int c;

            do
            {
                c = io.Peek(caller);
                if (c != term)
                    return true;
                //TRAPBEG;
                io.Read(caller);
                //TRAPEND;
            } while (c != -1);

            return false;
        }

        internal static object rb_io_taint_check(Frame caller, object io)
        {
            if (!((Basic)io).Tainted && Eval.rb_safe_level() >= 4)
                throw new SecurityError("Insecure: operation on untainted IO").raise(caller);
            TypeError.rb_check_frozen(caller, io);
            return io;
        }

        internal static void rb_io_fptr_cleanup(IO fptr, bool noraise)
        {
            if (fptr.finalize != null)
                fptr.finalize.Invoke(fptr, noraise);
            else
                fptr_finalize(fptr, noraise);
        }

        internal static void rb_io_fptr_finalize(IO fptr)
        {
            if (fptr == null)
                return;

            if (fptr._path != null)
            {
                fptr._path = null;
            }

            if (fptr.f == null && fptr.f2 == null)
                return;

            if (fptr.f.Equals(System.Console.OpenStandardInput()) || fptr.f.Equals(System.Console.OpenStandardOutput()) || fptr.f.Equals(System.Console.OpenStandardError()))
                return;

            rb_io_fptr_cleanup(fptr, true);
        }

        internal static object rb_open_file(IO io, Frame caller, Array rest)
        {
            Class.rb_scan_args(caller, rest, 1, 2, false);

            string fname = String.StringValue(rest[0], caller);
            object vmode = null, perm = null;
            if (rest.Count > 1)
                vmode = rest[1];
            if (rest.Count > 2)
                perm = rest[2];

            string mode = vmode == null ? "r" : String.StringValue(vmode, caller);

            int flags;
            if (vmode is int)
            {
                flags = (int)vmode;
                io.Init(fname, IO.modenumToFileMode(flags), IO.modenumToFileAccess(flags), IO.rb_io_modenum_flags(flags));
            }
            else
            {
                flags = IO.rb_io_mode_modenum(caller, mode);
                io.Init(fname, IO.modenumToFileMode(flags), IO.modenumToFileAccess(flags), IO.rb_io_mode_flags(caller, mode));
            }

            return io;
        }

        internal static bool valid_filename(string filename)
        {
            try
            {
                System.IO.Path.GetFileName(filename);
            }
            catch (System.ArgumentException)
            {
                return true;
            }

            return true;
        }

        internal static Stream GetWriteFile(IO fptr)
        {
            return ((fptr.f2 != null) ? fptr.f2 : fptr.f);
        }

        internal static IO GetOpenFile(Frame caller, object obj)
        {
            rb_io_taint_check(caller, obj);

            IO io = (IO)obj;

            rb_io_check_closed(caller, io);

            return io;
        }

        internal static IO MakeOpenFile(object obj)
        {
            IO io = (IO)obj;

            if (io != null)
            {
                rb_io_close(io);
            }

            io.InitOpenFile(0, 0, 0, null, null, null, null);

            return io;
        }

        class UngetcStreamReader : StreamReader
        {
            bool hasChar;
            int ungetChar;

            internal UngetcStreamReader(Stream s)
                : base(s)
            {
            }

            internal int ungetc(int c)
            {
                if (this.hasChar)
                    return IO.EOF;

                this.hasChar = true;
                this.ungetChar = c;

                return c;
            }

            public override int Read()
            {
                if (this.hasChar)
                {
                    this.hasChar = false;
                    return ungetChar;
                }
                return base.Read();
            }

            public override int Read(char[] buffer, int index, int count)
            {
                if (this.hasChar)
                {
                    this.hasChar = false;
                    buffer[0] = Convert.ToChar(this.ungetChar);
                    return base.Read(buffer, index + 1, count - 1) + 1;
                }
                else
                    return base.Read(buffer, index, count);
            }

            public override int ReadBlock(char[] buffer, int index, int count)
            {
                if (this.hasChar)
                {
                    this.hasChar = false;
                    buffer[0] = Convert.ToChar(this.ungetChar);
                    return base.ReadBlock(buffer, index + 1, count - 1) + 1;
                }
                else
                    return base.ReadBlock(buffer, index, count);
            }

            public override string ReadLine()
            {
                if (this.hasChar)
                {
                    this.hasChar = false;
                    string s = new string(Convert.ToChar(this.ungetChar), 1);
                    s += base.ReadLine();
                    return s;
                }
                else
                    return base.ReadLine();
            }

            public override string ReadToEnd()
            {
                if (this.hasChar)
                {
                    this.hasChar = false;
                    string s = new string(Convert.ToChar(this.ungetChar), 1);
                    s += base.ReadToEnd();
                    return s;
                }
                else
                    return base.ReadToEnd();
            }

            public override int Peek()
            {
                if (this.hasChar)
                {
                    return ungetChar;
                }
                return base.Peek();
            }
        }

        internal int Ungetc(int c, Frame caller)
        {
            if (this.EndOfStream(caller))
                return EOF;

            return this.Reader(caller).ungetc(c);
        }

        private static void fptr_finalize(IO fptr, bool noraise)
        {
            throw new System.NotImplementedException("fptr_finalize");
            //int n1 = 0, n2 = 0, f1, f2 = -1;

            //if (fptr->f2)
            //{
            //    f2 = fileno(fptr->f2);
            //    while (n2 = 0 && fflush(fptr->f2) < 0)
            //    {
            //        n2 = errno;
            //        if (!rb_io_wait_writable(f2))
            //        {
            //            break;
            //        }
            //        if (!fptr->f2) break;
            //    }
            //    if (fclose(fptr->f2) < 0 && n2 == 0)
            //    {
            //        n2 = errno;
            //    }
            //    fptr->f2 = 0;
            //}
            //if (fptr->f)
            //{
            //    f1 = fileno(fptr->f);
            //    if ((f2 == -1) && (fptr->mode & FMODE_WBUF))
            //    {
            //        while (n1 = 0 && fflush(fptr->f) < 0)
            //        {
            //            n1 = errno;
            //            if (!rb_io_wait_writable(f1)) break;
            //            if (!fptr->f) break;
            //        }
            //    }
            //    if (fclose(fptr->f) < 0 && n1 == 0)
            //    {
            //        n1 = errno;
            //    }
            //    fptr->f = 0;
            //    if (n1 == EBADF && f1 == f2)
            //    {
            //        n1 = 0;
            //    }
            //}
            //if (!noraise && (n1 || n2))
            //{
            //    errno = (n1 ? n1 : n2);
            //    rb_sys_fail(fptr->path);
            //}
        }

        // ---------------------------------------------------------------
        // Pipe Methods

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int _pipe(int[] _PtHandles, uint _PipeSize, int _TextMode);

        private static void pipe_add_fptr(IO fptr)
        {
            pipe_list list = new pipe_list(fptr, _pipe_list);
            _pipe_list = list;
        }

        internal static void pipe_atexit()
        {
            pipe_list list = _pipe_list;
            pipe_list tmp;

            while (list != null)
            {
                tmp = list.next;
                rb_io_fptr_finalize(list.fptr);
                list = tmp;
            }
        }

        private static void pipe_del_fptr(IO fptr)
        {
            pipe_list list = _pipe_list;
            pipe_list tmp;

            if (list.fptr == fptr)
            {
                _pipe_list = list.next;
                list = null;
                return;
            }

            while (list.next != null)
            {
                if (list.next.fptr == fptr)
                {
                    tmp = list.next;
                    list.next = list.next.next;
                    tmp = null;
                    return;
                }
                list = list.next;
            }
        }

        private static int pipe_exec(Frame caller, string name, int mode, out System.IO.FileInfo f1, out System.IO.FileInfo f2)
        {
            f1 = null;
            f2 = null;

            throw new NotImplementedError("pipe_exec").raise(caller);
        }

        private static void pipe_finalize(IO fptr, bool noraise)
        {
            fptr_finalize(fptr, noraise);
            pipe_del_fptr(fptr);
        }

        internal static IO pipe_open(object pstr, string pname, string mode, Frame caller)
        {
            int modef = rb_io_mode_flags(caller, mode);

            int pid;
            System.IO.FileInfo fpr, fpw;

            if (pname == null)
                pname = String.StringValue(pstr, caller);

            while (true)
            {
                try
                {
                    pid = pipe_exec(caller, pname, rb_io_mode_modenum(caller, mode), out fpr, out fpw);

                    IO port = (IO)Ruby.Methods.io_alloc.singleton.Call0(null, Ruby.Runtime.Init.rb_cIO, caller, null);

                    IO fptr = MakeOpenFile(port);
                    fptr.mode = modef;
                    fptr.mode |= FMODE_SYNC;
                    fptr._pid = pid;

                    if ((modef & FMODE_READABLE) > 0)
                    {
                        fptr.f = fpr.OpenRead();
                    }
                    if ((modef & FMODE_WRITABLE) > 0)
                    {
                        if (fptr.f != null)
                            fptr.f2 = fpw.OpenWrite();
                        else
                            fptr.f = fpw.OpenWrite();
                    }
                    fptr.finalize = pipe_finalize;

                    pipe_add_fptr(fptr);

                    return port;
                }
                catch (System.IO.IOException ioe)
                {
                    if (System.Runtime.InteropServices.Marshal.GetHRForException(ioe) == Errno.EAGAIN)
                    {
                        //rb_thread_sleep(1);
                    }
                    throw SystemCallError.rb_sys_fail(pname, ioe, caller).raise(caller);
                }
            }
        }

        // ----------------------------------------------------------------------------


        internal static void lineno_setter(object value, string id, object data, global_variable var)
        {
            gets_lineno = Integer.rb_num2long(value, null);
            var.value = gets_lineno;
        }

        internal static void stdout_setter(Frame caller, object value, string id, object data, global_variable var)
        {
            must_respond_to(caller, "write", value, id);
            var.value = value;
        }

        internal static void defout_setter(Frame caller, object value, string id, object data, global_variable var)
        {
            stdout_setter(caller, value, id, data, var);
            Errors.rb_warn("$defout is obsolete; use $stdout instead");
        }

        internal static void deferr_setter(Frame caller, object value, string id, object data, global_variable var)
        {
            stdout_setter(caller, value, id, data, var);
            Errors.rb_warn("$deferr is obsolete; use $stderr instead");
        }

        internal static object opt_i_get(string id, object data, global_variable var)
        {
            if (ruby_inplace_mode == null)
                return null;
            else
                return new String((string)ruby_inplace_mode);
        }

        internal static void opt_i_set(object value, string id, object data, global_variable var)
        {
            if (!Eval.Test(value))
            {
                ruby_inplace_mode = null;
            }
            else
            {
                ruby_inplace_mode = String.StringValue(value, null); // fixme: might need a valid frame to call StringValue
            }
        }

        internal static void must_respond_to(Frame caller, string mid, object val, string id)
        {
            if (!Eval.RespondTo(val, mid))
            {
                throw new TypeError(string.Format(CultureInfo.InvariantCulture, "{0} must have {1} method, {2} given", id, mid, ((Class)val).my_class._name)).raise(caller);
            }
        }

        internal static bool rb_io_wait_readable(Stream f, IOException ioe)
        {
            return true;
        }

        private class pipe_list
        {
            internal IO fptr;
            internal pipe_list next;

            internal pipe_list(IO fptr, pipe_list next)
            {
                this.fptr = fptr;
                this.next = next;
            }
        }
    }


    // -------------------------------------------------------
    // EXCEPTIONS


    public class IOError : StandardError
    {
        public IOError(string message) : this(message, Ruby.Runtime.Init.rb_eIOError) { }

        public IOError(string message, Class klass) : base(message, klass) { }

        public IOError(Class klass) : base(klass) { }
    }



    public class EOFError : IOError
    {
        public EOFError(string message) : this(message, Ruby.Runtime.Init.rb_eEOFError) { }

        public EOFError(string message, Class klass) : base(message, klass) { }

        public EOFError(Class klass) : base(klass) { }

        public static EOFError rb_eof_error()
        {
            return new EOFError("End of file reached");
        }
    }
}

    // -------------------------------------------------------
    // GLOBALS

namespace Ruby.Runtime
{
    internal class lineno_global : global_variable
    {
        internal override void setter(string id, object val, Frame caller)
        {
            IO.lineno_setter(val, id, null, this);
        }
    }


    internal class stdout_global : global_variable
    {
        internal override void setter(string id, object val, Frame caller)
        {
            IO.stdout_setter(caller, val, id, null, this);
        }
    }


    internal class defout_global : global_variable
    {
        internal override object getter(string id, Frame caller)
        {
            return IO.rb_stdout.getter(id, caller);
        }

        internal override void setter(string id, object val, Frame caller)
        {
            IO.defout_setter(caller, value, id, null, this);
        }
    }


    internal class deferr_global : global_variable
    {
        internal override object getter(string id, Frame caller)
        {
            return IO.rb_stderr.value;
        }

        internal override void setter(string id, object val, Frame caller)
        {
            IO.deferr_setter(caller, value, id, null, this);
        }
    }


    internal class op_i_global : global_variable
    {
        internal override object getter(string id, Frame caller)
        {
            return IO.opt_i_get(id, null, this);
        }

        internal override void setter(string id, object val, Frame caller)
        {
            IO.opt_i_set(val, id, null, this);
        }
    }
}
