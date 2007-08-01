using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class IO
    {
         [InteropMethod("for_fd")]
        public static object for_fd(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cIO, "for_fd", args);
        }

         [InteropMethod("foreach")]
        public static object ForEach(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cIO, "foreach", args);
        }

         [InteropMethod("new")]
        public static object new_instance(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cIO, "new", args);
        }

         [InteropMethod("open")]
        public static object open(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cIO, "open", args);
        }

         [InteropMethod("pipe")]
        public static object pipe()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_cIO, "pipe");
        }

         [InteropMethod("popen")]
        public static object popen(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cIO, "popen", args);
        }

         [InteropMethod("sysopen")]
        public static object sysopen(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cIO, "sysopen", args);
        }

         [InteropMethod("<<")]
        public object lshift(object p1)
        {
            return Eval.Call1(this, "<<", p1);
        }

         [InteropMethod("all?")]
        public object is_all()
        {
            return Eval.Call0(this, "all?");
        }

         [InteropMethod("any?")]
        public object is_any()
        {
            return Eval.Call0(this, "any?");
        }

         [InteropMethod("binmode")]
        public object binmode()
        {
            return Eval.Call0(this, "binmode");
        }

         [InteropMethod("close")]
        public object close()
        {
            return Eval.Call0(this, "close");
        }

         [InteropMethod("close_read")]
        public object close_read()
        {
            return Eval.Call0(this, "close_read");
        }

         [InteropMethod("close_write")]
        public object close_write()
        {
            return Eval.Call0(this, "close_write");
        }

         [InteropMethod("closed?")]
        public object is_closed()
        {
            return Eval.Call0(this, "closed?");
        }

         [InteropMethod("collect")]
        public object collect()
        {
            return Eval.Call0(this, "collect");
        }

         [InteropMethod("detect")]
        public object detect(params object[] args)
        {
            return Eval.Call(this, "detect", args);
        }

         [InteropMethod("each")]
        public object each(params object[] args)
        {
            return Eval.Call(this, "each", args);
        }

         [InteropMethod("each_byte")]
        public object each_byte()
        {
            return Eval.Call0(this, "each_byte");
        }

         [InteropMethod("each_line")]
        public object each_line(params object[] args)
        {
            return Eval.Call(this, "each_line", args);
        }

         [InteropMethod("each_with_index")]
        public object each_with_index()
        {
            return Eval.Call0(this, "each_with_index");
        }

         [InteropMethod("entries")]
        public object entries()
        {
            return Eval.Call0(this, "entries");
        }

         [InteropMethod("eof")]
        public object eof()
        {
            return Eval.Call0(this, "eof");
        }

         [InteropMethod("eof?")]
        public object is_eof()
        {
            return Eval.Call0(this, "eof?");
        }

         [InteropMethod("fcntl")]
        public object fcntl(params object[] args)
        {
            return Eval.Call(this, "fcntl", args);
        }

         [InteropMethod("fileno")]
        public object fileno()
        {
            return Eval.Call0(this, "fileno");
        }

         [InteropMethod("find")]
        public object find(params object[] args)
        {
            return Eval.Call(this, "find", args);
        }

         [InteropMethod("find_all")]
        public object find_all()
        {
            return Eval.Call0(this, "find_all");
        }

         [InteropMethod("flush")]
        public object flush()
        {
            return Eval.Call0(this, "flush");
        }

         [InteropMethod("fsync")]
        public object fsync()
        {
            return Eval.Call0(this, "fsync");
        }

         [InteropMethod("getc")]
        public object getc()
        {
            return Eval.Call0(this, "getc");
        }

         [InteropMethod("gets")]
        public object gets(params object[] args)
        {
            return Eval.Call(this, "gets", args);
        }

         [InteropMethod("grep")]
        public object grep(object p1)
        {
            return Eval.Call1(this, "grep", p1);
        }

         [InteropMethod("include?")]
        public object is_include(object p1)
        {
            return Eval.Call1(this, "include?", p1);
        }

         [InteropMethod("inject")]
        public object inject(params object[] args)
        {
            return Eval.Call(this, "inject", args);
        }

         [InteropMethod("ioctl")]
        public object ioctl(params object[] args)
        {
            return Eval.Call(this, "ioctl", args);
        }

         [InteropMethod("isatty")]
        public object isatty()
        {
            return Eval.Call0(this, "isatty");
        }

         [InteropMethod("lineno")]
        public object lineno()
        {
            return Eval.Call0(this, "lineno");
        }

         [InteropMethod("lineno=")]
        public object set_lineno(object p1)
        {
            return Eval.Call1(this, "lineno=", p1);
        }

         [InteropMethod("map")]
        public object map()
        {
            return Eval.Call0(this, "map");
        }

         [InteropMethod("max")]
        public object max()
        {
            return Eval.Call0(this, "max");
        }

         [InteropMethod("member?")]
        public object is_member(object p1)
        {
            return Eval.Call1(this, "member?", p1);
        }

         [InteropMethod("min")]
        public object min()
        {
            return Eval.Call0(this, "min");
        }

         [InteropMethod("partition")]
        public object partition()
        {
            return Eval.Call0(this, "partition");
        }

         [InteropMethod("pid")]
        public object pid()
        {
            return Eval.Call0(this, "pid");
        }

         [InteropMethod("pos")]
        public object pos()
        {
            return Eval.Call0(this, "pos");
        }

         [InteropMethod("pos=")]
        public object set_pos(object p1)
        {
            return Eval.Call1(this, "pos=", p1);
        }

         [InteropMethod("print")]
        public object print(params object[] args)
        {
            return Eval.Call(this, "print", args);
        }

         [InteropMethod("printf")]
        public object printf(params object[] args)
        {
            return Eval.Call(this, "printf", args);
        }

         [InteropMethod("putc")]
        public object putc(object p1)
        {
            return Eval.Call1(this, "putc", p1);
        }

         [InteropMethod("puts")]
        public object puts(params object[] args)
        {
            return Eval.Call(this, "puts", args);
        }

         [InteropMethod("read")]
        public object read(params object[] args)
        {
            return Eval.Call(this, "read", args);
        }

         [InteropMethod("read_nonblock")]
        public object read_nonblock(params object[] args)
        {
            return Eval.Call(this, "read_nonblock", args);
        }

         [InteropMethod("readchar")]
        public object readchar()
        {
            return Eval.Call0(this, "readchar");
        }

         [InteropMethod("readline")]
        public object readline(params object[] args)
        {
            return Eval.Call(this, "readline", args);
        }

         [InteropMethod("readlines")]
        public object readlines(params object[] args)
        {
            return Eval.Call(this, "readlines", args);
        }

         [InteropMethod("readpartial")]
        public object readpartial(params object[] args)
        {
            return Eval.Call(this, "readpartial", args);
        }

         [InteropMethod("reject")]
        public object reject()
        {
            return Eval.Call0(this, "reject");
        }

         [InteropMethod("reopen")]
        public object reopen(params object[] args)
        {
            return Eval.Call(this, "reopen", args);
        }

         [InteropMethod("rewind")]
        public object rewind()
        {
            return Eval.Call0(this, "rewind");
        }

         [InteropMethod("seek")]
        public object seek(params object[] args)
        {
            return Eval.Call(this, "seek", args);
        }

         [InteropMethod("select")]
        public object select()
        {
            return Eval.Call0(this, "select");
        }

         [InteropMethod("sort")]
        public object sort()
        {
            return Eval.Call0(this, "sort");
        }

         [InteropMethod("sort_by")]
        public object sort_by()
        {
            return Eval.Call0(this, "sort_by");
        }

         [InteropMethod("stat")]
        public object stat()
        {
            return Eval.Call0(this, "stat");
        }

         [InteropMethod("sync")]
        public object sync()
        {
            return Eval.Call0(this, "sync");
        }

         [InteropMethod("sync=")]
        public object set_sync(object p1)
        {
            return Eval.Call1(this, "sync=", p1);
        }

         [InteropMethod("sysread")]
        public object sysread(params object[] args)
        {
            return Eval.Call(this, "sysread", args);
        }

         [InteropMethod("sysseek")]
        public object sysseek(params object[] args)
        {
            return Eval.Call(this, "sysseek", args);
        }

         [InteropMethod("syswrite")]
        public object syswrite(object p1)
        {
            return Eval.Call1(this, "syswrite", p1);
        }

         [InteropMethod("tell")]
        public object tell()
        {
            return Eval.Call0(this, "tell");
        }

         [InteropMethod("to_i")]
        public object to_i()
        {
            return Eval.Call0(this, "to_i");
        }

         [InteropMethod("to_io")]
        public object to_io()
        {
            return Eval.Call0(this, "to_io");
        }

         [InteropMethod("tty?")]
        public object is_tty()
        {
            return Eval.Call0(this, "tty?");
        }

         [InteropMethod("ungetc")]
        public object ungetc(object p1)
        {
            return Eval.Call1(this, "ungetc", p1);
        }

         [InteropMethod("write")]
        public object write(object p1)
        {
            return Eval.Call1(this, "write", p1);
        }

         [InteropMethod("write_nonblock")]
        public object write_nonblock(object p1)
        {
            return Eval.Call1(this, "write_nonblock", p1);
        }

         [InteropMethod("zip")]
        public object zip(params object[] args)
        {
            return Eval.Call(this, "zip", args);
        }

    }
}

