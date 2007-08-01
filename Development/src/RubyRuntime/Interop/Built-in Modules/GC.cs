using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class GC
    {
         [InteropMethod("disable")]
        public static object disable()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mGC, "disable");
        }

         [InteropMethod("enable")]
        public static object enable()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mGC, "enable");
        }

         [InteropMethod("start")]
        public static object start()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mGC, "start");
        }

    }
}

