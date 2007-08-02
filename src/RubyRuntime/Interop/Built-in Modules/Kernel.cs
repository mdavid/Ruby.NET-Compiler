using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Kernel
    {
         [InteropMethod("Array")]
        public static object Array(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mKernel, "Array", p1);
        }

         [InteropMethod("Float")]
        public static object Float(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mKernel, "Float", p1);
        }

         [InteropMethod("Integer")]
        public static object Integer(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mKernel, "Integer", p1);
        }

         [InteropMethod("String")]
        public static object String(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mKernel, "String", p1);
        }

         [InteropMethod("`")]
        public static object quote(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mKernel, "`", p1);
        }

         [InteropMethod("abort")]
        public static object abort(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "abort", args);
        }

         [InteropMethod("at_exit")]
        public static object at_exit()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "at_exit");
        }

         [InteropMethod("autoload")]
        public static object autoload(object p1, object p2)
        {
            return Eval.Call2(Ruby.Runtime.Init.rb_mKernel, "autoload", p1, p2);
        }

         [InteropMethod("autoload?")]
        public static object is_autoload(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mKernel, "autoload?", p1);
        }

         [InteropMethod("binding")]
        public static object binding()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "binding");
        }

         [InteropMethod("block_given?")]
        public static object is_block_given()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "block_given?");
        }

         [InteropMethod("callcc")]
        public static object callcc()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "callcc");
        }

         [InteropMethod("caller")]
        public static object caller(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "caller", args);
        }

         [InteropMethod("catch")]
        public static object Catch(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mKernel, "catch", p1);
        }

         [InteropMethod("chomp")]
        public static object chomp(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "chomp", args);
        }

         [InteropMethod("chomp!")]
        public static object do_chomp(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "chomp!", args);
        }

         [InteropMethod("chop")]
        public static object chop()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "chop");
        }

         [InteropMethod("chop!")]
        public static object do_chop()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "chop!");
        }

         [InteropMethod("eval")]
        public static object eval(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "eval", args);
        }

         [InteropMethod("exec")]
        public static object exec(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "exec", args);
        }

         [InteropMethod("exit")]
        public static object exit(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "exit", args);
        }

         [InteropMethod("exit!")]
        public static object do_exit(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "exit!", args);
        }

         [InteropMethod("fail")]
        public static object fail(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "fail", args);
        }

         [InteropMethod("fork")]
        public static object fork()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "fork");
        }

         [InteropMethod("format")]
        public static object format(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "format", args);
        }

         [InteropMethod("getc")]
        public static object getc()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "getc");
        }

         [InteropMethod("gets")]
        public static object gets(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "gets", args);
        }

         [InteropMethod("global_variables")]
        public static object global_variables()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "global_variables");
        }

         [InteropMethod("gsub")]
        public static object gsub(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "gsub", args);
        }

         [InteropMethod("gsub!")]
        public static object do_gsub(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "gsub!", args);
        }

         [InteropMethod("iterator?")]
        public static object is_iterator()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "iterator?");
        }

         [InteropMethod("lambda")]
        public static object lambda()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "lambda");
        }

         [InteropMethod("load")]
        public static object load(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "load", args);
        }

         [InteropMethod("local_variables")]
        public static object local_variables()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "local_variables");
        }

         [InteropMethod("loop")]
        public static object loop()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "loop");
        }

         [InteropMethod("method_missing")]
        public static object method_missing(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "method_missing", args);
        }

         [InteropMethod("open")]
        public static object open(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "open", args);
        }

         [InteropMethod("p")]
        public static object p(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "p", args);
        }

         [InteropMethod("print")]
        public static object print(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "print", args);
        }

         [InteropMethod("printf")]
        public static object printf(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "printf", args);
        }

         [InteropMethod("proc")]
        public static object proc()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mKernel, "proc");
        }

         [InteropMethod("putc")]
        public static object putc(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mKernel, "putc", p1);
        }

         [InteropMethod("puts")]
        public static object puts(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "puts", args);
        }

         [InteropMethod("raise")]
        public static object raise(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "raise", args);
        }

         [InteropMethod("rand")]
        public static object rand(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "rand", args);
        }

         [InteropMethod("readline")]
        public static object readline(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "readline", args);
        }

         [InteropMethod("readlines")]
        public static object readlines(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "readlines", args);
        }

         [InteropMethod("require")]
        public static object require(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mKernel, "require", p1);
        }

         [InteropMethod("scan")]
        public static object scan(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mKernel, "scan", p1);
        }

         [InteropMethod("select")]
        public static object select(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "select", args);
        }

         [InteropMethod("set_trace_func")]
        public static object set_trace_func(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mKernel, "set_trace_func", p1);
        }

         [InteropMethod("sleep")]
        public static object sleep(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "sleep", args);
        }

         [InteropMethod("split")]
        public static object split(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "split", args);
        }

         [InteropMethod("sprintf")]
        public static object sprintf(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "sprintf", args);
        }

         [InteropMethod("srand")]
        public static object srand(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "srand", args);
        }

         [InteropMethod("sub")]
        public static object sub(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "sub", args);
        }

         [InteropMethod("sub!")]
        public static object do_sub(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "sub!", args);
        }

         [InteropMethod("syscall")]
        public static object syscall(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "syscall", args);
        }

         [InteropMethod("system")]
        public static object system(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "system", args);
        }

         [InteropMethod("test")]
        public static object test(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "test", args);
        }

         [InteropMethod("throw")]
        public static object Throw(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "throw", args);
        }

         [InteropMethod("trace_var")]
        public static object trace_var(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "trace_var", args);
        }

         [InteropMethod("trap")]
        public static object trap(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "trap", args);
        }

         [InteropMethod("untrace_var")]
        public static object untrace_var(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mKernel, "untrace_var", args);
        }

         [InteropMethod("warn")]
        public static object warn(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mKernel, "warn", p1);
        }

    }
}

