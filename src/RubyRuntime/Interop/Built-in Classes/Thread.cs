using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Thread
    {
         [InteropMethod("critical")]
        public static object critical()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_cThread, "critical");
        }

         [InteropMethod("critical=")]
        public static object set_critical(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cThread, "critical=", p1);
        }

         [InteropMethod("current")]
        public static object current()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_cThread, "current");
        }

         [InteropMethod("fork")]
        public static object fork(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cThread, "fork", args);
        }

         [InteropMethod("list")]
        public static object list()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_cThread, "list");
        }

         [InteropMethod("main")]
        public static object main()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_cThread, "main");
        }

         [InteropMethod("new")]
        public static object new_instance(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cThread, "new", args);
        }

         [InteropMethod("pass")]
        public static object pass()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_cThread, "pass");
        }

         [InteropMethod("start")]
        public static object start(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cThread, "start", args);
        }

         [InteropMethod("stop")]
        public static object stop()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_cThread, "stop");
        }

         [InteropMethod("[]")]
        public object indexer(object p1)
        {
            return Eval.Call1(this, "[]", p1);
        }

         [InteropMethod("[]=")]
        public object setitem(object p1, object p2)
        {
            return Eval.Call2(this, "[]=", p1, p2);
        }

         [InteropMethod("abort_on_exception")]
        public object abort_on_exception()
        {
            return Eval.Call0(this, "abort_on_exception");
        }

         [InteropMethod("abort_on_exception=")]
        public object set_abort_on_exception(object p1)
        {
            return Eval.Call1(this, "abort_on_exception=", p1);
        }

         [InteropMethod("alive?")]
        public object is_alive()
        {
            return Eval.Call0(this, "alive?");
        }

         [InteropMethod("exit")]
        public object exit()
        {
            return Eval.Call0(this, "exit");
        }

         [InteropMethod("group")]
        public object group()
        {
            return Eval.Call0(this, "group");
        }

         [InteropMethod("join")]
        public object join(params object[] args)
        {
            return Eval.Call(this, "join", args);
        }

         [InteropMethod("key?")]
        public object is_key(object p1)
        {
            return Eval.Call1(this, "key?", p1);
        }

         [InteropMethod("keys")]
        public object keys()
        {
            return Eval.Call0(this, "keys");
        }

         [InteropMethod("kill")]
        public object kill()
        {
            return Eval.Call0(this, "kill");
        }

         [InteropMethod("priority")]
        public object priority()
        {
            return Eval.Call0(this, "priority");
        }

         [InteropMethod("priority=")]
        public object set_priority(object p1)
        {
            return Eval.Call1(this, "priority=", p1);
        }

         [InteropMethod("raise")]
        public object raise(params object[] args)
        {
            return Eval.Call(this, "raise", args);
        }

         [InteropMethod("run")]
        public object run()
        {
            return Eval.Call0(this, "run");
        }

         [InteropMethod("safe_level")]
        public object safe_level()
        {
            return Eval.Call0(this, "safe_level");
        }

         [InteropMethod("status")]
        public object status()
        {
            return Eval.Call0(this, "status");
        }

         [InteropMethod("stop?")]
        public object is_stop()
        {
            return Eval.Call0(this, "stop?");
        }

         [InteropMethod("terminate")]
        public object terminate()
        {
            return Eval.Call0(this, "terminate");
        }

         [InteropMethod("value")]
        public object value()
        {
            return Eval.Call0(this, "value");
        }

         [InteropMethod("wakeup")]
        public object wakeup()
        {
            return Eval.Call0(this, "wakeup");
        }

    }
}

