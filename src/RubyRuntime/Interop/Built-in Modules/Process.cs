using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Process
    {
         [InteropMethod("abort")]
        public static object abort(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mProcess, "abort", args);
        }

         [InteropMethod("detach")]
        public static object detach(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mProcess, "detach", p1);
        }

         [InteropMethod("egid")]
        public static object egid()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "egid");
        }

         [InteropMethod("egid=")]
        public static object set_egid(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mProcess, "egid=", p1);
        }

         [InteropMethod("euid")]
        public static object euid()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "euid");
        }

         [InteropMethod("euid=")]
        public static object set_euid(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mProcess, "euid=", p1);
        }

         [InteropMethod("exit")]
        public static object exit(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mProcess, "exit", args);
        }

         [InteropMethod("exit!")]
        public static object do_exit(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mProcess, "exit!", args);
        }

         [InteropMethod("fork")]
        public static object fork()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "fork");
        }

         [InteropMethod("getpgid")]
        public static object getpgid(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mProcess, "getpgid", p1);
        }

         [InteropMethod("getpgrp")]
        public static object getpgrp()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "getpgrp");
        }

         [InteropMethod("getpriority")]
        public static object getpriority(object p1, object p2)
        {
            return Eval.Call2(Ruby.Runtime.Init.rb_mProcess, "getpriority", p1, p2);
        }

         [InteropMethod("getrlimit")]
        public static object getrlimit(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mProcess, "getrlimit", p1);
        }

         [InteropMethod("gid")]
        public static object gid()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "gid");
        }

         [InteropMethod("gid=")]
        public static object set_gid(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mProcess, "gid=", p1);
        }

         [InteropMethod("groups")]
        public static object groups()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "groups");
        }

         [InteropMethod("groups=")]
        public static object set_groups(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mProcess, "groups=", p1);
        }

         [InteropMethod("initgroups")]
        public static object initgroups(object p1, object p2)
        {
            return Eval.Call2(Ruby.Runtime.Init.rb_mProcess, "initgroups", p1, p2);
        }

         [InteropMethod("kill")]
        public static object kill(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mProcess, "kill", args);
        }

         [InteropMethod("maxgroups")]
        public static object maxgroups()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "maxgroups");
        }

         [InteropMethod("maxgroups=")]
        public static object set_maxgroups(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mProcess, "maxgroups=", p1);
        }

         [InteropMethod("pid")]
        public static object pid()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "pid");
        }

         [InteropMethod("ppid")]
        public static object ppid()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "ppid");
        }

         [InteropMethod("setpgid")]
        public static object setpgid(object p1, object p2)
        {
            return Eval.Call2(Ruby.Runtime.Init.rb_mProcess, "setpgid", p1, p2);
        }

         [InteropMethod("setpgrp")]
        public static object setpgrp()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "setpgrp");
        }

         [InteropMethod("setpriority")]
        public static object setpriority(object p1, object p2, object p3)
        {
            return Eval.Call3(Ruby.Runtime.Init.rb_mProcess, "setpriority", p1, p2, p3);
        }

         [InteropMethod("setrlimit")]
        public static object setrlimit(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mProcess, "setrlimit", args);
        }

         [InteropMethod("setsid")]
        public static object setsid()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "setsid");
        }

         [InteropMethod("times")]
        public static object times()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "times");
        }

         [InteropMethod("uid")]
        public static object uid()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "uid");
        }

         [InteropMethod("uid=")]
        public static object set_uid(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mProcess, "uid=", p1);
        }

         [InteropMethod("wait")]
        public static object wait(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mProcess, "wait", args);
        }

         [InteropMethod("wait2")]
        public static object wait2(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mProcess, "wait2", args);
        }

         [InteropMethod("waitall")]
        public static object waitall()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mProcess, "waitall");
        }

         [InteropMethod("waitpid")]
        public static object waitpid(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mProcess, "waitpid", args);
        }

         [InteropMethod("waitpid2")]
        public static object waitpid2(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mProcess, "waitpid2", args);
        }

    }
}

